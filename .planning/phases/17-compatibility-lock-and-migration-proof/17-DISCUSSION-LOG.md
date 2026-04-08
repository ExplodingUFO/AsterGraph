# Phase 17: Compatibility Lock And Migration Proof - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in `17-CONTEXT.md` - this log preserves the alternatives considered.

**Date:** 2026-04-08
**Phase:** 17-compatibility-lock-and-migration-proof
**Areas discussed:** migration-window posture, proof matrix scope, canonical-route signaling

---

## Migration-window posture

| Option | Description | Selected |
|--------|-------------|----------|
| Start deprecating `GraphEditorViewModel` / `GraphEditorView` aggressively now | Push hosts toward the canonical path by immediate obsolescence pressure. | |
| Keep retained constructors/views supported, but label them explicitly as compatibility-only while proving alignment | Preserve the migration window and make the canonical route clearer without forcing a rewrite yet. | X |
| Freeze wording and avoid touching migration posture until Phase 18 | Delay explicit signaling so proof remains purely behavioral. | |

**User's choice:** Auto-selected recommended option because `MIG-01` requires a staged migration path, not a forced cutover.
**Notes:** This phase is about locking and proving the window, not closing it.

---

## Proof matrix scope

| Option | Description | Selected |
|--------|-------------|----------|
| Rely mostly on HostSample and PackageSmoke | Treat tests as secondary and use samples/smoke as the main migration proof. | |
| Use focused regressions plus HostSample and PackageSmoke together | Keep tests as the authoritative parity gate, with sample/smoke repeating the same story in host-facing form. | X |
| Add broad end-to-end UI automation first | Build a much larger UI proof system before tightening the existing focused tests. | |

**User's choice:** Auto-selected recommended option because this repo already uses tests + HostSample + PackageSmoke as its phase-close proof ring.
**Notes:** Phase 17 should extend that pattern instead of replacing it.

---

## Canonical-route signaling strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Keep canonical-vs-compatibility guidance mostly in planning artifacts | Let phase summaries carry the story without repeating it in API comments and user docs. | |
| Align XML docs, READMEs, quick start, and proof output around one migration story | Make the same canonical/compatibility guidance visible in code comments, docs, and proof tools. | X |
| Signal canonical status only through code-level `Obsolete` attributes | Use compiler warnings as the primary migration communication channel. | |

**User's choice:** Auto-selected recommended option because current drift is as much a communication problem as a runtime problem.
**Notes:** Narrow shims can keep targeted annotations, but the main retained path should remain supported and clearly documented during this phase.

---

## the agent's Discretion

- Exact shape of the parity helper/signature model in tests.
- Which docs need the strongest migration wording first.
- Whether any new Phase 17 markers should be human-readable only, machine-checkable only, or both.

## Deferred Ideas

- Immediate hard deprecation of `GraphEditorViewModel` / `GraphEditorView`
- Broader API removal planning
- Plugin-readiness proof work that belongs to Phase 18
