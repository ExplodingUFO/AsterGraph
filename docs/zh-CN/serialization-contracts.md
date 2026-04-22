# 工作区与片段序列化契约（中文）

本文只陈述当前实现强制执行的边界，不推断超出实现的行为。

## 工作区文档持久化（canonical）

- 工作区持久化的 canonical 路线是：`IGraphWorkspaceService`（保存/加载工作区） + `GraphDocumentSerializer`（文档 JSON 编码/解码）。
- `GraphWorkspaceService` 的实现直接调用：
  - `GraphDocumentSerializer.Save(document, WorkspacePath)`
  - `GraphDocumentSerializer.Load(WorkspacePath)`
- `GraphDocumentSerializer` 的写入 payload 的版本为 `SchemaVersion = 5`，由 `GraphDocumentCompatibility.CurrentSchemaVersion` 固定。

## 工作区文档写入 envelope（canonical）

`GraphDocumentSerializer` 写入 payload 的 JSON 字段固定为：

- `SchemaVersion`
- `Title`
- `Description`
- `RootGraphId`
- `GraphScopes`

## 工作区文档读取兼容性

- 读取时：
  - 若 JSON 中没有 `SchemaVersion`，按旧版无版本 payload 兼容路径反序列化。
  - 若有 `SchemaVersion`，只接受 `1` 到 `5` 的版本号。
    - `5`：按当前 schema payload 解析。
    - `1`-`4`：按旧版 schema payload 解析并归一化。
  - 版本号 `< 1` 或 `> 5` 时抛出 `InvalidOperationException`，即视为不支持。

## 片段/剪贴板序列化边界（独立于工作区）

- 片段/剪贴板路径使用独立的 payload 契约与版本控制，不复用工作区文档 `SchemaVersion`。
- `GraphClipboardPayloadCompatibility` 当前工作区间版本为 `1`（与工作区文档版本无关）。
- 片段/剪贴板读取规则：
  - 若无 `SchemaVersion`，按 legacy fragment payload（含节点与连线）兼容读取。
  - 若有 `SchemaVersion`，必须等于 `1` 才接受；否则拒绝。
- 这条契约用于 `GraphSelectionFragment` 的序列化与反序列化，不代表工作区文档契约。
