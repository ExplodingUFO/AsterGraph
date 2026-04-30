# Roadmap

## Current Milestone

**v0.76.0-beta Professional Canvas Engine And Authoring Workbench** — active locally 2026-04-30.

**Goal:** take a larger step from library-grade proof into a professional desktop canvas engine and authoring workbench with virtualized scene/indexing, advanced routing, groups/subgraphs, layout services, designer-grade UX, extension contracts, and release proof.

**Focus:** canvas engine architecture, virtualized scene index, edge routing, groups/subgraphs, layout services, designer workbench UX, extension contracts, proof closure.

**Known inputs:**
- The user wants the next milestone to take a bigger product step.
- v0.75 completed library-grade proof for rendering/viewport, interaction, customization, packaging, examples, and release gates.
- New work must keep supported contracts in package APIs, samples, docs, proof markers, and CI-sensitive tests.
- Planning and docs must not name external inspiration projects or packages.

**Epic bead:** `avalonia-node-map-y7i`

**Phase numbering:** continues from v0.75 and starts at **Phase 451**.

## Phases

- [x] **Phase 451: Canvas Engine Architecture And Scale Audit** — map current seams, hot paths, state ownership, and narrow implementation cuts.
- [x] **Phase 452: Virtualized Scene Index And Viewport Pipeline** — support bounded visible-scene lookup, viewport projection, minimap refresh, and invalidation proof.
- [x] **Phase 453: Professional Edge Routing And Connection Geometry** — support route styles, anchors, preview geometry, reconnect feedback, and bounded crossing/obstacle evidence.
- [x] **Phase 454: Groups, Subgraphs, And Collapsible Containers** — support group/container creation, nested selection, collapse/expand projection, boundary edges, and serialization snapshots.
- [x] **Phase 455: Layout Services And Alignment Tools** — expose layout, align, distribute, snap, and incremental relayout services through host contracts and workbench commands.
- [ ] **Phase 456: Designer Workbench Authoring UX** — integrate navigator/outline, inspector, route-aware affordances, group/layout workflows, recovery states, and cookbook scenarios.
- [ ] **Phase 457: Extension Contracts, Documentation, And Release Proof** — close supported contracts, docs, examples, proof markers, full verification, beads, Dolt, and Git handoff.

## Phase Details

### Phase 451: Canvas Engine Architecture And Scale Audit

**Bead:** `avalonia-node-map-y7i.1`

**Goal:** map current canvas/render/interaction architecture and define the smallest supported engine contracts for v0.76 before implementation.

**Depends on:** v0.76 milestone start

**Requirements:** ENGINE-01

**Success Criteria:**
1. Render, scene, interaction, layout, and workbench seams are mapped with file/class evidence.
2. Hot paths and state ownership boundaries are identified.
3. Later implementation phases have narrow write scopes and dependency order.
4. No runtime rewrite, second renderer, compatibility layer, or fallback layer is planned.

**Status:** Complete. See `.planning/phases/451-canvas-engine-architecture-and-scale-audit/`.

### Phase 452: Virtualized Scene Index And Viewport Pipeline

**Bead:** `avalonia-node-map-y7i.2`

**Goal:** implement a supported scene index and viewport projection path that keeps large canvas updates bounded and measurable.

**Depends on:** Phase 451

**Requirements:** VIRTUAL-01

**Success Criteria:**
1. Visible node/edge lookup has a supported contract and focused tests.
2. Viewport projection and minimap refresh use bounded invalidation behavior.
3. Large-graph update proof reports measurable budgets.
4. Docs avoid unsupported graph-size, virtualization, and renderer-replacement claims.

**Status:** Complete. See `.planning/phases/452-virtualized-scene-index-and-viewport-pipeline/`.

### Phase 453: Professional Edge Routing And Connection Geometry

**Bead:** `avalonia-node-map-y7i.3`

**Goal:** add a first-class edge routing and connection geometry layer for professional authoring without replacing the runtime graph model.

**Depends on:** Phase 451

**Requirements:** ROUTE-01

**Success Criteria:**
1. Route styles and anchor contracts are package-owned and source-backed.
2. Connection preview and reconnect feedback use route-aware geometry.
3. Obstacle/crossing evidence is bounded and testable.
4. Existing edge editing behavior remains direct, simple, and covered.

**Status:** Complete. See `.planning/phases/453-professional-edge-routing-and-connection-geometry/`.

### Phase 454: Groups, Subgraphs, And Collapsible Containers

**Bead:** `avalonia-node-map-y7i.4`

**Goal:** make groups, nested containers, subgraph boundaries, collapse/expand, and selection semantics coherent for host-owned authoring.

**Depends on:** Phase 452, Phase 453

**Requirements:** GROUP-01

**Success Criteria:**
1. Group/container creation and nested selection are supported.
2. Collapse/expand projection preserves selection and edge boundary behavior.
3. Serialization snapshots represent group and subgraph state.
4. No separate workflow execution engine is introduced.

**Status:** Complete. See `.planning/phases/454-groups-subgraphs-and-collapsible-containers/`.

### Phase 455: Layout Services And Alignment Tools

**Bead:** `avalonia-node-map-y7i.5`

**Goal:** turn layout from sample behavior into supported services and workbench tools for arrange, align, distribute, snap, and incremental relayout.

**Depends on:** Phase 452, Phase 454

**Requirements:** LAYOUT-01

**Success Criteria:**
1. Host-callable layout service contracts are documented and tested.
2. Workbench commands expose arrange, align, distribute, snap, and incremental relayout.
3. Undo/redo boundaries are explicit.
4. Large-graph layout budgets stay measured and bounded.

**Status:** Complete. See `.planning/phases/455-layout-services-and-alignment-tools/`.

### Phase 456: Designer Workbench Authoring UX

**Bead:** `avalonia-node-map-y7i.6`

**Goal:** raise the Avalonia workbench into a designer-grade authoring surface with navigator, outline, inspector, route-aware affordances, and cookbook demonstrations.

**Depends on:** Phase 454, Phase 455

**Requirements:** DESIGNER-01

**Success Criteria:**
1. Users can navigate, inspect, edit, group, route, align, and recover from authoring errors in one coherent UI.
2. Demo cookbook scenarios pair code, visual proof, proof markers, and support boundaries.
3. UI behavior is tested at the view-model/proof level.
4. Demo remains a sample/proof surface, not a package contract boundary.

### Phase 457: Extension Contracts, Documentation, And Release Proof

**Bead:** `avalonia-node-map-y7i.7`

**Goal:** close v0.76 with supported extension contracts, docs, samples, proof markers, CI-sensitive tests, and clean beads/Dolt/Git handoff.

**Depends on:** Phase 455, Phase 456

**Requirements:** CONTRACT-01

**Success Criteria:**
1. Public docs and examples explain the supported v0.76 contracts.
2. All v0.76 requirements map to phases and proof markers.
3. Full required verification passes, including performance-sensitive gates touched by the milestone.
4. Beads, Dolt, Git branch, and workspace are clean and pushed.

## Completed Milestones

- **v0.75.0-beta Cross-Platform High-Performance Desktop Node Graph Library** — completed locally 2026-04-30.
- **v0.74.0-beta Cookbook Scenario Depth And Component Polish** — completed locally 2026-04-30.
- **v0.73.0-beta CI Reliability And Release Gate Recovery** — completed 2026-04-30; GitHub Actions run `25159303518` passed all jobs.
- Earlier completed milestones remain archived under `.planning/milestones/`.

## Next Step

Start Phase 455 after Phase 454.

---
*Roadmap updated: 2026-04-30 after completing Phase 454*
