# redis

2026-10-15T10:02:00, AMD Ryzen 9 7900X 12-Core Processor, kernel 7.1.5-76070105-generic, server CPUs 0-3,12-15, client CPUs 6-11,18-23, 3 repeats of 10 s after 3 s warmup. Medians; the percentage is against the .NET 10 row (of the same library, for Redis) in the same cell. **busy** is CPU cores the client kept busy (process CPU time / wall time).

## GET, 1 caller

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| v3 .NET 10 | 47,580 | +0.0% | 1.1% | 3.10 | 64.75 | 20 | 25 | 6 |
| v3 RC1 ra-off | 39,829 | -16.3% | 3.9% | 1.41 | 35.30 | 25 | 32 | 5 |
| v3 RC1 ra-on | 40,346 | -15.2% | 2.8% | 1.38 | 33.44 | 24 | 32 | 5 |
| v3 RC2 ra-off | 43,969 | -7.6% | 1.2% | 3.82 | 86.86 | 22 | 28 | 5 |
| v3 RC2 ra-on | 43,827 | -7.9% | 1.0% | 3.91 | 89.33 | 22 | 28 | 5 |
| v3 RC2 ra-off threshold=0 | 41,854 | -12.0% | 1.4% | 1.39 | 33.08 | 23 | 30 | 5 |
| v3 RC2 ra-off threshold=32767 | 44,642 | -6.2% | 2.7% | 3.83 | 85.83 | 22 | 28 | 5 |
| v4 .NET 10 | 46,430 | +0.0% | 0.4% | 3.18 | 68.39 | 20 | 26 | 12 |
| v4 RC1 ra-off | 42,509 | -8.4% | 10.1% | 2.36 | 55.36 | 23 | 32 | 10 |
| v4 RC1 ra-on | 39,558 | -14.8% | 7.0% | 2.40 | 61.42 | 25 | 32 | 9 |
| v4 RC2 ra-off | 44,396 | -4.4% | 1.1% | 4.09 | 92.93 | 22 | 28 | 9 |
| v4 RC2 ra-on | 43,437 | -6.4% | 0.7% | 4.17 | 95.98 | 23 | 28 | 10 |
| v4 RC2 ra-off threshold=0 | 43,048 | -7.3% | 1.3% | 2.38 | 55.24 | 23 | 30 | 9 |
| v4 RC2 ra-off threshold=32767 | 43,849 | -5.6% | 0.2% | 4.09 | 93.46 | 22 | 28 | 10 |

## GET, 64 callers

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| v3 .NET 10 | 379,584 | +0.0% | 2.6% | 8.29 | 21.85 | 156 | 311 | 16 |
| v3 RC1 ra-off | 382,261 | +0.7% | 2.5% | 3.05 | 8.01 | 156 | 360 | 13 |
| v3 RC1 ra-on | 374,992 | -1.2% | 2.6% | 3.26 | 8.73 | 164 | 410 | 14 |
| v3 RC2 ra-off | 345,182 | -9.1% | 0.9% | 7.69 | 22.10 | 172 | 410 | 14 |
| v3 RC2 ra-on | 355,973 | -6.2% | 1.9% | 7.71 | 21.67 | 172 | 459 | 14 |
| v3 RC2 ra-off threshold=0 | 374,575 | -1.3% | 0.7% | 3.13 | 8.38 | 164 | 360 | 12 |
| v3 RC2 ra-off threshold=32767 | 341,909 | -9.9% | 1.3% | 7.66 | 22.30 | 180 | 410 | 15 |
| v4 .NET 10 | 980,174 | +0.0% | 1.6% | 11.04 | 11.34 | 61 | 188 | 21 |
| v4 RC1 ra-off | 974,914 | -0.5% | 2.0% | 5.46 | 5.60 | 63 | 180 | 20 |
| v4 RC1 ra-on | 952,449 | -2.8% | 1.4% | 5.67 | 5.95 | 66 | 197 | 21 |
| v4 RC2 ra-off | 846,333 | -13.7% | 2.5% | 7.45 | 8.81 | 59 | 410 | 23 |
| v4 RC2 ra-on | 831,128 | -15.2% | 2.7% | 7.47 | 8.96 | 61 | 393 | 23 |
| v4 RC2 ra-off threshold=0 | 1,028,262 | +4.9% | 0.8% | 6.39 | 6.25 | 59 | 188 | 18 |
| v4 RC2 ra-off threshold=32767 | 758,586 | -22.6% | 0.9% | 7.25 | 9.59 | 61 | 508 | 23 |

## GET, 64 callers, 20 us work

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| v3 .NET 10 | 313,903 | +0.0% | 1.0% | 10.38 | 33.06 | 172 | 426 | 18 |
| v3 RC1 ra-off | 282,140 | -10.1% | 2.1% | 7.86 | 27.94 | 197 | 623 | 21 |
| v3 RC1 ra-on | 279,894 | -10.8% | 1.0% | 7.93 | 28.35 | 197 | 590 | 16 |
| v3 RC2 ra-off | 254,727 | -18.9% | 0.2% | 8.18 | 32.12 | 221 | 786 | 20 |
| v3 RC2 ra-on | 255,472 | -18.6% | 2.2% | 8.18 | 32.14 | 221 | 754 | 16 |
| v3 RC2 ra-off threshold=0 | 259,036 | -17.5% | 0.7% | 8.07 | 31.32 | 221 | 754 | 21 |
| v3 RC2 ra-off threshold=32767 | 256,366 | -18.3% | 1.3% | 8.41 | 33.01 | 221 | 786 | 16 |
| v4 .NET 10 | 370,697 | +0.0% | 1.7% | 11.06 | 29.84 | 147 | 377 | 23 |
| v4 RC1 ra-off | 311,497 | -16.0% | 1.5% | 8.73 | 28.16 | 172 | 655 | 23 |
| v4 RC1 ra-on | 305,984 | -17.5% | 2.2% | 8.66 | 28.23 | 172 | 655 | 22 |
| v4 RC2 ra-off | 305,845 | -17.5% | 3.1% | 9.31 | 30.50 | 172 | 623 | 20 |
| v4 RC2 ra-on | 302,040 | -18.5% | 1.4% | 9.26 | 30.65 | 172 | 623 | 23 |
| v4 RC2 ra-off threshold=0 | 305,974 | -17.5% | 2.7% | 9.27 | 30.29 | 172 | 623 | 19 |
| v4 RC2 ra-off threshold=32767 | 290,262 | -21.7% | 1.6% | 9.12 | 31.43 | 180 | 721 | 18 |

## GET, 64 callers, v4 client cache, Zipf

| row | ops/s | vs .NET 10 | spread | busy | CPU us/op | p50 us | p99 us | threads |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| v4 .NET 10 | 2,702,274 | +0.0% | 0.4% | 10.67 | 3.95 | 1 | 111 | 23 |
| v4 RC1 ra-off | 1,754,941 | -35.1% | 0.9% | 5.67 | 3.22 | 0 | 524 | 23 |
| v4 RC1 ra-on | 1,735,427 | -35.8% | 2.3% | 5.68 | 3.28 | 0 | 524 | 21 |
| v4 RC2 ra-off | 2,044,608 | -24.3% | 1.1% | 7.55 | 3.69 | 0 | 279 | 30 |
| v4 RC2 ra-on | 2,032,973 | -24.8% | 0.3% | 7.57 | 3.73 | 1 | 279 | 23 |
| v4 RC2 ra-off threshold=0 | 1,843,873 | -31.8% | 1.2% | 6.80 | 3.67 | 1 | 410 | 23 |
| v4 RC2 ra-off threshold=32767 | 2,093,615 | -22.5% | 5.7% | 8.16 | 3.95 | 0 | 254 | 23 |

## What ran

- **v3 .NET 10**: 10.0.12-servicing.26422.108+95017c711e6afc1085133d440e42b4bd78155701 on Ubuntu 24.04.5 LTS; env {}; RESPite 0 runtime-async / 2 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 98 state-machine; System.Net.Sockets 0 runtime-async / 9 state-machine; System.Private.CoreLib 10 runtime-async / 0 state-machine; StackExchange.Redis 3.4.0 @ refs/tags/3.4.0
- **v3 RC1 ra-off**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; RESPite 0 runtime-async / 2 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 98 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 3.4.0 @ refs/tags/3.4.0
- **v3 RC1 ra-on**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; RESPite 2 runtime-async / 0 state-machine; RedisRepro 4 runtime-async / 0 state-machine; StackExchange.Redis 98 runtime-async / 0 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 3.4.0 @ refs/tags/3.4.0
- **v3 RC2 ra-off**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; RESPite 0 runtime-async / 2 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 98 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 3.4.0 @ refs/tags/3.4.0
- **v3 RC2 ra-on**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; RESPite 2 runtime-async / 0 state-machine; RedisRepro 4 runtime-async / 0 state-machine; StackExchange.Redis 98 runtime-async / 0 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 3.4.0 @ refs/tags/3.4.0
- **v3 RC2 ra-off threshold=0**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {'DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold': '0'}; RESPite 0 runtime-async / 2 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 98 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 3.4.0 @ refs/tags/3.4.0
- **v3 RC2 ra-off threshold=32767**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {'DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold': '32767'}; RESPite 0 runtime-async / 2 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 98 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 3.4.0 @ refs/tags/3.4.0
- **v4 .NET 10**: 10.0.12-servicing.26422.108+95017c711e6afc1085133d440e42b4bd78155701 on Ubuntu 24.04.5 LTS; env {}; RESPite 0 runtime-async / 8 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 162 state-machine; System.Net.Sockets 0 runtime-async / 9 state-machine; System.Private.CoreLib 10 runtime-async / 0 state-machine; StackExchange.Redis 4.0.0-alpha @ 31c80eef443fcb0a03ea981e04ce15e5476a3fa1
- **v4 RC1 ra-off**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; RESPite 0 runtime-async / 8 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 162 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 4.0.0-alpha @ 31c80eef443fcb0a03ea981e04ce15e5476a3fa1
- **v4 RC1 ra-on**: 11.0.0-rc.1.26425.128+3551975be08744f0418857c5bed8ab1545c5dd47 on Ubuntu 26.04.1 LTS; env {}; RESPite 8 runtime-async / 0 state-machine; RedisRepro 4 runtime-async / 0 state-machine; StackExchange.Redis 160 runtime-async / 2 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 4.0.0-alpha @ 31c80eef443fcb0a03ea981e04ce15e5476a3fa1
- **v4 RC2 ra-off**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; RESPite 0 runtime-async / 8 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 162 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 4.0.0-alpha @ 31c80eef443fcb0a03ea981e04ce15e5476a3fa1
- **v4 RC2 ra-on**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {}; RESPite 8 runtime-async / 0 state-machine; RedisRepro 4 runtime-async / 0 state-machine; StackExchange.Redis 160 runtime-async / 2 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 4.0.0-alpha @ 31c80eef443fcb0a03ea981e04ce15e5476a3fa1
- **v4 RC2 ra-off threshold=0**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {'DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold': '0'}; RESPite 0 runtime-async / 8 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 162 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 4.0.0-alpha @ 31c80eef443fcb0a03ea981e04ce15e5476a3fa1
- **v4 RC2 ra-off threshold=32767**: 11.0.0-rc.2.26504.105+820230c386db5b0106c5430b3e5aee92a7826bf5 on Ubuntu 26.04.1 LTS; env {'DOTNET_ThreadPool_SpuriousDispatchNoSpinThreshold': '32767'}; RESPite 0 runtime-async / 8 state-machine; RedisRepro 0 runtime-async / 4 state-machine; StackExchange.Redis 0 runtime-async / 162 state-machine; System.Net.Sockets 9 runtime-async / 0 state-machine; System.Private.CoreLib 101 runtime-async / 0 state-machine; StackExchange.Redis 4.0.0-alpha @ 31c80eef443fcb0a03ea981e04ce15e5476a3fa1

Processor count seen by the client: 12.
