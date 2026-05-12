# Scale Baseline

`release validation lane` is the public larger-graph confidence harness for AsterGraph.

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

If any defended metric exceeds one of those numbers, `Scale Baseline` emits `SCALE_PERFORMANCE_BUDGET_OK:<tier>:False:...` and the release gate fails.

### Authoring redlines

| Tier | stencil | command-surface | quick-tool-projection | quick-tool-execution | inspector-open | node-resize | edge-create |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `baseline` | 100 ms | 250 ms | 100 ms | 150 ms | 50 ms | 30 ms | 50 ms |
| `large` | 150 ms | 400 ms | 250 ms | 300 ms | 100 ms | 60 ms | 100 ms |
| `stress` | 150 ms | 900 ms | 1000 ms | 1200 ms | 100 ms | 200 ms | 350 ms |

`Scale Baseline` emits `SCALE_AUTHORING_BUDGET:...`, `SCALE_AUTHORING_METRICS:...`, `SCALE_AUTHORING_BUDGET_OK:...`, and `SCALE_AUTHORING_SUMMARY:...` for these defended tiers.

### Export redlines

| Tier | svg | png | jpeg | reload |
| --- | ---: | ---: | ---: | ---: |
| `baseline` | 300 ms | 2500 ms | 3500 ms | 250 ms |
| `large` | 600 ms | 16000 ms | 12000 ms | 400 ms |
| `stress` | telemetry | 120000 ms | 100000 ms | 800 ms |

`Scale Baseline` emits `SCALE_EXPORT_BUDGET:...`, `SCALE_EXPORT_METRICS:...`, `SCALE_EXPORT_BUDGET_OK:...`, `EXPORT_PROGRESS_OK:...`, `EXPORT_CANCEL_OK:...`, `EXPORT_SCOPE_OK:...`, `EXPORT_SELECTION_SCOPE_OK:...`, and `SCALE_EXPORT_SUMMARY:...` for these defended tiers.

Pair the hosted tuning handoff with [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) when you want the `src/AsterGraph.Demo` metrics and the defended `Scale Baseline` budgets on one copyable route.

## Stress Raster Export

The `stress` tier now defends performance, authoring, PNG/JPEG raster export, and export reload. Stress SVG export remains telemetry-only because its 5000-node serialized scene size is environment-sensitive; the raster redlines are intentionally conservative so release validation can fail on pathological regressions without claiming that 5000-node raster export is fast.

## Rendering Virtualization Boundary

AsterGraph currently defends a viewport-budgeted scene projection/rendering contract, not a renderer virtualization contract. The tested path is:

- `ViewportVisibleSceneProjector.Project(...)` computes visible node, group, and connection IDs from the current viewport plus overscan.
- `NodeCanvasSceneHost.RebuildScene()` uses that budget to materialize visible node and group visuals when viewport size and zoom are available.
- `NodeCanvasConnectionSceneRenderer.RenderConnections(...)` applies the same budget to committed connection routes while preserving the pending connection preview.

This path still scans current document/session collections and rebuilds visible visuals during scene rebuilds. Do not market it as ItemsRepeater/Skia-style incremental renderer virtualization, a background graph index, or a new graph-size support tier. If a future release wants that claim, add renderer thresholds, repeatable proof commands, and focused renderer tests before changing this boundary.

## Renderer Virtualization Proof Contract

Phase 489 defines the minimum renderer virtualization contract before any future issue can promote the current viewport-budgeted scene projection/rendering boundary into an ItemsRepeater/Skia-style incremental renderer virtualization claim. A follow-up implementation issue must provide:

- renderer thresholds that are non-informational and tied to a named defended tier;
- a repeatable proof command that produces stable pass/fail output on the release runner class;
- focused renderer tests that cover incremental visual lifecycle, invalidation evidence, and connection preview preservation;
- artifact metadata for the graph size, viewport, zoom, overscan, visual counts, invalidation counts, and measured timings;
- evidence that the renderer does not depend on a full collection scan or full scene rebuild for the claimed operation;
- a clear boundary for any background graph index, including freshness, cancellation, and invalidation behavior.

Until those items exist, `xlarge` remains telemetry-only. Its 10000-node markers are not renderer virtualization evidence, not a background graph index proof, and not a support-tier expansion.

Phase 499 records the execution boundary for that future proof. This slice is docs/tests-only and does not authorize a benchmark harness implementation, renderer rewrite, public API change, runtime behavior change, UI change, or support-claim expansion. The first executable proof must still be created by a separate implementation issue and must produce CI-repeatable evidence for all of these surfaces before the public claim changes: non-informational renderer thresholds, repeatable proof command output, artifact metadata, incremental visual lifecycle evidence, invalidation evidence, connection preview preservation, and proof that the claimed operation avoids full collection scans and full scene rebuilds.

Phase 502 is GitHub #127 / `avalonia-node-map-mai`, the renderer virtualization execution proof contract selected after the Phase 501 queue refresh. This remains a docs/tests-only contract slice: it names the executable proof shape future implementation work must satisfy, but it does not implement renderer virtualization, does not defend non-informational renderer thresholds yet, and makes no support-claim expansion.

The Phase 502 proof contract reserves these machine-readable markers for the future proof artifact:

- `RENDERER_VIRTUALIZATION_PROOF_CONTRACT:phase=502:scope=contract-only:no-support-claim-expansion`
- `RENDERER_VIRTUALIZATION_PROOF_COMMAND:phase=502:command=dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --configuration Release --filter FullyQualifiedName~RendererVirtualizationProof`
- `RENDERER_VIRTUALIZATION_ARTIFACT_METADATA:phase=502:fields=graphSize,viewport,zoom,overscan,visibleVisualCounts,invalidationCounts,measuredTimings`

Any future implementation PR that claims true renderer virtualization must write artifact metadata with `graphSize`, `viewport`, `zoom`, `overscan`, `visibleVisualCounts`, `invalidationCounts`, and `measuredTimings`; pair those artifacts with focused renderer tests; define non-informational renderer thresholds; and prove the claimed operation avoids both a full collection scan and a full scene rebuild. Until that proof exists, the current public statement stays viewport-budgeted scene projection/rendering only, `xlarge` remains telemetry-only, and Phase 502 records no support-claim expansion.

Phase 514 is GitHub #150 / `avalonia-node-map-ien`, the first executable renderer virtualization proof harness. It adds `RendererVirtualizationProofHarnessTests` and the machine-readable `RENDERER_VIRTUALIZATION_PROOF_ARTIFACT` marker with `graphSize`, `viewport`, `zoom`, `overscan`, `visibleVisualCounts`, `invalidationCounts`, and `measuredTimings`. The artifact records `avoidsFullCollectionScan=false` and `avoidsFullSceneRebuild=false`, so the public claim remains viewport-budgeted scene projection/rendering with no support-claim expansion.

## Commands

```powershell
# defended release-lane baseline
.\eng\ci.ps1 -Lane release -Framework all -Configuration Release

# defended large-graph release budget
.\eng\ci.ps1 -Lane release -Framework all -Configuration Release

# defended 5000-node stress gate with conservative raster redlines
.\eng\ci.ps1 -Lane release -Framework all -Configuration Release

# telemetry-only 10000-node probe; not part of the release gate
.\eng\ci.ps1 -Lane release -Framework all -Configuration Release
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

Do not read these markers as a 10000-node claim, renderer virtualization evidence, or a general virtualization commitment. Faster 5000-node raster commitments need their own tighter thresholds and repeated proof.
