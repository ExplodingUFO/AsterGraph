---
gsd_state_version: 1.0
milestone: v1.8
milestone_name: Public Alpha Readiness and Canonical Demo
status: active
stopped_at: Phase 39 in progress
last_updated: "2026-04-16T14:55:00.0000000Z"
last_activity: 2026-04-16 -- Completed Phase 38 alpha framing and OSS baseline
progress:
  total_phases: 4
  completed_phases: 1
  total_plans: 12
  completed_plans: 3
  percent: 25
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** v1.8 public alpha readiness, canonical demo, bilingual docs, and public release closure

## Current Position

Phase: 39
Plan: 39 execution
Status: In progress
Last activity: 2026-04-16 -- Completed Phase 38 alpha framing and OSS baseline

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
- [Phase 34]: Treat `eng/ci.ps1 -Lane release` and `eng/ci.ps1 -Lane maintenance` as the official proof-ring entry points, then describe `HostSample`, `PackageSmoke`, `ScaleSmoke`, and `Demo` by role under that gate. — The repo already had the scripted gate; the gap was inconsistent narrative and discoverability.
- [Phase 34]: Restore `AsterGraph.HostSample` as a narrow canonical consumer sample instead of reviving the older proof-heavy host app. — The minimal host path should be easy to read and run, while the broader proof burden stays in smoke tools and focused regressions.
- [Phase 34]: Keep `src/AsterGraph.Demo` as the visual/manual sample and move the minimal consumer burden to `HostSample`. — Visual inspection and minimal adoption proof serve different audiences and should not share the same artifact.
- [Phase 35]: Add a focused `contract` lane instead of widening `maintenance`, then make `release` run that focused proof before packed-package smoke. — The repo needed a consumer/proof gate without diluting the hotspot-refactor loop.
- [Phase 35]: Treat packed `HostSample` as part of the release proof ring, not as a README-only manual command. — The minimal consumer path must be machine-checked from the same packed artifacts shipped to consumers.
- [Phase 35]: Keep CI responsibilities explicit as framework-matrix, contract-proof, and release-validation jobs. — The target matrix, focused contracts, and publish gate each fail for different reasons and should be visible separately.
- [Phase 36]: Publish one compact consumer route matrix instead of further expanding narrative onboarding text. — Consumers need packages, entry points, and verification commands in one place.
- [Phase 36]: Publish history/save/dirty semantics as a host-facing contract and point it at the `contract` lane plus `SCALE_HISTORY_CONTRACT_OK`. — The behavior was already proven; the missing part was a written contract.
- [Phase 37]: Publish explicit stability tiers, extension-precedence rules, and lane ownership from the real runtime behavior instead of reopening boundary-changing refactors. — The repo already had the right implementation and proof; the gap was contract clarity.

New v1.7 framing decisions:

- The next milestone should be a consumer-closure and release-hardening pass, not another feature-expansion band. — The main risk now is repo self-description, proof discoverability, release automation, and maintenance clarity rather than missing runtime/plugin capability.
- Truth alignment across README, planning artifacts, and codebase maps is first-order work, not polish. — A repo that ships more capability than its docs can describe cleanly is harder to adopt and maintain.
- The official proof ring must be treated as one executable system around real tool entry points and consumer host proof, not a loose set of remembered commands and implied tools.
- Minimal consumer onboarding, explicit history/save contracts, compatibility retirement, and extension-precedence rules are product requirements because they determine whether external hosts can adopt the SDK safely.

New v1.8 framing decisions:

- The next milestone should make the repo suitable for public alpha use instead of widening the core runtime surface again. — The shipped SDK baseline is already credible; the gap is external readiness.
- Public package semantics should move from `preview` cadence to an explicit alpha contract. — External consumers need a version story that reads as prerelease product posture rather than internal iteration.
- `AsterGraph.Demo` should adopt the canonical factory/session/view-factory path as its main route, while retained construction remains proof and migration context. — The showcase host must teach the recommended embedding path.
- `HostSample` and `AsterGraph.Demo` should stay separate and explicitly labeled. — Minimal consumer onboarding and feature showcase have different jobs.
- The bilingual strategy should be intentional: English plus `zh-CN` guides, plus a real demo language toggle. — The repo already mixes languages; alpha readiness requires making that policy deliberate and testable.
- Public-alpha publishing should be tag-driven, while pull requests remain verification-only. — Publishing on every validated branch would blur release intent and artifact provenance.
- CI should expand with Linux validation, artifact uploads, caching, and concurrency without replacing the existing Windows release lane. — Public alpha consumers need broader signal, but the current Windows proof remains the deepest packaging path.

### Pending Todos

None captured yet.

### Blockers/Concerns

Current framing concerns to close through v1.8:

- `AsterGraph.Demo` still uses direct `GraphEditorViewModel` construction instead of the canonical factory/view-factory route.
- CI is still Windows-only and does not yet upload public release artifacts or run tag-driven prerelease publishing.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-16T06:20:00.0000000Z
Stopped at: Phase 39 in progress
Resume file: .planning/ROADMAP.md
