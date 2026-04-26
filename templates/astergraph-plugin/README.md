# AsterGraph Plugin

This project is a minimal in-process AsterGraph plugin.

Build it with:

```powershell
dotnet build
```

Validate the built assembly with:

```powershell
dotnet run --project ../path/to/AsterGraph.PluginTool -- validate ./bin/Debug/net8.0/AsterGraphPlugin.dll
```

AsterGraph plugins are trusted, in-process extensions. Use host-owned allowlists, hashes, or publisher checks before loading third-party code.
