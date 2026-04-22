# AsterGraph Alpha 状态

## 当前 Alpha 目标

- 包版本基线：`0.2.0-alpha.3`
- 仓库状态：public alpha
- 对外版本说明：[Versioning](./versioning.md)
- 对外入口：`README.md` / `README.zh-CN.md` 加上 `docs/en` / `docs/zh-CN`
- 项目状态页：[Project Status](./project-status.md)
- 对外发布检查清单：[Public Launch Checklist](./public-launch-checklist.md)

## Alpha Surface 当前包含

- 四个可发布包
- canonical factory/session 组合
- 默认 Avalonia hosted UI 与 standalone surfaces
- trusted / loaded / blocked 的 runtime inspection surface
- showcase 宿主里的 command/trust timeline 和 perf overlay
- `tools/AsterGraph.Starter.Avalonia` 作为 shipped 的 Avalonia starter scaffold
- plugin discovery / trust policy / load inspection / runtime loading
- `IGraphEditorSession.Automation`
- 官方 proof lanes 与 smoke tools
- 一个介于 HelloWorld 和 Demo 之间的中等 hosted-UI consumer 样例
- shipped definition-driven inspector 元数据和内建 text/number/boolean/enum/list editor
- 分层节点表面、hierarchy snapshot、composite scope navigation、边注解和 routed edge geometry tooling

## 当前明确不包含

- plugin marketplace 或远程安装 / 更新
- plugin unload lifecycle
- sandbox / 不受信任代码隔离保证
- 超出 shipped definition-driven inspector 之外的任意 host-agnostic property editor framework
- 更复杂的 automation authoring UI 或内建脚本 IDE
- 突然移除 retained compatibility API

## 稳定性说明

- stable canonical runtime surfaces：`CreateSession(...)`、`IGraphEditorSession`、DTO/snapshot query、runtime-boundary diagnostics/automation/plugin inspection
- 受支持的 hosted-UI 组合 helper：`Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
- retained migration surfaces：`GraphEditorViewModel`、`GraphEditorView`
- compatibility-only shims：见 [Extension Contracts](./extension-contracts.md)

## 已知限制

- 当前最新对外 prerelease tag 是 `v0.2.0-alpha.3`；`v1.9` 只保留为公开前的历史里程碑 tag
- public prerelease publish 和 release artifact 仍然依赖维护者 release flow
- 最深的 package validation lane 目前仍以 Windows 为主
- retained compatibility API 仍处于迁移窗口内，但只作为迁移桥接
- 当前 public alpha 仍然只有一套官方 UI adapter：Avalonia
- `v0.9.0-beta` 已经锁定 `WPF` 作为 adapter 2，但在 [Adapter Capability Matrix](./adapter-capability-matrix.md) 真正用 `supported` / `partial` / `fallback` 填出来之前，不代表已经承诺对齐
- advanced editing 已经作为宿主能力模块公开，但更广的 ecosystem 与第二 adapter 验证仍在后续阶段

## 推荐入口

- [Versioning](./versioning.md)
- [Project Status](./project-status.md)
- [Quick Start](./quick-start.md)
- [ScaleSmoke 基线](./scale-baseline.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [State Contracts](./state-contracts.md)
- [Extension Contracts](./extension-contracts.md)
- [Demo Guide](./demo-guide.md)
