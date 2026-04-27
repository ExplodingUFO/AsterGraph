# Beta Support Bundle

这份文档定义的是 public beta 的本地证据合同。
提交 beta 反馈时，可用时把 support bundle 作为附件使用。
它不会上传任何东西，也不代表遥测、云服务或托管后端。
如果你需要来自受防守 hosted proof route 的本地证据附件，就把它和 [公开 Beta 评估路径](./evaluation-path.md) 一起看。support bundle 的附件备注要写进受限 intake 记录里。

## Canonical 生成入口

当前推荐从 `ConsumerSample.Avalonia` 生成 support bundle，因为它是这条 beta 路线里已经防守住的真实宿主 proof。

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

额外会输出两条 proof marker：

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

如果写入失败，proof 运行会先输出 `SUPPORT_BUNDLE_PERSISTENCE_OK:False`，然后再停止。

## 仅限本地证据

可复制的本地证据参考：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

这个证据包只保留本地证据，仍然绑定在受防守的 hosted route 上，不会扩大 support 边界。把 proof 输出里的 `SUPPORT_BUNDLE_PATH:...` 这一行作为受限 intake 记录里的附件备注；如果某条 route 不能产出 bundle，就记录 `NO_SUPPORT_BUNDLE:route-cannot-produce-one`。
当 `CONSUMER_SAMPLE_PARAMETER_OK` 或 `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` 失败时，把失败的 proof-marker 行和 bundle 的 `parameterSnapshots` 行一起保留，这样同一套证据就能支持 `status`、`owner` 和 `priority` 分类。
如果你在做 screen-reader-ready 的本地评估，就把这份 bundle 和 route ladder 之后 `HostSample` 输出的 `HOST_SAMPLE_AUTOMATION_OK:True`、`HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`、`HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True` 一起放在同一条受限 intake 记录里。

## 合同字段

support bundle 是一个本地 JSON 文件，顶层字段固定为：

- `schemaVersion`
- `packageVersion`
- `publicTag`
- `route`
- `generatedAtUtc`
- `persistenceStatus`
- `proofLines`
- `parameterSnapshots`
- `environment`
- `reproduction`
- `graphSummary` — 本次会话里的节点数和连接数
- `featureDescriptors` — 捕获时可用的 capability 列表
- `recentDiagnostics` — 用于排查静默失败的近期 diagnostic code
- `runtimeNodeOverlays` — 宿主持有的节点运行态快照
- `runtimeConnectionOverlays` — 宿主持有的连接 payload 快照
- `runtimeLogs` — 宿主持有的近期运行日志

`persistenceStatus` 记录 bundle 写入结果。当前 proof 路线里它是 `written`。

`proofLines` 里应该包含 proof mode 已经输出的完整 marker 集：`CONSUMER_SAMPLE_HOST_ACTION_OK:True`、`CONSUMER_SAMPLE_PLUGIN_OK:True`、`AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True`、`AUTHORING_SURFACE_METADATA_PROJECTION_OK:True`、`INSPECTOR_METADATA_POLISH_OK:True`、`INSPECTOR_MIXED_VALUE_OK:True`、`INSPECTOR_VALIDATION_FIX_OK:True`、`SUPPORT_BUNDLE_PARAMETER_EVIDENCE_OK:True`、`AUTHORING_QUICK_ADD_CONNECTED_NODE_OK:True`、`AUTHORING_PORT_FILTERED_NODE_SEARCH_OK:True`、`RUNTIME_OVERLAY_SNAPSHOT_OK:True`、`RUNTIME_LOG_PANEL_OK:True`、`RUNTIME_LOG_FILTER_OK:True`、`RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True`、`LAYOUT_PROVIDER_SEAM_OK:True`、`LAYOUT_PREVIEW_APPLY_CANCEL_OK:True`、`LAYOUT_UNDO_TRANSACTION_OK:True`、`READABILITY_FOCUS_SUBGRAPH_OK:True`、`READABILITY_ROUTE_CLEANUP_OK:True`、`READABILITY_ALIGNMENT_HELPERS_OK:True`、`AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True`、`AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True`、`CONSUMER_SAMPLE_PARAMETER_OK:True`、`CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`、`CONSUMER_SAMPLE_WINDOW_OK:True`、`CONSUMER_SAMPLE_TRUST_OK:True`、`COMMAND_SURFACE_OK:True`、`CAPABILITY_BREADTH_STENCIL_OK:True`、`CAPABILITY_BREADTH_EXPORT_OK:True`、`CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`、`CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`、`CAPABILITY_BREADTH_OK:True`、`HOSTED_ACCESSIBILITY_BASELINE_OK:True`、`HOSTED_ACCESSIBILITY_FOCUS_OK:True`、`HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`、`HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_OK:True`、`WIDENED_SURFACE_PERFORMANCE_OK:True`、`HOST_NATIVE_METRIC:startup_ms=...`、`HOST_NATIVE_METRIC:inspector_projection_ms=...`、`HOST_NATIVE_METRIC:plugin_scan_ms=...`、`HOST_NATIVE_METRIC:command_latency_ms=...`、`HOST_NATIVE_METRIC:stencil_search_ms=...`、`HOST_NATIVE_METRIC:command_surface_refresh_ms=...`、`HOST_NATIVE_METRIC:node_tool_projection_ms=...`、`HOST_NATIVE_METRIC:edge_tool_projection_ms=...` 这些行，以及 `AUTHORING_SURFACE_OK:True` 和 `CONSUMER_SAMPLE_OK:True`。

onboarding proof 行还应该包含 `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True`、`CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`、`CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`、`FIVE_MINUTE_ONBOARDING_OK:True` 和 `ONBOARDING_CONFIGURATION_OK:True`。

`parameterSnapshots` 用一份受限结构记录 review 节点参数投影，以及 mixed value 和 validation fix 的证据。每条 snapshot 会在存在时记录 `key`、`valueType`、`editorKind`、`currentValue`、`defaultValue`、`hasMixedValues`、`canEdit`、`isValid`、`validationMessage`、`readOnlyReason`、`helpText`、`groupName`、`placeholderText`、`isAdvanced`、`valueState`、`valueDisplayText`、`usesMultilineTextInput`、`isCodeLikeText`、`supportsEnumSearch`、`numberSliderHint`、`canApplyValidationFix`、`validationFixActionLabel`、`allowedOptions`、`minimum` 和 `maximum`。
分类报告时就用这些行来判断 `status`、`owner`、`priority`、`review-script` 和 `slug`。

`runtimeNodeOverlays`、`runtimeConnectionOverlays` 和 `runtimeLogs` 记录宿主持有的运行反馈。它们只作为展示和 support evidence，不代表 core 内置了图执行引擎。

`environment` 记录本次运行所在的 runtime 和操作系统信息。

`reproduction` 记录摩擦说明和：

- 捕获到的命令行
- 工作目录
- 可选的人工备注

## 什么时候用

- 提 beta support issue 时附上它
- 走到真实宿主 proof 之后提交 adopter feedback 时，把它作为受限 intake 记录的一部分附上
- 包版本、路线或环境变化后重新生成
- 把它当成 [Project Status](./project-status.md) 就绪门禁的反馈证据，而不是自动扩大支持边界的证明
- 如果当前 route 还生成不了，就继续保留同一条 route/version/proof 标记/摩擦记录，并注明 support bundle 暂时不可用

## 相关文档

- [公开 Beta 评估路径](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md)
