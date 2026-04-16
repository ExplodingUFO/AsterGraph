# AsterGraph.Editor

`AsterGraph.Editor` is the standard host-facing runtime package for AsterGraph.

It belongs to the supported package set with `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Avalonia`, and it targets `net8.0` and `net9.0`.

It intentionally contains:

- `IGraphEditorSession` plus `Commands`, `Queries`, `Events`, and mutation batching
- `AsterGraphEditorFactory` and `AsterGraphEditorOptions`
- `GraphEditorViewModel`
- replaceable storage/clipboard/diagnostics seams
- node/template catalogs
- context-menu intent and command wiring
- selection, pending connection, and workspace state
- parameter Inspector view-model state

It intentionally does **not** contain:

- Avalonia visual controls
- demo node content
- host-specific business commands

Typical consumers:

- hosts that build or extend an editor session, even when they also ship the default Avalonia UI
- integration layers that extend menus, commands, inspector behavior, localization, or presentation

Route guidance:

- runtime-first session creation via `AsterGraphEditorFactory.CreateSession(...)`
- canonical hosted-UI creation via `AsterGraphEditorFactory.Create(...)`, then the Avalonia factories from `AsterGraph.Avalonia`
- core runtime interaction ownership via session commands for selection, node positioning, connection lifecycle, and viewport control
- pending connection observation via `GetPendingConnectionSnapshot()` and `PendingConnectionChanged`
- DTO-based compatible-target discovery via `GetCompatiblePortTargets(...)`
- staged migration support through the retained `GraphEditorViewModel` constructor
- package-neutral default storage redirection through `StorageRootPath`
- replaceable services via `IGraphWorkspaceService`, `IGraphFragmentWorkspaceService`, `IGraphFragmentLibraryService`, and `IGraphClipboardPayloadSerializer`
- recoverable-failure publication through `IGraphEditorDiagnosticsSink`
- readiness discovery through `GetFeatureDescriptors()` for capability, service, and integration seams
- localization via `IGraphLocalizationProvider`
- runtime node display state via `INodePresentationProvider`
- host-owned menu actions via `IGraphContextMenuAugmentor`
- typed host context access through `GraphHostContextExtensions`

Recommended split:

- use `CreateSession(...)` when the host owns the UI and wants the canonical runtime boundary
- use `Create(...)` when the host wants the shipped UI/factory story but still needs the retained editor facade
- use the direct `GraphEditorViewModel` constructor only when the host is intentionally staying on the retained compatibility path during migration

Stability tiers:

- stable canonical surfaces:
  - `AsterGraphEditorFactory.CreateSession(...)`
  - `AsterGraphEditorFactory.Create(...)`
  - `IGraphEditorSession`
  - DTO/snapshot queries such as `GetCompatiblePortTargets(...)`
- retained compatibility surfaces:
  - `GraphEditorViewModel`
  - `GraphEditorView`
  - `GraphEditorViewModel.Session`
- compatibility-only shims:
  - `GetCompatibleTargets(...)`
  - `CompatiblePortTarget`

Retirement guidance:

- keep new code on the stable canonical surfaces
- keep retained facade usage only where migration is still in progress
- treat compatibility-only shims as transitional; later minor releases may add stronger warnings and a future major release may remove them

Extension precedence:

- plugin trust is host-owned through `PluginTrustPolicy`
- localization providers compose plugin-first, host-last
- node presentation composes plugin-first, host-last, with host override fields winning final subtitle/description/status-bar output
- runtime session menus apply plugin augmentors over stock descriptors
- retained `GraphEditorViewModel.BuildContextMenu(...)` gives the host `IGraphContextMenuAugmentor` the final override point after runtime/plugin composition

Lane ownership:

- `all` = framework-matrix build/test
- `contract` = focused consumer/state-contract proof
- `maintenance` = hotspot-refactor proof
- `release` = publish gate
- `tests/AsterGraph.Demo.Tests` = demo/sample-host lane

Use this package together with `AsterGraph.Avalonia` when the host embeds the default shell or standalone Avalonia surfaces. Hosts that provide their own UI can stop at the `IGraphEditorSession` boundary and avoid taking an Avalonia dependency in their composition root.

The MVVM-typed compatibility query path remains available for legacy integrations, but new host code should treat it as compatibility-only rather than the canonical runtime surface.

Compatibility shim migration:

- canonical runtime query: `IGraphEditorQueries.GetCompatiblePortTargets(...)`
- retained compatibility shim: `IGraphEditorQueries.GetCompatibleTargets(...)` plus `CompatiblePortTarget`
- retained compatibility facade: `GraphEditorViewModel`
- v1.5 keeps the shim with strong migration guidance
- later minor releases may add stronger warnings
- future major release may remove it

The same guidance now applies to host extension seams:

- prefer `GraphContextMenuAugmentationContext` over taking `GraphEditorViewModel` directly
- prefer `NodePresentationContext` over taking `NodeViewModel` directly

The older MVVM-rooted extension methods remain available only as migration shims.

Phase 18 readiness proof is anchored on the session boundary, not on the retained constructor path:

- `tools/AsterGraph.HostSample` is the minimal consumer-facing sample for the canonical host path
- `src/AsterGraph.Demo` remains the visual/default host-composition sample
- `tools/AsterGraph.PackageSmoke` emits machine-checkable `PACKAGE_SMOKE_*` markers
- `tools/AsterGraph.ScaleSmoke` proves the same session/inspection-driven readiness story on a larger graph

Reference material:

- [Host Integration Guide](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/en/host-integration.md)
- [Extension Contracts](https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/en/extension-contracts.md)
- [Demo App](https://github.com/ExplodingUFO/AsterGraph/tree/master/src/AsterGraph.Demo)
- [Root README](https://github.com/ExplodingUFO/AsterGraph/blob/master/README.md)
