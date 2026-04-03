---
gsd_state_version: 1.0
milestone: v1.1
milestone_name: host-boundary-native-integration-and-scaling
status: Milestone complete
stopped_at: Milestone v1.1 completed
last_updated: "2026-04-03T12:00:00Z"
progress:
  total_phases: 6
  completed_phases: 6
  total_plans: 18
  completed_plans: 18
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-03)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Milestone v1.1 hardening roadmap

## Current Position

Phase: 12 (proof ring for hosts and large graphs) — COMPLETED  
Plan: 03  
Status: Milestone v1.1 implementation and proof ring completed  
Last activity: 2026-04-03 — Completed phases 07-12, added final proof tooling, and closed the milestone validation loop

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
- Keep large-graph validation in a dedicated smoke-style harness so proof remains repeatable without making the unit test suite timing-fragile.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the active worktree.
- Phase 12 uncovered a `net8.0` runtime-XAML loading gap for sample/smoke tools; this was closed by adding `Avalonia.Markup.Xaml.Loader` to `AsterGraph.Avalonia`.
- Future milestone work should preserve the new proof ring instead of replacing it with ad hoc spot checks.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-03
Stopped at: Milestone v1.1 completed
Resume file: None
