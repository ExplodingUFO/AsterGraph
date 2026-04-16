# AsterGraph Quick Start

This is the shortest path from a blank host to a running public-alpha integration.

See also:

- [Alpha Status](./alpha-status.md)
- [Host Integration](./host-integration.md)
- [State Contracts](./state-contracts.md)
- [Extension Contracts](./extension-contracts.md)
- [Demo Guide](./demo-guide.md)

## Package Entry

| Host goal | Start package | Why |
| --- | --- | --- |
| Default Avalonia UI host | `AsterGraph.Avalonia` | Main UI entry with the shipped shell and view factories |
| Contract-first integration | `AsterGraph.Abstractions` | Stable identifiers, definitions, and provider contracts |
| Runtime-first custom host | `AsterGraph.Editor` | Canonical session/runtime surface |

`AsterGraph.Demo` is sample-only and is not part of the supported package boundary.

## Package Source

Current source shapes:

- repo-local feed in `artifacts/packages`
- optional GitHub Packages feed
- planned public prerelease channel on NuGet.org

For branch validation from source:

```powershell
copy NuGet.config.sample NuGet.config
```

## Canonical Adoption Path

| If your host needs | Start here | Verify with |
| --- | --- | --- |
| Runtime-only or custom UI | `AsterGraphEditorFactory.CreateSession(...)` | `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo` |
| Shipped Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | same `HostSample` command |
| Plugin trust/discovery | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release` |
| Automation | `IGraphEditorSession.Automation.Execute(...)` | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release` |
| Retained migration | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release` |

For new work, start with the runtime/session route or the shipped Avalonia route. Treat the retained route as migration-only.

## Minimal Shipped-UI Composition

```csharp
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;

INodeCatalog catalog = CreateCatalog();
var document = GraphDocument.Empty;

var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new DefaultPortCompatibilityService(),
});

var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
});
```

## Verification

Preferred repo entrypoints:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

Raw proof tools:

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo
```
