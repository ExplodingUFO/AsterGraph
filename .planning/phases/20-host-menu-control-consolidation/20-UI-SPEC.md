---
phase: 20
slug: host-menu-control-consolidation
status: approved
shadcn_initialized: false
preset: none
created: 2026-04-08
---

# Phase 20 — UI Design Contract

> Visual and interaction contract for turning the graph-first shell into a compact host-menu-driven control surface.

---

## Design System

| Property | Value |
|----------|-------|
| Tool | none |
| Preset | not applicable |
| Component library | Avalonia built-in controls + existing AsterGraph shell resources |
| Icon library | optional built-in path or glyph icon only if needed for menu emphasis |
| Font | Inter |

Contract notes:
- Reuse the Phase 19 visual baseline.
- This is a control-density pass, not a full visual redesign.
- The shell must still read as “host controls one live SDK session”, not as a debug-only control board.

---

## Spacing Scale

Declared values (must be multiples of 4):

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Inline state markers, dense text gaps |
| sm | 8px | Menu item spacing, compact control row gaps |
| md | 16px | Default pane card padding |
| lg | 24px | Shell padding and section gaps |
| xl | 32px | Rare larger separation only |

Layout contract:
- Keep outer shell padding at 24px.
- The menu row must remain one visual band.
- The right-side pane should feel like a compact tool drawer, not a second page column.
- Dense control rows should prefer 8-12px internal spacing over large card padding.

---

## Typography

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | 15px | 400 | 1.5 |
| Label | 12px | 600 | 1.4 |
| Heading | 22px | 600 | 1.25 |
| Metric | 13px | 500 | 1.35 |

Typography contract:
- Keep menu labels short and operational.
- Use headings only for the active pane section and graph intro strip.
- Runtime metrics should be readable as dense operational rows, not narrative paragraphs.

---

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant | #09111A | Window and graph backdrop |
| Secondary | #101923 | Menu surface, drawer surface, compact cards |
| Accent | #7FE7D7 | Checked menu state, active group cues, runtime proof markers |
| Neutral Border | #365063 | Drawer and compact card borders |
| Support Text | #B6CBD8 | Metric and explanation text |

Color contract:
- Checked states may use accent foreground or icon emphasis, but do not flood the menu with bright fills.
- Runtime/proof emphasis should stay narrow and informational.

---

## Copywriting Contract

Required control labels:

### View
- `显示顶栏`
- `显示节点库`
- `显示检查器`
- `显示状态栏`
- `打开视图控制`

### Behavior
- `只读模式`
- `网格吸附`
- `对齐辅助线`
- `工作区命令`
- `片段命令`
- `宿主菜单扩展`
- `打开行为控制`

### Runtime
- `打开运行时摘要`
- `查看最近诊断`
- `文档标题`
- `节点数量`
- `连线数量`
- `当前选择`
- `视口缩放`
- `最近诊断`

Copy contract:
- Control labels should describe what changes now, not explain the whole SDK story.
- Use one-line helper text inside the drawer when needed.
- Longer proof/narrative language is deferred to Phase 21.

---

## Phase-Specific Layout Contract

### Top Menu

- Keep the top menu as the first visible interaction band.
- `视图` and `行为` should expose direct checkable items via menu dropdowns.
- Avoid more than two levels of menu depth.
- Use separators to group quick toggles from “open pane” actions.

### Right-Side Control Drawer

- Keep `SplitView` with `PanePlacement="Right"` and `DisplayMode="Overlay"` unless implementation evidence requires a different bounded pattern.
- Drawer width should remain in the 320-360px range.
- Drawer content should switch between compact sections for `展示`, `视图`, `行为`, `运行时`, and `证明`.
- `视图` and `行为` sections should render actual control rows, not summary-only bullet lists.
- `运行时` should render compact metrics and diagnostics rows sourced from the current session.

### Graph Dominance

- The graph remains visually dominant at the default window size.
- Opening the control drawer must not visually demote the graph to a side artifact.
- No new permanent left navigation or right explanation rail may be added.

---

## Phase-Specific Interaction Contract

### View And Behavior

- Menu checked state and drawer control state must stay synchronized.
- Toggling a menu item should update the current shell/runtime immediately.
- Toggling a drawer control should update the menu's checked state if the same boolean is bound there.

### Runtime

- Opening the runtime group must show the current live session state without scene switching.
- Runtime summaries should remain read-mostly and compact.
- Diagnostics rows should be visibly separate from compatibility status text.

### Session Continuity

- None of the Phase 20 controls may recreate `MainWindowViewModel.Editor`.
- All view, behavior, and runtime groups must act on the same live `Editor.Session`.

---

## Component Inventory For This Phase

- `Menu` and `MenuItem` for the top-level host control plane
- `MenuItem ToggleType="CheckBox"` for direct view/behavior quick toggles
- `SplitView` for the compact right-side control drawer
- `CheckBox`, `Button`, `TextBlock`, and `ItemsControl` for grouped pane controls and runtime summaries
- `MainWindowViewModel` as the single host-control state owner

---

## Checker Sign-Off

- [x] Dimension 1 Copywriting: PASS
- [x] Dimension 2 Visuals: PASS
- [x] Dimension 3 Color: PASS
- [x] Dimension 4 Typography: PASS
- [x] Dimension 5 Spacing: PASS
- [x] Dimension 6 Registry Safety: PASS

**Approval:** approved 2026-04-08
