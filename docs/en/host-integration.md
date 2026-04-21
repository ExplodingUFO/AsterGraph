# Host Integration Guide

This guide expands the supported host routes without turning the public onboarding flow into maintainer proof documentation.

## Canonical Routes

1. Runtime-only or custom UI  
   `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
2. Shipped Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. Retained migration  
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

Routes 1 and 2 are the canonical surfaces for new work. Route 3 remains supported only as a retained compatibility facade during migration.

If the host owns its UI, route 1 is the canonical native/custom-UI path; you compose your own surface around the same session/runtime owner instead of introducing a second model.

Standalone Avalonia surfaces such as `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, and `AsterGraphMiniMapViewFactory` belong to route 2. They are composition details under the hosted-UI family, not a fourth primary route.

## Consumer Route Matrix

| Need | Packages to start with | Canonical entry point | First sample |
| --- | --- | --- | --- |
| Runtime-only/custom UI | `AsterGraph.Editor` (+ `AsterGraph.Abstractions` when defining nodes) | `CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| Default Avalonia UI | `AsterGraph.Avalonia` | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HelloWorld.Avalonia` |
| Plugin trust/discovery | `AsterGraph.Editor` | `DiscoverPluginCandidates(...)` + `PluginTrustPolicy` | `tools/AsterGraph.ConsumerSample.Avalonia` |
| Automation | `AsterGraph.Editor` | `IGraphEditorSession.Automation.Execute(...)` | `src/AsterGraph.Demo` |
| Retained migration | `AsterGraph.Editor` (+ `AsterGraph.Avalonia` when embedding `GraphEditorView`) | retained constructor path | migration-only |

## Sample Roles

- `AsterGraph.HelloWorld` = first-run sample for the runtime-only path
- `AsterGraph.HelloWorld.Avalonia` = first-run sample for the shipped Avalonia UI path
- `AsterGraph.ConsumerSample.Avalonia` = medium hosted-UI sample on the canonical route with host actions, parameter editing, and one trusted plugin
- `AsterGraph.HostSample` = narrow proof harness for the canonical runtime-only and hosted-UI routes
- `AsterGraph.PackageSmoke` = packed-package proof
- `AsterGraph.ScaleSmoke` = public scale baseline plus history/state proof
- `AsterGraph.Demo` = full showcase host for visual inspection

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
- retained and runtime mutations still share one kernel-owned history/save authority

## Export Versus Persistence

Treat these as three separate host seams:

- workspace persistence: `IGraphWorkspaceService` owns save/load of the full editable graph state
- fragment persistence: fragment workspace + fragment library services own reusable selection payloads
- scene export: `IGraphSceneSvgExportService` owns non-workspace SVG output built from `IGraphEditorSession.Queries.GetSceneSnapshot()`

The shipped SVG export seam is intentionally separate from workspace save/load and does not replace fragment/template flows.

## Extension Contract

The stability and precedence rules are published in [Extension Contracts](./extension-contracts.md).

Important defaults:

- canonical runtime surfaces are `CreateSession(...)`, `IGraphEditorSession`, and DTO/snapshot queries
- `Create(...)` remains the supported hosted-Avalonia composition helper; `Editor.Session` is still the shared runtime owner behind that route
- retained `GraphEditorViewModel` / `GraphEditorView` remain supported migration facades and are explicitly labeled as advanced compatibility surfaces
- host localization runs after plugin localization, so host override wins
- plugin-contributed commands now surface through the canonical session command descriptors and execute through `IGraphEditorSession.Commands.TryExecuteCommand(...)`
- retained host augmentor composition still differs from the runtime path; use the runtime path for new work

## Plugin Trust Boundary

Plugin trust is host-owned. AsterGraph can help the host discover candidates, apply allow/block policy, and inspect outcomes, but it does not sandbox plugin code or isolate untrusted execution.

For deeper proof, CI lanes, and release gates, use [CONTRIBUTING.md](../../CONTRIBUTING.md) and [Public Launch Checklist](./public-launch-checklist.md).

## Recipes

- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
