# Host Recipe 阶梯

这是面向 AsterGraph 宿主的统一“从这里复制”阶梯。

按顺序执行。每一步只增加一条 bounded seam；后面的步骤不会推翻前面的结果。

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。

## 第 1 步：Starter.Avalonia — 最小端到端脚手架

先跑这个，确认第一条 hosted Avalonia 路线能通。

```powershell
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo
```

### 复制这个

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphEditorOptions`
- document/catalog/editor/view 的组合流程

### 替换这个

- 宿主自己的 top-level window 和它的 title/size
- 随着宿主成长逐步替换 sample graph/catalog definitions

### Proof Handoff

壳层能打开后，再去看最小 stock sample。

## 第 2 步：HelloWorld.Avalonia — 最小 shipped Avalonia surface

跑这个来确认 shipped Avalonia surface 在没有额外宿主逻辑时也能工作。

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo
```

### 复制这个

- 和第 1 步相同的 factory 组合
- 如果宿主需要一个极简窗口，可复制最小 menu 和 shell chrome

### 替换这个

- 把样例 node family 换成自己的 catalog definitions
- 替换 window chrome 和 narrative text

### Proof Handoff

确认无误后，进入真实宿主 proof。

## 第 3 步：ConsumerSample.Avalonia — 真实宿主 proof

跑这个，在受防守的路线上验证 host-owned actions、trusted-plugin flow、参数编辑、command projection 和 hosted accessibility semantics。

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

### 复制这个

- Action rail / command projection：查询 `session.Queries.GetCommandDescriptors()`，并通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 和 `AsterGraphHostedActionFactory.CreateProjection(...)` 投影
- Plugin trust workflow：`GraphEditorPluginDiscoveryOptions`、`AsterGraphEditorOptions.PluginTrustPolicy`、provenance snapshots，以及显式的宿主自管 allowlist policy
- 选中节点参数读写 seam：`IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 负责读，`IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 负责写

### 替换这个

- review/audit node family
- action ids 和 titles
- window layout 和 narrative text
- 超出 defended markers 的 proof labels

### Proof Handoff

期待看到以下 markers：

- `CONSUMER_SAMPLE_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`
- `HOST_NATIVE_METRIC:*`

如果 `CONSUMER_SAMPLE_PARAMETER_OK` 或 `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` 失败，就把失败的 proof-marker 行和 support bundle 的 `parameterSnapshots` 行一起保留在同一条受限 intake 记录里。

如需正式 intake 记录，加上 `--support-bundle <support-bundle-path>` 运行，并复用输出的 `SUPPORT_BUNDLE_PATH:...` 行。详见 [Beta Support Bundle](./support-bundle.md)。

## 第 4 步：阶梯之后的 proof harness

只有在真实宿主样例已经看懂之后，再跑 `HostSample`。

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
```

期待：

- `HOST_SAMPLE_OK:True`
- `HOST_SAMPLE_AUTOMATION_OK:True`
- `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`
- `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True`

## 按 seam 的交叉链接

| Seam | 从哪里复制 | 详细 recipe |
| --- | --- | --- |
| Factory 组合 | `Starter.Avalonia` / `HelloWorld.Avalonia` | [快速开始](./quick-start.md) |
| Plugin discovery/trust | `ConsumerSample.Avalonia` | [Plugin Host Recipe](./plugin-host-recipe.md) |
| 自定义 node/port/edge | `ConsumerSample.Avalonia` | [Custom Node Host Recipe](./custom-node-host-recipe.md) |
| 参数元数据 | Inspector 词汇表 | [Authoring Inspector Recipe](./authoring-inspector-recipe.md) |
| Node surface authoring | `ConsumerSample.Avalonia` | [Authoring Surface Recipe](./authoring-surface-recipe.md) |
| Retained 迁移 | 现有 `GraphEditorViewModel` 宿主 | [Retained 迁移 Recipe](./retained-migration-recipe.md) |
| Accessibility handoff | `ConsumerSample.Avalonia` | [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md) |
| 性能预算 | `ScaleSmoke` | [ScaleSmoke 基线](./scale-baseline.md) |

## 相关文档

- [快速开始](./quick-start.md)
- [公开 Beta 评估路径](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Integration](./host-integration.md)
