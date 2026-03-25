---
phase: 01-consumption-compatibility-guardrails
plan: 03
subsystem: testing
tags: [migration, compatibility, avalonia, editor, package-smoke]
requires:
  - phase: 01-02
    provides: factory-first editor and Avalonia composition APIs
provides:
  - Compatibility-facing XML docs on the retained constructor and view entry paths
  - Legacy-versus-factory migration parity regression coverage
  - Package smoke markers for both staged migration routes
affects: [01-04, host-guidance, package-consumption]
tech-stack:
  added: []
  patterns: [compatibility facade guidance, legacy-vs-factory parity regression, staged migration smoke markers]
key-files:
  created:
    - .planning/phases/01-consumption-compatibility-guardrails/01-03-SUMMARY.md
  modified:
    - src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs
    - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
    - tools/AsterGraph.PackageSmoke/Program.cs
    - .planning/STATE.md
    - .planning/ROADMAP.md
key-decisions:
  - "Keep the retained GraphEditorViewModel constructor and GraphEditorView Editor-assignment path documented as supported compatibility facades instead of adding obsoletions in Phase 1."
  - "Use smoke markers for legacy editor/view creation and factory editor/view creation so migration-stage package validation is machine-checkable."
patterns-established:
  - "Migration parity assertions compare legacy and factory-created editors through the same host-facing snapshot rather than duplicating implementation details."
  - "Compatibility smoke validates both partial-migration combinations and full factory adoption without depending on demo-only assets."
requirements-completed: [PKG-03]
duration: 33min
completed: 2026-03-26
---

# Phase 01 Plan 03: Consumption & Compatibility Guardrails Summary

**Compatibility-facade docs plus legacy-versus-factory parity coverage for staged AsterGraph host migration**

## Performance

- **Duration:** 33 min
- **Started:** 2026-03-26T00:17:00+08:00
- **Completed:** 2026-03-26T00:49:54+08:00
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments

- Added IDE-visible migration guidance to the retained `GraphEditorViewModel` constructor and `GraphEditorView` entry path so existing hosts have an explicit Phase 1 compatibility contract.
- Replaced the skipped migration placeholder tests with concrete legacy-versus-factory parity coverage for host seams, view binding, and staged migration combinations.
- Extended `tools/AsterGraph.PackageSmoke` to construct both legacy and factory editors/views at runtime and print unambiguous success markers for each path.

## Task Commits

Each task was committed atomically:

1. **Task 1: Document the retained constructor and view paths as the Phase 1 compatibility facade** - `b421d4a` (docs)
2. **Task 2: Add parity regression and smoke coverage for staged migration** - `8c49b94` (test)

**Plan metadata:** pending

## Files Created/Modified

- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` - Added Phase 1 compatibility-facade XML remarks to the retained public constructor path.
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` - Added Phase 1 compatibility-facade XML remarks to the retained view and `Editor` assignment path.
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` - Added real parity assertions for legacy and factory editor/view composition.
- `tools/AsterGraph.PackageSmoke/Program.cs` - Added runtime smoke coverage and explicit markers for legacy/factory migration stages.
- `.planning/phases/01-consumption-compatibility-guardrails/01-03-SUMMARY.md` - Recorded plan execution, verification evidence, and blockers.
- `.planning/STATE.md` - Updated plan position, metrics, and accumulated context after execution.
- `.planning/ROADMAP.md` - Updated Phase 1 plan progress.

## Decisions Made

- Kept the retained constructor and view path documentation additive only; Phase 1 still guides new hosts toward the factories without warning existing hosts off the compatibility path.
- Captured migration parity through shared observable snapshots so the tests enforce behavior equivalence instead of mirroring implementation branches line by line.
- Made the smoke output marker-based (`LEGACY_*` / `FACTORY_*`) so later doc and package verification can assert the migration stages directly.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Replaced nonexistent `ViewBehaviorOptions.ZoomStep` usage in new migration coverage**
- **Found during:** Task 2 (Add parity regression and smoke coverage for staged migration)
- **Issue:** The first draft of the new migration test/smoke code referenced `ViewBehaviorOptions.ZoomStep`, but the real public surface only exposes `ShowMiniMap`.
- **Fix:** Switched the parity setup and snapshot assertions to the real `ShowMiniMap` option.
- **Files modified:** `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`, `tools/AsterGraph.PackageSmoke/Program.cs`
- **Verification:** `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`
- **Committed in:** `8c49b94`

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** The auto-fix corrected the regression harness to target the real public API surface. No scope creep.

## Issues Encountered

- The required targeted test command `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal` is still blocked by the pre-existing out-of-scope untracked file `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`, which fails to resolve `GraphEditorViewTestsAppBuilder`.
- A command-line attempt to exclude that out-of-scope file for local verification surfaced duplicate assembly attributes from `src/AsterGraph.Abstractions/artifacts/audit/...`; that workaround was not used as success evidence because it changes the evaluation surface.
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` already contained broader uncommitted worktree changes when this plan started, so the task-1 documentation landed on top of the existing local view delta rather than resetting the file.

## Verification Evidence

- `dotnet build avalonia-node-map.sln -v minimal` ❌ blocked by `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal` ❌ blocked by `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` ✅
  - `LEGACY_EDITOR_OK:Package Smoke Graph:Smoke stats 1/0/88:Smoke subtitle:#F3B36B`
  - `LEGACY_VIEW_OK:CanvasOnly:Package Smoke Graph`
  - `FACTORY_EDITOR_OK:Package Smoke Graph:Smoke stats 1/0/88:Smoke subtitle:#F3B36B`
  - `FACTORY_VIEW_OK:CanvasOnly:Package Smoke Graph`

## User Setup Required

None.

## Next Phase Readiness

- Plan `01-04` can reference concrete migration markers and compatibility-facade docs when aligning README and host guidance.
- The package smoke now proves both migration stages against the publishable package set in the current workspace.
- Clean targeted editor-test verification still requires resolving the out-of-scope `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` compile blocker separately.

## Self-Check
PASSED

- FOUND: `.planning/phases/01-consumption-compatibility-guardrails/01-03-SUMMARY.md`
- FOUND: `b421d4a`
- FOUND: `8c49b94`
