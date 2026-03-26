---
phase: 04-replaceable-presentation-kit
plan: 01
subsystem: ui
tags: [avalonia, presentation, contracts, hosting, factory]
requires:
  - phase: 03-embeddable-avalonia-surfaces
    provides: standalone canvas/inspector/minimap surfaces and full-shell composition over them
provides:
  - Avalonia-side presenter contracts for node visuals, menus, inspector, and mini map
  - shared AsterGraphPresentationOptions container for opt-in per-surface replacement
  - canonical Presentation plumbing through full-shell and standalone factories
  - focused contract-forwarding regression coverage
affects: [04-02, 04-03, 04-04, host-samples, smoke-tests]
tech-stack:
  added:
    - src/AsterGraph.Avalonia/Presentation/*
  patterns:
    - opt-in per-surface presentation replacement stays in AsterGraph.Avalonia
    - factory/options entry points remain the canonical host configuration surface
key-files:
  created:
    - src/AsterGraph.Avalonia/Presentation/AsterGraphPresentationOptions.cs
    - src/AsterGraph.Avalonia/Presentation/IGraphNodeVisualPresenter.cs
    - src/AsterGraph.Avalonia/Presentation/IGraphContextMenuPresenter.cs
    - src/AsterGraph.Avalonia/Presentation/IGraphInspectorPresenter.cs
    - src/AsterGraph.Avalonia/Presentation/IGraphMiniMapPresenter.cs
    - src/AsterGraph.Avalonia/Presentation/GraphNodeVisual.cs
    - src/AsterGraph.Avalonia/Presentation/GraphNodeVisualContext.cs
  modified:
    - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs
    - src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml.cs
    - src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs
    - src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewFactory.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewFactory.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewFactory.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewOptions.cs
    - src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs
key-decisions:
  - "Phase 4 的 presenter seam 继续保持中等粒度，按 node/menu/inspector/minimap 四个 surface 暴露，不拆成更细碎的按钮级 API。"
  - "所有替换入口统一通过 AsterGraphPresentationOptions 进入，避免 full shell 和 standalone surface 出现两套平行配置模型。"
  - "Avalonia-specific presenter contract 全部留在 AsterGraph.Avalonia，不向 AsterGraph.Editor 下沉任何 UI 抽象。"
patterns-established:
  - "GraphEditorView and standalone factories forward Presentation options without breaking direct Editor assignment or stock defaults."
  - "Focused temp harnesses continue to validate Phase 4 changes without depending on workspace-local GraphEditorViewTests noise."
requirements-completed: [PRES-01, PRES-02, PRES-03, PRES-04]
duration: 28min
completed: 2026-03-26
---

# Phase 04-01 Summary

**Avalonia 侧 presenter contract surface 和 canonical Presentation plumbing 已经锁定，Phase 4 后续替换实现现在可以基于稳定入口继续推进。**

## Performance

- **Duration:** 28 min
- **Started:** 2026-03-26T17:30:00+08:00
- **Completed:** 2026-03-26T17:58:37+08:00
- **Tasks:** 2
- **Files modified:** 14

## Accomplishments

- 新增 `AsterGraph.Avalonia.Presentation` 命名空间，公开节点、菜单、检查器、缩略图四类 presenter contract。
- 新增 `AsterGraphPresentationOptions`，把 per-surface replacement 明确收口到一个可选配置对象。
- `GraphEditorView` 与所有 standalone factory/options 已接入 `Presentation` 配置，stock default 仍保持零配置可用。
- `GraphContextMenuPresenter` 已正式实现上下文菜单 presenter 契约，focused initialization harness 覆盖了 forwarding 行为。

## Task Commits

This plan was delivered in one execution commit:

1. **04-01 execution** - `64c90ea` (`feat(04-01): add presentation contract plumbing`)

## Files Created/Modified

- `src/AsterGraph.Avalonia/Presentation/*` - Phase 4 的 Avalonia-side presenter contracts、shared option container、node visual result/context types。
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` - 新增 `Presentation` 属性，并把 presenter 配置转发到 canvas/inspector/minimap surface。
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` - 新增 `NodeVisualPresenter` 与 `ContextMenuPresenter` 属性，作为后续 04-02 的落点。
- `src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml.cs` - 新增 `InspectorPresenter` 属性，作为后续 04-03 的落点。
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs` - 新增 `MiniMapPresenter` 属性，作为后续 04-03 的落点。
- `src/AsterGraph.Avalonia/Hosting/*Options.cs` / `*Factory.cs` - 为 full-shell 与 standalone surface 加入 shared `Presentation` 配置入口。
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - 覆盖 full-shell 和 standalone factory 对 presenter 选项的 forwarding 契约。

## Decisions Made

- 新属性命名使用 `NodeVisualPresenter`、`ContextMenuPresenter`、`InspectorPresenter`、`MiniMapPresenter`，避免和 Avalonia 自带成员重名。
- 先锁 public contract 和 options plumbing，再进入实际 replacement implementation，避免 04-02/04-03 重复修改 host 入口。
- focused contract validation 继续使用 `%TEMP%\astergraph-phase4-contract-validation\AsterGraph.Phase4.Contracts.Validation.csproj`，不碰 `GraphEditorViewTests.cs` 噪音文件。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- `GraphInspectorView.Presenter` 初版命名与 Avalonia 继承成员冲突，已在同一轮中改为 `InspectorPresenter`，同步消除了歧义警告。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `04-02` 现在可以在不再改 public host entry 的前提下，专注把 `NodeCanvas` 的 node visual/menu presenter seam 实际跑通。
- `04-03` 也已经具备了 inspector/minimap presenter 的属性和 factory 入口，只需要落具体 replacement behavior。

---
*Phase: 04-replaceable-presentation-kit*
*Completed: 2026-03-26*
