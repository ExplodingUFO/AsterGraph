---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: kernel-extraction-capability-contracts-and-plugin-readiness
status: Ready To Plan Phase 17
stopped_at: Phase 16 completed; Phase 17 planning is next
last_updated: "2026-04-08T05:22:45Z"
progress:
  total_phases: 6
  completed_phases: 4
  total_plans: 12
  completed_plans: 12
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-04)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 17 planning after Phase 16 closed the Avalonia adapter-boundary cleanup with proof/sample/docs lock-up

## Current Position

Phase: 17. Compatibility Lock And Migration Proof
Plan: Not started
Status: Ready To Plan Phase 17
Last activity: 2026-04-08 — Completed 16-03 proof/sample/docs lock-up and closed Phase 16

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
- Keep the new `PHASE16_*` HostSample and PackageSmoke markers as the proof baseline for Avalonia adapter-boundary behavior until Phase 17 extends migration coverage further.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the main workspace.
- The main design constraint is preserving staged migration while continuing to move public control-plane seams away from MVVM object shape.
- Known transaction-history test failures remain present even on the pre-15-02 baseline; they should not be misclassified as Phase 15 regressions when Phase 16 starts.
- `GraphEditorView.axaml.cs` and `NodeCanvas.axaml.cs` remain the main Avalonia coordination hotspots and should be changed with proof coverage, not opportunistically.
- The next leverage point is migration proof: Phase 17 needs to keep the retained `GraphEditorViewModel` / `GraphEditorView` path behaviorally aligned while the kernel-first/runtime-descriptor path becomes the clearer canonical route.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 16 completed; Phase 17 planning is next
Resume file: .planning/ROADMAP.md
