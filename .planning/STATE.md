---
gsd_state_version: 1.0
milestone: v1.3
milestone_name: Demo Showcase
status: Ready to plan
stopped_at: Phase 20 completed; next step is to plan Phase 21 in-context proof and narrative polish
last_updated: "2026-04-08T08:02:58.152Z"
progress:
  total_phases: 3
  completed_phases: 2
  total_plans: 6
  completed_plans: 6
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-08)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 21 in-context proof and narrative polish planning

## Current Position

Phase: 21
Plan: Not started

## Accumulated Context

### Decisions

Carry-forward decisions from shipped milestones:

- Keep the four-package SDK boundary (`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`) as the supported publish surface.
- Keep `CreateSession(...)` and `Create(...)` as the canonical composition routes, while `GraphEditorViewModel` / `GraphEditorView` remain the retained compatibility window.
- Prefer descriptor and snapshot control-plane contracts over exposing MVVM implementation types as the canonical host surface.
- Keep Avalonia as an adapter layer over shared runtime contracts and proof-backed platform seams.
- Preserve `HostSample`, `PackageSmoke`, and `ScaleSmoke` as the runnable proof ring for migration, readiness, and future extension work.
- Lead v1.3 with a demo showcase refresh so the shipped SDK story is easier to understand before adding another layer of runtime features.
- Keep the next demo experience on one live graph session controlled by host-level menus rather than switching between canned scenes.
- Keep the new in-window host menu as the first visible shell control plane in the demo.
- Keep secondary showcase detail behind a compact on-demand pane so the graph remains the dominant surface.
- Keep the top host menu as the first control plane and use direct checkable menu items for the highest-signal view/behavior toggles.
- Keep the right-side `SplitView` pane as the compact dense control/readout surface for the currently active menu group.
- Keep all Phase 20 control state bound to the existing `MainWindowViewModel` booleans and `Editor.Session` projections rather than duplicating editor or runtime state.
- Keep runtime summary metrics and recent diagnostics as separate sections inside the existing right-side pane instead of introducing a second runtime dashboard.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No active blocker in the main workspace.
- The known `STATE_HISTORY_OK` mismatch remains a pre-existing baseline issue if the next milestone touches history/save semantics.
- The next milestone should avoid reopening kernel/facade ownership drift unless new evidence requires it.
- The demo redesign should stay focused on showcase clarity, not expand into unrelated end-user feature work.
- Phase 20 should keep the host menu shallow and compact; over-nesting or ribbon-like growth would undercut the graph-first shell.
- Runtime readouts should stay compact in this phase; broader proof/narrative cleanup remains Phase 21 work.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 20 completed; next step is to plan Phase 21 in-context proof and narrative polish
Resume file: .planning/phases/20-host-menu-control-consolidation/20-VERIFICATION.md
