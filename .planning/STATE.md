---
gsd_state_version: 1.0
milestone: v1.4
milestone_name: Plugin Loading and Automation Execution
status: Phase 24 Complete
stopped_at: Phase 24 complete; next step is to plan plugin and automation proof ring
last_updated: "2026-04-08T10:55:43.3035672Z"
last_activity: 2026-04-08
progress:
  total_phases: 4
  completed_phases: 3
  total_plans: 12
  completed_plans: 9
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-08)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 25 plugin and automation proof ring planning

## Current Position

Phase: 24
Plan: Execution Complete
Status: Phase 24 Complete
Last activity: 2026-04-08 — Completed Phase 24 automation execution runner

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
- Return to plugin and automation implementation after the showcase milestone instead of extending presentation-only work indefinitely.
- Keep plugin loading and automation execution rooted in `IGraphEditorSession`, descriptors, and command IDs rather than retained MVVM compatibility APIs.
- Keep the first public plugin surface inside `AsterGraph.Editor` and rooted in `AsterGraphEditorOptions` / `AsterGraphEditorFactory`, not `AsterGraph.Avalonia`.
- Use a custom `AssemblyLoadContext` plus `AssemblyDependencyResolver` for assembly-path plugins while keeping shared `AsterGraph.*` runtime contracts in the default load context.
- Keep Phase 22 limited to public plugin composition/loading contracts and loader discoverability; actual plugin-contributed seam integration remains Phase 23.
- Keep plugin-load failures recoverable and descriptor-first through canonical session diagnostics instead of factory-thrown ordinary host failures.
- Keep Phase 23 focused on live integration of the explicit Phase 22 plugin contribution contracts plus canonical plugin inspection, rather than widening into arbitrary plugin-owned service injection.
- Keep plugin inspection readable through canonical query/inspection DTOs, with diagnostics history as supporting evidence rather than the only source of truth.
- Keep host-supplied context-menu, localization, and node-presentation seams as the final override layer when plugin contributions are composed.
- Keep plugin load inspection parity across `CreateSession(...)` and `Create(...)` so retained compatibility hosts do not lose canonical runtime visibility.
- Keep plugin node definitions, menu augmentation, localization, and presentation composed through one factory/session path rather than retained-only compatibility hooks.
- Keep the first automation baseline rooted in `IGraphEditorSession`, stable command IDs, query snapshots, and mutation batching rather than `GraphEditorViewModel` methods.
- Keep automation progress/failure visibility on typed runtime events plus machine-readable diagnostics instead of status-text scraping or UI-bound progress surfaces.
- Keep the first automation runner synchronous, in-process, and descriptor-first rather than expanding into a workflow engine or script host in the same phase.
- Reuse the existing mutation scope as the automation batching primitive instead of introducing a parallel transaction model.

### Pending Todos

None captured yet.

### Blockers/Concerns

- No active blocker in the main workspace.
- The known `STATE_HISTORY_OK` mismatch remains a pre-existing baseline issue if the next milestone touches history/save semantics.
- `GraphEditorTransactionTests.GraphEditorViewModel_HistoryInteraction_PreservesUndoAndDirtySemantics` and `GraphEditorViewModel_SaveBoundary_PreservesUndoRedoDirtySemantics` still fail on the detached clean `d7939a5` baseline; they remain a carry-forward proof concern rather than a Phase 24 regression.
- The next milestone should avoid reopening kernel/facade ownership drift unless new evidence requires it.
- The first automation baseline should stay command/query/batch driven rather than expand into a full scripting language or workflow-designer product.
- Proof work must stay aligned across focused tests, `HostSample`, `PackageSmoke`, and `ScaleSmoke` so extension claims remain machine-checkable.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |

## Session Continuity

Last session: 2026-04-08
Stopped at: Phase 24 complete; next step is to plan plugin and automation proof ring
Resume file: .planning/phases/24-automation-execution-runner/24-VERIFICATION.md
