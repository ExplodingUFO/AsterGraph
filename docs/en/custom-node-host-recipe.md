# Custom Node Host Recipe

This recipe shows the host-centric path for registering custom node definitions, declaring ports with grouping and validation, sizing defaults, choosing `NodeBodyPresenter` or `NodeVisualPresenter`, marking drag handles with `NodeDragHandle`, and wiring anchors for edge geometry.

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

## 4. Choose the Presentation Seam

Pick the smallest supported presentation seam that matches the amount of visual ownership your host needs:

| Need | Use | What stays stock |
| --- | --- | --- |
| Keep the stock shell, title, ports, selection chrome, resize/drag behavior, and only replace the body content | `AsterGraphPresentationOptions.NodeBodyPresenter` / `IGraphNodeBodyPresenter` | `DefaultGraphNodeVisualPresenter`, `GraphNodeVisual.PortAnchors`, stock node drag, stock committed-edge rendering |
| Replace the entire node visual tree, including custom port placement and pointer routing | `AsterGraphPresentationOptions.NodeVisualPresenter` / `IGraphNodeVisualPresenter` | Runtime/session contracts, connection geometry snapshots, persistence, undo/redo |

Use `NodeBodyPresenter` first when your custom node is mostly custom content inside the standard AsterGraph node frame. Use `NodeVisualPresenter` only when you need to own the full root visual, port controls, layout, or pointer behavior.

### Stock Shell Body Customization

Implement `IGraphNodeBodyPresenter` when you want a React Flow-style custom body without giving up the stock AsterGraph shell:

```csharp
using Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;

public sealed class MyNodeBodyPresenter : IGraphNodeBodyPresenter
{
    public GraphNodeBodyVisual Create(GraphNodeVisualContext context)
    {
        var handle = new Border { Name = "PART_ReviewDragHandle" };
        NodeDragHandle.SetIsDragHandle(handle, true);

        var body = new StackPanel
        {
            Children =
            {
                handle,
                new TextBlock { Text = context.Node.DisplayName },
            },
        };

        return new GraphNodeBodyVisual(body);
    }

    public void Update(GraphNodeBodyVisual visual, GraphNodeVisualContext context)
    {
        // Refresh body-only state from context.Node.
    }
}
```

Wire the body presenter without replacing the full node visual presenter:

```csharp
Presentation = new AsterGraphPresentationOptions
{
    NodeBodyPresenter = new MyNodeBodyPresenter(),
}
```

Drag handle boundary:

- `NodeDragHandle.SetIsDragHandle(control, true)` marks a control that can start node dragging inside the stock shell.
- If a stock-shell node contains any marked drag handle, dragging starts only from that handle or its descendants.
- Unmarked body content remains available for text selection, buttons, sliders, parameter editors, or other host-owned interactions.
- Full `NodeVisualPresenter` replacements own their root pointer routing and can read the same attached property if they choose to mirror the stock-shell rule.

## 5. Replacing Full Node Visuals

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

## 6. PortAnchors, TargetAnchors, and Edge Geometry

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

## 7. Proof Validation

Close the custom-node handoff with the defended proof run:

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof
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
3. Use `IGraphNodeBodyPresenter` plus `AsterGraphPresentationOptions.NodeBodyPresenter` when the stock shell should own title, ports, resize, drag, and selection chrome
4. Mark body drag handles with `NodeDragHandle.SetIsDragHandle(control, true)` when only part of the body should start node dragging
5. Use `IGraphNodeVisualPresenter` plus `AsterGraphPresentationOptions.NodeVisualPresenter` only when the host must replace the full visual tree
6. Populate `GraphNodeVisual.PortAnchors` with port-id-to-control mappings in full visual replacements
7. Populate `GraphNodeVisual.ConnectionTargetAnchors` for typed parameter endpoints when needed
8. Render custom edge badges or labels from `GetConnectionGeometrySnapshots()` if stock styling is not enough
9. Validate with `src/AsterGraph.Demo -- --proof` and expect `AUTHORING_SURFACE_OK:True` plus `CUSTOM_EXTENSION_SURFACE_OK:True`

## Related Docs

- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Node Presentation Guidelines](./node-presentation-guidelines.md)
- [Host Integration](./host-integration.md)
- [Host Recipe Ladder](./host-recipe-ladder.md)
