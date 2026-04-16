---
gsd_state_version: 1.0
milestone: v1.6
milestone_name: Facade Convergence and Proof Guardrails
status: executing
stopped_at: Phase 33 planning complete
last_updated: "2026-04-16T06:55:00.0000000Z"
last_activity: 2026-04-16 -- Phase 33 planning complete
progress:
  total_phases: 4
  completed_phases: 3
  total_plans: 12
  completed_plans: 9
  percent: 75
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 33 ready to execute - kernel-canvas-and-guardrail-follow-through

## Current Position

Phase: 33 (kernel-canvas-and-guardrail-follow-through) - READY TO EXECUTE
Plan: 3 plans ready
Status: Ready to execute
Last activity: 2026-04-16 -- Phase 33 planning complete

## Performance Metrics

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| Phase 30 P01 | 1 min | 2 tasks | 7 files |
| Phase 30 P02 | 1 min | 2 tasks | 1 file |
| Phase 30 P03 | 1 min | 2 tasks | 5 files |

## Accumulated Context

### Decisions

Carry-forward decisions from shipped milestones:

- Keep the four-package SDK boundary (`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`) as the supported publish surface.
- Keep `CreateSession(...)` and `Create(...)` as the canonical composition routes, while `GraphEditorViewModel` / `GraphEditorView` remain the retained compatibility window.
- Prefer descriptor and snapshot control-plane contracts over exposing MVVM implementation types as the canonical host surface.
- Keep Avalonia as an adapter layer over shared runtime contracts and proof-backed platform seams.
- Preserve `PackageSmoke` and `ScaleSmoke` as runnable proof surfaces for migration, readiness, and future extension work.
- Keep plugin loading and automation execution rooted in `IGraphEditorSession`, descriptors, and command IDs rather than retained MVVM compatibility APIs.
- Keep plugin load failures and automation telemetry recoverable and machine-readable through canonical runtime diagnostics/events.

New v1.6 framing decisions:

- Use the next milestone for contraction and hotspot guardrails rather than another plugin/automation capability band.
- Build on the v1.5 quality baseline instead of reopening already-shipped `.editorconfig`, central package management, CI, or `ScaleSmoke` alignment work.
- Treat the missing `v1.4` archive and the carried `STATE_HISTORY_OK` mismatch as current milestone work, not passive background debt.
- Continue phase numbering from 30 because the latest executed phase is 29.
- [Phase 30]: Reconstruct the missing v1.4 archive from historical snapshot commits instead of rewriting current milestone docs. — The archive needed explicit git-backed evidence so the recovered roadmap and requirements keep their original framing.
- [Phase 30]: Record later pre-v1.5 trust, discovery, staging, and proof follow-up in the v1.4 ledger notes instead of editing it back into the archived snapshots. — The delivered pre-v1.5 surface exceeded the original four-phase roadmap, so the ledger needed to separate original framing from later follow-up honestly.
- [Phase 30]: Keep the maintenance guardrail inside `eng/ci.ps1` as a dedicated `maintenance` lane instead of introducing a second script path. — Contributors and docs now have one repo-local refactor gate to point at.
- [Phase 30]: Keep `maintenance` narrower than `release` by running focused hotspot suites plus `ScaleSmoke`, without pack, `PackageSmoke`, coverage, or package validation. — The new lane stays fast enough for refactor loops while still exercising one runnable readiness proof.
- [Phase 31]: Retained mutation completion and retained save must commit through kernel-owned history/save authority instead of keeping a second retained undo/redo owner. — Mixed runtime-plus-retained flows were already shipping, so semantic parity had to be fixed at the authority boundary rather than by adding more compatibility cases.
- [Phase 31]: Successful save must replace the current history entry rather than append a new undo step. — Save-boundary dirty semantics only stay stable when the saved snapshot becomes the current history baseline.
- [Phase 31]: `ScaleSmoke` and proof-ring tests should expose explicit history-contract pass/fail output instead of a carried known-mismatch tuple. — Planning notes were no longer an acceptable substitute for machine-checkable proof.
- [Phase 32 planning]: The next highest-value facade contraction is bootstrap plus retained-only compatibility/fragment orchestration, not kernel or canvas refactoring. — `GraphEditorViewModel` is still the composition hotspot, while fragment/menu collaborators already exist and can be lifted cleanly behind the existing host pattern.
- [Phase 32 planning]: Phase 32 must keep public retained entry points stable and use focused menu/fragment/service-seam parity tests as the main guardrail. — The remaining risk is internal drift under a supported public compatibility surface, not missing capability.
- [Phase 32]: Keep retained public methods as thin delegations while moving bootstrap, descriptor assembly, context-menu compatibility, and fragment orchestration into narrower internal collaborators. — This materially reduces `GraphEditorViewModel` ownership without reopening public factory/session/view-model APIs.
- [Phase 32]: Promote retained command collaborators to namespace-level internal types while keeping private host adapters in `GraphEditorViewModel`. — The extracted services are now easier to change in isolation and still preserve kernel-owned runtime authority.
- [Phase 33 planning]: The next kernel hotspot should be command-router host ownership, because node/connection/workspace mutation paths already use dedicated host adapters while command routing still binds directly to GraphEditorKernel. — This is the clearest remaining inline ownership burden in the kernel.
- [Phase 33 planning]: The next NodeCanvas hotspot should be lifecycle/property-routing glue, not drag or pointer math. — Pointer, drag, wheel, scene, overlay, and context-menu behavior already live in dedicated coordinators, but attach/property-change/bootstrap routing is still inline in NodeCanvas.axaml.cs.
- [Phase 33 planning]: Repo-wide `CS1591` suppression is now broader than the real debt boundary. — `AsterGraph.Abstractions` and `AsterGraph.Core` build clean under `warnaserror:CS1591`, while `AsterGraph.Editor` still has explicit public-doc debt that should move to project scope.

### Pending Todos

None captured yet.

### Blockers/Concerns

- `GraphEditorKernel` and `NodeCanvas` remain the next obvious internal hotspots after the retained facade contraction completed.
- Publishable-package XML doc debt still needs the scoped cleanup planned for Phase 33.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-16T06:55:00.0000000Z
Stopped at: Phase 33 planning complete
Resume file: .planning/phases/33-kernel-canvas-and-guardrail-follow-through/33-01-PLAN.md
