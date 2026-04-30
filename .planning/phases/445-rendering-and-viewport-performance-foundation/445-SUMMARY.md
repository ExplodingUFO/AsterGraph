# Phase 445 Summary: Rendering And Viewport Performance Foundation

**Bead:** `avalonia-node-map-mqm.2`
**Status:** Passed

## Implemented

- Added `ViewportVisibleSceneProjector` and `ViewportVisibleSceneProjection`.
- Added deterministic projection counts for total/visible nodes, connections, and groups.
- Added `VISIBLE_SCENE_PROJECTION:<tier>:...` budget marker output.
- Added `NodeCanvasSceneHost.LastVisibleSceneProjection` and refresh points on scene rebuild, viewport transform, and connection rendering.
- Added minimap cadence readout and `MINIMAP_CADENCE:<mode>:...` marker to the existing workbench performance policy.
- Added focused tests for viewport projection and minimap cadence.

## Boundaries Preserved

- No fallback rendering layer.
- No new graph-size support claim.
- No interaction command semantics changed.
- No customization extension docs changed.
- No bead state files were intentionally modified.

## Residual Risks

- The scene host now records projection evidence, but it still renders the existing visual tree. Actual culling or visual recycling is intentionally deferred until a later phase has a narrower implementation mandate.
- Projection visibility for connections is endpoint-based. Route-aware edge clipping can be added later if a rendering phase needs that precision.
