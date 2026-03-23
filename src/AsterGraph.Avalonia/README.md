# AsterGraph.Avalonia

Avalonia UI shell for the AsterGraph editor.

Direct package reference:

- Yes, this is the default UI package for hosts embedding the editor.
- Pair it with `AsterGraph.Abstractions` by default.

This project intentionally contains:

- `GraphEditorView`
- canvas rendering and pointer interaction
- theme resources
- context-menu presentation from editor descriptors
- rendered-control-based anchor resolution for connection endpoints

This project intentionally does not own:

- node definition contracts
- compatibility policy
- editor-state orchestration
- demo node content

Those responsibilities live in `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Demo`.
