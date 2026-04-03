---
gsd_state_version: 1.0
milestone: v1.1
milestone_name: host-boundary-native-integration-and-scaling
status: Ready to execute
stopped_at: Milestone v1.1 initialized
last_updated: "2026-04-03T00:00:00Z"
progress:
  total_phases: 6
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-03)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Milestone v1.1 hardening roadmap

## Current Position

Phase: 07 (runtime host boundary completion) — NOT STARTED  
Plan: —  
Status: Roadmap created, ready for phase discussion/planning  
Last activity: 2026-04-03 — Milestone v1.1 initialized and roadmap reset around host boundary, native integration, and scaling

## Accumulated Context

### Decisions

Carry-forward decisions from the v1.0 foundation milestone:

- Keep the four-package SDK boundary (`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`) as the supported publish surface.
- Preserve `GraphEditorViewModel` and `GraphEditorView` as compatibility facades during phased migration.
- Keep embeddable surfaces, presenter replacement, and diagnostics in place as validated foundation work rather than reopening those goals from scratch.

New milestone decisions:

- Treat runtime/session self-sufficiency as the next highest SDK risk to retire.
- Reduce MVVM leakage in public seams before attempting more ambitious host/platform expansion.
- Treat full-shell shortcut routing, wheel behavior, focus, and keyboard menu behavior as host-integration requirements, not polish.
- Prioritize graph interaction hot paths and state recomputation hot spots before speculative broader optimization.
- Require proof-ring validation through tests, HostSample, PackageSmoke, and large-graph scenarios for this milestone.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the main workspace.
- The next milestone touches core runtime and Avalonia integration seams, so maintaining compatibility facades while shrinking concrete leakage is the main design constraint.
- Large-graph performance work should be validated with repeatable scenarios rather than ad hoc manual feel checks.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-03
Stopped at: Milestone v1.1 initialized
Resume file: None
