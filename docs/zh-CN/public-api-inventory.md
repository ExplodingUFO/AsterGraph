# Public API Inventory

这份清单是 AsterGraph 公开包 surface 的维护者地图。它不创建新的 runtime model，只给已经公开的 surface 分类，让宿主能区分 canonical API、hosted 组合 helper、retained 迁移桥、compatibility-only shim 和 internal implementation detail。

请和 [Host Integration](./host-integration.md)、[Extension Contracts](./extension-contracts.md) 一起使用。如果这些文档说法不一致，先修正文档，再扩大 public surface。

## 支持层级

| 层级 | 含义 | 消费者建议 |
| --- | --- | --- |
| Stable canonical | 定义受支持宿主接入路线的 runtime/session contract。 | 新工作和自定义 UI / 原生 shell 接入默认使用。 |
| Supported hosted helper | 建立在 canonical route 和 shipped Avalonia adapter 上的薄组合 helper。 | 当默认 Avalonia 路线足够时使用。 |
| Retained migration | 为旧宿主分批迁移保留的 MVVM / view surface。 | 只作为迁移桥使用。 |
| Compatibility-only | 已有 canonical 替代路线的 obsolete 或旧形状 helper。 | 新工作不要使用；按 replacement guidance 迁移。 |
| Internal-only | implementation detail、internal namespace、测试、样例和 proof tool。 | 不是公开支持契约。 |

## 包级清单

| 包 | Stable canonical | Supported hosted helper | Retained migration | Compatibility-only | Internal-only boundary |
| --- | --- | --- | --- | --- | --- |
| `AsterGraph.Abstractions` | 节点定义、端口定义、provider / plugin-facing contract、identifier、metadata DTO，以及只产出这些 DTO 的 thin definition builder。 | 无。 | 无。 | 当前不作为主支持层级发布。 | 未通过 package docs 暴露的实现 helper。 |
| `AsterGraph.Core` | 图文档、序列化模型契约、兼容规则输入、thin implicit-conversion rule builder，以及 editor/session 组合使用的共享数据类型。 | 无。 | 无。 | 只有在已有 runtime-first 路线时保留旧 conversion / compatibility helper。 | Core internals 与持久化实现细节。 |
| `AsterGraph.Editor` | `AsterGraphEditorFactory.CreateSession(...)`、`IGraphEditorSession`、`IGraphEditorCommands`、`IGraphEditorQueries`、DTO / snapshot 查询、diagnostics、automation、validation snapshots、runtime overlay snapshots/providers、layout plan snapshots/providers、plugin discovery / inspection、export services，包括 raster export progress/cancel options。 | `AsterGraphEditorFactory.Create(...)` 是 hosted 组合 helper，但仍返回 retained facade。 | `GraphEditorViewModel`、`GraphEditorViewModel.Session`、迁移宿主使用的 retained menu / context-menu hook。 | `IGraphEditorQueries.GetCompatibleTargets(...)`、`CompatiblePortTarget`，以及已有 `GetCompatiblePortTargets(...)` 或 command/query snapshot 替代物的旧 MVVM 形状 helper。 | `Runtime.Internal`、`Kernel.Internal`、projection/apply internals、proof-only helper。 |
| `AsterGraph.Avalonia` | canonical editor/session route 上的 adapter 投影，以及复用同一 runtime owner 的 hosted factory。 | `AsterGraphAvaloniaViewFactory.Create(...)`、`AsterGraphCanvasViewFactory`、`AsterGraphInspectorViewFactory`、`AsterGraphMiniMapViewFactory`、`AsterGraphHostBuilder`、`AsterGraphWorkbenchOptions`。 | 仍使用 retained editor facade 的宿主可嵌入 `GraphEditorView`。 | 仅限把现有宿主桥接到 canonical route 的 adapter-specific compatibility glue。 | Control internals、templates、interaction session internals、visual-only implementation details。 |

## 路线映射

| 宿主路线 | 支持层级 | 主要符号 | 说明 |
| --- | --- | --- | --- |
| 仅运行时 / 自定义 UI | Stable canonical | `AsterGraphEditorFactory.CreateSession(...)`、`IGraphEditorSession`、`IGraphEditorCommands.TryCreateConnectedNodeFromPendingConnection(...)`、`IGraphEditorCommands.TryInsertNodeIntoConnection(...)`、`IGraphEditorCommands.TryDeleteSelectionAndReconnect(...)`、`IGraphEditorCommands.TryDetachSelectionFromConnections(...)`、`IGraphEditorCommands.SetConnectionSelection(...)`、`IGraphEditorCommands.FitSelectionToViewport(...)`、`IGraphEditorCommands.FocusSelection(...)`、`IGraphEditorCommands.FocusCurrentScope(...)`、`IGraphEditorCommands.TryFocusValidationIssue(...)`、`IGraphEditorCommands.TryDeleteSelectedConnections()`、`IGraphEditorCommands.TrySliceConnections(...)`、`IGraphEditorQueries.GetSelectedNodeConnectionIds()`、`IGraphEditorQueries.GetValidationSnapshot()`、`IGraphEditorQueries.GetRuntimeOverlaySnapshot()`、`IGraphEditorQueries.CreateLayoutPlan(...)`、`GraphEditorCompatiblePortTargetSnapshot.PortHandleId`、`GraphEditorCompatiblePortTargetSnapshot.PortGroupName`、`GraphEditorCompatiblePortTargetSnapshot.ConnectionHint`、`GraphEditorPendingConnectionSnapshot.TargetNodeId`、`GraphEditorPendingConnectionSnapshot.TargetPortId`、`GraphEditorPendingConnectionSnapshot.IsTargetCompatible`、`GraphEditorPendingConnectionSnapshot.ValidationMessage`、`GraphEditorSelectionSnapshot.SelectedConnectionIds`、`GraphEditorValidationSnapshot`、`GraphEditorValidationIssueSnapshot`、`GraphEditorValidationIssueSnapshot.HelpTarget`、`GraphEditorValidationHelpTargetSnapshot`、`IGraphRuntimeOverlayProvider`、`GraphEditorRuntimeOverlaySnapshot`、`IGraphLayoutProvider`、`GraphLayoutPlan`、`GraphEditorSceneImageExportOptions`、`GraphEditorSceneImageExportProgressSnapshot`、`GraphEditorSceneImageExportScope` | 新的自定义 UI 和原生 shell 默认从这里开始；authoring productivity 命令、validation/readiness feedback、metadata-backed validation help target、port handle/group/hint metadata、pending invalid-target feedback、viewport selection/focus 命令、wire-selection snapshot、host-owned runtime feedback overlay、provider-owned layout plan 和 raster export progress/cancel/scope evidence 都走 canonical session route。 |
| Shipped Avalonia UI | Supported hosted helper | `AsterGraphEditorFactory.Create(...)`、`AsterGraphAvaloniaViewFactory.Create(...)` | 使用同一个 runtime owner，不是第二套 runtime model。 |
| Thin hosted builder | Supported hosted helper | `AsterGraphHostBuilder.Create(...).UseDefaultWorkbench().BuildAvaloniaView()` | 减少常见 Avalonia 组合样板，并委托给 canonical factories；`AsterGraphWorkbenchOptions` 只控制 stock hosted chrome。 |
| Hosted customization surface | Supported hosted helper | `AsterGraphPresentationOptions`、`IGraphNodeVisualPresenter`、`GraphNodeVisual.PortAnchors`、`GraphNodeVisual.ConnectionTargetAnchors`、`INodeParameterEditorRegistry`、`IGraphEditorQueries.GetConnectionGeometrySnapshots()`、`IGraphRuntimeOverlayProvider`、`IGraphEditorQueries.GetRuntimeOverlaySnapshot()` | 受支持的自定义节点、handle / anchor、edge overlay、参数 editor、inspector 和 runtime decoration 路线。自定义边视觉走 stock connection styling 或基于 geometry snapshots 的宿主自管 overlay；`NodeCanvas` 内部层，包括 `OverlayLayer`，不是 public API。 |
| Hosted performance mode | Supported hosted helper | `AsterGraphWorkbenchPerformanceMode`、`AsterGraphWorkbenchPerformancePolicy`、`AsterGraphWorkbenchOptions.PerformanceMode` | 只调节 stock Avalonia workbench projection；不是 runtime graph contract 或 execution mode。 |
| Thin definition builders | Stable canonical | `NodeDefinitionBuilder`、`PortDefinitionBuilder`、`PortDefinition.HandleId`、`PortDefinition.ConnectionHint`、`GraphPort.HandleId`、`GraphPort.ConnectionHint`、`NodeParameterDefinitionBuilder`、`ImplicitConversionRuleBuilder` | 只是 convenience constructor 和稳定 port/handle metadata；每个 builder 都落到现有 DTO，不创建第二套 authoring schema 或 runtime model。 |
| Retained migration bridge | Retained migration | `GraphEditorViewModel`、`GraphEditorView`、`GraphEditorViewModel.Session` | 只给旧宿主分批迁移使用。 |
| 旧 compatible-target query | Compatibility-only | `IGraphEditorQueries.GetCompatibleTargets(...)`、`CompatiblePortTarget` | 优先使用 `GetCompatiblePortTargets(...)` 和 `GraphEditorCompatiblePortTargetSnapshot`。 |

## Release Handoff

- release handoff：Stable canonical surface 是默认路线，Retained migration surface 只作为迁移桥，Compatibility-only 或 obsolete surface 必须继续带 replacement guidance。
- 在同一个 release proof block 中同时保留 `PUBLIC_API_SURFACE_OK`、`PUBLIC_API_SCOPE_OK`、`PUBLIC_API_GUIDANCE_OK`、`ASTERGRAPH_TEMPLATE_SMOKE_OK` 和 `TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK`。
- Release-candidate proof markers：`API_RELEASE_CANDIDATE_PROOF_OK:True`、`PUBLIC_API_GUIDANCE_HANDOFF_OK:True` 和 `RELEASE_BOUNDARY_STABILITY_OK:True`。
- v0.61 API stabilization markers：`PUBLIC_API_DIFF_GATE_OK:True`、`PUBLIC_API_USAGE_GUIDANCE_OK:True` 和 `PUBLIC_API_STABILITY_SCOPE_OK:True`。
- v0.61 adoption/API handoff markers：`ADOPTION_API_STABILIZATION_HANDOFF_OK:True`、`ADOPTION_API_SCOPE_BOUNDARY_OK:True` 和 `V061_MILESTONE_PROOF_OK:True`。
- v0.75 customization handoff markers：`CUSTOM_EXTENSION_NODE_PRESENTER_LIFECYCLE_OK:True`、`CUSTOM_EXTENSION_ANCHOR_SURFACE_OK:True`、`CUSTOM_EXTENSION_EDGE_OVERLAY_OK:True`、`CUSTOM_EXTENSION_RUNTIME_INSPECTOR_OK:True`、`CUSTOM_EXTENSION_SCOPE_BOUNDARY_OK:True` 和 `CUSTOM_EXTENSION_SURFACE_OK:True`。
- 这份 handoff 不新增 analyzer、adapter 或 compatibility 承诺；它只发布当前 package guidance 和 proof marker。

## Drift 规则

- 新增 public host-facing symbol 必须在发布前进入这份 inventory 分类。
- Public API 变化必须有意更新 `eng/public-api-baseline.txt`；未分类 drift 会让 release gate 失败。
- 新增 Stable canonical surface 必须同步到 [Host Integration](./host-integration.md) 或 [Extension Contracts](./extension-contracts.md)。
- 新增 retained migration 或 compatibility-only surface 必须带 replacement guidance。
- 当 canonical replacement 已存在时，compatibility-only API 必须标记 obsolete。
- README、quick-start 和 release notes 不应宣传 internal implementation details。

## Baseline Gate

`eng/validate-public-api-surface.ps1` 只覆盖四个受支持公开包：`AsterGraph.Abstractions`、`AsterGraph.Core`、`AsterGraph.Editor` 和 `AsterGraph.Avalonia`。Demo、sample、WPF validation adapter、tools 和 tests 不进入 package support contract。

检查当前 baseline：

```powershell
.\eng\validate-public-api-surface.ps1 -Configuration Release -Framework net9.0
```

有意改变 public API 后重新生成 baseline：

```powershell
.\eng\validate-public-api-surface.ps1 -Configuration Release -Framework net9.0 -UpdateBaseline
```

Release proof 必须在 `PUBLIC_API_SURFACE_OK`、`PUBLIC_API_GUIDANCE_OK`、`PUBLIC_API_DIFF_GATE_OK:True`、`PUBLIC_API_USAGE_GUIDANCE_OK:True`、`PUBLIC_API_STABILITY_SCOPE_OK:True`、`ADOPTION_API_STABILIZATION_HANDOFF_OK:True`、`ADOPTION_API_SCOPE_BOUNDARY_OK:True` 和 `V061_MILESTONE_PROOF_OK:True` 旁边包含 `PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia`。

## 维护者检查表

发布前：

1. 检查 package docs、README、quick-start 和 release notes 是否使用同一套路由命名。
2. 检查 retained 与 compatibility-only API 是否仍指向 canonical replacement。
3. 检查 `AsterGraphHostBuilder` 是否仍被描述为 thin hosted helper，而不是 runtime model。
4. 检查 WPF wording 是否仍保持 validation-only，除非 adapter support matrix 明确改变。
