---
phase: 03-embeddable-avalonia-surfaces
plan: 01
subsystem: ui
tags: [avalonia, nodecanvas, context-menu, shortcuts, embedding]
requires:
  - phase: 02-runtime-contracts-service-seams
    provides: public editor session/runtime seams and stable host-facing editor state
provides:
  - standalone NodeCanvas factory/options surface
  - opt-out switches for stock context menu and stock command shortcuts
  - public stock Avalonia context-menu presenter
  - focused standalone canvas regression coverage
affects: [03-02, 03-03, 03-04, host-samples, smoke-tests]
tech-stack:
  added: []
  patterns:
    - factory-first standalone Avalonia surface composition
    - focused temp harness for Phase 3 surface verification
key-files:
  created:
    - src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewFactory.cs
    - tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs
  modified:
    - src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs
    - src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs
    - tests/AsterGraph.Editor.Tests/GraphContextMenuPresenterTests.cs
key-decisions:
  - "把默认菜单开关收口到 NodeCanvas 自身，而不是引入新的 presenter 抽象。"
  - "把独立画布快捷键实现为 NodeCanvas 内建路由，这样 standalone surface 不依赖 GraphEditorView shell。"
  - "focused 验证继续使用临时 harness，避免 workspace-local GraphEditorViewTests 噪音阻塞 Phase 3。"
patterns-established:
  - "Standalone Avalonia surfaces use Hosting/*Options + *Factory entry points and bind directly to GraphEditorViewModel."
  - "Default stock UI behaviors stay enabled by default, but hosts get explicit opt-out flags for replacement scenarios."
requirements-completed: [EMBD-02, EMBD-05]
duration: 21min
completed: 2026-03-26
---

# Phase 03-01 Summary

**独立 NodeCanvas 宿主入口、默认菜单/快捷键开关，以及可复用的 Avalonia stock context-menu presenter 已作为一级表面落地。**

## Performance

- **Duration:** 21 min
- **Started:** 2026-03-26T15:05:00+08:00
- **Completed:** 2026-03-26T15:26:23+08:00
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments

- 新增 `AsterGraphCanvasViewOptions` 与 `AsterGraphCanvasViewFactory.Create(...)`，宿主可直接创建独立交互画布。
- `NodeCanvas` 默认保留 stock context menu 与 stock command shortcuts，但宿主现在可显式关闭两类默认行为。
- `GraphContextMenuPresenter` 提升为 public stock presenter，并补齐 focused standalone regression tests。

## Task Commits

This plan was delivered in one execution commit:

1. **03-01 execution** - `2a886e0` (`feat(03-01): publish standalone canvas surface`)

## Files Created/Modified

- `src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewOptions.cs` - 定义独立画布的宿主输入和默认行为开关。
- `src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewFactory.cs` - 提供独立 `NodeCanvas` 的规范创建入口。
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` - 接入默认菜单/快捷键开关，并让 standalone canvas 自己处理 stock shortcuts。
- `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs` - 公开 stock menu presenter 供宿主复用。
- `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs` - 覆盖独立画布绑定、默认菜单、Delete/Escape 快捷键及开关行为。
- `tests/AsterGraph.Editor.Tests/GraphContextMenuPresenterTests.cs` - 覆盖 presenter 公开性与禁用项 tooltip 行为。

## Decisions Made

- 不新增 presenter interface；本阶段只把现有 stock presenter 公开。
- 不为 main test project 修 `GraphEditorViewTests.cs`；Phase 3 focused 验证继续走 temp harness。
- standalone canvas 的默认快捷键以 `NodeCanvas` 为宿主边界实现，避免依赖完整 shell。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- 临时 harness 的 Avalonia test application assembly attribute 起初指向了未限定名，修正为完整类型名后恢复正常。
- `GraphContextMenuPresenterTests` 在普通 `[Fact]` 下会触发 Avalonia UI 线程校验，切换为 `[AvaloniaFact]` 后验证稳定。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `03-01` 已提供 Phase 3 后续所有 surface 计划都可复用的 focused harness 路径：`$env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj`。
- `03-02` 现在可以基于同一 `GraphEditorViewModel` 继续拆独立 inspector 与 mini map，而不需要再补 standalone canvas 基础设施。

---
*Phase: 03-embeddable-avalonia-surfaces*
*Completed: 2026-03-26*
