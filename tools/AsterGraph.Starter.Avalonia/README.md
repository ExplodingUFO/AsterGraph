# AsterGraph.Starter.Avalonia

This is the first hosted entry in the public cookbook path.

Use it when you want the smallest end-to-end Avalonia scaffold that already wires the editor factory to the view factory, before moving on to `AsterGraph.HelloWorld`, `AsterGraph.HelloWorld.Avalonia`, and `AsterGraph.ConsumerSample.Avalonia`.

It deliberately stays narrow:

- one hosted Avalonia window
- one tiny graph surface
- one canonical editor/view composition route

Run it with:

```powershell
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo
```

Use `AsterGraph.HelloWorld` when you want the runtime-only route, `AsterGraph.HelloWorld.Avalonia` when you want the smallest stock hosted UI, `AsterGraph.ConsumerSample.Avalonia` when you need a realistic host hop, and `AsterGraph.HostSample` when you need proof-oriented route validation.
