# Phase 15: Capability And Descriptor Contract Normalization - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in `15-CONTEXT.md` — this log preserves the alternatives considered.

**Date:** 2026-04-08
**Phase:** 15-capability-and-descriptor-contract-normalization
**Areas discussed:** capability discovery shape, command/menu normalization boundary, migration posture

---

## Capability discovery shape

| Option | Description | Selected |
|--------|-------------|----------|
| Keep boolean-only capability snapshot | Leave discovery at `Can*` flags and let hosts infer the rest from object shape or nullable seams. | |
| Add explicit immutable capability/service descriptors plus compatibility booleans | Make discovery explicit on the runtime path while retaining the current boolean snapshot as a compatibility layer. | X |
| Reflect directly over `AsterGraphEditorOptions`/VM properties | Treat composition objects and facade shape as the discovery mechanism. | |

**User's choice:** Auto-selected recommended option because the user asked to continue without additional discussion.
**Notes:** Phase 14 already pushed host-facing state toward snapshots; the next safe move is explicit descriptor discovery on the canonical runtime path, not more object-shape inference.

---

## Command and menu normalization boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Replace everything in one public break | Remove `ICommand`/VM-shaped menu contracts immediately and force all hosts onto the new model at once. | |
| Introduce canonical descriptor contracts with compatibility adapters | Make data/ID-based descriptors canonical while preserving `MenuItemDescriptor`, `ICommand`, and VM adapters during migration. | X |
| Defer menu/command work to Phase 16 | Only plan capability discovery here and leave command/menu normalization for later. | |

**User's choice:** Auto-selected recommended option because the roadmap already assigns `CAP-02` to this phase.
**Notes:** The current `GraphContextMenuBuilder` and `IGraphContextMenuHost` are the densest MVVM seam; Phase 15 needs to normalize them without making Avalonia cleanup or final migration lock part of the same change.

---

## Migration posture

| Option | Description | Selected |
|--------|-------------|----------|
| Break legacy augmentor/facade seams now | Remove compatibility hooks as soon as canonical descriptors exist. | |
| Keep legacy seams as compatibility-only adapters | Preserve current augmentor/facade paths, but make them consume canonical runtime/descriptor data and clearly position them as migration shims. | X |
| Freeze the current compatibility shape untouched | Avoid any compatibility posture changes during Phase 15. | |

**User's choice:** Auto-selected recommended option because staged migration is still a milestone constraint.
**Notes:** This keeps Phase 15 aligned with the milestone's phased migration strategy and leaves final compatibility locking to Phase 17.

---

## the agent's Discretion

- Exact descriptor type names and grouping.
- Exact command invocation payload shape.
- Which compatibility shims should be marked compatibility-only in this phase.

## Deferred Ideas

- Avalonia adapter cleanup remains Phase 16.
- Final migration lock remains Phase 17.
- Plugin and automation readiness proof remains Phase 18.
