---
phase: 36-consumer-path-and-state-contract-closure
researched: 2026-04-16
status: complete
---

# Phase 36 Research

## Focus

Phase 36 needs to close two related gaps:

1. consumers still have to infer the shortest adoption route from multiple docs instead of seeing one compact route contract
2. history/save/dirty semantics are already proven in tests and smoke output, but the rules themselves are not yet published as a concise host-facing contract

## Evidence Collected

### 1. The minimal consumer sample already exists and now proves packed-package consumption

- `tools/AsterGraph.HostSample` now exists in the solution and is part of the automated proof ring.
- Phase 35 made the release lane run `HostSample` both:
  - against project references for focused contract proof
  - against packed packages for release validation

Conclusion: Phase 36 should not invent another sample. It should make the existing canonical path easier to discover and follow.

### 2. Consumer routes are present, but still spread across long-form docs

- `docs/quick-start.md` already lists the three canonical paths.
- `docs/host-integration.md` expands them with examples and package notes.
- README also references runtime-only, shipped UI, trust/discovery, and automation surfaces.

Gap: the docs do not yet present one compact route matrix that tells a consumer, in one place:

- which packages to reference
- which public entry point to start from
- which local proof command verifies that route

### 3. History/save/dirty rules are already encoded in focused proof

Focused tests already describe the real behavior:

- `GraphEditorHistoryInteractionTests`
  - saving makes the current state clean
  - undo from a saved state returns to the saved baseline and clears dirty
  - no-op drag does not leave dirty latched
- `GraphEditorSaveBoundaryTests`
  - save resets dirty
  - undo after save makes the editor dirty
  - redo back to the saved state clears dirty again
- `GraphEditorHistorySemanticTests`
  - mixed retained + session mutations still resolve through one kernel-owned save/history authority
- `ScaleSmoke`
  - emits `SCALE_HISTORY_CONTRACT_OK`

Gap: these rules are not summarized in a short, consumer-facing contract document or section.

## Conclusions

### Conclusion A

Phase 36 should tighten docs around one short route matrix instead of adding more narrative duplication.

### Conclusion B

Phase 36 should publish a small history/save/dirty contract as explicit product behavior, then point both docs and proof gates at it.

### Conclusion C

Phase 36 can validate primarily through `HostSample` and the `contract` lane because the required behavior is already automated; the main work is to surface and align it.

## Phase 36 Shape

Recommended three-plan execution:

1. add a compact consumer route matrix with packages, entry points, and verification commands
2. publish the explicit history/save/dirty contract and link it from consumer docs
3. align proof/testing docs so the contract lane is clearly the gate that enforces those state rules
