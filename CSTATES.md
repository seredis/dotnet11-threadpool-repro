# Idle states (C-states) and the regression

[@VSadov asked](https://github.com/dotnet/runtime/issues/135855#issuecomment-6483057817) whether deep
sleep (C6 and similar) explains why not everyone can reproduce this: a parked worker that has to wake
from a deep state costs latency, and in a ping-pong workload latency gates throughput.

## Windows: done

[results/cstates-windows-20261029-144127](results/cstates-windows-20261029-144127/summary.md), posted
[here](https://github.com/dotnet/runtime/issues/135855#issuecomment-6500895461). It ran on the same
7900X as the Linux results (the machine dual-boots), natively, with `tools/cstates-windows.py`.

- The regression does not reproduce on Windows, with idle states on or off. RC1 matches .NET 10, and
  RC2 is 10-25% *faster* with idle states on and 1-8% faster with them off.
- .NET 10 on Windows looks like .NET 11 on Linux: in the 5 µs cell it keeps about 7 cores busy
  (689k ops/s), against 11.6 (1,029k) on Linux. RC2 keeps about 11 busy (825k) on Windows.
- Disabling idle states helps the runtime that parks more: .NET 10 +10% in the 5 µs cell and +29%
  at 1 caller. RC2 changes by +11% at 1 caller and -3% to -10% with 64 callers.
- Only "all idle states off" (`IDLEDISABLE=1`) could be tested. `IDLESTATEMAX`, which would keep C1,
  is ignored while Hyper-V is running.

## Linux (popos): to do

Same machine, Pop!_OS, kernel 7.1. Run `./run.py core` under three idle configurations, so the C-state
effect is measured on the OS where the regression shows:

1. **default:** all states enabled (a same-day baseline; the 2026-10-15 run was presumably default, but its idle settings were not recorded)
2. **deep states off, C1 kept:** the configuration VSadov asked about
3. **all states off (POLL only):** matches the Windows "idle disabled" run

```bash
cpupower idle-info                    # if missing: sudo apt install linux-tools-common linux-tools-$(uname -r)
grep . /sys/devices/system/cpu/cpu6/cpuidle/state*/{name,latency}   # state names and exit latency (us)

sudo cpupower idle-set -E && ./run.py core                     # 1. default
sudo cpupower idle-set -E && sudo cpupower idle-set -D 2 && ./run.py core   # 2. disable states with latency > 2 us
sudo cpupower idle-set -E && sudo cpupower idle-set -D 0 && ./run.py core   # 3. POLL only
sudo cpupower idle-set -E                                       # restore
```

Check the `-D` threshold against the latencies listed: configuration 2 should leave POLL and C1
enabled and disable everything deeper (on Zen 4, ACPI C2 is the deep CC6 state). `cpupower idle-set`
without `cpupower` installed: write `1` to `/sys/devices/system/cpu/cpu*/cpuidle/stateN/disable`.

`run.py` does not record idle state, so for each run save the configuration next to its results
(`cpupower idle-info > results/core-<timestamp>/idle.txt`) and rename the directory to say which
configuration it is (e.g. `core-20261030-...-c1only`). Confirm each setting took effect: the `usage`
counter of a disabled state in `/sys/devices/system/cpu/cpu6/cpuidle/stateN/usage` should not move
during a run.

Each `core` run takes about 30 minutes. Then draft the Linux numbers as a follow-up for the issue,
compared with .NET 10 under the same idle configuration, like the Windows comment.
