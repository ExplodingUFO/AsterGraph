# AsterGraph.HelloWorld.Avalonia

This sample is the smallest shipped-UI starting point in the repo.

Use it when you want to see the default Avalonia shell with the canonical factory-based route and without the extra proof logic from `AsterGraph.HostSample`.

It starts with one node selected so the shipped inspector immediately shows grouped parameter sections, stock editors, validation cues, and placeholder hints.

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo
```

It deliberately shows only one route:

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`

Use `AsterGraph.HelloWorld` for the runtime-only first run. Use `AsterGraph.HostSample` when you need the proof harness for canonical route validation. Use [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md) when you want to reproduce the same definition-driven inspector pattern in your own host.
