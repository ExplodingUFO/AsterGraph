---
status: passed
phase: 451
bead: avalonia-node-map-y7i.1
updated: 2026-04-30
---

# Phase 451 Plan

## Success Criteria

- Current canvas engine ownership and data flow are documented with concrete file seams.
- Hot paths and scale constraints are identified before implementation.
- Phase 452 and Phase 453 are unblocked and can run independently.
- Later phases have narrow write scopes and explicit dependencies.
- No code behavior is changed in this audit phase.

## Execution

1. Audit rendering, viewport, minimap, scene projection, and invalidation.
2. Audit interaction, selection, connection lifecycle, groups, and cookbook workbench surfaces.
3. Audit layout, alignment, snap, host contracts, scale proof, and CI-sensitive paths.
4. Convert findings into narrow follow-up bead boundaries.
5. Record verification evidence and close the phase bead.

## Follow-Up Bead Boundaries

### Phase 452: Virtualized Scene Index And Viewport Pipeline

Responsibility:

- Keep visible-scene lookup and viewport projection bounded around the existing scene host.
- Tighten minimap refresh and invalidation behavior.
- Preserve the current snapshot-first runtime and Avalonia scene host ownership.

Primary write scope:

- `src/AsterGraph.Editor/Viewport/ViewportVisibleSceneProjection.cs`
- New or existing viewport/index support under `src/AsterGraph.Editor/Viewport/`
- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasSceneHost.cs`
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`
- Focused tests under `tests/AsterGraph.Editor.Tests/` and `tests/AsterGraph.Avalonia.Tests/`

Avoid:

- Replacing `NodeCanvasSceneHost`.
- Building a second renderer.
- Moving document semantics into Avalonia controls.

Verification:

- Projection and minimap tests.
- Scale smoke markers.
- No unmeasured graph-size claims in docs.

### Phase 453: Professional Edge Routing And Connection Geometry

Responsibility:

- Make route styles, anchors, preview geometry, reconnect feedback, and route proof explicit.
- Work through existing connection mutation and geometry projector seams.

Primary write scope:

- `src/AsterGraph.Editor/Runtime/Internal/GraphEditorConnectionGeometryProjector.cs`
- `src/AsterGraph.Editor/Geometry/ConnectionPathBuilder.cs`
- `src/AsterGraph.Editor/Kernel/Internal/ConnectionMutation/GraphEditorKernelConnectionMutationCoordinator.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasConnectionSceneRenderer.cs`
- Focused connection geometry and renderer tests.

Avoid:

- Changing connection semantics to satisfy rendering-only needs.
- Adding broad routing frameworks before route contracts are explicit.

Verification:

- Geometry contract tests.
- Renderer tests.
- Cookbook route scenario proof.

### Phase 454: Groups, Subgraphs, And Collapsible Containers

Responsibility:

- Formalize group/container semantics after viewport and routing foundations are stable.
- Support nested selection, collapse projection, boundary edges, and serialization snapshots.

Primary write scope:

- `src/AsterGraph.Core/Models/GraphNodeGroup.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorNodeGroupSnapshot.cs`
- `src/AsterGraph.Editor/Runtime/Internal/GraphEditorHierarchyStateProjector.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasGroupChromeMetrics.cs`
- Group interaction and serialization tests.

Avoid:

- Introducing a workflow execution model.
- Encoding group behavior only in demo view models.

Verification:

- Group snapshot tests.
- Collapse/expand and selection tests.
- Cookbook group scenario proof.

### Phase 455: Layout Services And Alignment Tools

Responsibility:

- Clarify the separation between read-only layout plans and command-driven layout execution.
- Strengthen align, distribute, snap, and incremental relayout services.

Primary write scope:

- `src/AsterGraph.Editor/Runtime/IGraphLayoutProvider.cs`
- `src/AsterGraph.Editor/Runtime/GraphLayoutRequest.cs`
- `src/AsterGraph.Editor/Runtime/GraphLayoutPlan.cs`
- `src/AsterGraph.Editor/Kernel/Internal/Layout/NodeSelectionLayoutService.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Interaction/NodeCanvasDragAssistCalculator.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Overlay/NodeCanvasOverlayCoordinator.cs`

Avoid:

- Treating `CreateLayoutPlan` as an execute path.
- Adding layout providers that cannot be proven in tests and examples.

Verification:

- Layout contract tests.
- Align/distribute command tests.
- Snap/guide boundary tests.

### Phase 456: Designer Workbench Authoring UX

Responsibility:

- Improve the authoring workbench UX around navigator/outline, inspector, route-aware affordances, group/layout workflows, recovery states, and cookbook examples.

Primary write scope:

- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- Existing workbench composition/coordinator files.
- `src/AsterGraph.Demo/Cookbook/`
- Demo view model files for cookbook projection.

Avoid:

- Turning the demo into a package contract boundary.
- Growing all UX logic inside one large view class.

Verification:

- Cookbook catalog/proof tests.
- Visual baseline tests where applicable.
- Focused workbench interaction tests.

### Phase 457: Extension Contracts, Documentation, And Release Proof

Responsibility:

- Close supported extension contracts, docs, examples, proof markers, CI-sensitive verification, beads, Dolt, and Git handoff.

Primary write scope:

- `docs/`
- `README.md` and localized docs where already present.
- `src/AsterGraph.ConsumerSample/`
- `eng/ci.ps1`
- `tests/AsterGraph.ScaleSmoke.Tests/`
- Release proof artifacts.

Avoid:

- Adding new features late in the milestone.
- Publishing unsupported platform, adapter, or scale claims.

Verification:

- Full CI script.
- Scale smoke proof.
- Documentation scan for unsupported or forbidden references.

