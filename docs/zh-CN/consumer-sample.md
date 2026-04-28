# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` 是位于 canonical session/runtime 路线上的中等 hosted-UI 样例，排在 starter 脚手架和最小 `HelloWorld.Avalonia` 路线之后、完整 `AsterGraph.Demo` 展示宿主之前。它默认打开 `Content Review Release Lane` 场景图。
如果你要看 trust-policy 和本地证据，就把 [插件信任契约 v1](./plugin-trust-contracts.md) 和 [Beta Support Bundle](./support-bundle.md) 配在这条路线旁边。

它是这些宿主管线 seam 的宿主 seam 示例：

- action rail / command projection
- plugin trust workflow
- 选中节点参数读写 seam
- snippet catalog 与 connected-node 插入 seam

如果你要看 inspector metadata recipe，就把这条路线和 [Authoring Inspector Recipe](./authoring-inspector-recipe.md) 配在一起。这个样例只聚焦宿主自管 seam 和 shipped inspector surface；它不拥有元数据词汇。完整的 `defaultValue`、`isAdvanced`、`helpText`、`placeholderText` 和只读词汇都放在 canonical recipe 里。
如果你要在同一条路线里复制自定义节点、端口和边展示，就再配上 [Authoring Surface Recipe](./authoring-surface-recipe.md)。
如果你要在同一条路线里复制 searchable grouped stencil、SVG/PNG/JPEG export breadth，以及共享 node/edge quick tools，就再配上 [Capability Breadth Recipe](./capability-breadth-recipe.md)。
如果你要把这些 widened surface 继续和受防守的 `ScaleSmoke` 预算绑定成一条可复制的宿主调优 handoff，就再配上 [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)。
如果你要在同一条路线里复制宿主可控的键盘、焦点和可访问性 handoff，就再配上 [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md)。

这是把宿主自管 seam 复制到你自己的应用里的受防守 beta 路线。把 action projection、trust workflow 和选中节点参数读写 seam 保持在宿主里，只复制样例自有的展示细节。

它只停留在 canonical session/runtime model 上，不引入第二套 editor model、sandbox，或更大的 plugin ecosystem。
最终 experience handoff marker 只汇总已有 proof 行：`EXPERIENCE_POLISH_HANDOFF_OK:True`、`FEATURE_ENHANCEMENT_PROOF_OK:True` 和 `EXPERIENCE_SCOPE_BOUNDARY_OK:True`。
authoring flow handoff 会汇总 quick-add、insert-on-wire、reconnect editing、edge multiselect 和 wire slicing，并继续走现有 session command path：`AUTHORING_FLOW_PROOF_OK:True`、`AUTHORING_FLOW_HANDOFF_OK:True` 和 `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`。

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。
`HostSample` 是这条 ladder 之后的 proof harness。

## 它证明什么

这个样例保留了一个真实宿主窗口，但不会膨胀成完整 showcase shell。它会同时展示：

- 一条宿主自管的动作栏，通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 和 `AsterGraphHostedActionFactory.CreateProjection(...)` 从共享 command descriptor 投到宿主层
- 一组宿主自定义节点，这部分是样例自有且可替换的
- 一个 plugin command 也走同一条动作路径，而不是样例私有的菜单占位项
- 通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 和 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 做选中节点参数读写 seam
- 一个宿主自管 snippet catalog，通过 `StartConnection(...)` 和 `TryCreateConnectedNodeFromPendingConnection(...)` 插入样例 review queue lane
- 一个可信插件注册，以及可见的 provenance、trust reason 和 allowlist 导入/导出
- 一条 support-bundle proof 路径，带场景图、宿主自管动作、canonical graph readiness 证据、support-bundle payload readiness 和五分钟 handoff 健康度 markers
- 一条 authoring flow proof handoff，把 quick-add、insert-on-wire、reconnect editing、edge multiselect 和 wire slicing 保持在现有 session command path 上
- 一组最终 handoff marker，把 UX polish、feature enhancement proof 和 scope-boundary 证据合在一起，但不增加 runtime API、marketplace、sandbox 或 WPF parity
- 一条 runtime feedback proof handoff，包含 `RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`、`RUNTIME_LOG_LOCATE_OK:True` 和 `RUNTIME_LOG_EXPORT_OK:True`，并与 Demo 的 `AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`、`AI_PIPELINE_PAYLOAD_PREVIEW_OK:True` 和 `AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True` 配套核对；这些只是宿主自管 debug/support marker，不代表 execution engine、workflow scripting UI、marketplace、sandbox、WPF parity 或 GA 声明
- 一条宿主自管 navigation proof handoff，包含 `NAVIGATION_HISTORY_OK:True`、`SCOPE_BREADCRUMB_NAVIGATION_OK:True`、`FOCUS_RESTORE_OK:True`、`NAVIGATION_PRODUCTIVITY_PROOF_OK:True`、`NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True` 和 `NAVIGATION_SCOPE_BOUNDARY_OK:True`，不引入 command macro engine、后台图索引、WPF parity 或新的 runtime navigation API
- 基于 factory 的默认 Avalonia hosted-UI 路线

## 复制这些宿主自管 seam

- action rail / command projection：宿主动作放在编辑器壳层之外，并且通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 和 `AsterGraphHostedActionFactory.CreateProjection(...)` 投影共享 command descriptor
- plugin trust workflow：把 `GraphEditorPluginDiscoveryOptions`、`AsterGraphEditorOptions.PluginTrustPolicy`、provenance snapshot 和宿主自管 allowlist policy 放在同一层
- 选中节点参数读写 seam：只通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 读取当前选中节点参数，并只通过 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 写回
- snippet catalog 与 connected-node 插入 seam：snippet id 和 template 内容都留在宿主里，插入时只复用 `StartConnection(...)` 加 `TryCreateConnectedNodeFromPendingConnection(...)`，不把 snippet 写成 runtime abstraction

## 需要保持的路线边界

| 路线 | 从这个样例复制 | 不要复制 |
| ---- | -------------- | -------- |
| Hosted UI | `AsterGraphEditorFactory.Create(...)` 加 `AsterGraphAvaloniaViewFactory.Create(...)` 组合 | demo-only shell state 或 showcase 面板 |
| Runtime-only | 同一套 document/catalog 定义，然后在自有 UI 中使用 `AsterGraphEditorFactory.CreateSession(...)` | Avalonia 窗口布局 |
| Plugin | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)`、`PluginTrustPolicy`、provenance 和 allowlist 导入/导出 | 样例 plugin id、audit 节点族或 trust 文案 |
| Migration | 只有分批迁移现有宿主时才使用 retained `GraphEditorViewModel` / `GraphEditorView` | 把 retained surface 当成新主路线 |

## 替换这些样例自有内容

下面这些样例自有内容保持在你自己的 app 内部即可：

- review/audit 节点族
- action ids/titles
- snippet ids 和 catalog entries
- 窗口布局和叙述文本
- defended markers 之外的 proof 文案

## 宿主自管参数与元数据复制图

按每个 bounded source 复制它负责的那一部分：

- 从这份样例复制：action rail / command projection、plugin trust workflow，以及选中节点参数读写 seam。
- 从 [Authoring Inspector Recipe](./authoring-inspector-recipe.md) 复制：definition-driven 的元数据词汇（`defaultValue`、`isAdvanced`、`helpText`、`placeholderText`、`constraints.IsReadOnly`）以及 stock inspector 的字段分组。
- 本地保留：review/audit 节点族、action ids/titles、窗口布局和叙述文本，以及 defended markers 之外的 proof 文案。

按这个顺序复制：

- 定义 metadata：先从 [Authoring Inspector Recipe](./authoring-inspector-recipe.md) 复制 `defaultValue`、`editorKind`、`constraints` 和 `groupName`。
- 投影并写回：在这份样例里通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 和 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 处理选中节点值。
- 投影 inspector 状态：通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()`。
- 投影节点旁路编辑器状态：通过 `IGraphEditorSession.Queries.GetNodeParameterSnapshots(nodeId)` 和 `INodeParameterEditorRegistry`。
- 写回时继续走 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 或 `IGraphEditorSession.Commands.TrySetNodeParameterValue(...)`，把 validation 保留在共享 session command 路线上。
- 宿主动作继续从 `session.Queries.GetCommandDescriptors()` 和共享 host-action 路线投影，再用 proof mode 加 support bundle 对照 `parameterSnapshots`、`CONSUMER_SAMPLE_PARAMETER_OK:True`、`CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True` 和 `AUTHORING_SURFACE_OK:True`。
- 验证证据：用 proof mode 加 support bundle；把 `parameterSnapshots` 和 `CONSUMER_SAMPLE_PARAMETER_OK:True`、`CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`、`AUTHORING_SURFACE_OK:True` 对照。

Consumer Sample 证明 seam 分工；它不拥有元数据词汇。Authoring Inspector Recipe 是元数据词汇的唯一 owner。

## 可复制的 Widened Surface Performance Handoff

- 在 `AsterGraph.ConsumerSample.Avalonia -- --proof` 上继续守住 widened hosted metrics：`HOST_NATIVE_METRIC:stencil_search_ms`、`HOST_NATIVE_METRIC:command_surface_refresh_ms`、`HOST_NATIVE_METRIC:node_tool_projection_ms` 和 `HOST_NATIVE_METRIC:edge_tool_projection_ms`，并期待 `WIDENED_SURFACE_PERFORMANCE_OK:True`。
- 在 `ScaleSmoke` 上继续守住 `large` authoring/export 预算：`SCALE_AUTHORING_BUDGET_OK:large:True:none` 和 `SCALE_EXPORT_BUDGET_OK:large:True:none`。
- 当你要复制同一条调优 handoff 时，直接复用 [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)，不要再开新的 hosted proof lane。

## 可复制的 Hosted Accessibility Handoff

- 给 `GraphEditorView`、`NodeCanvas`、`GraphInspectorView` 和 stock 搜索 surface 保留稳定的 automation name，并期待 `HOSTED_ACCESSIBILITY_BASELINE_OK:True`。
- 把 command palette 的键盘流继续留在共享 hosted route 上，让焦点在关闭后回到打开它的 host surface，并期待 `HOSTED_ACCESSIBILITY_FOCUS_OK:True`。
- 把 hosted automation navigation 和 authoring diagnostics 继续放在同一轮 proof 里，并期待 `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True` 和 `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`。
- 让 header、palette、node-tool 和 edge-tool 的名称继续来自同一套共享 action descriptor，并期待 `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`。
- 把 selected-node parameter metadata 和 connection text editor 保留在同一条 hosted authoring route 上，最后配合 [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md) 收口到 `HOSTED_ACCESSIBILITY_OK:True`。
- 如果要走一条 screen-reader-ready 的本地评估路径，就把 `ConsumerSample.Avalonia -- --proof`、support-bundle 附件备注，以及 route ladder 之后的 `HostSample` proof 行放在同一条受限 intake 记录里。

## 信任与证明速查

可复制的信任与证明参考：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

预期 proof marker：

- `CONSUMER_SAMPLE_TRUST_OK:True`
- `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`
- `PORT_HANDLE_ID_OK:True`
- `PORT_GROUP_AUTHORING_OK:True`
- `PORT_CONNECTION_HINT_OK:True`
- `PORT_AUTHORING_SCOPE_BOUNDARY_OK:True`
- `CONNECTION_VALIDATION_REASON_OK:True`
- `CONNECTION_INVALID_HOVER_FEEDBACK_OK:True`
- `CONNECTION_VALIDATION_SUPPORT_BUNDLE_OK:True`
- `CONNECTION_VALIDATION_SCOPE_BOUNDARY_OK:True`
- `AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True`
- `AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True`
- `CAPABILITY_BREADTH_STENCIL_OK:True`
- `STENCIL_GROUPING_OK:True`
- `STENCIL_SEARCH_OK:True`
- `STENCIL_RECENTS_FAVORITES_OK:True`
- `STENCIL_SOURCE_FILTER_OK:True`
- `STENCIL_STATE_PERSISTENCE_OK:True`
- `CAPABILITY_BREADTH_EXPORT_OK:True`
- `EXPORT_PANEL_OK:True`
- `EXPORT_PANEL_SCOPE_OK:True`
- `EXPORT_PANEL_PROGRESS_CANCEL_OK:True`
- `EXPORT_DOCS_ALIGNMENT_OK:True`
- `CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`
- `NODE_TOOLBAR_CONTRIBUTION_OK:True`
- `EDGE_TOOLBAR_CONTRIBUTION_OK:True`
- `TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:True`
- `TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:True`
- `CAPABILITY_BREADTH_OK:True`
- `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`
- `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`
- `WIDENED_SURFACE_PERFORMANCE_OK:True`
- `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True`
- `CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`
- `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`
- `GRAPH_VALIDATION_FEEDBACK_OK:True`
- `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`
- `GRAPH_READINESS_STATUS_OK:True`
- `GRAPH_SNIPPET_CATALOG_OK:True`
- `GRAPH_SNIPPET_INSERT_OK:True`
- `WORKBENCH_DEFAULTS_OK:True`
- `WORKBENCH_HOST_BUILDER_HANDOFF_OK:True`
- `WORKBENCH_PERFORMANCE_MODE_OK:True`
- `BALANCED_MODE_DEFAULT_OK:True`
- `WORKBENCH_LOD_POLICY_OK:True`
- `PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True`
- `WORKBENCH_SCOPE_BOUNDARY_OK:True`
- `GRAPH_SEARCH_LOCATE_OK:True`
- `GRAPH_SEARCH_SCOPE_FILTER_OK:True`
- `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`
- `COMMAND_PALETTE_GROUPING_OK:True`
- `COMMAND_PALETTE_DISABLED_REASON_OK:True`
- `COMMAND_PALETTE_RECENT_ACTIONS_OK:True`
- `COMMAND_PROJECTION_UNIFIED_OK:True`
- `COMMAND_PALETTE_OK:True`
- `TOOLBAR_DESCRIPTOR_OK:True`
- `CONTEXT_MENU_DESCRIPTOR_OK:True`
- `COMMAND_DISABLED_REASON_OK:True`
- `NODE_TOOLBAR_CONTRIBUTION_OK:True`
- `EDGE_TOOLBAR_CONTRIBUTION_OK:True`
- `TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:True`
- `TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:True`
- `NAVIGATION_HISTORY_OK:True`
- `SCOPE_BREADCRUMB_NAVIGATION_OK:True`
- `FOCUS_RESTORE_OK:True`
- `NAVIGATION_PRODUCTIVITY_PROOF_OK:True`
- `NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True`
- `NAVIGATION_SCOPE_BOUNDARY_OK:True`
- `FIVE_MINUTE_ONBOARDING_OK:True`
- `ONBOARDING_CONFIGURATION_OK:True`
- `AUTHORING_SURFACE_OK:True`
- `COMMAND_SURFACE_OK:True`
- `EXPERIENCE_POLISH_HANDOFF_OK:True`
- `FEATURE_ENHANCEMENT_PROOF_OK:True`
- `AUTHORING_FLOW_PROOF_OK:True`
- `AUTHORING_FLOW_HANDOFF_OK:True`
- `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`
- `EXPERIENCE_SCOPE_BOUNDARY_OK:True`
- `HOST_NATIVE_METRIC:*`

当提供 `--support-bundle <support-bundle-path>` 时，预期 bundle marker：

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`
- `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`
- `GRAPH_VALIDATION_FEEDBACK_OK:True`
- `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`
- `GRAPH_READINESS_STATUS_OK:True`

这份速查只做 summary-only；实际 intake 说明由 `Proof Handoff` 负责。
support bundle 只保留本地证据，不会扩大 support 边界。

下一步 beta intake 文档：

- [Beta Support Bundle](./support-bundle.md)
- [Adoption Feedback Loop](./adoption-feedback.md)

## 如何运行

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

如果你想跑 proof 模式：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

如果你要生成本地 support bundle：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

## Proof Handoff

Proof Handoff 负责实际 intake 说明。

如果你是从 starter recipe 复制过来的，就在这里完成 proof handoff：先跑 `AsterGraph.ConsumerSample.Avalonia -- --proof` 来验证受防守的宿主路线。

如果你要把它写进实际 intake 记录，就运行 `AsterGraph.ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`，并把输出里的 `SUPPORT_BUNDLE_PATH:...` 作为 support-bundle 附件备注，写入受限 intake 记录。

如果 route 不能产出 bundle，就记录 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`。
如果 `CONSUMER_SAMPLE_PARAMETER_OK` 或 `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` 失败，就把失败的 proof-marker 行和 support bundle 的 `parameterSnapshots` 行一起保留在同一条受限 intake 记录里。
如果要排查 graph readiness，就保留同一份 bundle 里的 `readinessStatus`、`validationSummary` 和 `validationFeedback`；这些字段来自 canonical session validation snapshot。

它应当只作为本地证据，不应扩大支持边界。

预期 proof marker：

- `CONSUMER_SAMPLE_HOST_ACTION_OK:True`
- `CONSUMER_SAMPLE_PLUGIN_OK:True`
- `AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True`
- `AUTHORING_SURFACE_METADATA_PROJECTION_OK:True`
- `AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True`
- `AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True`
- `CONSUMER_SAMPLE_PARAMETER_OK:True`
- `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`
- `CONSUMER_SAMPLE_WINDOW_OK:True`
- `CONSUMER_SAMPLE_TRUST_OK:True`
- `COMMAND_SURFACE_OK:True`
- `CAPABILITY_BREADTH_STENCIL_OK:True`
- `STENCIL_GROUPING_OK:True`
- `STENCIL_SEARCH_OK:True`
- `STENCIL_RECENTS_FAVORITES_OK:True`
- `STENCIL_SOURCE_FILTER_OK:True`
- `STENCIL_STATE_PERSISTENCE_OK:True`
- `CAPABILITY_BREADTH_EXPORT_OK:True`
- `EXPORT_PANEL_OK:True`
- `EXPORT_PANEL_SCOPE_OK:True`
- `EXPORT_PANEL_PROGRESS_CANCEL_OK:True`
- `EXPORT_DOCS_ALIGNMENT_OK:True`
- `CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`
- `NODE_TOOLBAR_CONTRIBUTION_OK:True`
- `EDGE_TOOLBAR_CONTRIBUTION_OK:True`
- `TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:True`
- `TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:True`
- `CAPABILITY_BREADTH_OK:True`
- `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`
- `WIDENED_SURFACE_PERFORMANCE_OK:True`
- `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True`
- `CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`
- `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`
- `GRAPH_VALIDATION_FEEDBACK_OK:True`
- `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`
- `GRAPH_READINESS_STATUS_OK:True`
- `GRAPH_SNIPPET_CATALOG_OK:True`
- `GRAPH_SNIPPET_INSERT_OK:True`
- `WORKBENCH_DEFAULTS_OK:True`
- `WORKBENCH_HOST_BUILDER_HANDOFF_OK:True`
- `WORKBENCH_PERFORMANCE_MODE_OK:True`
- `BALANCED_MODE_DEFAULT_OK:True`
- `WORKBENCH_LOD_POLICY_OK:True`
- `PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True`
- `WORKBENCH_SCOPE_BOUNDARY_OK:True`
- `GRAPH_SEARCH_LOCATE_OK:True`
- `GRAPH_SEARCH_SCOPE_FILTER_OK:True`
- `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`
- `COMMAND_PALETTE_GROUPING_OK:True`
- `COMMAND_PALETTE_DISABLED_REASON_OK:True`
- `COMMAND_PALETTE_RECENT_ACTIONS_OK:True`
- `COMMAND_PROJECTION_UNIFIED_OK:True`
- `COMMAND_PALETTE_OK:True`
- `TOOLBAR_DESCRIPTOR_OK:True`
- `CONTEXT_MENU_DESCRIPTOR_OK:True`
- `COMMAND_DISABLED_REASON_OK:True`
- `NODE_TOOLBAR_CONTRIBUTION_OK:True`
- `EDGE_TOOLBAR_CONTRIBUTION_OK:True`
- `TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:True`
- `TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:True`
- `NAVIGATION_HISTORY_OK:True`
- `SCOPE_BREADCRUMB_NAVIGATION_OK:True`
- `FOCUS_RESTORE_OK:True`
- `NAVIGATION_PRODUCTIVITY_PROOF_OK:True`
- `NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True`
- `NAVIGATION_SCOPE_BOUNDARY_OK:True`
- `FIVE_MINUTE_ONBOARDING_OK:True`
- `ONBOARDING_CONFIGURATION_OK:True`
- `HOST_NATIVE_METRIC:startup_ms=...`
- `HOST_NATIVE_METRIC:inspector_projection_ms=...`
- `HOST_NATIVE_METRIC:plugin_scan_ms=...`
- `HOST_NATIVE_METRIC:command_latency_ms=...`
- `HOST_NATIVE_METRIC:stencil_search_ms=...`
- `HOST_NATIVE_METRIC:command_surface_refresh_ms=...`
- `HOST_NATIVE_METRIC:node_tool_projection_ms=...`
- `HOST_NATIVE_METRIC:edge_tool_projection_ms=...`
- `AUTHORING_SURFACE_OK:True`
- `EXPERIENCE_POLISH_HANDOFF_OK:True`
- `FEATURE_ENHANCEMENT_PROOF_OK:True`
- `AUTHORING_FLOW_PROOF_OK:True`
- `AUTHORING_FLOW_HANDOFF_OK:True`
- `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`
- `EXPERIENCE_SCOPE_BOUNDARY_OK:True`
- `CONSUMER_SAMPLE_OK:True`

当提供 `--support-bundle <support-bundle-path>` 时，预期 bundle marker：

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`
- `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`
- `GRAPH_VALIDATION_FEEDBACK_OK:True`
- `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`
- `GRAPH_READINESS_STATUS_OK:True`
- `CONSUMER_SAMPLE_OK:True`

## 什么时候看它

当你想在 `HelloWorld.Avalonia` 之后、跳到完整 `Demo` 之前，先看一个真实宿主集成时，就看 `ConsumerSample.Avalonia`。

如果你需要的是更窄的入口，请用：

- `Starter.Avalonia` = 第一个 hosted 脚手架，也是最小端到端 Avalonia 入口
- `HelloWorld` = 最小 runtime-only 第一跑
- `HelloWorld.Avalonia` = 最小 hosted-UI 第一跑
- `HostSample` = 推荐路线验证用 proof harness
- `PackageSmoke` = 打包消费验证
- `ScaleSmoke` = 规模与状态连续性验证
- `Demo` = 完整展示宿主

## 集成说明

这个样例刻意控制在可复制范围内：

- action rail / command projection：宿主动作在编辑器壳层之外，并且通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 和 `AsterGraphHostedActionFactory.CreateProjection(...)` 复用共享 command descriptor
- plugin trust workflow：把 `GraphEditorPluginDiscoveryOptions`、`AsterGraphEditorOptions.PluginTrustPolicy`、provenance snapshot 和 allowlist 导入/导出放在同一层；插件信任策略保持显式且由宿主管理，通过 discovery snapshot、reason 字符串和 allowlist 导入/导出保持可见，allowlist 决策可以导出/导入而不需要重建宿主 trust-policy 流程。
- trusted plugin proof handoff：把这份样例的 `CONSUMER_SAMPLE_TRUST_OK:True` 和 `AsterGraph.PluginTool validate` 的 `ASTERGRAPH_PLUGIN_VALIDATE_OK:True` 配在一起，再阅读 [插件信任契约 v1](./plugin-trust-contracts.md)，然后才把第三方插件 artifact 当成可加载对象。
- 选中节点参数读写 seam：通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 读选中节点参数，并通过 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 写回
- 插件加载仍是进程内执行，不提供 sandbox 或不受信任代码隔离
- review/audit 节点族、action ids/titles、窗口布局、叙述文本，以及 defended markers 之外的 proof 文案，都是样例自有内容

更细的 v1 manifest 和 trust-policy 合同见 [插件信任契约 v1](./plugin-trust-contracts.md)。

## 可直接照抄的接法

如果你想在自己的宿主里做同等级别的接入，可以按这个顺序抄：

- action rail / command projection：通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 消费共享 command descriptor，并用 `AsterGraphHostedActionFactory.CreateProjection(...)` 组合宿主动作
- plugin trust workflow：把 `GraphEditorPluginDiscoveryOptions`、`AsterGraphEditorOptions.PluginTrustPolicy`、provenance snapshot 和宿主自管 allowlist policy 放在同一层
- trusted plugin proof handoff：评审一条可信插件路径时，把 `CONSUMER_SAMPLE_TRUST_OK:True`、`ASTERGRAPH_PLUGIN_VALIDATE_OK:True` 和 [插件信任契约 v1](./plugin-trust-contracts.md) 放在一起看
- 选中节点参数读写 seam：只通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 读取当前选中节点参数，并只通过 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 写回
- snippet catalog 与插入 seam：把 `consumer.sample.snippet.queue-lane` 和其他 snippet 保持为宿主自管，再通过 `StartConnection(...)` 加 `TryCreateConnectedNodeFromPendingConnection(...)` 插入
- 节点旁路 authoring seam：通过 `IGraphEditorSession.Queries.GetNodeParameterSnapshots(nodeId)` 和 `INodeParameterEditorRegistry` 把节点表面保持在和 inspector 一样的 metadata/validation 合同上
- snippet seam：通过一个小 catalog 暴露宿主自管 snippet，用已有 pending-connection command path 插入，并期待 `GRAPH_SNIPPET_CATALOG_OK:True` 和 `GRAPH_SNIPPET_INSERT_OK:True`
- default workbench seam：当 stock toolbar、command palette、stencil、inspector、mini-map、fragment、diagnostics 和 status chrome 已经足够时，使用 `AsterGraphHostBuilder.UseDefaultWorkbench()`；期待 `WORKBENCH_DEFAULTS_OK:True`、`WORKBENCH_HOST_BUILDER_HANDOFF_OK:True`、`WORKBENCH_PERFORMANCE_MODE_OK:True`、`BALANCED_MODE_DEFAULT_OK:True`、`WORKBENCH_LOD_POLICY_OK:True`、`PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True` 和 `WORKBENCH_SCOPE_BOUNDARY_OK:True`
- graph search seam：从当前 snapshot 搜索 hosted graph，通过已有 selection 和 viewport command 定位节点/连接，并期待 `GRAPH_SEARCH_LOCATE_OK:True`、`GRAPH_SEARCH_SCOPE_FILTER_OK:True` 和 `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`
- command palette productivity：按已有 command descriptor group 对 palette action 分组，展示共享 command route 的 disabled reason，保留有界的内存 recent-action 区域，并证明 toolbar / palette / context-menu 都来自同一套 descriptor；期待 `COMMAND_PALETTE_GROUPING_OK:True`、`COMMAND_PALETTE_DISABLED_REASON_OK:True`、`COMMAND_PALETTE_RECENT_ACTIONS_OK:True`、`COMMAND_PROJECTION_UNIFIED_OK:True`、`COMMAND_PALETTE_OK:True`、`TOOLBAR_DESCRIPTOR_OK:True`、`CONTEXT_MENU_DESCRIPTOR_OK:True` 和 `COMMAND_DISABLED_REASON_OK:True`
- navigation productivity：把 graph search、command palette、back/forward history、scope breadcrumbs 和 focus restore 保持为宿主自管，并继续复用已有 selection/scope/viewport command，期待 `NAVIGATION_PRODUCTIVITY_PROOF_OK:True`、`NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True` 和 `NAVIGATION_SCOPE_BOUNDARY_OK:True`
- proof mode：输出 `AUTHORING_SURFACE_*`、`COMMAND_SURFACE_OK` 和扩展后的 `HOST_NATIVE_METRIC:*`，这样你能和官方 sample 做横向比较，并继续把 `ScaleSmoke` 的 defended large-tier contract 放在视野里
- widened hosted tuning：输出 `WIDENED_SURFACE_PERFORMANCE_OK:True`，并复用 [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)，这样宿主指标和 `Quality` / `Balanced` / `Throughput` projection policy 会继续和 `ScaleSmoke` 绑定在同一条路线里
- capability breadth：把同一条路线和 [Capability Breadth Recipe](./capability-breadth-recipe.md) 配在一起，并从 `AsterGraph.ConsumerSample.Avalonia -- --proof` 输出 `CAPABILITY_BREADTH_*` markers
- onboarding markers：继续守住 `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True`、`CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`、`CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`、`GRAPH_VALIDATION_FEEDBACK_OK:True`、`GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`、`GRAPH_READINESS_STATUS_OK:True`、`FIVE_MINUTE_ONBOARDING_OK:True` 和 `ONBOARDING_CONFIGURATION_OK:True`
- authoring flow markers：把 `AUTHORING_FLOW_PROOF_OK:True`、`AUTHORING_FLOW_HANDOFF_OK:True` 和 `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True` 与 quick-add、insert-on-wire、reconnect editing、edge multiselect、wire slicing proof 行放在一起看
- 最终 handoff markers：把 `EXPERIENCE_POLISH_HANDOFF_OK:True`、`FEATURE_ENHANCEMENT_PROOF_OK:True` 和 `EXPERIENCE_SCOPE_BOUNDARY_OK:True` 放在一起看，让 scope boundary 保持显式
- support bundle：在 proof mode 上额外附带 `--support-bundle`，生成本地 JSON 证据包给 support/feedback 使用
- sample-owned content：review/audit 节点族、action ids/titles 和 proof labels 应该保持在你的 app 内部，不要写成 canonical contract

## 相关文档

- [Quick Start](./quick-start.md)
- [Capability Breadth Recipe](./capability-breadth-recipe.md)
- [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)
- [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
- [Host Integration](./host-integration.md)
- [Sample README](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Alpha Status](./alpha-status.md)
