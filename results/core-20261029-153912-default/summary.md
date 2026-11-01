# core

2026-10-29T15:39:12, AMD Ryzen 9 7900X 12-Core Processor, kernel 7.2.6-76070206-generic, server CPUs 0-3,12-15, client CPUs 6-11,18-23, 3 repeats of 10 s after 3 s warmup. Medians; the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. **busy** is CPU cores the client kept busy (process CPU time / wall time).

## 1 caller

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 55,776 | +0.0% | 1.1% | 3.12 | 55.97 | 17 | 20 | 4 |
| RC1 ra-off | 45,852 | -17.8% | 3.7% | 1.50 | 32.70 | 22 | 27 | 4 |
| RC1 ra-on | 46,003 | -17.5% | 6.2% | 1.51 | 32.74 | 22 | 27 | 4 |
| RC2 ra-off | 52,804 | -5.3% | 1.9% | 3.51 | 66.38 | 18 | 23 | 4 |
| RC2 ra-on | 52,542 | -5.8% | 1.2% | 3.68 | 70.02 | 18 | 23 | 4 |
| RC2 ra-off threshold=0 | 46,965 | -15.8% | 2.6% | 1.42 | 30.14 | 20 | 26 | 4 |
| RC2 ra-off threshold=4 | 52,848 | -5.3% | 2.1% | 3.60 | 68.71 | 18 | 23 | 4 |
| RC2 ra-off threshold=32767 | 53,564 | -4.0% | 2.2% | 3.48 | 64.60 | 17 | 22 | 4 |

## 64 callers

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,306,060 | +0.0% | 1.0% | 11.36 | 8.70 | 49 | 66 | 13 |
| RC1 ra-off | 1,411,612 | +8.1% | 1.4% | 3.07 | 2.18 | 45 | 61 | 13 |
| RC1 ra-on | 1,392,671 | +6.6% | 0.8% | 3.17 | 2.28 | 47 | 63 | 13 |
| RC2 ra-off | 1,347,721 | +3.2% | 2.3% | 8.00 | 5.93 | 43 | 98 | 13 |
| RC2 ra-on | 1,312,252 | +0.5% | 1.3% | 7.95 | 6.03 | 43 | 131 | 14 |
| RC2 ra-off threshold=0 | 1,427,320 | +9.3% | 0.6% | 3.35 | 2.35 | 45 | 59 | 13 |
| RC2 ra-off threshold=4 | 1,466,751 | +12.3% | 1.3% | 5.92 | 4.02 | 43 | 57 | 13 |
| RC2 ra-off threshold=32767 | 1,321,798 | +1.2% | 2.1% | 7.88 | 5.98 | 43 | 131 | 13 |

## 64 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,029,290 | +0.0% | 0.4% | 11.58 | 11.25 | 55 | 86 | 13 |
| RC1 ra-off | 593,281 | -42.4% | 7.8% | 5.90 | 9.94 | 74 | 623 | 14 |
| RC1 ra-on | 590,876 | -42.6% | 1.4% | 5.95 | 10.08 | 78 | 623 | 15 |
| RC2 ra-off | 676,674 | -34.3% | 0.5% | 7.87 | 11.63 | 70 | 344 | 13 |
| RC2 ra-on | 667,200 | -35.2% | 1.2% | 7.72 | 11.60 | 70 | 360 | 14 |
| RC2 ra-off threshold=0 | 627,601 | -39.0% | 1.3% | 7.28 | 11.60 | 74 | 459 | 14 |
| RC2 ra-off threshold=4 | 628,357 | -39.0% | 1.0% | 7.32 | 11.66 | 78 | 393 | 13 |
| RC2 ra-off threshold=32767 | 696,779 | -32.3% | 3.3% | 8.57 | 12.30 | 66 | 328 | 13 |

## 64 callers, 20 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 423,921 | +0.0% | 0.3% | 11.60 | 27.32 | 131 | 205 | 13 |
| RC1 ra-off | 346,726 | -18.2% | 0.2% | 8.96 | 25.82 | 156 | 524 | 14 |
| RC1 ra-on | 348,247 | -17.9% | 0.6% | 8.97 | 25.74 | 156 | 508 | 15 |
| RC2 ra-off | 336,704 | -20.6% | 0.5% | 9.52 | 28.32 | 164 | 508 | 13 |
| RC2 ra-on | 335,535 | -20.8% | 0.7% | 9.51 | 28.36 | 156 | 508 | 13 |
| RC2 ra-off threshold=0 | 339,884 | -19.8% | 1.3% | 9.51 | 28.00 | 164 | 410 | 13 |
| RC2 ra-off threshold=4 | 341,310 | -19.5% | 1.8% | 9.59 | 28.09 | 156 | 459 | 14 |
| RC2 ra-off threshold=32767 | 324,936 | -23.3% | 0.7% | 9.45 | 29.09 | 172 | 557 | 13 |

## 64 callers, 78% local hits, 1 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 4,189,021 | +0.0% | 0.4% | 11.41 | 2.72 | 0 | 82 | 14 |
| RC1 ra-off | 2,549,064 | -39.1% | 2.4% | 5.52 | 2.16 | 0 | 279 | 14 |
| RC1 ra-on | 2,572,109 | -38.6% | 0.7% | 5.70 | 2.21 | 0 | 262 | 14 |
| RC2 ra-off | 2,965,051 | -29.2% | 1.7% | 7.79 | 2.63 | 0 | 164 | 14 |
| RC2 ra-on | 2,952,391 | -29.5% | 2.2% | 7.74 | 2.62 | 0 | 172 | 14 |
| RC2 ra-off threshold=0 | 2,791,034 | -33.4% | 1.2% | 7.22 | 2.58 | 0 | 180 | 13 |
| RC2 ra-off threshold=4 | 2,808,881 | -32.9% | 0.5% | 7.36 | 2.62 | 0 | 180 | 14 |
| RC2 ra-off threshold=32767 | 3,014,816 | -28.0% | 0.7% | 8.39 | 2.80 | 0 | 164 | 13 |

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
