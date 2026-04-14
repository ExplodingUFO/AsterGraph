# AsterGraph.Avalonia

Avalonia UI shell for the AsterGraph editor.

Quick Start: [main UI entry quick start](../../docs/quick-start.md).

This package belongs to the supported AsterGraph package set with `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Editor`, and it targets `net8.0` and `net9.0`.

Direct package reference:

- Yes, this is the default UI package for hosts embedding the editor.
- Pair it with `AsterGraph.Editor` for the canonical hosted-UI composition path.
- Pair it with `AsterGraph.Abstractions` for node definitions and shared style contracts.
- Add `AsterGraph.Core` when the host also needs direct model or serialization access.

This project intentionally contains:

- `GraphEditorView`
- `GraphEditorView.ChromeMode` for switching between the default shell and `CanvasOnly`
- `NodeCanvas` as a supported standalone graph surface
- `GraphInspectorView` as a supported standalone inspector surface
- `GraphMiniMap` as a supported standalone mini map surface
- `GraphContextMenuPresenter` as the stock Avalonia menu presenter
- `AsterGraphPresentationOptions` plus node/menu/inspector/mini-map presenter contracts for opt-in visual replacement
- canvas rendering and pointer interaction
- theme resources
- context-menu presentation from editor descriptors
- rendered-control-based anchor resolution for connection endpoints

This project intentionally does not own:

- node definition contracts
- compatibility policy
- editor-state orchestration
- demo node content

Those responsibilities live in `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Editor`. `AsterGraph.Demo` remains a sample app only.

Integration entry points:

- [Host Integration Guide](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/host-integration.md)
- [Demo App](https://github.com/ExplodingUFO/AsterGraph/tree/master/src/AsterGraph.Demo)

Canonical and compatibility UI entry paths:

- Canonical full shell: `AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions { ... })`
- Canonical standalone canvas: `AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions { ... })`
- Canonical standalone inspector: `AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions { ... })`
- Canonical standalone mini map: `AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions { ... })`
- Compatibility: `new GraphEditorView { Editor = editor }`

Per-surface presentation replacement is opt-in through `AsterGraphPresentationOptions`.

Phase 18 readiness note:

- future plugin/automation work should anchor on `editor.Session` / `CreateSession(...)`, not on Avalonia controls themselves
- `src/AsterGraph.Demo`, `tools/AsterGraph.PackageSmoke`, and `tools/AsterGraph.ScaleSmoke` now carry the current host/UI validation signals for that runtime boundary

Phase 16 adapter boundary:

- Stock full-shell and standalone-canvas context menus are built from `editor.Session.Queries.BuildContextMenuDescriptors(...)`.
- `IGraphContextMenuPresenter` implementations can opt into the canonical descriptor overload and receive `IGraphEditorCommands` directly. The older `MenuItemDescriptor` overload remains as a compatibility path.
- Stock default keyboard shortcuts in `GraphEditorView` and `NodeCanvas` now route through a shared Avalonia shortcut adapter over `editor.Session.Commands`.
- `GraphEditorView` owns Avalonia clipboard and host-context seam binding for the full shell. Its embedded `NodeCanvas` keeps platform-seam ownership disabled.
- A standalone `NodeCanvas` owns those Avalonia clipboard and host-context seams when it is attached to the visual tree.

Example full-shell configuration:

```csharp
var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    Presentation = new AsterGraphPresentationOptions
    {
        NodeVisualPresenter = customNodePresenter,
        ContextMenuPresenter = customMenuPresenter,
        InspectorPresenter = customInspectorPresenter,
        MiniMapPresenter = customMiniMapPresenter,
    },
});
```

If `Presentation` is omitted, the shipped stock presenters remain active.

Standalone canvas keeps the stock context menu and stock command shortcuts enabled by default. Hosts can explicitly opt out through:

- `EnableDefaultContextMenu`
- `EnableDefaultCommandShortcuts`

If a host replaces only the presenter, not the editor state, the recommended path is to keep consuming canonical session descriptors/commands and treat `GraphEditorViewModel.BuildContextMenu(...)` as retained compatibility surface.

Header/library/status chrome remain shell-only. Phase 4 adds presenter replacement for node visuals, menus, inspector, and mini map without moving editor behavior into the host.
