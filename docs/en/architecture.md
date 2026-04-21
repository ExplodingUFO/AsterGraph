# Architecture

This document explains the platform skeleton that `v0.3.0-alpha` is freezing before broader capability work starts.

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

The public alpha exposes different stability levels on purpose:

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
- future adapters should plug into the same `Editor Kernel` + `Scene/Interaction` shape instead of forking editor semantics.

## Official Capability Modules

The official capability modules sit above this platform skeleton. `Selection`, `History`, `Clipboard`, `Shortcut Policy`, `Layout`, `MiniMap`, `Stencil`, `Fragment Library`, `Export`, and `Baseline Edge Authoring` are public module names rooted in the canonical runtime/session contract, not alternate route names.

Use [Host Integration](./host-integration.md) for the module-to-seam matrix and [Quick Start](./quick-start.md) for the proof/sample map that shows which public entry point demonstrates each module first.

## Proof Ring

The platform skeleton is defended in three places:

- regression tests that verify canonical runtime contracts and retained compatibility boundaries
- CI lanes on Windows, Linux, and macOS running the canonical solution validation path
- proof artifacts and release summaries that surface contract markers before release

Read this together with [Quick Start](./quick-start.md) and [Host Integration](./host-integration.md).
