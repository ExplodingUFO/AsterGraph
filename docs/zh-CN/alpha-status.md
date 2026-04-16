# AsterGraph Alpha 状态

## 当前 Alpha 目标

- 包版本基线：`0.2.0-alpha.1`
- 仓库状态：public alpha readiness
- 对外入口：`README.md` / `README.zh-CN.md` 加上 `docs/en` / `docs/zh-CN`

`.planning` 仍然是维护者上下文，不是外部接入入口。

## Alpha Surface 当前包含

- 四个可发布包
- canonical factory/session 组合
- 默认 Avalonia hosted UI 与 standalone surfaces
- plugin discovery / trust policy / load inspection / runtime loading
- `IGraphEditorSession.Automation`
- 官方 proof lanes 与 smoke tools

## 当前明确不包含

- plugin marketplace 或远程安装 / 更新
- plugin unload lifecycle
- sandbox / 不受信任代码隔离保证
- 更复杂的 automation authoring UI 或内建脚本 IDE
- 突然移除 retained compatibility API

## 稳定性说明

- stable canonical surfaces：factory、`IGraphEditorSession`、DTO/snapshot query、runtime-boundary diagnostics/automation/plugin inspection
- retained migration surfaces：`GraphEditorViewModel`、`GraphEditorView`
- compatibility-only shims：见 [Extension Contracts](./extension-contracts.md)

## 已知限制

- public prerelease publish 还在继续收口到 tag-driven release flow
- 最深的 package validation lane 目前仍以 Windows 为主
- retained compatibility API 仍处于迁移窗口内

## 推荐入口

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [State Contracts](./state-contracts.md)
- [Extension Contracts](./extension-contracts.md)
- [Demo Guide](./demo-guide.md)
