# AsterGraph Quick Start

This is the fastest path from a blank host to a running embedded editor.

## 1) Choose package entry

| Host goal | Start package | Why |
| --- | --- | --- |
| Default Avalonia UI host | `AsterGraph.Avalonia` | Main UI entry. Includes the shipped shell/factories and pulls required editor/core dependencies. |
| Contract-first integration (definitions/IDs/providers) | `AsterGraph.Abstractions` | Stable contract entry with no UI dependency. |

`AsterGraph.Demo` is sample-only. Do not use it as a package dependency.

## 2) Configure private GitHub Packages feed

```powershell
# Replace OWNER and credentials with your own values.
dotnet nuget add source "https://nuget.pkg.github.com/OWNER/index.json" `
  --name github-astergraph `
  --username GITHUB_USERNAME `
  --password GITHUB_PAT `
  --store-password-in-clear-text
```

Credential expectations:

- restore: token with `read:packages`
- publish: token with `write:packages`
- private repo package access may also require repo scopes by org policy

Safe config guidance:

- keep secrets in user-level NuGet config or CI secret variables
- keep repository `NuGet.config` credential-free

Credential-free source template:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-astergraph" value="https://nuget.pkg.github.com/OWNER/index.json" />
  </packageSources>
</configuration>
```

## 3) Install packages

Minimal UI host path:

```powershell
dotnet add package AsterGraph.Avalonia
```

Contract-first extension path (add contract layer first):

```powershell
dotnet add package AsterGraph.Abstractions
```

When your host directly composes runtime/editor APIs, also add:

```powershell
dotnet add package AsterGraph.Editor
```

## Canonical Adoption Path

This is the short source of truth for host adoption.

| If your host needs | Start here | Notes |
| --- | --- | --- |
| Runtime-only or custom UI | `AsterGraphEditorFactory.CreateSession(...)` | Canonical runtime-first boundary. Drive the host through `IGraphEditorSession.Commands`, `Queries`, `Events`, and `Diagnostics`. |
| Shipped Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | Canonical hosted-UI path for new Avalonia hosts that want the shipped shell. |
| Retained migration | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | Compatibility-only path for hosts that are intentionally migrating in stages. |

Use the first two routes for new integrations. Keep the third route only when you are deliberately staying on the retained migration window.

If you want to own Avalonia layout while still reusing the stock canvas, inspector, or mini map, stay on the `Create(...)` family and treat those surface factories as an advanced hosted-UI detail, not as a fourth canonical entry path.

See [`docs/host-integration.md`](./host-integration.md) for the longer composition walkthrough behind each route.

## Minimal Shipped UI Composition

```csharp
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;

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
    ChromeMode = GraphEditorViewChromeMode.Default,
});

// Set `view` as content in your Avalonia window.
```

This is the canonical shipped-UI route for new Avalonia hosts.

For the other two routes:

- runtime-only/custom UI details: [`docs/host-integration.md`](./host-integration.md#runtime-session-surface)
- retained migration details: [`docs/host-integration.md`](./host-integration.md#staged-migration-compatibility-path)
- minimal canonical host sample: `tools/AsterGraph.HostSample`
- visual/default hosted-UI reference: `src/AsterGraph.Demo`
- machine-checkable package/runtime proof: `tools/AsterGraph.PackageSmoke`
- machine-checkable scale/readiness proof: `tools/AsterGraph.ScaleSmoke`

## 5) Where Abstractions fits

Use `AsterGraph.Abstractions` to define and distribute node contracts (`INodeDefinitionProvider`, `NodeDefinitionId`, related definition/identifier types) so host-side providers and libraries remain stable and UI-independent.

## 6) Proof surface and regression lanes

The official proof ring now uses these maintained entry points:

- `tools/AsterGraph.HostSample`
- `tools/AsterGraph.PackageSmoke`
- `tools/AsterGraph.ScaleSmoke`

Validate the full release surface through the repository entrypoint first:

```powershell
# packs packages, runs PackageSmoke + ScaleSmoke, collects coverage, and runs package validation
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

For a faster build/test-only loop before the release gate:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
```

Run the smoke tools separately only when you want their raw marker output:

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo
```

When you need lane-level checks:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --nologo -v minimal
dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj --nologo -v minimal
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj --nologo -v minimal
```

This split keeps sample-host behavior in `AsterGraph.Demo.Tests` separate from the core SDK regression lane.
