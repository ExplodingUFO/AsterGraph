# Phase 280: Performance Guardrails - Summary

**Completed:** 2026-04-25

## Delivered

- Extended `ScaleSmokeAuthoringMetrics` with `InspectorOpenMs`, `NodeResizeMs`, `EdgeCreateMs`
- Updated budget tiers (baseline/large) with redlines for new authoring-interaction metrics
- Wired Stopwatch probes in `ScaleSmoke/Program.cs` for inspector open, node resize, edge create
- Fixed all ScaleSmoke test constructors and expected marker strings (21 tests)
- Added `CommandPaletteMs` metric to `ConsumerSampleProof` with round-trip measurement
- Hardened CI prerelease notes validation to check `SCALE_AUTHORING_BUDGET_OK` for all tiers
- Verified stress tier remains informational-only (null budgets, no enforced pass/fail)

## Files Changed

- `tools/AsterGraph.ScaleSmoke/ScaleSmokeBudgeting.cs` — metrics/budget records and tier definitions
- `tools/AsterGraph.ScaleSmoke/ScaleSmokeAuthoringMetricSummary.cs` — summary with new percentile fields
- `tools/AsterGraph.ScaleSmoke/ScaleSmokeAuthoringProbe.cs` — probe constructor update
- `tools/AsterGraph.ScaleSmoke/Program.cs` — Stopwatch probes and budget evaluation
- `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` — command palette metric
- `tests/AsterGraph.ConsumerSample.Tests/ConsumerSampleProofTests.cs` — test updates
- `tests/AsterGraph.ScaleSmoke.Tests/ScaleSmokeAuthoringBudgetTests.cs` — budget marker assertions
- `tests/AsterGraph.ScaleSmoke.Tests/ScaleSmokeAuthoringStatisticsTests.cs` — summary marker assertions
- `eng/ci.ps1` — prerelease notes required-text checks

## Key Decisions

- Command palette latency lives in ConsumerSample (telemetry) only, not in ScaleSmoke authoring probe — palette is host-side UI, not kernel throughput.
- 7-field `ScaleSmokeAuthoringMetrics` shape is frozen for this milestone; no `CommandPaletteMs` added.
- Stress tier explicitly stays informational: `null` budgets, no exceptions on overrun.

## Next Milestone

v0.37.0-beta Authoring Surface Polish is now complete. Ready for milestone audit → complete → cleanup.
