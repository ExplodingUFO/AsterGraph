---
phase: 33-kernel-canvas-and-guardrail-follow-through
validated: 2026-04-16
status: ready
---

# Phase 33 Validation Contract

## Requirements

- `FACADE-03`: contributors can change one hotspot collaborator at a time because the remaining responsibilities are split into narrower internal seams with focused tests
- `FACADE-04`: contributors can continue hotspot reduction in `GraphEditorKernel` and `NodeCanvas` without changing public embedding behavior because cross-cutting responsibilities are isolated behind internal coordinators or helpers
- `GUARD-02`: touched publishable packages stop extending blanket public XML-doc debt, using real docs or scoped suppressions instead of one repo-wide blanket

## Validation Strategy

### Kernel hotspot proof

- `GraphEditorKernelCommandRouterTests` should prove command routing still works and the kernel no longer owns the router-host contract directly.

### Canvas hotspot proof

- focused `NodeCanvas` lifecycle/standalone tests should prove property-change routing, platform seam binding, and standalone embed behavior still work after lifecycle extraction
- existing pointer/drag coordinator tests remain part of the maintenance surface

### Guardrail proof

- `AsterGraph.Abstractions` and `AsterGraph.Core` must still build with `CS1591` treated as an error
- `AsterGraph.Editor` and `AsterGraph.Avalonia` must build successfully with scoped suppression or clean docs after the repo-wide blanket is removed

### Refactor gate proof

- maintenance lane should stay green after the kernel/canvas hotspot updates

## Required Commands

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorKernelCommandRouterTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~NodeCanvasStandaloneTests|FullyQualifiedName~NodeCanvasNodeDragCoordinatorTests|FullyQualifiedName~NodeCanvasPointerInteractionCoordinatorTests" -v minimal
dotnet build src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release /warnaserror:CS1591 -v minimal
dotnet build src/AsterGraph.Core/AsterGraph.Core.csproj -c Release /warnaserror:CS1591 -v minimal
dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -v minimal
dotnet build src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -v minimal
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release
```

## Exit Criteria

- focused kernel tests pass
- focused canvas tests pass
- maintenance lane passes
- repo-wide `CS1591` blanket is gone
- clean packages prove they stay clean under `warnaserror:CS1591`
- remaining doc debt is explicit and scoped, not hidden at repo root
