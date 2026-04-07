# Phase 16: Avalonia Adapter Boundary Cleanup - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in `16-CONTEXT.md` — this log preserves the alternatives considered.

**Date:** 2026-04-08
**Phase:** 16-avalonia-adapter-boundary-cleanup
**Areas discussed:** default input/menu routing source, platform seam ownership boundary, public migration posture

---

## Default input/menu routing source

| Option | Description | Selected |
|--------|-------------|----------|
| Keep shortcut and menu-opening logic duplicated in shell and canvas | Leave `GraphEditorView` and `NodeCanvas` with separate default hotkey and stock menu code paths. | |
| Move default routing into shared Avalonia adapters over session descriptors | Keep gestures/UI ownership in Avalonia, but share routing logic and drive stock flow from canonical session commands/queries. | X |
| Push default routing into the editor package | Make editor/runtime own UI-level key and menu event handling directly. | |

**User's choice:** Auto-selected recommended option because the user asked to continue without additional discussion.
**Notes:** Phase 15 already made commands and menus descriptor-first on the runtime path. Phase 16 should make Avalonia consume that seam, not invent another UI-owned policy model.

---

## Platform seam ownership boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Keep clipboard and host-context wiring scattered across control code | Preserve direct `GraphEditorViewModel` setter calls and one-off seam wiring in `GraphEditorView`. | |
| Keep platform seams Avalonia-owned, but consolidate them behind thinner adapters | Preserve Avalonia ownership of clipboard/host-context/input routing while reducing direct retained-facade internal coupling. | X |
| Move clipboard and host context into editor/runtime composition | Treat these as editor-layer concerns rather than UI-platform adapters. | |

**User's choice:** Auto-selected recommended option because `ADAPT-02` explicitly keeps these seams Avalonia-owned.
**Notes:** The objective is thinner boundaries, not relocating platform concerns into the editor package.

---

## Public migration posture

| Option | Description | Selected |
|--------|-------------|----------|
| Break Avalonia presenter and factory contracts immediately | Remove compatibility-shaped public inputs in one step. | |
| Add thinner canonical seams where needed and keep compatibility entry paths alive | Tighten public contracts additively while preserving `GraphEditorView` and existing factories during migration. | X |
| Freeze public Avalonia surface entirely until Phase 17 | Avoid touching public Avalonia contracts during adapter cleanup. | |

**User's choice:** Auto-selected recommended option because staged migration remains a milestone constraint.
**Notes:** This leaves room to add canonical menu/presenter overloads if needed without forcing a one-phase public reset.

---

## the agent's Discretion

- Exact shape of the shared Avalonia routing helpers.
- Whether stock menu presentation should take canonical descriptors directly or via a narrow adapter.
- Which public Avalonia seams need additive canonical overloads versus compatibility-only documentation.

## Deferred Ideas

- Full presenter-surface redesign that removes `GraphEditorViewModel` from all Avalonia contracts.
- Non-Avalonia UI experimentation.
- Visual redesign work unrelated to adapter cleanup.
