# Custom Node Host Recipe

这份 recipe 展示的是宿主视角的自定义节点定义注册、带分组和校验的端口声明、默认尺寸设置、通过 `IGraphNodeVisualPresenter` 替换节点视觉树，以及为边几何提供 `PortAnchors` 的完整路线。

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

## 4. 替换节点视觉树

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

## 5. PortAnchors、TargetAnchors 与边几何

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

## 6. Proof 验证

用受防守的 proof run 收口自定义节点 handoff：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
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
3. 实现 `IGraphNodeVisualPresenter` 做自定义视觉树
4. 在 `GraphNodeVisual.PortAnchors` 里填入 port-id 到 control 的映射
5. 需要 typed parameter endpoints 时，在 `GraphNodeVisual.ConnectionTargetAnchors` 里填入目标锚点
6. 当 stock styling 不够时，用 `GetConnectionGeometrySnapshots()` 渲染自定义 edge badge 或 label
7. 把 presenter 接入 `AsterGraphPresentationOptions.NodeVisualPresenter`
8. 用 `ConsumerSample.Avalonia -- --proof` 验证，期待 `AUTHORING_SURFACE_OK:True` 和 `CUSTOM_EXTENSION_SURFACE_OK:True`

## 相关文档

- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [节点展示指南](./node-presentation-guidelines.md)
- [Host Integration](./host-integration.md)
- [Host Recipe 阶梯](./host-recipe-ladder.md)
