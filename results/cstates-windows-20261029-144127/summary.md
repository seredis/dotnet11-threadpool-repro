# C-states, native Windows

2026-10-29, AMD Ryzen 9 7900X, Windows 26340, server cores 0-3, client cores 6-11 (12 CPUs), 3 repeats of 10 s after 3 s warmup, interleaved. Medians.

## 1 caller

| row | idle | ops/s | vs .NET 10 (same idle) | busy | p50 us | p99 us | threads |
|---|---|---:|---:|---:|---:|---:|---:|
| .NET 10 | deep idle on (default) | 19,627 |  | 1.42 | 50 | 77 | 4 |
| .NET 10 | idle disabled | 25,326 |  | 2.15 | 38 | 54 | 5 |
| RC1 | deep idle on (default) | 19,810 | +0.9% | 1.11 | 48 | 74 | 4 |
| RC1 | idle disabled | 24,522 | -3.2% | 1.66 | 38 | 54 | 4 |
| RC2 | deep idle on (default) | 24,517 | +24.9% | 3.09 | 38 | 64 | 4 |
| RC2 | idle disabled | 27,248 | +7.6% | 3.54 | 35 | 50 | 4 |
| RC2 threshold=0 | deep idle on (default) | 22,857 | +16.5% | 1.29 | 42 | 67 | 4 |
| RC2 threshold=0 | idle disabled | 26,276 | +3.8% | 2.28 | 37 | 51 | 4 |

## 64 callers

| row | idle | ops/s | vs .NET 10 (same idle) | busy | p50 us | p99 us | threads |
|---|---|---:|---:|---:|---:|---:|---:|
| .NET 10 | deep idle on (default) | 928,980 |  | 2.19 | 64 | 109 | 15 |
| .NET 10 | idle disabled | 914,584 |  | 4.78 | 67 | 102 | 18 |
| RC1 | deep idle on (default) | 919,747 | -1.0% | 1.94 | 64 | 115 | 12 |
| RC1 | idle disabled | 899,725 | -1.6% | 3.01 | 67 | 102 | 12 |
| RC2 | deep idle on (default) | 1,023,367 | +10.2% | 4.62 | 58 | 102 | 12 |
| RC2 | idle disabled | 921,501 | +0.8% | 6.20 | 67 | 99 | 13 |
| RC2 threshold=0 | deep idle on (default) | 980,224 | +5.5% | 2.55 | 58 | 109 | 12 |
| RC2 threshold=0 | idle disabled | 912,216 | -0.3% | 4.05 | 67 | 102 | 12 |

## 64 callers, 5 us work

| row | idle | ops/s | vs .NET 10 (same idle) | busy | p50 us | p99 us | threads |
|---|---|---:|---:|---:|---:|---:|---:|
| .NET 10 | deep idle on (default) | 689,489 |  | 7.22 | 90 | 141 | 14 |
| .NET 10 | idle disabled | 758,409 |  | 8.65 | 77 | 115 | 13 |
| RC1 | deep idle on (default) | 686,416 | -0.4% | 7.40 | 86 | 147 | 13 |
| RC1 | idle disabled | 724,890 | -4.4% | 8.38 | 80 | 122 | 13 |
| RC2 | deep idle on (default) | 825,327 | +19.7% | 10.86 | 67 | 141 | 13 |
| RC2 | idle disabled | 797,805 | +5.2% | 11.19 | 74 | 115 | 17 |
| RC2 threshold=0 | deep idle on (default) | 819,979 | +18.9% | 10.61 | 67 | 134 | 14 |
| RC2 threshold=0 | idle disabled | 785,821 | +3.6% | 10.99 | 74 | 115 | 13 |

## 64 callers, 20 us work

| row | idle | ops/s | vs .NET 10 (same idle) | busy | p50 us | p99 us | threads |
|---|---|---:|---:|---:|---:|---:|---:|
| .NET 10 | deep idle on (default) | 526,835 |  | 11.89 | 99 | 205 | 13 |
| .NET 10 | idle disabled | 524,616 |  | 11.84 | 99 | 198 | 13 |
| RC1 | deep idle on (default) | 524,110 | -0.5% | 11.80 | 99 | 205 | 14 |
| RC1 | idle disabled | 523,483 | -0.2% | 11.89 | 99 | 198 | 13 |
| RC2 | deep idle on (default) | 527,521 | +0.1% | 11.85 | 99 | 205 | 14 |
| RC2 | idle disabled | 524,996 | +0.1% | 11.86 | 99 | 192 | 13 |
| RC2 threshold=0 | deep idle on (default) | 526,459 | -0.1% | 11.86 | 99 | 205 | 14 |
| RC2 threshold=0 | idle disabled | 526,577 | +0.4% | 11.86 | 99 | 186 | 13 |

## 64 callers, 78% local hits, 1 us work

| row | idle | ops/s | vs .NET 10 (same idle) | busy | p50 us | p99 us | threads |
|---|---|---:|---:|---:|---:|---:|---:|
| .NET 10 | deep idle on (default) | 3,255,349 |  | 7.61 | 0 | 122 | 13 |
| .NET 10 | idle disabled | 3,505,894 |  | 8.84 | 0 | 102 | 13 |
| RC1 | deep idle on (default) | 3,238,711 | -0.5% | 7.06 | 0 | 122 | 14 |
| RC1 | idle disabled | 3,348,308 | -4.5% | 8.35 | 0 | 109 | 13 |
| RC2 | deep idle on (default) | 3,803,682 | +16.8% | 10.85 | 0 | 109 | 13 |
| RC2 | idle disabled | 3,667,711 | +4.6% | 11.03 | 0 | 102 | 14 |
| RC2 threshold=0 | deep idle on (default) | 3,769,055 | +15.8% | 10.14 | 0 | 109 | 13 |
| RC2 threshold=0 | idle disabled | 3,629,946 | +3.5% | 10.83 | 0 | 102 | 13 |
