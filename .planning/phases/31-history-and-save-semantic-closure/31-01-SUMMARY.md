---
phase: 31-history-and-save-semantic-closure
plan: 01
subsystem: runtime
tags: [history, save-boundary, retained-facade, kernel]
requires: []
provides:
  - unified retained mutation commit path through kernel-owned history/save state
  - save-boundary normalization without adding a new undo step
  - focused mixed-path retained history semantic regression coverage
affects: [history-semantics, retained-facade, kernel-authority]
tech-stack:
  added: []
  patterns: [kernel-owned retained mutation commit, canonical retained snapshot merge]
key-files:
  created:
    - tests/AsterGraph.Editor.Tests/GraphEditorHistorySemanticTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorHistoryTestSupport.cs
  modified:
    - src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs
    - src/AsterGraph.Editor/Kernel/Internal/Workspace/GraphEditorKernelWorkspaceSaveCoordinatorHost.cs
    - src/AsterGraph.Editor/Services/History/GraphEditorHistoryService.cs
    - src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.InteractionSurface.cs
    - src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.PersistenceSurface.cs
    - src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.StateProjection.cs
    - src/AsterGraph.Editor/ViewModels/Internal/GraphEditorViewModelKernelAdapter.cs
key-decisions:
  - "Commit retained drag/save completions through kernel-owned history/save state instead of a local retained fallback."
  - "Merge retained snapshots against the current kernel document so canonical runtime shape stays stable across save/undo/redo."
patterns-established:
  - "Retained live interaction can stay local during preview, but completion/save must commit once through kernel authority."
  - "Successful save replaces the current history entry instead of pushing a new undo step."
requirements-completed: [STATE-01]
duration: working session
completed: 2026-04-16
---

# Phase 31 Plan 01: History And Save Authority Convergence Summary

**Closed the mixed retained-versus-kernel history split by routing retained mutation completion and retained save through the kernel-owned history/save path, then locked the behavior with a focused mixed-path regression.**

## Accomplishments

- Added `GraphEditorHistorySemanticTests` to reproduce the real mixed-path case: runtime/kernel mutation first, then retained drag/save/undo/redo.
- Changed retained `CompleteHistoryInteraction(...)`, retained `MarkDirty(...)`, and retained `SaveWorkspace()` to commit through kernel-owned history/save authority.
- Aligned `CanUndo` and `CanRedo` with the same session host authority that `Undo()` and `Redo()` execute.
- Added history-entry replacement on successful save so save-boundary dirty semantics stay stable after undo/redo.
- Preserved canonical runtime document shape during retained save/commit by merging retained positions and parameter values into the current kernel snapshot instead of blindly rewriting the whole retained snapshot.

## Task Commits

Plan work was committed atomically:

1. `2116fe7` - `fix(31-01): unify retained history and save semantics`

## Files Created/Modified

- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` - adds retained commit/save entry points and retained document/selection application.
- `src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.PersistenceSurface.cs` - routes retained dirty/save completions through kernel authority and builds canonical retained snapshots.
- `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorViewModelKernelAdapter.cs` - suppresses owner reprojection during retained kernel commits/saves and bridges diagnostics/failures back to the retained facade.
- `src/AsterGraph.Editor/Services/History/GraphEditorHistoryService.cs` - adds `ReplaceCurrent(...)` so successful save normalizes the current history baseline.
- `tests/AsterGraph.Editor.Tests/GraphEditorHistorySemanticTests.cs` - focused regression for mixed kernel/runtime plus retained history/save sequences.
- `tests/AsterGraph.Editor.Tests/GraphEditorHistoryTestSupport.cs` - shared catalog/document/workspace test support for the focused suites.

## Issues Encountered

- A naive retained save/commit path polluted canonical kernel document shape with retained view-model dimensions and `ParameterValues` shape differences.
- Kernel document/selection events initially reprojected retained owner state during retained commit/save, which broke retained object identity.
- Recoverable failures and diagnostics initially stayed on the kernel event path instead of surfacing back through the retained facade.

## Resolutions

- Canonical retained snapshots now reuse the current kernel document and only merge retained-owned fields that must persist.
- The kernel adapter now suppresses owner reprojection during retained commit/save and lets the retained owner keep its live object graph.
- Kernel recoverable failures and diagnostics now bridge back into the retained facade event surface.

## Next Phase Readiness

- Retained history/save semantics now run on one authority, which removes the carried semantic ambiguity Phase 32 would otherwise inherit.
- Focused retained history test support now exists for the broader suite split in Plan 02.

## Self-Check: PASSED

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorHistorySemanticTests|FullyQualifiedName~GraphEditorTransactionTests|FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal`
- Result: 99 passed, 0 failed

---
*Phase: 31-history-and-save-semantic-closure*
*Completed: 2026-04-16*
