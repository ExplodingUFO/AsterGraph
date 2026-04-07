---
phase: 16-avalonia-adapter-boundary-cleanup
plan: 01
subsystem: shared-avalonia-routing
completed: 2026-04-08
---

# Phase 16 Plan 01 Summary

Collapsed Avalonia's stock menu and default shortcut routing into shared descriptor/session-backed adapters instead of keeping shell and canvas on duplicated retained-facade policy code.

Key changes:

- Added `GraphEditorDefaultCommandShortcutRouter` under `src/AsterGraph.Avalonia/Controls/Internal/` and switched both `GraphEditorView` and `NodeCanvas` to use it for the shipped save/load/undo/redo/delete and pending-connection cancel shortcuts.
- Kept copy/paste inside the same shared router, but still executed them through the retained compatibility command bridge because the runtime command surface does not yet expose canonical async clipboard command IDs.
- Added `GraphContextMenuDescriptorAdapter` plus a canonical overload on `IGraphContextMenuPresenter` so custom presenters can opt into descriptor-first menu input while old presenters continue working through the compatibility adapter.
- Extended `GraphContextMenuPresenter` with a canonical descriptor overload and canonical test hook so the stock Avalonia presenter can render runtime menu descriptors directly.
- Switched `NodeCanvas` stock context-menu opening from `GraphEditorViewModel.BuildContextMenu(...)` to `ViewModel.Session.Queries.BuildContextMenuDescriptors(...)`, passing `Session.Commands` into the presenter path.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphContextMenuPresenterTests|FullyQualifiedName~NodeCanvasStandaloneTests" -v minimal`
  - exit 0
  - `14` tests passed
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphContextMenuPresenterTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~NodeCanvasStandaloneTests|FullyQualifiedName~GraphEditorInitializationTests" -v minimal`
  - exit 0
  - `32` tests passed

Phase 16 status after this plan:

- `ADAPT-01` is partially closed: default stock command/menu routing now shares Avalonia adapter logic and the stock menu path is canonical-descriptor-driven.
- `GraphEditorView` and `NodeCanvas` no longer duplicate the shipped default shortcut chain.
- `16-02` remains next: platform seam attachment still needs narrowing around clipboard and host-context wiring.
