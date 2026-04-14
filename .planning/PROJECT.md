# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a kernel-first editor runtime, explicit descriptor-based host contracts, and an Avalonia UI shell. The shipped baseline now covers a publishable four-package SDK boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation seams, diagnostics hooks, plugin loading, automation execution, and proof-backed host integration. v1.5 hardened that SDK boundary so hosts can keep depending on the canonical runtime path without inheriting retained-facade debt or release-process drift.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Latest Shipped Milestone: v1.5 Runtime Boundary Cleanup and Quality Gates

**Shipped goal:** Reduce the remaining gap between the canonical runtime boundary and retained compatibility facades, then automate the validation and documentation surface that protects the SDK boundary.

**Delivered in v1.5:**
- Consolidate retained `GraphEditorViewModel` behavior around the kernel/session-owned runtime path and continue retiring MVVM-shaped runtime compatibility shims.
- Add tracked repo-level quality gates for style, package/version management, target-matrix validation, coverage, and package/public API checks.
- Realign documentation, solution membership, proof tools, and regression lanes around one trustworthy SDK verification surface.
- Preserve the current proof ring and migration path while making the canonical runtime/session surface easier to adopt and easier to verify.

## Current State

- Shipped packages remain `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- Canonical composition is now kernel-first through `AsterGraphEditorFactory.CreateSession(...)` and `AsterGraphEditorFactory.Create(...)`, without `GraphEditorViewModel` as the canonical runtime state owner.
- Public host-facing capability, command, menu, diagnostics, plugin, and automation reads now prefer descriptor and snapshot contracts over MVVM object shape.
- Plugin loading and automation execution shipped in v1.4 through the canonical session boundary and are already backed by focused regressions plus `PackageSmoke` and `ScaleSmoke`.
- Phase 26 completed the runtime-boundary cleanup for compatible-target discovery: canonical runtime queries now stay on DTO/snapshot contracts, while retained MVVM-shaped compatible-target APIs are explicitly isolated to the staged migration window.
- Phase 27 completed the repo-quality baseline: the repo now carries tracked `.editorconfig`, `Directory.Packages.props`, deterministic `NuGet.config`, a shared `eng/ci.ps1` validation script, and checked-in GitHub Actions CI for explicit `net8.0` / `net9.0` lanes.
- Phase 28 completed the proof-surface alignment: `AsterGraph.ScaleSmoke` is part of the tracked solution surface, stale `HostSample` claims are removed from current planning/codebase maps, public docs now describe the live `PackageSmoke` / `ScaleSmoke` / sample-only `AsterGraph.Demo` proof surface, and `AsterGraph.Demo.Tests` now carries the demo/sample regression lane separate from the core SDK regression lane.
- Phase 29 completed the remaining v1.5 gap: `eng/ci.ps1 -Lane release` now packs the publishable packages, runs `PackageSmoke`, runs `ScaleSmoke`, collects checked-in coverage/reporting, and enforces SDK package validation, while `docs/quick-start.md` now carries the synchronized three-way canonical adoption path for runtime-only, shipped-UI, and retained-migration hosts.

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

### Active

- None. v1.5 is archived; the next step is to archive v1.4 history cleanly or open a fresh post-v1.5 milestone.

### Out of Scope

- New graph-editing end-user features unrelated to boundary hardening, release validation, or host integration clarity - v1.5 is about SDK hardening, not broadening the editor feature surface
- Plugin marketplace/discovery UX, remote distribution, signing, or stronger isolation policy work - those remain follow-on platform investments after the current boundary and validation work lands
- Dedicated scripting language, workflow-designer UI, or richer automation authoring product layers - the shipped command/query automation runner remains the baseline for now
- Replacing Avalonia or rewriting the retained compatibility story from scratch - this milestone should harden the current stack rather than reopen product positioning
- A one-shot removal of all compatibility APIs - staged migration remains part of the product promise until stronger warnings, docs, and proof close the gap

## Context

Milestone `v1.4` finished execution on 2026-04-13 after phases 22-25 delivered plugin loading, runtime plugin inspection, descriptor-first automation execution, and the associated proof ring. The four-package boundary, canonical session-first factory path, and proof-backed host story are now real strengths rather than forward-looking plans.

The highest remaining product risk is now release trust and host-adoption clarity, not missing surface area. The codebase still carries some compatibility-only runtime debt, but Phase 28 closed the visible proof/doc drift by aligning solution membership, proof-tool references, and regression lanes around the live tree. The remaining weak point is that release-grade validation still depends too heavily on manual smoke execution and missing compatibility/coverage gates, while adoption guidance is still broader than it should be for first-time hosts.

v1.5 therefore focuses on hardening the SDK's actual contract boundary and the verification machinery around it. The milestone should build on the shipped v1.4 baseline, preserve the staged migration posture, and leave the repo in a state where future work on trust/distribution, richer automation authoring, or host ergonomics can proceed without first re-litigating the boundary.

## Constraints

- **Tech stack**: Keep the solution centered on .NET, C#, and Avalonia
- **Compatibility strategy**: Keep the migration window deliberate and additive rather than forcing a one-shot public break
- **Product positioning**: Preserve publishable package quality for the four supported SDK packages
- **Architecture**: Extend the shipped kernel-first baseline incrementally; do not rewrite the runtime or shell from scratch
- **Extensibility**: Prefer stable descriptors, snapshots, and explicit service seams over leaking mutable MVVM implementation types
- **Observability**: Diagnostics, proof outputs, and smoke markers remain part of the product surface, not local developer conveniences

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Keep the four-package SDK boundary as the supported publish surface | Hosts and packages already depend on that contract | ✓ Good |
| Extract the canonical state owner before plugin or automation work | Extensibility on a facade-owned runtime would compound the wrong boundary | ✓ Good |
| Normalize capability, command, and menu discovery around descriptors and snapshots | Stable host contracts are easier to version than MVVM object shape | ✓ Good |
| Treat Avalonia as an adapter layer over shared runtime routing and seam binders | Shell/canvas duplication should not remain the policy source | ✓ Good |
| Keep `GraphEditorViewModel` and `GraphEditorView` as retained compatibility facades with explicit proof | Staged migration remained possible while the canonical route moved to the kernel | ✓ Good |
| Use `PackageSmoke` and `ScaleSmoke` as proof-ring anchors for migration and readiness claims | Architectural claims stay machine-checkable and host-visible | ✓ Good |
| Lead v1.3 with demo showcase UX before plugin/automation implementation | The shipped architecture was ready, but the integration story was still undersold | ✓ Good |
| Keep plugin and automation surfaces rooted in `IGraphEditorSession`, descriptors, and command IDs | Extension work should build on the canonical runtime boundary rather than retained MVVM or Avalonia shims | ✓ Good |
| Keep the first automation runner synchronous, in-process, and descriptor-first | The first shipped automation value should validate the canonical runtime boundary before richer product layers are considered | ✓ Good |
| Keep plugin/automation proof aligned across focused tests, smoke tools, and README commands | Public claims should stay machine-checkable from the same canonical host boundary everywhere | ✓ Good |
| Focus v1.5 on runtime boundary cleanup, automated quality gates, and proof/doc alignment rather than another net-new feature band | The main remaining risk is SDK maintainability and release trust, not missing capability surface | ✓ Phase 26 started this cleanup by finishing runtime boundary canonicalization |
| Treat the four-package boundary, `CreateSession(...)`, and the current proof ring as fixed baseline during v1.5 | Current strengths should be hardened rather than reopened | ✓ Phase 27 kept the package boundary fixed while adding repo-level validation around it |
| Use staged deprecation guidance for compatibility APIs instead of a one-shot public break | Hosts still need a planned migration path while canonical DTO/snapshot contracts become authoritative | ✓ Phase 26 applied this guidance to compatible-target APIs |
| Keep one repo-local validation command path for both contributors and CI | Quality gates drift quickly if YAML and local commands diverge | ✓ Phase 27 shipped `eng/ci.ps1` plus workflow reuse |
| Align docs, solution membership, proof tools, and regression lanes around the live tree before adding stronger release gates | Release automation and host guidance are not trustworthy if they point at stale proof surfaces | ✓ Phase 28 aligned the tracked proof surface and split core-vs-demo regression lanes |

## Next Milestone Goals

- Archive v1.4 cleanly so the older milestone history matches the newer archive format.
- Start the next milestone from a fresh requirements pass once archive history is consistent.
- Decide whether the next investment should target plugin trust/distribution, richer automation authoring, or another host-facing gap surfaced by the hardened proof ring.

## Archived Milestone Framing

<details>
<summary>v1.4 planning snapshot</summary>

Plugin Loading and Automation Execution focused on turning the shipped readiness descriptors and graph-first showcase proof into real runtime extension surfaces rooted in `AsterGraphEditorFactory`, `IGraphEditorSession`, descriptor-based inspection, and the same machine-checkable proof ring used for public claims.

</details>

<details>
<summary>v1.3 planning snapshot</summary>

Demo Showcase focused on turning `AsterGraph.Demo` into a graph-first, host-menu-first SDK proof surface so hosts can see seam ownership, live configuration, and shared runtime state without reading through a capability-console layout.

</details>

## Evolution

This document evolves at milestone boundaries.

**After each milestone**:
1. Move newly shipped requirements into the validated section.
2. Re-check whether the current core value and product description still match what the codebase actually delivers.
3. Log the architectural decisions that proved durable during the milestone.
4. Reset active requirements so the next roadmap starts from the highest remaining product risk instead of stale execution context.

---
*Last updated: 2026-04-14 after the v1.5 milestone archive*
