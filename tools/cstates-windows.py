"""Native Windows run of the Redis-free core grid, with processor idle states enabled and disabled.

Idle states are toggled with powercfg IDLEDISABLE (all states off, the CPU polls). IDLESTATEMAX would
limit to C1, but it is ignored while Hyper-V is running (WSL/Docker), so it is not used. Modes are
interleaved per repeat; IDLEDISABLE is always restored to 0. No elevation needed.

Setup (PowerShell, from the repo root), then `python tools/cstates-windows.py [repeats]`:

    $w = "artifacts/cstates-windows"
    iwr https://dot.net/v1/dotnet-install.ps1 -OutFile $w/dotnet-install.ps1
    & $w/dotnet-install.ps1 -Runtime dotnet -Version 10.0.12 -InstallDir $w/rt/net10 -NoPath
    & $w/dotnet-install.ps1 -Runtime dotnet -Version 11.0.0-rc.1.26425.128 -InstallDir $w/rt/rc1 -NoPath
    & $w/dotnet-install.ps1 -Runtime dotnet -Version 11.0.0-rc.2.26504.105 -InstallDir $w/rt/rc2 -NoPath
    dotnet publish src/Repro -c Release -o $w/app/net10 -p:Tfm=net10.0
    dotnet publish src/Repro -c Release -o $w/app/net11 -p:Tfm=net11.0 -p:DisableRuntimeAsync=true

The CPU masks assume a 12-core/24-thread part (Windows numbers SMT siblings adjacently).
"""
import json, os, statistics, subprocess, sys, time, datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
S = ROOT / "artifacts" / "cstates-windows"
OUT = ROOT / "results" / ("cstates-windows-" + datetime.datetime.now().strftime("%Y%m%d-%H%M%S"))
OUT.mkdir(parents=True)
REPEATS, WARMUP, DURATION = int(sys.argv[1]) if len(sys.argv) > 1 else 3, 3, 10
SERVER_MASK, CLIENT_MASK = "FF", "FFF000"  # cores 0-3 / cores 6-11 (Windows numbers SMT siblings adjacently)
PORT = "7101"
ROWS = [(".NET 10", "net10", "net10", {}), ("RC1", "rc1", "net11", {}), ("RC2", "rc2", "net11", {}),
        ("RC2 threshold=0", "rc2", "net11", {"DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold": "0"})]
CELLS = [("1 caller", ["--callers", "1"]),
         ("64 callers", ["--callers", "64"]),
         ("64 callers, 5 us work", ["--callers", "64", "--work-us", "5"]),
         ("64 callers, 20 us work", ["--callers", "64", "--work-us", "20"]),
         ("64 callers, 78% local hits, 1 us work", ["--callers", "64", "--hit", "0.78", "--work-us", "1"])]
MODES = [("deep idle on (default)", "0"), ("idle disabled", "1")]


def dotnet(rt): return str(S / "rt" / rt / "dotnet.exe")
def app(tfm): return str(S / "app" / tfm / "Repro.dll")


def set_idle(v):
    subprocess.run(["powercfg", "/setacvalueindex", "scheme_current", "sub_processor", "IDLEDISABLE", v], check=True)
    subprocess.run(["powercfg", "/setactive", "scheme_current"], check=True)
    time.sleep(1)


def affinity_cmd(mask, args):
    # a string, so cmd sees start's empty title as "" (list2cmdline would escape it to \"\")
    return f'cmd /c start "" /b /wait /affinity {mask} ' + subprocess.list2cmdline(args)


def measure(rt, tfm, env, args):
    e = {k: v for k, v in os.environ.items() if not k.startswith("DOTNET_")} | env
    cmd = affinity_cmd(CLIENT_MASK, [dotnet(rt), app(tfm), "client", "--port", PORT,
                                     "--warmup", str(WARMUP), "--duration", str(DURATION), *args])
    out = subprocess.run(cmd, text=True, capture_output=True, env=e, timeout=WARMUP + DURATION + 120)
    lines = [l for l in out.stdout.splitlines() if l.startswith("{")]
    return json.loads(lines[-1]) if lines else {"error": (out.stderr or out.stdout)[-2000:]}


results = []
server = subprocess.Popen(affinity_cmd(SERVER_MASK, [dotnet("net10"), app("net10"), "serve", "--port", PORT]),
                          stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
time.sleep(2)
try:
    with open(OUT / "raw.jsonl", "w") as raw:
        for rep in range(REPEATS):
            for mode, v in (MODES if rep % 2 == 0 else MODES[::-1]):
                set_idle(v)
                for cell, args in CELLS:
                    for row, rt, tfm, env in ROWS:
                        r = measure(rt, tfm, env, args) | {"row": row, "cell": cell, "mode": mode, "repeat": rep}
                        results.append(r)
                        raw.write(json.dumps(r) + "\n"); raw.flush()
                        print(f"[{rep}] {mode:24} {cell:40} {row:16} "
                              + (f"{r['ops_per_sec']:>12,.0f} busy {r['cpu_cores_busy']:.2f}" if "ops_per_sec" in r else r["error"][:200]),
                              flush=True)
finally:
    set_idle("0")
    subprocess.run(["taskkill", "/F", "/T", "/PID", str(server.pid)], capture_output=True)

med = lambda xs: statistics.median(xs) if xs else float("nan")
lines = [f"# C-states, native Windows", "",
         f"{datetime.date.today()}, AMD Ryzen 9 7900X, Windows {sys.getwindowsversion().build}, server cores 0-3, client cores 6-11 "
         f"(12 CPUs), {REPEATS} repeats of {DURATION} s after {WARMUP} s warmup, interleaved. Medians.", ""]
for cell, _ in CELLS:
    lines += [f"## {cell}", "", "| row | idle | ops/s | vs .NET 10 (same idle) | busy | p50 us | p99 us | threads |",
              "|---|---|---:|---:|---:|---:|---:|---:|"]
    for row, *_ in ROWS:
        for mode, _ in MODES:
            rs = [r for r in results if r["cell"] == cell and r["row"] == row and r["mode"] == mode and "ops_per_sec" in r]
            base = med([r["ops_per_sec"] for r in results if r["cell"] == cell and r["row"] == ".NET 10" and r["mode"] == mode and "ops_per_sec" in r])
            ops = med([r["ops_per_sec"] for r in rs])
            vs = "" if row == ".NET 10" else f"{(ops / base - 1) * 100:+.1f}%"
            lines.append(f"| {row} | {mode} | {ops:,.0f} | {vs} | {med([r['cpu_cores_busy'] for r in rs]):.2f} | "
                         f"{med([r['op_latency_us']['p50'] for r in rs]):.0f} | {med([r['op_latency_us']['p99'] for r in rs]):.0f} | "
                         f"{med([r['threadpool_threads'] for r in rs]):.0f} |")
    lines.append("")
(OUT / "summary.md").write_text("\n".join(lines))
print(OUT / "summary.md")
