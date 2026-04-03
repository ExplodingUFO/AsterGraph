# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a reusable editor state layer and an Avalonia UI shell. The foundation and first hardening milestones are complete: the project now ships a publishable four-package boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation seams, diagnostics hooks, host proof tools, and repeatable scale validation.

The next milestone shifts from hardening shipped surfaces to fixing the architectural center of gravity. The remaining risk is that the SDK still behaves like a `GraphEditorViewModel`-centered system with a session facade, rather than a true kernel-first runtime with adapters on top.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Current Milestone: v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness

**Goal:** Extract the real editor kernel out of the current façade-centered architecture, normalize capability/descriptor contracts, and leave the SDK ready for later plugin and automation work without another deep boundary rewrite.

**Target features:**
- Extract the canonical mutable editor state owner so runtime/session composition no longer depends on constructing `GraphEditorViewModel`.
- Rebuild the session/facade relationship so `GraphEditorViewModel` becomes a compatibility adapter rather than the architectural core.
- Replace public MVVM-shaped command/menu/state exposure with explicit capability, descriptor, and read-only query contracts.
- Thin the Avalonia layer so shell/canvas/input/clipboard behavior consumes shared kernel contracts instead of duplicating policy.
- Prove the migration path through focused regressions, HostSample, PackageSmoke, and the retained scale/proof tooling.

## Requirements

### Validated Foundation

- ✓ Host can consume the four publishable AsterGraph packages through a documented SDK boundary and supported `net8.0` / `net9.0` target story — v1.0
- ✓ Host can initialize the editor runtime and default Avalonia view through public factory/options APIs while the constructor-based path remains supported — v1.0
- ✓ Host can migrate through a staged compatibility path backed by parity tests and smoke coverage across legacy and factory entry routes — v1.0
- ✓ Host can drive the editor through public runtime-session contracts, typed events, batching, and replaceable services without depending on Avalonia control internals for the already-shipped baseline flows — v1.0
- ✓ Host can embed the full shell, standalone canvas, standalone inspector, and standalone mini map against the same editor state, with explicit standalone canvas stock-behavior opt-outs — v1.0
- ✓ Host can replace stock visual presenters for nodes, menus, inspector, and mini map while reusing the existing editor-owned behavior and data projections — v1.0
- ✓ Host can inspect diagnostics and receive machine-readable recoverable failures through the shipped diagnostics/session surface — v1.0
- ✓ Host/runtime boundaries, native Avalonia behavior, hot-path scaling, and proof-ring validation are materially hardened through phases 07-12 — v1.1

### Active Milestone Requirements

- [ ] The canonical runtime/editor state owner can be composed without constructing `GraphEditorViewModel`.
- [ ] `IGraphEditorSession` and related runtime contracts operate over kernel-owned state/contracts rather than a VM-owned façade.
- [ ] Public command, capability, menu, and state-query contracts avoid depending on MVVM implementation types and mutable public collections where stable descriptors or snapshots are sufficient.
- [ ] `AsterGraph.Avalonia` consumes thinner kernel/facade contracts and stops duplicating command-routing policy between shell and canvas controls.
- [ ] Existing `GraphEditorViewModel` / `GraphEditorView` hosts keep a staged migration path while the kernel-first path becomes canonical.
- [ ] The resulting architecture is explicitly ready for later plugin loading and richer automation without another boundary rewrite.

### Out of Scope

- New end-user graph-editing features unrelated to extensibility, host integration, or scaling — this milestone is hardening-oriented
- A non-Avalonia presentation stack — the goal is to make the current Avalonia offering more reusable, not replace it
- Runtime plugin loading — still deferred until the public runtime and extension seams are more stable
- Algorithm execution engine — still a separate direction from SDK hardening
- Deep aesthetic redesign of the shipped UI — native-feeling interaction and host cooperation matter here, not wholesale visual restyling

## Context

The original v1.0 milestone delivered the intended foundations: package boundary, runtime contracts, embeddable Avalonia surfaces, replaceable presentation, diagnostics, documentation, package smoke, and host sample validation. Since then, the repository has been cleaned up, package validation has been re-run, and the remaining XML warning debt has been retired.

A fresh cross-cutting review of the current mainline surfaced the next real bottlenecks:

- the runtime/session host story is still incomplete for serious custom UI hosts because important interaction primitives remain on `GraphEditorViewModel`
- several public host seams still bind directly to MVVM implementation types rather than stable runtime abstractions
- the full-shell Avalonia host path hard-captures desktop shortcuts and stock behavior in ways that make embedding less native
- the canvas hot paths still do whole-layer connection rebuilds, repeated linear node lookups, and full-graph marquee selection recomputation
- state, inspector, history, and dirty tracking still lean on whole-graph or whole-document work in places where scaling pressure will show up first

This milestone therefore treats “kernel extraction”, “explicit capability contracts”, and “adapter thinning” as the primary product risks to retire next.

## Constraints

- **Tech stack**: Keep the solution centered on .NET, C#, and Avalonia
- **Compatibility strategy**: Keep `GraphEditorViewModel` available as a compatibility facade while making the kernel-first path canonical
- **Product positioning**: Preserve publishable package quality for the four supported SDK packages
- **Architecture**: Harden the existing system incrementally; do not rewrite the editor/runtime stack from scratch
- **Extensibility**: Prefer stable DTOs/contracts and explicit capability descriptors over leaking mutable MVVM types through public seams
- **Host integration**: Keep Avalonia-specific adapters out of the kernel control plane where possible
- **Architecture**: Extract, then normalize; do not combine kernel extraction with unrelated feature growth

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Treat v1.1 as the shipped hardening baseline and start a new kernel-focused milestone | The next risk is no longer missing behavior, but the façade-centered architecture itself | ✓ Good |
| Extract the state owner before building plugin or automation features | Plugin and automation work on top of a VM-centered runtime would compound the wrong boundary | ✓ Good |
| Normalize capability/menu/command contracts after kernel extraction, not before | Descriptor cleanup is safer once state ownership and session boundaries are stable | ✓ Good |
| Keep Avalonia as an adapter layer, not the policy source | Input/clipboard/host-context duplication should not remain spread across controls | ✓ Good |
| Preserve the existing proof ring during architecture changes | Kernel extraction without migration proof would create silent regressions for existing hosts | ✓ Good |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition**:
1. Move validated requirements into the foundation/validated section with phase references.
2. Update active requirements if the newly learned constraints change milestone scope.
3. Log any new SDK-boundary or host-integration decisions.
4. Re-check whether the current milestone goal still reflects the highest remaining product risk.

**After each milestone**:
1. Summarize what was actually shipped in `MILESTONES.md`.
2. Audit which “active” requirements are now validated, deferred, or superseded.
3. Reset `STATE.md` so the next roadmap starts from a clean position rather than stale execution state.

---
*Last updated: 2026-04-04 for milestone v1.2 planning*
