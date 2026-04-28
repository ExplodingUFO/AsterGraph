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
| `workbench.defaults` | Workbench | Beta | `AsterGraphHostBuilder.UseDefaultWorkbench()` plus `AsterGraphWorkbenchOptions` | Supported | NotApplicable | `Starter.Avalonia`, `ConsumerSample.Avalonia` | `WORKBENCH_DEFAULTS_OK:True` | hosted workbench metrics inherited from command/stencil budgets | [Host Integration](./host-integration.md), [Consumer Sample](./consumer-sample.md) |
| `workbench.performance-mode` | Workbench | Beta | `AsterGraphWorkbenchPerformanceMode` plus hosted `AsterGraphWorkbenchPerformancePolicy` projection limits | Supported | NotApplicable | `Starter.Avalonia`, `ConsumerSample.Avalonia` | `WORKBENCH_PERFORMANCE_MODE_OK:True`, `BALANCED_MODE_DEFAULT_OK:True`, `WORKBENCH_LOD_POLICY_OK:True`, `PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True` | hosted projection policy tied to command/stencil budgets | [Host Integration](./host-integration.md), [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) |
| `workbench.large-graph-ux-policy` | Workbench | Beta | hosted performance-mode and LOD policy evidence on existing workbench options | Supported | NotApplicable | `ConsumerSample.Avalonia` | `LARGE_GRAPH_UX_POLICY_OK:True`, `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`, `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`, `VIEWPORT_LOD_POLICY_OK:True`, `SELECTED_HOVERED_ADORNER_SCOPE_OK:True`, `LARGE_GRAPH_BALANCED_UX_OK:True`, `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True` | aggregates existing hosted metrics; not a new graph-size support tier | [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md), [ScaleSmoke Baseline](./scale-baseline.md) |
| `workbench.fragment-library` | Workbench | Beta | host-owned snippet catalog plus existing fragment/session commands | Supported | Partial | `ConsumerSample.Avalonia` | `GRAPH_SNIPPET_CATALOG_OK:True`, `GRAPH_SNIPPET_INSERT_OK:True`, `FRAGMENT_LIBRARY_SEARCH_OK:True`, `FRAGMENT_LIBRARY_PREVIEW_OK:True`, `FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK:True`, `FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK:True` | NotApplicable | [Consumer Sample](./consumer-sample.md), [Quick Start](./quick-start.md) |
| `workbench.command-projection` | Workbench | Beta | shared command descriptors plus hosted action projection | Supported | Partial | `ConsumerSample.Avalonia` | `COMMAND_PROJECTION_UNIFIED_OK:True`, `COMMAND_PALETTE_OK:True`, `TOOLBAR_DESCRIPTOR_OK:True`, `CONTEXT_MENU_DESCRIPTOR_OK:True`, `COMMAND_DISABLED_REASON_OK:True`, `NODE_TOOLBAR_CONTRIBUTION_OK:True`, `EDGE_TOOLBAR_CONTRIBUTION_OK:True`, `TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:True`, `TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:True` | command surface refresh budget defended | [Consumer Sample](./consumer-sample.md), [Quick Start](./quick-start.md) |
| `workbench.minimap` | Workbench | Beta | session/viewport snapshots plus `AsterGraphMiniMapViewFactory.Create(...)` | Supported | Fallback | `AsterGraph.Demo` | `DEMO_OK:True` | `large` defended through hosted metrics | [Host Integration](./host-integration.md), [Demo Guide](./demo-guide.md) |
| `workbench.stencil.basic` | Workbench | Beta | session stencil discovery and insertion commands | Supported | Fallback | `AsterGraph.Demo` | `CAPABILITY_BREADTH_STENCIL_OK:True`, `STENCIL_RECENTS_FAVORITES_OK:True` | `large` defended through command/search budgets | [Capability Breadth Recipe](./capability-breadth-recipe.md) |
| `workbench.export.scene` | Workbench | Beta | `IGraphSceneSvgExportService`, `TryExportSceneAsSvg()`, `TryExportSceneAsImage(...)`, raster scope/progress/cancel options for SVG/PNG/JPEG export panel evidence | Supported | Fallback | `HostSample`, `ScaleSmoke`, `ConsumerSample.Avalonia` | `EXPORT_PANEL_OK:True`, `EXPORT_PANEL_SCOPE_OK:True`, `EXPORT_PANEL_PROGRESS_CANCEL_OK:True`, `SCALE_RASTER_EXPORT_STRESS_OK:True` | `stress` raster export defended, `xlarge` telemetry-only | [ScaleSmoke Baseline](./scale-baseline.md) |
| `authoring.definition-builders` | Authoring | Beta | `NodeDefinitionBuilder`, `PortDefinitionBuilder`, `NodeParameterDefinitionBuilder`, `ImplicitConversionRuleBuilder` thin wrappers over existing DTOs | NotApplicable | NotApplicable | `ConsumerSample.Avalonia`, `Editor.Tests` | `NODE_DEFINITION_BUILDER_OK:True`, `PORT_DEFINITION_BUILDER_OK:True`, `PARAMETER_DEFINITION_BUILDER_OK:True`, `CONNECTION_RULE_BUILDER_OK:True`, `AUTHORING_BUILDER_THIN_WRAPPER_OK:True` | NotApplicable | [Authoring Inspector Recipe](./authoring-inspector-recipe.md), [Public API Inventory](./public-api-inventory.md) |
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
