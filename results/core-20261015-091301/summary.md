# core

2026-10-15T09:13:01, AMD Ryzen 9 7900X 12-Core Processor, kernel 7.1.5-76070105-generic, server CPUs 0-3,12-15, client CPUs 6-11,18-23, 3 repeats of 10 s after 3 s warmup. Medians; the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. **busy** is CPU cores the client kept busy (process CPU time / wall time).

## 1 caller

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 55,221 | +0.0% | 0.9% | 3.06 | 55.35 | 17 | 22 | 4 |
| RC1 ra-off | 46,105 | -16.5% | 8.1% | 1.46 | 31.76 | 20 | 27 | 4 |
| RC1 ra-on | 44,124 | -20.1% | 4.3% | 1.55 | 35.23 | 22 | 28 | 4 |
| RC2 ra-off | 52,983 | -4.1% | 0.1% | 3.60 | 68.01 | 18 | 23 | 4 |
| RC2 ra-on | 52,548 | -4.8% | 1.7% | 3.52 | 67.03 | 18 | 23 | 4 |
| RC2 ra-off threshold=0 | 48,473 | -12.2% | 4.7% | 1.48 | 30.44 | 20 | 26 | 4 |
| RC2 ra-off threshold=4 | 52,681 | -4.6% | 1.0% | 3.61 | 68.42 | 18 | 23 | 4 |
| RC2 ra-off threshold=32767 | 52,915 | -4.2% | 1.4% | 3.53 | 66.71 | 18 | 23 | 4 |

## 64 callers

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,308,509 | +0.0% | 0.7% | 11.30 | 8.62 | 47 | 66 | 13 |
| RC1 ra-off | 1,417,869 | +8.4% | 0.4% | 3.01 | 2.12 | 45 | 61 | 12 |
| RC1 ra-on | 1,397,010 | +6.8% | 0.9% | 3.23 | 2.31 | 47 | 63 | 13 |
| RC2 ra-off | 1,358,185 | +3.8% | 2.1% | 8.04 | 5.92 | 43 | 94 | 13 |
| RC2 ra-on | 1,323,084 | +1.1% | 0.9% | 7.98 | 6.05 | 43 | 119 | 14 |
| RC2 ra-off threshold=0 | 1,430,960 | +9.4% | 0.8% | 3.32 | 2.32 | 45 | 61 | 13 |
| RC2 ra-off threshold=4 | 1,465,565 | +12.0% | 0.6% | 5.87 | 4.01 | 43 | 57 | 13 |
| RC2 ra-off threshold=32767 | 1,358,890 | +3.9% | 1.3% | 7.98 | 5.88 | 43 | 102 | 13 |

## 64 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,029,128 | +0.0% | 0.6% | 11.56 | 11.24 | 55 | 86 | 13 |
| RC1 ra-off | 573,273 | -44.3% | 3.2% | 5.69 | 9.93 | 74 | 688 | 15 |
| RC1 ra-on | 570,609 | -44.6% | 3.3% | 5.69 | 9.99 | 78 | 721 | 15 |
| RC2 ra-off | 667,782 | -35.1% | 1.0% | 7.76 | 11.62 | 70 | 360 | 14 |
| RC2 ra-on | 668,228 | -35.1% | 0.6% | 7.77 | 11.63 | 70 | 360 | 14 |
| RC2 ra-off threshold=0 | 625,107 | -39.3% | 0.6% | 7.21 | 11.58 | 74 | 475 | 14 |
| RC2 ra-off threshold=4 | 621,719 | -39.6% | 1.1% | 7.26 | 11.70 | 78 | 393 | 13 |
| RC2 ra-off threshold=32767 | 695,212 | -32.4% | 1.3% | 8.55 | 12.26 | 66 | 328 | 13 |

## 64 callers, 20 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 425,625 | +0.0% | 0.6% | 11.56 | 27.20 | 131 | 205 | 13 |
| RC1 ra-off | 354,145 | -16.8% | 0.2% | 9.09 | 25.65 | 156 | 492 | 13 |
| RC1 ra-on | 350,098 | -17.7% | 0.7% | 9.01 | 25.74 | 156 | 508 | 16 |
| RC2 ra-off | 343,018 | -19.4% | 1.1% | 9.57 | 27.96 | 156 | 524 | 14 |
| RC2 ra-on | 339,933 | -20.1% | 2.0% | 9.55 | 28.09 | 156 | 524 | 13 |
| RC2 ra-off threshold=0 | 349,558 | -17.9% | 1.3% | 9.63 | 27.56 | 156 | 426 | 13 |
| RC2 ra-off threshold=4 | 347,762 | -18.3% | 1.7% | 9.64 | 27.71 | 147 | 475 | 13 |
| RC2 ra-off threshold=32767 | 326,388 | -23.3% | 0.1% | 9.39 | 28.77 | 164 | 623 | 13 |

## 64 callers, 78% local hits, 1 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 4,235,983 | +0.0% | 0.7% | 11.37 | 2.68 | 0 | 82 | 13 |
| RC1 ra-off | 2,470,390 | -41.7% | 1.0% | 5.40 | 2.19 | 0 | 311 | 15 |
| RC1 ra-on | 2,432,914 | -42.6% | 1.6% | 5.40 | 2.20 | 0 | 311 | 15 |
| RC2 ra-off | 2,946,735 | -30.4% | 1.3% | 7.73 | 2.62 | 0 | 164 | 15 |
| RC2 ra-on | 2,908,804 | -31.3% | 2.9% | 7.55 | 2.60 | 0 | 172 | 13 |
| RC2 ra-off threshold=0 | 2,746,548 | -35.2% | 1.7% | 7.09 | 2.58 | 0 | 188 | 14 |
| RC2 ra-off threshold=4 | 2,821,757 | -33.4% | 2.1% | 7.31 | 2.61 | 0 | 172 | 13 |
| RC2 ra-off threshold=32767 | 2,961,267 | -30.1% | 1.9% | 8.21 | 2.77 | 0 | 164 | 14 |

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
