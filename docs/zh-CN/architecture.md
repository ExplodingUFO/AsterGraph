# Architecture

这份文档说明支撑当前 public beta capability surface（`0.11.0-beta`）的平台骨架。

## Split

AsterGraph 当前按三层明确拆分：

1. `Editor Kernel`
   负责文档变更、history、diagnostics、automation、plugin loading，以及 canonical runtime 的 commands / queries / events。
2. `Scene/Interaction`
   负责 adapter-neutral 的场景快照与交互语义，例如 `GraphEditorSceneSnapshot`、`GraphEditorPointerInputRouter`、`GraphEditorSceneResizeHitTester`、`GraphEditorPlatformServices`。
3. `UI Adapter`
   负责某个具体框架下的渲染、控件组合、平台输入接线和宿主壳层 chrome，例如 Avalonia。

对外推荐入口仍然是 `CreateSession(...)` + `IGraphEditorSession`。默认 Avalonia hosted UI 是组合在这层 runtime 之上的，不会替代它。

## Stability Levels

当前 public beta 有意区分不同 stability level：

| Surface | 当前 stability | 说明 |
| --- | --- | --- |
| `CreateSession(...)`、`IGraphEditorSession`、DTO/snapshot queries、diagnostics、automation、plugin inspection | canonical | 新的宿主代码优先从这里开始 |
| `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | supported hosted adapter | 第一个官方 `UI Adapter`；共享运行时 owner 仍然是 `Editor.Session` |
| `GraphEditorViewModel`、`GraphEditorView` | compatibility | 仅保留给迁移路径，并且已显式标成 advanced |
| scene/input/platform 内部 helper | internal adapter seam | 用来保持 adapter boundary 简洁，不是当前公开合同 |

## Adapter Boundary

Avalonia 包是第一个官方 adapter，不是整个 SDK 的唯一形态。

- `AsterGraph.Editor` 持有 runtime model 和 canonical host contract。
- `AsterGraph.Avalonia` 消费共享的 `Scene/Interaction` seam，并补上 Avalonia 专属的渲染与组合。
- `WPF` 已经被锁定为当前公开 beta 线的 adapter 2 目标，它必须复用同一套 `Editor Kernel` + `Scene/Interaction` 形状，而不是复制一份 editor semantics。
- Avalonia/WPF 之间的差异要通过 [Adapter Capability Matrix](./adapter-capability-matrix.md) 里的 `supported` / `partial` / `fallback` 公开；其中 `partial` / `fallback` 表示 validation-only，不代表 parity，也不代表已经与 Avalonia 对齐。

## Layer Boundary Contract

React Flow parity 路线把这份依赖合同作为 v1 工作的可执行骨架。除非某个 issue 先明确更新本合同，否则新增代码必须沿用这些依赖箭头：

- `AsterGraph.Abstractions` 负责宿主可见的 identifiers、definitions、styling contracts 和 plugin-neutral interfaces。它不能引用 `AsterGraph.Core`、`AsterGraph.Editor`、`AsterGraph.Avalonia` 或 adapter 包。
- `AsterGraph.Core -> Abstractions` 负责持久化 graph models、schema/version rules、serialization 和显式 migration helpers。它不能引用 session state、rendering projection、`GraphEditorViewModel`、`NodeCanvas` 或任何 Avalonia 类型。
- `AsterGraph.Editor -> Core + Abstractions` 负责 `IGraphEditorSession`、commands、queries、events、runtime snapshots、layout services、history、validation、clipboard、export、plugin loading，以及 adapter-neutral scene/interaction seams。它不能引用 `AsterGraph.Avalonia` 或 Avalonia 包。
- `AsterGraph.Avalonia -> Editor + Core` 负责 Avalonia controls、renderers、input coordinators、themes、hosted composition helpers 和 visual adapters。它必须消费 runtime/session contracts 与 snapshots，不能拥有 document mutation semantics。
- Demo 与 Cookbook 项目只通过 public packages 演示宿主用法，不能成为 library behavior 的来源。

Public runtime contracts 必须保持无 Avalonia 类型、无 retained view-model 类型。当前 compatibility exception 按精确 symbol 跟踪，而不是按 namespace 放行：`GraphEditorSession(GraphEditorViewModel, ...)`、`GraphEditorViewModel`、`GraphEditorView`、retained hosted factory routes，以及 #48 负责清理的 `IGraphEditorQueries.GetCompatibleTargets(...)` / `CompatiblePortTarget` MVVM shim。它们只作为迁移桥保留；新宿主应从 `CreateSession(...)` 和 `IGraphEditorSession` 开始。

## Official Capability Modules

这些 `Official Capability Modules` 是建立在平台骨架之上的公开模块名：`Selection`、`History`、`Clipboard`、`Shortcut Policy`、`Layout`、`MiniMap`、`Stencil`、`Fragment Library`、`Export`、`Baseline Edge Authoring`、`Node Surface Authoring`、`Hierarchy Semantics`、`Composite Scope Authoring`、`Edge Semantics`、`Edge Geometry Tooling`。它们都根植于 canonical runtime/session contract，不是另一套 route 名称。

模块到 seam 的映射看 [Host Integration](./host-integration.md)，治理后的 feature record 看 [Feature Catalog](./feature-catalog.md)，第二适配器合同看 [Adapter Capability Matrix](./adapter-capability-matrix.md)，proof / sample 锚点看 [Quick Start](./quick-start.md)。

## Proof Ring

平台骨架当前通过三层 proof 守住：

- regression tests：验证 canonical runtime contract 和 retained compatibility boundary
- CI lanes：Windows、Linux、macOS 都跑 canonical solution validation path
- proof artifacts 与 release summary：在发布前把 contract marker 直接暴露出来

建议配合 [Quick Start](./quick-start.md) 和 [Host Integration](./host-integration.md) 一起看。
