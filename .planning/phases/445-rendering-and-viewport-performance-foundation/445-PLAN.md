# Phase 445 Plan: Rendering And Viewport Performance Foundation

**Bead:** `avalonia-node-map-mqm.2`
**Status:** Passed

## Success Criteria

1. Visible-scene projection has a deterministic, testable contract.
2. Minimap refresh cadence has a deterministic host-facing budget marker.
3. The existing scene host records projection evidence on rendering/viewport paths.
4. Focused tests pass for touched behavior.
5. Planning/docs artifacts avoid prohibited external project names.

## Implementation Steps

1. Add `ViewportVisibleSceneProjector` under `AsterGraph.Editor.Viewport`.
   - Verify it counts visible nodes, connections, and groups from viewport pan/zoom/size.
   - Verify it emits `VISIBLE_SCENE_PROJECTION` markers.
2. Wire `NodeCanvasSceneHost` to refresh `LastVisibleSceneProjection`.
   - Refresh on rebuild, viewport transform, and connection render.
   - Keep rendering behavior unchanged.
3. Add minimap cadence evidence to `AsterGraphWorkbenchPerformancePolicy`.
   - Keep existing policy constructor shape.
   - Add computed cadence string and `MINIMAP_CADENCE` marker.
4. Add focused tests.
   - Projection tests for visibility counts, pan/zoom world bounds, and marker output.
   - Minimap policy test for balanced and throughput cadence markers.
5. Verify and record results.
   - Run focused tests for the changed test surface.
   - Run the full touched test project.
   - Scan touched planning artifacts for prohibited external names.
