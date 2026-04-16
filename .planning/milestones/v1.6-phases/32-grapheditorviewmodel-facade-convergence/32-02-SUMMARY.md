---
phase: 32-grapheditorviewmodel-facade-convergence
plan: 02
subsystem: runtime
tags: [facade, bootstrap, session-descriptors, retained-compatibility]
requires:
  - phase: 32-01
    provides: focused retained facade guardrails and maintenance coverage
provides:
  - extracted retained bootstrap helper
  - extracted session descriptor support builder
  - narrower constructor/bootstrap responsibility in GraphEditorViewModel.cs
affects: [retained-facade, constructor-composition, session-descriptors]
tech-stack:
  added: []
  patterns: [internal bootstrap helper, internal descriptor builder]
key-files:
  created:
    - src/AsterGraph.Editor/ViewModels/Internal/GraphEditorViewModelFacadeBootstrap.cs
    - src/AsterGraph.Editor/ViewModels/Internal/GraphEditorSessionDescriptorSupportBuilder.cs
  modified:
    - src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs
key-decisions:
  - "Move constructor-only collaborator wiring behind a dedicated internal bootstrap helper instead of keeping it inline in GraphEditorViewModel.cs."
  - "Move retained session descriptor support construction behind a dedicated internal builder while leaving public session and factory entry points unchanged."
patterns-established:
  - "Retained facade extraction should move assembly knowledge into internal helpers, not new public composition APIs."
  - "Descriptor support can be builder-owned without changing retained session behavior."
requirements-completed: [FACADE-01, FACADE-02]
duration: working session
completed: 2026-04-16
---

# Phase 32 Plan 02: Bootstrap And Descriptor Builder Extraction Summary

**Pulled retained bootstrap wiring and session descriptor construction out of `GraphEditorViewModel.cs`, keeping the public constructor and session routes stable while materially shrinking inline composition responsibility.**

## Accomplishments

- Added `GraphEditorViewModelFacadeBootstrap` to centralize retained collaborator setup that previously lived inline in the view-model constructor.
- Added `GraphEditorSessionDescriptorSupportBuilder` so retained descriptor support no longer stays assembled directly inside `GraphEditorViewModel.cs`.
- Reduced constructor noise in `GraphEditorViewModel.cs` without changing public factory/session/view-model signatures.
- Added refactor tests that prove the dedicated bootstrap and builder helpers exist.

## Task Commits

Plan work was committed atomically:

1. `70489f0` - `refactor(32-02): extract facade bootstrap and descriptor builder`

## Files Created/Modified

- `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorViewModelFacadeBootstrap.cs` - constructs retained collaborators and exposes them back to the owner.
- `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorSessionDescriptorSupportBuilder.cs` - assembles retained session descriptor support from owner state.
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` - delegates bootstrap and descriptor assembly to the new internal helpers.
- `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs` - adds reflection guardrails for the new helper types.

## Issues Encountered

- The public view-model constructor still directly owned too much retained/runtime assembly knowledge after earlier Phase 31 work.

## Resolutions

- Moved assembly knowledge into explicit internal helper types while preserving initialization order and public entry points.

## Next Phase Readiness

- With bootstrap and descriptor assembly extracted, the remaining retained compatibility and fragment command orchestration can now leave `GraphEditorViewModel` cleanly.

## Self-Check: PASSED

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal`
- Result: 93 passed, 0 failed
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release`
- Result: maintenance lane completed successfully with 143 passing focused editor tests plus `ScaleSmoke`

---
*Phase: 32-grapheditorviewmodel-facade-convergence*
*Completed: 2026-04-16*
