# AsterGraph.Avalonia

`AsterGraph.Avalonia` is the default Avalonia UI package for AsterGraph.

It belongs to the supported published package set with `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Editor`, and it targets `net8.0` and `net9.0`.

## Reference This Package When

- the host wants the shipped full editor shell
- the host wants standalone Avalonia surfaces such as the stock canvas, inspector, or mini map
- the host wants stock Avalonia presenters with optional presenter replacement through `AsterGraphPresentationOptions`

## This Package Owns

- `GraphEditorView`
- `NodeCanvas`
- `GraphInspectorView`
- `GraphMiniMap`
- stock Avalonia menu and presentation wiring
- stock progressive node cards with persisted width, expand/collapse, inline literal editors, and adaptive editor-only group boundaries
- stock grouped inspector sections plus text/number/boolean/enum/list editors
- `AsterGraphAvaloniaViewFactory` plus standalone surface factories
- Avalonia theme resources, input handling, and control-level integration glue

## This Package Does Not Own

- node-definition contracts
- compatibility policy
- editor-state orchestration
- demo content

Those responsibilities live in `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and the consuming host.

## Canonical UI Entry Paths

- hosted full shell: `AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions { ... })`
- standalone canvas: `AsterGraphCanvasViewFactory.Create(...)`
- standalone inspector: `AsterGraphInspectorViewFactory.Create(...)`
- standalone mini map: `AsterGraphMiniMapViewFactory.Create(...)`
- retained compatibility: `new GraphEditorView { Editor = editor }`

For new work, prefer the factory-based routes. Treat the direct `GraphEditorView` constructor path as retained compatibility.

`NodeCanvas` consumes the shared editor command/query surface for progressive authoring UX. The same persisted node-surface state drives resize handles, double-click expand/collapse, inline input literals, adaptive group bounds, and editor-only group chrome.

Adaptive group framing resolves as `member bounds + persisted extra padding`: the stock canvas keeps group frames attached to member-node geometry, exposes per-edge resize handles for extra whitespace, and lets users drag the frame together with its member nodes.

## Start Here

- quickest hosted-UI first run: [`tools/AsterGraph.HelloWorld.Avalonia`](../../tools/AsterGraph.HelloWorld.Avalonia/)
- canonical onboarding: [Quick Start](../../docs/en/quick-start.md)
- route and composition guidance: [Host Integration](../../docs/en/host-integration.md)
- definition-driven inspector recipe: [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md)
- visual showcase host: [`src/AsterGraph.Demo`](../../src/AsterGraph.Demo/)
- product overview: [Root README](../../README.md)
