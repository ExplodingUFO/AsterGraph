# 扩展与维护契约

这份文档发布 surface stability、compatibility retirement、extension precedence 与 lane ownership 的契约。
本仓库采用 canonical-first 模式：稳定语义由 `AsterGraph.Editor` 的 `CreateSession(...)`-based 会话 API 定义。hosted helper 只负责把这条路线组合成 stock UI，retained surface 只用于迁移，compatibility-only shim 必须指向 canonical replacement，internal-only 实现细节不是 public contract。

## 稳定性层级

### Stable canonical surfaces

- `AsterGraphEditorFactory.CreateSession(...)`
- `IGraphEditorSession`
- `GetCompatiblePortTargets(...)` 这类 DTO / snapshot 查询
- runtime-boundary 上的 diagnostics / automation / plugin inspection

### Supported hosted helper

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphHostBuilder`
- `AsterGraphCanvasViewFactory`、`AsterGraphInspectorViewFactory`、`AsterGraphMiniMapViewFactory` 等 standalone Avalonia factories

### Retained migration surfaces

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` 作为桥接 canonical runtime contract 的入口

### Compatibility-only surfaces

- `IGraphEditorQueries.GetCompatibleTargets(...)`
- `CompatiblePortTarget`
- 已有 runtime-first 替代物的旧 MVVM 形状 helper

### Internal-only surfaces

- `Runtime.Internal`、`Kernel.Internal`、adapter control internals、projection/apply internals、测试、样例和 proof tools

## 兼容性与淘汰策略

- API 淘汰和维护使用与 inventory 相同的五类支持层级：stable canonical、supported hosted helper、retained migration、compatibility-only、internal-only。
- `Compatibility-only surfaces`（如 `IGraphEditorQueries.GetCompatibleTargets(...)`、`CompatiblePortTarget`）仅是迁移桥接，不用于新特性开发。
- 每个 `Compatibility-only` API 都必须标记为 `Obsolete`，并带有明确的替代符号路径（replacement path）。
- 不得突发性移除：任何移除都必须先经过明确的弃用周期（包含去向迁移说明与按符号级别的迁移指引）。
- 迁移指引按符号给出旧→新映射：
  - `IGraphEditorQueries.GetCompatibleTargets(...)` → `AsterGraphEditorFactory.CreateSession(...).GetCompatiblePortTargets(...)`
  - `CompatiblePortTarget` → `AsterGraphEditorFactory.CreateSession(...).GetCompatiblePortTargets(...)` 返回的 `GraphEditorCompatiblePortTargetSnapshot`
  - `GraphEditorViewModel` / `GraphEditorView` → runtime-only 宿主使用 `AsterGraphEditorFactory.CreateSession(...)`，hosted Avalonia 宿主使用 `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
  - `GraphEditorViewModel.Session` → `AsterGraphEditorFactory.CreateSession(...)` 返回的 `IGraphEditorSession`

`retained` 与 `compatibility-only` 仅用于迁移；新工作应优先从 `Stable canonical surfaces` 起步，再决定是否叠加 `Supported hosted helper` 组合。
包级支持层级清单见 [Public API Inventory](./public-api-inventory.md)。
与 [Host Integration](./host-integration.md) 对齐时，先使用 `CreateSession(...)`/`IGraphEditorSession` 的 canonical seam，再按需调用 `Create(...)` 完成 hosted UI 组合。

## 包级稳定性边界（`AsterGraph` 发布边界）

维护者视角的 package inventory 发布在 [Public API Inventory](./public-api-inventory.md)。扩大 public API claims 前，先保持这里和 inventory 一致。

- `AsterGraph.Abstractions`
  - Stable canonical：节点定义、端口定义、provider / plugin-facing contract、identifier、metadata DTO
  - Supported hosted helper：无
  - Retained migration：无
  - Compatibility-only：当前不作为主支持层级发布
  - Internal-only：未通过 package docs 暴露的实现 helper
- `AsterGraph.Core`
  - Stable canonical：图文档、序列化模型契约、兼容规则输入，以及 editor/session 组合使用的共享数据类型
  - Supported hosted helper：无
  - Retained migration：无
  - Compatibility-only：已有 runtime-first 路线时保留的旧 conversion / compatibility helper
  - Internal-only：Core internals 与持久化实现细节
- `AsterGraph.Editor`
  - Stable canonical：`AsterGraphEditorFactory.CreateSession(...)`、`IGraphEditorSession`、command/query DTO、diagnostics、automation、plugin discovery / inspection、export services
  - Supported hosted helper：`AsterGraphEditorFactory.Create(...)` 是 hosted 组合 helper，但仍返回 retained facade
  - Retained migration：`GraphEditorViewModel`、`GraphEditorViewModel.Session`、迁移宿主使用的 retained menu / context-menu hook
  - Compatibility-only：`IGraphEditorQueries.GetCompatibleTargets(...)`、`CompatiblePortTarget`，以及已有 runtime snapshot 替代物的旧 MVVM 形状 helper
  - Internal-only：`Runtime.Internal`、`Kernel.Internal`、projection/apply internals、proof-only helper
- `AsterGraph.Avalonia`
  - Stable canonical：canonical editor/session route 上的 adapter 投影
  - Supported hosted helper：`AsterGraphAvaloniaViewFactory.Create(...)`、standalone surface factories、`AsterGraphHostBuilder`
  - Retained migration：仍使用 retained editor facade 的宿主可嵌入 `GraphEditorView`
  - Compatibility-only：仅限把现有宿主桥接到 canonical route 的 adapter-specific glue
  - Internal-only：control internals、templates、interaction session internals、visual-only implementation details

canonical-first 指导是：优先从 `AsterGraph.Editor` 的 canonical surface 做新功能，再决定是否叠加 `AsterGraph.Avalonia`；`retained` 只用于迁移，不是默认起点。

## 扩展优先级

- plugin trust 由 host 决定，并且发生在 activation 之前
- plugin localization 先组合，host localization 最后覆盖
- plugin node presentation 先组合，host presentation 覆盖最终字段，合并型 adornment 继续累积
- plugin command 通过 canonical session command descriptor pipeline 注册；如果和 stock command 撞 id，stock command 继续保留执行权
- runtime/session menu 当前仍以 stock descriptor 投影为主，并继续向共享 command source 收敛
- retained `GraphEditorViewModel.BuildContextMenu(...)` 仍是 compatibility host 的最终 override 点

## Lane Ownership

- `eng/ci.ps1 -Lane all` = framework-matrix build/test lane
- `eng/ci.ps1 -Lane contract` = focused consumer/state-contract gate
- `eng/ci.ps1 -Lane maintenance` = hotspot-refactor gate
- `eng/ci.ps1 -Lane release` = packed publish gate + smoke + coverage
- `tests/AsterGraph.Demo.Tests` = demo/sample-host lane

排查失败时，先按 lane 分类，再决定改哪一层代码。
