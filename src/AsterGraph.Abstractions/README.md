# AsterGraph.Abstractions

`AsterGraph.Abstractions` is the stable contract layer inside the supported AsterGraph package set.

Quick Start: [contract entry quick start](../../docs/quick-start.md).

Supported package set:

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

All four publishable packages target `net8.0` and `net9.0`.

Direct package reference:

- Yes, for hosts that declare custom node definitions, stable identifiers, or shared style tokens.
- Pair it with `AsterGraph.Editor` for editor runtime composition.
- Pair it with `AsterGraph.Avalonia` when the host embeds the shipped Avalonia UI.

It intentionally contains only:

- identifier value types (`NodeDefinitionId`, `PortTypeId`, `ConversionId`)
- immutable definition contracts (`INodeDefinition`, `NodeDefinition`, `PortDefinition`, `NodeParameterDefinition`)
- extension-facing interfaces (`INodeDefinitionProvider`, `INodeCatalog`, `IPortCompatibilityService`)
- compatibility result modeling (`PortCompatibilityResult`)

It intentionally does **not** contain:

- Avalonia UI types
- demo data
- editor state implementation
- runtime plugin loading mechanics

Keeping this project small and dependency-light protects binary compatibility for
future third-party node libraries.
