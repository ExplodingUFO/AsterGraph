# Host Integration Guide

This guide expands the supported host routes without turning the public onboarding flow into maintainer proof documentation.

## Canonical Routes

1. Runtime-only or custom UI  
   `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
2. Shipped Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. Retained migration  
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

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
- `AsterGraph.ConsumerSample.Avalonia` = medium hosted-UI sample with host actions, parameter editing, and one trusted plugin
- `AsterGraph.HostSample` = narrow proof harness for the canonical runtime-only and hosted-UI routes
- `AsterGraph.PackageSmoke` = packed-package proof
- `AsterGraph.ScaleSmoke` = public scale baseline plus history/state proof
- `AsterGraph.Demo` = full showcase host for visual inspection

## State Contract

The host-facing save/history/dirty rules are published in [State Contracts](./state-contracts.md).

Short version:

- save establishes the clean baseline
- leaving the saved snapshot through undo makes the editor dirty
- returning to the saved snapshot through redo clears dirty again
- no-op interactions must not latch fake dirty or undo state
- retained and runtime mutations still share one kernel-owned history/save authority

## Extension Contract

The stability and precedence rules are published in [Extension Contracts](./extension-contracts.md).

Important defaults:

- canonical runtime surfaces are `CreateSession(...)`, `IGraphEditorSession`, and DTO/snapshot queries
- `Create(...)` remains the supported hosted-Avalonia composition helper and returns the retained editor facade
- retained `GraphEditorViewModel` / `GraphEditorView` remain supported migration facades
- host localization runs after plugin localization, so host override wins
- plugin-contributed commands now surface through the canonical session command descriptors and execute through `IGraphEditorSession.Commands.TryExecuteCommand(...)`
- retained host augmentor composition still differs from the runtime path; use the runtime path for new work

## Plugin Trust Boundary

Plugin trust is host-owned. AsterGraph can help the host discover candidates, apply allow/block policy, and inspect outcomes, but it does not sandbox plugin code or isolate untrusted execution.

For deeper proof, CI lanes, and release gates, use [CONTRIBUTING.md](../../CONTRIBUTING.md) and [Public Launch Checklist](./public-launch-checklist.md).

## Recipes

- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
