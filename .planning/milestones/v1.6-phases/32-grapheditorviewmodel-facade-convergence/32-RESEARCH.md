---
phase: 32
slug: grapheditorviewmodel-facade-convergence
status: planned
created: 2026-04-16
updated: 2026-04-16
sources:
  - src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs
  - src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.CommandSurface.cs
  - src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.InteractionSurface.cs
  - src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.PersistenceSurface.cs
  - src/AsterGraph.Editor/ViewModels/Internal/GraphEditorCompatibilityCommands.cs
  - src/AsterGraph.Editor/ViewModels/Internal/GraphEditorFragmentCommands.cs
  - src/AsterGraph.Editor/Runtime/Core/GraphEditorSession.Core.cs
  - tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs
  - tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs
  - tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs
---

# Phase 32 Research

## Baseline

Phase 31 closed the carried history/save semantic drift. The next remaining hotspot is the retained facade itself.

Current repo evidence points to `GraphEditorViewModel` composition and facade-only command orchestration as the highest-value Phase 32 target:

- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` is still 421 lines and remains the constructor/composition center for a long list of collaborators, hosts, commands, session setup, and collection wiring.
- The retained facade partials still total over 1,300 lines (`CanvasSurface` 304, `StateProjection` 228, `CommandSurface` 203, `InteractionSurface` 206, `Infrastructure` 206, `PersistenceSurface` 198).
- Public retained entry points for context menus and fragment operations already delegate through collaborator objects:
  - `BuildContextMenu(...)` delegates to `_compatibilityCommands`.
  - `CopySelectionAsync()`, `ExportSelectionFragment()`, `ImportFragment()`, and related calls delegate to `_fragmentCommands`.
- Those collaborator types already exist, but they are still nested under `GraphEditorViewModel` partial files:
  - `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorCompatibilityCommands.cs`
  - `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorFragmentCommands.cs`
- `GraphEditorSession(ViewModels.GraphEditorViewModel editor, ...)` still reaches into `editor.SessionHost` and `editor.CreateSessionDescriptorSupport()`, so the retained facade still owns some runtime bootstrap/composition knowledge even though kernel state is canonical.

## What This Means

Phase 32 should not try to reopen kernel semantics or canvas interaction internals. That is Phase 33 work.

The highest-leverage contraction for Phase 32 is narrower:

1. keep the public retained surface unchanged
2. reduce `GraphEditorViewModel` constructor/composition burden
3. lift retained-only compatibility/fragment orchestration into clearer internal collaborators
4. prove retained/runtime parity for the moved seams through focused tests

This matches the existing code shape. The repo already uses host adapters and internal services for selection, history, projection, layout, persistence, and presentation localization. Compatibility-menu and fragment command orchestration are the next obvious seams to treat the same way.

## Guardrails Already Available

The repo already has good test anchors for this refactor:

- `GraphEditorFacadeRefactorTests.cs` checks for internal collaborator extraction patterns and host boundaries.
- `GraphEditorSessionTests.cs` covers runtime/session descriptor and menu behavior.
- `GraphEditorServiceSeamsTests.cs` covers host-supplied services, diagnostics, context-menu augmentation, and fragment/workspace seams.
- `GraphContextMenuBuilderTests.cs`, `EditorClipboardAndFragmentCompatibilityTests.cs`, and `GraphEditorMigrationCompatibilityTests.cs` cover retained compatibility behavior directly.
- `eng/ci.ps1 -Lane maintenance` already includes `GraphEditorSessionTests` and `GraphEditorFacadeRefactorTests`, so Phase 32 can build on the shipped refactor gate instead of inventing another one.

## Recommended Scope

### In Scope

- Extract retained-facade bootstrap helpers out of `GraphEditorViewModel.cs` without changing the public constructor or public properties.
- Move nested compatibility/fragment command collaborators toward top-level internal services with narrow host interfaces.
- Add or refine focused parity tests for session descriptor support, fragment/menu service seams, and retained compatibility behavior affected by the extraction.

### Out Of Scope

- Kernel hotspot splitting beyond the retained facade boundary.
- Node canvas interaction or rendering refactors.
- Public API removals, constructor changes, or migration-surface redesign.
- XML doc debt cleanup outside touched files needed for the extracted seams.

## Recommended Execution Order

### Plan 01

Add the facade/runtime parity guardrails that specifically protect menu, fragment, and session-descriptor behavior while the retained facade gets narrower.

### Plan 02

Extract session-descriptor/bootstrap composition out of `GraphEditorViewModel.cs` so the retained constructor stops directly assembling as much runtime bootstrap state.

### Plan 03

Lift retained-only compatibility/fragment command orchestration out of nested `GraphEditorViewModel` partial types into clearer internal collaborators, then finish with planning/state sync and refactor-proof verification.

## Risks

- If Phase 32 changes public constructor shape or event/property ordering, it breaks the retained compatibility promise.
- If Phase 32 moves fragment/menu logic without explicit parity tests, retained/runtime drift will reappear in a subtler form than the Phase 31 history issue.
- If Phase 32 widens into kernel/canvas work, it will blur the line between facade contraction and downstream hotspot follow-through.

## Conclusion

Phase 32 should treat `GraphEditorViewModel` as a supported public shell over narrower internals, not as a direct refactor target. The concrete next step is to protect and extract retained-only bootstrap, compatibility-menu, and fragment-command orchestration while leaving kernel-owned state and public entry points unchanged.
