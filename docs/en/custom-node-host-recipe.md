# Custom Node Host Recipe

This recipe shows the host-centric path for registering custom node definitions, declaring ports with grouping and validation, sizing defaults, replacing node visuals through `IGraphNodeVisualPresenter`, and wiring `PortAnchors` for edge geometry.

Use it when your host owns the node catalog and wants custom presentation on the canonical Avalonia route.

For the smaller plugin-author path, see [Plugin And Custom Node Recipe](./plugin-recipe.md). For the full authoring surface story, see [Authoring Surface Recipe](./authoring-surface-recipe.md).

## Packages

```powershell
dotnet add package AsterGraph.Abstractions --prerelease
dotnet add package AsterGraph.Avalonia --prerelease
```

Add `AsterGraph.Editor` if you are building a runtime-only or custom UI host.

## 1. Registering Definitions

Define nodes in a catalog or through a plugin's `INodeDefinitionProvider`.

```csharp
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

public sealed class MyNodeProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        =>
        [
            new NodeDefinition(
                id: new NodeDefinitionId("myhost.review"),
                displayName: "Review Node",
                category: "MyHost",
                subtitle: "Review",
                inputPorts:
                [
                    new PortDefinition("in1", "Input A", new PortTypeId("string"), "#6AD5C4"),
                    new PortDefinition("in2", "Input B", new PortTypeId("number"), "#6AD5C4"),
                ],
                outputPorts:
                [
                    new PortDefinition("out1", "Result", new PortTypeId("boolean"), "#F3B36B"),
                ],
                parameters:
                [
                    new NodeParameterDefinition(
                        key: "threshold",
                        displayName: "Threshold",
                        typeId: new PortTypeId("number"),
                        defaultValue: 0.5,
                        constraints: new NodeParameterConstraints(isReadOnly: false)),
                ],
                description: "A host-owned review node.",
                accentHex: "#406379",
                defaultWidth: 280d,
                defaultHeight: 180d)
        ];
}
```

Register the provider:

```csharp
// Direct catalog composition
var catalog = new NodeCatalog();
catalog.Register(new MyNodeProvider());

// Or through a plugin
builder.AddNodeDefinitionProvider(new MyNodeProvider());
```

## 2. Declaring Ports

Port rules:

- `key` must be unique within the node's input set, output set, and across both sets
- `displayName` is the visible label
- `typeId` drives compatibility through the host's `IPortCompatibilityService`
- `accentHex` sets the port dot color

Validation is automatic: `NodeDefinition` throws when duplicate port keys are supplied.

## 3. Setting Default Size

Set `defaultWidth` and `defaultHeight` on `NodeDefinition`:

```csharp
defaultWidth: 280d,
defaultHeight: 180d
```

The stock canvas respects these as the initial size. Hosts can mutate size later through `IGraphEditorSession.Commands.TrySetNodeSize(...)` or `TrySetNodeWidth(...)`.

## 4. Replacing Node Visuals

Replace the visual tree by implementing `IGraphNodeVisualPresenter`:

```csharp
using Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;

public sealed class MyNodeVisualPresenter : IGraphNodeVisualPresenter
{
    private readonly DefaultGraphNodeVisualPresenter _fallback = new();

    public GraphNodeVisual Create(GraphNodeVisualContext context)
    {
        if (!ShouldUseCustomVisual(context.Node))
        {
            return _fallback.Create(context);
        }

        var portAnchors = new Dictionary<string, Control>(StringComparer.Ordinal);
        var root = BuildRootVisual(context, portAnchors);

        var visual = new GraphNodeVisual(root, portAnchors, presenterState: null);
        Update(visual, context);
        return visual;
    }

    public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
    {
        if (!ShouldUseCustomVisual(context.Node))
        {
            _fallback.Update(visual, context);
            return;
        }

        // Update title, port labels, parameter rows from context.Node
    }

    private static bool ShouldUseCustomVisual(NodeViewModel node)
        => node.DefinitionId == "myhost.review";
}
```

Wire it into the hosted view:

```csharp
var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    Presentation = new AsterGraphPresentationOptions
    {
        NodeVisualPresenter = new MyNodeVisualPresenter(),
    },
});
```

Lifecycle boundary:

- `Create(...)` builds the root control, anchor dictionaries, and optional presenter state.
- `Update(...)` refreshes text, parameter rows, toolbar state, and anchor dictionaries from the latest `GraphNodeVisualContext`.
- A custom presenter should delegate to `DefaultGraphNodeVisualPresenter` for nodes it does not own.
- Presenter state stays host-owned; persisted graph changes still go through `IGraphEditorSession.Commands`.

## 5. PortAnchors, TargetAnchors, and Edge Geometry

`GraphNodeVisual.PortAnchors` is the anchor map the stock canvas uses for committed connections.

Rules:

- Keys are port ids (`port.Id` from `PortViewModel`)
- Values are `Control` instances (usually small dots or buttons)
- The canvas reads anchor positions from these controls to draw edge endpoints
- Custom presenters must populate this dictionary during `Create` and keep it in sync during `Update`

`GraphNodeVisual.ConnectionTargetAnchors` is the matching typed anchor map for parameter endpoints and other non-port connection targets. Use `GraphConnectionTargetRef` keys and call `context.ActivateConnectionTarget(...)` from the target control when the user starts a connection from that endpoint.

For custom edge presentation, query geometry from the session:

```csharp
var geometries = session.Queries.GetConnectionGeometrySnapshots();
```

Each snapshot contains source/target anchor positions and route vertices. Hosts can render host-owned edge overlays from this data without changing the runtime route.

The supported custom edge path is stock edge styling plus an optional host-owned overlay from geometry snapshots. There is no public `IGraphEdgeVisualPresenter`, and `NodeCanvas` internal layers such as `OverlayLayer` are intentionally not part of this recipe.

## 6. Proof Validation

Close the custom-node handoff with the defended proof run:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

Expect:

- `AUTHORING_SURFACE_OK:True`
- `CUSTOM_EXTENSION_SURFACE_OK:True`
- `CUSTOM_EXTENSION_NODE_PRESENTER_LIFECYCLE_OK:True`
- `CUSTOM_EXTENSION_ANCHOR_SURFACE_OK:True`
- `CUSTOM_EXTENSION_EDGE_OVERLAY_OK:True`
- `CUSTOM_EXTENSION_RUNTIME_INSPECTOR_OK:True`
- `CUSTOM_EXTENSION_SCOPE_BOUNDARY_OK:True`
- `AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True`
- `AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True`
- `CONSUMER_SAMPLE_OK:True`

## Copy Path Summary

1. Define `NodeDefinition` with inputs, outputs, parameters, `defaultWidth`, and `defaultHeight`
2. Register the provider in the catalog or through a plugin
3. Implement `IGraphNodeVisualPresenter` for custom visuals
4. Populate `GraphNodeVisual.PortAnchors` with port-id-to-control mappings
5. Populate `GraphNodeVisual.ConnectionTargetAnchors` for typed parameter endpoints when needed
6. Render custom edge badges or labels from `GetConnectionGeometrySnapshots()` if stock styling is not enough
7. Wire the presenter into `AsterGraphPresentationOptions.NodeVisualPresenter`
8. Validate with `ConsumerSample.Avalonia -- --proof` and expect `AUTHORING_SURFACE_OK:True` plus `CUSTOM_EXTENSION_SURFACE_OK:True`

## Related Docs

- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Node Presentation Guidelines](./node-presentation-guidelines.md)
- [Host Integration](./host-integration.md)
- [Host Recipe Ladder](./host-recipe-ladder.md)
