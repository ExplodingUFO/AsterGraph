# AsterGraph

<p align="right">
  <img alt="English (current)" src="https://img.shields.io/badge/English-current-111827?style=flat-square" />
  <a href="./README.zh-CN.md"><img alt="简体中文" src="https://img.shields.io/badge/简体中文-switch-2563eb?style=flat-square" /></a>
</p>

AsterGraph is a modular node-graph editor toolkit for .NET. It gives hosts a reusable editor runtime, a shipped Avalonia UI path, and explicit seams for plugins, automation, localization, diagnostics, and presentation overrides.

## Public Alpha

- latest installable package version: `0.2.0-alpha.3`
- latest semver-aligned public prerelease tag: `v0.2.0-alpha.3`
- latest legacy repository milestone tag: `v1.9` (historical pre-launch checkpoint, not the NuGet version)
- published packages target `net8.0` and `net9.0`
- packed `HostSample` also proves downstream `.NET 10` consumption during the release gate
- future public prerelease tags should match package SemVer, for example `v0.2.0-alpha.3`
- package version versus historical repository-tag guidance: [Versioning](./docs/en/versioning.md)
- current scope, non-goals, and known limitations: [Alpha Status](./docs/en/alpha-status.md)

## Start Here

| I want to... | Start here | Why |
| --- | --- | --- |
| get the fastest first run | [`tools/AsterGraph.HelloWorld`](./tools/AsterGraph.HelloWorld/) | smallest runtime-only sample; one route, one command |
| embed the shipped Avalonia UI | [`tools/AsterGraph.HelloWorld.Avalonia`](./tools/AsterGraph.HelloWorld.Avalonia/) | smallest hosted-UI sample on the factory-based Avalonia route |
| try a realistic hosted integration | [Consumer Sample](./docs/en/consumer-sample.md) | medium sample with host-owned actions, parameter editing, and one trusted plugin |
| integrate into an existing host | [Host Integration](./docs/en/host-integration.md) | route matrix, package boundary, migration guidance |
| inspect the full surface visually | [Demo Guide](./docs/en/demo-guide.md) | showcase host for plugins, automation, localization, and standalone surfaces |
| validate packed consumption or maintain releases | [Contributing](./CONTRIBUTING.md) and [Public Launch Checklist](./docs/en/public-launch-checklist.md) | proof lanes, CI, and release flow |

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
| Runtime-only or custom UI | the host owns its own UI and wants the canonical runtime boundary | `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession` | [`AsterGraph.HelloWorld`](./tools/AsterGraph.HelloWorld/) |
| Shipped Avalonia UI | the host wants the stock editor shell or stock standalone Avalonia surfaces | `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | [Quick Start](./docs/en/quick-start.md) |
| Retained migration | the host is moving off older MVVM-shaped entry points in planned batches | `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }` | [Host Integration](./docs/en/host-integration.md) |

For new runtime-facing work, anchor on the first route. The Avalonia route is the supported hosted adapter path today, while the retained route stays migration-only.

## Public Entry Map

- [`tools/AsterGraph.HelloWorld`](./tools/AsterGraph.HelloWorld/) = first-run sample for the runtime-only path
- [`tools/AsterGraph.HelloWorld.Avalonia`](./tools/AsterGraph.HelloWorld.Avalonia/) = first-run sample for the hosted Avalonia UI path
- [`tools/AsterGraph.ConsumerSample.Avalonia`](./tools/AsterGraph.ConsumerSample.Avalonia/) = medium hosted-UI consumer sample with one custom node family, one host action rail, and one trusted plugin
- [`tools/AsterGraph.HostSample`](./tools/AsterGraph.HostSample/) = minimal canonical proof harness for runtime-only and hosted-UI validation
- [`tools/AsterGraph.PackageSmoke`](./tools/AsterGraph.PackageSmoke/) = packed-package proof
- [`tools/AsterGraph.ScaleSmoke`](./tools/AsterGraph.ScaleSmoke/) = public larger-graph baseline plus history/state proof
- [`src/AsterGraph.Demo`](./src/AsterGraph.Demo/) = showcase host; menu labels follow the current UI language

## Official Capability Modules

These are the public `v0.4.0-alpha` capability modules. They sit on top of the canonical session/runtime route; hosted Avalonia composition reuses the same seams instead of defining a second capability model.

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

It does **not** provide sandboxing or untrusted-code isolation. For public-alpha hosts, prefer fixed plugin directories, explicit allowlists, and your own signature or hash validation policy.

## Documentation

Consumer-facing guides:

- [Versioning](./docs/en/versioning.md)
- [Quick Start](./docs/en/quick-start.md)
- [Consumer Sample](./docs/en/consumer-sample.md)
- [Host Integration](./docs/en/host-integration.md)
- [ScaleSmoke Baseline](./docs/en/scale-baseline.md)
- [Authoring Inspector Recipe](./docs/en/authoring-inspector-recipe.md)
- [Adoption Feedback Loop](./docs/en/adoption-feedback.md)
- [Plugin And Custom Node Recipe](./docs/en/plugin-recipe.md)
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
