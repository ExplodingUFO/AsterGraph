---
phase: 03-embeddable-avalonia-surfaces
plan: 03
subsystem: ui
tags: [avalonia, shell, composition, chrome, embedding]
requires:
  - phase: 03-01
    provides: standalone canvas surface and stock menu presenter
  - phase: 03-02
    provides: standalone inspector and standalone mini map surfaces
provides:
  - thin full-shell composition over standalone surfaces
  - composition-focused surface regression coverage
  - retained full-shell factory continuity
affects: [03-04, host-samples, smoke-tests, docs]
tech-stack:
  added: []
  patterns:
    - GraphEditorView composes standalone surfaces plus shell-only sections
    - existing initialization/migration tests use AvaloniaFact when they construct Avalonia views
key-files:
  created:
    - tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs
  modified:
    - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml
    - src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
key-decisions:
  - "GraphEditorView 继续保留 shell-only 的 workspace/fragments/shortcut-help 区块，但 inspector 与 minimap 改为嵌入已抽取的独立 surface。"
  - "不修改本地噪音文件 GraphEditorViewTests.cs，而是新增独立的 composition-focused regression 文件。"
patterns-established:
  - "Full shell stays as a convenience root, but surface reuse flows through GraphInspectorView and GraphMiniMap."
  - "Standalone composition and full shell both bind to the same GraphEditorViewModel/session-backed state."
requirements-completed: [EMBD-01, EMBD-05]
duration: 22min
completed: 2026-03-26
---

# Phase 03-03 Summary

**默认 GraphEditorView 现已成为对独立 canvas/inspector/minimap surface 的薄组合根，同时保留完整 shell 作为便捷宿主入口。**

## Performance

- **Duration:** 22 min
- **Started:** 2026-03-26T15:39:37+08:00
- **Completed:** 2026-03-26T16:01:18+08:00
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments

- `GraphEditorView` 右栏不再内联完整 inspector/minimap 内容，而是组合 `GraphInspectorView` 和 `GraphMiniMap`。
- 新增 `GraphEditorSurfaceCompositionTests.cs`，明确覆盖 full shell、standalone surface 组合、以及 standalone canvas opt-out 仍保留 session/menu 行为。
- 为当前 focused harness 修补 initialization/migration 中实际构造 Avalonia view 的用例，使 retained full-shell path 回归可稳定执行。

## Task Commits

This plan was delivered in one execution commit:

1. **03-03 execution** - `227d77d` (`feat(03-03): recompose full shell over standalone surfaces`)

## Files Created/Modified

- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` - 改成嵌入 standalone inspector/minimap，同时保留 shell-only 区块。
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs` - 明确默认入口代表完整 shell。
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs` - 文档说明更新为“由独立 surface 组合出的完整 shell”。
- `tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs` - full shell 与 standalone composition 的 focused regression coverage。
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - 对构造 Avalonia view 的 retained-path 测试补 AvaloniaFact。
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` - 对构造 Avalonia view 的 staged-migration 测试补 AvaloniaFact。

## Decisions Made

- full shell 继续存在且继续是最简便入口，但 Phase 3 后它不再是唯一能够承载 stock UI 的方式。
- header/library/status 仍然不提升为一级公开控件；直接 standalone composition 才是省略 shell chrome 的支持路径。
- focused harness 继续复用现有 initialization/migration 回归，而不是再复制一份平行测试矩阵。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- temp harness 引入 initialization/migration 回归后，实际构造 `GraphEditorView` 的用例需要显式切到 `AvaloniaFact`，否则会触发 UI 线程校验。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `03-04` 现在可以直接用 full shell 内嵌 surface 的真实结构去更新 host sample、package smoke 和文档。
- focused composition ring 已经证明 full shell 与 standalone surfaces 共享一套 editor/session state。

---
*Phase: 03-embeddable-avalonia-surfaces*
*Completed: 2026-03-26*
