# Host Integration 指南

这份文档只展开受支持的宿主路线，不把第一次接入和维护者 proof 文档混在一起。canonical 路线保持 session-first/runtime-first；新接入默认先走 Avalonia-first；retained MVVM 只作为迁移期的兼容桥接。

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。

## Canonical Routes

1. 只要运行时 / 自定义 UI  
   `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
2. 使用默认 Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. retained 迁移桥接
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

前两条是新代码的 canonical surface。第 3 条仍然受支持，但只作为迁移期保留的 compatibility bridge。
只有在现有宿主要分批迁移时才选 retained。需要这座桥接时，先看 [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)；否则优先从第 1 条或第 2 条开始。

如果宿主管的是自己的 UI，那么第 1 条就是 canonical 的原生 / 自定义 UI 路线；你是在同一个 session/runtime owner 上组合自己的表面，而不是再引入一套第二模型。

`AsterGraphCanvasViewFactory`、`AsterGraphInspectorViewFactory`、`AsterGraphMiniMapViewFactory` 这些独立表面都属于第 2 条路线下的组合细节，不是第四条主路线。

## 何时选择 retained

| 路线 | 适用时机 | 不适用时机 | recipe |
| --- | --- | --- | --- |
| 运行时 / 自定义 UI | 当你在开始新工作或宿主自己拥有 UI 时，请使用 `CreateSession(...)`。 | 不要在需要 shipped Avalonia 壳层或 retained 桥接时使用这条路线。 | [快速开始](./quick-start.md) |
| shipped Avalonia | 当你想使用 shipped Avalonia 路线时，请使用 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`。 | 不要在保留旧的 `GraphEditorViewModel` 宿主时使用这条路线。 | [快速开始](./quick-start.md) |
| retained 迁移桥接 | 只有在现有宿主已经构造 `GraphEditorViewModel` 或 `GraphEditorView` 且正在分批迁移时才使用 retained。 | 不要把它用在新宿主工作、第四条主路线或 WPF 专属 runtime model。 | [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md) |

唯一的 retained recipe 是 [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)。
retained 仍然只用于迁移，不会新增新的兼容性承诺。retained 不是第四条主路线。如果这座桥接确实有必要，就把维护者导向这一个 bounded 的 retained recipe 集合，而不是让他们拼接多份文档。

## Consumer Route Matrix

| 需求 | 起始包 | canonical 入口 | 第一个样例 |
| --- | --- | --- | --- |
| 仅运行时 / 自定义 UI | `AsterGraph.Editor`（自定义节点时再加 `AsterGraph.Abstractions`） | `CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| 默认 Avalonia UI | `AsterGraph.Avalonia` | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HelloWorld.Avalonia` |
| plugin trust / discovery | `AsterGraph.Editor` | `DiscoverPluginCandidates(...)` + `PluginTrustPolicy` | `tools/AsterGraph.ConsumerSample.Avalonia` |
| automation | `AsterGraph.Editor` | `IGraphEditorSession.Automation.Execute(...)` | `src/AsterGraph.Demo` |
| retained 迁移桥接 | `AsterGraph.Editor`（嵌入 `GraphEditorView` 时再加 `AsterGraph.Avalonia`） | retained constructor 路线 | 仅用于旧宿主迁移 |

如果你在做新工作，请先看 [Quick Start](./quick-start.md)，retained 路线只保留给旧宿主迁移。

## 样例分工

- `AsterGraph.HelloWorld` = 仅运行时第一跑样例
- `AsterGraph.HelloWorld.Avalonia` = 默认 Avalonia UI 第一跑样例
- `AsterGraph.Starter.Avalonia` = 默认 Avalonia 路线的 starter scaffold
- `AsterGraph.ConsumerSample.Avalonia` = canonical 路线上的中等复杂度 hosted-UI 样例，展示宿主动作、参数编辑和一个可信插件
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
| `Export` | `IGraphSceneSvgExportService` + `TryExportSceneAsSvg()` | export 和 workspace persistence、fragment storage 明确分离 | `AsterGraph.HostSample` |
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
- 场景导出：`IGraphSceneSvgExportService` 负责基于 `IGraphEditorSession.Queries.GetSceneSnapshot()` 生成非工作区的 SVG 输出

内置 SVG 导出 seam 不属于 workspace save/load，也不替代 fragment/template 流程。

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
- [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)
- [ScaleSmoke 基线](./scale-baseline.md)
