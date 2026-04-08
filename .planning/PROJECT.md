# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a kernel-first editor runtime, explicit descriptor-based host contracts, and an Avalonia UI shell. The shipped baseline now covers a publishable four-package SDK boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation seams, diagnostics hooks, scaling hardening, migration proof, and plugin/automation readiness proof.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Current Milestone: v1.3 Demo Showcase

**Goal:** Rebuild `AsterGraph.Demo` into a graph-first, host-menu-first SDK showcase so users immediately see the live node graph and can adjust view, behavior, and runtime capabilities through compact host-level controls on the same session.

**Target features:**
- Replace the current explanation-heavy three-column demo shell with a graph-first layout led by a host-level menu.
- Consolidate view/chrome, editing behavior, and runtime-facing demo controls into compact grouped menu or drawer surfaces.
- Keep all showcase adjustments bound to the same live `Editor` / `Session` instead of switching scenes or rebuilding the runtime.
- Replace long explanatory cards with compact in-context proof that shows seam ownership and current live configuration.

## Current State

- Shipped packages remain `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- Canonical composition is now kernel-first through `AsterGraphEditorFactory.CreateSession(...)` and `AsterGraphEditorFactory.Create(...)`, without `GraphEditorViewModel` as the runtime state owner.
- Public host-facing capability, command, menu, and graph-state reads now prefer descriptor and snapshot contracts over MVVM object shape.
- Avalonia shell and canvas routing/platform seams now flow through shared adapters instead of duplicating policy independently.
- Migration posture and plugin/automation readiness are locked by focused regressions plus runnable `HostSample`, `PackageSmoke`, and `ScaleSmoke` proof markers.
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

### Active

- [ ] The demo opens into a graph-first shell where the node graph and a host-level menu are the first things users see.
- [ ] Users can adjust shell/view, editing behavior, and runtime-facing demo controls from compact host-level menu groups while staying on the same live graph.
- [ ] The demo explains replaceable seams and current runtime configuration with compact in-context proof instead of large static explanation panels.
- [ ] The redesigned showcase remains a proof of SDK capabilities rather than a fake product shell or a scene-switching sample gallery.

### Out of Scope

- New graph-editing end-user features unrelated to extensibility, host integration, or scaling
- Replacing Avalonia with another UI stack before plugin/automation value is realized on the current stack
- Algorithm execution engine work unrelated to SDK hardening
- Large-scale visual redesign of the shipped shell
- Runtime plugin loading and automation APIs in this milestone; this cycle focuses on presenting the already-shipped extensibility seams more clearly

## Context

Milestone `v1.2` shipped on 2026-04-08 after phases 13-18 extracted the kernel, normalized descriptor contracts, thinned Avalonia adapters, and closed with migration/readiness proof. The next product risk is no longer architectural center-of-gravity drift inside the current runtime. It is how to use the newly explicit seams for real plugin loading, automation APIs, and higher-level tooling without reintroducing facade-shaped dependencies.

The immediate adoption problem, however, is more basic inside the demo host: the current `AsterGraph.Demo` UI behaves like a capability console with large explanatory side panels. It proves many seams, but it does not make the graph or the host-level integration story legible at first glance. The next milestone therefore focuses on presentation of the shipped SDK story before it adds the next layer of runtime features.

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
| Lead the next milestone with demo showcase UX rather than plugin/automation implementation | The shipped architecture is ready, but the demo still undersells the host integration story | — Pending |
| Keep the demo on one live graph session with host-level menu controls instead of scene switching | A single-session showcase better demonstrates seam replacement without hiding runtime continuity | — Pending |

## Next Milestone Goals

- Turn `AsterGraph.Demo` into a graph-first showcase that makes host-controlled seams obvious on first read.
- Replace the current side-panel-heavy capability console with a compact host-level menu and on-demand control surfaces.
- Preserve the proof-ring discipline so future plugin/automation work extends a clearer and more persuasive demo baseline.

## Archived Milestone Framing

<details>
<summary>v1.2 planning snapshot</summary>

Kernel Extraction, Capability Contracts, and Plugin Readiness focused on extracting the real editor kernel, rebuilding the session/facade relationship around that kernel, normalizing host contracts around descriptors, thinning the Avalonia adapter boundary, and proving migration/readiness with focused regressions plus runnable samples and smoke tools.

</details>

## Evolution

This document evolves at milestone boundaries.

**After each milestone**:
1. Move newly shipped requirements into the validated section.
2. Re-check whether the current core value and product description still match what the codebase actually delivers.
3. Log the architectural decisions that proved durable during the milestone.
4. Reset active requirements so the next roadmap starts from the highest remaining product risk instead of stale execution context.

---
*Last updated: 2026-04-08 after starting milestone v1.3*
