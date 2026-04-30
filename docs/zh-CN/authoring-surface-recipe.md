# Authoring Surface Recipe

当你想在 canonical session 路线上，把自定义节点、端口、参数编辑器和边展示收成一条可复制的宿主自管路径时，就看这份 recipe。

真正承载这条 recipe 代码的是 `ConsumerSample.Avalonia`。元数据词汇仍由 [Authoring Inspector Recipe](./authoring-inspector-recipe.md) 负责，而宿主边界和 proof 位置仍由 [Consumer Sample](./consumer-sample.md) 负责。

## 可复制的自定义 authoring presentation

这条 recipe 继续停留在 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` 上，并且复用同一个 `IGraphEditorSession` owner。它不会引入 adapter-specific runtime forks。

## 受支持的扩展 surface

受支持的自定义 surface 是一条连贯路线：

- 自定义节点生命周期：`IGraphNodeVisualPresenter.Create(...)` 创建 root 和 anchor maps；`Update(...)` 从 `GraphNodeVisualContext` 刷新同一棵视觉树。
- handles 与 targets：`GraphNodeVisual.PortAnchors` 把 port id 映射到 control；`GraphNodeVisual.ConnectionTargetAnchors` 把 typed `GraphConnectionTargetRef` endpoint（例如参数目标）映射到 control。
- 自定义边路线：stock edge styling 仍是 renderer contract；宿主自管 label、badge 或 diagnostics 使用 `GetConnectionGeometrySnapshots()`，并保持在 `NodeCanvas` internals 之外。
- runtime decoration 到 inspector：`IGraphRuntimeOverlayProvider` 供给 `GetRuntimeOverlaySnapshot()`；inspector 与节点旁路 editor 继续使用 parameter snapshots 和 `INodeParameterEditorRegistry`。
- 边界：不要依赖 `OverlayLayer`，不要新增 `IGraphEdgeVisualPresenter`，也不要引入 execution engine。

## PRES-01：多 Handle 节点

先用 `NodeDefinition` 把多输入、多输出、默认宽高这些事实声明出来，再通过 `IGraphNodeVisualPresenter` 投成宿主自定义节点可视树。

- `ConsumerSampleHost.CreateReviewDefinition()` 把多个输入端口、多个输出端口、`defaultWidth` 和 `defaultHeight` 这些事实放在同一处。
- `ConsumerSampleNodeVisualPresenter` 只负责把这些 `NodeDefinition` 事实变成可替换的 UI，不改变底层 runtime/session contract。
- `GraphNodeVisual.PortAnchors` 仍然是 stock canvas 计算 committed connections 所需的唯一锚点图。

复制时保持这条分工：

- 多 handle 事实继续留在 `NodeDefinition`
- `PortDefinition.Key` 继续作为稳定 `HandleId`
- 用 `PortDefinition.GroupName`、连接数限制和 `ConnectionHint` 承载可复制的端口分组与 hover/search 文案
- 默认尺寸继续留在 `defaultWidth` / `defaultHeight`
- 节点可视替换继续留在 `IGraphNodeVisualPresenter`
- 底层 session/runtime owner 不变

## PRES-02：可缩放节点与节点工具栏

节点尺寸继续走共享 surface/query 路线，节点工具栏继续走共享 tool/action 路线。

- 用 `GetNodeSurfaceSnapshots()` 读取尺寸和 tier state。
- 用 `TrySetNodeSize(...)` 持久化尺寸变化。
- 用 `GetToolDescriptors(...)` 取节点工具动作。
- 用 `GraphEditorToolContextSnapshot.ForNode(...)` 约束节点上下文。
- 当宿主要把同一批动作投到节点外部工具栏时，继续复用 `AsterGraphHostedActionFactory.CreateToolActions(...)`。

`ConsumerSampleNodeVisualPresenter` 展示的是最小可用拆分：

- 节点工具栏是样例自管 presentation
- 宽高变化仍然通过 `TrySetNodeSize(...)` 落回共享 session 路线
- resize handles 和 pointer routing 继续由 stock canvas 管
- 宿主不需要为 toolbar 行为再开 adapter-specific runtime fork

## PRES-03：自定义端口与边展示

端口和边的自定义展示继续建立在共享锚点和几何快照之上。

- 用 `GraphNodeVisual.PortAnchors` 控制自定义端口位置。
- 用 `GraphNodeVisual.ConnectionTargetAnchors` 暴露 typed parameter endpoints。
- 用 `GetConnectionGeometrySnapshots()` 驱动宿主自管的自定义边展示。
- 当你想保留 stock renderer、只额外叠一层产品自有视觉时，把 edge overlay 独立出来即可。

`ConsumerSampleConnectionOverlay` 就是这里的 bounded sample path：它读取 `GetConnectionGeometrySnapshots()`，计算边 badge 的位置，并在不改变 runtime 路线的前提下渲染宿主自管的连接标签。

## 把参数编辑器放进同一条 recipe

参数 metadata 继续由 [Authoring Inspector Recipe](./authoring-inspector-recipe.md) 负责，而 editor body 则通过 `INodeParameterEditorRegistry` 替换。

- `ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions()` 把样例 presenter 和 registry 一起挂进 `AsterGraphPresentationOptions`。
- `NodeParameterEditorHost` 继续把 editor body 创建收口到 `INodeParameterEditorRegistry`。
- template-specific 的 editor body 仍然是样例自有；metadata vocabulary 仍然归 inspector recipe 所有。

## Hosted authoring handoff

不要把 inspector、节点旁路编辑器和 proof 拆成几段分开的故事；这条 hosted handoff 要从 definitions 一直连到 proof。

1. 先在 `NodeDefinition` 里定义节点、端口和参数事实，包括 `defaultWidth`、`defaultHeight`、`templateKey`、`defaultValue`，以及 validation/read-only constraints。
2. 再用 `GetSelectedNodeParameterSnapshots()` 投影 inspector 状态，让 shipped inspector 继续复用同一份 metadata、validation 和只读合同。
3. 然后用 `GetNodeParameterSnapshots(nodeId)` 投影节点旁路状态，让 `NodeParameterEditorHost` 和 `INodeParameterEditorRegistry` 在自定义节点表面上继续复用同一份 metadata 和 validation 合同。
4. 写回时继续走 `TrySetSelectedNodeParameterValue(...)` 或 `TrySetNodeParameterValue(...)`，把 validation 保留在共享 session command 路线上，不再引入第二套 editor model。
5. 宿主命令继续从 `GetCommandDescriptors()` 投影，这样 toolbar、menu、shortcut 和 palette action 都停留在同一条共享 command route 上。
6. 最后用 `AsterGraph.ConsumerSample.Avalonia -- --proof` 收口，并期待看到 `PORT_HANDLE_ID_OK:True`、`PORT_GROUP_AUTHORING_OK:True`、`PORT_CONNECTION_HINT_OK:True`、`PORT_AUTHORING_SCOPE_BOUNDARY_OK:True`、`CUSTOM_EXTENSION_SURFACE_OK:True` 和 `AUTHORING_SURFACE_OK:True`。

## 复制顺序

1. 先在 `NodeDefinition` 里定义节点和参数事实，包括多输入/多输出、`defaultWidth`、`defaultHeight` 以及参数 `templateKey`。
2. 再按 `ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions()` 的形状，把 wiring 接到 `AsterGraphAvaloniaViewOptions.Presentation`。
3. 通过 `IGraphNodeVisualPresenter` 替换节点可视树。
4. 通过 `INodeParameterEditorRegistry` 替换 editor body。
5. 通过 `GetConnectionGeometrySnapshots()` 渲染宿主自管 edge overlay。
6. runtime decoration 保持在 `IGraphRuntimeOverlayProvider` 和 inspector snapshots 上；不要把图执行搬进 editor。

## 相关文档

- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Integration](./host-integration.md)
- [Advanced Editing Guide](./advanced-editing.md)
