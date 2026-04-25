# Phase 280: Performance Guardrails - Plan

**Phase:** 280
**Goal:** Harden authoring-interaction latency budgets and proof markers; keep 5000-node stress tier as informational telemetry only.

## Tasks

### Task 1: Extend ScaleSmoke authoring metrics (completed in prior commit)
- Add `InspectorOpenMs`, `NodeResizeMs`, `EdgeCreateMs` to `ScaleSmokeAuthoringMetrics`
- Update `ScaleSmokeAuthoringBudget` and `ScaleSmokeAuthoringMetricSummary` to include new fields
- Update baseline/large tier redlines
- Wire probes in `Program.cs`
- Fix all test constructors and expected marker strings

### Task 2: Add command palette metric to ConsumerSampleProof
- Add `CommandPaletteMs` parameter to `ConsumerSampleProofResult`
- Add `MeasureCommandPaletteMilliseconds` method (open palette → flush → close palette → flush)
- Add `command_palette_ms` to `MetricLines`
- Update all `ConsumerSampleProofTests` constructors and assertions

### Task 3: Harden CI prerelease notes validation
- Add `SCALE_AUTHORING_BUDGET_OK:baseline:True:` to required-text checks
- Add `SCALE_AUTHORING_BUDGET_OK:large:True:` to required-text checks
- Add `SCALE_AUTHORING_BUDGET_OK:stress:True:informational-only` to required-text checks

### Task 4: Verify full test suite
- Run `dotnet test AsterGraph.sln --no-build`
- Confirm all 7 test projects pass

## Verification Criteria

- [x] `ScaleSmokeAuthoringMetrics` includes 7 fields (4 original + 3 new)
- [x] `SCALE_AUTHORING_BUDGET_OK` markers appear in ScaleSmoke output for all tiers
- [x] ConsumerSampleProof emits `HOST_NATIVE_METRIC:command_palette_ms=`
- [x] CI prerelease notes validation enforces authoring budget markers
- [x] All 789 tests pass across 7 test projects
- [x] Stress tier remains informational-only (null budgets, no exceptions on overrun)
