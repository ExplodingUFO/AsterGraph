# Phase 1: Consumption & Compatibility Guardrails - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-25
**Phase:** 01-Consumption & Compatibility Guardrails
**Areas discussed:** Package boundary, Initialization entry points, Migration compatibility

---

## Package Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Freeze current default boundary | Keep `AsterGraph.Avalonia + AsterGraph.Abstractions` as the recommended default and treat `Editor/Core` as optional advanced dependencies | |
| Promote `AsterGraph.Editor` as a standard entry package | Treat `Editor` as part of the intended public host-consumption path rather than merely optional/internal | ✓ |
| Avoid freezing the package boundary yet | Limit Phase 1 to docs cleanup without turning package guidance into a stronger public contract | |

**User's choice:** Promote `AsterGraph.Editor` as a standard entry package.
**Notes:** User chose a more aggressive package-boundary stance for external hosts instead of keeping the current recommendation unchanged.

---

## Initialization Entry Points

| Option | Description | Selected |
|--------|-------------|----------|
| Add formal entry points and keep compatibility path | Introduce registration/options/factory-style public entry points while preserving the current manual construction path | ✓ |
| Only document the current constructor path | Keep `GraphEditorViewModel` construction as the sole documented setup path in Phase 1 | |
| Push hard toward a new builder-only model | Start actively replacing the current constructor-led host composition style | |

**User's choice:** Add formal entry points and keep the compatibility path.
**Notes:** User wants Phase 1 to establish real public entry points but not at the cost of immediately breaking the current host composition model.

---

## Migration Compatibility

| Option | Description | Selected |
|--------|-------------|----------|
| Keep `GraphEditorViewModel` and `GraphEditorView` as compatibility facades | Use additive APIs, migration notes, and staged deprecation instead of immediate hard breaks | ✓ |
| Allow some early breaking changes in Phase 1 | Use this phase to begin removing old host-entry patterns directly | |
| Provide docs only without compatibility facade intent | Document migration but do not deliberately preserve facades or shims | |

**User's choice:** Keep `GraphEditorViewModel` and `GraphEditorView` as compatibility facades.
**Notes:** User wants a staged migration path, not a one-shot cutover, even while formal entry points are added.

---

## the agent's Discretion

- Exact API naming and package placement for the new public registration/construction helpers
- Exact obsolete/deprecation mechanics used to communicate the staged migration path

## Deferred Ideas

None.
