# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` is the medium hosted-UI sample on the canonical session/runtime route, after the starter scaffold and the smallest `HelloWorld.Avalonia` route, and before the full `AsterGraph.Demo` showcase host.
For trust-policy review and local evidence, pair this route with [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md) and [Beta Support Bundle](./support-bundle.md).

It is the copyable host recipe for three host-owned seams:

- action rail / command projection
- plugin trust workflow
- parameter-editing composition

This is the defended beta route for copying host-owned seams into your own host. Keep action projection, trust workflow, and parameter-editing composition host-owned; copy only the sample-owned presentation.

It stays on the canonical session/runtime model only. It does not introduce a second editor model, a sandbox, or a broader plugin ecosystem.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.
`HostSample` is the post-ladder proof harness.

## What It Proves

This sample keeps one realistic host window without turning into a full showcase shell. It demonstrates:

- one host-owned action rail projected from shared command descriptors through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- one host-defined node family that is intentionally sample-owned and replaceable
- one plugin-contributed command flowing through the same action path instead of a sample-only menu placeholder
- shared parameter editing through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`
- one trusted plugin registration with visible provenance, trust reasons, and allowlist import or export
- the shipped Avalonia editor surface on the factory-based hosted-UI route

## Copy These Host-Owned Seams

- action rail / command projection: keep the host actions outside the editor shell and project shared descriptors through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: keep `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and an explicit host-owned allowlist policy together
- parameter-editing composition: mutate selected-node values only through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`

## Replace These Sample-Owned Details

Keep these sample-owned details local to your app:

- review/audit node family
- action ids and titles
- window layout and narrative text
- proof labels beyond the defended markers

## Trust and proof quick reference

Copyable trust and proof reference:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

Expected proof markers:

- `CONSUMER_SAMPLE_TRUST_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOST_NATIVE_METRIC:*`

Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:

- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

The support bundle stays local evidence only and does not widen the support boundary.

Next beta intake links:

- [Beta Support Bundle](./support-bundle.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Public Launch Checklist](./public-launch-checklist.md)

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

If you copied the starter recipe, validate the defended host here with `AsterGraph.ConsumerSample.Avalonia -- --proof` first.

For reviewable local evidence, run `AsterGraph.ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>` and reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the note.

If the route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`.

It should stay local evidence only and should not widen the support boundary.

Expected proof markers:

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

Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:

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
- parameter-editing composition: read selection through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and write through `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`
- plugin loading remains in-process; there is no sandbox or untrusted-code isolation
- sample-owned details such as the review/audit node family, action ids and titles, the window layout, and the narrative text are replaceable
- the v1 manifest and trust-policy contract is published in [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)

## Copy This Pattern

If you want to build the same medium host in your own app, copy these seams in this order:

- action rail / command projection: query `session.Queries.GetCommandDescriptors()` indirectly through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and project them with `AsterGraphHostedActionFactory.CreateProjection(...)`
- plugin trust workflow: keep `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and an explicit host-owned allowlist policy together
- parameter-editing composition: mutate selected-node values only through `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` and `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)`
- proof mode: emit `COMMAND_SURFACE_OK` plus the four `HOST_NATIVE_METRIC:*` lines so you can compare your host with the shipped samples
- sample-owned content such as the review/audit node family, action ids and titles, and proof labels beyond the defended markers should stay local to your app

## Related Docs

- [Quick Start](./quick-start.md)
- [Beta Support Bundle](./support-bundle.md)
- [Host Integration](./host-integration.md)
- [Sample README](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)
- [Alpha Status](./alpha-status.md)
