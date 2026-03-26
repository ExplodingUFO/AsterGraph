# Phase 2: Runtime Contracts & Service Seams - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-26
**Phase:** 02-Runtime Contracts & Service Seams
**Areas discussed:** Compatibility strategy, runtime contract shape, replaceable service priorities, batching/transactions

---

## Compatibility Strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Keep `GraphEditorViewModel` as compatibility facade | Extract new runtime contracts behind it instead of replacing it immediately | ✓ |
| Replace `GraphEditorViewModel` directly | Make the new runtime surface the only public path in Phase 2 | |
| Keep adding features to `GraphEditorViewModel` first | Delay extraction and continue the monolithic path for now | |

**User's choice:** Keep `GraphEditorViewModel` as the compatibility facade.
**Notes:** Phase 2 should reduce facade responsibility, not remove the facade itself.

---

## Runtime Contract Shape

| Option | Description | Selected |
|--------|-------------|----------|
| `session / state / commands / queries / events` | Build a framework-neutral runtime API around these core surfaces | ✓ |
| Commands-only focus | Defer public queries/events until later | |
| Service-first only | Extract storage/services first without a coherent session contract | |

**User's choice:** `session / state / commands / queries / events`
**Notes:** The host must be able to drive behavior without Avalonia control internals.

---

## Replaceable Service Priorities

| Option | Description | Selected |
|--------|-------------|----------|
| Workspace/fragment/template/clipboard/diagnostics first | Extract the most persistence-heavy and currently fragile seams first | ✓ |
| Localization/presentation/menu first | Make existing presentation seams the main focus of this phase | |
| Evenly extract everything | Treat all seams as equal priority in one sweep | |

**User's choice:** Workspace/fragment/template/clipboard/diagnostics first.
**Notes:** Existing localization/presentation/menu seams stay in scope for compatibility, but they are not the main extraction target.

---

## Batching And Transactions

| Option | Description | Selected |
|--------|-------------|----------|
| Lightweight mutation scope / transaction API | Support grouped mutations and coherent notifications without a heavy bus architecture | ✓ |
| Full command bus / messaging model | Introduce generalized runtime messaging in Phase 2 | |
| No public batching yet | Defer batching/transactions to a later phase | |

**User's choice:** Lightweight mutation scope / transaction API.
**Notes:** The batching surface should be practical and host-facing, not a generalized architecture project.

---

## the agent's Discretion

- Exact contract names and file layout inside `AsterGraph.Editor`
- Whether the batching API is disposable-scope, callback-based, or explicit begin/end

## Deferred Ideas

None.
