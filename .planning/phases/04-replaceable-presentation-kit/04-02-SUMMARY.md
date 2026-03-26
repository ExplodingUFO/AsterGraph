---
phase: 04-replaceable-presentation-kit
plan: 02
subsystem: ui
tags: [avalonia, nodecanvas, presenter, context-menu, fallback]
requires:
  - phase: 04-01
    provides: Avalonia-side presenter contracts and shared Presentation plumbing
provides:
  - replaceable node visual presenter seam inside NodeCanvas
  - stock DefaultGraphNodeVisualPresenter implementation
  - replaceable Avalonia context-menu presentation seam with stock fallback
  - focused node/menu replacement regression coverage
affects: [04-03, 04-04, host-samples, smoke-tests]
tech-stack:
  added:
    - src/AsterGraph.Avalonia/Presentation/DefaultGraphNodeVisualPresenter.cs
  patterns:
    - NodeCanvas stays behavior owner while presenters only own visuals and menu rendering
    - focused temp harness uses public node/menu regressions only and keeps GraphEditorViewTests noise out of the loop
key-files:
  created:
    - src/AsterGraph.Avalonia/Presentation/DefaultGraphNodeVisualPresenter.cs
  modified:
    - src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs
    - tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs
    - tests/AsterGraph.Editor.Tests/GraphContextMenuPresenterTests.cs
key-decisions:
  - "NodeCanvas 只下放 node visual tree 创建/更新和菜单展示层，不下放拖拽、选择、连线、视口、anchor resolution 的行为所有权。"
  - "custom menu presenter 只接收 editor 产出的 descriptor tree；GraphEditorViewModel.BuildContextMenu(...) 仍然是唯一 intent source。"
  - "stock fallback 继续使用 public GraphContextMenuPresenter 和新的 DefaultGraphNodeVisualPresenter，而不是把默认路径藏回 internal。"
patterns-established:
  - "Per-node rendered visuals now carry both the presenter instance and published port anchors, letting NodeCanvas keep z-order and connection math."
  - "Custom presenter validation can run through a focused temp harness without opening real Avalonia popups."
requirements-completed: [PRES-01, PRES-02]
duration: 21min
completed: 2026-03-26
---

# Phase 04-02 Summary

**`NodeCanvas` 现在已经支持可替换的 node visual presenter 与可替换的 context-menu presenter，同时继续保留 stock fallback 和原有交互所有权。**

## Performance

- **Duration:** 21 min
- **Started:** 2026-03-26T17:58:37+08:00
- **Completed:** 2026-03-26T18:19:13+08:00
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments

- 新增 `DefaultGraphNodeVisualPresenter`，把原来内嵌在 `NodeCanvas` 里的 stock node visual tree 提取为独立 presenter。
- `NodeCanvas` 现在会优先解析 `NodeVisualPresenter` 与 `ContextMenuPresenter`，未配置时分别回退到 `DefaultGraphNodeVisualPresenter` 和 `GraphContextMenuPresenter`。
- `_nodeVisuals` 现在持有 presenter + visual result + port anchor 映射，保证 custom presenter 下连线 anchor 解析仍由 canvas 统一消费。
- focused node/menu harness 已验证 custom node visual、生效的 anchor mapping、custom menu presenter、`EnableDefaultContextMenu=false`、以及 stock menu presenter contract。

## Task Commits

This plan was delivered in one execution commit:

1. **04-02 execution** - `bb333f5` (`feat(04-02): add node and menu presenters`)

## Files Created/Modified

- `src/AsterGraph.Avalonia/Presentation/DefaultGraphNodeVisualPresenter.cs` - stock node visual presenter，实现原有 node card 外观和端口按钮布局。
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` - 改为以 presenter seam 创建/更新 node visuals，并通过 configurable menu presenter 打开上下文菜单。
- `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs` - 增加 custom node visual、custom menu presenter、stock fallback 的 focused regression。
- `tests/AsterGraph.Editor.Tests/GraphContextMenuPresenterTests.cs` - 明确锁定 stock menu presenter 继续实现 replaceable contract。

## Decisions Made

- 把 node visual replacement contract 设计成 `GraphNodeVisual + GraphNodeVisualContext`，由 presenter 发布 anchor controls，由 canvas 统一读取坐标。
- `OpenNodeContextMenu` / `OpenPortContextMenu` 逻辑保留在 `NodeCanvas`，这样 custom visual presenter 不需要复制 selection/menu context 规则。
- focused node/menu harness 改用 `AsterGraph.Editor.Tests` 作为临时程序集名，以复用既有 `InternalsVisibleTo` 设置并隔离噪音测试。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- 初版 temp harness 把依赖 internal helper 的测试一起编进来，导致 `NodeCanvasContextMenuSnapshot` 可见性报错；随后把 harness 收窄到 public node/menu regression ring。
- 初版 `NodeCanvas` 仍然落到 stock popup presenter，直接暴露出 custom menu presenter 尚未接线的问题；修正后 failure 直接消失。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `04-03` 现在可以沿用相同策略，把 inspector 和 mini map 也改成 opt-in replacement，而不用再调整 canvas/menu seam。
- `04-04` 后续可以直接在 host sample 和 package smoke 里证明四类 presenter replacement 共存。

---
*Phase: 04-replaceable-presentation-kit*
*Completed: 2026-03-26*
