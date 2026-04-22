# Extension and Maintenance Contracts

This document publishes the contract around surface stability, compatibility retirement, extension precedence, and lane ownership.

The public SDK contract is canonical-first: shipped stability is defined by `CreateSession`-based runtime/session APIs in `AsterGraph.Editor`; all other surfaces are compatibility or adapter-projection support layered on top.

## Stability Tiers

### Stable canonical surfaces

- `AsterGraphEditorFactory.CreateSession(...)`
- `IGraphEditorSession`
- DTO/snapshot queries such as `GetCompatiblePortTargets(...)`
- runtime-boundary diagnostics, automation, and plugin inspection

### Supported hosted-UI composition helper

- `AsterGraphEditorFactory.Create(...)`

### Retained migration surfaces

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` as a bridge into the canonical runtime contract

### Compatibility-only shims

- `IGraphEditorQueries.GetCompatibleTargets(...)`
- `CompatiblePortTarget`
- older MVVM-shaped overloads where newer runtime-first alternatives already exist

Use the retained surfaces only as migration bridges. New work should start on the stable canonical surfaces above, with [Host Integration](./host-integration.md) as the route map.

## Package-level contract inventory

- `AsterGraph.Abstractions`
  - Stable canonical contracts: session/query abstractions and DTO/snapshot-shape contracts for the canonical seam
  - Compatibility-only shims: legacy query/migration helpers where a runtime-first route exists
  - Hosted composition helper: none
- `AsterGraph.Core`
  - Stable canonical contracts: runtime/session support dependencies and implementation behavior needed to run the canonical contract
  - Compatibility-only shims: none published as stable public API in this phase
  - Hosted composition helper: none
- `AsterGraph.Editor`
  - Stable canonical surfaces: `AsterGraphEditorFactory.CreateSession(...)`, `IGraphEditorSession`, and canonical diagnostics/inspection contracts
  - Compatibility-only shims: `IGraphEditorQueries.GetCompatibleTargets(...)`, `CompatiblePortTarget` when hosted adapters still need migration bridges
  - Hosted composition helper: not a source of hosted UI composition itself; helper is adapter-owned
- `AsterGraph.Avalonia`
  - Stable hosted composition helper: `AsterGraphEditorFactory.Create(...)` (hosted UI composition entry point)
  - Stable canonical route access: uses canonical `CreateSession(...)`/`IGraphEditorSession` seam for all routed operations
  - Compatibility-only shims: avoid adding new host-layer contracts until parity is shipped in that adapter

## Extension Precedence

- plugin trust is host-owned and runs before activation
- plugin localization composes first; host localization runs last and wins final override
- plugin node presentation composes first; host presentation wins final override fields while merged adornments remain
- plugin commands register through the canonical session command descriptor pipeline; stock commands keep id authority when collisions exist
- runtime/session menus project stock descriptors today and will keep converging toward that shared command source
- retained `GraphEditorViewModel.BuildContextMenu(...)` remains the final host override point for compatibility hosts

## Lane Ownership

- `eng/ci.ps1 -Lane all` = framework-matrix build/test lane
- `eng/ci.ps1 -Lane contract` = focused consumer/state-contract gate
- `eng/ci.ps1 -Lane maintenance` = hotspot-refactor gate
- `eng/ci.ps1 -Lane release` = packed publish gate with smoke tools and coverage
- `tests/AsterGraph.Demo.Tests` = demo/sample-host lane

Classify failures by lane before changing code. That keeps refactor work, contract work, and release work from bleeding into each other without evidence.
