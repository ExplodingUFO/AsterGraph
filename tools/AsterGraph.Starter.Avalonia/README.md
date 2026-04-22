# AsterGraph.Starter.Avalonia

This is the first hosted recipe in the public cookbook path. It is copyable as-is when you want the smallest Avalonia host scaffold.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

Use it when you want the smallest end-to-end Avalonia scaffold that already wires the editor factory to the view factory, before moving on to `AsterGraph.HelloWorld.Avalonia` and then `AsterGraph.ConsumerSample.Avalonia`.

Keep/copy from this recipe:

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphEditorOptions`
- the document/catalog/editor/view composition flow

Replace/own in your host:

- the top-level window and its title/size

- the sample graph/catalog definitions once the host outgrows the tiny starter recipe

Run it with:

```powershell
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo
```

Use `AsterGraph.HelloWorld` when you want the runtime-only route, `AsterGraph.HelloWorld.Avalonia` when you want the smallest stock hosted UI, `AsterGraph.ConsumerSample.Avalonia` when you need a realistic host hop, and `AsterGraph.HostSample` when you need proof-oriented route validation.

