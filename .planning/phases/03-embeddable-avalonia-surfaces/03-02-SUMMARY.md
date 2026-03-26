---
phase: 03-embeddable-avalonia-surfaces
plan: 02
subsystem: ui
tags: [avalonia, inspector, minimap, embedding, factory]
requires:
  - phase: 03-01
    provides: focused surface harness and standalone canvas entry patterns
provides:
  - standalone GraphInspectorView factory/options surface
  - standalone GraphMiniMap factory/options surface
  - pure-inspector boundary regression coverage
  - focused minimap viewport recenter regression coverage
affects: [03-03, 03-04, host-samples, smoke-tests]
tech-stack:
  added: []
  patterns:
    - standalone shell sub-surfaces keep factory-first entry points under Hosting/
    - pure surfaces exclude shell-only UI rather than reusing the whole GraphEditorView
key-files:
  created:
    - src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml
    - src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewFactory.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewFactory.cs
    - tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs
    - tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs
  modified: []
key-decisions:
  - "本 plan 先复制出纯 inspector surface，不在这一阶段改写 GraphEditorView.axaml 的现有壳层结构。"
  - "minimap 的 focused regression 收口在 shared viewport recenter 核心逻辑，而不模拟 Avalonia 内部不可实现的 IPointer。"
patterns-established:
  - "Standalone inspector binds directly to GraphEditorViewModel and excludes workspace/fragments/shortcut-help/minimap chrome."
  - "Standalone mini map remains a narrow control with a factory wrapper, not a shell card with headings or commands."
requirements-completed: [EMBD-03, EMBD-04]
duration: 13min
completed: 2026-03-26
---

# Phase 03-02 Summary

**独立 inspector 与独立 mini map 已作为纯表面提供给宿主，且保持与 full shell 更窄的职责边界。**

## Performance

- **Duration:** 13 min
- **Started:** 2026-03-26T15:26:23+08:00
- **Completed:** 2026-03-26T15:39:37+08:00
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments

- 新增 `GraphInspectorView` 及其 factory/options，使宿主可单独嵌入纯 inspector surface。
- 新增 `GraphMiniMap` 的 factory/options，使宿主可直接复用共享 editor state 的 minimap surface。
- 补齐 focused tests，锁定 inspector 的纯边界和 minimap 的 shared viewport recenter 行为。

## Task Commits

This plan was delivered in one execution commit:

1. **03-02 execution** - `5a48c7a` (`feat(03-02): add standalone inspector and minimap surfaces`)

## Files Created/Modified

- `src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml` - 独立检查器的纯展示/参数编辑 XAML。
- `src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml.cs` - 独立检查器的 `Editor` 绑定面。
- `src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewOptions.cs` - 独立检查器宿主输入。
- `src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewFactory.cs` - 独立检查器规范创建入口。
- `src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewOptions.cs` - 独立 minimap 宿主输入。
- `src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewFactory.cs` - 独立 minimap 规范创建入口。
- `tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs` - 验证 pure-inspector 边界和参数区展示。
- `tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs` - 验证 minimap shared editor 绑定与重心更新逻辑。

## Decisions Made

- 不在 `03-02` 修改现有 `GraphEditorView.axaml`，避免在 shell 组合 plan 之前提前搅动你当前已有的本地改动。
- standalone inspector 直接复用现有 inspector markup 的必要部分，但明确剔除 workspace/fragments/shortcut-help/minimap 块。
- minimap 测试只锁定宿主可见行为边界，不去模拟 Avalonia 内部不可由用户代码实现的 pointer plumbing。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- `IPointer` 在 Avalonia 中不是给普通用户代码实现的接口，最初的 pointer 事件模拟方案会把测试耦合到框架私有约束；随后改为直接验证 minimap 的 shared viewport recenter 核心逻辑。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `03-03` 现在可以开始把 full shell 组合到这些独立 surface 上，而不是继续内联大块 inspector UI。
- `03-04` 可以直接在 host sample / smoke 中展示 standalone canvas + inspector + minimap 的组合入口。

---
*Phase: 03-embeddable-avalonia-surfaces*
*Completed: 2026-03-26*
