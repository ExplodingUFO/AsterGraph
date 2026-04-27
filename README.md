# AsterGraph

<p align="right">
  <img alt="English (current)" src="https://img.shields.io/badge/English-current-111827?style=flat-square" />
  <a href="./README.zh-CN.md"><img alt="简体中文" src="https://img.shields.io/badge/简体中文-switch-2563eb?style=flat-square" /></a>
</p>

AsterGraph is a modular node-graph editor toolkit for .NET. It gives hosts a reusable editor runtime, a canonical session/runtime route for custom UI or native shells, a shipped Avalonia hosted-UI path, and explicit seams for plugins, automation, localization, diagnostics, and presentation overrides.

![AsterGraph AI workflow scenario](./docs/assets/astergraph-ai-pipeline-demo.svg)

Launch the prebuilt AI workflow scenario:

```powershell
dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline
```

The scenario shows the SDK as an embeddable authoring surface: drag definition-backed nodes, connect typed ports, edit grouped parameters, inspect trusted plugin context, run host automation, then save or export the graph through the same session/runtime APIs a host uses.

## Evaluator Ladder

| Time | Do this | Outcome |
| --- | --- | --- |
| 30 seconds | Look at the AI workflow scenario above and run `src/AsterGraph.Demo -- --scenario ai-pipeline` when you want the visual tour. | You can tell what kind of graph editor SDK this is before reading maintainer proof. |
| 5 minutes | Generate `dotnet new astergraph-avalonia`, run the starter, then validate `ConsumerSample.Avalonia -- --proof --support-bundle <path>`. | You have a copyable hosted route with host actions, selected-node parameters, trusted plugin evidence, and a local support bundle. |
| 30 minutes | Read [Quick Start](./docs/en/quick-start.md), [Consumer Sample](./tools/AsterGraph.ConsumerSample.Avalonia/README.md), and [Host Integration](./docs/en/host-integration.md). | You can choose hosted UI, runtime-only, plugin, or migration routes without changing the runtime model. |

Maintainer and advanced proof details stay under [Public Launch Checklist](./docs/en/public-launch-checklist.md), [Adapter Capability Matrix](./docs/en/adapter-capability-matrix.md), and [Beta Support Bundle](./docs/en/support-bundle.md).

Generate a native Avalonia host or a plugin starter:

```powershell
dotnet new install ./templates
dotnet new astergraph-avalonia -n MyGraphHost
dotnet new astergraph-plugin -n MyGraphPlugin --PluginId my.graph.plugin
dotnet run --project tools/AsterGraph.PluginTool -- validate ./MyGraphPlugin/bin/Debug/net8.0/MyGraphPlugin.dll
```

## Start Here

| I want to... | Start here | Why |
| --- | --- | --- |
| get the first hosted entry | [`tools/AsterGraph.Starter.Avalonia`](./tools/AsterGraph.Starter.Avalonia/) | smallest end-to-end Avalonia scaffold; the first hosted hop in the cookbook |
| generate a native hosted app | [`templates/astergraph-avalonia`](./templates/astergraph-avalonia/) | `dotnet new` scaffold for a cross-platform Avalonia host |
| generate a plugin starter | [`templates/astergraph-plugin`](./templates/astergraph-plugin/) | `dotnet new` scaffold for a trusted in-process plugin |
| get the fastest runtime-only first run | [`tools/AsterGraph.HelloWorld`](./tools/AsterGraph.HelloWorld/) | smallest runtime-only sample; one canonical route for custom UI or native shells |
| embed the shipped Avalonia UI | [`tools/AsterGraph.HelloWorld.Avalonia`](./tools/AsterGraph.HelloWorld.Avalonia/) | smallest stock hosted-UI sample after the starter scaffold |
| try a realistic hosted integration | [Consumer Sample](./tools/AsterGraph.ConsumerSample.Avalonia/README.md) | medium sample on the same canonical route, with host-owned actions, parameter editing, and one trusted plugin |
| integrate into an existing host | [Host Integration](./docs/en/host-integration.md) | route matrix, package boundary, migration guidance |
| inspect the full surface visually | [Demo Guide](./docs/en/demo-guide.md) | showcase host for plugins, automation, localization, and standalone surfaces |
| validate packed consumption or maintain releases | [Contributing](./CONTRIBUTING.md) and [Public Launch Checklist](./docs/en/public-launch-checklist.md) | proof lanes, CI, and release flow |

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.
For a five-minute hosted copy path, run the starter scaffold first, then validate `ConsumerSample.Avalonia -- --proof --support-bundle <path>` before moving to the full Demo.
For the shortest hosted composition code, use `AsterGraphHostBuilder.Create().UseDocument(document).UseCatalog(catalog).UseDefaultCompatibility().BuildAvaloniaView()`; drop down to `AsterGraphEditorFactory.Create(...)` and `AsterGraphAvaloniaViewFactory.Create(...)` when you need explicit per-service wiring.

## Public Beta

- current installable package version: `0.11.0-beta`
- matching public prerelease tag for this package line: `v0.11.0-beta`
- historical legacy repository milestone tag series: `v1.x`-style pre-launch checkpoints (historical pre-launch checkpoints, not the NuGet version)
- GitHub prerelease/Release entries must use the same SemVer as the NuGet packages; local planning milestones are not public release identifiers
- published packages target `net8.0` and `net9.0`
- packed `HostSample` also proves downstream `.NET 10` consumption during the release gate
- public prerelease tags must match the package version exactly for this package line, for example `v0.11.0-beta`
- package version versus historical repository-tag guidance: [Versioning](./docs/en/versioning.md)
- frozen support boundary and `v1.0.0` upgrade guidance: [Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)
- evaluator ladder from first install to realistic hosted proof: [Beta Evaluation Path](./docs/en/evaluation-path.md)
- current scope, non-goals, and known limitations: [Alpha Status](./docs/en/alpha-status.md)

## Install From NuGet

Most new hosts start with one of these commands:

```powershell
# shipped Avalonia UI host
dotnet add package AsterGraph.Avalonia --prerelease

# runtime-only or custom UI host
dotnet add package AsterGraph.Editor --prerelease

# node definitions, identifiers, and provider contracts
dotnet add package AsterGraph.Abstractions --prerelease
```

Add `AsterGraph.Core` only when the host also needs direct `GraphDocument`, serialization, or compatibility APIs.

## Choose An Integration Route

| Route | Use when | First API | First sample |
| --- | --- | --- | --- |
| Hosted starter scaffold | the host wants the smallest end-to-end Avalonia entry before a fuller app | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | [`AsterGraph.Starter.Avalonia`](./tools/AsterGraph.Starter.Avalonia/) |
| Thin hosted builder | the host wants the common Avalonia route with less composition boilerplate | `AsterGraphHostBuilder.Create(...).BuildAvaloniaView()` | [`AsterGraph.Starter.Avalonia`](./tools/AsterGraph.Starter.Avalonia/) |
| Runtime-only or custom UI | the host owns its own UI and wants the canonical runtime boundary | `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession` | [`AsterGraph.HelloWorld`](./tools/AsterGraph.HelloWorld/) |
| Shipped Avalonia UI | the host wants the stock editor shell or stock standalone Avalonia surfaces | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | [`AsterGraph.HelloWorld.Avalonia`](./tools/AsterGraph.HelloWorld.Avalonia/) |
| Retained migration | the host is moving off older MVVM-shaped entry points in planned batches | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | [Host Integration](./docs/en/host-integration.md) |

For new runtime-facing work, anchor on the first route. The Avalonia route is the supported hosted adapter path today, while the retained route stays migration-only. The current public beta keeps `WPF` as adapter 2 and publishes Avalonia/WPF gaps through the [Adapter Capability Matrix](./docs/en/adapter-capability-matrix.md) instead of adding adapter-specific runtime APIs or claiming parity.

## Public Entry Map

- [`tools/AsterGraph.Starter.Avalonia`](./tools/AsterGraph.Starter.Avalonia/) = first hosted scaffold; the smallest end-to-end Avalonia entry
- [`tools/AsterGraph.HelloWorld`](./tools/AsterGraph.HelloWorld/) = first-run sample for the runtime-only path
- [`tools/AsterGraph.HelloWorld.Avalonia`](./tools/AsterGraph.HelloWorld.Avalonia/) = smallest stock hosted-UI sample after the starter scaffold
- [`tools/AsterGraph.ConsumerSample.Avalonia`](./tools/AsterGraph.ConsumerSample.Avalonia/README.md) = realistic hosted-UI consumer sample between `HelloWorld.Avalonia` and `Demo`
- [`tools/AsterGraph.HostSample`](./tools/AsterGraph.HostSample/) = proof harness for runtime-only and hosted-UI validation, not the onboarding step
- [`tools/AsterGraph.PackageSmoke`](./tools/AsterGraph.PackageSmoke/) = packed-package proof
- [`tools/AsterGraph.ScaleSmoke`](./tools/AsterGraph.ScaleSmoke/) = defended baseline/large proof, partially defended 5000-node stress proof, and history/state proof
- [`tools/AsterGraph.PluginTool`](./tools/AsterGraph.PluginTool/) = cross-platform CLI for plugin validation, trust evidence, and hash inspection
- [`templates/astergraph-avalonia`](./templates/astergraph-avalonia/) = `dotnet new astergraph-avalonia` native Avalonia starter
- [`templates/astergraph-plugin`](./templates/astergraph-plugin/) = `dotnet new astergraph-plugin` trusted plugin starter
- [`src/AsterGraph.Demo`](./src/AsterGraph.Demo/) = showcase host; menu labels follow the current UI language

## Official Capability Modules

These are the current public capability modules. They sit on top of the canonical session/runtime route; hosted Avalonia composition reuses the same seams instead of defining a second capability model.

| Module | Canonical seam | First proof/sample anchor |
| --- | --- | --- |
| `Selection` | `IGraphEditorSession.Commands.SetSelection(...)` + `Queries.GetSelectionSnapshot()` | `tools/AsterGraph.ScaleSmoke`, `tools/AsterGraph.HelloWorld` |
| `History` | `IGraphEditorSession.Commands.Undo()` / `Redo()` plus the save/dirty contract | `tools/AsterGraph.ScaleSmoke`, [State Contracts](./docs/en/state-contracts.md) |
| `Clipboard` | `TryCopySelectionAsync()` / `TryPasteSelectionAsync()` through host clipboard services | `tools/AsterGraph.HostSample` |
| `Shortcut Policy` | `AsterGraphCommandShortcutPolicy` on the hosted Avalonia route | `tools/AsterGraph.PackageSmoke`, `tools/AsterGraph.HelloWorld.Avalonia` |
| `Layout` | session-backed align/distribute commands | `src/AsterGraph.Demo` |
| `MiniMap` | session snapshots projected into `AsterGraphMiniMapViewFactory.Create(...)` | `src/AsterGraph.Demo` |
| `Stencil` | session stencil queries plus the shipped Avalonia insertion surface | `src/AsterGraph.Demo` |
| `Fragment Library` | session fragment/template commands backed by fragment workspace/library services | `src/AsterGraph.Demo` |
| `Export` | `IGraphSceneSvgExportService` + `TryExportSceneAsSvg()` | `tools/AsterGraph.HostSample` |
| `Baseline Edge Authoring` | `StartConnection(...)`, `CompleteConnection(...)`, reconnect/disconnect commands, and the pending-connection snapshot | `tools/AsterGraph.HostSample`, `tools/AsterGraph.ScaleSmoke` |
| `Node Surface Authoring` | `GetNodeSurfaceSnapshots()`, `TrySetNodeSize(...)`, and parameter edits through the shared session command path | `src/AsterGraph.Demo`, [Advanced Editing Guide](./docs/en/advanced-editing.md) |
| `Hierarchy Semantics` | `GetHierarchyStateSnapshot()`, `GetNodeGroups()`, `GetNodeGroupSnapshots()`, and group collapse/move/resize/membership commands | `src/AsterGraph.Demo`, [Advanced Editing Guide](./docs/en/advanced-editing.md) |
| `Composite Scope Authoring` | wrap/promote/expose/unexpose/scope-navigation commands plus scope/composite queries | `src/AsterGraph.Demo`, [Advanced Editing Guide](./docs/en/advanced-editing.md) |
| `Edge Semantics` | connection note, reconnect, and disconnect commands on the canonical session route | `src/AsterGraph.Demo`, [Advanced Editing Guide](./docs/en/advanced-editing.md) |
| `Edge Geometry Tooling` | `GetConnectionGeometrySnapshots()` plus route-vertex insert/move/remove commands | `src/AsterGraph.Demo`, [Advanced Editing Guide](./docs/en/advanced-editing.md) |

## Supported Package Boundary

Only these four libraries are published as the supported SDK surface:

| Package | Reference when | Notes |
| --- | --- | --- |
| `AsterGraph.Abstractions` | defining nodes, identifiers, catalogs, style tokens, or provider contracts | stable contract layer with no UI dependency |
| `AsterGraph.Core` | working directly with `GraphDocument`, serialization, or compatibility services | model and persistence layer |
| `AsterGraph.Editor` | building or extending an editor runtime or runtime session | canonical host-facing runtime package |
| `AsterGraph.Avalonia` | embedding the shipped Avalonia UI | default UI entry package for hosts |

`AsterGraph.Demo` is sample-only and is not part of the supported package boundary.

## Current Alpha Scope

Current capabilities:

- draggable, selectable graph nodes with marquee selection and multi-selection
- zoom, pan, mini map, and pending-connection preview
- save/load, undo/redo, copy/paste, fragment import/export, and SVG scene export
- batch alignment, distribution, and shared-parameter editing for compatible selections
- definition-driven inspector metadata with grouped parameters, shipped list/text/number/bool/enum editors, and validation feedback
- tiered node surfaces, fixed group frames, hierarchy snapshots, scoped composites, connection notes, and routed edge geometry editing
- compile-time node-definition registration and runtime plugin registration
- host-governed plugin trust policy, local candidate discovery, and load-state inspection
- descriptor-first automation through `IGraphEditorSession.Automation`
- runtime diagnostics, inspection snapshots, and replaceable host services

Current non-goals:

- algorithm execution engine
- arbitrary host-agnostic property editor framework beyond the shipped definition-driven inspector
- plugin marketplace or remote install/update workflows
- plugin unload lifecycle
- process sandboxing or untrusted-code isolation guarantees
- dedicated scripting language or workflow-designer UI for automation authoring

## Plugin Trust Boundary

Plugin loading is in-process. AsterGraph currently gives hosts:

- allow/block trust policy through `PluginTrustPolicy`
- local candidate discovery before activation
- runtime inspection of trusted, loaded, and blocked outcomes

It does **not** provide sandboxing or untrusted-code isolation. For public prerelease hosts, prefer fixed plugin directories, explicit allowlists, and your own signature or hash validation policy.

## Documentation

Consumer-facing guides:

- [Versioning](./docs/en/versioning.md)
- [Quick Start](./docs/en/quick-start.md)
- [Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)
- [Consumer Sample](./docs/en/consumer-sample.md)
- [Host Integration](./docs/en/host-integration.md)
- [Adapter Capability Matrix](./docs/en/adapter-capability-matrix.md)
- [Advanced Editing Guide](./docs/en/advanced-editing.md)
- [ScaleSmoke Baseline](./docs/en/scale-baseline.md)
- [Authoring Inspector Recipe](./docs/en/authoring-inspector-recipe.md)
- [Adoption Feedback Loop](./docs/en/adoption-feedback.md)
- [Plugin And Custom Node Recipe](./docs/en/plugin-recipe.md)
- [Plugin Host Recipe](./docs/en/plugin-host-recipe.md)
- [Custom Node Host Recipe](./docs/en/custom-node-host-recipe.md)
- [Host Recipe Ladder](./docs/en/host-recipe-ladder.md)
- [Retained-To-Session Migration Recipe](./docs/en/retained-migration-recipe.md)
- [State Contracts](./docs/en/state-contracts.md)
- [Extension Contracts](./docs/en/extension-contracts.md)
- [Alpha Status](./docs/en/alpha-status.md)
- [Project Status](./docs/en/project-status.md)
- [Demo Guide](./docs/en/demo-guide.md)

Chinese equivalents live under [docs/zh-CN/](./docs/zh-CN/).

## Contributors And Maintainers

- contributor workflow: [CONTRIBUTING.md](./CONTRIBUTING.md)
- code of conduct: [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md)
- security reporting: [SECURITY.md](./SECURITY.md)
- public launch and release sign-off: [Public Launch Checklist](./docs/en/public-launch-checklist.md)
