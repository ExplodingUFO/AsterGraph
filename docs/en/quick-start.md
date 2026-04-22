# AsterGraph Quick Start

This guide is the shortest path from a blank host to a running AsterGraph integration.

For first-time adopters, start on the default Avalonia path by default.
Treat `WPF` only as adapter-2 portability validation on the same canonical route; it is not a second route or a parity promise.
For the frozen support boundary and upgrade guidance toward `v1.0.0`, see [Stabilization Support Matrix](./stabilization-support-matrix.md).
If you are evaluating the public beta end to end, use [Beta Evaluation Path](./evaluation-path.md) as the hosted route ladder from first install to realistic hosted proof.

## 1. Pick Your Starting Package

| Host goal | Start package | Why |
| --- | --- | --- |
| Hosted starter scaffold | `AsterGraph.Starter.Avalonia` | smallest end-to-end Avalonia scaffold; the first hosted hop in the cookbook |
| Default Avalonia UI host | `AsterGraph.Avalonia` | main UI entry with the shipped shell and view factories |
| Runtime-only or custom UI host | `AsterGraph.Editor` | canonical session/runtime surface for custom UI or native shells |
| Contract-first integration | `AsterGraph.Abstractions` | stable identifiers, definitions, and provider contracts |

Add `AsterGraph.Core` only when the host also needs direct `GraphDocument`, serialization, or compatibility APIs.
Use `AsterGraph.Starter.Avalonia` as the starter recipe. Keep/copy `AsterGraphEditorFactory.Create(...)`, `AsterGraphAvaloniaViewFactory.Create(...)`, `AsterGraphEditorOptions`, and the document/catalog/editor/view composition flow. Replace the top-level window and its title/size, and replace the sample graph/catalog definitions as the host grows. Copy the host-owned seams, not the sample-owned presentation. The next hosted step is `AsterGraph.HelloWorld.Avalonia`. When you move to `AsterGraph.ConsumerSample.Avalonia`, keep action projection, trust workflow, and parameter-editing composition host-owned.

Copy this starter scaffold:

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphEditorOptions`
- the document/catalog/editor/view composition flow

Replace in your host:

- the top-level window and its title/size
- the sample graph/catalog definitions as the host grows


## 2. Install From NuGet

The public beta packages are published to nuget.org, so the default `dotnet` restore flow plus `--prerelease` is enough.

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

For the first hosted entry, run:

```powershell
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo
```

For the smallest possible runtime-only sample, run:

```powershell
dotnet run --project tools/AsterGraph.HelloWorld/AsterGraph.HelloWorld.csproj --nologo
```

For the smallest hosted-UI sample, run:

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo
```

For one realistic hosted integration with a host-owned action rail, parameter editing, and one trusted plugin, run:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

Run `AsterGraph.ConsumerSample.Avalonia -- --proof` first for proof handoff; use `HostSample` only after that as the post-ladder proof harness.

For plugin-capable evaluators, the defended hosted trust hop is `AsterGraph.ConsumerSample.Avalonia`. Read [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md) and [Plugin And Custom Node Recipe](./plugin-recipe.md) before treating the route as complete.

Use `Starter.Avalonia` when you want the first hosted entry and the smallest end-to-end Avalonia scaffold. Use `HelloWorld` when you want the simplest runtime-only starting point. Use `HelloWorld.Avalonia` when you want the smallest shipped-shell sample after the starter scaffold. Use `ConsumerSample.Avalonia` when you want one realistic host before jumping to `Demo`. Use `HostSample` only when you want a proof harness for canonical route validation, not as the onboarding step.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

The sample README is [`tools/AsterGraph.ConsumerSample.Avalonia/README.md`](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md).

## 4. Canonical Adoption Routes

| If your host needs | Start here | First sample |
| --- | --- | --- |
| Hosted starter scaffold | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.Starter.Avalonia` |
| Runtime-only or custom UI | `AsterGraphEditorFactory.CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| Shipped Avalonia UI | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HelloWorld.Avalonia` |
| Plugin trust/discovery | `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` + `AsterGraphEditorOptions.PluginTrustPolicy` | [`tools/AsterGraph.ConsumerSample.Avalonia`](../../tools/AsterGraph.ConsumerSample.Avalonia/) |
| Automation | `IGraphEditorSession.Automation.Execute(...)` | [Host Integration](./host-integration.md) |
| Retained compatibility bridge | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | [Host Integration](./host-integration.md) |

For new work, start with the runtime/session route or the shipped Avalonia route. Treat the retained route as migration-only.
Choose retained only when you are migrating an existing host in batches. Use the retained recipe only as a copyable migration aid for an existing host. If you need that bridge, use [Retained Migration Recipe](./retained-migration-recipe.md); otherwise start with `CreateSession(...)` or `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`.
If you are choosing retained, stop at that recipe instead of stitching together multiple docs; it is the single bounded retained recipe set for existing `GraphEditorViewModel` / `GraphEditorView` hosts.
New adopters should start with `AsterGraph.Starter.Avalonia` unless they are intentionally building a custom UI host from day one.
If the host owns its UI, the runtime/session route is the canonical native path; `Editor.Session` still owns host actions, diagnostics, automation, and proof logic.
Quick Start remains Avalonia-first today. The current public beta line validates `WPF` as adapter 2 on the same canonical route; see [Adapter Capability Matrix](./adapter-capability-matrix.md) for that contract instead of treating it as a second beginner route or a parity promise.

## 5. Minimal Hosted-UI Composition

```csharp
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;

INodeCatalog catalog = CreateCatalog();
var document = CreateDocument();

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
- [Stabilization Support Matrix](./stabilization-support-matrix.md) = frozen package/framework/adapter boundary plus `v1.0.0` upgrade guidance
- [Architecture](./architecture.md) = editor-kernel / scene-interaction / adapter split plus public stability levels
- [Adapter Capability Matrix](./adapter-capability-matrix.md) = locked `WPF` adapter-2 contract plus `Supported` / `Partial` / `Fallback`
- [Consumer Sample](./consumer-sample.md) = one realistic hosted integration between HelloWorld and Demo
- [Alpha Status](./alpha-status.md) = current scope, non-goals, and known limitations
- [Demo Guide](./demo-guide.md) = full showcase host
- [HostSample](../../tools/AsterGraph.HostSample/) = post-ladder proof harness for route validation, not the onboarding step
- [ScaleSmoke Baseline](./scale-baseline.md) = public graph-size tiers and defended redlines
- [Authoring Inspector Recipe](./authoring-inspector-recipe.md) = definition-driven parameters, groups, validation, and shipped inspector editors
- [Plugin And Custom Node Recipe](./plugin-recipe.md) = smallest copyable plugin/custom-node path
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md) = phased migration guide for older hosts

## 8. Maintainer And Source-Validation Paths

If you are validating the repository itself instead of consuming the published packages:

- maintainer workflow and lanes: [CONTRIBUTING.md](../../CONTRIBUTING.md)
- release sign-off and manual NuGet publish flow: [Public Launch Checklist](./public-launch-checklist.md)
- historical tags versus package versions: [Versioning](./versioning.md)
- proof harnesses: [`tools/AsterGraph.HostSample`](../../tools/AsterGraph.HostSample/), [`tools/AsterGraph.PackageSmoke`](../../tools/AsterGraph.PackageSmoke/), [`tools/AsterGraph.ScaleSmoke`](../../tools/AsterGraph.ScaleSmoke/)
- medium consumer sample: [`tools/AsterGraph.ConsumerSample.Avalonia`](../../tools/AsterGraph.ConsumerSample.Avalonia/)

