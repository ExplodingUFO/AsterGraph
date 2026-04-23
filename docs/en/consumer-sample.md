# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` is the medium hosted-UI sample on the canonical session/runtime route, after the starter scaffold and the smallest `HelloWorld.Avalonia` route, and before the full `AsterGraph.Demo` showcase host.
For trust-policy review and local evidence, pair this route with [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md) and [Beta Support Bundle](./support-bundle.md).

It is the host seam example for three host-owned seams:

- action rail / command projection
- plugin trust workflow
- selected-node parameter read/write seam

For the inspector metadata recipe, pair this route with [Authoring Inspector Recipe](./authoring-inspector-recipe.md). This sample stays focused on the host-owned seams and the shipped inspector surface; it does not own the metadata vocabulary. The canonical recipe carries the full `defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, and read-only vocabulary.
For copyable custom node, port, and edge presentation on the same route, pair it with [Authoring Surface Recipe](./authoring-surface-recipe.md).
For searchable grouped stencil, SVG/PNG/JPEG export breadth, and shared node or edge quick tools on the same route, pair it with [Capability Breadth Recipe](./capability-breadth-recipe.md).
For one copyable hosted tuning handoff that keeps those widened surfaces tied to defended `ScaleSmoke` budgets, pair it with [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md).

This is the defended beta route for copying host-owned seams into your own host. Keep action projection, trust workflow, and the selected-node parameter read/write seam host-owned; copy only the sample-owned presentation.

It stays on the canonical session/runtime model only. It does not introduce a second editor model, a sandbox, or a broader plugin ecosystem.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.
`HostSample` is the post-ladder proof harness.

## What It Proves

This sample keeps one realistic host window without turning into a full showcase shell. It demonstrates:

- one host-owned action rail projected from shared command descriptors through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- one host-defined node family that is intentionally sample-owned and replaceable
- one plugin-contributed command flowing through the same action path instead of a sample-only menu placeholder
- one selected-node parameter read/write seam through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`
- one trusted plugin registration with visible provenance, trust reasons, and allowlist import or export
- the shipped Avalonia editor surface on the factory-based hosted-UI route

## Copy These Host-Owned Seams

- action rail / command projection: keep the host actions outside the editor shell and project shared descriptors through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: keep `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and an explicit host-owned allowlist policy together
- selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads the selected node parameters, and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes them back

## Replace These Sample-Owned Details

Keep these sample-owned details local to your app:

- review/audit node family
- action ids and titles
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
- `WIDENED_SURFACE_PERFORMANCE_OK:True`
- `AUTHORING_SURFACE_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOST_NATIVE_METRIC:*`

Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:

- `SUPPORT_BUNDLE_PERSISTENCE_OK:True`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

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
- `WIDENED_SURFACE_PERFORMANCE_OK:True`
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
- selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads the selected node parameters, and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes them back
- plugin loading remains in-process; there is no sandbox or untrusted-code isolation
- sample-owned details such as the review/audit node family, action ids and titles, the window layout, and the narrative text are replaceable
- the v1 manifest and trust-policy contract is published in [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)

## Copy This Pattern

If you want to build the same medium host in your own app, copy these seams in this order:

- action rail / command projection: query `session.Queries.GetCommandDescriptors()` indirectly through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and project them with `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: keep `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and an explicit host-owned allowlist policy together
- selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads the selected node parameters, and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes them back
- node-side authoring seam: `IGraphEditorSession.Queries.GetNodeParameterSnapshots(nodeId)` plus `INodeParameterEditorRegistry` keep the node surface on the same metadata and validation contract as the inspector
- proof mode: emit the `AUTHORING_SURFACE_*` markers, `COMMAND_SURFACE_OK`, and the widened `HOST_NATIVE_METRIC:*` lines so you can compare your host with the shipped samples and keep the defended large-tier contract in view through `ScaleSmoke`
- widened hosted tuning: emit `WIDENED_SURFACE_PERFORMANCE_OK:True` and reuse [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md) so the hosted metrics stay tied to `ScaleSmoke`
- capability breadth: pair the same route with [Capability Breadth Recipe](./capability-breadth-recipe.md) and emit the `CAPABILITY_BREADTH_*` markers from `AsterGraph.ConsumerSample.Avalonia -- --proof`
- sample-owned content such as the review/audit node family, action ids and titles, and proof labels beyond the defended markers should stay local to your app

## Related Docs

- [Quick Start](./quick-start.md)
- [Capability Breadth Recipe](./capability-breadth-recipe.md)
- [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
- [Host Integration](./host-integration.md)
- [Sample README](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)
- [Alpha Status](./alpha-status.md)
