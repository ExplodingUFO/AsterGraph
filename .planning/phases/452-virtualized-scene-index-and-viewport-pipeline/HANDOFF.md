# Phase 452 Handoff

## Contract

- `ViewportVisibleSceneProjection` now reports stable visible node, connection, and group IDs in document order, in addition to aggregate counts.
- `ViewportVisibleSceneProjection.Diff(...)` reports only scene items that entered or left the visible set. Its budget marker encodes the bounded invalidation count.
- Throughput/lightweight minimap projection keeps cached node geometry and selection state, but refreshes the viewport snapshot for each projection read so the viewport rectangle does not go stale after pan/zoom changes.

## Boundaries

- Routing and connection geometry files were not modified.
- Beads, Dolt data, remotes, and push state were not touched.
