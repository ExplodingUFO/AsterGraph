# AsterGraph.Core

`AsterGraph.Core` contains the pure graph-domain layer for AsterGraph.

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

