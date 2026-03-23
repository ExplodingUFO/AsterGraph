# AsterGraph.Avalonia

Avalonia UI shell for the AsterGraph editor.

Direct package reference:

- Yes, this is the default UI package for hosts embedding the editor.
- Pair it with `AsterGraph.Abstractions` by default.
- Add `AsterGraph.Editor` only when the host needs direct control over localization, node presentation, menu augmentation, or editor behavior/state seams.

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

Integration entry points:

- [Host Integration Guide](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/host-integration.md)
- [Host Sample](https://github.com/ExplodingUFO/AsterGraph/tree/master/tools/AsterGraph.HostSample)
