---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: kernel-extraction-capability-contracts-and-plugin-readiness
status: Planning Phase 15
stopped_at: Phase 15 planning prepared after Phase 14 closeout
last_updated: "2026-04-08T02:00:00Z"
progress:
  total_phases: 6
  completed_phases: 2
  total_plans: 9
  completed_plans: 6
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-04)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 15 capability and descriptor contract planning after Phase 14 runtime/facade decoupling completed

## Current Position

Phase: 15. Capability And Descriptor Contract Normalization  
Plan: 15-01 through 15-03 drafted  
Status: Planning Phase 15  
Last activity: 2026-04-08 — Closed Phase 14, captured Phase 15 context/research, and drafted capability/descriptor normalization plans

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
- Treat Phase 15 capability and descriptor normalization as the next highest-leverage milestone risk now that the retained session path is adapter-backed.
- Prefer additive canonical descriptor contracts with compatibility shims over one-shot public breaks while Phase 15 normalizes capability, command, and menu discovery.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the main workspace.
- The main design constraint is preserving staged migration while continuing to move public control-plane seams away from MVVM object shape.
- Capability discovery and descriptor normalization are now the highest-leverage architecture debt left in the milestone.
- The main execution risk for Phase 15 is splitting canonical descriptor contracts from existing `ICommand` / menu presenter compatibility without letting the two models drift.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 15 planning prepared after Phase 14 closeout
Resume file: None
