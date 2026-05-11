# Custom Node Host Recipe

这份 recipe 展示的是宿主视角的自定义节点定义注册、带分组和校验的端口声明、默认尺寸设置、选择 `NodeBodyPresenter` 或 `NodeVisualPresenter`、通过 `NodeDragHandle` 标记拖拽手柄，以及为边几何提供 anchors 的完整路线。

当宿主管着 node catalog 并想在 canonical Avalonia 路线上做自定义展示时，使用本文档。

更小的插件作者路线见 [Plugin 与自定义节点 Recipe](./plugin-recipe.md)。完整的 authoring surface 说明见 [Authoring Surface Recipe](./authoring-surface-recipe.md)。

## 包

```powershell
dotnet add package AsterGraph.Abstractions --prerelease
dotnet add package AsterGraph.Avalonia --prerelease
```

如果是仅运行时或自定义 UI 宿主，再加 `AsterGraph.Editor`。

## 1. 注册定义

在 catalog 里定义节点，或者通过插件的 `INodeDefinitionProvider` 注册。

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

注册 provider：

```csharp
// 直接组合 catalog
var catalog = new NodeCatalog();
catalog.Register(new MyNodeProvider());

// 或者通过插件
builder.AddNodeDefinitionProvider(new MyNodeProvider());
```

## 2. 声明端口

端口规则：

- `key` 在节点输入集合、输出集合以及两者合并后都必须唯一
- `displayName` 是可见标签
- `typeId` 通过宿主的 `IPortCompatibilityService` 驱动兼容性校验
- `accentHex` 设置端口圆点颜色

校验是自动的：传入重复 port key 时 `NodeDefinition` 会抛出异常。

## 3. 设置默认尺寸

在 `NodeDefinition` 上设置 `defaultWidth` 和 `defaultHeight`：

```csharp
defaultWidth: 280d,
defaultHeight: 180d
```

默认画布会以此作为初始尺寸。宿主之后可通过 `IGraphEditorSession.Commands.TrySetNodeSize(...)` 或 `TrySetNodeWidth(...)` 改变尺寸。

## 4. 选择 presentation seam

先选择能满足需求的最小 presentation seam：

| 需求 | 使用 | 继续由 stock 路线负责 |
| --- | --- | --- |
| 保留 stock shell、标题、端口、selection chrome、resize/drag 行为，只替换 body 内容 | `AsterGraphPresentationOptions.NodeBodyPresenter` / `IGraphNodeBodyPresenter` | `DefaultGraphNodeVisualPresenter`、`GraphNodeVisual.PortAnchors`、stock node drag、stock committed-edge rendering |
| 替换整棵节点视觉树，包括自定义端口位置和 pointer routing | `AsterGraphPresentationOptions.NodeVisualPresenter` / `IGraphNodeVisualPresenter` | runtime/session contracts、connection geometry snapshots、persistence、undo/redo |

当自定义节点主要是标准 AsterGraph 节点框架内的自定义内容时，优先使用 `NodeBodyPresenter`。只有当宿主必须拥有完整 root visual、port controls、layout 或 pointer behavior 时，才使用 `NodeVisualPresenter`。

### Stock shell body 自定义

当你想要 React Flow-style custom body，但仍保留 stock AsterGraph shell 时，实现 `IGraphNodeBodyPresenter`：

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
        // 从 context.Node 刷新 body-only state。
    }
}
```

接入 body presenter，不替换完整节点视觉 presenter：

```csharp
Presentation = new AsterGraphPresentationOptions
{
    NodeBodyPresenter = new MyNodeBodyPresenter(),
}
```

拖拽手柄边界：

- `NodeDragHandle.SetIsDragHandle(control, true)` 标记 stock shell 内可启动节点拖拽的 control。
- 如果 stock-shell 节点里存在任何已标记 drag handle，节点拖拽只能从该 handle 或其 descendants 启动。
- 未标记的 body 内容仍可用于文本选择、按钮、滑块、parameter editors 或其他宿主自有交互。
- 完整 `NodeVisualPresenter` 替换拥有自己的 root pointer routing；如果要复用 stock-shell 规则，可以读取同一个 attached property。

## 5. 替换完整节点视觉树

实现 `IGraphNodeVisualPresenter` 来替换视觉树：

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

        // 从 context.Node 更新 title、port labels、parameter rows
    }

    private static bool ShouldUseCustomVisual(NodeViewModel node)
        => node.DefinitionId == "myhost.review";
}
```

将其接入 hosted view：

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

生命周期边界：

- `Create(...)` 创建 root control、anchor dictionaries 和可选 presenter state。
- `Update(...)` 从最新 `GraphNodeVisualContext` 刷新文本、参数行、工具栏状态和 anchor dictionaries。
- 自定义 presenter 对不归自己管理的节点应委托给 `DefaultGraphNodeVisualPresenter`。
- presenter state 归宿主管；持久化图变化仍然走 `IGraphEditorSession.Commands`。

## 6. PortAnchors、TargetAnchors 与边几何

`GraphNodeVisual.PortAnchors` 是默认画布用于已提交连线的锚点映射。

规则：

- Key 是端口 id（来自 `PortViewModel` 的 `port.Id`）
- Value 是 `Control` 实例（通常是小圆点或按钮）
- 画布从这些控件的位置读取锚点坐标来绘制边端点
- 自定义展示器必须在 `Create` 时填充该字典，并在 `Update` 时保持同步

`GraphNodeVisual.ConnectionTargetAnchors` 是参数端点和其他非端口连接目标的 typed anchor map。使用 `GraphConnectionTargetRef` 作为 key，并在目标控件上调用 `context.ActivateConnectionTarget(...)` 来启动该 endpoint 的连接。

如需自定义边展示，从 session 查询几何数据：

```csharp
var geometries = session.Queries.GetConnectionGeometrySnapshots();
```

每个 snapshot 包含源/目标锚点位置和 route vertices。宿主可据此渲染宿主自有的边覆盖层，而无需改动 runtime 路线。

受支持的自定义边路径是 stock edge styling 加可选的、基于 geometry snapshots 的宿主自管 overlay。这里没有 public `IGraphEdgeVisualPresenter`；`NodeCanvas` 的内部层（包括 `OverlayLayer`）也不是这条 recipe 的一部分。

## 7. Proof 验证

用受防守的 proof run 收口自定义节点 handoff：

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof
```

期待：

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

## 复制路径总结

1. 用 `NodeDefinition` 定义输入、输出、参数、`defaultWidth` 和 `defaultHeight`
2. 在 catalog 里注册 provider，或通过插件注册
3. 当 stock shell 应继续拥有 title、ports、resize、drag 和 selection chrome 时，使用 `IGraphNodeBodyPresenter` 加 `AsterGraphPresentationOptions.NodeBodyPresenter`
4. 当只有 body 的一部分应启动节点拖拽时，用 `NodeDragHandle.SetIsDragHandle(control, true)` 标记 body drag handle
5. 只有宿主必须替换完整视觉树时，才使用 `IGraphNodeVisualPresenter` 加 `AsterGraphPresentationOptions.NodeVisualPresenter`
6. 在完整 visual replacement 中，用 `GraphNodeVisual.PortAnchors` 填入 port-id 到 control 的映射
7. 需要 typed parameter endpoints 时，在 `GraphNodeVisual.ConnectionTargetAnchors` 里填入目标锚点
8. 当 stock styling 不够时，用 `GetConnectionGeometrySnapshots()` 渲染自定义 edge badge 或 label
9. 用 `src/AsterGraph.Demo -- --proof` 验证，期待 `AUTHORING_SURFACE_OK:True` 和 `CUSTOM_EXTENSION_SURFACE_OK:True`

## 相关文档

- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [节点展示指南](./node-presentation-guidelines.md)
- [Host Integration](./host-integration.md)
- [Host Recipe 阶梯](./host-recipe-ladder.md)
