# AsterGraph.ConsumerSample.Avalonia

This is the medium hosted-UI sample on the canonical session/runtime route, after `AsterGraph.Starter.Avalonia` and `AsterGraph.HelloWorld.Avalonia`, and before `AsterGraph.Demo`.
It opens the `Content Review Release Lane` scenario graph and shows a host-owned action rail, plugin trust workflow, support-bundle proof path, and the selected-node parameter read/write seam without implying a second editor model, a sandbox, or a broader plugin ecosystem.
It also includes a host-owned snippet catalog for inserting one copyable review queue lane through the existing pending-connection command path.
For plugin-capable evaluators, this is the defended hosted trust hop. Read [Plugin Manifest and Trust Policy Contract v1](../../docs/en/plugin-trust-contracts.md) and [Plugin And Custom Node Recipe](../../docs/en/plugin-recipe.md) before treating the route as complete.

For the inspector metadata recipe, pair this sample with [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md). The sample stays focused on the host-owned seams and the shipped inspector surface; it does not own the metadata vocabulary. The canonical recipe carries the full `defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, and read-only vocabulary.
For copyable custom node, port, parameter, and edge presentation on the same hosted route, pair it with [Authoring Surface Recipe](../../docs/en/authoring-surface-recipe.md).
For searchable grouped stencil, SVG/PNG/JPEG export breadth, and shared node or edge quick tools on the same hosted route, pair it with [Capability Breadth Recipe](../../docs/en/capability-breadth-recipe.md).
For one copyable hosted tuning handoff that keeps those widened surfaces tied to defended `ScaleSmoke` budgets, pair it with [Widened Surface Performance Recipe](../../docs/en/widened-surface-performance-recipe.md).
For one copyable hosted keyboard, focus, and accessibility handoff on the same route, pair it with [Hosted Accessibility Recipe](../../docs/en/hosted-accessibility-recipe.md).

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

Five-minute hosted handoff:

1. Run `tools/AsterGraph.Starter.Avalonia` to copy the document/catalog/editor/view composition.
2. Run `tools/AsterGraph.HelloWorld.Avalonia` to confirm the smallest hosted shell.
3. Run this sample to inspect the review-to-queue scenario graph, host actions, selected-parameter editor, and trusted audit plugin.
4. Run proof mode with `--support-bundle` before copying the same seams into your own host.

## Run

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

For proof mode:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

For a local support bundle:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

Validate the defended route here with `AsterGraph.ConsumerSample.Avalonia -- --proof`. For reviewable local evidence, run the bundle-producing command above and reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note. If the route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`. The support bundle stays local evidence only.
The bundle also serializes canonical graph readiness evidence from the session validation snapshot: `readinessStatus`, `validationSummary`, and `validationFeedback` with focus targets.

Expected proof markers:

- `CONSUMER_SAMPLE_HOST_ACTION_OK:True`
- `CONSUMER_SAMPLE_PLUGIN_OK:True`
- `AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True`
- `AUTHORING_SURFACE_METADATA_PROJECTION_OK:True`
- `INSPECTOR_METADATA_POLISH_OK:True`
- `INSPECTOR_MIXED_VALUE_OK:True`
- `INSPECTOR_VALIDATION_FIX_OK:True`
- `SUPPORT_BUNDLE_PARAMETER_EVIDENCE_OK:True`
- `AUTHORING_QUICK_ADD_CONNECTED_NODE_OK:True`
- `AUTHORING_PORT_FILTERED_NODE_SEARCH_OK:True`
- `AUTHORING_DROP_NODE_ON_EDGE_OK:True`
- `AUTHORING_EDGE_SPLIT_COMPATIBILITY_OK:True`
- `AUTHORING_EDGE_SPLIT_UNDO_OK:True`
- `AUTHORING_DELETE_AND_RECONNECT_OK:True`
- `AUTHORING_DETACH_NODE_OK:True`
- `AUTHORING_RECONNECT_CONFLICT_REPORT_OK:True`
- `AUTHORING_EDGE_MULTISELECT_OK:True`
- `AUTHORING_WIRE_SLICE_OK:True`
- `AUTHORING_SELECTED_NODE_EDGE_HIGHLIGHT_OK:True`
- `RUNTIME_OVERLAY_SNAPSHOT_OK:True`
- `RUNTIME_LOG_PANEL_OK:True`
- `RUNTIME_LOG_FILTER_OK:True`
- `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True`
- `LAYOUT_PROVIDER_SEAM_OK:True`
- `LAYOUT_PREVIEW_APPLY_CANCEL_OK:True`
- `LAYOUT_UNDO_TRANSACTION_OK:True`
- `READABILITY_FOCUS_SUBGRAPH_OK:True`
- `READABILITY_ROUTE_CLEANUP_OK:True`
- `READABILITY_ALIGNMENT_HELPERS_OK:True`
- `PLUGIN_LOCAL_GALLERY_OK:True`
- `PLUGIN_TRUST_EVIDENCE_PANEL_OK:True`
- `PLUGIN_ALLOWLIST_ROUNDTRIP_OK:True`
- `PLUGIN_SAMPLE_PACK_OK:True`
- `PLUGIN_SAMPLE_NODE_DEFINITIONS_OK:True`
- `PLUGIN_SAMPLE_PARAMETER_METADATA_OK:True`
- `AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True`
- `AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True`
- `CONSUMER_SAMPLE_PARAMETER_OK:True`
- `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`
- `CONSUMER_SAMPLE_WINDOW_OK:True`
- `CONSUMER_SAMPLE_TRUST_OK:True`
- `COMMAND_SURFACE_OK:True`
- `CAPABILITY_BREADTH_STENCIL_OK:True`
- `CAPABILITY_BREADTH_EXPORT_OK:True`
- `CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`
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
- `CONSUMER_SAMPLE_OK:True`

Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`
- `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`
- `GRAPH_VALIDATION_FEEDBACK_OK:True`
- `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`
- `GRAPH_READINESS_STATUS_OK:True`
- `CONSUMER_SAMPLE_OK:True`

## Trust and proof quick reference

Copyable trust and proof reference:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

The support bundle stays local evidence only and does not widen the support boundary.

Next beta intake links:

- [Beta Support Bundle](../../docs/en/support-bundle.md)
- [Adoption Feedback Loop](../../docs/en/adoption-feedback.md)
- [Public Launch Checklist](../../docs/en/public-launch-checklist.md)

## Host Seam Example

Use this sample to copy the host-owned seams, not the sample-specific presentation layer. This is the defended beta route for copying host-owned seams into your own host. Copy the host-owned seams in this order:

### Copy These Host-Owned Seams

- action rail / command projection: `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, and the host allowlist import/export path
- selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads the selected node parameters, and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes them back
- snippet catalog and insertion seam: `ConsumerSampleHost.SnippetCatalog` stays sample-owned, while `ConsumerSampleHost.TryInsertSnippet(...)` uses `StartConnection(...)` plus `TryCreateConnectedNodeFromPendingConnection(...)` instead of adding a runtime snippet abstraction

### Route Boundaries To Keep

| Route | Copy from this sample | Do not copy |
| ----- | --------------------- | ----------- |
| Hosted UI | `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` composition | demo-only shell state or showcase panels |
| Runtime-only | the same document/catalog definitions, then use `AsterGraphEditorFactory.CreateSession(...)` in your own UI | Avalonia window layout |
| Plugin | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)`, `PluginTrustPolicy`, provenance, and allowlist import/export | sample plugin id, audit node family, or trust text |
| Migration | retained `GraphEditorViewModel` / `GraphEditorView` only when moving an existing host in batches | new primary host code on retained surfaces |

### Copyable Parameter/Metadata Path

- Define metadata in [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md) first with `defaultValue`, `editorKind`, `constraints`, and `groupName`.
- Project and write selected-node values in this sample through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`.
- Validate evidence with proof mode plus a support bundle; compare `parameterSnapshots` with `CONSUMER_SAMPLE_PARAMETER_OK:True` and `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`, then use `readinessStatus`, `validationSummary`, and `validationFeedback` for graph readiness triage.

### Copyable Authoring Surface Handoff

- Keep definitions and metadata in `NodeDefinition`, then project inspector state from `GetSelectedNodeParameterSnapshots()`.
- Project node-side editor state from `GetNodeParameterSnapshots(nodeId)` so `NodeParameterEditorHost` and `INodeParameterEditorRegistry` reuse the same validation-aware parameter contract.
- Keep writes on the shared session command path through `TrySetSelectedNodeParameterValue(...)` or `TrySetNodeParameterValue(...)`.
- Project host actions from `GetCommandDescriptors()` and close the route with `AsterGraph.ConsumerSample.Avalonia -- --proof`, expecting `AUTHORING_SURFACE_OK:True`.
- Keep snippets host-owned: expose sample snippet ids through `ConsumerSampleHost.SnippetCatalog`, insert them through existing session commands, and expect `GRAPH_SNIPPET_CATALOG_OK:True` plus `GRAPH_SNIPPET_INSERT_OK:True`.

### Copyable Capability Breadth Handoff

- Keep searchable grouped stencil on the stock hosted view and drive it from `IGraphEditorSession.Queries.GetNodeTemplateSnapshots()`.
- Keep scene export on `IGraphEditorSession.Commands.TryExportSceneAsSvg(...)` plus `IGraphEditorSession.Commands.TryExportSceneAsImage(...)` for `GraphEditorSceneImageExportFormat.Png` and `GraphEditorSceneImageExportFormat.Jpeg`.
- Keep node and edge quick tools on the shared tool route through `IGraphEditorSession.Queries.GetToolDescriptors(...)` and `AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(...)`.
- Close the route with [Capability Breadth Recipe](../../docs/en/capability-breadth-recipe.md) and `AsterGraph.ConsumerSample.Avalonia -- --proof`, expecting `CAPABILITY_BREADTH_OK:True`.

### Copyable Widened Surface Performance Handoff

- Keep widened hosted metrics on `AsterGraph.ConsumerSample.Avalonia -- --proof` through `HOST_NATIVE_METRIC:stencil_search_ms`, `HOST_NATIVE_METRIC:command_surface_refresh_ms`, `HOST_NATIVE_METRIC:node_tool_projection_ms`, and `HOST_NATIVE_METRIC:edge_tool_projection_ms`, and expect `WIDENED_SURFACE_PERFORMANCE_OK:True`.
- Keep defended `large` authoring and export budgets on `ScaleSmoke` through `SCALE_AUTHORING_BUDGET_OK:large:True:none` and `SCALE_EXPORT_BUDGET_OK:large:True:none`.
- Close the tuning handoff with [Widened Surface Performance Recipe](../../docs/en/widened-surface-performance-recipe.md) instead of inventing a new proof lane.

### Copyable Hosted Accessibility Handoff

- Keep baseline automation names on `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, and the stock search surfaces, then expect `HOSTED_ACCESSIBILITY_BASELINE_OK:True`.
- Keep command-palette keyboard flow on the shared hosted route so focus returns to the host surface that opened it, then expect `HOSTED_ACCESSIBILITY_FOCUS_OK:True`.
- Keep hosted automation navigation and authoring diagnostics on that same proof run, expect `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, and `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`, then pair the same bounded intake record with post-ladder `HostSample` lines `HOST_SAMPLE_AUTOMATION_OK:True`, `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`, and `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True` for screen-reader-ready local evaluation.
- Keep header, palette, node-tool, and edge-tool names projected from the same shared action descriptors, then expect `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`.
- Keep selected-node parameter metadata and connection text editors on the same hosted authoring route, then close the handoff with [Hosted Accessibility Recipe](../../docs/en/hosted-accessibility-recipe.md) and `HOSTED_ACCESSIBILITY_OK:True`.

### Replace These Sample-Owned Details

Replaceable sample-owned details are the review/audit node family, the sample action ids and titles, the window layout and narrative text, and any proof labels or copy beyond the defended public markers.

- sample-owned details are the review/audit node family
- sample action ids and titles
- sample snippet ids and catalog entries
- window layout and narrative text
- proof labels or copy beyond the defended public markers

## Where It Fits

- `Starter.Avalonia` = first hosted scaffold and smallest end-to-end Avalonia entry
- `HelloWorld` = smallest runtime-only first run
- `HelloWorld.Avalonia` = smallest hosted-UI first run
- `ConsumerSample.Avalonia` = defended hosted trust hop for plugin-capable evaluators
- [Plugin Manifest and Trust Policy Contract v1](../../docs/en/plugin-trust-contracts.md) = host-owned trust-policy contract for this defended hop
- [Plugin And Custom Node Recipe](../../docs/en/plugin-recipe.md) = copyable plugin path to pair with the defended trust hop
- [Quick Start](../../docs/en/quick-start.md)
- [Beta Support Bundle](../../docs/en/support-bundle.md)
- [Host Integration](../../docs/en/host-integration.md)
- [Demo Guide](../../docs/en/demo-guide.md)
