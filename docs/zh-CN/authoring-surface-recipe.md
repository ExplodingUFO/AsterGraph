# Authoring Surface Recipe

当你想在 canonical session 路线上，把自定义节点、端口、参数编辑器和边展示收成一条可复制的宿主自管路径时，就看这份 recipe。

真正承载这条 recipe 代码的是 `ConsumerSample.Avalonia`。元数据词汇仍由 [Authoring Inspector Recipe](./authoring-inspector-recipe.md) 负责，而宿主边界和 proof 位置仍由 [Consumer Sample](./consumer-sample.md) 负责。

## 可复制的自定义 authoring presentation

这条 recipe 继续停留在 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` 上，并且复用同一个 `IGraphEditorSession` owner。它不会引入 adapter-specific runtime forks。

## PRES-01：多 Handle 节点

先用 `NodeDefinition` 把多输入、多输出、默认宽高这些事实声明出来，再通过 `IGraphNodeVisualPresenter` 投成宿主自定义节点可视树。

- `ConsumerSampleHost.CreateReviewDefinition()` 把多个输入端口、多个输出端口、`defaultWidth` 和 `defaultHeight` 这些事实放在同一处。
- `ConsumerSampleNodeVisualPresenter` 只负责把这些 `NodeDefinition` 事实变成可替换的 UI，不改变底层 runtime/session contract。
- `GraphNodeVisual.PortAnchors` 仍然是 stock canvas 计算 committed connections 所需的唯一锚点图。

复制时保持这条分工：

- 多 handle 事实继续留在 `NodeDefinition`
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

## 复制顺序

1. 先在 `NodeDefinition` 里定义节点和参数事实，包括多输入/多输出、`defaultWidth`、`defaultHeight` 以及参数 `templateKey`。
2. 再按 `ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions()` 的形状，把 wiring 接到 `AsterGraphAvaloniaViewOptions.Presentation`。
3. 通过 `IGraphNodeVisualPresenter` 替换节点可视树。
4. 通过 `INodeParameterEditorRegistry` 替换 editor body。
5. 通过 `GetConnectionGeometrySnapshots()` 渲染宿主自管 edge overlay。

## 相关文档

- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Integration](./host-integration.md)
- [Advanced Editing Guide](./advanced-editing.md)
