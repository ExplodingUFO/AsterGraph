# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` is the medium hosted-UI sample between the smallest `HelloWorld.Avalonia` route and the full `AsterGraph.Demo` showcase host.

## What It Proves

This sample keeps one realistic host window without turning into a full showcase shell. It demonstrates:

- one host-owned action rail
- one host-defined node family
- shared parameter editing through the canonical session commands and queries
- one trusted plugin registration with visible load snapshots
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

- host actions live outside the editor shell
- plugin trust stays explicit and host-owned
- parameter editing goes through `IGraphEditorSession.Commands` and `IGraphEditorSession.Queries`
- plugin loading remains in-process; there is no sandbox or untrusted-code isolation

## Related Docs

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Alpha Status](./alpha-status.md)
