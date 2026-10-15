# callers

2026-10-15T09:39:51, AMD Ryzen 9 7900X 12-Core Processor, kernel 7.1.5-76070105-generic, server CPUs 0-3,12-15, client CPUs 6-11,18-23, 3 repeats of 10 s after 3 s warmup. Medians; the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. **busy** is CPU cores the client kept busy (process CPU time / wall time).

## 1 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 42,821 | +0.0% | 1.4% | 2.99 | 69.74 | 17 | 22 | 4 |
| RC1 ra-off | 36,896 | -13.8% | 3.3% | 1.45 | 39.40 | 22 | 27 | 4 |
| RC2 ra-off | 40,748 | -4.8% | 2.1% | 3.20 | 78.44 | 18 | 24 | 4 |

## 4 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 120,694 | +0.0% | 0.5% | 5.22 | 43.27 | 27 | 41 | 9 |
| RC1 ra-off | 114,065 | -5.5% | 1.7% | 2.69 | 23.74 | 30 | 45 | 7 |
| RC2 ra-off | 116,546 | -3.4% | 1.1% | 5.43 | 46.87 | 28 | 43 | 8 |

## 16 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 395,782 | +0.0% | 0.3% | 11.73 | 29.67 | 33 | 57 | 13 |
| RC1 ra-off | 345,448 | -12.7% | 0.9% | 5.67 | 16.39 | 39 | 66 | 12 |
| RC2 ra-off | 333,504 | -15.7% | 1.0% | 8.30 | 24.93 | 37 | 131 | 13 |

## 64 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,033,697 | +0.0% | 0.6% | 11.55 | 11.19 | 55 | 86 | 13 |
| RC1 ra-off | 578,273 | -44.1% | 4.4% | 5.77 | 9.96 | 78 | 688 | 14 |
| RC2 ra-off | 662,196 | -35.9% | 1.8% | 7.72 | 11.60 | 70 | 360 | 14 |

## 256 callers, 5 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| .NET 10 | 1,367,244 | +0.0% | 0.7% | 11.37 | 8.33 | 180 | 229 | 15 |
| RC1 ra-off | 994,114 | -27.3% | 0.8% | 7.72 | 7.76 | 213 | 1180 | 15 |
| RC2 ra-off | 1,007,302 | -26.3% | 1.1% | 8.60 | 8.56 | 229 | 819 | 14 |

## What ran

- **.NET 10**: 10.0.12-servicing.26422.108+95017c711e6afc1085133d440e42b4bd78155701 on Ubuntu 24.04.5 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 0 runtime-async / 9 state-machine; System.Private.CoreLib 10 runtime-async / 0 state-machine
- **RC1 ra-off**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine
- **RC2 ra-off**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; Repro 0 runtime-async / 5 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine

Processor count seen by the client: 12.
