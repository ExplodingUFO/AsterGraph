---
status: passed
phase: 451
bead: avalonia-node-map-y7i.1
updated: 2026-04-30
---

# Phase 451 Summary

## Outcome

Phase 451 is complete as an audit and planning handoff phase.

The current system already has usable seams for v0.76:

- Snapshot-first runtime projection.
- Avalonia scene host with separate layers for groups, connections, and nodes.
- Viewport projection and minimap surfaces.
- Command-driven interaction, selection, connection, group, and layout behavior.
- Cookbook and scale-smoke proof surfaces.

The main engineering constraint is scale discipline. Later phases should bound and measure work in the existing pipeline instead of replacing the renderer or moving runtime semantics into the UI layer.

## Decisions

- Phase 452 and Phase 453 are independent after Phase 451 and can be executed in parallel project-internal worktrees.
- Phase 452 should focus on scene index/projection/invalidation around `NodeCanvasSceneHost` and `ViewportVisibleSceneProjection`.
- Phase 453 should focus on routing and geometry around the connection projector, path builder, mutation coordinator, and scene renderer.
- Phase 454 waits for both viewport and routing foundations.
- Phase 455 waits for viewport and group foundations.
- Phase 456 waits for group and layout foundations.
- Phase 457 closes contracts, docs, examples, proof, and release handoff.

## Risks Carried Forward

- `RenderConnections()` currently clears and rebuilds connection visuals.
- `RebuildScene()` remains a full structural rebuild.
- `ViewportVisibleSceneProjector.Project(...)` scans scene data linearly.
- Minimap lightweight projection needs a clearer refresh contract.
- Layout plan queries and layout command execution can be misunderstood if docs and API tests do not keep the distinction explicit.
- Snap/guide candidate eligibility is implicit and should become explicit before complex authoring workflows depend on it.

## Handoff

Recommended next execution:

1. Run Phase 452 and Phase 453 in parallel isolated worktrees under `.worktrees/`.
2. Keep each bead small and file-scoped.
3. Use tests and scale markers to prove bounded behavior before updating docs.
4. Do not broaden public claims until Phase 457 closes release proof.

