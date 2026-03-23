# AsterGraph.Editor

`AsterGraph.Editor` owns editor-state orchestration for AsterGraph.

It intentionally contains:

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

- advanced hosts that need direct access to editor-state orchestration
- integration layers that extend menus, commands, or inspector behavior

Start here when a host needs deeper editor integration beyond the default Avalonia shell:

- localization via `IGraphLocalizationProvider`
- runtime node display state via `INodePresentationProvider`
- host-owned menu actions via `IGraphContextMenuAugmentor`
- typed host context access through `GraphHostContextExtensions`

Reference material:

- [Host Integration Guide](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/host-integration.md)
- [Host Sample](https://github.com/ExplodingUFO/AsterGraph/tree/master/tools/AsterGraph.HostSample)

