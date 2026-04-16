---
phase: 32-grapheditorviewmodel-facade-convergence
plan: 03
subsystem: runtime
tags: [facade, retained-commands, fragments, context-menu]
requires:
  - phase: 32-01
    provides: focused retained facade guardrails and maintenance coverage
  - phase: 32-02
    provides: extracted bootstrap and descriptor builder
provides:
  - top-level internal compatibility command collaborators
  - top-level internal fragment command collaborators
  - retained public methods preserved as thin delegation points
affects: [retained-facade, fragment-commands, context-menu, migration-compatibility]
tech-stack:
  added: []
  patterns: [namespace-level internal collaborator, narrow host interface]
key-files:
  created: []
  modified:
    - src/AsterGraph.Editor/ViewModels/Internal/GraphEditorCompatibilityCommands.cs
    - src/AsterGraph.Editor/ViewModels/Internal/GraphEditorFragmentCommands.cs
    - src/AsterGraph.Editor/ViewModels/Internal/Fragments/GraphEditorFragmentTransferSupport.cs
    - src/AsterGraph.Editor/ViewModels/Internal/Fragments/GraphEditorFragmentClipboardCommands.cs
    - src/AsterGraph.Editor/ViewModels/Internal/Fragments/GraphEditorFragmentWorkspaceCommands.cs
    - src/AsterGraph.Editor/ViewModels/Internal/Fragments/GraphEditorFragmentTemplateCommands.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs
key-decisions:
  - "Move retained compatibility-menu and fragment-command collaborators out of the GraphEditorViewModel type instead of leaving them as nested private architecture debt."
  - "Keep the existing host adapter pattern so retained public methods remain thin delegations over narrow internal command hosts."
patterns-established:
  - "Retained collaborators can live as namespace-level internal services while host adapters remain private to the owner."
  - "Reflection guardrails should prove moved collaborators stay outside GraphEditorViewModel once extracted."
requirements-completed: [FACADE-01, FACADE-02]
duration: working session
completed: 2026-04-16
---

# Phase 32 Plan 03: Compatibility And Fragment Command Extraction Summary

**Finished Phase 32 by moving the retained compatibility-menu and fragment command collaborators out of `GraphEditorViewModel`, leaving the public facade as a thin delegation layer over narrower internal services.**

## Accomplishments

- Converted `IGraphEditorCompatibilityCommandHost`, `GraphEditorCompatibilityCommands`, `IGraphEditorFragmentCommandHost`, and `GraphEditorFragmentCommands` into namespace-level internal types.
- Updated fragment helper types to depend on the narrow fragment command host interface instead of nested `GraphEditorViewModel` types.
- Added reflection guardrails that prove the moved collaborators now exist outside `GraphEditorViewModel` and do not regress back into nested type debt.
- Kept retained menu, clipboard, fragment workspace, and migration-facing behavior stable through the existing public facade methods.

## Task Commits

Plan work was committed atomically:

1. `cf00c5e` - `refactor(32-03): extract retained compatibility and fragment commands`

## Files Created/Modified

- `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorCompatibilityCommands.cs` - now exposes top-level internal compatibility command collaborator types.
- `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorFragmentCommands.cs` - now exposes top-level internal fragment command collaborator types.
- `src/AsterGraph.Editor/ViewModels/Internal/Fragments/*.cs` - updated fragment helpers to depend on the narrow host interface directly.
- `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs` - guards the new collaborator locations and asserts the old nested types stay gone.

## Issues Encountered

- The retained compatibility and fragment command layers were already logically separate, but still inherited the view-model's nested type burden and ownership coupling.

## Resolutions

- Lifted the collaborator types out to namespace scope while keeping the private host adapters in the owner, which reduced `GraphEditorViewModel` ownership without changing retained call sites.

## Next Phase Readiness

- Phase 33 can now focus on downstream hotspot follow-through in `GraphEditorKernel`, `NodeCanvas`, and documentation debt instead of finishing leftover retained-facade extraction.

## Self-Check: PASSED

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~EditorClipboardAndFragmentCompatibilityTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphContextMenuBuilderTests|FullyQualifiedName~GraphEditorFacadeRefactorTests" -v minimal`
- Result: 87 passed, 0 failed
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release`
- Result: maintenance lane completed successfully with 146 passing focused editor tests plus `ScaleSmoke`

---
*Phase: 32-grapheditorviewmodel-facade-convergence*
*Completed: 2026-04-16*
