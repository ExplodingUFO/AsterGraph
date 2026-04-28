# Beta Support Bundle

This is the local evidence contract for beta evaluation and support.
Use the support bundle as the support-bundle attachment note on the bounded intake record when available from the defended hosted proof route.
It does not upload anything and it does not imply telemetry or hosted services.
Use it alongside [Beta Evaluation Path](./evaluation-path.md) when you need the local evidence attachment from the defended hosted proof route.
The intake handoff is covered by `SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True` and stays local evidence only.

## Canonical Producer

Generate the support bundle from `ConsumerSample.Avalonia`, because that is the defended realistic hosted proof on the current beta route.

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

Expected additional proof markers:

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

If the write fails, the proof run emits `SUPPORT_BUNDLE_PERSISTENCE_OK:False` before it stops.

## Local Evidence Only

Copyable local evidence reference:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

Local evidence only means this bundle remains tied to the defended hosted route and does not widen the support boundary. Use the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note on the bounded intake record. If a route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`.
When `CONSUMER_SAMPLE_PARAMETER_OK` or `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` fail, keep the failed proof-marker lines with the bundle's `parameterSnapshots` rows so the bounded intake record can classify `status`, `owner`, and `priority` from the same evidence set.
When graph readiness or connection/parameter validity is the failure area, keep `readinessStatus`, `validationSummary`, and `validationFeedback`; they are serialized from the canonical session validation snapshot.
For screen-reader-ready local evaluation, keep this bundle on the same bounded intake record as the post-ladder `HostSample` lines `HOST_SAMPLE_AUTOMATION_OK:True`, `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`, and `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True`.

## Contract

The support bundle is one local JSON file with these top-level fields:

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
- `graphSummary` — node and connection counts from the evaluated session
- `readinessStatus` — stable `Ready`, `Warnings`, or `Blocked` status from the canonical validation snapshot
- `validationSummary` — total, error, warning, invalid connection, and invalid parameter counts
- `validationFeedback` — canonical validation issue rows with `code`, `severity`, `message`, and `focusTarget`
- `featureDescriptors` — list of available capabilities detected at capture time
- `recentDiagnostics` — recent diagnostic codes for silent-failure triage
- `runtimeNodeOverlays` — host-owned node runtime state snapshots
- `runtimeConnectionOverlays` — host-owned connection runtime payload snapshots
- `runtimeLogs` — recent host-owned runtime log entries

`persistenceStatus` records the bundle write outcome. For the current proof path it is `written`.

`proofLines` should include the full marker set emitted by proof mode: `CONSUMER_SAMPLE_HOST_ACTION_OK:True`, `CONSUMER_SAMPLE_PLUGIN_OK:True`, `AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True`, `AUTHORING_SURFACE_METADATA_PROJECTION_OK:True`, `INSPECTOR_METADATA_POLISH_OK:True`, `INSPECTOR_MIXED_VALUE_OK:True`, `INSPECTOR_VALIDATION_FIX_OK:True`, `SUPPORT_BUNDLE_PARAMETER_EVIDENCE_OK:True`, `AUTHORING_QUICK_ADD_CONNECTED_NODE_OK:True`, `AUTHORING_PORT_FILTERED_NODE_SEARCH_OK:True`, `RUNTIME_OVERLAY_SNAPSHOT_OK:True`, `RUNTIME_OVERLAY_SNAPSHOT_POLISH_OK:True`, `RUNTIME_OVERLAY_SCOPE_FILTER_OK:True`, `RUNTIME_LOG_PANEL_OK:True`, `RUNTIME_LOG_FILTER_OK:True`, `RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`, `RUNTIME_LOG_LOCATE_OK:True`, `RUNTIME_LOG_EXPORT_OK:True`, `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True`, `GRAPH_SEARCH_LOCATE_OK:True`, `GRAPH_SEARCH_SCOPE_FILTER_OK:True`, `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`, `COMMAND_PALETTE_GROUPING_OK:True`, `COMMAND_PALETTE_DISABLED_REASON_OK:True`, `COMMAND_PALETTE_RECENT_ACTIONS_OK:True`, `NAVIGATION_HISTORY_OK:True`, `SCOPE_BREADCRUMB_NAVIGATION_OK:True`, `FOCUS_RESTORE_OK:True`, `NAVIGATION_PRODUCTIVITY_PROOF_OK:True`, `NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True`, `NAVIGATION_SCOPE_BOUNDARY_OK:True`, `LAYOUT_PROVIDER_SEAM_OK:True`, `LAYOUT_PREVIEW_APPLY_CANCEL_OK:True`, `LAYOUT_UNDO_TRANSACTION_OK:True`, `READABILITY_FOCUS_SUBGRAPH_OK:True`, `READABILITY_ROUTE_CLEANUP_OK:True`, `READABILITY_ALIGNMENT_HELPERS_OK:True`, `PLUGIN_LOCAL_GALLERY_OK:True`, `PLUGIN_TRUST_EVIDENCE_PANEL_OK:True`, `PLUGIN_ALLOWLIST_ROUNDTRIP_OK:True`, `PLUGIN_SAMPLE_PACK_OK:True`, `PLUGIN_SAMPLE_NODE_DEFINITIONS_OK:True`, `PLUGIN_SAMPLE_PARAMETER_METADATA_OK:True`, `AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True`, `AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True`, `CONSUMER_SAMPLE_PARAMETER_OK:True`, `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`, `CONSUMER_SAMPLE_WINDOW_OK:True`, `CONSUMER_SAMPLE_TRUST_OK:True`, `COMMAND_SURFACE_OK:True`, `CAPABILITY_BREADTH_STENCIL_OK:True`, `CAPABILITY_BREADTH_EXPORT_OK:True`, `CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`, `CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`, `CAPABILITY_BREADTH_OK:True`, `HOSTED_ACCESSIBILITY_BASELINE_OK:True`, `HOSTED_ACCESSIBILITY_FOCUS_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_OK:True`, `WIDENED_SURFACE_PERFORMANCE_OK:True`, `GRAPH_VALIDATION_FEEDBACK_OK:True`, `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`, `GRAPH_READINESS_STATUS_OK:True`, `GRAPH_SNIPPET_CATALOG_OK:True`, `GRAPH_SNIPPET_INSERT_OK:True`, `HOST_NATIVE_METRIC:startup_ms=...`, `HOST_NATIVE_METRIC:inspector_projection_ms=...`, `HOST_NATIVE_METRIC:plugin_scan_ms=...`, `HOST_NATIVE_METRIC:command_latency_ms=...`, `HOST_NATIVE_METRIC:stencil_search_ms=...`, `HOST_NATIVE_METRIC:command_surface_refresh_ms=...`, `HOST_NATIVE_METRIC:node_tool_projection_ms=...`, `HOST_NATIVE_METRIC:edge_tool_projection_ms=...`, `AUTHORING_SURFACE_OK:True`, `EXPERIENCE_POLISH_HANDOFF_OK:True`, `FEATURE_ENHANCEMENT_PROOF_OK:True`, `AUTHORING_FLOW_PROOF_OK:True`, `AUTHORING_FLOW_HANDOFF_OK:True`, `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`, `EXPERIENCE_SCOPE_BOUNDARY_OK:True`, and `CONSUMER_SAMPLE_OK:True`.

Onboarding proof lines should also include `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True`, `CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`, `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`, `GRAPH_VALIDATION_FEEDBACK_OK:True`, `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`, `GRAPH_READINESS_STATUS_OK:True`, `FIVE_MINUTE_ONBOARDING_OK:True`, and `ONBOARDING_CONFIGURATION_OK:True`.

`parameterSnapshots` captures review-node parameter projection plus bounded mixed-value and validation-fix evidence in one structure. Each snapshot records `key`, `valueType`, `editorKind`, `currentValue`, `defaultValue`, `hasMixedValues`, `canEdit`, `isValid`, `validationMessage`, `readOnlyReason`, `helpText`, `groupName`, `placeholderText`, `isAdvanced`, `valueState`, `valueDisplayText`, `usesMultilineTextInput`, `isCodeLikeText`, `supportsEnumSearch`, `numberSliderHint`, `canApplyValidationFix`, `validationFixActionLabel`, `allowedOptions`, `minimum`, and `maximum` when present.
Use those rows with `status`, `owner`, `priority`, `review-script`, and `slug` when you classify the report.

`readinessStatus` is `Ready` when the validation snapshot has no issues, `Warnings` when it has only non-blocking warnings, and `Blocked` when it has blocking errors.
`validationSummary` records `totalIssueCount`, `errorCount`, `warningCount`, `invalidConnectionCount`, and `invalidParameterCount`.
`validationFeedback` contains one row per canonical validation issue. Each row records `code`, `severity`, `message`, and `focusTarget`; `focusTarget` includes a stable `kind` plus `nodeId`, `connectionId`, `endpointId`, and `parameterKey` when the canonical issue has those IDs. An empty `validationFeedback` array is valid for a ready graph.

`runtimeNodeOverlays`, `runtimeConnectionOverlays`, and `runtimeLogs` capture host-owned runtime feedback. They are display/support evidence only and do not imply a core graph execution engine.

Runtime feedback release handoff should also cross-check Demo proof markers `AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`, `AI_PIPELINE_PAYLOAD_PREVIEW_OK:True`, and `AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True` alongside the ConsumerSample runtime log markers. These markers prove host-owned overlay/debug evidence, not a workflow scripting UI, marketplace, sandbox, WPF parity, or GA claim.

Graph search proof remains hosted and snapshot-driven: `GRAPH_SEARCH_LOCATE_OK:True`, `GRAPH_SEARCH_SCOPE_FILTER_OK:True`, and `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True` prove search/locate evidence without a background graph index service or command macro engine.
Command palette proof remains on the shared command/session route: `COMMAND_PALETTE_GROUPING_OK:True`, `COMMAND_PALETTE_DISABLED_REASON_OK:True`, and `COMMAND_PALETTE_RECENT_ACTIONS_OK:True` prove grouped palette discovery, disabled reason visibility, and bounded recent actions without a command macro engine or scripting UI.
Navigation proof remains host-owned: `NAVIGATION_HISTORY_OK:True`, `SCOPE_BREADCRUMB_NAVIGATION_OK:True`, `FOCUS_RESTORE_OK:True`, `NAVIGATION_PRODUCTIVITY_PROOF_OK:True`, `NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True`, and `NAVIGATION_SCOPE_BOUNDARY_OK:True` prove search, palette, back/forward, breadcrumb, and focus-restore evidence without a new runtime navigation API.

`environment` records the local runtime and OS details used for the run.

`reproduction` records the friction note plus:

- the captured command line
- the working directory
- an optional human note

## When To Use It

- attach it when reporting beta support issues
- attach it to the bounded intake record when filing adopter feedback after reaching the realistic hosted proof
- regenerate it after you change package version, route, or environment
- treat it as intake evidence for the [Project Status](./project-status.md) readiness gate, not as automatic proof that the support boundary should widen
- if a route cannot produce one yet, keep the same route/version/proof markers/friction record and note that no support bundle was available

## Related Docs

- [Beta Evaluation Path](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md)
