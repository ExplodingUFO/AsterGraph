---
status: passed
phase: 451
bead: avalonia-node-map-y7i.1
updated: 2026-04-30
---

# Phase 451 Context

## Goal

Map the current canvas engine, viewport, interaction, routing, grouping, layout, demo, and proof seams before implementing v0.76 professional canvas work.

This phase is audit-only. It does not introduce a runtime rewrite, a second renderer, compatibility layers, fallback behavior, or speculative contracts.

## Current Architecture

### Runtime and Projection

- `src/AsterGraph.Editor/Runtime/GraphEditorSessionSnapshot.cs` is the runtime snapshot source for scene state.
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` exposes query seams used by hosts and tests.
- `src/AsterGraph.Editor/Services/GraphEditorKernelProjectionApplier.cs` applies runtime projection state into the view model layer.
- `src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.CanvasSurface.cs` exposes canvas-facing state without making the Avalonia control own document semantics.

### Avalonia Canvas

- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` is the control entry point.
- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasSceneHost.cs` owns the scene layers, node/group presenters, coordinate root transform, and connection rendering calls.
- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasConnectionSceneRenderer.cs` creates connection visuals from geometry snapshots.
- `src/AsterGraph.Avalonia/Controls/Internal/Observation/NodeCanvasViewModelObserver.cs` maps view model changes into host updates and invalidation.

### Viewport and Visibility

- `src/AsterGraph.Editor/Viewport/ViewportVisibleSceneProjection.cs` computes visible node/group/connection counts and world bounds.
- `src/AsterGraph.Editor/Viewport/ViewportMath.cs` holds world/screen conversion helpers.
- `src/AsterGraph.Editor/Kernel/Internal/Viewport/GraphEditorKernelViewportCoordinator.cs` owns viewport mutation at kernel level.
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs` is the minimap surface and alternate viewport control.

### Interaction, Selection, and Connections

- `src/AsterGraph.Editor/Scene/GraphEditorPointerInputRouter.cs` classifies pointer routes.
- `src/AsterGraph.Avalonia/Controls/Internal/Interaction/NodeCanvasPointerInteractionCoordinator.cs` coordinates pointer sessions.
- `src/AsterGraph.Avalonia/Controls/Internal/Interaction/NodeCanvasNodeDragCoordinator.cs` handles node drag behavior.
- `src/AsterGraph.Editor/Kernel/Internal/Selection/GraphEditorKernelSelectionCoordinator.cs` owns kernel selection changes.
- `src/AsterGraph.Editor/Kernel/Internal/ConnectionMutation/GraphEditorKernelConnectionMutationCoordinator.cs` owns connection mutation commands.
- `src/AsterGraph.Editor/Runtime/Internal/GraphEditorConnectionGeometryProjector.cs` projects semantic connections into geometry snapshots.
- `src/AsterGraph.Editor/Geometry/ConnectionPathBuilder.cs` builds rendered connection paths.

### Groups and Containers

- `src/AsterGraph.Core/Models/GraphNodeGroup.cs` persists editor-owned group metadata.
- `src/AsterGraph.Editor/Runtime/GraphEditorNodeGroupSnapshot.cs` projects group runtime state.
- `src/AsterGraph.Editor/Runtime/Internal/GraphEditorHierarchyStateProjector.cs` projects hierarchy and collapse state.
- `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasGroupChromeMetrics.cs` defines group chrome metrics.

### Layout and Alignment

- `src/AsterGraph.Editor/Runtime/IGraphLayoutProvider.cs` is the supported host extension seam for layout plans.
- `src/AsterGraph.Editor/Runtime/GraphLayoutRequest.cs` and `src/AsterGraph.Editor/Runtime/GraphLayoutPlan.cs` define read-only layout plan contracts.
- `src/AsterGraph.Editor/Kernel/Internal/Layout/NodeSelectionLayoutService.cs` executes built-in align/distribute commands.
- `src/AsterGraph.Avalonia/Controls/Internal/Interaction/NodeCanvasDragAssistCalculator.cs` computes grid snap and guide assist behavior.
- `src/AsterGraph.Avalonia/Controls/Internal/Overlay/NodeCanvasOverlayCoordinator.cs` displays guide overlays.

### Workbench, Demo, and Proof

- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` hosts the current workbench chrome and composition state.
- `src/AsterGraph.Avalonia/Hosting/AsterGraphWorkbenchOptions.cs` and `src/AsterGraph.Avalonia/Hosting/AsterGraphWorkbenchPerformancePolicy.cs` are the workbench configuration seams.
- `src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.cs` and `src/AsterGraph.Demo/Cookbook/DemoCookbookWorkspaceProjection.cs` form the cookbook proof surface.
- `tools/AsterGraph.ScaleSmoke/Program.cs`, `tests/AsterGraph.ScaleSmoke.Tests/ScaleSmokeBudgetTests.cs`, and `eng/ci.ps1` form the scale proof chain.

## Hot Paths

- `NodeCanvasSceneHost.RebuildScene()` clears scene layers and reconstructs node/group visuals before rendering connections. It is the highest-risk structural rebuild path.
- `NodeCanvasSceneHost.RenderConnections()` clears and recreates connection visuals each time it renders. Large graphs and dense interaction updates can make this path expensive.
- `ViewportVisibleSceneProjector.Project(...)` currently scans document nodes, groups, and connections linearly for every projection refresh.
- `NodeCanvasViewModelObserver` can map a single semantic edit into multiple visual updates; later phases should avoid duplicate invalidation bursts.
- `GraphMiniMap` has a lightweight projection mode, but its refresh contract must stay explicit so it does not silently lag behind viewport or document changes.
- `GraphEditorView.axaml.cs` is chrome-heavy; workbench UX changes should be isolated through composition/coordinator seams instead of growing a monolithic view.

## Non-Goals

- No second rendering engine.
- No runtime graph model rewrite.
- No compatibility or fallback layer.
- No unmeasured graph-size claims.
- No generated runnable code snippets in the demo.
- No workflow engine, scripting system, marketplace behavior, or unsupported adapter claims.

