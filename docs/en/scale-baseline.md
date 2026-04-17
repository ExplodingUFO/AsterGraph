# ScaleSmoke Baseline

`tools/AsterGraph.ScaleSmoke` is the public larger-graph confidence harness for AsterGraph.

It is now used in two ways:

- as a release-lane proof that history/save/state behavior still holds
- as a public baseline for defended graph-size and timing expectations

## Tiers

| Tier | Nodes | Selection batch | Move batch | Intended use |
| --- | ---: | ---: | ---: | --- |
| `baseline` | 180 | 48 | 24 | defended release-lane redline |
| `large` | 1000 | 128 | 64 | public informational telemetry |
| `stress` | 5000 | 256 | 96 | manual stretch telemetry |

## Scenarios

Each run measures the same scenario set:

- editor/session setup on a repeatable generated graph
- selection on a representative batch of nodes
- delete/reconnect against the canonical connection path
- history interaction plus save/undo/redo dirty semantics
- viewport resize plus fit-to-viewport navigation
- workspace save and reload of the saved document

The harness still emits the existing correctness markers such as `SCALE_HISTORY_CONTRACT_OK`, `PHASE25_SCALE_AUTOMATION_OK`, and `PHASE18_SCALE_READINESS_OK`.

## Defended Redlines

The current release lane defends the `baseline` tier with these redlines:

| Metric | Baseline redline |
| --- | ---: |
| setup | 1500 ms |
| selection | 150 ms |
| connection | 150 ms |
| history interaction | 400 ms |
| viewport / fit | 150 ms |
| save | 150 ms |
| reload | 1200 ms |

If the baseline tier exceeds one of those numbers, `ScaleSmoke` emits `SCALE_PERFORMANCE_BUDGET_OK:baseline:False:...` and the release gate fails.

## Informational Metrics

The `large` and `stress` tiers are informative only. They do not currently fail CI, but they give maintainers and adopters a repeatable way to inspect how the toolkit behaves on denser graphs.

## Commands

```powershell
# defended release-lane baseline
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier baseline

# larger informational run
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier large

# manual stretch run
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier stress
```

## Reading The Output

High-signal lines:

- `SCALE_TIER_INFO:...`
- `SCALE_PERF_METRICS:...`
- `SCALE_PERFORMANCE_BUDGET_OK:...`
- `SCALE_HISTORY_CONTRACT_OK:...`

Treat the budget marker as the defended release signal. Treat the larger-tier metric line as public telemetry until the alpha line is ready to commit to stronger scale guarantees.
