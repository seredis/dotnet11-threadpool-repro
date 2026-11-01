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

## Linux (popos): done

Same machine, Pop!_OS, kernel 7.2.6, 2026-10-29. Three `./run.py core` runs, one per idle
configuration. The cpuidle states on this kernel are POLL (0 µs exit latency), C1 (1 µs), ACPI C2
(18 µs) and C3 (350 µs); `cpupower` lacked the module for this kernel, so states were toggled by
writing `/sys/devices/system/cpu/cpu*/cpuidle/stateN/disable` directly. Each run's `idle.txt`
records the configuration, and the `usage` counters of every disabled state were confirmed frozen
across each run.

1. **default** (all states enabled): [results/core-20261029-153912-default](results/core-20261029-153912-default/summary.md)
2. **C1 only** (C2/C3 disabled): [results/core-20261029-194534-c1only](results/core-20261029-194534-c1only/summary.md)
3. **POLL only** (C1/C2/C3 disabled): [results/core-20261029-202517-pollonly](results/core-20261029-202517-pollonly/summary.md)

Findings:

- **Idle states do not explain the Linux regression.** The RC2 deficit vs .NET 10 is the same in
  all three configurations: 5 µs cell −34.3% / −34.0% / −34.6%; 20 µs cell −20.6% / −19.8% / −21.2%;
  78%-hits cell −29.2% / −29.0% / −30.2%. RC1 likewise (5 µs: −42.4% / −40.2% / −38.4%).
- The default run reproduces the 2026-10-15 numbers almost exactly (.NET 10 at 1,029k ops/s and
  11.6 busy in the 5 µs cell, RC2 at 677k and 7.9), so the original results were a default-idle
  baseline in practice.
- Unlike Windows, where `IDLEDISABLE` gave .NET 10 +10% (5 µs) and +29% (1 caller), on Linux
  disabling C-states never helps: C1-only is a wash everywhere, and POLL-only *costs* ~18% at
  1 caller for every runtime (.NET 10 55.8k → 45.6k, RC2 52.8k → 43.7k) while leaving the
  64-caller cells within noise. .NET 10 already keeps ~11.5 of 12 cores busy in the work cells
  under every configuration, so there is no parked-thread wake latency for C-states to add.

Posted [here](https://github.com/dotnet/runtime/issues/135855#issuecomment-6509023761), as a
follow-up to the Windows comment.
