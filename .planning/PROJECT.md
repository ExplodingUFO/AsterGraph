# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a kernel-first editor runtime, explicit descriptor-based host contracts, and an Avalonia UI shell. The shipped baseline already covers a publishable four-package SDK boundary, plugin loading, automation execution, proof tools, and release validation. v1.9 focuses on the last launch-gate blockers that still separate the shipped public-alpha surface from a repo that can be opened confidently: green GitHub-hosted CI, a valid tag-only prerelease workflow, and one small checked-in public-launch checklist.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Current Milestone: v1.9 Public Launch Gate and CI Stabilization

**Goal:** Close the remaining clean-runner CI, workflow-validity, and public-launch-checklist gaps so the existing `0.2.0-alpha.1` surface can move from "technically ready" to "operationally ready" for public opening.

**Target features:**
- Make `ci.yml` pass on clean GitHub-hosted Windows and Linux runners without relying on locally prebuilt Debug plugin artifacts.
- Fix the tag-driven prerelease workflow so it evaluates and schedules jobs correctly while staying secret-gated for real publishing.
- Publish one short launch checklist and tighten public docs around the real remaining blockers instead of replaying work already shipped in v1.8.

## Latest Shipped Milestone: v1.8 Public Alpha Readiness and Canonical Demo

**Status:** Shipped and archived on 2026-04-16

**Goal:** Turn the already-shipped SDK baseline into a repo and demo that are suitable for public alpha evaluation, adoption, and contribution without reopening the runtime-boundary work that v1.2-v1.7 already settled.

**Delivered in v1.8:**
- Realigned public package/version language around `0.2.0-alpha.1`, added alpha-status guidance, and completed the public governance files plus SDK pinning.
- Moved the main demo onto the canonical factory/session/view-factory route and exposed plugin trust, automation, standalone surfaces, and presenter replacement as visible product surfaces.
- Published paired English and `zh-CN` public guides, added a Chinese root README, and made the demo language toggle prove the host/runtime localization seam.
- Extended CI and release automation with concurrency, caching, Linux validation, proof artifacts, and a tag-driven prerelease workflow.

## Prior Shipped Milestone: v1.7 Consumer Closure / Release Hardening

**Status:** Shipped and archived on 2026-04-16

**Goal:** Turn the shipped runtime, plugin, automation, and proof surfaces into a tighter consumer-facing product story with one truthful narrative, one executable proof ring, stronger release automation, and clearer long-term extension contracts.

## Prior Shipped Milestone: v1.6 Facade Convergence and Proof Guardrails

**Status:** Shipped and archived on 2026-04-16

**Goal:** Reduce the remaining internal complexity around the retained facade path, close the carried history/save semantic concern, and tighten the maintenance guardrails needed for continued hotspot refactoring without changing the public SDK surface.

**Delivered in v1.6:**
- Archived the missing `v1.4` milestone history and aligned live planning/docs around one current proof story.
- Replaced the carried `STATE_HISTORY_OK` mismatch with explicit passing history/save regression and smoke coverage.
- Narrowed `GraphEditorViewModel` further toward a compatibility facade while keeping kernel-owned state canonical.
- Continued hotspot reduction in `GraphEditorKernel` and `NodeCanvas`, then scoped `CS1591` debt to the real remaining project boundary.

## Earlier Shipped Milestone: v1.5 Runtime Boundary Cleanup and Quality Gates

**Shipped goal:** Reduce the remaining gap between the canonical runtime boundary and retained compatibility facades, then automate the validation and documentation surface that protects the SDK boundary.

**Delivered in v1.5:**
- Consolidated retained `GraphEditorViewModel` behavior around the kernel/session-owned runtime path and continued retiring MVVM-shaped runtime compatibility shims.
- Added tracked repo-level quality gates for style, package/version management, target-matrix validation, coverage collection, CI, and package/public API checks.
- Realigned documentation, solution membership, proof tools, and regression lanes around one trustworthy SDK verification surface.
- Shortened the canonical host-adoption path while preserving the current proof ring and staged migration posture.

## Current State

- Shipped packages remain `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- Canonical composition is kernel-first through `AsterGraphEditorFactory.CreateSession(...)` and `AsterGraphEditorFactory.Create(...)`, without `GraphEditorViewModel` as the canonical runtime state owner.
- Plugin loading and automation execution already ship on the canonical session boundary and are backed by focused regressions plus `PackageSmoke`, `ScaleSmoke`, `HostSample`, and the contract/release lanes.
- Package metadata, README surfaces, and public docs now align on `0.2.0-alpha.1`.
- `AsterGraph.HostSample` proves the minimal canonical consumer path, while `AsterGraph.Demo` proves the fuller showcase host route through the same factory/session boundary.
- Public governance files, bilingual docs, a demo language toggle, and explicit alpha-status guidance now exist as part of the shipped repo surface.
- CI now exposes concurrency, NuGet cache, Linux validation, uploaded proof artifacts, and a tag-driven prerelease workflow while keeping the Windows release lane authoritative.
- The remaining blockers were exposed only after pushing v1.8 to GitHub-hosted runners: plugin proof tests still assume Debug test-plugin outputs, `actions/setup-dotnet` cache cleanup can fail when the default global package folder does not exist, and `release.yml` is currently failing before any jobs schedule.
- v1.9 is therefore a launch-gate stabilization pass, not another feature milestone.

## Requirements

### Validated

- ✓ Host can consume the four publishable AsterGraph packages through a documented SDK boundary and supported `net8.0` / `net9.0` target story - v1.0
- ✓ Host can initialize the editor runtime and default Avalonia view through public factory/options APIs while the constructor-based path remains supported - v1.0
- ✓ Host can migrate through a staged compatibility path backed by parity tests and smoke coverage across legacy and factory entry routes - v1.0 to v1.2
- ✓ Host can drive the editor through public runtime-session contracts, typed events, batching, and replaceable services without depending on Avalonia control internals for shipped baseline flows - v1.0
- ✓ Host can embed the full shell, standalone canvas, standalone inspector, and standalone mini map against the same editor state, with explicit standalone canvas stock-behavior opt-outs - v1.0
- ✓ Host can replace stock visual presenters for nodes, menus, inspector, and mini map while reusing the existing editor-owned behavior and data projections - v1.0
- ✓ Host can inspect diagnostics and receive machine-readable recoverable failures through the shipped diagnostics/session surface - v1.0
- ✓ Host/runtime boundaries, native Avalonia behavior, hot-path scaling, and proof-ring validation are materially hardened through phases 07-12 - v1.1
- ✓ The canonical runtime/editor state owner can be composed without constructing `GraphEditorViewModel` - v1.2
- ✓ `IGraphEditorSession` and related runtime contracts now operate over kernel-owned state/contracts rather than a VM-owned facade - v1.2
- ✓ Public command, capability, menu, and state-query contracts now use explicit descriptors and snapshots as the canonical host surface - v1.2
- ✓ `AsterGraph.Avalonia` now consumes thinner kernel/facade contracts with shared command/menu/platform adapters - v1.2
- ✓ Existing `GraphEditorViewModel` / `GraphEditorView` hosts keep a staged migration path while the kernel-first route is canonical and explicitly proven - v1.2
- ✓ The shipped architecture is explicitly ready for later plugin loading and richer automation without another deep boundary rewrite - v1.2
- ✓ The demo opens into a graph-first shell where the node graph and a host-level menu are the first things users see - v1.3 Phase 19
- ✓ Users can adjust shell/view, editing behavior, and runtime-facing demo controls from compact host-level menu groups while staying on the same live graph - v1.3 Phase 20
- ✓ The demo now distinguishes host-owned seams from shared runtime state through compact proof cues, live configuration sections, and aligned demo-facing documentation - v1.3 Phase 21
- ✓ Host can now load one or more runtime plugins through a public composition path rooted in `AsterGraphEditorFactory` / `AsterGraphEditorOptions`, with canonical loader readiness and recoverable diagnostics - v1.4 Phase 22
- ✓ Loaded plugins now contribute node definitions, context-menu augmentation, localization, and node presentation through the canonical factory/session boundary while host-supplied providers keep final override authority - v1.4 Phase 23
- ✓ Host can now inspect loaded plugin descriptors, contribution shape, and recoverable failures through canonical runtime queries and inspection snapshots rather than diagnostics scraping alone - v1.4 Phase 23
- ✓ Host can now execute multi-step automation runs against canonical command IDs, batching, and query snapshots through `IGraphEditorSession` instead of retained `GraphEditorViewModel` methods - v1.4 Phase 24
- ✓ Host can now observe automation started/progress/completed signals through typed runtime events and machine-readable diagnostics suitable for non-Avalonia consumers - v1.4 Phase 24
- ✓ `PackageSmoke`, `ScaleSmoke`, and focused regressions now prove plugin composition and automation execution from the canonical host boundary - v1.4 Phase 25
- ✓ README-backed proof commands now route hosts to the same canonical plugin/automation story used in milestone verification - v1.4 Phase 25
- ✓ Canonical compatible-target discovery now stays on DTO/snapshot runtime queries, while internal host/kernel/runtime seams no longer expose the legacy MVVM-shaped shim - v1.5 Phase 26
- ✓ Retained `GraphEditorViewModel` / `GraphEditorView` hosts still run over the adapter-backed kernel/session runtime path, and the remaining compatibility-only APIs now carry explicit staged retirement guidance - v1.5 Phase 26
- ✓ Contributors now build and test under one tracked repo baseline with shared editor rules, centralized package versions, deterministic restore sources, and a reusable `eng/ci.ps1` entry point - v1.5 Phase 27
- ✓ The supported package boundary is now validated automatically across explicit `net8.0` and `net9.0` lanes through checked-in CI and the same repo-local command path used outside CI - v1.5 Phase 27
- ✓ README, planning docs, solution membership, proof-tool references, and regression-lane guidance now point at the same live Phase 28 verification surface with no stale `HostSample` claims - v1.5 Phase 28
- ✓ Core SDK regression coverage is now split cleanly from demo/sample integration coverage through `AsterGraph.Editor.Tests` / `AsterGraph.Serialization.Tests` versus `AsterGraph.Demo.Tests`, so failures identify the right layer - v1.5 Phase 28
- ✓ Release validation now packs the publishable packages, runs `PackageSmoke`, runs `ScaleSmoke`, collects coverage, and enforces package validation from one repo-local entry point - v1.5 Phase 29
- ✓ Hosts can now follow one short canonical integration path for runtime-only, shipped-UI, and retained-migration scenarios - v1.5 Phase 29
- ✓ Maintainer can now read archived `v1.4` milestone history through checked-in archive files and the milestone ledger without contradictory active-vs-archived planning state claims - v1.6 Phase 30
- ✓ Contributors can now run `eng/ci.ps1 -Lane maintenance` as the hotspot-sensitive refactor gate while the history/save semantic contract remains active work for Phase 31 - v1.6 Phase 30
- ✓ Host now sees one explicit retained undo/redo/dirty/save contract across mixed runtime and retained flows, including save-boundary behavior after undo/redo - v1.6 Phase 31
- ✓ Contributors can now localize retained history interaction, save-boundary, and drag-boundary regressions through focused suites instead of one broad transaction file - v1.6 Phase 31
- ✓ `ScaleSmoke` and proof-ring coverage now expose the same explicit history/save contract without the carried `STATE_HISTORY_OK` mismatch - v1.6 Phase 31
- ✓ Host keeps the same public factory/session/view-model entry points while hotspot refactors continue to move retained-facade orchestration behind kernel-owned collaborators - v1.6 Phase 32
- ✓ Contributors can keep reducing `GraphEditorViewModel`, `GraphEditorKernel`, and `NodeCanvas` hotspots under the maintenance gate without widening public XML-doc debt again - v1.6 Phase 33
- ✓ Maintainers can now read README, ROADMAP, STATE, PROJECT, and current codebase maps without conflicting v1.7 proof-ring or milestone-state claims - v1.7 Phase 34
- ✓ Consumer-facing docs no longer contradict themselves on undo/redo capability, target support, or proof-tool availability - v1.7 Phase 34
- ✓ The official proof ring is now explicit around `eng/ci.ps1`, `HostSample`, `PackageSmoke`, `ScaleSmoke`, focused regressions, and the visual demo role split - v1.7 Phase 34
- ✓ Maintainers can now run one official release entry point that executes focused contract proof, pack, packed consumer proof, smoke proof, coverage, and package validation - v1.7 Phase 35
- ✓ CI now exposes explicit framework-matrix, contract-proof, and release-validation jobs across the supported `net8.0` / `net9.0` story - v1.7 Phase 35
- ✓ Packed consumer restore now stays compatible with central package management because the four publishable AsterGraph packages all have tracked central package versions - v1.7 Phase 35
- ✓ Consumers now have one compact route matrix for runtime-only, shipped UI, plugin trust/discovery, automation, and retained migration, with package and verification guidance attached - v1.7 Phase 36
- ✓ History/save/dirty behavior is now published as an explicit product contract and linked back to the proof lane plus `SCALE_HISTORY_CONTRACT_OK` - v1.7 Phase 36
- ✓ Stability tiers, compatibility-retirement guidance, extension-precedence rules, and lane ownership are now published as explicit contracts instead of remaining implicit in code/tests - v1.7 Phase 37

### Active

- [ ] Unify public alpha versioning, repo narrative, and outward-facing alpha-status guidance around one explicit prerelease contract.
- [ ] Move the main demo onto the canonical host path and turn plugin, automation, standalone-surface, and presenter-replacement seams into visible showcase surfaces.
- [ ] Publish paired English and `zh-CN` public guides, and make localization switching part of the demo proof story.
- [ ] Add the missing public governance files and extend CI/release automation for a tag-driven public alpha path.

### Out of Scope

- New graph-editing end-user features unrelated to public alpha readiness, demo completeness, or public release workflows
- Marketplace UX, remote plugin installation UI, stronger plugin signing/isolation, or other post-alpha distribution product work
- Rich automation authoring UI, workflow-designer surfaces, or embedded scripting-editor work beyond the shipped execution baseline
- Replacing Avalonia, reopening kernel/session ownership, or redesigning the canonical runtime boundary
- A one-shot removal of retained compatibility APIs or other abrupt public breaks before the alpha surface is externally exercised

## Context

v1.7 closed the consumer-closure milestone: the repo now has one clearer proof story, an explicit contract lane, packed `HostSample` release proof, a public state contract, and explicit extension-stability guidance. The next real gap is no longer missing runtime capability. It is whether this repo, demo, and release flow are actually ready for outside users instead of only for informed maintainers.

v1.8 closed that gap. The repo now has an external-facing alpha version story, explicit governance files, canonical demo composition, bilingual public guides, a real demo localization proof, and public release automation with proof artifacts. The code, docs, demo, and CI now describe the same outward-facing contract instead of leaving maintainers to infer it from implementation detail.

## Constraints

- **Tech stack**: Keep the solution centered on .NET, C#, and Avalonia
- **Compatibility strategy**: Keep the migration window deliberate and additive rather than forcing a one-shot public break
- **Product positioning**: Preserve publishable package quality for the four supported SDK packages
- **Architecture**: Keep `GraphEditorKernel` as the canonical mutable runtime state owner while retained facades stay compatibility-only
- **Public API stability**: Do not change the public factory, session, or retained view-model entry points during this milestone
- **Public alpha scope**: Prefer closing adoption, docs, demo, and release gaps over expanding runtime capability surface
- **Bilingual delivery**: Treat English plus `zh-CN` public guides and demo language switching as first-class product requirements rather than incidental mixed-language content
- **Observability**: Diagnostics, proof outputs, smoke markers, and regression lanes remain part of the product surface, not local developer conveniences

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Keep the four-package SDK boundary as the supported publish surface | Hosts and packages already depend on that contract | ✓ Good |
| Extract the canonical state owner before plugin or automation work | Extensibility on a facade-owned runtime would compound the wrong boundary | ✓ Good |
| Normalize capability, command, and menu discovery around descriptors and snapshots | Stable host contracts are easier to version than MVVM object shape | ✓ Good |
| Treat Avalonia as an adapter layer over shared runtime routing and seam binders | Shell/canvas duplication should not remain the policy source | ✓ Good |
| Keep `GraphEditorViewModel` and `GraphEditorView` as retained compatibility facades with explicit proof | Staged migration remained possible while the canonical route moved to the kernel | ✓ Good |
| Use `PackageSmoke` and `ScaleSmoke` as proof-ring anchors for migration and readiness claims | Architectural claims stay machine-checkable and host-visible | ✓ Good |
| Keep plugin and automation surfaces rooted in `IGraphEditorSession`, descriptors, and command IDs | Extension work should build on the canonical runtime boundary rather than retained MVVM or Avalonia shims | ✓ Good |
| Keep one repo-local validation command path for both contributors and CI | Quality gates drift quickly if YAML and local commands diverge | ✓ Good |
| Use v1.6 as a contraction milestone instead of another plugin/automation feature band | The remaining risk is internal complexity and semantic drift, not missing capability surface | ✓ Good |
| Build on the shipped v1.5 guardrails instead of recreating them | `.editorconfig`, central package versions, CI, coverage collection, and `ScaleSmoke` alignment already exist in the live repo | ✓ Good |
| Treat v1.4 archive closure as current milestone work | Planning history drift is now a real maintenance cost rather than harmless backlog noise | ✓ Good |
| Add one explicit maintenance lane to `eng/ci.ps1` instead of a second script path | Refactor-proof validation should stay on the same repo-local command story contributors and docs already use | ✓ Good |
| Keep phase numbering continuous from 30 | The latest executed phase is 29, and reset numbering is unnecessary for this milestone | ✓ Good |
| Use v1.8 as a public-alpha readiness milestone instead of another runtime/plugin feature band | The underlying SDK baseline is already shipped; the gap is external readiness and discoverability | ✓ Good |
| Move public package semantics from `preview` to an explicit alpha contract | External consumers need a version story that reads as prerelease product posture instead of internal iteration cadence | ✓ Good |
| Make the main demo use the canonical factory/session/view-factory route | The showcase host should demonstrate the recommended adoption path, while retained construction remains migration proof | ✓ Good |
| Keep `HostSample` and `AsterGraph.Demo` as separate artifacts with separate jobs | The minimal consumer path and the capability showcase serve different audiences and should not collapse into one sample | ✓ Good |
| Use bilingual docs plus a demo language toggle as the public localization strategy | The repo already mixes Chinese and English; alpha readiness requires making that policy intentional and testable | ✓ Good |
| Keep public publishing tag-driven and verification-first | Pull requests should prove the surface; only tagged milestones should publish prerelease packages and release artifacts | ✓ Good |
| Extend CI with Linux validation and public artifacts without replacing the existing Windows release lane | Public alpha consumers expect broader signal, but the current Windows release path remains the most complete packaging proof | ✓ Good |

## Next Milestone Goals

- Make the current CI lanes pass on GitHub-hosted runners by removing hidden local-environment assumptions from plugin proof tests and helper code.
- Fix the current release-workflow validity problem and keep prerelease publication tag-scoped and secret-gated.
- Publish one minimal public-launch checklist that covers repo visibility, required checks, first prerelease tag, and artifact inspection.
- Keep the current public docs truthful by pointing them at the real remaining launch blockers instead of re-describing work already shipped in v1.8.

## Archived Milestone Framing

<details>
<summary>v1.7 planning snapshot</summary>

Consumer Closure / Release Hardening focused on aligning the shipped proof story, making release validation executable and consumer-facing, publishing explicit state and extension contracts, and restoring `HostSample` as the minimal canonical host sample.

</details>

<details>
<summary>v1.6 planning snapshot</summary>

Facade Convergence and Proof Guardrails focused on closing the carried history/save concern, shrinking retained-facade hotspots, and tightening the maintenance guardrails needed for continued internal contraction without public API churn.

</details>

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition**:
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone**:
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-04-16 for milestone v1.9 initialization*
