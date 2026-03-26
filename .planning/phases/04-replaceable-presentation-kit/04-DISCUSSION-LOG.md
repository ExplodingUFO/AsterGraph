# Phase 4: Replaceable Presentation Kit - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-26
**Phase:** 04-replaceable-presentation-kit
**Areas discussed:** presenter granularity, node visual replacement, menu presentation replacement, inspector and mini map replacement, compatibility and adoption path

---

## Presenter Granularity

| Option | Description | Selected |
|--------|-------------|----------|
| Surface-level presenter seams | Replace node/menu/inspector/mini-map presentation as medium-grain units while keeping behavior owners intact | ✓ |
| Fine-grained widget seams | Expose per-subpart public replacement points for many small shell pieces | |
| Full-shell-only replacement | Focus only on replacing one top-level shell presenter | |

**User's choice:** `[auto]` Selected the recommended surface-level presenter seam.
**Notes:** Chosen because Phase 3 already locked a medium-grain surface strategy and explicitly deferred deeper presenter replacement to Phase 4 without reopening shell chrome decomposition.

---

## Node Visual Replacement

| Option | Description | Selected |
|--------|-------------|----------|
| Behavior-owned canvas + replaceable node visual presenter | Keep `NodeCanvas` interaction ownership and swap only the node visual presentation layer | ✓ |
| Host-owned node control tree and interactions | Let hosts replace both visuals and node interaction plumbing | |
| Render-callback-only seam | Keep everything else fixed and expose only low-level draw callbacks | |

**User's choice:** `[auto]` Selected the recommended behavior-owned canvas plus replaceable node visual presenter path.
**Notes:** Chosen because `NodeCanvas` already owns drag/selection/connection behavior and the phase goal is visual replacement without forcing hosts to reimplement those behaviors.

---

## Menu Presentation Replacement

| Option | Description | Selected |
|--------|-------------|----------|
| Keep editor menu intent, swap Avalonia presenter only | Preserve `MenuItemDescriptor` and editor menu building, replace only the Avalonia presenter layer | ✓ |
| Host rebuilds menus from scratch | Hand raw menu state to the host and let it rebuild all menu visuals and wiring | |
| Keep stock presenter only | Do not add replacement at all, keep Phase 3 behavior | |

**User's choice:** `[auto]` Selected the recommended menu-intent-preserving replacement path.
**Notes:** Chosen because Phase 3 already made the stock `GraphContextMenuPresenter` public and the editor-level augmentor/path should remain valid in Phase 4.

---

## Inspector And Mini Map Replacement

| Option | Description | Selected |
|--------|-------------|----------|
| Separate replaceable presenters per surface | Replace inspector and mini-map visuals independently while reusing existing editor data/viewport behavior | ✓ |
| Single right-panel presenter seam | Merge inspector and mini-map replacement into one larger side-panel concept | |
| Defer one or both surfaces | Only replace node/menu now and leave inspector/mini-map for later | |

**User's choice:** `[auto]` Selected the recommended separate per-surface replacement seams.
**Notes:** Chosen because Phase 3 already split these into standalone public surfaces with distinct responsibilities.

---

## Compatibility And Adoption Path

| Option | Description | Selected |
|--------|-------------|----------|
| Optional replacement with stock defaults preserved | Keep existing stock shell/surfaces working and let hosts opt into replacement per surface | ✓ |
| New mandatory replacement API | Force advanced hosts onto a new replacement-first composition path | |
| Breaking shell rewrite | Replace current `GraphEditorView` story with a new presentation kit entry point | |

**User's choice:** `[auto]` Selected the recommended optional replacement path.
**Notes:** Chosen because the project constraint is phased migration, not a forced rewrite, and Phase 3 just established a coherent stock host story that should remain valid.

---

## the agent's Discretion

- Exact presenter interface names and namespace placement
- Factory-vs-template-vs-adapter mechanics for each presenter seam
- How stock/default presenters are resolved through the existing hosting factories

## Deferred Ideas

- First-class shell chrome presenter surfaces for header/library/status
- Non-Avalonia visual stacks
- Diagnostics workbench UI and deep inspection tooling
