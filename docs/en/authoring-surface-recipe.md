# Authoring Surface Recipe

Use this recipe when you want one copyable host-owned path for custom node, port, parameter, and edge presentation on top of the canonical session route.

`ConsumerSample.Avalonia` is the concrete host that carries this recipe in code. Pair this document with [Authoring Inspector Recipe](./authoring-inspector-recipe.md) for metadata vocabulary and with [Consumer Sample](./consumer-sample.md) for the sample-owned host boundary.

## Copyable custom authoring presentation

This recipe stays on `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` and reuses the same `IGraphEditorSession` owner. It does not add adapter-specific runtime forks.

## Supported extension surface

The supported customization surface is one cohesive route:

- custom node lifecycle: `IGraphNodeVisualPresenter.Create(...)` creates the root and anchor maps; `Update(...)` refreshes the same visual from `GraphNodeVisualContext`
- handles and targets: `GraphNodeVisual.PortAnchors` maps port ids to controls; `GraphNodeVisual.ConnectionTargetAnchors` maps typed `GraphConnectionTargetRef` endpoints such as parameter targets
- custom edge path: stock edge styling remains the renderer contract; host-owned labels, badges, or diagnostics use `GetConnectionGeometrySnapshots()` and stay outside `NodeCanvas` internals
- runtime decoration to inspector: `IGraphRuntimeOverlayProvider` feeds `GetRuntimeOverlaySnapshot()`, while inspector and node-side editors keep using parameter snapshots and `INodeParameterEditorRegistry`
- boundary: do not depend on `OverlayLayer`, do not add `IGraphEdgeVisualPresenter`, and do not introduce an execution engine

## PRES-01: Multi-handle custom nodes

Use `NodeDefinition` plus multiple input and output ports to declare the handle surface, then project it through `IGraphNodeVisualPresenter`.

- `ConsumerSampleHost.CreateReviewDefinition()` keeps the node facts in one place: multiple inputs, multiple outputs, `defaultWidth`, and `defaultHeight`.
- `ConsumerSampleNodeVisualPresenter` maps those `NodeDefinition` facts into one replaceable visual tree without changing the runtime/session contract.
- `GraphNodeVisual.PortAnchors` is the only anchor map the stock canvas needs for committed connections.

Copy this shape:

- keep multi-handle facts in `NodeDefinition`
- keep `PortDefinition.Key` as the stable `HandleId`
- use `PortDefinition.GroupName`, connection limits, and `ConnectionHint` for copyable grouped handles and hover/search text
- keep size defaults in `defaultWidth` and `defaultHeight`
- keep the visual replacement in `IGraphNodeVisualPresenter`
- keep the session/runtime owner unchanged underneath

## PRES-02: Resizable nodes and node-toolbar surfaces

Use the shared surface/query path for sizing and the shared tool/action path for node-local toolbar buttons.

- Read size and tier state from `GetNodeSurfaceSnapshots()`.
- Persist changes through `TrySetNodeSize(...)`.
- Project node-local actions from `GetToolDescriptors(...)`.
- Scope those actions with `GraphEditorToolContextSnapshot.ForNode(...)`.
- Reuse `AsterGraphHostedActionFactory.CreateToolActions(...)` when the host wants a toolbar outside the node surface.

`ConsumerSampleNodeVisualPresenter` shows the smallest useful split:

- the node toolbar is sample-owned presentation
- width and height still mutate through `TrySetNodeSize(...)`
- the stock canvas keeps resize handles and pointer routing
- the host does not need an adapter-specific runtime fork for toolbar behavior

## PRES-03: Custom port and edge presentation

Keep custom port and edge visuals on top of shared anchors and geometry snapshots.

- Use `GraphNodeVisual.PortAnchors` for custom port placement.
- Use `GraphNodeVisual.ConnectionTargetAnchors` for typed parameter endpoints.
- Use `GetConnectionGeometrySnapshots()` for host-owned custom edge presentation.
- Keep the edge overlay separate from the stock renderer when you want sample-owned or product-owned visuals.

`ConsumerSampleConnectionOverlay` is the bounded sample path here: it reads `GetConnectionGeometrySnapshots()`, derives edge badge positions, and renders host-owned connection labels without changing the runtime route.

## Parameter editors inside the same recipe

Keep parameter metadata in [Authoring Inspector Recipe](./authoring-inspector-recipe.md), then swap editor bodies through `INodeParameterEditorRegistry`.

- `ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions()` wires the sample presenter and registry into `AsterGraphPresentationOptions`.
- `NodeParameterEditorHost` keeps editor creation bounded to `INodeParameterEditorRegistry`.
- Template-specific editor bodies stay sample-owned; metadata vocabulary stays in the inspector recipe.

## Hosted authoring handoff

Use one hosted handoff from definitions to proof instead of stitching together separate inspector-only and node-only stories.

1. Define node, port, and parameter facts in `NodeDefinition`, including `defaultWidth`, `defaultHeight`, `templateKey`, `defaultValue`, and validation/read-only constraints.
2. Project inspector state from `GetSelectedNodeParameterSnapshots()` so the shipped inspector keeps the same metadata, validation, and read-only surface.
3. Project node-side state from `GetNodeParameterSnapshots(nodeId)` so `NodeParameterEditorHost` and `INodeParameterEditorRegistry` reuse the same metadata and validation contract on the custom node surface.
4. Write values back through `TrySetSelectedNodeParameterValue(...)` or `TrySetNodeParameterValue(...)`; keep validation on the shared session command path instead of adding a second editor model.
5. Project host commands from `GetCommandDescriptors()` so toolbars, menus, shortcuts, and palette actions stay on the same shared command route.
6. Close the handoff with `AsterGraph.ConsumerSample.Avalonia -- --proof` and expect `PORT_HANDLE_ID_OK:True`, `PORT_GROUP_AUTHORING_OK:True`, `PORT_CONNECTION_HINT_OK:True`, `PORT_AUTHORING_SCOPE_BOUNDARY_OK:True`, `CUSTOM_EXTENSION_SURFACE_OK:True`, and `AUTHORING_SURFACE_OK:True`.

## Copy path

1. Define the node and parameter facts in `NodeDefinition`, including multiple inputs/outputs, `defaultWidth`, `defaultHeight`, and parameter `templateKey` values.
2. Add `ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions()`-style wiring to `AsterGraphAvaloniaViewOptions.Presentation`.
3. Replace node visuals through `IGraphNodeVisualPresenter`.
4. Replace editor bodies through `INodeParameterEditorRegistry`.
5. Render any host-owned edge overlay from `GetConnectionGeometrySnapshots()`.
6. Keep runtime decorations on `IGraphRuntimeOverlayProvider` and inspector snapshots; do not move graph execution into the editor.

## Related docs

- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Integration](./host-integration.md)
- [Advanced Editing Guide](./advanced-editing.md)
