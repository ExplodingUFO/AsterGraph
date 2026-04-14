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

## 4) Minimal Avalonia host composition

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

This is the canonical hosted-UI path for new Avalonia hosts.

Route guide:

- use `AsterGraphEditorFactory.CreateSession(...)` if your host owns the UI and only wants the canonical runtime boundary
- use `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` for the canonical shipped-UI path
 - keep `new GraphEditorViewModel(...)` or `new GraphEditorView { Editor = ... }` only when you are intentionally staying on the retained compatibility path during migration
 - see `src/AsterGraph.Demo` for the visual hosted-UI reference
 - see `tools/AsterGraph.PackageSmoke` for the machine-checkable `PACKAGE_SMOKE_*` package-consumption markers
 - see `tools/AsterGraph.ScaleSmoke` for the large-graph scale/readiness markers

## 5) Where Abstractions fits

Use `AsterGraph.Abstractions` to define and distribute node contracts (`INodeDefinitionProvider`, `NodeDefinitionId`, related definition/identifier types) so host-side providers and libraries remain stable and UI-independent.

## 6) Proof surface and regression lanes

The live proof surface now uses the dedicated tools:

- `tools/AsterGraph.PackageSmoke`
- `tools/AsterGraph.ScaleSmoke`

Validate through the repository CI entrypoint first:

```powershell
# validate with the repository entrypoint in eng/ci.ps1
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
```

When you need lane-level checks:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --nologo -v minimal
dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj --nologo -v minimal
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj --nologo -v minimal
```

This split keeps sample-host behavior in `AsterGraph.Demo.Tests` separate from the core SDK regression lane.
