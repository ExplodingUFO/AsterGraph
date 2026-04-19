# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` is the medium hosted-UI sample between the smallest `HelloWorld.Avalonia` route and the full `AsterGraph.Demo` showcase host.

## What It Proves

This sample keeps one realistic host window without turning into a full showcase shell. It demonstrates:

- one host-owned action rail
- one host-defined node family
- shared command descriptors flowing into the host rail through `AsterGraphHostedActionFactory.CreateCommandActions(...)`
- shared parameter editing through the canonical session commands and queries
- one trusted plugin registration with visible provenance, trust reasons, and allowlist import or export
- the shipped Avalonia editor surface on the factory-based hosted-UI route

## Run It

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

For CI-style proof mode:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

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
- `CONSUMER_SAMPLE_OK:True`

## When To Use This Sample

Use `ConsumerSample.Avalonia` when you need one realistic host before jumping to the full `Demo`.

Use a different artifact when you need something narrower:

- `HelloWorld` = smallest runtime-only first run
- `HelloWorld.Avalonia` = smallest hosted-UI first run
- `HostSample` = proof harness for canonical route validation
- `PackageSmoke` = packed-package proof
- `ScaleSmoke` = scale and state-continuity proof
- `Demo` = full showcase host

## Integration Notes

The sample is intentionally small enough to copy from:

- host actions live outside the editor shell and are projected from shared command descriptors instead of a second action table
- plugin trust stays explicit and host-owned through discovery snapshots, reason strings, and a persisted allowlist
- parameter editing goes through `IGraphEditorSession.Commands` and `IGraphEditorSession.Queries`
- allowlist decisions can be exported or imported without rebuilding the host trust-policy flow
- plugin loading remains in-process; there is no sandbox or untrusted-code isolation

## Copy This Pattern

If you want to build the same medium host in your own app, copy these seams in this order:

- command rail: query `session.Queries.GetCommandDescriptors()` indirectly through `AsterGraphHostedActionFactory.CreateCommandActions(...)`
- trust workflow: keep `GraphEditorPluginDiscoveryOptions`, provenance snapshots, and an explicit host-owned allowlist policy together
- parameter editing: mutate selected-node values only through `IGraphEditorSession.Commands`
- proof mode: emit `COMMAND_SURFACE_OK` plus the four `HOST_NATIVE_METRIC:*` lines so you can compare your host with the shipped samples

## Related Docs

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Alpha Status](./alpha-status.md)
