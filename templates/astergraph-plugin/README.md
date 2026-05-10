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

Validate the built assembly through the host or release template-smoke lane. The local starter check is:

```powershell
dotnet build
```

The repository release lane validates generated plugin artifacts, manifest shape, capability summary, trust metadata, and hash evidence.

Expected validation evidence includes `TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True`, `TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK:True`, and `TEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK:True`.

AsterGraph plugins are trusted, in-process extensions. Use host-owned allowlists, hashes, or publisher checks before loading third-party code. This template does not add marketplace distribution, sandboxing, unload/reload, or untrusted-code isolation.
