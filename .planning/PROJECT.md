# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a kernel-first editor runtime, explicit descriptor-based host contracts, and an Avalonia UI shell. The shipped baseline already covers a publishable four-package SDK boundary, plugin loading, automation execution, proof tools, and release validation. v1.7 focuses on consumer closure and release hardening so that the shipped surface is easier to publish, verify, adopt, and extend without reopening fundamental runtime-boundary work.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Current Milestone: v1.7 Consumer Closure / Release Hardening

**Goal:** Turn the shipped runtime, plugin, automation, and proof surfaces into a tighter consumer-facing product story with one truthful narrative, one executable proof ring, stronger release automation, and clearer long-term extension contracts.

**Target features:**
- Align README, planning artifacts, and codebase maps to one current story about shipped capabilities, proof tools, and support boundaries.
- Collapse the proof ring into a real executable system around `PackageSmoke`, `ScaleSmoke`, focused regressions, and a minimal consumer host path.
- Automate build, test, pack, smoke, and compatibility verification across the supported target-framework matrix.
- Close the consumer path and maintenance gaps around minimal host onboarding, history/save semantics, compatibility retirement, and extension precedence rules.

## Latest Shipped Milestone: v1.6 Facade Convergence and Proof Guardrails

**Status:** Shipped and archived on 2026-04-16

**Goal:** Reduce the remaining internal complexity around the retained facade path, close the carried history/save semantic concern, and tighten the maintenance guardrails needed for continued hotspot refactoring without changing the public SDK surface.

**Delivered in v1.6:**
- Archived the missing `v1.4` milestone history and aligned live planning/docs around one current proof story.
- Replaced the carried `STATE_HISTORY_OK` mismatch with explicit passing history/save regression and smoke coverage.
- Narrowed `GraphEditorViewModel` further toward a compatibility facade while keeping kernel-owned state canonical.
- Continued hotspot reduction in `GraphEditorKernel` and `NodeCanvas`, then scoped `CS1591` debt to the real remaining project boundary.

## Prior Shipped Milestone: v1.5 Runtime Boundary Cleanup and Quality Gates

**Shipped goal:** Reduce the remaining gap between the canonical runtime boundary and retained compatibility facades, then automate the validation and documentation surface that protects the SDK boundary.

**Delivered in v1.5:**
- Consolidated retained `GraphEditorViewModel` behavior around the kernel/session-owned runtime path and continued retiring MVVM-shaped runtime compatibility shims.
- Added tracked repo-level quality gates for style, package/version management, target-matrix validation, coverage collection, CI, and package/public API checks.
- Realigned documentation, solution membership, proof tools, and regression lanes around one trustworthy SDK verification surface.
- Shortened the canonical host-adoption path while preserving the current proof ring and staged migration posture.

## Current State

- Shipped packages remain `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- Canonical composition is kernel-first through `AsterGraphEditorFactory.CreateSession(...)` and `AsterGraphEditorFactory.Create(...)`, without `GraphEditorViewModel` as the canonical runtime state owner.
- Plugin loading and automation execution already ship on the canonical session boundary and are backed by focused regressions plus `PackageSmoke` and `ScaleSmoke`.
- The repo already carries `.editorconfig`, `Directory.Packages.props`, `tests/coverage.runsettings`, `.github/workflows/ci.yml`, and `eng/ci.ps1`; v1.6 should build on those guardrails rather than recreate them.
- `v1.4` now has checked-in archive files under `.planning/milestones/` plus a retrospective milestone-ledger entry, so milestone history no longer depends on stale phase directories alone.
- `eng/ci.ps1 -Lane maintenance` now exists as the hotspot-sensitive refactor gate over focused editor regressions plus `ScaleSmoke`.
- Phase 31 closed the carried history/save concern: retained undo/redo/dirty/save semantics now run on one kernel-owned authority, focused suites cover the interaction/save boundary directly, and `ScaleSmoke` emits `SCALE_HISTORY_CONTRACT_OK`.
- Phase 32 moved more retained bootstrap, menu, and fragment orchestration out of `GraphEditorViewModel` while keeping the public factory/session/view-model entry points stable.
- Phase 33 split the next kernel and canvas hotspots behind dedicated internal collaborators, and publishable-package XML-doc debt no longer hides behind a repo-wide `CS1591` blanket.
- Phase 34 aligned README, host docs, top-level planning artifacts, and codebase maps around one v1.7 proof story, then restored `AsterGraph.HostSample` as the minimal consumer-facing host sample.
- Phase 35 turned release validation into an explicit three-lane system: framework matrix, focused contract proof, and the full publish gate, while also making packed `HostSample` part of the real release proof path.
- v1.7 is now scoped as a productization-closeout milestone: truth alignment, proof-ring closure, release automation, minimal consumer onboarding, history/save contract publication, and extension-boundary hardening.

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

### Active

- [ ] Consumers can follow a minimal host onboarding path with canonical runtime-only, default Avalonia UI, trust/discovery, and automation entry routes.
- [ ] History/save/dirty semantics should be published as a product contract rather than remaining mostly implicit in tests and smoke markers.
- [ ] Maintainers can keep shrinking compatibility and hotspot seams under explicit history/save, extension precedence, and compatibility-retirement contracts.

### Out of Scope

- New plugin marketplace, trust/distribution policy, signing, or stronger isolation product work - v1.6 is a contraction milestone, not the next plugin feature band
- Dedicated scripting language, workflow-designer UI, or broader automation authoring product layers - the shipped command/query automation runner remains the baseline for now
- New graph-editing end-user features unrelated to facade convergence, state semantics, or maintenance guardrails - this milestone is about internal contraction rather than broadening the editor surface
- Replacing Avalonia or rewriting the retained compatibility story from scratch - the milestone should harden the current stack instead of reopening product positioning
- A one-shot removal of all compatibility APIs or other public breaking changes - staged migration remains part of the product promise
- Repeating v1.5 baseline work such as `.editorconfig`, central package management, CI setup, or `ScaleSmoke` solution alignment unless current repo evidence shows an actual regression

## Context

Milestone `v1.5` shipped on 2026-04-14 and left the repo in a materially better release posture: the canonical runtime boundary is clearer, the proof surface is aligned, and the release lane is scripted. The next real gap is no longer missing capability surface. It is the cost of carrying retained compatibility complexity and hotspot classes while trying to keep the SDK stable.

Current repo evidence on 2026-04-16 supports a different next step from v1.6. The runtime/session-first boundary is largely settled, plugin trust/discovery and automation are no longer just README claims, and the repo already ships `PackageSmoke`, `ScaleSmoke`, CI, package validation, and a maintenance gate. The bigger risk is that README, planning artifacts, proof-ring entry points, and consumer onboarding still drift apart more easily than the underlying code.

v1.7 therefore focuses on productization closure rather than new capability expansion. The milestone should make the repo tell one truthful story, make the proof ring executable and discoverable, push release verification deeper into machine gates, provide a real minimal consumer host path, publish the history/save contract explicitly, and document what is stable versus transitional in the extension surface.

## Constraints

- **Tech stack**: Keep the solution centered on .NET, C#, and Avalonia
- **Compatibility strategy**: Keep the migration window deliberate and additive rather than forcing a one-shot public break
- **Product positioning**: Preserve publishable package quality for the four supported SDK packages
- **Architecture**: Keep `GraphEditorKernel` as the canonical mutable runtime state owner while retained facades stay compatibility-only
- **Public API stability**: Do not change the public factory, session, or retained view-model entry points during this milestone
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

## Next Milestone Goals

- Refresh README, planning artifacts, and codebase maps until they all describe the same shipped capability and proof surface.
- Turn build/test/pack/smoke and compatibility checks into one official release gate with explicit target-matrix coverage.
- Close the consumer-facing adoption and maintenance gaps around minimal host onboarding, history/save contract publication, test-lane layering, and extension-boundary rules.

## Archived Milestone Framing

<details>
<summary>v1.5 planning snapshot</summary>

Runtime Boundary Cleanup and Quality Gates focused on tightening the canonical runtime boundary, automating release validation, and aligning docs/proof/test lanes around one trustworthy SDK verification surface.

</details>

<details>
<summary>v1.4 planning snapshot</summary>

Plugin Loading and Automation Execution focused on turning the shipped readiness descriptors and graph-first showcase proof into real runtime extension surfaces rooted in `AsterGraphEditorFactory`, `IGraphEditorSession`, descriptor-based inspection, and the same machine-checkable proof ring used for public claims.

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
*Last updated: 2026-04-16 after Phase 35 completion*
