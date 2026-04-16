# AsterGraph Quick Start

This is the fastest path from a blank host to a running embedded editor.

For current alpha scope, known limitations, and stability notes, see [`alpha-status.md`](./alpha-status.md).

## 1) Choose package entry

| Host goal | Start package | Why |
| --- | --- | --- |
| Default Avalonia UI host | `AsterGraph.Avalonia` | Main UI entry. Includes the shipped shell/factories and pulls required editor/core dependencies. |
| Contract-first integration (definitions/IDs/providers) | `AsterGraph.Abstractions` | Stable contract entry with no UI dependency. |

`AsterGraph.Demo` is sample-only. Do not use it as a package dependency.

## 2) Choose package source

Today there are three source shapes to know about:

- repo-local packages in `artifacts/packages`
  - this is the proof path used by the maintained smoke and release-validation flows
- GitHub Packages
  - optional private/internal feed
- NuGet.org prerelease
  - intended public alpha channel once the tag-driven prerelease workflow is active

Use the repo-local feed when validating the current branch from source.

```powershell
copy NuGet.config.sample NuGet.config
```

Optional GitHub Packages setup:

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

See [`host-integration.md`](./host-integration.md#package-feed-options) for the longer feed and publish-channel guidance.

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

| If your host needs | Packages to start with | Start here | Verify with |
| --- | --- | --- | --- |
| Runtime-only or custom UI | `AsterGraph.Abstractions`, `AsterGraph.Editor`; add `AsterGraph.Core` when host code also uses `GraphDocument`, serialization, or compatibility services directly | `AsterGraphEditorFactory.CreateSession(...)` | `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo` |
| Shipped Avalonia UI | `AsterGraph.Avalonia`; add direct `AsterGraph.Editor` and/or `AsterGraph.Core` references only when the host uses those APIs directly | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo` |
| Plugin trust/discovery | `AsterGraph.Editor`; add `AsterGraph.Abstractions` when host code also shares node/provider contracts | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release` |
| Automation | `AsterGraph.Editor`; add `AsterGraph.Core` when host code also consumes direct snapshots/models outside the session facade | `IGraphEditorSession.Automation.Execute(...)` | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release` |
| Retained migration | `AsterGraph.Editor` plus `AsterGraph.Avalonia` when the host still embeds `GraphEditorView` | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release` |

Use the runtime-only/custom UI route or the shipped Avalonia UI route for new integrations. Treat plugin trust/discovery and automation as capabilities layered on top of those canonical entry points. Keep the retained migration route only when you are deliberately staying on the compatibility window.

If you want to own Avalonia layout while still reusing the stock canvas, inspector, or mini map, stay on the `Create(...)` family and treat those surface factories as an advanced hosted-UI detail, not as a fourth canonical entry path.

See [`docs/host-integration.md`](./host-integration.md) for the longer composition walkthrough behind each route.
See [`docs/state-contracts.md`](./state-contracts.md) for the explicit history/save/dirty behavior contract enforced by the focused proof lane.

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
# runs focused contract proof, packs packages, runs HostSample + PackageSmoke + ScaleSmoke against packed packages, collects coverage, and runs package validation
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

For the focused consumer/contract lane:

```powershell
# plugin trust/discovery, automation, hosted-surface proof, and history/save/dirty contract
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
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
