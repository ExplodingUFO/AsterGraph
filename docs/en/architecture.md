# Architecture

This document explains the platform skeleton that underpins the current public beta capability surface for `0.11.0-beta`.

## Split

AsterGraph is organized around three explicit layers:

1. `Editor Kernel`
   Owns document mutation, history, diagnostics, automation, plugin loading, and canonical runtime command/query/event behavior.
2. `Scene/Interaction`
   Projects adapter-neutral scene state and semantic interaction seams such as `GraphEditorSceneSnapshot`, `GraphEditorPointerInputRouter`, `GraphEditorSceneResizeHitTester`, and `GraphEditorPlatformServices`.
3. `UI Adapter`
   Owns concrete rendering, control composition, platform input plumbing, and host-shell chrome for one framework such as Avalonia.

The public host root stays on `CreateSession(...)` + `IGraphEditorSession`. Hosted Avalonia composition is built on top of that runtime instead of replacing it.

## Stability Levels

The public beta exposes different stability levels on purpose:

| Surface | Current stability | Notes |
| --- | --- | --- |
| `CreateSession(...)`, `IGraphEditorSession`, DTO/snapshot queries, diagnostics, automation, plugin inspection | canonical | new host-facing work should start here |
| `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | supported hosted adapter | first official `UI Adapter`; runtime ownership still belongs to `Editor.Session` |
| `GraphEditorViewModel`, `GraphEditorView` | compatibility | retained migration surfaces, explicitly labeled as advanced |
| internal scene/input/platform helpers | internal adapter seam | used to keep the adapter boundary thin, not a public contract yet |

## Adapter Boundary

The Avalonia package is the first official adapter, not the definition of the whole SDK.

- `AsterGraph.Editor` owns the runtime model and canonical host contracts.
- `AsterGraph.Avalonia` consumes shared `Scene/Interaction` seams and adds Avalonia-specific rendering and composition.
- `WPF` is the locked adapter 2 target for the current public beta line, and it must plug into the same `Editor Kernel` + `Scene/Interaction` shape instead of forking editor semantics.
- Avalonia/WPF gaps should be published through the [Adapter Capability Matrix](./adapter-capability-matrix.md) as `Supported`, `Partial`, or `Fallback`; those states are validation-only and not a parity promise.

## Layer Boundary Contract

The React Flow parity roadmap uses this dependency contract as the executable spine for v1 work. New code should follow these arrows unless an issue explicitly updates this contract first:

- `AsterGraph.Abstractions` owns host-facing identifiers, definitions, styling contracts, and plugin-neutral interfaces. It must not reference `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`, or adapter packages.
- `AsterGraph.Core -> Abstractions` owns persisted graph models, schema/version rules, serialization, and explicit migration helpers. It must not reference session state, rendering projection, `GraphEditorViewModel`, `NodeCanvas`, or any Avalonia type.
- `AsterGraph.Editor -> Core + Abstractions` owns `IGraphEditorSession`, commands, queries, events, runtime snapshots, layout services, history, validation, clipboard, export, plugin loading, and adapter-neutral scene/interaction seams. It must not reference `AsterGraph.Avalonia` or Avalonia packages.
- `AsterGraph.Avalonia -> Editor + Core` owns Avalonia controls, renderers, input coordinators, themes, hosted composition helpers, and visual adapters. It must consume runtime/session contracts and snapshots instead of owning document mutation semantics.
- Demo and Cookbook projects consume the public packages only to demonstrate host usage. They must not become the source of library behavior.

Public runtime contracts must stay free of Avalonia types and retained view-model types. Compatibility exceptions are tracked by exact symbol, not by namespace. Current exceptions are `GraphEditorSession(GraphEditorViewModel, ...)`, `GraphEditorViewModel`, `GraphEditorView`, retained hosted factory routes, and the #48-owned `IGraphEditorQueries.GetCompatibleTargets(...)` / `CompatiblePortTarget` MVVM shim. They exist only as migration bridges while new hosts start from `CreateSession(...)` and `IGraphEditorSession`.

## Official Capability Modules

The official capability modules sit above this platform skeleton. `Selection`, `History`, `Clipboard`, `Shortcut Policy`, `Layout`, `MiniMap`, `Stencil`, `Fragment Library`, `Export`, `Baseline Edge Authoring`, `Node Surface Authoring`, `Hierarchy Semantics`, `Composite Scope Authoring`, `Edge Semantics`, and `Edge Geometry Tooling` are public module names rooted in the canonical runtime/session contract, not alternate route names.

Use [Host Integration](./host-integration.md) for the module-to-seam matrix, [Feature Catalog](./feature-catalog.md) for governed feature records, [Adapter Capability Matrix](./adapter-capability-matrix.md) for the second-adapter contract, and [Quick Start](./quick-start.md) for the proof/sample map that shows which public entry point demonstrates each module first.

## Proof Ring

The platform skeleton is defended in three places:

- regression tests that verify canonical runtime contracts and retained compatibility boundaries
- CI lanes on Windows, Linux, and macOS running the canonical solution validation path
- proof artifacts and release summaries that surface contract markers before release

Read this together with [Quick Start](./quick-start.md) and [Host Integration](./host-integration.md).
