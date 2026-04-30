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
- stock tiered node cards with persisted width/height, node-side parameter editors gated by tier and connection state, fixed user-owned group frames, and geometry-based group membership
- stock grouped inspector sections plus text/number/boolean/enum/list editors
- stock parameter-editor registry wiring through `AsterGraphPresentationOptions.NodeParameterEditorRegistry`
- `AsterGraphAvaloniaViewFactory` plus standalone surface factories
- `AsterGraphHostBuilder` as a thin hosted composition facade over `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)`
- narrow `AsterGraphHostBuilder` pass-throughs for stable editor seams such as behavior options, context-menu augmentation, node presentation state, tool descriptors, runtime overlays, and layout plans
- Avalonia theme resources, input handling, and control-level integration glue

## This Package Does Not Own

- node-definition contracts
- compatibility policy
- editor-state orchestration
- demo content

Those responsibilities live in `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and the consuming host.

## Canonical UI Entry Paths

- thin hosted builder: `AsterGraphHostBuilder.Create().UseDocument(document).UseCatalog(catalog).UseDefaultCompatibility().BuildAvaloniaView()`
- hosted full shell: `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions { ... })`
- standalone canvas: `AsterGraphCanvasViewFactory.Create(...)`
- standalone inspector: `AsterGraphInspectorViewFactory.Create(...)`
- standalone mini map: `AsterGraphMiniMapViewFactory.Create(...)`
- retained compatibility: `new GraphEditorView { Editor = editor }`

For new hosted work, prefer `AsterGraphHostBuilder` when the default composition is enough, and use the factory-based routes when you need explicit service wiring. `CreateSession(...)` plus `IGraphEditorSession` remain the canonical runtime surface; this package composes the current Avalonia adapter on top of the retained hosted-UI facade. Treat the direct `GraphEditorView` constructor path as retained compatibility.

## Hosted Builder Cookbook

Default hosted route:

```csharp
var view = AsterGraphHostBuilder
    .Create()
    .UseDocument(document)
    .UseCatalog(catalog)
    .UseDefaultCompatibility()
    .BuildAvaloniaView();
```

Explicit factory route:

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibility,
});

var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
});
```

Use the builder to reduce boilerplate in the common hosted route. Use explicit factories when the host needs editor/view options that are not exposed by the builder, owns storage/export services directly, or composes standalone surfaces separately. Both routes share the same editor/session owner.

`AsterGraphHostBuilder.UseNodePresentationProvider(...)` forwards `AsterGraphEditorOptions.NodePresentationProvider`, which supplies editor-runtime node presentation state. It is not the same seam as `AsterGraphPresentationOptions.NodeVisualPresenter`, `NodeBodyPresenter`, inspector, mini-map, or parameter-editor presenters, which replace Avalonia visuals.

`NodeCanvas` consumes the shared editor command/query surface for tiered authoring UX. The same persisted node/group state drives resize handles, width/height tiers, node-side parameter values, fixed group frames, geometry-based membership, and editor-only group chrome.

Group frames keep header, border, and content-area semantics separate: the stock canvas drags from the header, resizes from the border, and treats the content area as the membership zone while keeping the stored frame boundary stable.

Node-side parameter editors are created through the stock `NodeParameterEditorRegistry` seam. The canvas only renders a parameter editor when the active tier exposes the section and the parameter endpoint is not connected upstream.

## Start Here

- quickest hosted-UI first run: [`tools/AsterGraph.HelloWorld.Avalonia`](../../tools/AsterGraph.HelloWorld.Avalonia/)
- canonical onboarding: [Quick Start](../../docs/en/quick-start.md)
- route and composition guidance: [Host Integration](../../docs/en/host-integration.md)
- advanced editing surface map: [Advanced Editing Guide](../../docs/en/advanced-editing.md)
- definition-driven inspector recipe: [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md)
- visual showcase host: [`src/AsterGraph.Demo`](../../src/AsterGraph.Demo/)
- product overview: [Root README](../../README.md)
