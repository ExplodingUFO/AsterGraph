# AsterGraph.HelloWorld.Avalonia

This sample is the smallest shipped-UI starting point in the repo.

Use it when you want to see the default Avalonia shell with the canonical factory-based route and without the extra proof logic from `AsterGraph.HostSample`.

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo
```

It deliberately shows only one route:

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`

Use `AsterGraph.HelloWorld` for the runtime-only first run. Use `AsterGraph.HostSample` when you need the proof harness for canonical route validation.
