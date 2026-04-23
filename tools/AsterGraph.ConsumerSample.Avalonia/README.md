# AsterGraph.ConsumerSample.Avalonia

This is the medium hosted-UI sample on the canonical session/runtime route, after `AsterGraph.Starter.Avalonia` and `AsterGraph.HelloWorld.Avalonia`, and before `AsterGraph.Demo`.
It shows a host-owned action rail, plugin trust workflow, and parameter-editing composition without implying a second editor model, a sandbox, or a broader plugin ecosystem.
For plugin-capable evaluators, this is the defended hosted trust hop. Read [Plugin Manifest and Trust Policy Contract v1](../../docs/en/plugin-trust-contracts.md) and [Plugin And Custom Node Recipe](../../docs/en/plugin-recipe.md) before treating the route as complete.

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

Validate the defended route here with `AsterGraph.ConsumerSample.Avalonia -- --proof`. For reviewable local evidence, run the bundle-producing command above and reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the attachment note. If the route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`. The support bundle stays local evidence only.

Expected markers:

- `CONSUMER_SAMPLE_HOST_ACTION_OK:True`
- `CONSUMER_SAMPLE_PLUGIN_OK:True`
- `CONSUMER_SAMPLE_PARAMETER_OK:True`
- `CONSUMER_SAMPLE_WINDOW_OK:True`
- `CONSUMER_SAMPLE_TRUST_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOST_NATIVE_METRIC:startup_ms=...`
- `HOST_NATIVE_METRIC:inspector_projection_ms=...`
- `HOST_NATIVE_METRIC:plugin_scan_ms=...`
- `HOST_NATIVE_METRIC:command_latency_ms=...`
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

## Copyable Host Recipe

Use this sample to copy the host-owned seams, not the sample-specific presentation layer. This is the defended beta route for copying host-owned seams into your own host. Copy the host-owned seams in this order:

### Copy These Host-Owned Seams

- action rail / command projection: `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, and the host allowlist import/export path
- parameter-editing composition: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`

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
