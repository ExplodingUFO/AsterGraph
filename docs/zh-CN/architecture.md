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

## Official Capability Modules

这些 `Official Capability Modules` 是建立在平台骨架之上的公开模块名：`Selection`、`History`、`Clipboard`、`Shortcut Policy`、`Layout`、`MiniMap`、`Stencil`、`Fragment Library`、`Export`、`Baseline Edge Authoring`、`Node Surface Authoring`、`Hierarchy Semantics`、`Composite Scope Authoring`、`Edge Semantics`、`Edge Geometry Tooling`。它们都根植于 canonical runtime/session contract，不是另一套 route 名称。

模块到 seam 的映射看 [Host Integration](./host-integration.md)，第二适配器合同看 [Adapter Capability Matrix](./adapter-capability-matrix.md)，proof / sample 锚点看 [Quick Start](./quick-start.md)。

## Proof Ring

平台骨架当前通过三层 proof 守住：

- regression tests：验证 canonical runtime contract 和 retained compatibility boundary
- CI lanes：Windows、Linux、macOS 都跑 canonical solution validation path
- proof artifacts 与 release summary：在发布前把 contract marker 直接暴露出来

建议配合 [Quick Start](./quick-start.md) 和 [Host Integration](./host-integration.md) 一起看。
