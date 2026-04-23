# Authoring Inspector Recipe

当你希望 shipped definition-driven inspector 直接承担大部分 authoring 工作时，用这份 recipe。
这份 recipe 是元数据词汇的唯一 owner。

## 宿主自管参数与元数据复制图

当你需要时复制这份 recipe；它提供 canonical 的元数据词汇和 stock inspector 行为。

- 当你需要 `defaultValue`、`isAdvanced`、`helpText`、`placeholderText`、`constraints.IsReadOnly`、`editorKind`、`constraints` 和 `groupName` 时，复制这份 recipe。
- 从 `ConsumerSample.Avalonia` 复制：选中节点参数读写 seam 和宿主自管 proof 边界。
- 把样例和这份 recipe 分开：样例证明 seam，recipe 承载元数据词汇。

第 1 步是这份 recipe 里的 metadata；第 2 步是 `ConsumerSample.Avalonia` 里的选中节点 seam；第 3 步是 support-bundle 的 `parameterSnapshots` 证据。

## 统一的 recipe 词汇

在文档、样例和节点定义里保持这些字段名一致：

- `defaultValue` 作为新节点和投影回退值
- `isAdvanced` 让高级参数默认保持折叠
- `helpText` 在字段旁提供简短说明
- `placeholderText` 为文本型 editor 提供简短输入提示
- 当宿主或定义锁定字段时，会明确显示只读原因

## 你需要定义什么

从 `NodeParameterDefinition` 元数据开始：

- `editorKind` 决定内建 editor 类型
- `constraints` 定义校验与只读约束
- `groupName` 决定 inspector 分组
- `helpText` 让短说明尽量贴近字段
- `isAdvanced` 标记需要默认折叠的高级参数
- 只读原因会在宿主或定义锁定字段时明确显示

shipped inspector 只保持在 definition-driven inspector 的边界内，不会变成通用 property framework。

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

## 可复制的定义示例

这个示例把 canonical metadata vocabulary 放在同一个 bounded inspector recipe 里：

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
            placeholderText: "authoring-node",
            helpText: "Used in filenames and automation labels."),
        new NodeParameterDefinition(
            "tags",
            "Tags",
            new PortTypeId("string-list"),
            ParameterEditorKind.List,
            defaultValue: new[] { "alpha", "beta" },
            constraints: new ParameterConstraints(MinimumItemCount: 1, MaximumItemCount: 5),
            groupName: "Metadata",
            placeholderText: "one tag per line"),
        new NodeParameterDefinition(
            "system-key",
            "System Key",
            new PortTypeId("string"),
            ParameterEditorKind.Text,
            defaultValue: "system-core",
            constraints: new ParameterConstraints(IsReadOnly: true),
            groupName: "Metadata",
            placeholderText: "system-core"),
        new NodeParameterDefinition(
            "debug-bias",
            "Debug Bias",
            new PortTypeId("float"),
            ParameterEditorKind.Number,
            defaultValue: 0.1d,
            helpText: "Used only for expert tuning.",
            groupName: "Advanced",
            isAdvanced: true),
    ]);
```

把 `defaultValue` 当成种子值，把 `helpText` 当成字段旁的说明，把 `placeholderText` 当成输入提示，把 `isAdvanced` 当成高级参数标记，把 `constraints.IsReadOnly` 当成必须锁定的定义约束。

## 去哪里看现成效果

- 最小默认 UI 样例：[`tools/AsterGraph.HelloWorld.Avalonia`](../../tools/AsterGraph.HelloWorld.Avalonia/)
- 完整展示宿主：[`src/AsterGraph.Demo`](../../src/AsterGraph.Demo/)
- 更真实的宿主集成：[Consumer Sample](./consumer-sample.md)
- 补充这份 recipe 的样例指导：[Consumer Sample](./consumer-sample.md)

## 什么时候该自己扩展

如果宿主只需要常见 authoring 控件和可预期的校验，优先继续用 shipped inspector。

只有当你需要下面这些能力时，再在它上面做自定义呈现：

- 领域专用的复合 editor
- 不适合拆成单字段编辑的跨参数工作流
- 需要把图编辑和业务 review UI 混在一起的宿主自有面板
