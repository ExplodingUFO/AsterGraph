# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` is the medium hosted-UI sample on the canonical session/runtime route, after the starter scaffold and the smallest `HelloWorld.Avalonia` route, and before the full `AsterGraph.Demo` showcase host. It opens the `Content Review Release Lane` scenario graph by default.
For trust-policy review and local evidence, pair this route with [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md) and [Beta Support Bundle](./support-bundle.md).

It is the host seam example for these host-owned seams:

- action rail / command projection
- plugin trust workflow
- selected-node parameter read/write seam
- snippet catalog and connected-node insertion seam

For the inspector metadata recipe, pair this route with [Authoring Inspector Recipe](./authoring-inspector-recipe.md). This sample stays focused on the host-owned seams and the shipped inspector surface; it does not own the metadata vocabulary. The canonical recipe carries the full `defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, and read-only vocabulary.
For copyable custom node, port, and edge presentation on the same route, pair it with [Authoring Surface Recipe](./authoring-surface-recipe.md).
For searchable grouped stencil, SVG/PNG/JPEG export breadth, and shared node or edge quick tools on the same route, pair it with [Capability Breadth Recipe](./capability-breadth-recipe.md).
For one copyable hosted tuning handoff that keeps those widened surfaces tied to defended `ScaleSmoke` budgets, pair it with [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md).
For one copyable hosted keyboard, focus, and accessibility handoff on the same route, pair it with [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md).

This is the defended beta route for copying host-owned seams into your own host. Keep action projection, trust workflow, and the selected-node parameter read/write seam host-owned; copy only the sample-owned presentation.

It stays on the canonical session/runtime model only. It does not introduce a second editor model, a sandbox, or a broader plugin ecosystem.
Its final experience handoff markers summarize existing proof lines only: `EXPERIENCE_POLISH_HANDOFF_OK:True`, `FEATURE_ENHANCEMENT_PROOF_OK:True`, and `EXPERIENCE_SCOPE_BOUNDARY_OK:True`.
Its authoring flow handoff summarizes quick-add connected nodes, insert-on-wire, reconnect editing, edge multiselect, and wire slicing on the existing session command path: `AUTHORING_FLOW_PROOF_OK:True`, `AUTHORING_FLOW_HANDOFF_OK:True`, and `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.
`HostSample` is the post-ladder proof harness.

## What It Proves

This sample keeps one realistic host window without turning into a full showcase shell. It demonstrates:

- one host-owned action rail projected from shared command descriptors through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- one host-defined node family that is intentionally sample-owned and replaceable
- one plugin-contributed command flowing through the same action path instead of a sample-only menu placeholder
- one selected-node parameter read/write seam through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`
- one host-owned snippet catalog that inserts the sample review queue lane through `StartConnection(...)` and `TryCreateConnectedNodeFromPendingConnection(...)`
- one trusted plugin registration with visible provenance, trust reasons, and allowlist import or export
- one support-bundle proof path with onboarding markers for the scenario graph, host-owned actions, canonical graph readiness evidence, support-bundle payload readiness, and five-minute handoff health
- one authoring flow proof handoff that keeps quick-add, insert-on-wire, reconnect editing, edge multiselect, and wire slicing on the existing session command path
- one final handoff marker set that ties UX polish, feature enhancement proof, and scope-boundary evidence together without adding runtime APIs, marketplace behavior, sandboxing, or WPF parity
- one runtime feedback proof handoff with `RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`, `RUNTIME_LOG_LOCATE_OK:True`, and `RUNTIME_LOG_EXPORT_OK:True`, paired with Demo's `AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`, `AI_PIPELINE_PAYLOAD_PREVIEW_OK:True`, and `AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True`; these are host-owned debug/support markers, not an execution engine, workflow scripting UI, marketplace, sandbox, WPF parity, or GA claim
- one host-owned navigation proof handoff with `NAVIGATION_HISTORY_OK:True`, `SCOPE_BREADCRUMB_NAVIGATION_OK:True`, `FOCUS_RESTORE_OK:True`, `NAVIGATION_PRODUCTIVITY_PROOF_OK:True`, `NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True`, and `NAVIGATION_SCOPE_BOUNDARY_OK:True` without introducing a command macro engine, background graph index, WPF parity, or a new runtime navigation API
- the shipped Avalonia editor surface on the factory-based hosted-UI route

## Copy These Host-Owned Seams

- action rail / command projection: keep the host actions outside the editor shell and project shared descriptors through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: keep `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and an explicit host-owned allowlist policy together
- selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads the selected node parameters, and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes them back
- snippet catalog and connected-node insertion seam: keep snippet ids and template content in the host, then insert through `StartConnection(...)` plus `TryCreateConnectedNodeFromPendingConnection(...)` so snippets do not become a runtime abstraction

## Route Boundaries To Keep

| Route | Copy from this sample | Do not copy |
| ----- | --------------------- | ----------- |
| Hosted UI | `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` composition | demo-only shell state or showcase panels |
| Runtime-only | the same document/catalog definitions, then use `AsterGraphEditorFactory.CreateSession(...)` in your own UI | Avalonia window layout |
| Plugin | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)`, `PluginTrustPolicy`, provenance, and allowlist import/export | sample plugin id, audit node family, or trust text |
| Migration | retained `GraphEditorViewModel` / `GraphEditorView` only when moving an existing host in batches | new primary host code on retained surfaces |

## Replace These Sample-Owned Details

Keep these sample-owned details local to your app:

- review/audit node family
- action ids and titles
- snippet ids and catalog entries
- window layout and narrative text
- proof labels beyond the defended markers

## Host-Owned Parameter And Metadata Copy Map

Copy from each bounded source for the part it owns:

- Copy from this sample: action rail / command projection, plugin trust workflow, and selected-node parameter read/write seam.
- Copy from [Authoring Inspector Recipe](./authoring-inspector-recipe.md): the definition-driven metadata vocabulary (`defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, `constraints.IsReadOnly`) and stock inspector field grouping.
- Keep local: review/audit node family, action ids/titles, window layout and narrative text, and proof labels beyond the defended markers.

Copy the path in this order:

- Define metadata in [Authoring Inspector Recipe](./authoring-inspector-recipe.md) first with `defaultValue`, `editorKind`, `constraints`, and `groupName`.
- Project and write selected-node values in this sample through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`.
- Project inspector state through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()`.
- Project node-side editor state through `IGraphEditorSession.Queries.GetNodeParameterSnapshots(nodeId)` and `INodeParameterEditorRegistry`.
- Write values back through `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` or `IGraphEditorSession.Commands.TrySetNodeParameterValue(...)` so validation stays on the shared session command path.
- Project host actions from `session.Queries.GetCommandDescriptors()` and the shared host-action route, then validate evidence with proof mode plus a support bundle; compare `parameterSnapshots` with `CONSUMER_SAMPLE_PARAMETER_OK:True`, `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`, and `AUTHORING_SURFACE_OK:True`.
- Validate evidence with proof mode plus a support bundle; compare `parameterSnapshots` with `CONSUMER_SAMPLE_PARAMETER_OK:True`, `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`, and `AUTHORING_SURFACE_OK:True`.

Consumer Sample proves the seam split; it does not own the metadata vocabulary. Authoring Inspector Recipe is the sole owner of the metadata vocabulary.

## Copyable Widened Surface Performance Handoff

- Keep widened hosted metrics on `AsterGraph.ConsumerSample.Avalonia -- --proof` through `HOST_NATIVE_METRIC:stencil_search_ms`, `HOST_NATIVE_METRIC:command_surface_refresh_ms`, `HOST_NATIVE_METRIC:node_tool_projection_ms`, and `HOST_NATIVE_METRIC:edge_tool_projection_ms`, and expect `WIDENED_SURFACE_PERFORMANCE_OK:True`.
- Keep defended `large` authoring and export budgets on `ScaleSmoke` through `SCALE_AUTHORING_BUDGET_OK:large:True:none` and `SCALE_EXPORT_BUDGET_OK:large:True:none`.
- Reuse [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) when you want one copyable tuning handoff instead of a new hosted proof lane.

## Copyable Hosted Accessibility Handoff

- Keep baseline automation names on `GraphEditorView`, `NodeCanvas`, `GraphInspectorView`, and the stock search surfaces, then expect `HOSTED_ACCESSIBILITY_BASELINE_OK:True`.
- Keep command-palette keyboard flow on the shared hosted route so focus returns to the host surface that opened it, then expect `HOSTED_ACCESSIBILITY_FOCUS_OK:True`.
- Keep hosted automation navigation and authoring diagnostics on the same proof run, then expect `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, and `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`.
- Keep header, palette, node-tool, and edge-tool names projected from the same shared action descriptors, then expect `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`.
- Keep selected-node parameter metadata and connection text editors on the same hosted authoring route, then close the handoff with [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md) and `HOSTED_ACCESSIBILITY_OK:True`.
- For one screen-reader-ready local evaluation path, keep `ConsumerSample.Avalonia -- --proof`, the support-bundle attachment note, and the post-ladder `HostSample` proof lines on the same bounded intake record.

## Trust and proof quick reference

Copyable trust and proof reference:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

Expected proof markers:

- `CONSUMER_SAMPLE_TRUST_OK:True`
- `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`
- `AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True`
- `AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True`
- `CAPABILITY_BREADTH_STENCIL_OK:True`
- `CAPABILITY_BREADTH_EXPORT_OK:True`
- `CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`
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
- `WORKBENCH_SCOPE_BOUNDARY_OK:True`
- `GRAPH_SEARCH_LOCATE_OK:True`
- `GRAPH_SEARCH_SCOPE_FILTER_OK:True`
- `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`
- `COMMAND_PALETTE_GROUPING_OK:True`
- `COMMAND_PALETTE_DISABLED_REASON_OK:True`
- `COMMAND_PALETTE_RECENT_ACTIONS_OK:True`
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

Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`
- `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`
- `GRAPH_VALIDATION_FEEDBACK_OK:True`
- `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`
- `GRAPH_READINESS_STATUS_OK:True`

This quick reference is summary-only; Proof Handoff owns the actual intake instructions.
The support bundle stays local evidence only and does not widen the support boundary.

Next beta intake links:

- [Beta Support Bundle](./support-bundle.md)
- [Adoption Feedback Loop](./adoption-feedback.md)

## Run It

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

For CI-style proof mode:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

For a local beta support bundle:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

## Proof Handoff

Proof Handoff owns the actual intake instructions.

If you copied the starter recipe, validate the defended host here with `AsterGraph.ConsumerSample.Avalonia -- --proof` first.

For the actual intake record, run `AsterGraph.ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>` and reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note on the bounded intake record.

If the route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`.
If `CONSUMER_SAMPLE_PARAMETER_OK` or `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` fail, keep the failed proof-marker lines with the support bundle's `parameterSnapshots` rows on that same bounded intake record.
If graph readiness is the question, keep `readinessStatus`, `validationSummary`, and `validationFeedback` from the same bundle; those fields are projected from the canonical session validation snapshot.

It should stay local evidence only and should not widen the support boundary.

Expected proof markers:

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
- `WORKBENCH_DEFAULTS_OK:True`
- `WORKBENCH_HOST_BUILDER_HANDOFF_OK:True`
- `WORKBENCH_SCOPE_BOUNDARY_OK:True`
- `GRAPH_SEARCH_LOCATE_OK:True`
- `GRAPH_SEARCH_SCOPE_FILTER_OK:True`
- `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`
- `COMMAND_PALETTE_GROUPING_OK:True`
- `COMMAND_PALETTE_DISABLED_REASON_OK:True`
- `COMMAND_PALETTE_RECENT_ACTIONS_OK:True`
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

Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`
- `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`
- `GRAPH_VALIDATION_FEEDBACK_OK:True`
- `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`
- `GRAPH_READINESS_STATUS_OK:True`
- `CONSUMER_SAMPLE_OK:True`

## When To Use This Sample

Use `ConsumerSample.Avalonia` when you need one realistic host after `HelloWorld.Avalonia` and before the full `Demo`.

Use a different artifact when you need something narrower:

- `Starter.Avalonia` = first hosted scaffold and smallest end-to-end Avalonia entry
- `HelloWorld` = smallest runtime-only first run
- `HelloWorld.Avalonia` = smallest hosted-UI first run
- `HostSample` = proof harness for canonical route validation
- `PackageSmoke` = packed-package proof
- `ScaleSmoke` = scale and state-continuity proof
- `Demo` = full showcase host

## Integration Notes

The sample is intentionally small enough to copy from:

- action rail / command projection: keep the host actions outside the editor shell and project shared descriptors through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: keep `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and allowlist import/export together in the host; plugin trust stays explicit and host-owned through discovery snapshots, reason strings, and allowlist import/export. allowlist decisions can be exported or imported without rebuilding the host trust-policy flow.
- trusted plugin proof handoff: pair `CONSUMER_SAMPLE_TRUST_OK:True` from this sample with `ASTERGRAPH_PLUGIN_VALIDATE_OK:True` from `AsterGraph.PluginTool validate`, then review [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md) before treating a third-party plugin artifact as loadable.
- selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads the selected node parameters, and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes them back
- snippet catalog and insertion seam: keep `consumer.sample.snippet.queue-lane` and any other snippets host-owned, then insert by reusing `StartConnection(...)` plus `TryCreateConnectedNodeFromPendingConnection(...)`
- plugin loading remains in-process; there is no sandbox or untrusted-code isolation
- sample-owned details such as the review/audit node family, action ids and titles, the window layout, and the narrative text are replaceable
- the v1 manifest and trust-policy contract is published in [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)

## Copy This Pattern

If you want to build the same medium host in your own app, copy these seams in this order:

- action rail / command projection: query `session.Queries.GetCommandDescriptors()` indirectly through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and project them with `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: keep `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and an explicit host-owned allowlist policy together
- trusted plugin proof handoff: keep `CONSUMER_SAMPLE_TRUST_OK:True`, `ASTERGRAPH_PLUGIN_VALIDATE_OK:True`, and [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md) together when reviewing one trusted plugin path
- selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads the selected node parameters, and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes them back
- node-side authoring seam: `IGraphEditorSession.Queries.GetNodeParameterSnapshots(nodeId)` plus `INodeParameterEditorRegistry` keep the node surface on the same metadata and validation contract as the inspector
- snippet seam: expose host-owned snippets through a small catalog, insert them with the existing pending-connection command path, and expect `GRAPH_SNIPPET_CATALOG_OK:True` plus `GRAPH_SNIPPET_INSERT_OK:True`
- default workbench seam: use `AsterGraphHostBuilder.UseDefaultWorkbench()` for the stock toolbar, command palette, stencil, inspector, mini-map, fragment, diagnostics, and status chrome; expect `WORKBENCH_DEFAULTS_OK:True`, `WORKBENCH_HOST_BUILDER_HANDOFF_OK:True`, and `WORKBENCH_SCOPE_BOUNDARY_OK:True`
- graph search seam: search the hosted graph from current snapshots, locate nodes/connections through existing selection and viewport commands, and expect `GRAPH_SEARCH_LOCATE_OK:True`, `GRAPH_SEARCH_SCOPE_FILTER_OK:True`, and `GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True`
- command palette productivity: group palette actions by the existing command descriptor group, surface disabled reasons from the shared command route, and keep a bounded in-memory recent-action section with `COMMAND_PALETTE_GROUPING_OK:True`, `COMMAND_PALETTE_DISABLED_REASON_OK:True`, and `COMMAND_PALETTE_RECENT_ACTIONS_OK:True`
- navigation productivity: keep graph search, command palette, back/forward history, scope breadcrumbs, and focus restore host-owned on top of existing selection/scope/viewport commands, and expect `NAVIGATION_PRODUCTIVITY_PROOF_OK:True`, `NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True`, and `NAVIGATION_SCOPE_BOUNDARY_OK:True`
- proof mode: emit the `AUTHORING_SURFACE_*` markers, `COMMAND_SURFACE_OK`, and the widened `HOST_NATIVE_METRIC:*` lines so you can compare your host with the shipped samples and keep the defended large-tier contract in view through `ScaleSmoke`
- widened hosted tuning: emit `WIDENED_SURFACE_PERFORMANCE_OK:True` and reuse [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) so the hosted metrics stay tied to `ScaleSmoke`
- capability breadth: pair the same route with [Capability Breadth Recipe](./capability-breadth-recipe.md) and emit the `CAPABILITY_BREADTH_*` markers from `AsterGraph.ConsumerSample.Avalonia -- --proof`
- onboarding markers: keep `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True`, `CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`, `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True`, `GRAPH_VALIDATION_FEEDBACK_OK:True`, `GRAPH_FEEDBACK_FOCUS_TARGET_OK:True`, `GRAPH_READINESS_STATUS_OK:True`, `FIVE_MINUTE_ONBOARDING_OK:True`, and `ONBOARDING_CONFIGURATION_OK:True`
- authoring flow markers: keep `AUTHORING_FLOW_PROOF_OK:True`, `AUTHORING_FLOW_HANDOFF_OK:True`, and `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True` together with the quick-add, insert-on-wire, reconnect editing, edge multiselect, and wire slicing proof lines
- final handoff markers: keep `EXPERIENCE_POLISH_HANDOFF_OK:True`, `FEATURE_ENHANCEMENT_PROOF_OK:True`, and `EXPERIENCE_SCOPE_BOUNDARY_OK:True` together so the scope boundary remains explicit
- sample-owned content such as the review/audit node family, action ids and titles, and proof labels beyond the defended markers should stay local to your app

## Related Docs

- [Quick Start](./quick-start.md)
- [Capability Breadth Recipe](./capability-breadth-recipe.md)
- [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)
- [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
- [Host Integration](./host-integration.md)
- [Sample README](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)
- [Alpha Status](./alpha-status.md)
