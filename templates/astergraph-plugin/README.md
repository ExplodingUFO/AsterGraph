# AsterGraph Plugin

This project is a minimal trusted in-process AsterGraph plugin.

The sample keeps the authoring contract small:

- `SamplePlugin.Descriptor` owns the stable plugin id, display name, description, and optional version.
- `SampleNodeDefinitionProvider` contributes one public node definition through `INodeDefinitionProvider`.
- `astergraph.plugin.json` points package discovery at the built assembly and plugin type.
- host trust policy decides whether the plugin may activate; the plugin cannot authorize itself.

Build it with:

```powershell
dotnet build
```

Validate the built assembly with:

```powershell
dotnet run --project ../path/to/AsterGraph.PluginTool -- validate ./bin/Debug/net8.0/AsterGraphPlugin.dll
```

PluginTool also accepts a `.nupkg` plugin package or a directory containing plugin `.dll` or `.nupkg` artifacts:

```powershell
dotnet run --project ../path/to/AsterGraph.PluginTool -- validate ./bin/Debug/net8.0/AsterGraphPlugin.dll
dotnet run --project ../path/to/AsterGraph.PluginTool -- validate ./artifacts/AsterGraphPlugin.1.0.0.nupkg
dotnet run --project ../path/to/AsterGraph.PluginTool -- validate ./artifacts/plugins
dotnet run --project ../path/to/AsterGraph.PluginTool -- inspect ./bin/Debug/net8.0/AsterGraphPlugin.dll --host-version 0.15.0-beta --json
dotnet run --project ../path/to/AsterGraph.PluginTool -- hash ./bin/Debug/net8.0/AsterGraphPlugin.dll
```

Expected validation evidence includes `ASTERGRAPH_PLUGIN_VALIDATE_OK:True`, `PLUGIN_COMPATIBILITY_OK:True`, `PLUGIN_MANIFEST_OK:True`, `PLUGIN_NODE_DEFINITIONS_OK:True`, `PLUGIN_PARAMETER_METADATA_OK:True`, `PLUGIN_TRUST_EVIDENCE_OK:True`, `PLUGIN:<id>`, `target_framework:`, `capability_summary:`, `host_compatibility:`, `trust:`, `signature:`, and `sha256:`.

AsterGraph plugins are trusted, in-process extensions. Use host-owned allowlists, hashes, or publisher checks before loading third-party code. This template does not add marketplace distribution, sandboxing, unload/reload, or untrusted-code isolation.
