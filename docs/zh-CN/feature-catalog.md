# Feature Catalog

这份 catalog 是 AsterGraph capability 增长的治理层。它不创建新的 runtime route、package、adapter 承诺、marketplace、sandbox、execution engine 或 GA 声明。它只记录每个 feature 如何回到 canonical session/runtime surface、shipped Avalonia projection、validation-only WPF 状态、sample 覆盖、proof marker 和 performance budget。

Catalog proof markers：`FEATURE_CATALOG_OK:True`、`FEATURE_MANIFEST_BOUNDARY_OK:True` 和 `FEATURE_PACK_GOVERNANCE_OK:True`。

## Manifest 字段

每个新增 feature record 都必须同时保留这些字段：

- `FeatureId`：稳定的小写 dotted id，例如 `workbench.stencil.search`
- `Pack`：`Core`、`Authoring`、`Workbench`、`Advanced Graph` 或 `Diagnostics`
- `Status`：`Beta`、`Preview`、`DemoOnly` 或 `Internal`
- `Public seam`：canonical API、hosted helper、recipe，或明确写 `None`
- `Avalonia projection`：`Supported`、`Partial`、`Fallback` 或 `NotApplicable`
- `WPF projection`：`Supported`、`Partial`、`Fallback` 或 `NotApplicable`
- `Sample / Demo entry`：第一个可复制 sample 或 showcase 入口
- `Proof marker`：证明 feature 仍存在的 marker
- `Perf budget`：defended tier、telemetry-only tier，或 `NotApplicable`
- `Docs`：主要英文和中文文档

## Feature Packs

| Pack | 范围 | 边界 |
| --- | --- | --- |
| Core | Selection、history、clipboard、save/load、zoom/pan、shortcuts | 优先回到 canonical runtime/session route |
| Authoring | Node surface、parameters、ports、edge authoring、validation、quick tools | 基于现有 definitions 的薄命令和 metadata |
| Workbench | Stencil、MiniMap、Inspector、toolbar、command palette、export panel、fragment library | Hosted Avalonia composition，不是第二套 runtime model |
| Advanced Graph | Groups、hierarchy、composite scope、edge notes、edge geometry | 现有 advanced-editing modules 和 proof markers |
| Diagnostics | Runtime overlay、support bundle、plugin trust、performance proof、logs | 只做 host-owned evidence 和本地 support artifact |

## 初始记录

| FeatureId | Pack | Status | Public seam | Avalonia projection | WPF projection | Sample / Demo entry | Proof marker | Perf budget | Docs |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `core.selection` | Core | Beta | `SetSelection(...)`、`GetSelectionSnapshot()` | Supported | Partial | `AsterGraph.HelloWorld`、`ScaleSmoke` | `SCALE_PERFORMANCE_BUDGET_OK:*` | `baseline` / `large` defended | [Host Integration](./host-integration.md)、[Quick Start](./quick-start.md) |
| `core.history` | Core | Beta | `Undo()`、`Redo()`、save/dirty contract | Supported | Partial | `ScaleSmoke` | `SCALE_HISTORY_CONTRACT_OK` | `baseline` / `large` defended | [State Contracts](./state-contracts.md) |
| `core.clipboard` | Core | Beta | `TryCopySelectionAsync()`、`TryPasteSelectionAsync()` | Supported | Fallback | `HostSample` | `HOST_SAMPLE_OK` | NotApplicable | [Host Integration](./host-integration.md) |
| `workbench.defaults` | Workbench | Beta | `AsterGraphHostBuilder.UseDefaultWorkbench()` 加 `AsterGraphWorkbenchOptions` | Supported | NotApplicable | `Starter.Avalonia`、`ConsumerSample.Avalonia` | `WORKBENCH_DEFAULTS_OK:True` | 继承 command/stencil budgets 下的 hosted workbench metrics | [Host Integration](./host-integration.md)、[Consumer Sample](./consumer-sample.md) |
| `workbench.performance-mode` | Workbench | Beta | `AsterGraphWorkbenchPerformanceMode` 加 hosted `AsterGraphWorkbenchPerformancePolicy` projection limits | Supported | NotApplicable | `Starter.Avalonia`、`ConsumerSample.Avalonia` | `WORKBENCH_PERFORMANCE_MODE_OK:True`、`BALANCED_MODE_DEFAULT_OK:True`、`WORKBENCH_LOD_POLICY_OK:True`、`PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True` | hosted projection policy 绑定 command/stencil budgets | [Host Integration](./host-integration.md)、[Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) |
| `workbench.layout-state` | Workbench | Beta | `AsterGraphWorkbenchLayoutPreset` 加 hosted `AsterGraphWorkbenchPanelState` | Supported | NotApplicable | `Starter.Avalonia`、`ConsumerSample.Avalonia` | `WORKBENCH_LAYOUT_PRESETS_OK:True`、`WORKBENCH_LAYOUT_RESET_OK:True`、`PANEL_STATE_PERSISTENCE_OK:True` | hosted panel-state evidence；不是 runtime layout contract | [Host Integration](./host-integration.md)、[Consumer Sample](./consumer-sample.md) |
| `workbench.large-graph-ux-policy` | Workbench | Beta | 现有 workbench options 上的 hosted performance-mode 与 LOD policy 证据 | Supported | NotApplicable | `ConsumerSample.Avalonia` | `LARGE_GRAPH_UX_POLICY_OK:True`、`LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`、`LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`、`VIEWPORT_LOD_POLICY_OK:True`、`SELECTED_HOVERED_ADORNER_SCOPE_OK:True`、`LARGE_GRAPH_BALANCED_UX_OK:True`、`VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True` | 聚合现有 hosted metrics；不是新的图规模支持层级 | [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)、[ScaleSmoke 基线](./scale-baseline.md) |
| `workbench.edge-interaction-evidence` | Workbench | Beta | 基于现有 descriptor 的 hosted edge quick-tool、toolbar 和 geometry proof | Supported | NotApplicable | `ConsumerSample.Avalonia` | `EDGE_INTERACTION_CACHE_OK:True`、`EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`、`SELECTED_EDGE_FEEDBACK_OK:True`、`EDGE_RENDERING_SCOPE_BOUNDARY_OK:True` | 聚合现有 edge 证据；不是 runtime renderer contract | [Consumer Sample](./consumer-sample.md)、[Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) |
| `workbench.panel-projection-evidence` | Workbench | Beta | hosted mini-map lightweight projection 与 selected-scope inspector projection 证据 | Supported | NotApplicable | `ConsumerSample.Avalonia` | `MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True`、`INSPECTOR_NARROW_PROJECTION_OK:True`、`LARGE_GRAPH_PANEL_SCOPE_OK:True`、`PROJECTION_PERFORMANCE_EVIDENCE_OK:True` | 聚合现有 panel projection 证据；不是 broad graph subscription contract | [Consumer Sample](./consumer-sample.md)、[Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) |
| `workbench.large-graph-ux-handoff` | Workbench | Beta | v0.59 hosted Large Graph UX proof aggregation across phases 371-374 | Supported | NotApplicable | `ConsumerSample.Avalonia` | `LARGE_GRAPH_UX_HANDOFF_OK:True`、`LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`、`V059_MILESTONE_PROOF_OK:True` | 聚合现有 hosted 证据；不是新的图规模支持层级 | [Consumer Sample](./consumer-sample.md)、[Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) |
| `adapter2.validation-scope` | Diagnostics | Beta | canonical session/runtime route 上的 validation-only WPF adapter-2 proof | Supported | Supported | `AsterGraph.HelloWorld.Wpf` | `ADAPTER2_VALIDATION_SCOPE_OK:True`、`ADAPTER2_MATRIX_HANDOFF_OK:True`、`ADAPTER2_SCOPE_BOUNDARY_OK:True` | 只做 validation-only metrics 和 matrix evidence；不是 WPF parity 或 public WPF support | [Adapter Capability Matrix](./adapter-capability-matrix.md)、[Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md) |
| `adapter2.wpf-proof-sample` | Diagnostics | Beta | canonical session/runtime route 上可复制的 WPF proof sample evidence | Supported | Supported | `AsterGraph.HelloWorld.Wpf` | `ADAPTER2_WPF_SAMPLE_PROOF_OK:True`、`ADAPTER2_CANONICAL_ROUTE_OK:True`、`ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True` | 只做 validation-only sample evidence；不是第二条 onboarding route | [Adapter Capability Matrix](./adapter-capability-matrix.md)、[Host Integration](./host-integration.md) |
| `workbench.fragment-library` | Workbench | Beta | host-owned snippet catalog 加现有 fragment/session commands | Supported | Partial | `ConsumerSample.Avalonia` | `GRAPH_SNIPPET_CATALOG_OK:True`、`GRAPH_SNIPPET_INSERT_OK:True`、`FRAGMENT_LIBRARY_SEARCH_OK:True`、`FRAGMENT_LIBRARY_PREVIEW_OK:True`、`FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK:True`、`FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK:True` | NotApplicable | [Consumer Sample](./consumer-sample.md)、[Quick Start](./quick-start.md) |
| `workbench.discovery-surface` | Workbench | Beta | unified source-labelled discovery proof，覆盖 stencil templates、host snippets、graph search、plugin gallery entries 和 command palette actions | Supported | Partial | `ConsumerSample.Avalonia` | `UNIFIED_DISCOVERY_SURFACE_OK:True`、`DISCOVERY_SOURCE_LABELS_OK:True`、`DISCOVERY_COMMAND_ROUTE_OK:True` | 复用现有 command/session routes；不是 macro engine 或后台索引服务 | [Consumer Sample](./consumer-sample.md)、[Quick Start](./quick-start.md) |
| `workbench.recents-favorites` | Workbench | Beta | bounded host-owned recents/favorites evidence，覆盖 nodes、fragments、commands 和 plugin/source entries | Supported | Partial | `ConsumerSample.Avalonia` | `WORKBENCH_RECENTS_OK:True`、`WORKBENCH_FAVORITES_OK:True`、`RECENTS_FAVORITES_SUPPORT_BUNDLE_OK:True` | 只输出 support-bundle evidence；不是 remote sync 或 marketplace state | [Consumer Sample](./consumer-sample.md)、[Quick Start](./quick-start.md) |
| `workbench.discoverability-handoff` | Workbench | Beta | v0.63 hosted discoverability handoff，汇总 layout presets、panel state、unified discovery 和 recents/favorites | Supported | NotApplicable | `ConsumerSample.Avalonia` | `WORKBENCH_DISCOVERABILITY_HANDOFF_OK:True`、`WORKBENCH_DISCOVERABILITY_SCOPE_BOUNDARY_OK:True`、`V063_MILESTONE_PROOF_OK:True` | 汇总 hosted workbench evidence；不是 runtime route、macro/query system、remote sync、marketplace state、WPF parity 或 GA claim | [Consumer Sample](./consumer-sample.md)、[Project Status](./project-status.md) |
| `workbench.command-projection` | Workbench | Beta | shared command descriptors 加 hosted action projection | Supported | Partial | `ConsumerSample.Avalonia` | `COMMAND_PROJECTION_UNIFIED_OK:True`、`COMMAND_PALETTE_OK:True`、`TOOLBAR_DESCRIPTOR_OK:True`、`CONTEXT_MENU_DESCRIPTOR_OK:True`、`COMMAND_DISABLED_REASON_OK:True`、`NODE_TOOLBAR_CONTRIBUTION_OK:True`、`EDGE_TOOLBAR_CONTRIBUTION_OK:True`、`TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:True`、`TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:True` | command surface refresh budget defended | [Consumer Sample](./consumer-sample.md)、[Quick Start](./quick-start.md) |
| `workbench.minimap` | Workbench | Beta | session/viewport snapshots 加 `AsterGraphMiniMapViewFactory.Create(...)` | Supported | Fallback | `AsterGraph.Demo` | `DEMO_OK:True` | 通过 hosted metrics 防守 `large` | [Host Integration](./host-integration.md)、[Demo Guide](./demo-guide.md) |
| `workbench.stencil.basic` | Workbench | Beta | session stencil discovery 和 insertion commands | Supported | Fallback | `AsterGraph.Demo` | `CAPABILITY_BREADTH_STENCIL_OK:True`、`STENCIL_RECENTS_FAVORITES_OK:True` | 通过 command/search budget 防守 `large` | [Capability Breadth Recipe](./capability-breadth-recipe.md) |
| `workbench.export.scene` | Workbench | Beta | `IGraphSceneSvgExportService`、`TryExportSceneAsSvg()`、`TryExportSceneAsImage(...)`，以及 SVG/PNG/JPEG export panel 的 raster scope/progress/cancel options | Supported | Fallback | `HostSample`、`ScaleSmoke`、`ConsumerSample.Avalonia` | `EXPORT_PANEL_OK:True`、`EXPORT_PANEL_SCOPE_OK:True`、`EXPORT_PANEL_PROGRESS_CANCEL_OK:True`、`SCALE_RASTER_EXPORT_STRESS_OK:True` | `stress` raster export defended，`xlarge` telemetry-only | [ScaleSmoke Baseline](./scale-baseline.md) |
| `authoring.definition-builders` | Authoring | Beta | `NodeDefinitionBuilder`、`PortDefinitionBuilder`、`NodeParameterDefinitionBuilder`、`ImplicitConversionRuleBuilder`，只包装现有 DTO | NotApplicable | NotApplicable | `ConsumerSample.Avalonia`、`Editor.Tests` | `NODE_DEFINITION_BUILDER_OK:True`、`PORT_DEFINITION_BUILDER_OK:True`、`PARAMETER_DEFINITION_BUILDER_OK:True`、`CONNECTION_RULE_BUILDER_OK:True`、`AUTHORING_BUILDER_THIN_WRAPPER_OK:True` | NotApplicable | [Authoring Inspector Recipe](./authoring-inspector-recipe.md)、[Public API Inventory](./public-api-inventory.md) |
| `authoring.node-surface` | Authoring | Beta | node surface snapshots、size commands、parameter edits | Supported | Partial | `AsterGraph.Demo` | `AUTHORING_SURFACE_OK:True` | `baseline` / `large` authoring defended | [Authoring Surface Recipe](./authoring-surface-recipe.md) |
| `authoring.inspector.metadata` | Authoring | Beta | definition-driven parameter metadata | Supported | Fallback | `ConsumerSample.Avalonia` | `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True` | inspector-open budget defended | [Authoring Inspector Recipe](./authoring-inspector-recipe.md) |
| `authoring.edge-flow` | Authoring | Beta | connection start/complete/reconnect/disconnect commands | Supported | Partial | `HostSample`、`ScaleSmoke` | `AUTHORING_FLOW_PROOF_OK:True` | authoring budgets defended | [Host Integration](./host-integration.md) |
| `advanced.hierarchy` | Advanced Graph | Beta | hierarchy snapshots 和 group commands | Supported | Fallback | `AsterGraph.Demo` | `HIERARCHY_SEMANTICS_OK:True` | large graph command budget defended | [Advanced Editing](./advanced-editing.md) |
| `advanced.composite-scope` | Advanced Graph | Beta | composite scope commands 和 scope navigation snapshots | Supported | Fallback | `AsterGraph.Demo` | `COMPOSITE_SCOPE_OK:True` | large graph command budget defended | [Advanced Editing](./advanced-editing.md) |
| `advanced.edge-geometry` | Advanced Graph | Beta | connection geometry snapshots 和 route-vertex commands | Supported | Fallback | `AsterGraph.Demo` | `EDGE_GEOMETRY_OK:True` | large graph command budget defended | [Advanced Editing](./advanced-editing.md) |
| `diagnostics.runtime-overlay` | Diagnostics | Beta | `IGraphRuntimeOverlayProvider`、runtime overlay snapshots | Supported | NotApplicable | `ConsumerSample.Avalonia`、`AsterGraph.Demo` | `RUNTIME_OVERLAY_SNAPSHOT_OK:True` | NotApplicable | [Support Bundle](./support-bundle.md) |
| `diagnostics.plugin-trust` | Diagnostics | Beta | plugin discovery、trust policy、load snapshots | Supported | NotApplicable | `ConsumerSample.Avalonia`、`PluginTool` | `CONSUMER_SAMPLE_TRUST_OK:True` | NotApplicable | [Plugin Trust Contracts](./plugin-trust-contracts.md) |
| `diagnostics.support-bundle` | Diagnostics | Beta | 本地 support bundle 输出 | Supported | NotApplicable | `ConsumerSample.Avalonia` | `SUPPORT_BUNDLE_OK:True` | NotApplicable | [Support Bundle](./support-bundle.md) |
| `workbench.interaction-reliability-handoff` | Workbench | Beta | v0.62 interaction reliability milestone proof | Supported | NotApplicable | `ConsumerSample.Avalonia`、`AsterGraph.Demo` | `INTERACTION_RELIABILITY_HANDOFF_OK:True`、`INTERACTION_SCOPE_BOUNDARY_OK:True`、`V062_MILESTONE_PROOF_OK:True` | hosted interaction proof only | [Consumer Sample](./consumer-sample.md) |
| `adapter2.performance-accessibility-handoff` | Diagnostics | Beta | 现有 WPF proof budgets 和 adapter-2 recipes | Supported | Supported | `AsterGraph.HelloWorld.Wpf` | `ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True`、`ADAPTER2_RECIPE_ALIGNMENT_OK:True`、`ADAPTER2_PROOF_BUDGET_OK:True` | WPF validation-only proof budget | [Adapter Capability Matrix](./adapter-capability-matrix.md) |
| `adapter2.validation-handoff` | Diagnostics | Beta | v0.60 adapter-2 validation-only milestone proof | Supported | Supported | `AsterGraph.HelloWorld.Wpf` | `ADAPTER2_VALIDATION_HANDOFF_OK:True`、`ADAPTER2_VALIDATION_SCOPE_BOUNDARY_OK:True`、`V060_MILESTONE_PROOF_OK:True` | WPF validation-only milestone proof | [Adapter Capability Matrix](./adapter-capability-matrix.md) |

## 治理规则

新增 feature 工作必须在同一个变更里新增或更新 catalog record，并同步 docs/proof。`DemoOnly` 或 `Internal` 状态的 record 不能写成 public support promise。`Partial` 或 `Fallback` 的 WPF projection 仍然是 validation-only，不能写成 WPF parity 或 public WPF support。

如果 feature 影响 graph size、authoring、export、search、layout 或 workbench projection，就必须写出 defended performance tier，或者明确标成 `telemetry-only` / `NotApplicable`。

## 相关文档

- [Host Integration](./host-integration.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Public API Inventory](./public-api-inventory.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
- [Public Launch Checklist](./public-launch-checklist.md)
