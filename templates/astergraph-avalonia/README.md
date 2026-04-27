# AsterGraph Avalonia Host

This project is a minimal native Avalonia desktop host for AsterGraph.

Run it with:

```powershell
dotnet run
```

## Builder-first hosted route

The generated host starts on `AsterGraphHostBuilder`:

- replace the sample `GraphDocument` with your product document
- replace the sample `NodeCatalog` with your product catalog
- keep `UseDefaultCompatibility()` until your host owns custom compatibility rules
- keep `BuildAvaloniaView()` when the stock hosted Avalonia shell is enough

Drop down to explicit `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` wiring when your host owns document storage, plugin trust, localization, diagnostics, presentation overrides, or standalone surface composition separately. That explicit route is still the same hosted editor/session owner, not a second runtime model.

## Proof handoff

Use the template smoke path to validate the generated app still opens as a hosted Avalonia scaffold. If the host adds plugins, validate plugin artifacts with:

```powershell
dotnet run --project tools/AsterGraph.PluginTool -- validate <plugin-path>
```

Expect PluginTool validation to report `ASTERGRAPH_PLUGIN_VALIDATE_OK:True` before loading third-party plugin artifacts in a copied host.
