# Phase 21: In-Context Proof And Narrative Polish - Research

**Completed:** 2026-04-08
**Goal:** Identify the smallest implementation surface that makes the graph-first demo self-explanatory without reintroducing the old explanation-wall layout.

## Current Codebase Baseline

The required proof signals already exist in the current demo:

- `MainWindow.axaml` already has a top host menu, a graph intro strip, and dedicated drawer sections for `展示`, `视图`, `行为`, `运行时`, and `证明`.
- `MainWindowViewModel` already owns the live host-controlled booleans and the canonical runtime/session projections.
- Phase 20 already proved that runtime metrics and recent diagnostics come from `Editor.Session.Diagnostics` and inspection snapshots.

Conclusion: Phase 21 should be a narrative and projection-structure pass, not a runtime-feature phase.

## Key Findings

### 1. No new shell primitive is required

The existing `Menu`, `SplitView`, `Border`, `ItemsControl`, and graph intro strip are enough to satisfy the phase.

Implication:

- keep the current shell structure
- express proof through better labels, tighter grouping, and dedicated view-model projections
- avoid reopening layout choice debates that Phase 19/20 already settled

### 2. The main remaining problem is generic summary abstraction

The strongest leftover friction is not missing data. It is generic copy and broad summary properties:

- `打开展示面板`
- `打开证明面板`
- `宿主展示面板`
- `MainEditorSummary`
- `SelectedHostMenuGroupSummary`
- `SelectedHostMenuGroupLines`

Implication:

- move away from one-size-fits-all summary strings
- introduce more explicit proof/configuration properties with clearer ownership semantics
- keep generic summary collections only where they still add signal

### 3. Runtime proof should stay live and read-mostly

The runtime side is already technically correct. What it lacks is clearer separation between:

- host-controlled configuration
- shared runtime/session state
- diagnostics observations

Implication:

- keep runtime values sourced from the existing inspection snapshot and diagnostics path
- restructure the drawer into compact sections instead of adding another cache or “proof model”

### 4. README is the right minimum doc-alignment surface

The repository README already explains the package boundary and runtime session contracts, but it does not yet explain the demo as the new graph-first showcase.

Implication:

- add or revise a short README section describing what `AsterGraph.Demo` now proves
- keep the README language aligned with the UI terms: host menu, graph-first, one live session, shared runtime signals

## Recommended Implementation Posture

### 1. Proof-in-place, not proof-as-panel

Use the graph intro strip and the current drawer sections as the proof surface.

Recommended breakdown:

- intro strip: immediate ownership cues and active-group context
- runtime/proof drawer sections: compact configuration rows, runtime signals, and seam-ownership evidence
- menu copy: concise summary/proof wording instead of legacy panel wording

### 2. Dedicated properties beat generic summary prose

Prefer purpose-built properties for:

- ownership badges
- current configuration rows
- runtime signal rows
- seam-ownership proof lines

This keeps tests more precise and reduces the chance that one broad summary string becomes outdated.

### 3. Documentation should mirror the UI, not out-explain it

The demo should carry most of the proof visually and interactively.

The README only needs to:

- explain the graph-first showcase framing
- point readers at the host menu and same-session proof
- connect the demo story back to the supported SDK boundary

## Risks And Guardrails

- Avoid drifting back into paragraph-heavy cards inside the drawer.
- Avoid introducing a second configuration cache separate from the live view-model and inspection snapshot.
- Avoid brittle tests that lock every sentence verbatim; prefer stable labels, headings, and high-signal proof rows.
- Avoid expanding the phase into broader documentation rewrites or onboarding flows.

## Recommended Wave Structure

### Wave 1

Replace the remaining generic shell copy with compact in-context ownership cues in the menu, drawer header, and graph intro strip.

### Wave 2

Restructure runtime/proof content into live configuration and proof sections backed by dedicated properties instead of broad summary prose.

### Wave 3

Align the README and final regression coverage with the graph-first proof narrative.

---

*Research completed for Phase 21 on 2026-04-08*
