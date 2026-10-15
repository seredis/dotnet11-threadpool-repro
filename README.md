# .NET 11 thread pool: idle cores and lost throughput for multiplexed clients

A self-contained reproduction of a throughput regression from .NET 10 to .NET 11 (RC1 and RC2), first
seen with [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) and reproduced
here **without Redis**. It is a companion to
[dotnet/runtime#135738](https://github.com/dotnet/runtime/issues/135738), which covers the same
thread pool change from the other side.

**The shape:** many callers share one socket. Each caller sends a request and awaits a
`TaskCompletionSource`, and a single read loop completes callers as replies arrive, so every reply is
handed off through the thread pool. Each caller then does a few microseconds of its own CPU work.
On .NET 10 the client keeps 11.5 of its 12 CPUs busy. On .NET 11, with default settings, it keeps
about 6 (RC1) or about 8 (RC2) busy. Throughput drops by 30–45% and p99 latency rises 4–8×.

- **Runtime async plays no part.** Building the app and the library with `runtime-async=on` or off
  changes throughput by about 1%. CoreLib and System.Net.Sockets are runtime-async on .NET 11 either way.
- **`DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold` doesn't fix it.** This is the knob added by
  #132765 and #132908. At 0 it behaves close to RC1, and at 32767 it never parks early. No value
  brings these cells back to .NET 10; the best is still 30% short.
- **The knob favours different workloads at each end.** With StackExchange.Redis v4 and pure round
  trips (64 callers, no work), the RC2 default is 14% slower than .NET 10. Threshold 0 fixes that
  cell, but it costs 7 points on the client-cache cell.
- **Raising min threads doesn't help.** Threads are not in short supply: the pool already has 13–16
  workers for 12 CPUs, and the workers are parked while work is waiting. In the 5 µs cell, setting
  `--min-threads` to 32 or 128 cost RC2 8% (about 613k ops/s against 669k) and .NET 10 6%. Those
  figures come from two runs each, measured by hand.

## Results

Ryzen 9 7900X (12 cores, 24 threads), Linux 7.1. The server and the client run in separate
containers on disjoint CPU sets: the server on `0-3,12-15`, the client on `6-11,18-23` (12 CPUs).
Each figure is the median of 3 interleaved runs of 10 s, measured after a 3 s warmup. **busy** is the
number of CPU cores the client kept busy (process CPU time ÷ wall time).

Runtimes are pinned by image digest:

| row | runtime |
|---|---|
| .NET 10 | `10.0.12` |
| RC1 | `11.0.0-rc.1.26425.128` |
| RC2 | `11.0.0-rc.2.26504.105` |

`ra-on`/`ra-off` says whether the app was compiled with runtime async.

### Redis-free, 64 callers, 5 µs of work per operation ([full results](results/core-20261015-091301/summary.md))

| row | ops/s | vs .NET 10 | busy (of 12) | p99 µs |
|---|---:|---:|---:|---:|
| .NET 10 | 1,029,128 | | 11.56 | 86 |
| RC1 ra-off | 573,273 | −44.3% | 5.69 | 688 |
| RC1 ra-on | 570,609 | −44.6% | 5.69 | 721 |
| RC2 ra-off | 667,782 | −35.1% | 7.76 | 360 |
| RC2 ra-on | 668,228 | −35.1% | 7.77 | 360 |
| RC2, threshold 0 | 625,107 | −39.3% | 7.21 | 475 |
| RC2, threshold 4 | 621,719 | −39.6% | 7.26 | 393 |
| RC2, threshold 32767 | 695,212 | −32.4% | 8.55 | 328 |

### Redis-free, every cell, vs .NET 10 ([full results](results/core-20261015-091301/summary.md))

| cell | RC1 | RC2 | RC2, best threshold | busy: .NET 10 / RC1 / RC2 |
|---|---:|---:|---:|---|
| 1 caller | −16.5% | −4.1% | −4.2% (32767) | 3.06 / 1.46 / 3.60 |
| 64 callers, no work | +8.4% | +3.8% | +12.0% (4) | 11.30 / 3.01 / 8.04 |
| 64 callers, 5 µs work | −44.3% | −35.1% | −32.4% (32767) | 11.56 / 5.69 / 7.76 |
| 64 callers, 20 µs work | −16.8% | −19.4% | −17.9% (0) | 11.56 / 9.09 / 9.57 |
| 64 callers, 78% hits completed in place, 1 µs work | −41.7% | −30.4% | −30.1% (32767) | 11.37 / 5.40 / 7.73 |

With no work between operations, .NET 11 matches .NET 10 throughput on far less CPU, because the
CPU it saves was spinning. Once callers do work of their own, the parked workers are what the work
waits on.

### More callers, fewer CPUs ([callers](results/callers-20261015-093951/summary.md), [cpus](results/cpus-20261015-094957/summary.md))

These cells use 5 µs of work per operation, with runtime async off.

| cell | RC1 | RC2 |
|---|---:|---:|
| 1 caller | −13.8% | −4.8% |
| 4 callers | −5.5% | −3.4% |
| 16 callers | −12.7% | −15.7% |
| 64 callers | −44.1% | −35.9% |
| 256 callers | −27.3% | −26.3% |
| 64 callers, 4 client CPUs | −18.0% | −28.6% |
| 64 callers, 8 client CPUs | −24.8% | −32.0% |
| 64 callers, 12 client CPUs | −44.9% | −36.2% |

On 4 and 8 CPUs, RC2 is worse than RC1.

### StackExchange.Redis v3 and v4 against Redis ([full results](results/redis-20261015-100200/summary.md))

Every operation is a GET of a 1 KiB value through one `ConnectionMultiplexer`, against Redis 8.10.2.
Each row is compared with the same library on .NET 10. The libraries are v3 at
[3.4.0](https://github.com/StackExchange/StackExchange.Redis/releases/tag/3.4.0) and v4 at
[`31c80ee`](https://github.com/StackExchange/StackExchange.Redis/commit/31c80eef443fcb0a03ea981e04ce15e5476a3fa1).
Both are built from source, with runtime async on or off for the library as well as the app.

| cell | lib | RC1 off | RC1 on | RC2 off | RC2 on | RC2 thr 0 | RC2 thr 32767 |
|---|---|---:|---:|---:|---:|---:|---:|
| 1 caller | v3 | −16.3% | −15.2% | −7.6% | −7.9% | −12.0% | −6.2% |
| | v4 | −8.4% | −14.8% | −4.4% | −6.4% | −7.3% | −5.6% |
| 64 callers | v3 | +0.7% | −1.2% | −9.1% | −6.2% | −1.3% | −9.9% |
| | v4 | −0.5% | −2.8% | −13.7% | −15.2% | **+4.9%** | −22.6% |
| 64 callers, 20 µs work | v3 | −10.1% | −10.8% | −18.9% | −18.6% | −17.5% | −18.3% |
| | v4 | −16.0% | −17.5% | −17.5% | −18.5% | −17.5% | −21.7% |
| 64 callers, v4 client-side cache, Zipf keys | v4 | −35.1% | −35.8% | −24.3% | −24.8% | −31.8% | −22.5% |

The client-side cache cell answers most GETs from memory, with no I/O. The rest are round trips whose
completions go through the thread pool. It is the Redis version of the Redis-free "78% hits" cell, and
it regresses the same way.

## What changed in .NET 11

This history is from reading dotnet/runtime; it has not been bisected here.

- [#122726](https://github.com/dotnet/runtime/pull/122726) (January 2026) shortened spinning in the
  thread pool's LIFO semaphore. It is in RC1 and RC2.
- [#123921](https://github.com/dotnet/runtime/pull/123921) changed the semaphore. It was reverted in
  [#125193](https://github.com/dotnet/runtime/pull/125193), then reapplied in
  [#128606](https://github.com/dotnet/runtime/pull/128606) (June 2026). Among other changes, #128606
  parks a worker unconditionally when it wakes and finds no work (a "spurious dispatch"). RC1 has
  this.
- [#132765](https://github.com/dotnet/runtime/pull/132765), backported in
  [#132908](https://github.com/dotnet/runtime/pull/132908), is in RC2. It parks only once more than
  2/3 of the processors have busy workers, and adds `DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold`.
  The default is ProcessorCount × 2/3, which is 8 here.
- [#135738](https://github.com/dotnet/runtime/issues/135738) reports that #132765 lost runtime-async
  microbenchmark gains that RC1 had, and that threshold 0 restores them.

RC2 recovers part of what RC1 lost. The remaining 25–35% doesn't respond to the threshold at any
value, so it is not only the parking decision that #132765 made tunable. The older semaphore and
spinning changes are the next candidates.

## Running it

You need Docker and Python 3.10+. Nothing else is installed on the host.

```bash
./run.py core --quick    # a few minutes: checks the setup
./run.py core            # the Redis-free grid above, about 30 minutes
./run.py callers         # 1..256 callers
./run.py cpus            # 4, 8, 12 client CPUs
./run.py redis           # StackExchange.Redis v3 and v4 against Redis (clones and builds SE.Redis)
./run.py all
```

The default CPU sets suit a 12-core/24-thread machine. Elsewhere, pass `--server-cpus` and
`--client-cpus`, keeping the two sets disjoint and the client on whole cores. Each run writes
`results/<experiment>-<timestamp>/` containing:

- `raw.jsonl`: everything each measurement reported, including the exact runtime build, OS, the
  `DOTNET_*` environment, and how each assembly's async methods were compiled (read back from
  metadata).
- `summary.md`: the tables.

To run the Redis-free repro by hand on any machine with a .NET SDK:

```bash
dotnet run -c Release --project src/Repro -p:Tfm=net10.0 -- serve --port 7000 &
dotnet run -c Release --project src/Repro -p:Tfm=net11.0 -- client --port 7000 --callers 64 --work-us 5
#   -p:RuntimeAsync=on      compile with runtime async (net11.0)
#   --hit 0.78              complete 78% of operations in place, without I/O
#   --min-threads N         ThreadPool.SetMinThreads before starting
```

Each client run prints one JSON line. Pin the server and the client to different cores (with
`taskset`, for example), or the server competes with the pool being measured.

## Layout

| path | what |
|---|---|
| `src/Repro/Program.cs` | The Redis-free repro: a blocking echo server, and a multiplexed client (one socket; callers await a `TaskCompletionSource` completed by one read loop). |
| `src/RedisRepro/Program.cs` | The same measured loop with StackExchange.Redis v3 or v4. |
| `src/Shared/Driver.cs` | The callers, the measurement window, the latency histogram, and the JSON result. |
| `build/inject.targets` | Builds StackExchange.Redis from source for net11.0, with runtime async on or off. |
| `Dockerfile`, `run.py` | One image per row, built on the pinned SDK image of the runtime under test; the grid runner. |
| `results/` | The runs quoted above. |
