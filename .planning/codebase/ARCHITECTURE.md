# Architecture

**Analysis Date:** 2026-04-14

## Pattern Overview

- AsterGraph is a layered .NET SDK with a framework-neutral contract layer, immutable graph models, an editor runtime layer, and an Avalonia adapter layer.
- The architectural direction has shifted from a `GraphEditorViewModel`-centered design toward a kernel-first runtime centered on `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` and `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`.
- That shift is only partially complete: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` is still the retained compatibility facade and still implements `IGraphEditorSessionHost`.

## Current Architecture State

- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` now provides two composition paths:
  - `CreateSession(...)` for the kernel-first runtime path.
  - `Create(...)` for the compatibility `GraphEditorViewModel` path.
- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` owns canonical runtime state for the session-first path.
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` exposes commands, queries, events, diagnostics, and mutation batching over an internal `IGraphEditorSessionHost`.
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` still acts as a large adapter and compatibility entry point for legacy hosts and Avalonia surfaces.
- The repo's `.planning/STATE.md` and `.planning/ROADMAP.md` track `.planning` reality at v1.5: runtime-boundary cleanup is completed in Phase 26, proof-surface alignment is completed in Phase 28, and Phase 29 is next.

## Layers

### Contract Layer

- Location: `src/AsterGraph.Abstractions`
- Responsibility: stable identifiers, definitions, compatibility contracts, catalogs, and style tokens.
- Examples: `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Abstractions/Compatibility/IPortCompatibilityService.cs`, `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`, `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`
- Dependencies: BCL only.

### Core Model And Persistence Layer

- Location: `src/AsterGraph.Core`
- Responsibility: immutable graph records, default compatibility policy, and JSON persistence.
- Examples: `src/AsterGraph.Core/Models/GraphDocument.cs`, `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`, `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`
- Dependencies: `src/AsterGraph.Abstractions`

### Editor Kernel And Runtime Layer

- Location: `src/AsterGraph.Editor/Kernel` and `src/AsterGraph.Editor/Runtime`
- Responsibility: runtime state ownership, command/query/event contracts, diagnostics snapshots, and mutation orchestration.
- Examples: `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`, `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`, `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs`, `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs`, `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`, `src/AsterGraph.Editor/Runtime/IGraphEditorEvents.cs`
- Dependencies: `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `CommunityToolkit.Mvvm`, `Microsoft.Extensions.Logging.Abstractions`

### Compatibility Facade And Editor Services Layer

- Location: `src/AsterGraph.Editor/ViewModels`, `src/AsterGraph.Editor/Services`, `src/AsterGraph.Editor/Menus`, `src/AsterGraph.Editor/Presentation`, `src/AsterGraph.Editor/Configuration`
- Responsibility: retained `GraphEditorViewModel` API, MVVM projections, menu building, clipboard/file services, configuration objects, and migration shims.
- Examples: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`, `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`, `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`

### Avalonia Adapter Layer

- Location: `src/AsterGraph.Avalonia`
- Responsibility: control composition, canvas interaction, view factories, style adaptation, menu presentation, and Avalonia-specific host-context/clipboard bridging.
- Examples: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`, `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs`, `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`, `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`

### Host, Demo, And Proof Layer

- Location: `src/AsterGraph.Demo`, `tools/AsterGraph.PackageSmoke`, `tools/AsterGraph.ScaleSmoke`
- Responsibility: demonstrate supported composition paths and prove package/runtime behavior.
- Examples: `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`, `tools/AsterGraph.PackageSmoke/Program.cs`, `tools/AsterGraph.ScaleSmoke/Program.cs`

## Data Flow

1. A host creates a `GraphDocument` from `src/AsterGraph.Core/Models/GraphDocument.cs` and a catalog through `src/AsterGraph.Editor/Catalog/NodeCatalog.cs` or another `INodeCatalog` implementation.
2. The host composes runtime options through `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs`.
3. `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` creates default storage/services, then composes either:
   - a kernel-first `IGraphEditorSession`, or
   - a retained `GraphEditorViewModel` with a `Session` property.
4. Runtime mutations flow through `IGraphEditorCommands`; detached state reads flow through `IGraphEditorQueries`; events and diagnostics flow through `IGraphEditorEvents` and `IGraphEditorDiagnostics`.
5. Avalonia factories and controls consume the editor/view-model layer to render nodes, connections, inspector state, menus, and minimap visuals.
6. Save/load and fragment flows pass through editor services into `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.

## Key Abstractions

### Node Catalog

- `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`
- `src/AsterGraph.Editor/Catalog/NodeCatalog.cs`
- Host-owned definitions are registered here before runtime composition.

### Kernel-Owned Runtime

- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`
- This is the canonical state owner for the kernel-first runtime path introduced in v1.2/Phase 13; v1.5 further tightened the boundary and migration posture.

### Compatibility Facade

- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- This remains the main migration-safe facade for existing hosts and the default binding target for the shipped Avalonia shell.

### Diagnostics And Inspection

- `src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs`
- `src/AsterGraph.Editor/Diagnostics/GraphEditorDiagnostic.cs`
- `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnosticsSink.cs`
- These keep support tooling and inspection data in the editor package, not in Avalonia controls.

### Menu And Presentation Descriptors

- `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`
- `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorCompatiblePortTargetSnapshot.cs`
- `src/AsterGraph.Avalonia/Presentation/*`
- Editor emits framework-neutral menu and presentation data; Avalonia renders it.

## Entry Points

- `src/AsterGraph.Demo/Program.cs` starts the demo app.
- `src/AsterGraph.Demo/App.axaml.cs` composes the main window.
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` is the main non-demo composition root.
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs` is the main full-shell UI composition root.
- `src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewFactory.cs`, `src/AsterGraph.Avalonia/Hosting/AsterGraphInspectorViewFactory.cs`, and `src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewFactory.cs` expose standalone surfaces.
- `src/AsterGraph.Demo/Program.cs` is the runnable visual sample integration path.
- `tools/AsterGraph.PackageSmoke/Program.cs` and `tools/AsterGraph.ScaleSmoke/Program.cs` are the machine-checkable proof entry points.

## Error Handling

- Leaf services and serializers fail fast on invalid inputs using explicit exceptions.
- Host-driven runtime failures are surfaced as recoverable diagnostics and status updates rather than always crashing the host.
- Menu/presentation augmentation is treated as an extension seam and guarded in editor code so stock behavior can remain available when host callbacks fail.
- Compatibility and command-permission checks are centralized in editor/runtime logic rather than scattered across controls.

## Cross-Cutting Concerns

- Styling stays framework-neutral in `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`.
- Behavior and permission policy stay in `src/AsterGraph.Editor/Configuration/*.cs`.
- Diagnostics and tracing stay in `src/AsterGraph.Editor/Diagnostics/*.cs`.
- Avalonia-specific concerns stay in `src/AsterGraph.Avalonia`.
- The main architectural risk remains dual-path drift between the kernel-first runtime and the retained compatibility facade.

---

*Architecture analysis refreshed: 2026-04-14*
