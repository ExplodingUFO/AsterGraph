# 扩展与维护契约

这份文档发布 surface stability、compatibility retirement、extension precedence 与 lane ownership 的契约。
本仓库采用 canonical-first 模式：稳定语义由 `AsterGraph.Editor` 的 `CreateSession(...)`-based 会话 API 定义，其他 surface 仅作为迁移层。

## 稳定性层级

### Stable canonical surfaces

- `AsterGraphEditorFactory.CreateSession(...)`
- `IGraphEditorSession`
- `GetCompatiblePortTargets(...)` 这类 DTO / snapshot 查询
- runtime-boundary 上的 diagnostics / automation / plugin inspection

### Supported hosted-UI composition helper

- `AsterGraphEditorFactory.Create(...)`

### Retained migration surfaces

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` 作为桥接 canonical runtime contract 的入口

### Compatibility-only shims

- `IGraphEditorQueries.GetCompatibleTargets(...)`
- `CompatiblePortTarget`
- 已有 runtime-first 替代物的旧 MVVM 形状 helper

## 兼容性与淘汰策略

- API 只有三个层级：`Stable canonical surfaces`、`Retained migration surfaces`、`Compatibility-only shims`。
- `Compatibility-only shims`（如 `IGraphEditorQueries.GetCompatibleTargets(...)`、`CompatiblePortTarget`）仅是迁移桥接，不用于新特性开发。
- 每个 `Compatibility-only` API 都必须标记为 `Obsolete`，并带有明确的替代符号路径（replacement path）。
- 不得突发性移除：任何移除都必须先经过明确的弃用周期（包含去向迁移说明与按符号级别的迁移指引）。
- 迁移指引按符号给出旧→新映射：
  - `IGraphEditorQueries.GetCompatibleTargets(...)` → `AsterGraphEditorFactory.CreateSession(...).GetCompatiblePortTargets(...)`
  - `CompatiblePortTarget` → 通过 `AsterGraphEditorFactory.CreateSession(...)` 获取的 canonical target DTO / snapshot
  - `GraphEditorViewModel` / `GraphEditorView` → `AsterGraphEditorFactory.CreateSession(...)` + `AsterGraphEditorFactory.Create(...)` 的 hosted session-first path
  - `GraphEditorViewModel.Session` → `AsterGraphEditorFactory.CreateSession(...).Session`

`retained` 与 `compatibility-only` 仅用于迁移；新工作应优先从 `Stable canonical surfaces` 起步，再决定是否叠加 hosted UI 组合。
包级支持层级清单见 [Public API Inventory](./public-api-inventory.md)。
与 [Host Integration](./host-integration.md) 对齐时，先使用 `CreateSession(...)`/`IGraphEditorSession` 的 canonical seam，再按需调用 `Create(...)` 完成 hosted UI 组合。

## 包级稳定性边界（`AsterGraph` 发布边界）

维护者视角的 package inventory 发布在 [Public API Inventory](./public-api-inventory.md)。扩大 public API claims 前，先保持这里和 inventory 一致。

- `AsterGraph.Abstractions`：稳定发布 node / provider / 插件契约，适合作为 contract-first 集成入口。
- `AsterGraph.Core`：当前用于 `GraphDocument`、序列化、兼容层与迁移辅助，不替代 `AsterGraph.Editor` 的 canonical runtime/session 语义。
- `AsterGraph.Editor`：canonical runtime/session owner，承载稳定语义与会话 API；新增能力优先从这里落地。
- `AsterGraph.Avalonia`：当前唯一 shipped 的官方 adapter，提供 `Create(...)` 与默认 shell/standalone composition 入口，不是第二套 runtime API。

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
