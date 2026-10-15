#!/usr/bin/env python3
"""Run the grid: build one image per row, then measure every (cell, row) pair, interleaved.

    ./run.py core                    # the Redis-free repro: runtimes x runtime async x thread pool knob
    ./run.py callers                 # the Redis-free repro: 1..256 callers
    ./run.py cpus                    # the Redis-free repro: 4, 8, 12 client CPUs
    ./run.py redis                   # StackExchange.Redis v3 and v4 against Redis
    ./run.py all                     # all of the above
    ./run.py core --quick            # 1 repeat of 3 s, to check the setup

Results land in results/<experiment>-<timestamp>/: raw.jsonl (one line per measurement, everything
the client reported plus the row and cell), and summary.md (medians, and each row against .NET 10).

Python 3.10+ and Docker only. The server and the client run in separate containers on disjoint CPU
sets (--server-cpus/--client-cpus), so the server never competes with the thread pool being measured.
"""

from __future__ import annotations

import argparse
import datetime
import itertools
import json
import os
import statistics
import subprocess
import sys
import time
from dataclasses import dataclass, field
from pathlib import Path

ROOT = Path(__file__).resolve().parent

# The runtimes, as SDK images pinned by digest. The SDK image carries the matching runtime.
RUNTIMES = {
    "net10": ("mcr.microsoft.com/dotnet/sdk:10.0@sha256:2fa828c68761b1b8c23d7662dc134421b9d3b59fe1425fdbc80804e390cdb24d", "net10.0"),
    "rc1": ("mcr.microsoft.com/dotnet/sdk:11.0.100-rc.1@sha256:d0a39ca4acefe992f26030f8fb8c60e1b14ac088822cb3fdfbe536ecf843a675", "net11.0"),
    "rc2": ("mcr.microsoft.com/dotnet/sdk:11.0.100-rc.2@sha256:bfabf51e74c5a7cf2fc4eac852145db86f4867d475257d809518d6a2b0fe71e1", "net11.0"),
}
REDIS_IMAGE = "redis@sha256:c94085d298b738be22c9ccdc0ac3761fa6649df7dd82ad1d42367f3cb9714935"  # redis:8 (8.10.2)
SER_V3 = "refs/tags/3.4.0"
SER_V4 = "31c80eef443fcb0a03ea981e04ce15e5476a3fa1"  # the v4 branch, 2026-10-12
THRESHOLD = "DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold"
ECHO_PORT, REDIS_PORT = 7101, 6390


@dataclass(frozen=True)
class Row:
    """One build of the client, plus the environment it runs under."""
    name: str
    runtime: str
    app: str = "Repro"
    runtime_async: str = "off"
    ser: str = ""
    env: tuple[tuple[str, str], ...] = ()

    @property
    def image(self) -> str:
        parts = [self.app.lower(), self.runtime, f"ra-{self.runtime_async}"]
        if self.ser:
            parts.append("ser" + ("3" if self.ser == SER_V3 else "4"))
        return "tp-repro:" + "-".join(parts)


@dataclass(frozen=True)
class Cell:
    name: str
    args: tuple[str, ...]
    client_cpus: str | None = None  # None: the --client-cpus default
    only: tuple[str, ...] = ()      # if set, only these rows


def raw_rows(runtimes=("rc1", "rc2"), knobs=True) -> list[Row]:
    rows = [Row(".NET 10", "net10")]
    for rt in runtimes:
        for ra in ("off", "on"):
            rows.append(Row(f"{rt.upper()} ra-{ra}", rt, runtime_async=ra))
    if knobs:
        for value in ("0", "4", "32767"):
            rows.append(Row(f"RC2 ra-off threshold={value}", "rc2", env=((THRESHOLD, value),)))
    return rows


def experiments(client_cpus: str) -> dict[str, tuple[str, list[Row], list[Cell]]]:
    cpu_list = expand(client_cpus)
    half_cores = [c for c in cpu_list[: len(cpu_list) // 2]]
    sweep = {}
    for n in (4, 8):
        # take whole cores (both SMT siblings) where the set allows, so 4 CPUs is 2 full cores
        pairs = list(zip(cpu_list[: len(cpu_list) // 2], cpu_list[len(cpu_list) // 2:]))
        chosen = sorted(itertools.chain.from_iterable(pairs[: n // 2])) if len(pairs) >= n // 2 else half_cores[:n]
        sweep[n] = ",".join(map(str, chosen))
    return {
        "core": ("echo", raw_rows(), [
            Cell("1 caller", ("--callers", "1")),
            Cell("64 callers", ("--callers", "64")),
            Cell("64 callers, 5 us work", ("--callers", "64", "--work-us", "5")),
            Cell("64 callers, 20 us work", ("--callers", "64", "--work-us", "20")),
            Cell("64 callers, 78% local hits, 1 us work", ("--callers", "64", "--hit", "0.78", "--work-us", "1")),
        ]),
        "callers": ("echo", [Row(".NET 10", "net10"), Row("RC1 ra-off", "rc1"), Row("RC2 ra-off", "rc2")], [
            Cell(f"{n} callers, 5 us work", ("--callers", str(n), "--work-us", "5")) for n in (1, 4, 16, 64, 256)
        ]),
        "cpus": ("echo", [Row(".NET 10", "net10"), Row("RC1 ra-off", "rc1"), Row("RC2 ra-off", "rc2")], [
            Cell(f"{n} client CPUs, 64 callers, 5 us work", ("--callers", "64", "--work-us", "5"), client_cpus=cpus)
            for n, cpus in [*sweep.items(), (len(cpu_list), client_cpus)]
        ]),
        "redis": ("redis", redis_rows(), [
            Cell("GET, 1 caller", ("--callers", "1", "--keyspace", "1000")),
            Cell("GET, 64 callers", ("--callers", "64", "--keyspace", "1000")),
            Cell("GET, 64 callers, 20 us work", ("--callers", "64", "--keyspace", "1000", "--work-us", "20")),
            Cell("GET, 64 callers, v4 client cache, Zipf", ("--callers", "64", "--cache", "--keyspace", "200000", "--zipf", "0.99"),
                 only=tuple(r.name for r in redis_rows() if r.ser == SER_V4)),
        ]),
    }


def redis_rows() -> list[Row]:
    rows = []
    for label, ser in (("v3", SER_V3), ("v4", SER_V4)):
        rows.append(Row(f"{label} .NET 10", "net10", "RedisRepro", ser=ser))
        for rt in ("rc1", "rc2"):
            for ra in ("off", "on"):
                rows.append(Row(f"{label} {rt.upper()} ra-{ra}", rt, "RedisRepro", ra, ser))
        for value in ("0", "32767"):
            rows.append(Row(f"{label} RC2 ra-off threshold={value}", "rc2", "RedisRepro", "off", ser, ((THRESHOLD, value),)))
    return rows


def expand(cpus: str) -> list[int]:
    out = []
    for part in cpus.split(","):
        a, _, b = part.partition("-")
        out.extend(range(int(a), int(b or a) + 1))
    return out


def sh(*cmd: str, check=True, capture=True) -> str:
    result = subprocess.run(cmd, check=False, text=True, capture_output=capture)
    if check and result.returncode != 0:
        raise SystemExit(f"failed: {' '.join(cmd)}\n{result.stdout}\n{result.stderr}")
    return result.stdout if capture else ""


def build(row: Row) -> None:
    sdk, tfm = RUNTIMES[row.runtime]
    args = ["docker", "build", "-q", "-t", row.image,
            "--build-arg", f"SDK_IMAGE={sdk}", "--build-arg", f"TFM={tfm}",
            "--build-arg", f"APP={row.app}", "--build-arg", f"RUNTIME_ASYNC={row.runtime_async}"]
    if row.ser:
        args += ["--build-arg", f"SER_REF={row.ser}", "--build-arg", f"SER_MAJOR={'3' if row.ser == SER_V3 else '4'}"]
    print(f"  building {row.image}", flush=True)
    sh(*args, str(ROOT))


def start_server(kind: str, server_cpus: str) -> str:
    sh("docker", "rm", "-f", "tp-server", check=False)
    if kind == "echo":
        # the echo server is always the .NET 10 build, so it is the same for every row
        image = Row(".NET 10", "net10").image
        sh("docker", "run", "-d", "--rm", "--name", "tp-server", "--network", "host", "--cpuset-cpus", server_cpus,
           image, "serve", "--port", str(ECHO_PORT))
        port = ECHO_PORT
    else:
        sh("docker", "run", "-d", "--rm", "--name", "tp-server", "--network", "host", "--cpuset-cpus", server_cpus,
           REDIS_IMAGE, "redis-server", "--port", str(REDIS_PORT), "--save", "", "--appendonly", "no")
        port = REDIS_PORT
    time.sleep(2)
    return str(port)


def measure(row: Row, cell: Cell, port: str, client_cpus: str, warmup: float, duration: float) -> dict:
    cmd = ["docker", "run", "--rm", "--network", "host", "--cpuset-cpus", cell.client_cpus or client_cpus]
    for k, v in row.env:
        cmd += ["-e", f"{k}={v}"]
    cmd += [row.image, "client", "--port", port, "--warmup", str(warmup), "--duration", str(duration), *cell.args]
    out = subprocess.run(cmd, text=True, capture_output=True, timeout=warmup + duration + 300)
    lines = [l for l in out.stdout.splitlines() if l.startswith("{")]
    if out.returncode != 0 or not lines:
        return {"error": (out.stderr or out.stdout)[-2000:]}
    return json.loads(lines[-1])


def summarise(path: Path, rows: list[Row], cells: list[Cell], results: list[dict], meta: dict) -> str:
    def med(xs):
        return statistics.median(xs) if xs else float("nan")

    out = [f"# {meta['experiment']}", "",
           f"{meta['started']}, {meta['host']}, server CPUs {meta['server_cpus']}, client CPUs {meta['client_cpus']}, "
           f"{meta['repeats']} repeats of {meta['duration']} s after {meta['warmup']} s warmup. Medians; "
           "the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. "
           "**busy** is CPU cores the client kept busy (process CPU time / wall time).", ""]
    for cell in cells:
        out += [f"## {cell.name}", ""]
        if cell.client_cpus:
            counts = sorted({r["processor_count"] for r in results if r["cell"] == cell.name and "processor_count" in r})
            out += [f"Client CPUs {cell.client_cpus} (processor count {', '.join(map(str, counts))}).", ""]
        out += ["| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |",
                "|---|---:|---:|---:|---:|---:|---:|---:|---:|"]
        base = {}  # per library: v3 rows against v3 on .NET 10, v4 against v4
        for row in rows:
            if cell.only and row.name not in cell.only:
                continue
            rs = [r for r in results if r["row"] == row.name and r["cell"] == cell.name and "ops_per_sec" in r]
            errors = [r for r in results if r["row"] == row.name and r["cell"] == cell.name and "error" in r]
            if not rs:
                out.append(f"| {row.name} | failed ({len(errors)}) | | | | | | | |")
                continue
            ops = [r["ops_per_sec"] for r in rs]
            m = med(ops)
            b = base.setdefault(row.ser, m)
            spread = (max(ops) - min(ops)) / m if m else 0
            out.append(
                f"| {row.name} | {m:,.0f} | {m / b - 1:+.1%} | {spread:.1%} | "
                f"{med([r['cpu_cores_busy'] for r in rs]):.2f} | {med([r['cpu_us_per_op'] for r in rs]):.2f} | "
                f"{med([r['op_latency_us']['p50'] for r in rs]):.0f} | {med([r['op_latency_us']['p99'] for r in rs]):.0f} | "
                f"{med([r['threadpool_threads'] for r in rs]):.0f} |")
        out.append("")
    out += ["## What ran", ""]
    for row in rows:
        r = next((x for x in results if x["row"] == row.name and "runtime_build" in x), None)
        if r:
            out.append(f"- **{row.name}**: {r['runtime_build']} on {r['os']}; env {r['env'] or '{}'}; "
                       + "; ".join(f"{k} {v}" for k, v in r["async_compilation"].items())
                       + (f"; {r['library']} @ {r.get('library_ref')}" if r.get("library") else ""))
    out.append("")
    counts = sorted({r["processor_count"] for r in results if "processor_count" in r})
    if counts:
        out.append(f"Processor count seen by the client: {', '.join(map(str, counts))}.")
    text = "\n".join(out) + "\n"
    path.write_text(text)
    return text


def run_experiment(name: str, a: argparse.Namespace) -> None:
    kind, rows, cells = experiments(a.client_cpus)[name]
    images = {r.image: r for r in rows}
    if kind == "echo":
        images.setdefault(Row(".NET 10", "net10").image, Row(".NET 10", "net10"))
    print(f"[{name}] building {len(images)} images", flush=True)
    for row in images.values():
        build(row)

    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    out_dir = ROOT / "results" / f"{name}-{stamp}"
    out_dir.mkdir(parents=True)
    meta = {"experiment": name, "started": datetime.datetime.now().isoformat(timespec="seconds"),
            "host": sh("sh", "-c", "lscpu | sed -n 's/^Model name: *//p'").strip() + f", kernel {os.uname().release}",
            "server_cpus": a.server_cpus, "client_cpus": a.client_cpus,
            "repeats": a.repeats, "warmup": a.warmup, "duration": a.duration}
    (out_dir / "meta.json").write_text(json.dumps(meta, indent=2))

    port = start_server(kind, a.server_cpus)
    results = []
    pairs = [(c, r) for c in cells for r in rows if not c.only or r.name in c.only]
    total = len(pairs) * a.repeats
    done = 0
    try:
        with (out_dir / "raw.jsonl").open("w") as raw:
            for repeat in range(a.repeats):
                # interleaved: every row of a cell back to back, rotating which row goes first
                for cell in cells:
                    cell_rows = [r for r in rows if not cell.only or r.name in cell.only]
                    shift = repeat % len(cell_rows)
                    for row in cell_rows[shift:] + cell_rows[:shift]:
                        result = measure(row, cell, port, a.client_cpus, a.warmup, a.duration)
                        result.update(row=row.name, cell=cell.name, repeat=repeat)
                        raw.write(json.dumps(result) + "\n")
                        raw.flush()
                        results.append(result)
                        done += 1
                        shown = f"{result['ops_per_sec']:,.0f} ops/s, busy {result['cpu_cores_busy']:.2f}" if "ops_per_sec" in result else "FAILED"
                        print(f"  [{done}/{total}] {cell.name} | {row.name}: {shown}", flush=True)
    finally:
        sh("docker", "rm", "-f", "tp-server", check=False)
    print(summarise(out_dir / "summary.md", rows, cells, results, meta))
    print(f"-> {out_dir}")


def main() -> int:
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("experiment", choices=["core", "callers", "cpus", "redis", "all"])
    p.add_argument("--server-cpus", default="0-3,12-15", help="cpuset for the server (default suits a 12-core/24-thread host)")
    p.add_argument("--client-cpus", default="6-11,18-23", help="cpuset for the measured client")
    p.add_argument("--repeats", type=int, default=3)
    p.add_argument("--warmup", type=float, default=3)
    p.add_argument("--duration", type=float, default=10)
    p.add_argument("--quick", action="store_true", help="1 repeat, 1 s warmup, 3 s measured")
    a = p.parse_args()
    if a.quick:
        a.repeats, a.warmup, a.duration = 1, 1, 3
    for name in (["core", "callers", "cpus", "redis"] if a.experiment == "all" else [a.experiment]):
        run_experiment(name, a)
    return 0


if __name__ == "__main__":
    sys.exit(main())
