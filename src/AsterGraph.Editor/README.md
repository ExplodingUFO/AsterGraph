# AsterGraph.Editor

`AsterGraph.Editor` is the standard host-facing runtime package for AsterGraph.

It belongs to the supported package set with `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Avalonia`, and it targets `net8.0` and `net9.0`.

It intentionally contains:

- `AsterGraphEditorFactory` and `AsterGraphEditorOptions`
- `GraphEditorViewModel`
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

- factory/options-based editor creation via `AsterGraphEditorFactory`
- staged migration support through the retained `GraphEditorViewModel` constructor
- localization via `IGraphLocalizationProvider`
- runtime node display state via `INodePresentationProvider`
- host-owned menu actions via `IGraphContextMenuAugmentor`
- typed host context access through `GraphHostContextExtensions`

Use this package together with `AsterGraph.Avalonia` when the host embeds the default `GraphEditorView`.

Reference material:

- [Host Integration Guide](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/host-integration.md)
- [Host Sample](https://github.com/ExplodingUFO/AsterGraph/tree/master/tools/AsterGraph.HostSample)
- [Root README](https://github.com/ExplodingUFO/AsterGraph/blob/master/README.md)
