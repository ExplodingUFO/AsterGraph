# Capability Breadth Recipe

Use this recipe when you want one copyable hosted path for searchable grouped stencil, SVG/PNG/JPEG export breadth, and shared node or edge quick tools.
Pair it with `ConsumerSample.Avalonia`, because that is the defended hosted proof route for the current beta evaluator ladder.

## What It Covers

- searchable grouped stencil on the stock hosted Avalonia surface
- scene export breadth through the canonical session command route
- shared node and edge quick tools projected from the same tool descriptors that feed the command surface

## Copy This Hosted Path

- Step 1: Project searchable grouped stencil from `ConsumerSample.Avalonia` through `IGraphEditorSession.Queries.GetNodeTemplateSnapshots()` and the stock hosted library chrome.
- Step 2: Close SVG plus raster export breadth through `IGraphEditorSession.Commands.TryExportSceneAsSvg(...)`, `IGraphEditorSession.Commands.TryExportSceneAsImage(...)`, and `GraphEditorSceneImageExportFormat.Png`.
- Step 3: Project shared node and edge quick tools through `AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(...)`, `IGraphEditorSession.Queries.GetToolDescriptors(...)`, and `AsterGraph.ConsumerSample.Avalonia -- --proof`.

## Proof Contract

Validate the route with:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

Expected capability-breadth proof markers:

- `CAPABILITY_BREADTH_STENCIL_OK:True`
- `CAPABILITY_BREADTH_EXPORT_OK:True`
- `CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_OK:True`

## Related Docs

- [Consumer Sample](./consumer-sample.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Host Integration](./host-integration.md)
