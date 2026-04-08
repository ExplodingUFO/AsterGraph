---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: kernel-extraction-capability-contracts-and-plugin-readiness
status: Planning Phase 18
stopped_at: Phase 18 planned; 18-01 readiness descriptors and contract proof is next
last_updated: "2026-04-08T06:07:50Z"
progress:
  total_phases: 6
  completed_phases: 5
  total_plans: 18
  completed_plans: 15
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-04)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 18 execution after planning locked the plugin-and-automation readiness proof ring into descriptor, sample/smoke/scale, and closeout waves

## Current Position

Phase: 18. Plugin And Automation Readiness Proof Ring
Plan: 18-01, 18-02, and 18-03 drafted
Status: Planning Phase 18
Last activity: 2026-04-08 — Planned Phase 18 around explicit readiness descriptors, runnable readiness markers, and milestone-close proof

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
- Prefer explicit canonical-vs-compatibility signaling through focused tests, sample/smoke markers, and API remarks before any harder deprecation posture is considered.
- Treat retained-path extra commands as compatibility-only surface area, while expecting the shared canonical command/menu subset to stay aligned with `CreateSession(...)`.
- Keep host-facing route guidance stable across XML comments, README files, quick start, and host integration docs so canonical-path messaging cannot drift independently of proof artifacts.
- Keep the new `PHASE17_*` HostSample and PackageSmoke markers as the migration-proof baseline while Phase 18 expands the proof ring toward plugin and automation readiness.
- Treat explicit seam discoverability as the first Phase 18 risk; `AsterGraphEditorOptions` already exposes the host seams, but the proof ring still needs to advertise them as readiness signals.
- Use `HostSample`, `PackageSmoke`, and `ScaleSmoke` together in Phase 18 so human-readable proof, machine-checkable package proof, and large-graph automation proof do not drift apart.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No current blocker in the main workspace.
- The main design constraint is preserving staged migration while continuing to move public control-plane seams away from MVVM object shape.
- Known transaction-history test failures remain present even on the pre-15-02 baseline; they should not be misclassified as Phase 15 regressions when Phase 16 starts.
- `GraphEditorView.axaml.cs` and `NodeCanvas.axaml.cs` remain the main Avalonia coordination hotspots and should be changed with proof coverage, not opportunistically.
- Phase 17 closed with migration proof, but the next phase should avoid reopening compatibility-route churn unless it is required by plugin/automation readiness evidence.
- The known `STATE_HISTORY_OK` mismatch in `tools/AsterGraph.PackageSmoke` is still a pre-existing baseline item, not a Phase 17 regression.
- Current runtime feature descriptors likely under-report some optional services/providers relative to `AsterGraphEditorOptions`; Phase 18 should close that gap explicitly instead of relying on docs alone.
- The main scope risk for Phase 18 is accidentally drifting into actual plugin loader or automation API work instead of keeping the phase centered on readiness proof.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 18 planned; 18-01 readiness descriptors and contract proof is next
Resume file: .planning/phases/18-plugin-and-automation-readiness-proof-ring/18-01-PLAN.md
