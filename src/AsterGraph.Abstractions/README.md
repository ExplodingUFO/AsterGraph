# AsterGraph.Abstractions

`AsterGraph.Abstractions` is the stable contract layer for extension and integration.

Direct package reference:

- Yes, for hosts that declare custom node definitions or depend on stable port/compatibility contracts.

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
