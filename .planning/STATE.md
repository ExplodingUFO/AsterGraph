---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: kernel-extraction-capability-contracts-and-plugin-readiness
status: Planning Phase 14
stopped_at: Phase 14 planning prepared after Phase 13 integration
last_updated: "2026-04-04T12:00:00Z"
progress:
  total_phases: 6
  completed_phases: 1
  total_plans: 6
  completed_plans: 3
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-04)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 14 planning after Phase 13 kernel extraction landed on `master`

## Current Position

Phase: 14. Session And Compatibility Facade Decoupling  
Plan: 14-01 through 14-03 drafted  
Status: Planning Phase 14  
Last activity: 2026-04-04 — Phase 13 merged to `master`, snapshot hardening added, and Phase 14 planning artifacts created

## Accumulated Context

### Decisions

Carry-forward decisions from completed milestones:

- Keep the four-package SDK boundary (`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`) as the supported publish surface.
- Preserve `GraphEditorViewModel` and `GraphEditorView` as compatibility facades during phased migration.
- Keep embeddable surfaces, presenter replacement, diagnostics, host proof tools, and large-graph smoke validation as retained foundation assets rather than reopening them from scratch.

New milestone decisions:

- Treat the remaining architectural center-of-gravity problem, not raw feature absence, as the next product risk.
- Extract the canonical editor state owner from `GraphEditorViewModel` before attempting plugin loading or richer automation.
- Normalize capability, menu, and command contracts around explicit descriptors rather than MVVM object shape.
- Keep the Avalonia layer as an adapter over kernel contracts, not the implicit owner of command-routing policy.
- Preserve the new proof ring while shifting the canonical composition path toward a kernel-first runtime.
- Treat `GraphEditorViewModel` as a retained compatibility façade that now needs adapter conversion, not further runtime ownership expansion.
- Prioritize detached/read-only host state on canonical runtime APIs whenever a choice must be made between convenience and boundary safety.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the main workspace.
- The main design constraint is preserving staged migration while changing the runtime center of gravity.
- Dual-path runtime drift between kernel-first session composition and the retained façade path is now the highest-leverage architecture debt.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-04
Stopped at: Phase 14 planning prepared after Phase 13 integration
Resume file: None
