# AsterGraph.ConsumerSample.Avalonia

This is the medium hosted-UI sample on the canonical session/runtime route.
It shows a host-owned action rail, parameter editing, and one trusted plugin without implying a second editor model or a retained-only path.

## Run

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

For proof mode:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

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
- `CONSUMER_SAMPLE_OK:True`

## Where It Fits

- [Quick Start](../../docs/en/quick-start.md)
- [Host Integration](../../docs/en/host-integration.md)
- [Demo Guide](../../docs/en/demo-guide.md)

