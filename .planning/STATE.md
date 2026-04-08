---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: kernel-extraction-capability-contracts-and-plugin-readiness
status: Executing Phase 16
stopped_at: Phase 16 plan 16-02 completed; 16-03 proof and docs lock-up is next
last_updated: "2026-04-08T05:10:27Z"
progress:
  total_phases: 6
  completed_phases: 3
  total_plans: 12
  completed_plans: 11
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-04)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 16 execution after 16-02 narrowed Avalonia clipboard and host-context seam attachment through a shared platform binder

## Current Position

Phase: 16. Avalonia Adapter Boundary Cleanup
Plan: 16-01 and 16-02 completed; 16-03 pending
Status: Executing Phase 16
Last activity: 2026-04-08 — Completed 16-02 platform seam narrowing and verified initialization/migration/host-context/standalone regressions

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
- Make shared Avalonia shortcut and stock context-menu routing over session descriptors the first Phase 16 execution target.
- Keep clipboard and host-context seams Avalonia-owned, but consolidate them behind thinner adapter touchpoints during Phase 16.
- Keep the stock Avalonia context-menu presenter canonical-descriptor-driven while allowing custom presenters to migrate through an additive overload plus compatibility fallback.
- Keep shared shortcut routing centralized in Avalonia, with copy/paste still compatibility-executed until the runtime surface grows canonical clipboard command IDs.
- Keep full-shell platform seam ownership on `GraphEditorView`, while allowing supported standalone `NodeCanvas` composition to attach the same Avalonia seams through a shared binder.
- Treat inspector/minimap/node-visual presenter contracts as compatibility-oriented seams for now; Phase 16 narrows their surrounding platform wiring without redesigning those contracts.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the main workspace.
- The main design constraint is preserving staged migration while continuing to move public control-plane seams away from MVVM object shape.
- Avalonia adapter thinning is now the highest-leverage architecture debt left in the milestone.
- Known transaction-history test failures remain present even on the pre-15-02 baseline; they should not be misclassified as Phase 15 regressions when Phase 16 starts.
- The next design constraint is making Avalonia consume the thinner descriptor/runtime contracts without recreating editor policy in the UI layer.
- The main Phase 16 risk is mixing input/menu policy cleanup with broader presenter-surface redesign instead of first collapsing duplicated shell/canvas routing.
- `GraphEditorView.axaml.cs` and `NodeCanvas.axaml.cs` remain the main Avalonia coordination hotspots and should be changed with proof coverage, not opportunistically.
- The next concrete risk is `16-03` under-proving the new platform seam story in sample/smoke/docs, leaving hosts to infer more than the artifacts actually show.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 16 plan 16-02 completed; 16-03 proof and docs lock-up is next
Resume file: .planning/phases/16-avalonia-adapter-boundary-cleanup/16-03-PLAN.md
