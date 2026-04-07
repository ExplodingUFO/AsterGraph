---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: kernel-extraction-capability-contracts-and-plugin-readiness
status: Ready To Plan Phase 16
stopped_at: Phase 15 completed; Phase 16 Avalonia adapter boundary cleanup planning is next
last_updated: "2026-04-08T03:00:00Z"
progress:
  total_phases: 6
  completed_phases: 3
  total_plans: 9
  completed_plans: 9
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-04)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 16 Avalonia adapter boundary cleanup planning after Phase 15 descriptor normalization completed

## Current Position

Phase: 15. Capability And Descriptor Contract Normalization  
Plan: 03  
Status: Phase 15 completed; ready to plan Phase 16  
Last activity: 2026-04-08 — Completed plans 15-01 through 15-03, locked descriptor-first proof/sample outputs, and closed Phase 15

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
- Keep explicit runtime feature discovery and inspection descriptor reuse as the locked Phase 15 baseline while command/menu normalization proceeds.
- Keep retained `BuildContextMenu(...)` on a compatibility adapter over canonical descriptor generation instead of letting MVVM `ICommand` objects remain the stock source of truth.
- Treat Phase 16 Avalonia adapter cleanup as the next leverage point now that runtime discovery and menu/command contracts are descriptor-first.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the main workspace.
- The main design constraint is preserving staged migration while continuing to move public control-plane seams away from MVVM object shape.
- Capability discovery and descriptor normalization are now the highest-leverage architecture debt left in the milestone.
- The main execution risk for Phase 15 is splitting canonical descriptor contracts from existing `ICommand` / menu presenter compatibility without letting the two models drift.
- The next concrete risk is Phase 15-02 over-reaching into Avalonia cleanup instead of keeping command/menu normalization editor-layer and descriptor-first.
- Known transaction-history test failures remain present even on the pre-15-02 baseline; they should not be misclassified as Phase 15 regressions when Phase 16 starts.
- The next design constraint is making Avalonia consume the thinner descriptor/runtime contracts without recreating editor policy in the UI layer.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 15 completed; Phase 16 Avalonia adapter boundary cleanup planning is next
Resume file: None
