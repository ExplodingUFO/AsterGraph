# Authoring Inspector Recipe

当你希望 shipped inspector 直接承担大部分 authoring 工作时，用这份 recipe。

## 你需要定义什么

从 `NodeParameterDefinition` 元数据开始：

- `editorKind` 决定内建 editor 类型
- `defaultValue` 作为新节点和投影回退值
- `constraints` 定义校验与只读约束
- `groupName` 决定 inspector 分组
- `placeholderText` 提供简短输入提示

当前 shipped editor kinds：

- `Text`
- `Number`
- `Boolean`
- `Enum`
- `Color`
- `List`

## shipped inspector 会自动做什么

当宿主使用默认 Avalonia surfaces 时：

- 参数会从当前选中节点定义投影出来
- 多选且共享定义时可以直接 batch edit
- 校验错误会直接显示在 inspector 中
- 只读约束会明确展示
- `List` 参数使用一行一个项的 multiline editor

## 最小定义示例

```csharp
var definition = new NodeDefinition(
    new NodeDefinitionId("sample.authoring.node"),
    "Authoring Node",
    "Samples",
    "Inspector",
    [],
    [],
    parameters:
    [
        new NodeParameterDefinition(
            "threshold",
            "Threshold",
            new PortTypeId("float"),
            ParameterEditorKind.Number,
            defaultValue: 0.5d,
            constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
            groupName: "Behavior"),
        new NodeParameterDefinition(
            "slug",
            "Slug",
            new PortTypeId("string"),
            ParameterEditorKind.Text,
            defaultValue: "authoring-node",
            constraints: new ParameterConstraints(
                MinimumLength: 3,
                ValidationPattern: "^[a-z-]+$",
                ValidationPatternDescription: "lowercase letters and dashes"),
            groupName: "Metadata",
            placeholderText: "authoring-node"),
        new NodeParameterDefinition(
            "tags",
            "Tags",
            new PortTypeId("string-list"),
            ParameterEditorKind.List,
            defaultValue: new[] { "alpha", "beta" },
            constraints: new ParameterConstraints(MinimumItemCount: 1, MaximumItemCount: 5),
            groupName: "Metadata",
            placeholderText: "one tag per line"),
    ]);
```

## 去哪里看现成效果

- 最小默认 UI 样例：[`tools/AsterGraph.HelloWorld.Avalonia`](../../tools/AsterGraph.HelloWorld.Avalonia/)
- 完整展示宿主：[`src/AsterGraph.Demo`](../../src/AsterGraph.Demo/)
- 更真实的宿主集成：[Consumer Sample](./consumer-sample.md)

## 什么时候该自己扩展

如果宿主只需要常见 authoring 控件和可预期的校验，优先继续用 shipped inspector。

只有当你需要下面这些能力时，再在它上面做自定义呈现：

- 领域专用的复合 editor
- 不适合拆成单字段编辑的跨参数工作流
- 需要把图编辑和业务 review UI 混在一起的宿主自有面板
