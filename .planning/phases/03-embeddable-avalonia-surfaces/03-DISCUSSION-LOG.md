# Phase 3: Embeddable Avalonia Surfaces - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in `03-CONTEXT.md` — this log preserves the alternatives considered.

**Date:** 2026-03-26
**Phase:** 03-embeddable-avalonia-surfaces
**Areas discussed:** surface granularity, standalone canvas, standalone inspector, standalone mini map, shell chrome, default menu presenter

---

## Surface Granularity

| Option | Description | Selected |
|--------|-------------|----------|
| Medium-grain decomposition | Full shell plus large reusable surfaces, not tiny shell fragments | ✓ |
| Coarse-grain decomposition | Only a few large shell variants | |
| Fine-grain decomposition | Publish every shell section as first-class public surface | |

**User's choice:** Medium-grain decomposition
**Notes:** Phase 3 should not split all the way down to every small shell widget.

---

## First-Class Surfaces

| Option | Description | Selected |
|--------|-------------|----------|
| `FullShell + Canvas + Inspector + MiniMap` | First public surface set balances reuse and scope | ✓ |
| `FullShell + Canvas + Inspector + MiniMap + Library` | Also lift the node library this phase | |
| `FullShell + Canvas + SidePanel` | Keep right side coarse and do less decomposition | |

**User's choice:** `FullShell + Canvas + Inspector + MiniMap`
**Notes:** `Library`, `Header`, and `Status` are not mandatory first-class surfaces in this phase.

---

## Standalone Canvas Responsibilities

| Option | Description | Selected |
|--------|-------------|----------|
| Open-box interactive canvas | Keep default graph interaction behavior built in | ✓ |
| Semi-hosted canvas | Keep some interaction but externalize more shell behavior | |
| Mostly rendering-only canvas | Let the host own most interaction behavior | |

**User's choice:** Open-box interactive canvas
**Notes:** Standalone canvas should still provide node interaction, connection interaction, selection, and viewport behavior without host re-wiring.

---

## Canvas Default Menu And Shortcuts

| Option | Description | Selected |
|--------|-------------|----------|
| Enabled by default, but host can disable | Best default usability while preserving embedding control | ✓ |
| No built-in menu/shortcuts by default | Host opts in explicitly | |
| Keep menu, externalize shortcuts | Split the behavior surface | |

**User's choice:** Enabled by default, but host can disable
**Notes:** Hosts should have explicit toggles to disable built-in default menu and shortcut behavior.

---

## Standalone Inspector Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Pure inspector | Selection, node info, connections, parameters only | ✓ |
| Mixed inspector | Inspector plus workspace/fragment operations | |
| Full right-side compatibility panel | Keep most of the current side panel content | |

**User's choice:** Pure inspector
**Notes:** `workspace`, `fragments`, and `shortcuts` should not ship as part of the standalone inspector surface.

---

## Standalone Mini Map Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Narrow mini map control | Overview and viewport drag only | ✓ |
| Light mini map panel | Add title and light shell wrapper | |
| Mini map plus status summary | Add extra shell/status data | |

**User's choice:** Narrow mini map control
**Notes:** Keep it as a reusable control instead of making it a shell-styled widget.

---

## Shell Chrome Publication Level

| Option | Description | Selected |
|--------|-------------|----------|
| Omittable chrome, not mandatory first-class controls | Hosts can leave shell chrome behind without Phase 3 publishing each chrome block separately | ✓ |
| Also publish the library as a first-class control | Middle path that lifts one more shell block | |
| Publish header/library/status as first-class controls now | Full chrome decomposition in Phase 3 | |

**User's choice:** Omittable chrome, not mandatory first-class controls
**Notes:** Full shell should still exist, but standalone composition should not depend on publishing every chrome block now.

---

## Default Menu Presenter Publication Level

| Option | Description | Selected |
|--------|-------------|----------|
| Public usable default presenter, no replacement yet | Keep stock menu path available without stepping into Phase 4 replacement work | ✓ |
| Keep presenter internal | Minimum scope, weaker embeddable-menu story | |
| Public interface plus swappable implementations | More complete, but crosses into Phase 4 | |

**User's choice:** Public usable default presenter, no replacement yet
**Notes:** Default menu surface should become publicly usable, but presenter replacement remains a later-phase concern.

---

## the agent's Discretion

- Exact names and factory/options shape for the new standalone Avalonia surfaces
- Exact toggle shape for disabling built-in standalone-canvas menus and shortcuts

## Deferred Ideas

- First-class public header/library/status controls if medium-grain decomposition proves insufficient
- Fully replaceable menu presenter contracts and alternate visual implementations in a later phase
