# Extension and Maintenance Contracts

This document publishes the contract around surface stability, compatibility retirement, extension precedence, and lane ownership.

The public SDK contract is canonical-first: shipped stability is defined by `CreateSession`-based runtime/session APIs in `AsterGraph.Editor`. Hosted helpers compose that route for stock UI, retained surfaces exist for migration, retired compatibility-only shims are tracked by the public inventory's v1 removal policy, and internal-only implementation details are not public contracts.

## Stability Tiers

### Stable canonical surfaces

- `AsterGraphEditorFactory.CreateSession(...)`
- `IGraphEditorSession`
- DTO/snapshot queries such as `GetCompatiblePortTargets(...)`
- node surface rotation through `IGraphEditorCommands.TrySetNodeRotation(...)` and `GraphEditorNodeSurfaceSnapshot.RotationDegrees`
- runtime-boundary diagnostics, automation, and plugin inspection

### Supported hosted helper

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphHostBuilder`
- standalone Avalonia factories such as `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, and `AsterGraphMiniMapViewFactory`

### Retained migration surfaces

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` as a bridge into the canonical runtime contract

### Compatibility-only surfaces

None are published in the primary v1 surface. Retired compatibility-only symbols and replacements are listed in [Public API Inventory](./public-api-inventory.md#v1-compatibility-removal-policy).

### Internal-only surfaces

- `Runtime.Internal`, `Kernel.Internal`, adapter control internals, projection/apply internals, tests, samples, and proof tools

Use the retained surfaces only as migration bridges. New work should start on the stable canonical surfaces above, with [Host Integration](./host-integration.md) as the route map and [Public API Inventory](./public-api-inventory.md) as the package-by-package support-tier map.

## Compatibility and Deprecation Policy

- Deprecation/retirement uses the same five support tiers as the public inventory: stable canonical, supported hosted helper, retained migration, compatibility-only, and internal-only.
- Compatibility-only APIs are not allowed in the primary v1 surface unless the public inventory names the symbol, replacement, and retirement boundary.
- Any temporary compatibility-only API must carry replacement guidance that points to a canonical replacement symbol path (for example `CreateSession(...)` + session/query members).
- API removal is never abrupt: it requires a documented deprecation cycle, with symbol-level migration guidance and a stated replacement path before any removal is planned.
- Migration guidance must be published per symbol, and includes old→new mapping (for example:
  - retired compatible-target shim → `AsterGraphEditorFactory.CreateSession(...).GetCompatiblePortTargets(...)`
  - retired MVVM target shape → `GraphEditorCompatiblePortTargetSnapshot` returned by `AsterGraphEditorFactory.CreateSession(...).GetCompatiblePortTargets(...)`
  - `GraphEditorViewModel` / `GraphEditorView` → `AsterGraphEditorFactory.CreateSession(...)` for runtime-only hosts, or `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` for the hosted Avalonia route)
- Canonical-first implementation remains anchored on `CreateSession(...)` and `IGraphEditorSession`; `AsterGraphHostBuilder` and Avalonia factories are supported hosted helpers, while `GraphEditorViewModel` and `GraphEditorView` are retained only to keep migration moving without forcing immediate breakage.

## Package-level contract inventory

The maintainer-facing package inventory is published in [Public API Inventory](./public-api-inventory.md). Keep this summary aligned with that inventory before widening public API claims.

- `AsterGraph.Abstractions`
  - Stable canonical: node definitions, port definitions, provider/plugin-facing contracts, identifiers, and metadata DTOs used by the canonical route
  - Supported hosted helper: none
  - Retained migration: none
  - Compatibility-only: none currently published as a primary support tier
  - Internal-only: implementation helpers not exposed through package docs
- `AsterGraph.Core`
  - Stable canonical: graph document, serialization-oriented model contracts, persisted node surface state, compatibility rule inputs, and shared data types used by editor/session composition
  - Supported hosted helper: none
  - Retained migration: none
  - Compatibility-only: none in the primary v1 surface; explicit legacy import is retained migration
  - Internal-only: core internals and persistence implementation details
- `AsterGraph.Editor`
  - Stable canonical: `AsterGraphEditorFactory.CreateSession(...)`, `IGraphEditorSession`, command/query DTOs including node rotation, diagnostics, automation, plugin discovery/inspection, and export services
  - Supported hosted helper: `AsterGraphEditorFactory.Create(...)` as hosted composition that still exposes the retained facade
  - Retained migration: `GraphEditorViewModel`, `GraphEditorViewModel.Session`, and retained menu/context-menu hooks used by migrating hosts
  - Compatibility-only: none in the primary v1 surface; retired symbols are tracked by the public inventory
  - Internal-only: `Runtime.Internal`, `Kernel.Internal`, projection/apply internals, and proof-only helpers
- `AsterGraph.Avalonia`
  - Stable canonical: adapter projection over the canonical editor/session route
  - Supported hosted helper: `AsterGraphAvaloniaViewFactory.Create(...)`, standalone surface factories, and `AsterGraphHostBuilder`
  - Retained migration: `GraphEditorView` embedding for hosts that still use the retained editor facade
  - Compatibility-only: adapter-specific glue only when bridging existing hosts to the canonical route
  - Internal-only: control internals, templates, interaction session internals, and visual-only implementation details

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
