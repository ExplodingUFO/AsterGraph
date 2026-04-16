---
phase: 31-history-and-save-semantic-closure
plan: 02
subsystem: testing
tags: [history, save-boundary, maintenance-gate, canvas]
requires:
  - phase: 31-01
    provides: unified retained mutation authority and focused test support
provides:
  - focused retained history interaction and save-boundary suites
  - explicit node-drag interaction-boundary coverage
  - maintenance lane filter updated to the focused suites
affects: [maintenance-gate, history-regressions, canvas-regressions]
tech-stack:
  added: []
  patterns: [focused semantic suites, narrow maintenance filter]
key-files:
  created:
    - tests/AsterGraph.Editor.Tests/GraphEditorHistoryInteractionTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorSaveBoundaryTests.cs
  modified:
    - tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs
    - tests/AsterGraph.Editor.Tests/NodeCanvasNodeDragCoordinatorTests.cs
    - tests/AsterGraph.Editor.Tests/NodeCanvasPointerInteractionCoordinatorTests.cs
    - eng/ci.ps1
key-decisions:
  - "Move retained history/save coverage out of GraphEditorTransactionTests.cs so failures localize to one semantic area."
  - "Point the maintenance lane at dedicated focused suites instead of the broad transaction file for this surface."
patterns-established:
  - "Retained history/save semantics get their own suites; transaction tests keep runtime batching and session transaction scope."
  - "Canvas drag tests must assert the history interaction boundary explicitly, not only movement."
requirements-completed: [STATE-02]
duration: working session
completed: 2026-04-16
---

# Phase 31 Plan 02: Focused History And Save Suite Split Summary

**Split retained history/save coverage into dedicated suites, kept runtime transaction coverage in `GraphEditorTransactionTests`, and retargeted the maintenance lane to the smaller semantic bundles plus explicit canvas interaction-boundary checks.**

## Accomplishments

- Moved retained history interaction, no-op drag, selection round-trip, and save-boundary semantics into focused suites.
- Left `GraphEditorTransactionTests.cs` scoped to runtime batching and session transaction behavior.
- Added explicit node-drag/pointer-release checks that the retained history interaction boundary commits only at completion.
- Updated `eng/ci.ps1 -Lane maintenance` to use the focused history/save suites instead of the broad transaction bundle.

## Task Commits

Plan work was committed atomically:

1. `2e51843` - `test(31-02): split history and save regression suites`

## Files Created/Modified

- `tests/AsterGraph.Editor.Tests/GraphEditorHistoryInteractionTests.cs` - focused retained interaction coverage.
- `tests/AsterGraph.Editor.Tests/GraphEditorSaveBoundaryTests.cs` - focused save-boundary dirty/undo/redo coverage.
- `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs` - now trimmed back to runtime/session transaction scope.
- `tests/AsterGraph.Editor.Tests/NodeCanvasNodeDragCoordinatorTests.cs` - asserts drag start does not commit undo history yet.
- `tests/AsterGraph.Editor.Tests/NodeCanvasPointerInteractionCoordinatorTests.cs` - asserts pointer release commits the retained history interaction boundary.
- `eng/ci.ps1` - maintenance lane now targets the focused suites.

## Issues Encountered

- The retained history story was previously buried inside one broad transaction file, which made failures harder to localize and encouraged broad maintenance filters.

## Resolutions

- The retained suites are now small and purpose-specific, while runtime transaction batching stayed in the existing transaction file.
- The maintenance lane now exercises the focused semantic suites directly and keeps the same hotspot-oriented posture.

## Next Phase Readiness

- Contributors can now change retained history/save behavior and canvas interaction boundaries without touching one broad test file.
- The focused suite split gives the proof-ring alignment work in Plan 03 a stable semantic base.

## Self-Check: PASSED

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorHistoryInteractionTests|FullyQualifiedName~GraphEditorSaveBoundaryTests|FullyQualifiedName~NodeCanvasNodeDragCoordinatorTests|FullyQualifiedName~NodeCanvasPointerInteractionCoordinatorTests" -v minimal`
- Result: 14 passed, 0 failed
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release`
- Result: maintenance lane completed successfully with 137 passing focused editor tests plus `ScaleSmoke`

---
*Phase: 31-history-and-save-semantic-closure*
*Completed: 2026-04-16*
