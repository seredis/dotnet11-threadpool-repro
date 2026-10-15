# cpus

2026-10-15T09:49:57, AMD Ryzen 9 7900X 12-Core Processor, kernel 7.1.5-76070105-generic, server CPUs 0-3,12-15, client CPUs 6-11,18-23, 3 repeats of 10 s after 3 s warmup. Medians; the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. **busy** is CPU cores the client kept busy (process CPU time / wall time).

## 4 client CPUs, 64 callers, 5 us work

Client CPUs 6,7,18,19 (processor count 4).

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 416,107 | +0.0% | 1.4% | 3.82 | 9.18 | 147 | 254 | 5 |
| RC1 ra-off | 341,176 | -18.0% | 1.5% | 3.11 | 9.11 | 172 | 688 | 5 |
| RC2 ra-off | 297,146 | -28.6% | 1.0% | 3.02 | 10.16 | 188 | 786 | 5 |

## 8 client CPUs, 64 callers, 5 us work

Client CPUs 6,7,8,9,18,19,20,21 (processor count 8).

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 788,466 | +0.0% | 0.6% | 7.77 | 9.85 | 78 | 102 | 10 |
| RC1 ra-off | 593,268 | -24.8% | 1.8% | 5.27 | 8.90 | 94 | 475 | 9 |
| RC2 ra-off | 535,963 | -32.0% | 2.5% | 5.60 | 10.45 | 98 | 459 | 9 |

## 12 client CPUs, 64 callers, 5 us work

Client CPUs 6-11,18-23 (processor count 12).

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,032,084 | +0.0% | 0.5% | 11.54 | 11.20 | 55 | 82 | 13 |
| RC1 ra-off | 569,086 | -44.9% | 1.9% | 5.63 | 9.89 | 78 | 688 | 16 |
| RC2 ra-off | 658,784 | -36.2% | 1.8% | 7.65 | 11.63 | 70 | 377 | 14 |

## What ran

- **.NET 10**: 10.0.12-servicing.26422.108+95017c711e6afc1085133d440e42b4bd78155701 on Ubuntu 24.04.5 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 0 runtime-async / 9 state-machine; System.Private.CoreLib 10 runtime-async / 0 state-machine
- **RC1 ra-off**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC2 ra-off**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine

Processor count seen by the client: 4, 8, 12.
