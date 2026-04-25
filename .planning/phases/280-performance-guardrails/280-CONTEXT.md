# Phase 280: Performance Guardrails - Context

**Gathered:** 2026-04-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Harden authoring-interaction latency budgets and proof markers; keep 5000-node stress tier as informational telemetry only.

Success criteria:
1. Authoring-interaction latency budgets (inspector open, node resize, edge create, command palette) have defended proof markers that fail loudly on regression.
2. The 1000-node `large` ScaleSmoke tier remains a defended budget with an explicit pass/fail contract in release validation.
3. The 5000-node `stress` tier publishes p50/p95 telemetry in prerelease proof summaries but is NOT marketed as a defended public claim.

</domain>

<decisions>
## Implementation Decisions

### ScaleSmoke Authoring Metrics
- Extend `ScaleSmokeAuthoringMetrics` with `InspectorOpenMs`, `NodeResizeMs`, `EdgeCreateMs`
- Update budget tiers (baseline: 50/30/50ms, large: 100/60/100ms) with redlines for new metrics
- Wire Stopwatch probes in `Program.cs` for inspector open, resize, edge create

### ConsumerSample Proof Markers
- Add `CommandPaletteMs` to `ConsumerSampleProofResult` as a `HOST_NATIVE_METRIC`
- Measure palette open/close round-trip in headless environment
- Keep metric as telemetry (not a hard pass/fail gate in ConsumerSample) — the defended gate is ScaleSmoke

### CI Release Validation
- Add `SCALE_AUTHORING_BUDGET_OK` markers to prerelease notes required-text check
- baseline/large must show `Passed=True` with empty failure summary
- stress must show `Passed=True:informational-only` (no enforced budgets)

### Stress Tier Status
- `ScaleSmokeTier.Parse("stress")` returns `null` budgets → `EnforceBudgets=false`
- Stress tier outputs telemetry summaries but never throws on budget overrun
- CI prerelease notes validate `SCALE_PERFORMANCE_BUDGET_OK:stress:True:informational-only`

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ScaleSmokeAuthoringProbe` — existing stencil/command-surface/quick-tool probe runner
- `ConsumerSampleProofResult` / `ConsumerSampleProof.Run()` — canonical host proof harness
- `eng/ci.ps1` — release validation with prerelease notes required-text checks

### Established Patterns
- `Stopwatch` measurements wrapped in `MeasureMilliseconds(Action)`
- Proof markers follow `KEY:True/False` or `HOST_NATIVE_METRIC:name=value` format
- Budget evaluation returns `(bool Passed, string FailureSummary)` — empty summary means pass

### Integration Points
- `SCALE_AUTHORING_BUDGET_OK` is emitted by `tools/AsterGraph.ScaleSmoke/Program.cs`
- `HOST_NATIVE_METRIC:*` is consumed by prerelease notes and support bundles
- CI `Invoke-ScaleSmoke` runs baseline → large → stress sequentially

</code_context>

<specifics>
## Specific Ideas

- Keep command palette measurement in ConsumerSample only (telemetry), not in ScaleSmoke authoring probe — the palette is a host-side concern, not a kernel throughput concern.
- Do not add `CommandPaletteMs` to `ScaleSmokeAuthoringMetrics` — the 7-metric shape is frozen for this milestone.

</specifics>

<deferred>
## Deferred Ideas

- Deeper command palette latency budgets in ScaleSmoke (requires host-level UI automation)
- Frame-time budgets for resize drag (currently only a trace warning at 16ms)
- GPU/render thread telemetry separation

</deferred>
