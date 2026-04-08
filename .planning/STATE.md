---
gsd_state_version: 1.0
milestone: v1.3
milestone_name: demo-showcase
status: Ready To Plan Phase 20
stopped_at: Phase 19 complete; next step is to plan host menu control consolidation
last_updated: "2026-04-08T07:25:00Z"
last_activity: 2026-04-08
progress:
  total_phases: 3
  completed_phases: 1
  total_plans: 3
  completed_plans: 3
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-08)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 20 host menu control consolidation planning on top of the new graph-first demo shell

## Current Position

Phase: 20
Plan: Not started
Status: Ready To Plan Phase 20
Last activity: 2026-04-08 — Completed Phase 19 graph-first demo shell work

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

### Pending Todos

None captured yet.

### Blockers/Concerns

- No active blocker in the main workspace.
- The known `STATE_HISTORY_OK` mismatch remains a pre-existing baseline issue if the next milestone touches history/save semantics.
- The next milestone should avoid reopening kernel/facade ownership drift unless new evidence requires it.
- The demo redesign should stay focused on showcase clarity, not expand into unrelated end-user feature work.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 19 complete; next step is to plan host menu control consolidation
Resume file: .planning/ROADMAP.md
