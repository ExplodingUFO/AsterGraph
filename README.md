# AsterGraph

AsterGraph is a modular node-graph editor for .NET with an Avalonia UI shell, a reusable editor state layer, and compile-time extension points for registering custom algorithm nodes.

## Current Scope

Current capabilities:

- draggable, selectable graph nodes
- left-drag marquee selection for multi-node editing
- `Shift` append selection and `Ctrl` toggle selection on node click
- selection-aware right-click menu for batch actions
- mini map overview with viewport recentering
- mini map drag navigation for continuous viewport repositioning
- zoom and pan canvas interaction
- connection rendering and pending connection preview
- graph save/load
- `Ctrl+Z` / `Ctrl+Y` undo and redo graph edits
- selection deletion with `Delete`
- selection copy/paste with `Ctrl+C` / `Ctrl+V`
- batch alignment and distribution for multi-selection
- same-definition multi-selection can batch-edit shared parameters
- selection fragments can be exported to and imported from a JSON file
- strict type compatibility with a small set of safe implicit conversions
- compile-time node-definition registration through providers

Current non-goals:

- runtime plugin loading
- algorithm execution engine
- undo/redo stack
- property editor framework

## Solution Structure

- `src/AsterGraph.Abstractions`
  Stable contracts for node definitions, catalogs, compatibility, and identifiers.
- `src/AsterGraph.Core`
  Pure graph models, serialization, and default compatibility rules.
- `src/AsterGraph.Editor`
  Editor state, catalogs, workspace services, and geometry/viewport helpers.
- `src/AsterGraph.Avalonia`
  Avalonia controls, theme, and input handling.
- `src/AsterGraph.Demo`
  Demo host, sample node-definition provider, and seeded graph document.

## Package Boundary

Recommended package-consumption boundary for external hosts:

- direct host dependencies:
  - `AsterGraph.Avalonia`
  - `AsterGraph.Abstractions`
- explicit optional dependencies:
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
- non-consumable sample host:
  - `AsterGraph.Demo`

This keeps most hosts on a small public surface while still allowing deeper integration when needed.

## Quick Start

Build:

```powershell
dotnet build avalonia-node-map.sln
```

Run the demo:

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj
```

Pack the publishable libraries:

```powershell
dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages
```

Sample local feed config:

```powershell
copy NuGet.config.sample NuGet.config
```

## Extension Model

Custom nodes are added at compile time by implementing `INodeDefinitionProvider` from `AsterGraph.Abstractions`.

Typical flow:

1. Create a provider that returns one or more `NodeDefinition` values.
2. Register that provider into an `INodeCatalog`.
3. Build a `GraphEditorViewModel` with the catalog and a compatibility service.
4. Host `GraphEditorView` from `AsterGraph.Avalonia`.

For host-side layout persistence without saving a full graph snapshot, `GraphEditorViewModel` also exposes:

- `GetNodePositions()`
- `TryGetNodePosition(nodeId, out snapshot)`
- `TrySetNodePosition(nodeId, position)`
- `SetNodePositions(snapshots)`

For host-side integration hooks, `GraphEditorViewModel` also exposes event subscriptions such as:

- `DocumentChanged`
- `SelectionChanged`
- `ViewportChanged`
- `FragmentExported`
- `FragmentImported`

For host-side right-click menu augmentation, hosts can pass an `IGraphContextMenuAugmentor` into `GraphEditorViewModel`.
The augmentor receives the current `GraphEditorViewModel`, the current `ContextMenuContext`, and the stock `MenuItemDescriptor` list, then returns the final menu.
This keeps menu customization in the editor layer and supports nested host menus such as `Results -> Preview / Publish / Create Comparison` without replacing `NodeCanvas`.

For host-side permission control, `GraphEditorBehaviorOptions` now also carries a grouped `GraphEditorCommandPermissions` object.
Hosts can start from `GraphEditorCommandPermissions.Default` or `GraphEditorCommandPermissions.ReadOnly`, then selectively override workspace, node, connection, clipboard, fragment, layout, history, and host-extension permissions.

## Type Compatibility

AsterGraph uses:

- exact type matches by default
- a narrow set of explicit safe implicit conversions

The built-in implicit conversions currently include:

- `int -> float`
- `int -> double`
- `float -> double`

Rejected conversions stay explicit and visible rather than guessing.

## Serialization

Graph documents are serialized in `AsterGraph.Core` and can persist connection-level conversion metadata. The stable contract identifiers in `AsterGraph.Abstractions` are intended to survive UI and host changes.

## Selection And Clipboard

AsterGraph keeps selection state in the editor layer:

- single selection still drives the inspector
- marquee selection can select multiple nodes
- right-click on the current multi-selection opens batch tools
- `Delete` removes the full current selection
- `Ctrl+C` copies the selected nodes plus only the internal links between them
- `Ctrl+V` pastes a new offset fragment near the current viewport center
- copy writes a versioned AsterGraph JSON payload to the system clipboard when the host provides clipboard access
- paste prefers the system clipboard payload and falls back to the in-memory fragment clipboard

Batch selection tools currently include:

- align left, center, right
- align top, middle, bottom
- distribute horizontally or vertically

Hosts can also opt into drag-assist behavior through `CanvasStyleOptions`:

- `EnableGridSnapping`
- `EnableAlignmentGuides`
- `SnapTolerance`

For runtime behavior switching, hosts should prefer `GraphEditorBehaviorOptions` plus `GraphEditorViewModel.UpdateBehaviorOptions(...)`.
The drag assistant now evaluates snapping from the drag start position instead of accumulating per-frame snap deltas, which avoids pointer drift during grid snapping.

## Fragments

In addition to clipboard-based fragment copy/paste, AsterGraph now supports explicit fragment file workflows:

- `Export Fragment` saves the current selection as JSON
- `Import Fragment` loads the saved fragment file and pastes it near the current viewport center
- the demo host uses the default fragment path exposed through the editor workspace services
- hosts can also call `ExportSelectionFragmentTo(path)` and `ImportFragmentFrom(path)` for custom fragment libraries
- hosts can inject a custom `GraphFragmentWorkspaceService` into `GraphEditorViewModel`

## Style Configuration

Hosts can provide a framework-neutral style configuration through `GraphEditorStyleOptions` in `AsterGraph.Abstractions.Styling`.

Recommended flow:

1. Create a `GraphEditorStyleOptions` value in the host
2. Pass it into `GraphEditorViewModel`
3. Let `AsterGraph.Avalonia` adapt those tokens into brushes, radii, spacing, menu rendering, and component visuals

This keeps the public styling surface stable without leaking Avalonia-specific types into the SDK contract.

The current style surface is organized by concern:

- `ShellStyleOptions` for shell colors, typography, and host-panel widths
- `InspectorStyleOptions` for inspector typography and section geometry
- `NodeCardStyleOptions` / `PortStyleOptions` / `ConnectionStyleOptions` for graph-scene visuals
- `ContextMenuStyleOptions` for right-click menu background, hover, foreground, separators, and item sizing

`AsterGraph.Avalonia` now keeps menu rendering behind a dedicated presenter, so hosts can keep driving behavior from editor-layer menu descriptors while styling stays in the Avalonia layer.

## Behavior Configuration

Hosts can also provide explicit editor behavior settings through `GraphEditorBehaviorOptions` in `AsterGraph.Editor.Configuration`.

Recommended flow:

1. Create a `GraphEditorBehaviorOptions` value in the host
2. Pass it into `GraphEditorViewModel`
3. Keep behavior toggles separate from visual tokens

Current behavior sections include:

- `HistoryBehaviorOptions`
- `SelectionBehaviorOptions`
- `DragAssistBehaviorOptions`
- `FragmentBehaviorOptions`
- `ViewBehaviorOptions`
- `GraphEditorCommandPermissions`

The demo host also exposes live toggles for:

- grid snapping
- alignment guides
- read-only mode
- workspace commands
- fragment commands
- host menu extensions

## Command Permissions

Hosts can centrally govern editability through `GraphEditorCommandPermissions`:

- `WorkspaceCommandPermissions`
- `HistoryCommandPermissions`
- `NodeCommandPermissions`
- `ConnectionCommandPermissions`
- `ClipboardCommandPermissions`
- `LayoutCommandPermissions`
- `FragmentCommandPermissions`
- `HostCommandPermissions`

The editor applies these permissions in one place:

- command `CanExecute`
- toolbar enabled state
- keyboard shortcuts
- parameter editor read-only state
- built-in right-click menus
- host context-menu contribution visibility

`ReadOnly` is not a second implementation path. It is a grouped permission preset that keeps navigation and inspection available while denying graph mutations.

## Context Menu Extension

Hosts that need business-specific node actions can use the public context-menu contributor API:

1. Implement `IGraphContextMenuAugmentor`
2. Inspect `ContextMenuContext.TargetKind`
3. Use `ContextMenuContext.ClickedNodeId`, `SelectedNodeIds`, and `SelectedConnectionIds` as needed
4. Start from the provided stock `MenuItemDescriptor` list and append your own items
5. Pass the augmentor into `GraphEditorViewModel`

The demo host now includes a sample node contribution:

- `Results`
- `Preview`
- `Publish`
- `Create Comparison`

This is appended legally through the public editor API rather than by replacing the Avalonia menu renderer.

## Roadmap

- move more sample-only styling and content out of shared projects where appropriate
- add richer graph definition metadata and parameter editing
- support runtime plugin loading on top of the existing provider contracts
- improve automated UI coverage for pointer-based graph gestures

## License

MIT. See [LICENSE.md](./LICENSE.md).
