# AsterGraph.Starter.Avalonia

This is the first hosted recipe in the public cookbook path. It is copyable as-is when you want the smallest Avalonia host scaffold.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

Use it when you want the smallest end-to-end Avalonia scaffold that already wires the editor factory to the view factory, before moving on to `AsterGraph.HelloWorld.Avalonia` and then `AsterGraph.ConsumerSample.Avalonia`.

## Copyable Seams

### Copy This Starter Scaffold

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphEditorOptions`
- the document/catalog/editor/view composition flow

## Host-Owned Seams

### Replace This In Your Host

- the top-level window and its title/size
- the sample graph/catalog definitions once the host outgrows the tiny starter recipe

Run it with:

```powershell
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo
```

Use `AsterGraph.HelloWorld` when you want the runtime-only route, `AsterGraph.HelloWorld.Avalonia` when you want the smallest stock hosted UI, `AsterGraph.ConsumerSample.Avalonia` when you need a realistic host hop, and `AsterGraph.HostSample` when you need proof-oriented route validation.

## Proof Handoff

This starter recipe is only the scaffold. If you are validating a copied hosted recipe, hand off to `AsterGraph.ConsumerSample.Avalonia -- --proof` for the defended beta route.

That proof run is where you should expect `CONSUMER_SAMPLE_OK:True`, `COMMAND_SURFACE_OK:True`, the widened `HOST_NATIVE_METRIC:*` lines, and the local support bundle markers `SUPPORT_BUNDLE_OK:True` and `SUPPORT_BUNDLE_PATH:...` for the resolved path.

The support bundle is local evidence only; it does not widen the support boundary or introduce a separate verification lane.
