# Host Integration 指南

这份文档只展开受支持的宿主路线，不把第一次接入和维护者 proof 文档混在一起。canonical 路线保持 session-first/runtime-first；新接入默认先走 Avalonia-first；retained MVVM 只作为迁移期的兼容桥接。

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。

## Canonical Routes

1. 只要运行时 / 自定义 UI  
   `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
2. 使用默认 Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. thin hosted builder
   `AsterGraphHostBuilder.Create(...).BuildAvaloniaView()`
4. retained 迁移桥接
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

前两条是新代码的 canonical surface。第 3 条只是第 2 条上的薄便利 facade，不是第四套 runtime model。第 4 条仍然受支持，但只作为迁移期保留的 compatibility bridge。
只有在现有宿主要分批迁移时才选 retained。需要这座桥接时，先看 [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)；否则优先从第 1 条、第 2 条或第 2 条上的 builder 开始。

新接入默认走第 2 条（`AsterGraphAvaloniaViewFactory`），这样 WPF 保持 adapter-2 portability validation-only，不会变成单独上手路径或 parity 承诺。

如果宿主管的是自己的 UI，那么第 1 条就是 canonical 的原生 / 自定义 UI 路线；你是在同一个 session/runtime owner 上组合自己的表面，而不是再引入一套第二模型。

`AsterGraphCanvasViewFactory`、`AsterGraphInspectorViewFactory`、`AsterGraphMiniMapViewFactory` 这些独立表面都属于第 2 条路线下的组合细节，不是第四条主路线。

## Hosted Builder Cookbook

当宿主想走常见 Avalonia 组合，并且可以一次性传入核心 inputs 时，用 builder：

```csharp
var view = AsterGraphHostBuilder
    .Create()
    .UseDocument(document)
    .UseCatalog(catalog)
    .UseDefaultCompatibility()
    .UseDefaultWorkbench()
    .UsePluginTrustPolicy(pluginTrustPolicy)
    .UseLocalization(localization)
    .UseDiagnostics(diagnostics)
    .UseRuntimeOverlayProvider(runtimeOverlayProvider)
    .UseLayoutProvider(layoutProvider)
    .BuildAvaloniaView();
```

`UseDefaultWorkbench()` 是 hosted Avalonia 的便利层，用于 stock toolbar、command palette、stencil、inspector、mini-map、fragment、diagnostics 和 status chrome。它仍然停在现有 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` 路线，不会创建第二套 runtime model。

默认 workbench 使用 `AsterGraphWorkbenchPerformanceMode.Balanced`。宿主可以通过 `AsterGraphWorkbenchOptions.PerformanceMode` 选择 `Quality`、`Balanced` 或 `Throughput`，用于调节 stencil card 数量限制、mini-map 投影节奏、advanced inspector 投影、hover toolbar 和 command refresh batching 等 hosted chrome 行为。这只是 Avalonia hosted policy，不是 runtime execution 或 graph-model contract。

builder 还提供针对稳定 `AsterGraphEditorOptions` seam 的窄透传：`UseBehaviorOptions(...)`、`UseContextMenuAugmentor(...)`、`UseNodePresentationProvider(...)`、`UseToolProvider(...)`、`UseRuntimeOverlayProvider(...)` 和 `UseLayoutProvider(...)`。当常见 hosted Avalonia 路线仍然正确、宿主只需要这些现有 editor seam 时使用它们。宿主需要这个列表之外的服务、storage/export 组合或 standalone surfaces 时，再降到显式 factory wiring。

`UseNodePresentationProvider(...)` 透传的是 `AsterGraphEditorOptions.NodePresentationProvider`，用于给节点提供 editor-runtime presentation state。Avalonia visual replacement 仍然放在 `AsterGraphPresentationOptions`，包括 `NodeVisualPresenter`、`NodeBodyPresenter`、inspector、mini-map、context-menu 和 parameter-editor presenters。

当宿主需要分别拥有 editor options、view options 或 standalone surfaces 时，用显式 factory wiring：

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibility,
    PluginTrustPolicy = pluginTrustPolicy,
});

var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
});
```

builder 会委托给现有 editor/session 和 Avalonia view factories。`CreateSession(...)` 仍是 canonical runtime-only 路线，retained surfaces 仍只用于迁移。

## 宿主自管 Runtime Feedback

如果宿主已经拥有图运行或模拟运行逻辑，可以传入 `AsterGraphEditorOptions.RuntimeOverlayProvider`，并通过 `IGraphEditorQueries.GetRuntimeOverlaySnapshot()` 读取当前投影。AsterGraph 只暴露用于节点 / 连接状态、payload preview 和 recent logs 的 `GraphEditorRuntimeOverlaySnapshot`；它不执行图，也不拥有 workflow engine。

## 宿主自管 Layout Plans

如果宿主自己拥有布局算法，可以传入 `AsterGraphEditorOptions.LayoutProvider`，并通过 `IGraphEditorQueries.CreateLayoutPlan(...)` 请求可预览的 plan。返回的 `GraphLayoutPlan` 只描述建议节点位置和 route-reset 意图；创建 plan 不会修改文档，也不会让 AsterGraph 依赖某一个固定布局引擎。

## 何时选择 retained

| 路线 | 适用时机 | 不适用时机 | recipe |
| --- | --- | --- | --- |
| 运行时 / 自定义 UI | 当你在开始新工作或宿主自己拥有 UI 时，请使用 `CreateSession(...)`。 | 不要在需要 shipped Avalonia 壳层或 retained 桥接时使用这条路线。 | [快速开始](./quick-start.md) |
| shipped Avalonia | 当你想使用 shipped Avalonia 路线时，请使用 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`。 | 不要在保留旧的 `GraphEditorViewModel` 宿主时使用这条路线。 | [快速开始](./quick-start.md) |
| thin hosted builder | 常见 Avalonia 路线够用、并且你想少写组合样板时，使用 `AsterGraphHostBuilder.Create(...).BuildAvaloniaView()`。 | 不要在宿主自己管 UI 或只需要 runtime session 时使用这条路线。 | [快速开始](./quick-start.md) |
| retained 迁移桥接 | 只有在现有宿主已经构造 `GraphEditorViewModel` 或 `GraphEditorView` 且正在分批迁移时才使用 retained。 | 不要把它用在新宿主工作、第四条主路线或 WPF 专属 runtime model。 | [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md) |

唯一的 retained recipe 是 [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)。
retained 仍然只用于迁移，不会新增新的兼容性承诺。retained 不是第四条主路线。如果这座桥接确实有必要，就把维护者导向这一个 bounded 的 retained recipe 集合，而不是让他们拼接多份文档。

包级支持层级请看 [Public API Inventory](./public-api-inventory.md)。它把 stable canonical、supported hosted helper、retained migration、compatibility-only 和 internal-only surface 分类，但不会创建另一条接入路线。

## Consumer Route Matrix

| 需求 | 起始包 | canonical 入口 | 第一个样例 |
| --- | --- | --- | --- |
| 仅运行时 / 自定义 UI | `AsterGraph.Editor`（自定义节点时再加 `AsterGraph.Abstractions`） | `CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| 默认 Avalonia UI | `AsterGraph.Avalonia` | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HelloWorld.Avalonia` |
| plugin trust / discovery | `AsterGraph.Editor` | `DiscoverPluginCandidates(...)` + `PluginTrustPolicy` | `tools/AsterGraph.ConsumerSample.Avalonia` |
| automation | `AsterGraph.Editor` | `IGraphEditorSession.Automation.Execute(...)` | `src/AsterGraph.Demo` |
| runtime feedback overlay | `AsterGraph.Editor` | `AsterGraphEditorOptions.RuntimeOverlayProvider` + `IGraphEditorQueries.GetRuntimeOverlaySnapshot()` | `src/AsterGraph.Demo` |
| layout plans | `AsterGraph.Editor` | `AsterGraphEditorOptions.LayoutProvider` + `IGraphEditorQueries.CreateLayoutPlan(...)` | `tools/AsterGraph.ConsumerSample.Avalonia` |
| retained 迁移桥接 | `AsterGraph.Editor`（嵌入 `GraphEditorView` 时再加 `AsterGraph.Avalonia`） | retained constructor 路线 | 仅用于旧宿主迁移 |

如果你在做新工作，请先看 [Quick Start](./quick-start.md)，retained 路线只保留给旧宿主迁移。

## 宿主自管参数与元数据复制图

按每个 bounded source 复制它负责的那一部分：

- 从 `CreateSession(...)` 复制：给自定义 UI 宿主使用的 host-owned runtime/session 投影。
- 从 `AsterGraphHostBuilder` 复制：当 document、catalog、compatibility、plugin trust、localization、diagnostics、behavior、menu augmentation、node-presentation state、tools、runtime overlay 和 layout inputs 已经足够时，用它完成常见 hosted Avalonia 组合。
- 从 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` 复制：给 hosted UI 宿主使用的 shipped Avalonia 组合。
- 从 `ConsumerSample.Avalonia` 复制：只复制 action projection、trust workflow 和选中节点参数读写 seam。
- 从 `Authoring Inspector Recipe` 复制：definition-driven 的参数元数据和 stock inspector 词汇。
- 从 [Authoring Surface Recipe](./authoring-surface-recipe.md) 复制：hosted Avalonia 路线下从节点旁路编辑器、validation 到共享 command 与 proof 的整条 handoff。
- 把分工保持明确：样例负责 seam 证明，recipe 负责元数据源。更具体地说，inspector recipe 负责元数据词汇，surface recipe 负责 hosted authoring handoff。

## 样例分工

- `AsterGraph.HelloWorld` = 仅运行时第一跑样例
- `AsterGraph.HelloWorld.Avalonia` = 默认 Avalonia UI 第一跑样例
- `AsterGraph.Starter.Avalonia` = 默认 Avalonia 路线的 starter scaffold
- `AsterGraph.ConsumerSample.Avalonia` = canonical 路线上的中等复杂度 hosted-UI 样例，展示宿主动作、参数编辑和一个可信插件
- `AsterGraph.Starter.Wpf` = validation-only adapter-2 组合验证样例，不是上手路线
- `AsterGraph.HelloWorld.Wpf` = validation-only adapter-2 的 WPF proof marker 样例，不代表 parity
- `AsterGraph.HostSample` = 推荐的仅运行时和默认 UI 两条路线的窄范围验证样例
- `AsterGraph.PackageSmoke` = 打包消费验证
- `AsterGraph.ScaleSmoke` = 公开的大图基线加 history/state 验证
- `AsterGraph.Demo` = 用于可视检查的完整展示宿主

## 第二适配器合同

当前公开 beta 线已经锁定 `WPF` 作为 adapter 2。当前真正 shipped 的 hosted adapter 仍然只有 Avalonia；第二适配器里程碑要验证的是同一条 canonical 路线上的可移植性，而不是引入 adapter 专属 runtime API，也不是给新用户提供第二条上手路径。

后续 Avalonia/WPF 文档统一使用 [Adapter Capability Matrix](./adapter-capability-matrix.md) 里的 vocabulary：

| Label | 含义 |
| --- | --- |
| `supported` | 该 adapter 已经能通过当前文档化的主路线提供对应 stock surface |
| `partial` | 能力仍然建立在同一条 canonical 路线上，但该 adapter 还存在明确范围限制或缺少 stock projection |
| `fallback` | 宿主继续停留在同一条 canonical session/runtime seam 上，改走更底层、已文档化的 path / sample / proof harness |

retained 迁移不是 `fallback`。它仍然只是 legacy host 的 compatibility bridge。

- `WPF partial` 表示宿主仍停留在 canonical session/runtime 路线上，用 session/query snapshot 做 `host-owned` projection 去补齐缺失的 stock surface；这是 validation-only，不是 parity。
- `WPF fallback` 表示宿主仍停留在 canonical session/runtime 路线上，沿已文档化的 proof/sample path 做 `host-owned` projection；这是 validation-only，不是 parity。
- 这条 fallback 规则本身不会转向 retained MVVM，也不会引入 `adapter-specific` runtime API。

## Official Capability Modules

把 `Official Capability Modules` 理解成压在 canonical routes 之上的宿主能力地图，而不是另一套接入路线。
当 feature 需要记录 status、adapter projection、proof marker 和 performance budget 时，统一看 [Feature Catalog](./feature-catalog.md)。

| Module | Canonical seam | Hosted/UI 说明 | 第一个 proof / sample 锚点 |
| --- | --- | --- | --- |
| `Selection` | `SetSelection(...)` + `GetSelectionSnapshot()` | 第 2 条路线只是把同一份 selection state 投影到 shipped visuals | `AsterGraph.ScaleSmoke`、`AsterGraph.HelloWorld` |
| `History` | `Undo()` / `Redo()` 加 save/dirty 契约 | hosted shell 复用同一个 kernel-owned history 边界 | `AsterGraph.ScaleSmoke`、[State Contracts](./state-contracts.md) |
| `Clipboard` | `TryCopySelectionAsync()` / `TryPasteSelectionAsync()` | 底层 seam 仍然是宿主 clipboard service | `AsterGraph.HostSample` |
| `Shortcut Policy` | `AsterGraphCommandShortcutPolicy` | Avalonia 路线上的组合开关，但仍然属于官方 hosted route | `AsterGraph.PackageSmoke`、`AsterGraph.HelloWorld.Avalonia` |
| `Layout` | session align/distribute commands | snapline 和视觉 guide 继续留在 adapter 层 | `AsterGraph.Demo` |
| `MiniMap` | session/viewport snapshots + `AsterGraphMiniMapViewFactory.Create(...)` | 属于第 2 条路线下的 standalone surface，不是独立路线 | `AsterGraph.Demo` |
| `Stencil` | session stencil discovery + insertion commands | shipped Avalonia surface 消费同一份 session discovery 数据 | `AsterGraph.Demo` |
| `Fragment Library` | 由 fragment workspace/library service 支撑的 session fragment/template commands | 宿主可替换存储，但不需要重写 command surface | `AsterGraph.Demo` |
| `Export` | `IGraphSceneSvgExportService` + `TryExportSceneAsSvg()`，以及 raster SVG/PNG/JPEG export 的 `GraphEditorSceneImageExportOptions` progress/cancel/scope options | export 和 workspace persistence、fragment storage 明确分离 | `AsterGraph.HostSample`、`AsterGraph.ScaleSmoke` |
| `Baseline Edge Authoring` | connection start/complete/reconnect/disconnect commands 加 pending snapshot | pointer gesture 是 adapter 行为，底层仍复用同一份 session semantics | `AsterGraph.HostSample`、`AsterGraph.ScaleSmoke` |
| `Node Surface Authoring` | `GetNodeSurfaceSnapshots()`、`TrySetNodeSize(...)` 以及走共享 session command 路径的参数编辑 | Avalonia 会把同一份 tier state 投影成卡片阈值、节点旁路参数编辑器和 stock authoring chrome | `AsterGraph.Demo`、[Advanced Editing Guide](./advanced-editing.md) |
| `Hierarchy Semantics` | `GetHierarchyStateSnapshot()`、`GetNodeGroups()`、`GetNodeGroupSnapshots()` 以及 group collapse/move/resize/membership commands | stock canvas 在同一份 hierarchy state 上叠加 frame chrome、content-area membership 和 collapse affordance | `AsterGraph.Demo`、[Advanced Editing Guide](./advanced-editing.md) |
| `Composite Scope Authoring` | wrap/promote/expose/unexpose/scope-navigation commands 加 scope/composite queries | breadcrumb chrome 和 host-owned workflow controls 复用同一份 session navigation state | `AsterGraph.Demo`、[Advanced Editing Guide](./advanced-editing.md) |
| `Edge Semantics` | canonical session 路线上的连线注释、reconnect 和 disconnect commands | hosted pointer flow 和 menu 只是同一份 edge semantics 的投影 | `AsterGraph.Demo`、[Advanced Editing Guide](./advanced-editing.md) |
| `Edge Geometry Tooling` | `GetConnectionGeometrySnapshots()` 加 route-vertex insert/move/remove commands | stock authoring tools 会投影几何编辑，而不会引入第二套 edge model | `AsterGraph.Demo`、[Advanced Editing Guide](./advanced-editing.md) |

## 状态契约

面向宿主的 save/history/dirty 规则见 [State Contracts](./state-contracts.md)。

短版：

- save 建立 clean baseline
- undo 离开保存态后会变 dirty
- redo 回到保存态后会恢复 clean
- no-op interaction 不能制造伪 dirty / undo 状态
- retained 与 runtime mutation 共享同一个 kernel-owned history/save authority，但新接入仍应先走 canonical session 路线

## 导出与持久化的边界

建议把下面三条宿主 seam 明确区分开：

- 工作区持久化：`IGraphWorkspaceService` 负责完整可编辑图状态的保存/加载；工作区文档契约见 [序列化契约](./serialization-contracts.md)
- 片段持久化：fragment workspace + fragment library services 负责可复用的选择片段载荷
- 场景导出：`IGraphSceneSvgExportService` 负责基于 `IGraphEditorSession.Queries.GetSceneSnapshot()` 生成非工作区的 SVG 输出；raster PNG/JPEG image export 可以上报 `GraphEditorSceneImageExportProgressSnapshot`，遵守 `GraphEditorSceneImageExportOptions.CancellationToken`，并显式限定为 full scene 或 selected nodes

内置导出 seam 不属于 workspace save/load，也不替代 fragment/template 流程。

## 扩展契约

稳定性与优先级规则见 [Extension Contracts](./extension-contracts.md)。

重要默认项：

- 推荐的 canonical runtime 入口是 `CreateSession(...)`、`IGraphEditorSession` 和 DTO/snapshot queries
- `Create(...)` 仍然是受支持的 hosted Avalonia 组合 helper，而 `Editor.Session` 仍然是这条路线背后的共享运行时 owner
- retained `GraphEditorViewModel` / `GraphEditorView` 仍是受支持的 migration bridge，并且已经被明确标成兼容性桥接 surface
- host localization 在 plugin localization 之后执行，所以 host override 最终生效
- plugin command 现在通过 canonical session command descriptor 暴露，并通过 `IGraphEditorSession.Commands.TryExecuteCommand(...)` 执行
- retained augmentor composition 仍然和 runtime 路线不同；新代码优先走运行时路线，把 retained 路线当作桥接即可

## 插件信任边界

插件信任由宿主负责。AsterGraph 可以帮助你发现候选、做 allow/block 策略、检查结果，但不提供沙箱或不受信任代码隔离。

更细的 v1 插件清单与 trust-policy 合同见 [插件信任契约 v1](./plugin-trust-contracts.md)。

更深的验证、CI lane 和 release gate，请看 [CONTRIBUTING.md](../../CONTRIBUTING.md) 和 [Public Launch Checklist](./public-launch-checklist.md)。

## Recipes

- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)
- [ScaleSmoke 基线](./scale-baseline.md)
