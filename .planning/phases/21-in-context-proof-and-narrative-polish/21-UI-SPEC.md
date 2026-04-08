---
phase: 21
slug: in-context-proof-and-narrative-polish
status: approved
shadcn_initialized: false
preset: none
created: 2026-04-08
---

# Phase 21 — UI Design Contract

> Visual and interaction contract for replacing the remaining explanation-heavy demo copy with compact proof cues, live configuration summaries, and aligned showcase messaging.

---

## Design System

| Property | Value |
|----------|-------|
| Tool | none |
| Preset | not applicable |
| Component library | Avalonia built-in controls + existing AsterGraph shell resources |
| Icon library | optional built-in glyph only if a proof cue needs a single accent icon |
| Font | Inter |

Contract notes:
- Reuse the Phase 19/20 visual baseline.
- This is a proof-density and wording pass, not a new shell redesign.
- The shell should read as “host proves the SDK on one live session”, not as a documentation panel.

---

## Spacing Scale

Declared values (must be multiples of 4):

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Inline label/value gaps |
| sm | 8px | Badge spacing, dense row spacing |
| md | 16px | Default drawer card padding |
| lg | 24px | Shell padding and section gaps |
| xl | 32px | Rare larger separation only |

Layout contract:
- Keep outer shell padding at 24px.
- The graph intro strip should remain compact enough to read in one glance.
- Proof/configuration groups inside the drawer should prefer dense rows and short helper text over tall paragraph cards.

---

## Typography

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | 15px | 400 | 1.5 |
| Label | 12px | 600 | 1.4 |
| Heading | 22px | 600 | 1.25 |
| Metric | 13px | 500 | 1.35 |

Typography contract:
- Use headings only for the current drawer group and graph intro identity.
- Ownership cues should read as labels or badges, not marketing display text.
- Helper text must stay short enough to scan without feeling like documentation prose.

---

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant | #09111A | Window and graph backdrop |
| Secondary | #101923 | Menu surface, drawer surface, proof cards |
| Accent | #7FE7D7 | Ownership cues, active group badge, live proof markers |
| Neutral Border | #365063 | Drawer and card borders |
| Support Text | #B6CBD8 | Secondary labels and helper text |

Color contract:
- Accent color should identify proof/ownership cues, not become a full-surface fill.
- Host-owned and runtime-owned cues may share the same palette family, but their labels must distinguish ownership explicitly.

---

## Copywriting Contract

Required compact labels:

### Menu and shell copy
- `打开展示摘要`
- `查看证明要点`
- `宿主控制抽屉`
- `实时 SDK 会话`

### Intro/proof cues
- `宿主控制`
- `共享运行时`
- `当前分组`
- `当前配置`
- `实时信号`
- `证明要点`

Copy contract:
- Replace generic “panel” wording with summary/proof wording.
- Keep proof copy to short labels and one-line support text.
- Prefer live value rows such as `节点数量：3` over narrative sentences when the UI is expressing current state.
- Preserve real seam names such as `Editor.Session` where they help users map UI proof to the SDK.

---

## Phase-Specific Layout Contract

### Graph Intro Strip

- The intro strip must show the live session story on first read, not just a generic graph title.
- It should include compact ownership cues for host-controlled seams and shared runtime state.
- It should also surface the current active host group as a compact badge or equivalent cue.

### Drawer Header And Sections

- Replace the generic drawer overline with a proof-oriented control-surface label.
- `运行时` and `证明` groups should render dedicated configuration/proof sections rather than a generic paragraph list.
- Dense sections should fit within the existing 320-360px drawer width.

### Proof Density

- No drawer group should rely on a tall stack of paragraph-only cards as its primary explanation mode.
- Short helper text is allowed, but the dominant pattern must be labels, values, and compact proof rows.

---

## Phase-Specific Interaction Contract

### Ownership And Continuity

- The active host group cue must update when `SelectedHostMenuGroupTitle` changes.
- View/behavior toggles must immediately update the current-configuration proof rows.
- Runtime proof rows must stay bound to the current inspection snapshot and diagnostics path.

### Session Continuity

- None of the proof/narrative polish work may recreate `MainWindowViewModel.Editor`.
- All proof cues must continue to describe the same live `Editor.Session`.

### Documentation Alignment

- README demo wording should match the UI's core proof terms: graph-first host menu, one live session, host-owned seams, and shared runtime signals.
- Documentation should reinforce the UI story, not introduce a competing narrative.

---

## Component Inventory For This Phase

- `Menu` and `MenuItem` for compact proof-oriented action labels
- `SplitView` for the existing drawer shell
- `Border`, `TextBlock`, `ItemsControl`, and compact badges/rows for in-context proof surfaces
- `MainWindowViewModel` as the single source of truth for configuration and runtime proof projections

---

## Checker Sign-Off

- [x] Dimension 1 Copywriting: PASS
- [x] Dimension 2 Visuals: PASS
- [x] Dimension 3 Color: PASS
- [x] Dimension 4 Typography: PASS
- [x] Dimension 5 Spacing: PASS
- [x] Dimension 6 Registry Safety: PASS

**Approval:** approved 2026-04-08
