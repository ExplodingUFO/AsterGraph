---
gsd_state_version: 1.0
milestone: none
milestone_name: none
status: Awaiting next milestone
stopped_at: v1.3 Demo Showcase archived; next step is to start a new milestone
last_updated: "2026-04-08T08:43:03.6095886Z"
last_activity: 2026-04-08
progress:
  total_phases: 0
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-08)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** waiting for the next milestone definition

## Current Position

Phase: None
Plan: None
Status: Awaiting next milestone
Last activity: 2026-04-08 — Archived v1.3 Demo Showcase

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
- Keep Phase 21 proof cues embedded near the active graph and drawer sections instead of reintroducing a permanent explanation rail.
- Keep host-owned seams and shared runtime/session state explicit through short labels plus live values rather than paragraph-heavy summary cards.
- Keep README aligned with the graph-first showcase story so the demo window and repo narrative describe the same proof.
- Keep the demo showcase positioned as a host-facing SDK proof surface rather than a productized workflow shell or scene-switching gallery.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No active blocker in the main workspace.
- The known `STATE_HISTORY_OK` mismatch remains a pre-existing baseline issue if the next milestone touches history/save semantics.
- The next milestone should avoid reopening kernel/facade ownership drift unless new evidence requires it.
- The demo redesign should stay focused on showcase clarity, not expand into unrelated end-user feature work.
- Phase 20 should keep the host menu shallow and compact; over-nesting or ribbon-like growth would undercut the graph-first shell.
- The demo showcase should remain a host-facing SDK proof surface rather than drift toward a productized workflow shell.
- Future work should start from a fresh milestone definition instead of extending archived v1.3 scope ad hoc.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: v1.3 Demo Showcase archived; next step is to start a new milestone
Resume file: .planning/milestones/v1.3-ROADMAP.md
