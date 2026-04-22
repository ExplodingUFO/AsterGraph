# Host Integration Guide

This guide expands the supported host routes without turning the public onboarding flow into maintainer proof documentation. The canonical route stays session-first/runtime-first; retained MVVM is only a compatibility bridge during migration.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

## Canonical Routes

1. Runtime-only or custom UI  
   `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
2. Shipped Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. Retained migration bridge
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

Routes 1 and 2 are the canonical surfaces for new work. Route 3 remains supported only as a retained compatibility bridge for legacy hosts during migration.
Choose retained only when you are migrating an existing host in batches. If you need that bridge, use [Retained Migration Recipe](./retained-migration-recipe.md); otherwise start with route 1 or 2.

For new adopters, default to route 2 (`AsterGraphAvaloniaViewFactory`) so WPF remains adapter-2 portability validation only, not a separate onboarding path or parity promise.

If the host owns its UI, route 1 is the canonical native/custom-UI path; you compose your own surface around the same session/runtime owner instead of introducing a second model.

Standalone Avalonia surfaces such as `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, and `AsterGraphMiniMapViewFactory` belong to route 2. They are composition details under the hosted-UI family, not a fourth primary route.

## When To Choose Retained

| Route | Choose this when | Do not use this when | Recipe |
| --- | --- | --- | --- |
| Runtime/session | Use `CreateSession(...)` when you are starting new work or own your UI. | Do not use this when you need the shipped Avalonia shell or a retained bridge. | [Quick Start](./quick-start.md) |
| Shipped Avalonia | Use `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` when you want the shipped Avalonia route. | Do not use this when you are preserving a legacy `GraphEditorViewModel`-based host. | [Quick Start](./quick-start.md) |
| Retained migration bridge | Use retained only when the host already constructs `GraphEditorViewModel` or `GraphEditorView` and you are migrating in batches. | Do not use this for new host work, a fourth primary route, or a WPF-specific runtime model. | [Retained-To-Session Migration Recipe](./retained-migration-recipe.md) |

The single bounded retained recipe is [Retained-To-Session Migration Recipe](./retained-migration-recipe.md).
Retained stays migration-only and does not add a new compatibility promise. Retained is not a fourth primary route. If the bridge is justified, send maintainers to that recipe instead of stitching together multiple docs.

## Consumer Route Matrix

| Need | Packages to start with | Canonical entry point | First sample |
| --- | --- | --- | --- |
| Runtime-only/custom UI | `AsterGraph.Editor` (+ `AsterGraph.Abstractions` when defining nodes) | `CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| Default Avalonia UI | `AsterGraph.Avalonia` | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HelloWorld.Avalonia` |
| Plugin trust/discovery | `AsterGraph.Editor` | `DiscoverPluginCandidates(...)` + `PluginTrustPolicy` | `tools/AsterGraph.ConsumerSample.Avalonia` |
| Automation | `AsterGraph.Editor` | `IGraphEditorSession.Automation.Execute(...)` | `src/AsterGraph.Demo` |
| Retained migration bridge | `AsterGraph.Editor` (+ `AsterGraph.Avalonia` when embedding `GraphEditorView`) | retained constructor path | migration-only legacy host |

If you are starting new work, begin with [Quick Start](./quick-start.md) and keep the retained bridge for legacy migration only.

## Sample Roles

- `AsterGraph.HelloWorld` = first-run sample for the runtime-only path
- `AsterGraph.HelloWorld.Avalonia` = first-run sample for the shipped Avalonia UI path
- `AsterGraph.Starter.Avalonia` = starter scaffold for the shipped Avalonia path
- `AsterGraph.ConsumerSample.Avalonia` = medium hosted-UI sample on the canonical route with host actions, parameter editing, and one trusted plugin
- `AsterGraph.HostSample` = narrow proof harness for the canonical runtime-only and hosted-UI routes
- `AsterGraph.PackageSmoke` = packed-package proof
- `AsterGraph.ScaleSmoke` = public scale baseline plus history/state proof
- `AsterGraph.Demo` = full showcase host for visual inspection

## Second Adapter Contract

The current public beta line locks `WPF` as adapter 2. Avalonia remains the only shipped hosted adapter today; the second-adapter milestone exists to validate portability on the same canonical route, not to add adapter-specific runtime APIs, a separate onboarding path, or a parity promise.

Use the [Adapter Capability Matrix](./adapter-capability-matrix.md) vocabulary consistently:

| Label | Meaning |
| --- | --- |
| `Supported` | the stock adapter surface is available on that adapter through the primary documented route |
| `Partial` | the capability stays on the same canonical route, but the adapter has an explicit scope limit or a missing stock projection; host remains on the canonical route and projects via session/query snapshots |
| `Fallback` | the host stays on the same canonical session/runtime seam and uses the lower-level documented path, sample, or proof harness; never retained-MVVM or adapter-specific runtime APIs |

Retained migration is not `Fallback`. It remains a compatibility bridge for legacy hosts.

- `WPF Partial` means the host stays on the canonical session/runtime route and fills the missing stock surface with host-owned projection from session/query snapshots; it is validation-only, not parity.
- `WPF Fallback` means the host still stays on the canonical session/runtime route and uses host-owned projection from documented proof/sample paths; it is validation-only, not parity.
- That documented fallback never switches to retained-MVVM or adapter-specific runtime APIs.

## Official Capability Modules

Treat the official capability modules as a host-facing map layered on top of the canonical routes, not as a second routing system.

| Module | Canonical seam | Hosted/UI note | First proof/sample anchor |
| --- | --- | --- | --- |
| `Selection` | `SetSelection(...)` + `GetSelectionSnapshot()` | route 2 projects the same selection state into the shipped visuals | `AsterGraph.ScaleSmoke`, `AsterGraph.HelloWorld` |
| `History` | `Undo()` / `Redo()` plus the save/dirty contract | hosted shells reuse the same kernel-owned history boundary | `AsterGraph.ScaleSmoke`, [State Contracts](./state-contracts.md) |
| `Clipboard` | `TryCopySelectionAsync()` / `TryPasteSelectionAsync()` | host clipboard services remain the seam underneath | `AsterGraph.HostSample` |
| `Shortcut Policy` | `AsterGraphCommandShortcutPolicy` | Avalonia-specific composition knob, but still part of the official hosted route | `AsterGraph.PackageSmoke`, `AsterGraph.HelloWorld.Avalonia` |
| `Layout` | session align/distribute commands | snaplines and visual guides stay adapter-owned | `AsterGraph.Demo` |
| `MiniMap` | session/viewport snapshots + `AsterGraphMiniMapViewFactory.Create(...)` | standalone surface under route 2, not a separate route | `AsterGraph.Demo` |
| `Stencil` | session stencil discovery + insertion commands | shipped Avalonia surface consumes the same session discovery data | `AsterGraph.Demo` |
| `Fragment Library` | session fragment/template commands backed by fragment workspace/library services | hosted shells can replace storage without replacing the command surface | `AsterGraph.Demo` |
| `Export` | `IGraphSceneSvgExportService` + `TryExportSceneAsSvg()` | export stays separate from workspace persistence and fragment storage | `AsterGraph.HostSample` |
| `Baseline Edge Authoring` | connection start/complete/reconnect/disconnect commands + pending snapshot | pointer gestures are adapter behavior layered on top of the same session semantics | `AsterGraph.HostSample`, `AsterGraph.ScaleSmoke` |
| `Node Surface Authoring` | `GetNodeSurfaceSnapshots()`, `TrySetNodeSize(...)`, and parameter edits through the shared session command path | Avalonia projects the same tier state into card thresholds, node-side parameter editors, and stock authoring chrome | `AsterGraph.Demo`, [Advanced Editing Guide](./advanced-editing.md) |
| `Hierarchy Semantics` | `GetHierarchyStateSnapshot()`, `GetNodeGroups()`, `GetNodeGroupSnapshots()`, and group collapse/move/resize/membership commands | the stock canvas keeps frame chrome, content-area membership, and collapse affordances on top of the same hierarchy state | `AsterGraph.Demo`, [Advanced Editing Guide](./advanced-editing.md) |
| `Composite Scope Authoring` | wrap/promote/expose/unexpose/scope-navigation commands plus scope/composite queries | breadcrumb chrome and host-owned workflow controls reuse the same session navigation state | `AsterGraph.Demo`, [Advanced Editing Guide](./advanced-editing.md) |
| `Edge Semantics` | connection note, reconnect, and disconnect commands on the canonical session route | hosted pointer flows and menus stay projections of the same edge semantics | `AsterGraph.Demo`, [Advanced Editing Guide](./advanced-editing.md) |
| `Edge Geometry Tooling` | `GetConnectionGeometrySnapshots()` plus route-vertex insert/move/remove commands | stock authoring tools project geometry editing without introducing a second edge model | `AsterGraph.Demo`, [Advanced Editing Guide](./advanced-editing.md) |

## State Contract

The host-facing save/history/dirty rules are published in [State Contracts](./state-contracts.md).

Short version:

- save establishes the clean baseline
- leaving the saved snapshot through undo makes the editor dirty
- returning to the saved snapshot through redo clears dirty again
- no-op interactions must not latch fake dirty or undo state
- retained and runtime mutations still share one kernel-owned history/save authority, but new integrations should still start on the canonical session route

## Export Versus Persistence

Treat these as three separate host seams:

- workspace persistence: `IGraphWorkspaceService` owns save/load of the full editable graph state using the canonical flow in [Serialization Contracts](./serialization-contracts.md)
- fragment persistence: fragment workspace + fragment library services own reusable selection payloads
- scene export: `IGraphSceneSvgExportService` owns non-workspace SVG output built from `IGraphEditorSession.Queries.GetSceneSnapshot()`

The shipped SVG export seam is intentionally separate from workspace save/load and does not replace fragment/template flows.

## Extension Contract

The stability and precedence rules are published in [Extension Contracts](./extension-contracts.md).

Important defaults:

- canonical runtime surfaces are `CreateSession(...)`, `IGraphEditorSession`, and DTO/snapshot queries
- `Create(...)` remains the supported hosted-Avalonia composition helper; `Editor.Session` is still the shared runtime owner behind that route
- retained `GraphEditorViewModel` / `GraphEditorView` remain supported migration facades and are explicitly labeled as advanced compatibility bridge surfaces
- host localization runs after plugin localization, so host override wins
- plugin-contributed commands now surface through the canonical session command descriptors and execute through `IGraphEditorSession.Commands.TryExecuteCommand(...)`
- retained host augmentor composition still differs from the runtime path; use the runtime path for new work and treat the retained route as a bridge only

## Plugin Trust Boundary

Plugin trust is host-owned. AsterGraph can help the host discover candidates, apply allow/block policy, and inspect outcomes, but it does not sandbox plugin code or isolate untrusted execution.

For the v1 manifest and trust-policy contract, see [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md).

For deeper proof, CI lanes, and release gates, use [CONTRIBUTING.md](../../CONTRIBUTING.md) and [Public Launch Checklist](./public-launch-checklist.md).

## Recipes

- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
