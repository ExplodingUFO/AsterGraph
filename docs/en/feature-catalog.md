# Feature Catalog

This catalog is the governance layer for AsterGraph capability growth. It does not create a new runtime route, package, adapter promise, marketplace, sandbox, execution engine, or GA claim. It records how each feature maps back to the canonical session/runtime surface, the shipped Avalonia projection, validation-only WPF status, sample coverage, proof markers, and performance budget.

Catalog proof markers: `FEATURE_CATALOG_OK:True`, `FEATURE_MANIFEST_BOUNDARY_OK:True`, and `FEATURE_PACK_GOVERNANCE_OK:True`.

## Manifest Fields

Every new feature record must keep these fields together:

- `FeatureId`: stable lowercase dotted id, for example `workbench.stencil.search`
- `Pack`: `Core`, `Authoring`, `Workbench`, `Advanced Graph`, or `Diagnostics`
- `Status`: `Beta`, `Preview`, `DemoOnly`, or `Internal`
- `Public seam`: canonical API, hosted helper, recipe, or explicit `None`
- `Avalonia projection`: `Supported`, `Partial`, `Fallback`, or `NotApplicable`
- `WPF projection`: `Supported`, `Partial`, `Fallback`, or `NotApplicable`
- `Sample / Demo entry`: first copyable sample or showcase entry
- `Proof marker`: marker that proves the feature remains present
- `Perf budget`: defended tier, telemetry-only tier, or `NotApplicable`
- `Docs`: primary English and Chinese docs

## Feature Packs

| Pack | Scope | Boundary |
| --- | --- | --- |
| Core | Selection, history, clipboard, save/load, zoom/pan, shortcuts | Canonical runtime/session route first |
| Authoring | Node surface, parameters, ports, edge authoring, validation, quick tools | Thin commands and metadata over existing definitions |
| Workbench | Stencil, MiniMap, Inspector, toolbar, command palette, export panel, fragment library | Hosted Avalonia composition, not a second runtime model |
| Advanced Graph | Groups, hierarchy, composite scope, edge notes, edge geometry | Existing advanced-editing modules and proof markers |
| Diagnostics | Runtime overlay, support bundle, plugin trust, performance proof, logs | Host-owned evidence and local support artifacts only |

## Initial Records

| FeatureId | Pack | Status | Public seam | Avalonia projection | WPF projection | Sample / Demo entry | Proof marker | Perf budget | Docs |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `core.selection` | Core | Beta | `SetSelection(...)`, `GetSelectionSnapshot()` | Supported | Partial | `AsterGraph.HelloWorld`, `ScaleSmoke` | `SCALE_PERFORMANCE_BUDGET_OK:*` | `baseline` / `large` defended | [Host Integration](./host-integration.md), [Quick Start](./quick-start.md) |
| `core.history` | Core | Beta | `Undo()`, `Redo()`, save/dirty contract | Supported | Partial | `ScaleSmoke` | `SCALE_HISTORY_CONTRACT_OK` | `baseline` / `large` defended | [State Contracts](./state-contracts.md) |
| `core.clipboard` | Core | Beta | `TryCopySelectionAsync()`, `TryPasteSelectionAsync()` | Supported | Fallback | `HostSample` | `HOST_SAMPLE_OK` | NotApplicable | [Host Integration](./host-integration.md) |
| `workbench.minimap` | Workbench | Beta | session/viewport snapshots plus `AsterGraphMiniMapViewFactory.Create(...)` | Supported | Fallback | `AsterGraph.Demo` | `DEMO_OK:True` | `large` defended through hosted metrics | [Host Integration](./host-integration.md), [Demo Guide](./demo-guide.md) |
| `workbench.stencil.basic` | Workbench | Beta | session stencil discovery and insertion commands | Supported | Fallback | `AsterGraph.Demo` | `CAPABILITY_BREADTH_STENCIL_OK:True` | `large` defended through command/search budgets | [Capability Breadth Recipe](./capability-breadth-recipe.md) |
| `workbench.export.scene` | Workbench | Beta | `IGraphSceneSvgExportService`, `TryExportSceneAsSvg()`, raster export options | Supported | Fallback | `HostSample`, `ScaleSmoke` | `SCALE_RASTER_EXPORT_STRESS_OK:True` | `stress` raster export defended, `xlarge` telemetry-only | [ScaleSmoke Baseline](./scale-baseline.md) |
| `authoring.node-surface` | Authoring | Beta | node surface snapshots, size commands, parameter edits | Supported | Partial | `AsterGraph.Demo` | `AUTHORING_SURFACE_OK:True` | `baseline` / `large` authoring defended | [Authoring Surface Recipe](./authoring-surface-recipe.md) |
| `authoring.inspector.metadata` | Authoring | Beta | definition-driven parameter metadata | Supported | Fallback | `ConsumerSample.Avalonia` | `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True` | inspector-open budget defended | [Authoring Inspector Recipe](./authoring-inspector-recipe.md) |
| `authoring.edge-flow` | Authoring | Beta | connection start/complete/reconnect/disconnect commands | Supported | Partial | `HostSample`, `ScaleSmoke` | `AUTHORING_FLOW_PROOF_OK:True` | authoring budgets defended | [Host Integration](./host-integration.md) |
| `advanced.hierarchy` | Advanced Graph | Beta | hierarchy snapshots and group commands | Supported | Fallback | `AsterGraph.Demo` | `HIERARCHY_SEMANTICS_OK:True` | large graph command budget defended | [Advanced Editing](./advanced-editing.md) |
| `advanced.composite-scope` | Advanced Graph | Beta | composite scope commands and scope navigation snapshots | Supported | Fallback | `AsterGraph.Demo` | `COMPOSITE_SCOPE_OK:True` | large graph command budget defended | [Advanced Editing](./advanced-editing.md) |
| `advanced.edge-geometry` | Advanced Graph | Beta | connection geometry snapshots and route-vertex commands | Supported | Fallback | `AsterGraph.Demo` | `EDGE_GEOMETRY_OK:True` | large graph command budget defended | [Advanced Editing](./advanced-editing.md) |
| `diagnostics.runtime-overlay` | Diagnostics | Beta | `IGraphRuntimeOverlayProvider`, runtime overlay snapshots | Supported | NotApplicable | `ConsumerSample.Avalonia`, `AsterGraph.Demo` | `RUNTIME_OVERLAY_SNAPSHOT_OK:True` | NotApplicable | [Support Bundle](./support-bundle.md) |
| `diagnostics.plugin-trust` | Diagnostics | Beta | plugin discovery, trust policy, load snapshots | Supported | NotApplicable | `ConsumerSample.Avalonia`, `PluginTool` | `CONSUMER_SAMPLE_TRUST_OK:True` | NotApplicable | [Plugin Trust Contracts](./plugin-trust-contracts.md) |
| `diagnostics.support-bundle` | Diagnostics | Beta | local support bundle output | Supported | NotApplicable | `ConsumerSample.Avalonia` | `SUPPORT_BUNDLE_OK:True` | NotApplicable | [Support Bundle](./support-bundle.md) |

## Governance Rules

New feature work must add or update a catalog record in the same change as its docs/proof updates. A record with `DemoOnly` or `Internal` status must not appear as a public support promise. A `Partial` or `Fallback` WPF projection remains validation-only and must not be described as WPF parity or public WPF support.

If a feature touches graph size, authoring, export, search, layout, or workbench projection, it must name the defended performance tier or explicitly say `telemetry-only` / `NotApplicable`.

## Related Docs

- [Host Integration](./host-integration.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Public API Inventory](./public-api-inventory.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
- [Public Launch Checklist](./public-launch-checklist.md)
