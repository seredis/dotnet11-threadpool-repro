# core

2026-10-29T20:25:17, AMD Ryzen 9 7900X 12-Core Processor, kernel 7.2.6-76070206-generic, server CPUs 0-3,12-15, client CPUs 6-11,18-23, 3 repeats of 10 s after 3 s warmup. Medians; the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. **busy** is CPU cores the client kept busy (process CPU time / wall time).

## 1 caller

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 45,559 | +0.0% | 0.2% | 3.11 | 68.20 | 22 | 27 | 4 |
| RC1 ra-off | 39,894 | -12.4% | 6.5% | 1.40 | 35.01 | 24 | 31 | 4 |
| RC1 ra-on | 37,500 | -17.7% | 8.4% | 1.31 | 34.86 | 26 | 31 | 4 |
| RC2 ra-off | 43,656 | -4.2% | 2.9% | 3.64 | 82.26 | 23 | 28 | 4 |
| RC2 ra-on | 43,297 | -5.0% | 1.4% | 3.56 | 82.21 | 23 | 28 | 4 |
| RC2 ra-off threshold=0 | 40,152 | -11.9% | 1.5% | 1.38 | 34.46 | 24 | 30 | 4 |
| RC2 ra-off threshold=4 | 43,590 | -4.3% | 1.1% | 3.56 | 81.46 | 23 | 28 | 4 |
| RC2 ra-off threshold=32767 | 43,499 | -4.5% | 0.8% | 3.59 | 82.05 | 23 | 28 | 4 |

## 64 callers

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,292,330 | +0.0% | 0.9% | 11.14 | 8.60 | 47 | 66 | 14 |
| RC1 ra-off | 1,143,585 | -11.5% | 1.2% | 2.87 | 2.49 | 55 | 66 | 13 |
| RC1 ra-on | 1,214,047 | -6.1% | 1.0% | 3.19 | 2.63 | 51 | 70 | 13 |
| RC2 ra-off | 1,292,638 | +0.0% | 2.9% | 7.95 | 6.09 | 45 | 106 | 14 |
| RC2 ra-on | 1,280,339 | -0.9% | 1.1% | 8.03 | 6.28 | 45 | 111 | 13 |
| RC2 ra-off threshold=0 | 1,198,844 | -7.2% | 0.3% | 3.41 | 2.85 | 53 | 66 | 13 |
| RC2 ra-off threshold=4 | 1,389,745 | +7.5% | 1.5% | 5.94 | 4.27 | 45 | 59 | 13 |
| RC2 ra-off threshold=32767 | 1,270,376 | -1.7% | 0.3% | 7.88 | 6.20 | 45 | 111 | 13 |

## 64 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,008,742 | +0.0% | 0.5% | 11.49 | 11.39 | 53 | 86 | 14 |
| RC1 ra-off | 621,249 | -38.4% | 2.8% | 6.35 | 10.22 | 74 | 590 | 13 |
| RC1 ra-on | 606,734 | -39.9% | 2.0% | 6.21 | 10.22 | 74 | 623 | 15 |
| RC2 ra-off | 659,785 | -34.6% | 1.2% | 7.73 | 11.71 | 70 | 344 | 14 |
| RC2 ra-on | 657,797 | -34.8% | 4.2% | 7.71 | 11.68 | 70 | 360 | 15 |
| RC2 ra-off threshold=0 | 619,308 | -38.6% | 1.2% | 7.30 | 11.80 | 78 | 442 | 15 |
| RC2 ra-off threshold=4 | 615,124 | -39.0% | 0.7% | 7.32 | 11.90 | 82 | 393 | 14 |
| RC2 ra-off threshold=32767 | 679,594 | -32.6% | 0.9% | 8.50 | 12.53 | 70 | 328 | 16 |

## 64 callers, 20 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 426,995 | +0.0% | 0.0% | 11.63 | 27.24 | 131 | 197 | 13 |
| RC1 ra-off | 342,154 | -19.9% | 1.3% | 8.88 | 25.98 | 156 | 524 | 13 |
| RC1 ra-on | 344,699 | -19.3% | 1.0% | 8.94 | 25.89 | 156 | 524 | 14 |
| RC2 ra-off | 336,539 | -21.2% | 1.6% | 9.55 | 28.39 | 164 | 492 | 13 |
| RC2 ra-on | 335,842 | -21.3% | 0.8% | 9.57 | 28.42 | 156 | 492 | 14 |
| RC2 ra-off threshold=0 | 337,876 | -20.9% | 0.9% | 9.55 | 28.23 | 164 | 426 | 14 |
| RC2 ra-off threshold=4 | 337,659 | -20.9% | 0.3% | 9.55 | 28.29 | 156 | 475 | 14 |
| RC2 ra-off threshold=32767 | 322,499 | -24.5% | 0.8% | 9.47 | 29.40 | 172 | 557 | 13 |

## 64 callers, 78% local hits, 1 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 4,239,317 | +0.0% | 0.2% | 11.33 | 2.67 | 0 | 86 | 15 |
| RC1 ra-off | 2,711,974 | -36.0% | 0.2% | 6.03 | 2.22 | 0 | 221 | 13 |
| RC1 ra-on | 2,640,277 | -37.7% | 1.2% | 5.90 | 2.23 | 0 | 246 | 14 |
| RC2 ra-off | 2,959,561 | -30.2% | 1.3% | 7.78 | 2.63 | 0 | 172 | 14 |
| RC2 ra-on | 2,977,956 | -29.8% | 1.9% | 7.86 | 2.64 | 0 | 164 | 13 |
| RC2 ra-off threshold=0 | 2,799,031 | -34.0% | 0.4% | 7.27 | 2.60 | 0 | 180 | 14 |
| RC2 ra-off threshold=4 | 2,786,089 | -34.3% | 1.6% | 7.35 | 2.64 | 0 | 188 | 14 |
| RC2 ra-off threshold=32767 | 3,002,115 | -29.2% | 1.9% | 8.45 | 2.80 | 0 | 164 | 13 |

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
