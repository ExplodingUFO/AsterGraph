# AsterGraph

## What This Is

AsterGraph is a modular node-graph editor toolkit for .NET with a reusable editor state layer and an Avalonia UI shell. The foundation milestone is now complete: the project already ships a publishable four-package boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation seams, diagnostics hooks, and sample hosts.

The next milestone shifts from capability creation to hardening. The main remaining product risk is no longer “can hosts do this at all?”, but whether the public runtime boundary is self-sufficient, whether the Avalonia layer behaves like a cooperative embedded desktop control, and whether the graph/editor hot paths still hold up as graph size grows.

## Core Value

Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Current Milestone: v1.1 Host Boundary, Native Integration, and Scaling

**Goal:** Turn the completed SDK foundation into a more production-grade host component line by hardening runtime boundaries, reducing MVVM leakage in public seams, improving native desktop host behavior, and addressing the most obvious large-graph performance risks.

**Target features:**
- Make the runtime/session host story self-sufficient for serious custom-UI hosts instead of forcing fallback to `GraphEditorViewModel`.
- Decouple the main extension seams from concrete MVVM types so hosts can extend menus and presentation against more stable contracts.
- Make the Avalonia full shell and standalone canvas cooperate better with host command routing, focus, context menu, and scrolling conventions.
- Reduce graph-wide rebuild and recomputation on drag, marquee selection, inspector refresh, and history/dirty-tracking paths.
- Prove the hardening work through focused regression tests, HostSample output, PackageSmoke output, and repeatable large-graph validation.

## Requirements

### Validated Foundation

- ✓ Host can consume the four publishable AsterGraph packages through a documented SDK boundary and supported `net8.0` / `net9.0` target story — v1.0
- ✓ Host can initialize the editor runtime and default Avalonia view through public factory/options APIs while the constructor-based path remains supported — v1.0
- ✓ Host can migrate through a staged compatibility path backed by parity tests and smoke coverage across legacy and factory entry routes — v1.0
- ✓ Host can drive the editor through public runtime-session contracts, typed events, batching, and replaceable services without depending on Avalonia control internals for the already-shipped baseline flows — v1.0
- ✓ Host can embed the full shell, standalone canvas, standalone inspector, and standalone mini map against the same editor state, with explicit standalone canvas stock-behavior opt-outs — v1.0
- ✓ Host can replace stock visual presenters for nodes, menus, inspector, and mini map while reusing the existing editor-owned behavior and data projections — v1.0
- ✓ Host can inspect diagnostics and receive machine-readable recoverable failures through the shipped diagnostics/session surface — v1.0

### Active Milestone Requirements

- [ ] Hosts with custom UI stacks can perform graph selection, node placement/movement, connection authoring, and viewport navigation through stable runtime contracts without depending on `GraphEditorViewModel`.
- [ ] Host-facing compatibility and extension contracts avoid leaking `NodeViewModel`, `PortViewModel`, or the concrete editor facade where a stable runtime abstraction or DTO is sufficient.
- [ ] Avalonia hosts can opt out of stock shell-level shortcuts and stock menu behavior in the full-shell entry path, not just the standalone canvas path.
- [ ] Embedded Avalonia surfaces cooperate with host-native wheel, pan, focus, keyboard menu, and command routing conventions instead of always taking input ownership.
- [ ] Dragging, connection preview, marquee selection, and canvas updates avoid graph-wide rebuild/requery patterns that scale directly with total node/edge count.
- [ ] Inspector, status, dirty tracking, and history paths avoid repeated whole-graph or whole-document recomputation on routine interactions.
- [ ] Host hardening and performance changes are covered by focused regressions, HostSample, PackageSmoke, and repeatable large-graph proof scenarios.

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

This milestone therefore treats “secondary development quality”, “native host cooperation”, and “large-graph responsiveness” as the primary product risks to retire next.

## Constraints

- **Tech stack**: Keep the solution centered on .NET, C#, and Avalonia
- **Compatibility strategy**: Keep `GraphEditorViewModel` available as a compatibility facade while making new host/runtime contracts thinner and more stable
- **Product positioning**: Preserve publishable package quality for the four supported SDK packages
- **Architecture**: Harden the existing system incrementally; do not rewrite the editor/runtime stack from scratch
- **Extensibility**: Prefer stable DTOs/contracts over leaking mutable MVVM types through new or revised public seams
- **Host integration**: Treat command routing, focus, wheel behavior, and context menu behavior as first-class embedding concerns
- **Performance**: Prioritize the graph interaction hot paths and state recomputation hot spots before broader speculative optimization

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Treat v1.0 as the shipped baseline and start a new hardening milestone instead of continuing the old roadmap in place | The original roadmap is effectively complete and the next work is qualitatively different | ✓ Good |
| Prioritize runtime host-boundary completion before more UI decomposition | A custom host story that still depends on `GraphEditorViewModel` is not a stable SDK story | ✓ Good |
| Reduce MVVM leakage in public seams even if compatibility facades remain | Host extension APIs should stabilize around contracts and DTOs, not internal state holders | ✓ Good |
| Treat native desktop host cooperation as a product requirement, not polish | Full-shell shortcut capture and forceful wheel/menu behavior directly hurt embedding quality | ✓ Good |
| Prioritize hot-path scaling fixes over broad micro-optimization | Drag, connection preview, marquee selection, and inspector recomputation are the most user-visible bottlenecks | ✓ Good |
| Require proof-ring validation for hardening work | HostSample, PackageSmoke, regressions, and large-graph validation should prove the changes, not just local reasoning | ✓ Good |

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
*Last updated: 2026-04-03 for milestone v1.1 planning*
