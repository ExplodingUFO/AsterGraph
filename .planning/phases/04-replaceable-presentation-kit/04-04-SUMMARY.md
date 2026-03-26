---
phase: 04-replaceable-presentation-kit
plan: 04
subsystem: ui
tags: [docs, host-sample, smoke, validation, presenter]
requires:
  - phase: 04-01
    provides: presenter contracts and Presentation plumbing
  - phase: 04-02
    provides: node and menu presenter replacement
  - phase: 04-03
    provides: inspector and mini-map presenter replacement
provides:
  - consumer-facing host sample proof for stock and custom presenters
  - package smoke presenter markers for all four surfaces
  - final Phase 4 focused validation harness
  - aligned README and host docs for the replaceable presentation kit
affects: [phase-5 planning, external consumers, docs, host sample, smoke validation]
tech-stack:
  added: []
  patterns:
    - host sample provides human-readable stock-vs-custom presenter proof
    - package smoke emits machine-checkable PRESENTER_* markers per surface
    - final Phase 4 validation ring stays isolated from workspace-local GraphEditorViewTests noise
key-files:
  created: []
  modified:
    - README.md
    - docs/host-integration.md
    - src/AsterGraph.Avalonia/README.md
    - tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
    - tools/AsterGraph.HostSample/Program.cs
    - tools/AsterGraph.PackageSmoke/Program.cs
key-decisions:
  - "用 sample 可读输出 + smoke 机器标记 + focused harness 三层证明 presenter replacement，而不是只依赖任一单点。"
  - "retained direct-construction path 继续支持 `GraphEditorView.Presentation`，这样 staged migration 不会因为 presenter seam 被迫一次性切到 factory-only。"
  - "文档明确 stock presenters 仍是零配置默认值，replacement 是 per-surface opt-in，不是全量替换工具包。"
patterns-established:
  - "Consumer docs, host sample, smoke markers, and focused regressions all describe one coherent presenter-replacement story."
  - "Final validation harness composes initialization, migration, node/menu, and inspector/minimap regressions into one Phase 4 proof ring."
requirements-completed: [PRES-01, PRES-02, PRES-03, PRES-04]
duration: 13min
completed: 2026-03-26
---

# Phase 04-04 Summary

**Phase 4 的 consumer-facing proof ring 已闭环：sample、smoke、focused harness 和三份文档现在都统一到同一套 replaceable presentation kit 叙事。**

## Performance

- **Duration:** 13 min
- **Started:** 2026-03-26T18:29:38+08:00
- **Completed:** 2026-03-26T18:42:41+08:00
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments

- `tools/AsterGraph.HostSample` 现在同时输出 stock fallback 和 custom presenter 的 full-shell/standalone 证据。
- `tools/AsterGraph.PackageSmoke` 保留 Phase 1-3 markers，并新增 `PRESENTER_*` 机器标记覆盖 node、menu、inspector、mini-map replacement。
- `README.md`、`docs/host-integration.md`、`src/AsterGraph.Avalonia/README.md` 已统一说明 `AsterGraphPresentationOptions`、四类 presenter contract、full-shell 与 standalone replacement 用法、以及 editor/Avalonia 边界。
- `%TEMP%\astergraph-phase4-validation\AsterGraph.Phase4.Validation.csproj` 已把 initialization、migration、node/menu、inspector/minimap 的 focused regressions 合并为最终 gate。

## Task Commits

This plan was delivered in one execution commit:

1. **04-04 execution** - `4a02d2d` (`feat(04-04): prove replaceable presentation kit`)

## Files Created/Modified

- `tools/AsterGraph.HostSample/Program.cs` - 输出 stock-vs-custom presenter 的人类可读证明。
- `tools/AsterGraph.PackageSmoke/Program.cs` - 输出 `PRESENTER_STOCK_DEFAULT_OK`、`PRESENTER_FULLSHELL_OK`、`PRESENTER_NODE_OK`、`PRESENTER_MENU_OK`、`PRESENTER_INSPECTOR_OK`、`PRESENTER_MINIMAP_OK`。
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - 增加 direct `GraphEditorView.Presentation` retained-path 回归。
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` - 增加 staged migration 下 presentation replacement continuity 回归。
- `README.md` - 根 README 增加 replaceable presentation kit 的 full-shell / standalone 示例。
- `docs/host-integration.md` - 集成指南增加 Phase 4 presenter replacement 合同和示例。
- `src/AsterGraph.Avalonia/README.md` - 包 README 更新为正式的 opt-in presenter replacement 说明。

## Decisions Made

- package smoke 只做 assignment-level machine markers，不强行在 smoke 程序里模拟真实 pointer/menu 打开流程。
- host sample 用简洁的 custom presenter 输出文字证明，不把 sample 复杂化成额外 demo app。
- final harness 继续使用 temp project，而不是去修你本地未纳入 phase 范围的 `GraphEditorViewTests.cs`。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- 早期 initialization/migration 测试里的 stub presenter 原本只为了验证属性转发，进入 Phase 4 final harness 后会真的参与 surface 构建；已改成 lightweight working presenters。
- `README.md` 还留着一条 “deferred to Phase 4” 的旧叙述，已在这轮统一清理。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 4 已完成，下一步应转到 Phase 5，设计 diagnostics / inspection 的稳定公开合同。
- 当前 sample、smoke、docs 和 focused harness 都已经能作为后续 diagnostics 叠加前的稳定基线。

---
*Phase: 04-replaceable-presentation-kit*
*Completed: 2026-03-26*
