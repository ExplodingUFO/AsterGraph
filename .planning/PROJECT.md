# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a kernel-first editor runtime, explicit descriptor-based host contracts, and an Avalonia UI shell. The shipped baseline now covers a publishable four-package SDK boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation seams, diagnostics hooks, scaling hardening, migration proof, and plugin/automation readiness proof.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

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

- [ ] Runtime plugin loading, discovery, isolation, and trust model on top of the shipped readiness descriptors
- [ ] Automation/macro APIs over canonical session descriptors, batching primitives, and runtime command IDs
- [ ] Diagnostics workbench or operator tooling on top of the public diagnostics/probe surface
- [ ] Resolve retained history/save semantic mismatches if the next milestone touches transaction or persistence behavior

### Out of Scope

- New graph-editing end-user features unrelated to extensibility, host integration, or scaling
- Replacing Avalonia with another UI stack before plugin/automation value is realized on the current stack
- Algorithm execution engine work unrelated to SDK hardening
- Large-scale visual redesign of the shipped shell

## Context

Milestone `v1.2` shipped on 2026-04-08 after phases 13-18 extracted the kernel, normalized descriptor contracts, thinned Avalonia adapters, and closed with migration/readiness proof. The next product risk is no longer architectural center-of-gravity drift inside the current runtime. It is how to use the newly explicit seams for real plugin loading, automation APIs, and higher-level tooling without reintroducing facade-shaped dependencies.

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

## Next Milestone Goals

- Turn readiness descriptors into actual plugin-loading and automation APIs.
- Decide whether the next milestone leads with plugin loading, automation, or diagnostics tooling.
- Preserve the proof-ring discipline so future work extends the shipped vocabulary instead of redefining it.

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
*Last updated: 2026-04-08 after v1.2 milestone*
