---
gsd_state_version: 1.0
milestone: v1.7
milestone_name: Consumer Closure / Release Hardening
status: planning
stopped_at: Milestone v1.7 initialized
last_updated: "2026-04-16T06:20:00.0000000Z"
last_activity: 2026-04-16 -- Milestone v1.7 initialized
progress:
  total_phases: 4
  completed_phases: 0
  total_plans: 12
  completed_plans: 0
  percent: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 34 ready to plan - truth-alignment-and-proof-ring-closure

## Current Position

Phase: 34 (truth-alignment-and-proof-ring-closure) - READY TO PLAN
Plan: 3 plans outlined in roadmap
Status: Ready to plan
Last activity: 2026-04-16 -- Milestone v1.7 initialized

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
- [Phase 33]: Keep command-router host ownership and NodeCanvas lifecycle/property routing behind dedicated internal collaborators, then prove those seams through focused tests and the maintenance lane. — The next hotspot follow-through should reduce change radius without changing the public embedding surface.
- [Phase 33]: Limit central `CS1591` suppression to non-packable projects and leave the remaining public XML-doc debt explicit in `AsterGraph.Editor`. — Package-boundary guardrails are only meaningful when the real debt boundary is visible in project configuration.

New v1.7 framing decisions:

- The next milestone should be a consumer-closure and release-hardening pass, not another feature-expansion band. — The main risk now is repo self-description, proof discoverability, release automation, and maintenance clarity rather than missing runtime/plugin capability.
- Truth alignment across README, planning artifacts, and codebase maps is first-order work, not polish. — A repo that ships more capability than its docs can describe cleanly is harder to adopt and maintain.
- The official proof ring must be treated as one executable system around real tool entry points and consumer host proof, not a loose set of remembered commands and implied tools.
- Minimal consumer onboarding, explicit history/save contracts, compatibility retirement, and extension-precedence rules are product requirements because they determine whether external hosts can adopt the SDK safely.

### Pending Todos

None captured yet.

### Blockers/Concerns

None at initialization. The first milestone work is Phase 34 truth alignment and proof-ring closure.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-16T06:20:00.0000000Z
Stopped at: Milestone v1.7 initialized
Resume file: .planning/ROADMAP.md
