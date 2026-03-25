---
phase: 01-consumption-compatibility-guardrails
plan: 01
subsystem: testing
tags: [msbuild, xunit, avalonia, compatibility, migration]
requires: []
provides:
  - Repository-wide default item exclusion for generated project-local artifacts trees
  - Explicit GraphEditor initialization regression suite entry points
  - Explicit GraphEditor staged-migration regression suite entry points
affects: [01-02, 01-03, validation]
tech-stack:
  added: []
  patterns: [DefaultItemExcludes guardrails, explicit skipped regression suite placeholders]
key-files:
  created:
    - .planning/phases/01-consumption-compatibility-guardrails/01-01-SUMMARY.md
    - tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
  modified:
    - Directory.Build.props
    - .planning/STATE.md
    - .planning/ROADMAP.md
key-decisions:
  - "Use DefaultItemExcludes in Directory.Build.props so project-local artifacts trees stay out of SDK default compile globs."
  - "Represent Phase 1 initialization and migration contracts as explicit skipped xUnit cases with stable method names for later implementation plans."
patterns-established:
  - "Build guardrails for generated audit outputs belong in shared MSBuild defaults, not per-project cleanup."
  - "Future public API phases should implement against named regression entry points instead of inventing new test files."
requirements-completed: [PKG-01, PKG-02, PKG-03]
duration: 5min
completed: 2026-03-25
---

# Phase 01 Plan 01: Consumption & Compatibility Guardrails Summary

**MSBuild artifact exclusion plus named initialization and migration regression suites for the Phase 1 public-consumption baseline**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-25T16:17:41Z
- **Completed:** 2026-03-25T16:22:35Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- Excluded generated `artifacts/**` sources from normal SDK default compile globs through shared MSBuild defaults.
- Added `GraphEditorInitializationTests` with explicit entry points for future factory/options and registration coverage.
- Added `GraphEditorMigrationCompatibilityTests` with explicit staged-migration parity cases and deterministic skip messages.

## Task Commits

Each task was committed atomically:

1. **Task 1: Remove generated audit outputs from default compile items** - `85776c1` (fix)
2. **Task 2: Create explicit initialization and migration regression suites** - `ed83e34` (test)

**Plan metadata:** pending

## Files Created/Modified
- `Directory.Build.props` - Adds the repository-wide `DefaultItemExcludes` guardrail for project-local `artifacts/**` trees.
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - Declares explicit skipped initialization/factory/registration regression entry points.
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` - Declares explicit skipped migration-compatibility regression entry points.
- `.planning/phases/01-consumption-compatibility-guardrails/01-01-SUMMARY.md` - Records execution, verification evidence, and blockers.
- `.planning/STATE.md` - Updated after plan execution.
- `.planning/ROADMAP.md` - Updated with Phase 1 plan progress.

## Decisions Made
- Used `DefaultItemExcludes` instead of a later `Compile Remove` item mutation because the shared SDK default glob had to be blocked before compile item creation.
- Kept the new regression suites compile-safe and intentionally skipped so Plans `01-02` and `01-03` can fill in behavior against fixed test names without changing file topology.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- The requested `dotnet build avalonia-node-map.sln -v minimal` no longer fails on `CS0579` duplicate assembly attributes from `src/**/artifacts/**`, but the current working tree still fails the editor test project because the pre-existing untracked file `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` references `GraphEditorViewTestsAppBuilder` from assembly scope without namespace qualification.
- The requested targeted test command for the new suites is blocked by that same out-of-scope `GraphEditorViewTests.cs` compile error. As a supplemental check, the two new suite files were compiled successfully with `csc` plus a temporary local `FactAttribute` stub to verify the new files themselves are syntactically valid.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Plan `01-02` can implement public initialization APIs directly against `GraphEditorInitializationTests`.
- Plan `01-03` can implement compatibility and staged migration behavior directly against `GraphEditorMigrationCompatibilityTests`.
- A clean full `dotnet test avalonia-node-map.sln -v minimal` run in this workspace still requires resolving the existing `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` compile issue outside this plan's allowed write scope.

## Self-Check
PASSED

- FOUND: `.planning/phases/01-consumption-compatibility-guardrails/01-01-SUMMARY.md`
- FOUND: `85776c1`
- FOUND: `ed83e34`

---
*Phase: 01-consumption-compatibility-guardrails*
*Completed: 2026-03-25*
