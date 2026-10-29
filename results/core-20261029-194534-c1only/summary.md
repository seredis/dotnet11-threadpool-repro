# core

2026-10-29T19:45:34, AMD Ryzen 9 7900X 12-Core Processor, kernel 7.2.6-76070206-generic, server CPUs 0-3,12-15, client CPUs 6-11,18-23, 3 repeats of 10 s after 3 s warmup. Medians; the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. **busy** is CPU cores the client kept busy (process CPU time / wall time).

## 1 caller

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 55,731 | +0.0% | 1.1% | 3.13 | 56.02 | 17 | 20 | 4 |
| RC1 ra-off | 48,901 | -12.3% | 7.6% | 1.41 | 28.92 | 18 | 26 | 4 |
| RC1 ra-on | 44,865 | -19.5% | 4.4% | 1.54 | 34.24 | 22 | 26 | 4 |
| RC2 ra-off | 52,876 | -5.1% | 1.0% | 3.71 | 70.13 | 18 | 22 | 4 |
| RC2 ra-on | 52,876 | -5.1% | 1.6% | 3.57 | 67.53 | 18 | 23 | 4 |
| RC2 ra-off threshold=0 | 48,419 | -13.1% | 4.0% | 1.42 | 29.30 | 20 | 26 | 4 |
| RC2 ra-off threshold=4 | 53,021 | -4.9% | 1.2% | 3.52 | 66.42 | 18 | 23 | 4 |
| RC2 ra-off threshold=32767 | 52,514 | -5.8% | 1.1% | 3.64 | 68.92 | 18 | 23 | 4 |

## 64 callers

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,306,241 | +0.0% | 1.0% | 11.32 | 8.66 | 49 | 66 | 13 |
| RC1 ra-off | 1,427,558 | +9.3% | 0.4% | 3.03 | 2.13 | 45 | 59 | 13 |
| RC1 ra-on | 1,404,707 | +7.5% | 0.7% | 3.18 | 2.25 | 47 | 61 | 13 |
| RC2 ra-off | 1,378,873 | +5.6% | 3.4% | 7.94 | 5.76 | 43 | 94 | 13 |
| RC2 ra-on | 1,324,036 | +1.4% | 0.7% | 7.97 | 6.01 | 43 | 123 | 14 |
| RC2 ra-off threshold=0 | 1,432,690 | +9.7% | 0.2% | 3.29 | 2.30 | 45 | 59 | 13 |
| RC2 ra-off threshold=4 | 1,472,274 | +12.7% | 0.8% | 5.87 | 4.00 | 43 | 57 | 13 |
| RC2 ra-off threshold=32767 | 1,335,011 | +2.2% | 4.3% | 7.91 | 5.92 | 43 | 111 | 14 |

## 64 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,026,742 | +0.0% | 0.5% | 11.57 | 11.27 | 55 | 82 | 14 |
| RC1 ra-off | 614,323 | -40.2% | 7.3% | 6.17 | 10.01 | 74 | 590 | 14 |
| RC1 ra-on | 603,785 | -41.2% | 8.4% | 6.05 | 9.93 | 74 | 623 | 15 |
| RC2 ra-off | 677,539 | -34.0% | 0.6% | 7.81 | 11.49 | 70 | 344 | 13 |
| RC2 ra-on | 679,302 | -33.8% | 1.5% | 7.84 | 11.55 | 70 | 344 | 13 |
| RC2 ra-off threshold=0 | 642,387 | -37.4% | 3.9% | 7.31 | 11.45 | 74 | 426 | 14 |
| RC2 ra-off threshold=4 | 639,904 | -37.7% | 4.5% | 7.30 | 11.55 | 74 | 360 | 14 |
| RC2 ra-off threshold=32767 | 690,645 | -32.7% | 2.6% | 8.46 | 12.17 | 66 | 344 | 14 |

## 64 callers, 20 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 425,829 | +0.0% | 0.8% | 11.58 | 27.19 | 131 | 213 | 13 |
| RC1 ra-off | 351,097 | -17.5% | 0.9% | 9.01 | 25.68 | 156 | 508 | 13 |
| RC1 ra-on | 348,594 | -18.1% | 1.1% | 9.01 | 25.79 | 156 | 508 | 14 |
| RC2 ra-off | 341,497 | -19.8% | 2.1% | 9.57 | 28.16 | 156 | 475 | 14 |
| RC2 ra-on | 337,790 | -20.7% | 2.1% | 9.57 | 28.32 | 156 | 492 | 13 |
| RC2 ra-off threshold=0 | 340,917 | -19.9% | 0.6% | 9.55 | 28.01 | 164 | 410 | 13 |
| RC2 ra-off threshold=4 | 343,514 | -19.3% | 0.5% | 9.60 | 27.95 | 156 | 459 | 13 |
| RC2 ra-off threshold=32767 | 325,339 | -23.6% | 0.8% | 9.46 | 29.02 | 172 | 557 | 13 |

## 64 callers, 78% local hits, 1 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 4,198,794 | +0.0% | 0.7% | 11.36 | 2.71 | 0 | 82 | 14 |
| RC1 ra-off | 2,573,253 | -38.7% | 2.4% | 5.63 | 2.19 | 0 | 262 | 16 |
| RC1 ra-on | 2,538,838 | -39.5% | 2.0% | 5.62 | 2.20 | 0 | 279 | 15 |
| RC2 ra-off | 2,979,361 | -29.0% | 1.4% | 7.73 | 2.59 | 0 | 164 | 14 |
| RC2 ra-on | 3,007,016 | -28.4% | 2.0% | 7.80 | 2.59 | 0 | 164 | 13 |
| RC2 ra-off threshold=0 | 2,796,504 | -33.4% | 2.1% | 7.21 | 2.57 | 0 | 188 | 14 |
| RC2 ra-off threshold=4 | 2,848,390 | -32.2% | 0.6% | 7.39 | 2.59 | 0 | 180 | 14 |
| RC2 ra-off threshold=32767 | 2,960,692 | -29.5% | 1.8% | 8.27 | 2.75 | 0 | 164 | 14 |

## What ran

- **.NET 10**: 10.0.12-servicing.26422.108+95017c711e6afc1085133d440e42b4bd78155701 on Ubuntu 24.04.5 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 0 runtime-async / 9 state-machine; System.Private.CoreLib 10 runtime-async / 0 state-machine
- **RC1 ra-off**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC1 ra-on**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; Repro 5 runtime-async / 0 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC2 ra-off**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC2 ra-on**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; Repro 5 runtime-async / 0 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC2 ra-off threshold=0**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {'DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold': '0'}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC2 ra-off threshold=4**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {'DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold': '4'}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC2 ra-off threshold=32767**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {'DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold': '32767'}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine

Processor count seen by the client: 12.
