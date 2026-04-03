# AsterGraph.Editor

`AsterGraph.Editor` is the standard host-facing runtime package for AsterGraph.

It belongs to the supported package set with `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Avalonia`, and it targets `net8.0` and `net9.0`.

It intentionally contains:

- `IGraphEditorSession` plus `Commands`, `Queries`, `Events`, and mutation batching
- `AsterGraphEditorFactory` and `AsterGraphEditorOptions`
- `GraphEditorViewModel`
- replaceable storage/clipboard/diagnostics seams
- node/template catalogs
- context-menu intent and command wiring
- selection, pending connection, and workspace state
- parameter Inspector view-model state

It intentionally does **not** contain:

- Avalonia visual controls
- demo node content
- host-specific business commands

Typical consumers:

- hosts that build or extend an editor session, even when they also ship the default Avalonia UI
- integration layers that extend menus, commands, inspector behavior, localization, or presentation

Start here when a host needs the canonical runtime entry point:

- runtime-first session creation via `AsterGraphEditorFactory.CreateSession(...)`
- core runtime interaction ownership via session commands for selection, node positioning, connection lifecycle, and viewport control
- pending connection observation via `GetPendingConnectionSnapshot()` and `PendingConnectionChanged`
- DTO-based compatible-target discovery via `GetCompatiblePortTargets(...)`
- factory/options-based editor creation via `AsterGraphEditorFactory`
- staged migration support through the retained `GraphEditorViewModel` constructor
- package-neutral default storage redirection through `StorageRootPath`
- replaceable services via `IGraphWorkspaceService`, `IGraphFragmentWorkspaceService`, `IGraphFragmentLibraryService`, and `IGraphClipboardPayloadSerializer`
- recoverable-failure publication through `IGraphEditorDiagnosticsSink`
- localization via `IGraphLocalizationProvider`
- runtime node display state via `INodePresentationProvider`
- host-owned menu actions via `IGraphContextMenuAugmentor`
- typed host context access through `GraphHostContextExtensions`

Use this package together with `AsterGraph.Avalonia` when the host embeds the default `GraphEditorView`. Hosts that provide their own UI can stop at the `IGraphEditorSession` boundary and avoid taking an Avalonia dependency in their composition root.

The MVVM-typed compatibility query path remains available for legacy integrations, but new host code should treat it as compatibility-only rather than the canonical runtime surface.

The same guidance now applies to host extension seams:

- prefer `GraphContextMenuAugmentationContext` over taking `GraphEditorViewModel` directly
- prefer `NodePresentationContext` over taking `NodeViewModel` directly

The older MVVM-rooted extension methods remain available only as migration shims.

Reference material:

- [Host Integration Guide](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/host-integration.md)
- [Host Sample](https://github.com/ExplodingUFO/AsterGraph/tree/master/tools/AsterGraph.HostSample)
- [Root README](https://github.com/ExplodingUFO/AsterGraph/blob/master/README.md)
