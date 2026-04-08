---
phase: 19
slug: graph-first-demo-shell
status: approved
shadcn_initialized: false
preset: none
created: 2026-04-08
---

# Phase 19 — UI Design Contract

> Visual and interaction contract for the graph-first demo shell. Approved as the design baseline for Phase 19 planning.

---

## Design System

| Property | Value |
|----------|-------|
| Tool | none |
| Preset | not applicable |
| Component library | Avalonia built-in controls + existing AsterGraph shell resources |
| Icon library | none required in Phase 19 baseline |
| Font | Inter |

Contract notes:
- Stay on the existing Avalonia/XAML stack.
- Keep the existing dark technical visual language instead of introducing a brand-new look.
- The shell should read as a host-controlled SDK showcase, not a product landing page.

---

## Spacing Scale

Declared values (must be multiples of 4):

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Icon gaps, inline badges, micro spacing |
| sm | 8px | Tight menu and proof spacing |
| md | 16px | Default control spacing and card padding |
| lg | 24px | Section padding and main shell gaps |
| xl | 32px | Major shell separation |
| 2xl | 48px | Reserved for large shell breaks |
| 3xl | 64px | Reserved for page-level breathing room only |

Exceptions: none

Layout contract:
- Window outer padding remains 24px.
- Top menu sits on one row and should not become a second toolbar band.
- Main graph frame padding stays compact; the graph should visually own the center.
- Secondary panel padding may use 16px or 20px, but no secondary panel should visually outweigh the graph frame.

---

## Typography

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | 15px | 400 | 1.5 |
| Label | 12px | 600 | 1.4 |
| Heading | 22px | 600 | 1.25 |
| Display | 28px | 600 | 1.2 |

Typography contract:
- Use only regular 400 and semibold 600.
- Display is reserved for the page-level identity only.
- Heading is for section titles and the main graph region title.
- Label is for badges, menu grouping, and proof metadata.
- Avoid long paragraphs; copy should feel compact and operational.

---

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | #09111A | Window background and graph-first shell backdrop |
| Secondary (30%) | #101923 | Menu bar surfaces, cards, and secondary panes |
| Accent (10%) | #7FE7D7 | Selected menu state, proof badges, active shell indicators |
| Destructive | #FF9B9B | Destructive actions only |

Accent reserved for:
- active host menu group or active proof badge
- single session/runtime continuity indicators
- narrow highlight states, never every interactive element

Color contract:
- The graph backdrop may remain slightly darker than surrounding cards.
- Secondary panes must support the graph, not compete with it.
- Avoid bright product-style CTA treatment in this phase.

---

## Copywriting Contract

| Element | Copy |
|---------|------|
| Primary CTA | 打开展示菜单 |
| Empty state heading | 未选择展示分组 |
| Empty state body | 先从顶部菜单选择一个展示分组，再查看当前宿主壳层如何控制同一张实时节点图。 |
| Error state | 当前展示面板未能加载。请关闭并重新打开该分组；如果问题持续，请检查运行时诊断摘要。 |
| Destructive confirmation | 重置展示布局：确认恢复默认展示布局与面板开合状态？ |

Copy contract:
- UI copy is Chinese-first.
- API/type/seam names remain English when they map to real SDK entry points.
- Phrase the shell as “宿主如何控制同一运行时” rather than as marketing copy.
- Keep explanatory text to one or two lines in Phase 19.

Required top-level menu groups:
- `展示`
- `视图`
- `行为`
- `运行时`
- `证明`

---

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| shadcn official | none | not applicable |
| third-party registries | none | not applicable |

---

## Phase-Specific Layout Contract

### Window Composition

- The window root should be top-first, using `DockPanel` or an equivalent shell where the menu is visibly docked at the top.
- The first visible chrome row is the host-level `Menu`, not a large explanatory hero card.
- The center region must be dominated by one real `GraphEditorView`.
- Secondary shell content should open from a compact right-side pane/drawer or another bounded on-demand surface.

### Graph Priority

- At the default window size (`1480x900`), the graph region should visually own most of the width.
- The graph region should keep a practical minimum width around 880px before secondary content begins to constrain it.
- There must not be a second equal-weight live editor in Phase 19.

### Secondary Pane

- The preferred pattern is a right-side pane using `SplitView` with `PanePlacement="Right"`.
- `DisplayMode="Overlay"` or `DisplayMode="CompactOverlay"` is preferred over fixed inline rails for the default state.
- Default state should keep the pane closed or visually minimized so the first read stays graph-first.
- Pane width should stay in the 320-360px range when fully open.

### Legacy Content Migration Rule

- The old left/right explanation cards are not allowed to remain as the dominant shell structure.
- Capability overview, diagnostics summaries, and seam explanations may persist, but only in compact, subordinate, or on-demand forms.

---

## Phase-Specific Interaction Contract

### Host Menu

- Top-level menu items must be keyboard reachable and visually read as host controls, not graph context actions.
- Menu items may launch panes, toggle compact surfaces, or focus proof groups.
- Menu grouping should anticipate later phases, even if Phase 19 does not move every control yet.

### Session Continuity

- Host menu interactions must not rebuild `MainWindowViewModel.Editor`.
- The live graph session remains the same while the shell composition changes.
- Any proof badge or summary should reinforce that the graph remains on one live runtime session.

### Proof Strip

- Phase 19 should include a minimal in-context proof strip or badge cluster near the graph.
- Proof should be compact: examples include “单一实时会话”, active menu group, pane state, or shell mode summary.
- Long narrative cards are deferred to compact proof refinements in later phases.

---

## Component Inventory For This Phase

- `MainWindow` becomes the graph-first host shell.
- `Menu` is the host-level top navigation/control surface.
- `GraphEditorView` remains the main live graph experience.
- A compact right-side pane/drawer hosts subordinate showcase content.
- `MainWindowViewModel` owns host-menu state, pane state, and shell summaries.

---

## Checker Sign-Off

- [x] Dimension 1 Copywriting: PASS
- [x] Dimension 2 Visuals: PASS
- [x] Dimension 3 Color: PASS
- [x] Dimension 4 Typography: PASS
- [x] Dimension 5 Spacing: PASS
- [x] Dimension 6 Registry Safety: PASS

**Approval:** approved 2026-04-08
