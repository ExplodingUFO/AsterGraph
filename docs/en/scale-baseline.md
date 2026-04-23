# ScaleSmoke Baseline

`tools/AsterGraph.ScaleSmoke` is the public larger-graph confidence harness for AsterGraph.

It is now used in two ways:

- as a release-lane proof that history/save/state behavior still holds
- as a public baseline for defended graph-size and timing expectations

## Tiers

| Tier | Nodes | Selection batch | Move batch | Intended use |
| --- | ---: | ---: | ---: | --- |
| `baseline` | 180 | 48 | 24 | defended release-lane redline |
| `large` | 1000 | 128 | 64 | defended release-lane large-graph budget |
| `stress` | 5000 | 256 | 96 | published informational telemetry |

## Scenarios

Each run measures the same scenario set:

- editor/session setup on a repeatable generated graph
- selection on a representative batch of nodes
- delete/reconnect against the canonical connection path
- history interaction plus save/undo/redo dirty semantics
- authoring probe coverage for stencil filtering, command-surface refresh, and quick-tool projection/execution
- viewport resize plus fit-to-viewport navigation
- workspace save and reload of the saved document
- svg/png/jpeg export plus reload on the same saved document path

The harness still emits the existing correctness markers such as `SCALE_HISTORY_CONTRACT_OK`, `PHASE25_SCALE_AUTOMATION_OK`, and `PHASE18_SCALE_READINESS_OK`.

## Defended Redlines

The current release lane defends the `baseline` and `large` tiers with these redlines:

These redlines are intentionally conservative and are validated on the current GitHub-hosted Windows runner class used by the public release lane, not on an optimized local workstation.

### Baseline redlines

| Metric | Baseline redline |
| --- | ---: |
| setup | 1500 ms |
| selection | 500 ms |
| connection | 150 ms |
| history interaction | 400 ms |
| viewport / fit | 150 ms |
| save | 150 ms |
| reload | 1200 ms |

### Large redlines

| Metric | Large redline |
| --- | ---: |
| setup | 2500 ms |
| selection | 750 ms |
| connection | 350 ms |
| history interaction | 800 ms |
| viewport / fit | 200 ms |
| save | 300 ms |
| reload | 1500 ms |

If either defended tier exceeds one of those numbers, `ScaleSmoke` emits `SCALE_PERFORMANCE_BUDGET_OK:<tier>:False:...` and the release gate fails.

### Authoring redlines

| Tier | stencil | command-surface | quick-tool-projection | quick-tool-execution |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 100 ms | 250 ms | 100 ms | 150 ms |
| `large` | 150 ms | 400 ms | 150 ms | 200 ms |

`ScaleSmoke` emits `SCALE_AUTHORING_BUDGET:...`, `SCALE_AUTHORING_METRICS:...`, `SCALE_AUTHORING_BUDGET_OK:...`, and `SCALE_AUTHORING_SUMMARY:...` for these defended tiers.

### Export redlines

| Tier | svg | png | jpeg | reload |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 300 ms | 2500 ms | 1800 ms | 250 ms |
| `large` | 300 ms | 12000 ms | 8000 ms | 400 ms |

`ScaleSmoke` emits `SCALE_EXPORT_BUDGET:...`, `SCALE_EXPORT_METRICS:...`, `SCALE_EXPORT_BUDGET_OK:...`, and `SCALE_EXPORT_SUMMARY:...` for these defended tiers.

Pair the hosted tuning handoff with [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) when you want the `ConsumerSample.Avalonia` metrics and the defended `ScaleSmoke` budgets on one copyable route.

## Informational Telemetry

The `stress` tier remains informational. It now publishes a `SCALE_PERF_SUMMARY:stress:...` line with p50/p95 timings, but it does not define a defended release budget yet.

## Commands

```powershell
# defended release-lane baseline
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier baseline

# defended large-graph release budget
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier large

# published informational stress telemetry
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier stress --samples 3
```

## Reading The Output

High-signal lines:

- `SCALE_TIER_INFO:...`
- `SCALE_TIER_BUDGET:...`
- `SCALE_PERF_METRICS:...`
- `SCALE_PERFORMANCE_BUDGET_OK:...`
- `SCALE_PERF_SUMMARY:...`
- `SCALE_AUTHORING_BUDGET:...`
- `SCALE_AUTHORING_METRICS:...`
- `SCALE_AUTHORING_BUDGET_OK:...`
- `SCALE_AUTHORING_SUMMARY:...`
- `SCALE_EXPORT_BUDGET:...`
- `SCALE_EXPORT_METRICS:...`
- `SCALE_EXPORT_BUDGET_OK:...`
- `SCALE_EXPORT_SUMMARY:...`
- `SCALE_HISTORY_CONTRACT_OK:...`

`SCALE_TIER_BUDGET` is the machine-readable declaration of the defended tier, scenario sizes, and threshold policy for the current run. Release notes and proof summaries can quote that marker directly without re-parsing the table above.

Treat `SCALE_PERFORMANCE_BUDGET_OK` as the defended release signal for `baseline` and `large`.

Treat `SCALE_AUTHORING_BUDGET_OK` and `SCALE_EXPORT_BUDGET_OK` as the defended widened-surface performance signals for `baseline` and `large`.

Treat `SCALE_PERF_SUMMARY:stress:...` as the published informational telemetry line for the 5000-node tier. It gives hosts a stable p50/p95 snapshot without turning `stress` into a defended release contract.

If AsterGraph later makes a stronger 5000-node commitment, that commitment should appear as a non-informational `SCALE_TIER_BUDGET:stress:...` marker plus a defended `SCALE_PERFORMANCE_BUDGET_OK:stress:True:none` release signal, or as a new defended tier with its own budget marker.
