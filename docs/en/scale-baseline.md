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
| `stress` | 5000 | 256 | 96 | defended 5000-node gate with conservative raster export redlines |
| `xlarge` | 10000 | 512 | 128 | telemetry-only probe; not a support claim |

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

The current release lane defends the `baseline`, `large`, and the promoted non-raster portions of `stress` with these redlines:

These redlines are intentionally conservative and are validated on the current GitHub-hosted Windows runner class used by the public release lane, not on an optimized local workstation.

### Baseline redlines

| Metric | Baseline redline |
| --- | ---: |
| setup | 1500 ms |
| selection | 500 ms |
| connection | 150 ms |
| history interaction | 1500 ms |
| viewport / fit | 150 ms |
| save | 1300 ms |
| reload | 1200 ms |

### Large redlines

| Metric | Large redline |
| --- | ---: |
| setup | 2500 ms |
| selection | 750 ms |
| connection | 450 ms |
| history interaction | 1000 ms |
| viewport / fit | 200 ms |
| save | 300 ms |
| reload | 1500 ms |

### Stress performance redlines

| Metric | Stress redline |
| --- | ---: |
| setup | 1500 ms |
| selection | 200 ms |
| connection | 2500 ms |
| history interaction | 2500 ms |
| viewport / fit | 100 ms |
| save | 700 ms |
| reload | 500 ms |

If any defended metric exceeds one of those numbers, `ScaleSmoke` emits `SCALE_PERFORMANCE_BUDGET_OK:<tier>:False:...` and the release gate fails.

### Authoring redlines

| Tier | stencil | command-surface | quick-tool-projection | quick-tool-execution | inspector-open | node-resize | edge-create |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `baseline` | 100 ms | 250 ms | 100 ms | 150 ms | 50 ms | 30 ms | 50 ms |
| `large` | 150 ms | 400 ms | 250 ms | 300 ms | 100 ms | 60 ms | 100 ms |
| `stress` | 150 ms | 900 ms | 1000 ms | 1200 ms | 100 ms | 200 ms | 350 ms |

`ScaleSmoke` emits `SCALE_AUTHORING_BUDGET:...`, `SCALE_AUTHORING_METRICS:...`, `SCALE_AUTHORING_BUDGET_OK:...`, and `SCALE_AUTHORING_SUMMARY:...` for these defended tiers.

### Export redlines

| Tier | svg | png | jpeg | reload |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 300 ms | 2500 ms | 3500 ms | 250 ms |
| `large` | 600 ms | 16000 ms | 12000 ms | 400 ms |
| `stress` | telemetry | 120000 ms | 100000 ms | 800 ms |

`ScaleSmoke` emits `SCALE_EXPORT_BUDGET:...`, `SCALE_EXPORT_METRICS:...`, `SCALE_EXPORT_BUDGET_OK:...`, `EXPORT_PROGRESS_OK:...`, `EXPORT_CANCEL_OK:...`, `EXPORT_SCOPE_OK:...`, `EXPORT_SELECTION_SCOPE_OK:...`, and `SCALE_EXPORT_SUMMARY:...` for these defended tiers.

Pair the hosted tuning handoff with [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) when you want the `ConsumerSample.Avalonia` metrics and the defended `ScaleSmoke` budgets on one copyable route.

## Stress Raster Export

The `stress` tier now defends performance, authoring, PNG/JPEG raster export, and export reload. Stress SVG export remains telemetry-only because its 5000-node serialized scene size is environment-sensitive; the raster redlines are intentionally conservative so release validation can fail on pathological regressions without claiming that 5000-node raster export is fast.

## Commands

```powershell
# defended release-lane baseline
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier baseline

# defended large-graph release budget
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier large

# defended 5000-node stress gate with conservative raster redlines
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier stress --samples 3

# telemetry-only 10000-node probe; not part of the release gate
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -- --tier xlarge --samples 1
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
- `EXPORT_PROGRESS_OK:...`
- `EXPORT_CANCEL_OK:...`
- `EXPORT_SCOPE_OK:...`
- `EXPORT_SELECTION_SCOPE_OK:...`
- `SCALE_EXPORT_METRICS:...`
- `SCALE_EXPORT_BUDGET_OK:...`
- `SCALE_EXPORT_SUMMARY:...`
- `SCALE_HISTORY_CONTRACT_OK:...`

`SCALE_TIER_BUDGET` is the machine-readable declaration of the defended tier, scenario sizes, and threshold policy for the current run. Release notes and proof summaries can quote that marker directly without re-parsing the table above.

Treat `SCALE_PERFORMANCE_BUDGET_OK` as the defended release signal for `baseline`, `large`, and promoted `stress` performance metrics.

Treat `SCALE_AUTHORING_BUDGET_OK` as the defended authoring signal for all three tiers.

Treat `SCALE_EXPORT_BUDGET:stress:svg=informational:png<=120000:jpeg<=100000:reload<=800` and `SCALE_RASTER_EXPORT_STRESS_OK:True` as the boundary for the 5000-node export story: raster export is defended by conservative redlines, not advertised as fast.

For `xlarge`, `SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only`, `SCALE_AUTHORING_BUDGET:xlarge:budget=informational-only`, and `SCALE_EXPORT_BUDGET:xlarge:budget=informational-only` are telemetry markers only.

Do not read these markers as a 10000-node claim or a general virtualization commitment. Faster 5000-node raster commitments need their own tighter thresholds and repeated proof.
