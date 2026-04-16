# Host Integration Guide

This guide expands the canonical host routes without introducing a second route tree.

## Canonical Routes

1. Runtime-only or custom UI  
   `AsterGraphEditorFactory.CreateSession(...)`
2. Shipped Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. Retained migration  
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

Standalone Avalonia surfaces such as `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, and `AsterGraphMiniMapViewFactory` belong to route 2. They are composition details under the canonical hosted-UI family, not a fourth primary route.

## Consumer Route Matrix

| Need | Packages to start with | Canonical entry point | Verify with |
| --- | --- | --- | --- |
| Runtime-only/custom UI | `AsterGraph.Abstractions`, `AsterGraph.Editor` | `CreateSession(...)` | packed `HostSample` |
| Default Avalonia UI | `AsterGraph.Avalonia` | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | packed `HostSample` |
| Plugin trust/discovery | `AsterGraph.Editor` | `DiscoverPluginCandidates(...)` + `PluginTrustPolicy` | `eng/ci.ps1 -Lane contract` |
| Automation | `AsterGraph.Editor` | `IGraphEditorSession.Automation.Execute(...)` | `eng/ci.ps1 -Lane contract` |
| Retained migration | `AsterGraph.Editor` (+ `AsterGraph.Avalonia` when embedding `GraphEditorView`) | retained constructor path | `eng/ci.ps1 -Lane contract` |

## Minimal Consumer Host

Use `tools/AsterGraph.HostSample` when you want the smallest runnable proof of:

- `CreateSession(...)`
- `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`

`HostSample` is intentionally narrow. It does not replace:

- `AsterGraph.PackageSmoke`
- `AsterGraph.ScaleSmoke`
- `AsterGraph.Demo`

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

- canonical surfaces are `CreateSession(...)`, `Create(...)`, `IGraphEditorSession`, and DTO/snapshot queries
- retained `GraphEditorViewModel` / `GraphEditorView` remain supported migration facades
- host localization runs after plugin localization, so host override wins
- runtime/session menu composition differs from retained host augmentor composition; use the runtime path for new work

## Release Verification

Preferred proof-ring entrypoints:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

Tool roles:

- `AsterGraph.HostSample` = minimal consumer proof
- `AsterGraph.PackageSmoke` = packed package consumption proof
- `AsterGraph.ScaleSmoke` = larger-graph and history/readiness proof
- `AsterGraph.Demo` = visual/manual showcase host
