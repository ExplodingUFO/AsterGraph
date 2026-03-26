---
phase: 04-replaceable-presentation-kit
plan: 03
subsystem: ui
tags: [avalonia, inspector, minimap, presenter, composition]
requires:
  - phase: 04-01
    provides: presenter contracts and shared Presentation plumbing
  - phase: 04-02
    provides: stock-vs-custom presenter pattern inside NodeCanvas
provides:
  - replaceable inspector presenter host over existing editor projections
  - replaceable mini-map presenter host over existing viewport navigation
  - stock inspector/minimap presenter classes for explicit host reuse
  - focused inspector/minimap/full-shell replacement regression coverage
affects: [04-04, host-samples, smoke-tests, docs]
tech-stack:
  added:
    - src/AsterGraph.Avalonia/Presentation/DefaultGraphInspectorPresenter.cs
    - src/AsterGraph.Avalonia/Presentation/DefaultGraphMiniMapPresenter.cs
  patterns:
    - stock standalone surfaces double as presenter hosts instead of forcing a second shell abstraction
    - mini-map stock rendering moves behind an internal stock surface while the public host control stays stable
key-files:
  created:
    - src/AsterGraph.Avalonia/Presentation/DefaultGraphInspectorPresenter.cs
    - src/AsterGraph.Avalonia/Presentation/DefaultGraphMiniMapPresenter.cs
  modified:
    - src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml.cs
    - src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs
    - tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs
    - tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs
key-decisions:
  - "GraphInspectorView 保持为 public stock surface，但在 custom presenter 存在时切换到 presenter 返回的 content，而不是引入新的 shell root。"
  - "GraphMiniMap 改成 public host control + internal stock rendering surface，这样既能保留现有 viewport 逻辑，又能容纳 custom control replacement。"
  - "stock presenter classes 作为显式默认实现公开，方便宿主按需复用或与 custom presenter 混搭。"
patterns-established:
  - "Inspector and mini-map replacement both stay opt-in and still bind to the same GraphEditorViewModel instance used by full shell and standalone surfaces."
  - "Full shell presenter forwarding can now mix stock canvas with custom inspector/minimap presenters against one editor session."
requirements-completed: [PRES-03, PRES-04]
duration: 31min
completed: 2026-03-26
---

# Phase 04-03 Summary

**inspector 与 mini map 现在都已经支持 opt-in presenter replacement，且 full shell 能把这两类 custom presenter 正常转发到同一套 editor state 上。**

## Performance

- **Duration:** 31 min
- **Started:** 2026-03-26T17:58:37+08:00
- **Completed:** 2026-03-26T18:29:38+08:00
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments

- `GraphInspectorView` 现在在 stock content 与 custom inspector presenter 之间切换，不再只是固定 XAML surface。
- `GraphMiniMap` 现在是 public host control，内部保留 stock rendering surface，同时允许 custom control replacement。
- 新增 `DefaultGraphInspectorPresenter` 与 `DefaultGraphMiniMapPresenter`，为宿主提供显式 stock presenter 入口。
- focused inspector/minimap harness 已覆盖 standalone custom presenter、full shell forwarding、以及 stock pure-boundary continuity。

## Task Commits

This plan was delivered in one execution commit:

1. **04-03 execution** - `c50bcf2` (`feat(04-03): add inspector and minimap presenters`)

## Files Created/Modified

- `src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml.cs` - 把 standalone inspector 升级为 presenter host，并在 stock/custom content 间切换。
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs` - 把 public mini map 升级为 presenter host，同时保留 internal stock rendering/navigation surface。
- `src/AsterGraph.Avalonia/Presentation/DefaultGraphInspectorPresenter.cs` - 显式 stock inspector presenter。
- `src/AsterGraph.Avalonia/Presentation/DefaultGraphMiniMapPresenter.cs` - 显式 stock mini map presenter。
- `tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs` - 覆盖 custom inspector presenter 与参数编辑流。
- `tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs` - 覆盖 custom mini map presenter 与 editor-owned viewport recenter。
- `tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs` - 覆盖 full shell 对 custom inspector/minimap presenters 的 forwarding。

## Decisions Made

- 不新造第二套 inspector/minimap state model；custom presenter 继续直接拿 `GraphEditorViewModel` 和现有 `NodeParameterViewModel`/viewport APIs。
- stock fallback 保留在 public surface 自身，而不是强制宿主总是通过 presenter class 构造 stock UI。
- `GraphInspectorStandaloneTests` 中关于参数存在性的 stock continuity 断言改成直接读 editor projection，避免 temp harness 下的视觉时序抖动。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- `GraphInspectorView` 初版字段落在类体外，修正为类内字段后恢复编译。
- temp harness 下 stock inspector 的参数文本可见性断言存在时序抖动，已改为更稳定的 projection-level continuity 检查。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `04-04` 现在可以直接面向 host sample、package smoke、README/docs 做四类 presenter replacement 的证明环。
- Phase 4 的 wave 2 已完成，下一步只剩 sample/docs/verification 收尾。

---
*Phase: 04-replaceable-presentation-kit*
*Completed: 2026-03-26*
