# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a kernel-first editor runtime, explicit descriptor-based host contracts, and an Avalonia UI shell. The shipped baseline now covers a publishable four-package SDK boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation seams, diagnostics hooks, scaling hardening, migration proof, and plugin/automation readiness proof.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Current Milestone: v1.4 Plugin Loading and Automation Execution

**Goal:** Turn the shipped readiness descriptors and graph-first showcase proof into real plugin-loading and automation-execution surfaces that hosts can compose from the canonical session boundary.

**Target features:**
- Add a public plugin composition/loading path rooted in `AsterGraphEditorFactory` and the kernel-first session contracts instead of internal editor or Avalonia object access.
- Let loaded plugins contribute additive services, menus, presentation, diagnostics, or other host-facing seams through explicit contracts that remain inspectable from the canonical runtime boundary.
- Add a descriptor-first automation runner that drives canonical command IDs, query snapshots, batching, and diagnostics without depending on `GraphEditorViewModel` methods.
- Extend `HostSample`, `PackageSmoke`, `ScaleSmoke`, focused tests, and docs so plugin and automation claims remain machine-checkable.

## Current State

- Shipped packages remain `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- Canonical composition is now kernel-first through `AsterGraphEditorFactory.CreateSession(...)` and `AsterGraphEditorFactory.Create(...)`, without `GraphEditorViewModel` as the runtime state owner.
- Public host-facing capability, command, menu, and graph-state reads now prefer descriptor and snapshot contracts over MVVM object shape.
- Avalonia shell and canvas routing/platform seams now flow through shared adapters instead of duplicating policy independently.
- Migration posture and plugin/automation readiness are locked by focused regressions plus runnable `HostSample`, `PackageSmoke`, and `ScaleSmoke` proof markers.
- `AsterGraph.Demo` now exposes live view, behavior, and runtime host controls from the top menu and compact right-side pane over one retained editor/session path.
- `AsterGraph.Demo` now also exposes compact in-context proof cues, live configuration summaries, and aligned README narrative over the same graph-first host shell.
- The v1.3 showcase work is shipped and archived, and Phase 23 now proves that the same canonical session boundary can host real plugin loading plus canonical plugin inspection.
- Loaded plugins now compose node definitions, context-menu augmentation, localization, and node presentation through the shared factory/session path while host-owned overrides keep final authority.
- Hosts can now inspect structured plugin load snapshots, descriptors, contribution shape, and recoverable failures through canonical runtime queries and inspection snapshots.
- The known `STATE_HISTORY_OK` mismatch remains an unresolved pre-v1.2 baseline if the next milestone touches history/save semantics.

## Requirements

### Validated

- ✓ Host can consume the four publishable AsterGraph packages through a documented SDK boundary and supported `net8.0` / `net9.0` target story — v1.0
- ✓ Host can initialize the editor runtime and default Avalonia view through public factory/options APIs while the constructor-based path remains supported — v1.0
- ✓ Host can migrate through a staged compatibility path backed by parity tests and smoke coverage across legacy and factory entry routes — v1.0 to v1.2
- ✓ Host can drive the editor through public runtime-session contracts, typed events, batching, and replaceable services without depending on Avalonia control internals for shipped baseline flows — v1.0
- ✓ Host can embed the full shell, standalone canvas, standalone inspector, and standalone mini map against the same editor state, with explicit standalone canvas stock-behavior opt-outs — v1.0
- ✓ Host can replace stock visual presenters for nodes, menus, inspector, and mini map while reusing the existing editor-owned behavior and data projections — v1.0
- ✓ Host can inspect diagnostics and receive machine-readable recoverable failures through the shipped diagnostics/session surface — v1.0
- ✓ Host/runtime boundaries, native Avalonia behavior, hot-path scaling, and proof-ring validation are materially hardened through phases 07-12 — v1.1
- ✓ The canonical runtime/editor state owner can be composed without constructing `GraphEditorViewModel` — v1.2
- ✓ `IGraphEditorSession` and related runtime contracts now operate over kernel-owned state/contracts rather than a VM-owned facade — v1.2
- ✓ Public command, capability, menu, and state-query contracts now use explicit descriptors and snapshots as the canonical host surface — v1.2
- ✓ `AsterGraph.Avalonia` now consumes thinner kernel/facade contracts with shared command/menu/platform adapters — v1.2
- ✓ Existing `GraphEditorViewModel` / `GraphEditorView` hosts keep a staged migration path while the kernel-first route is canonical and explicitly proven — v1.2
- ✓ The shipped architecture is explicitly ready for later plugin loading and richer automation without another deep boundary rewrite — v1.2
- ✓ The demo opens into a graph-first shell where the node graph and a host-level menu are the first things users see — v1.3 Phase 19
- ✓ Users can adjust shell/view, editing behavior, and runtime-facing demo controls from compact host-level menu groups while staying on the same live graph — v1.3 Phase 20
- ✓ The demo now distinguishes host-owned seams from shared runtime state through compact proof cues, live configuration sections, and aligned demo-facing documentation — v1.3 Phase 21
- ✓ Host can now load one or more runtime plugins through a public composition path rooted in `AsterGraphEditorFactory` / `AsterGraphEditorOptions`, with canonical loader readiness and recoverable diagnostics — v1.4 Phase 22
- ✓ Loaded plugins now contribute node definitions, context-menu augmentation, localization, and node presentation through the canonical factory/session boundary while host-supplied providers keep final override authority — v1.4 Phase 23
- ✓ Host can now inspect loaded plugin descriptors, contribution shape, and recoverable failures through canonical runtime queries and inspection snapshots rather than diagnostics scraping alone — v1.4 Phase 23

### Active

- [ ] Host can execute richer automation or macro workflows against canonical command IDs, query snapshots, batching, and diagnostics without relying on `GraphEditorViewModel` methods.
- [ ] Plugin and automation delivery stays backed by focused tests plus `HostSample`, `PackageSmoke`, and `ScaleSmoke` proof rather than doc-only claims.

### Out of Scope

- New graph-editing end-user features unrelated to extensibility, host integration, or automation on top of the shipped SDK seams
- Deep demo-showcase follow-on work such as named presets or guided tours in this milestone; that value is deferred until the platform work lands
- Plugin marketplace/discovery UX, remote installation flows, signing, or isolation policy work beyond the first in-process loader baseline
- Dedicated scripting language or workflow-designer product surface beyond descriptor-first automation execution
- Replacing Avalonia with another UI stack before plugin/automation value is realized on the current stack

## Context

Milestone `v1.2` shipped on 2026-04-08 after phases 13-18 extracted the kernel, normalized descriptor contracts, thinned Avalonia adapters, and closed with migration/readiness proof. Milestone `v1.3` then shipped the graph-first demo showcase, so the host-level integration story is now legible on first read instead of being buried behind explanation-heavy panels. Phase 22 and Phase 23 of `v1.4` have now turned that readiness posture into a real plugin-loading baseline with live additive composition and canonical inspection.

The next product risk is no longer whether plugin seams can be discovered or applied. The next product risk is whether those explicit descriptors, command IDs, query snapshots, batching hooks, and replaceable services can now support automation execution and a broader proof ring without reintroducing facade-shaped or Avalonia-shaped dependencies.

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
| Use `HostSample`, `PackageSmoke`, and `ScaleSmoke` as the proof ring for migration and readiness claims | Architectural claims stay machine-checkable and host-visible | ✓ Good |
| Lead the next milestone with demo showcase UX rather than plugin/automation implementation | The shipped architecture is ready, but the demo still undersells the host integration story | ✓ Good |
| Keep the demo on one live graph session with host-level menu controls instead of scene switching | A single-session showcase better demonstrates seam replacement without hiding runtime continuity | ✓ Good |
| Use an in-window host menu as the first control plane in the demo shell | Makes the host integration story visible before explanatory content | ✓ Good |
| Move secondary showcase content behind a compact on-demand pane | Preserves graph-first reading while keeping live proof available | ✓ Good |
| Return to plugin/automation execution immediately after the showcase milestone | The integration story is now clear enough that the highest remaining product risk is real extension delivery, not more presentation polish | ✓ Good |
| Keep new plugin and automation surfaces rooted in `IGraphEditorSession`, descriptors, and command IDs | Extension work should build on the canonical runtime boundary rather than retained MVVM or Avalonia compatibility shims | ✓ Good |
| Use `AssemblyLoadContext` + `AssemblyDependencyResolver` while keeping shared `AsterGraph.*` contracts in the default context | Assembly-path plugins need intentional dependency isolation without breaking host/plugin type identity | ✓ Good |
| Keep plugin inspection rooted in canonical query and inspection DTOs rather than diagnostics scraping | Hosts need stable current-state reads, not only append-only event history | ✓ Good |
| Compose plugin contributions beneath explicit host-owned overrides on one shared factory/session path | Live plugin value should not fork retained/runtime behavior or regress host precedence | ✓ Good |

## Next Milestone Goals

- Deliver the first real public plugin-loading baseline over the shipped kernel-first host seams.
- Deliver the first descriptor-first automation execution baseline over canonical command IDs, query snapshots, and batch mutation paths.
- Preserve the proof-ring discipline so plugin and automation claims close with machine-checkable tests, samples, smoke runs, and milestone artifacts.

## Archived Milestone Framing

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
*Last updated: 2026-04-08 after completing Phase 23*
