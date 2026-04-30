# Phase 447 Plan

## Goal

Make the supported custom node, edge, anchor, overlay, runtime decoration, and inspector path coherent without adding a new renderer seam or execution model.

## Success Criteria

1. Public docs classify the customization route as package-owned and bounded.
2. ConsumerSample proof emits a single custom extension marker cluster.
3. Tests defend the proof markers and documentation boundary.
4. Verification passes for touched test projects or records a concrete blocker.

## Work Plan

1. Add ConsumerSample proof markers for custom node presenter lifecycle, anchor surface, edge overlay, runtime-inspector route, and scope boundary.
2. Update English and Chinese docs for public API inventory, custom node host recipe, authoring surface recipe, and demo cookbook.
3. Add focused test assertions for the new proof and docs markers.
4. Run focused ConsumerSample and Demo tests.
5. Run the prohibited external-name scan on touched docs and planning files.
6. Commit the scoped branch without pushing.

## Non-Goals

- No `IGraphEdgeVisualPresenter`.
- No public `OverlayLayer` exposure.
- No runtime execution engine.
- No rendering/performance or interaction command changes.
