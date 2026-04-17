# AsterGraph Quick Start

This guide is the shortest path from a blank host to a running AsterGraph integration.

## 1. Pick Your Starting Package

| Host goal | Start package | Why |
| --- | --- | --- |
| Default Avalonia UI host | `AsterGraph.Avalonia` | main UI entry with the shipped shell and view factories |
| Runtime-only or custom UI host | `AsterGraph.Editor` | canonical session/runtime surface |
| Contract-first integration | `AsterGraph.Abstractions` | stable identifiers, definitions, and provider contracts |

Add `AsterGraph.Core` only when the host also needs direct `GraphDocument`, serialization, or compatibility APIs.

## 2. Install From NuGet

The public alpha packages are published to nuget.org, so the default `dotnet` restore flow plus `--prerelease` is enough.

```powershell
# shipped Avalonia UI host
dotnet add package AsterGraph.Avalonia --prerelease

# runtime-only or custom UI host
dotnet add package AsterGraph.Editor --prerelease

# node definitions and provider contracts
dotnet add package AsterGraph.Abstractions --prerelease
```

`AsterGraph.Demo` is sample-only and is not part of the supported SDK boundary.

## 3. Fastest First Run

For the smallest possible runtime-only sample, run:

```powershell
dotnet run --project tools/AsterGraph.HelloWorld/AsterGraph.HelloWorld.csproj --nologo
```

Use `HelloWorld` when you want the simplest starting point. Use `HostSample` when you want a proof harness for the canonical runtime-only and hosted-UI routes.

## 4. Canonical Adoption Routes

| If your host needs | Start here | First sample |
| --- | --- | --- |
| Runtime-only or custom UI | `AsterGraphEditorFactory.CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| Shipped Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HostSample` |
| Plugin trust/discovery | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | [Host Integration](./host-integration.md) |
| Automation | `IGraphEditorSession.Automation.Execute(...)` | [Host Integration](./host-integration.md) |
| Retained migration | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | [Host Integration](./host-integration.md) |

For new work, start with the runtime/session route or the shipped Avalonia route. Treat the retained route as migration-only.

## 5. Minimal Hosted-UI Composition

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

## 6. Plugin Trust Boundary

Plugin loading is currently in-process. Hosts can discover candidates, apply an allow/block trust policy, and inspect load results. AsterGraph does not provide sandboxing or untrusted-code isolation.

## 7. Need More Than The First Run?

- [Host Integration](./host-integration.md) = package boundary, route matrix, migration guidance
- [Alpha Status](./alpha-status.md) = current scope, non-goals, and known limitations
- [Demo Guide](./demo-guide.md) = full showcase host
- [`tools/AsterGraph.HostSample`](../../tools/AsterGraph.HostSample/) = minimal canonical proof harness
- [`tools/AsterGraph.PackageSmoke`](../../tools/AsterGraph.PackageSmoke/) = packed-package proof
- [`tools/AsterGraph.ScaleSmoke`](../../tools/AsterGraph.ScaleSmoke/) = scale/history/state proof

## 8. Maintainer And Source-Validation Paths

If you are validating the repository itself instead of consuming the published packages:

- maintainer workflow and lanes: [CONTRIBUTING.md](../../CONTRIBUTING.md)
- release sign-off and manual NuGet publish flow: [Public Launch Checklist](./public-launch-checklist.md)
- historical tags versus package versions: [Versioning](./versioning.md)
