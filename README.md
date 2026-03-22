# AsterGraph

AsterGraph is a modular node-graph editor for .NET with an Avalonia UI shell, a reusable editor state layer, and compile-time extension points for registering custom algorithm nodes.

## Current Scope

Current capabilities:

- draggable, selectable graph nodes
- zoom and pan canvas interaction
- connection rendering and pending connection preview
- graph save/load
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

## Quick Start

Build:

```powershell
dotnet build avalonia-node-map.sln
```

Run the demo:

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj
```

## Extension Model

Custom nodes are added at compile time by implementing `INodeDefinitionProvider` from `AsterGraph.Abstractions`.

Typical flow:

1. Create a provider that returns one or more `NodeDefinition` values.
2. Register that provider into an `INodeCatalog`.
3. Build a `GraphEditorViewModel` with the catalog and a compatibility service.
4. Host `GraphEditorView` from `AsterGraph.Avalonia`.

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

## Roadmap

- move more sample-only styling and content out of shared projects where appropriate
- add richer graph definition metadata and parameter editing
- support runtime plugin loading on top of the existing provider contracts
- improve automated UI coverage for pointer-based graph gestures

## License

This repository is proprietary software.

- Non-commercial private evaluation only by default
- Commercial use requires a paid written license

See [LICENSE.md](./LICENSE.md) for the full terms.
