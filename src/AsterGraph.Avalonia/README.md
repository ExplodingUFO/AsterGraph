# AsterGraph.Avalonia

Avalonia UI shell for the AsterGraph editor.

This package belongs to the supported AsterGraph package set with `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Editor`, and it targets `net8.0` and `net9.0`.

Direct package reference:

- Yes, this is the default UI package for hosts embedding the editor.
- Pair it with `AsterGraph.Editor` for the canonical Phase 1 host-composition path.
- Pair it with `AsterGraph.Abstractions` for node definitions and shared style contracts.
- Add `AsterGraph.Core` when the host also needs direct model or serialization access.

This project intentionally contains:

- `GraphEditorView`
- `GraphEditorView.ChromeMode` for switching between the default shell and `CanvasOnly`
- canvas rendering and pointer interaction
- theme resources
- context-menu presentation from editor descriptors
- rendered-control-based anchor resolution for connection endpoints

This project intentionally does not own:

- node definition contracts
- compatibility policy
- editor-state orchestration
- demo node content

Those responsibilities live in `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Editor`. `AsterGraph.Demo` remains a sample app only.

Integration entry points:

- [Host Integration Guide](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/host-integration.md)
- [Host Sample](https://github.com/ExplodingUFO/AsterGraph/tree/master/tools/AsterGraph.HostSample)

Canonical and compatibility UI entry paths:

- Canonical: `AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions { ... })`
- Compatibility: `new GraphEditorView { Editor = editor }`
