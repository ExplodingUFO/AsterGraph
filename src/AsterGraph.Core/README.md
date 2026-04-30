# AsterGraph.Core

`AsterGraph.Core` contains the pure graph-domain and persistence layer inside the supported AsterGraph package set.

Supported package set:

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

All four publishable packages target `net8.0`, `net9.0`, and `net10.0`.

It intentionally contains:

- graph document, node, port, and connection models
- serialization support
- compatibility result modeling
- default implicit-conversion rules

It intentionally does **not** contain:

- Avalonia UI types
- demo node definitions
- editor-state orchestration
- package-host-specific workflows

Typical consumers:

- hosts that need explicit model mapping
- adapters that project editor documents into business-specific graph models
- hosts that need direct serialization or compatibility-service access alongside `AsterGraph.Editor`
