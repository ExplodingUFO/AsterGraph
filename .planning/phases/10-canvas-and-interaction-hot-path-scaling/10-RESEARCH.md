# Phase 10: Canvas And Interaction Hot-Path Scaling - Research

## Summary

The biggest proven scaling problems are:

1. `RenderConnections()` clears and rebuilds the entire connection layer while also doing repeated linear node lookups.
2. Marquee selection scans all nodes and rewrites selection/projection state on every pointer sample.
3. Scene rebuilds are too coarse for small graph deltas.

The safest Phase 10 approach is:

- add cheaper lookup/state caches first
- localize redraw/update work before considering deeper visual virtualization
- keep `NodeCanvas` and `GraphEditorViewModel` behavior stable while reducing graph-wide recomputation
