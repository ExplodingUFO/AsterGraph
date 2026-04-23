# AsterGraph.ConsumerSample.Avalonia

This is the medium hosted-UI sample on the canonical session/runtime route, after `AsterGraph.Starter.Avalonia` and `AsterGraph.HelloWorld.Avalonia`, and before `AsterGraph.Demo`.
It shows a host-owned action rail, plugin trust workflow, and the selected-node parameter read/write seam without implying a second editor model, a sandbox, or a broader plugin ecosystem.
For plugin-capable evaluators, this is the defended hosted trust hop. Read [Plugin Manifest and Trust Policy Contract v1](../../docs/en/plugin-trust-contracts.md) and [Plugin And Custom Node Recipe](../../docs/en/plugin-recipe.md) before treating the route as complete.

For the inspector metadata recipe, pair this sample with [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md). The sample stays focused on the host-owned seams and the shipped inspector surface; it does not own the metadata vocabulary. The canonical recipe carries the full `defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, and read-only vocabulary.
For copyable custom node, port, parameter, and edge presentation on the same hosted route, pair it with [Authoring Surface Recipe](../../docs/en/authoring-surface-recipe.md).
For searchable grouped stencil, SVG/PNG/JPEG export breadth, and shared node or edge quick tools on the same hosted route, pair it with [Capability Breadth Recipe](../../docs/en/capability-breadth-recipe.md).

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

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

### Copyable Parameter/Metadata Path

- Define metadata in [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md) first with `defaultValue`, `editorKind`, `constraints`, and `groupName`.
- Project and write selected-node values in this sample through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`.
- Validate evidence with proof mode plus a support bundle; compare `parameterSnapshots` with `CONSUMER_SAMPLE_PARAMETER_OK:True` and `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`.

### Copyable Authoring Surface Handoff

- Keep definitions and metadata in `NodeDefinition`, then project inspector state from `GetSelectedNodeParameterSnapshots()`.
- Project node-side editor state from `GetNodeParameterSnapshots(nodeId)` so `NodeParameterEditorHost` and `INodeParameterEditorRegistry` reuse the same validation-aware parameter contract.
- Keep writes on the shared session command path through `TrySetSelectedNodeParameterValue(...)` or `TrySetNodeParameterValue(...)`.
- Project host actions from `GetCommandDescriptors()` and close the route with `AsterGraph.ConsumerSample.Avalonia -- --proof`, expecting `AUTHORING_SURFACE_OK:True`.

### Copyable Capability Breadth Handoff

- Keep searchable grouped stencil on the stock hosted view and drive it from `IGraphEditorSession.Queries.GetNodeTemplateSnapshots()`.
- Keep scene export on `IGraphEditorSession.Commands.TryExportSceneAsSvg(...)` plus `IGraphEditorSession.Commands.TryExportSceneAsImage(...)` for `GraphEditorSceneImageExportFormat.Png` and `GraphEditorSceneImageExportFormat.Jpeg`.
- Keep node and edge quick tools on the shared tool route through `IGraphEditorSession.Queries.GetToolDescriptors(...)` and `AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(...)`.
- Close the route with [Capability Breadth Recipe](../../docs/en/capability-breadth-recipe.md) and `AsterGraph.ConsumerSample.Avalonia -- --proof`, expecting `CAPABILITY_BREADTH_OK:True`.

### Replace These Sample-Owned Details

Replaceable sample-owned details are the review/audit node family, the sample action ids and titles, the window layout and narrative text, and any proof labels or copy beyond the defended public markers.

- sample-owned details are the review/audit node family
- sample action ids and titles
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
