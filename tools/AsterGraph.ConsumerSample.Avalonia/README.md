# AsterGraph.ConsumerSample.Avalonia

This is the medium hosted-UI sample on the canonical session/runtime route, after `AsterGraph.Starter.Avalonia` and `AsterGraph.HelloWorld.Avalonia`, and before `AsterGraph.Demo`.
It shows a host-owned action rail, parameter editing, and one trusted plugin without implying a second editor model or a retained-only path.

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
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle artifacts/consumer-support-bundle.json --support-note "what you were trying to validate"
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
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`
- `CONSUMER_SAMPLE_OK:True`

## Where It Fits

- `Starter.Avalonia` = first hosted scaffold and smallest end-to-end Avalonia entry
- `HelloWorld` = smallest runtime-only first run
- `HelloWorld.Avalonia` = smallest hosted-UI first run
- [Quick Start](../../docs/en/quick-start.md)
- [Beta Support Bundle](../../docs/en/support-bundle.md)
- [Host Integration](../../docs/en/host-integration.md)
- [Demo Guide](../../docs/en/demo-guide.md)

