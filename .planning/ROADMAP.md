# Roadmap

## Current Milestone

**v0.78.0-beta Professional Desktop Node Graph Component Platform** — active locally 2026-05-01.

**Goal:** take the next larger step from semantic authoring into a professional desktop node graph component platform with stronger canvas interaction, rendering/viewport performance, customization surfaces, spatial authoring workflows, cookbook showcase, and release contracts.

**Focus:** canvas interaction engine, rendering/viewport pipeline, node/edge customization, layout/spatial authoring, cookbook component showcase, release proof.

**Known inputs:**
- The user wants the next milestone to take a larger product step.
- v0.77 completed command platform, semantic editing, templates, selection transforms, navigation/search/focus, cookbook proof, public docs, API baseline, and release proof.
- New work must keep supported contracts in package APIs, samples, docs, proof markers, and CI-sensitive tests.
- Planning and docs must not name external inspiration projects or packages.
- Worktrees must stay inside the project folder.

**Epic bead:** `avalonia-node-map-v78`

**Phase numbering:** continues from v0.77 and starts at **Phase 466**.

## Phases

- [x] **Phase 466: Canvas Interaction Engine Audit** — map pan, zoom, select, drag, connect, resize, context menu, focus, keyboard, and command interaction ownership before implementation.
- [x] **Phase 467: High-Performance Rendering And Viewport Pipeline** — strengthen large-graph rendering, visible-scene projection, viewport invalidation, minimap cadence, hit testing, and interaction latency evidence.
- [x] **Phase 468: Professional Node And Edge Customization Surface** — deepen supported node visual, port handle, edge overlay, connection style, inspector/editor, and host-owned extension points.
- [x] **Phase 469: Layout And Spatial Authoring Workbench** — productize layout, alignment, snapping, group/subgraph, selection transform, and spatial editing workflows.
- [x] **Phase 470: Cookbook Component Showcase** — turn the Avalonia cookbook into a component showcase for rendering, interaction, customization, layout, and code-plus-demo recipes.
- [ ] **Phase 471: v0.78 Release Proof And API Governance** — close public API inventory, docs, baseline updates, CI-sensitive .NET 8/9/10 verification, beads, Dolt, Git, and workspace handoff.

## Phase Details

### Phase 466: Canvas Interaction Engine Audit

**Bead:** `avalonia-node-map-v78.1`

**Goal:** define the supported interaction-engine architecture before implementation so later rendering, customization, and spatial work stays narrow and source-backed.

**Depends on:** v0.78 milestone start

**Requirements:** INT-01

**Success Criteria:**
1. Current pan, zoom, select, drag, connect, resize, context menu, focus, keyboard, and command interaction ownership is mapped with file/class evidence.
2. Hot paths, state ownership, coupling risks, and test/proof gaps are explicit.
3. Later implementation phases have narrow write scopes and dependency order.
4. No fallback layer, compatibility shim, renderer rewrite, or second interaction runtime is planned.

### Phase 467: High-Performance Rendering And Viewport Pipeline

**Bead:** `avalonia-node-map-v78.2`

**Goal:** make rendering and viewport behavior more robust for professional large-graph desktop use.

**Depends on:** Phase 466

**Requirements:** RENDER-01

**Success Criteria:**
1. Rendering, visible-scene projection, viewport invalidation, minimap cadence, hit testing, and interaction latency are source-backed and measured.
2. Large-graph claims are tied to repeatable tests or proof output.
3. Changes preserve existing session/runtime ownership.
4. Docs avoid unsupported background indexing, query-language, or renderer-rewrite claims.

### Phase 468: Professional Node And Edge Customization Surface

**Bead:** `avalonia-node-map-v78.3`

**Goal:** deepen supported customization for hosts building professional node graph tools.

**Depends on:** Phase 466

**Requirements:** CUSTOM-01

**Success Criteria:**
1. Node visuals, port handles, edge overlays, connection styles, inspector/editor affordances, and host-owned extension points are mapped to supported contracts.
2. Package-owned customization surfaces are tested and documented.
3. Demo remains sample/proof only.
4. No marketplace, sandbox, or untrusted plugin claim is added.

### Phase 469: Layout And Spatial Authoring Workbench

**Bead:** `avalonia-node-map-v78.4`

**Goal:** make layout, alignment, snap, group/subgraph, selection transform, and spatial editing workflows feel coherent as one professional workbench.

**Depends on:** Phase 466

**Requirements:** SPACE-01

**Success Criteria:**
1. Layout/spatial workflows compose existing session contracts.
2. Group, route, layout, snap, and selection constraints stay coherent.
3. Stock Avalonia workbench projects the workflows without UI-only runtime ownership.
4. Focused tests cover composition and rejection behavior.

### Phase 470: Cookbook Component Showcase

**Bead:** `avalonia-node-map-v78.5`

**Goal:** make the Avalonia cookbook demonstrate the v0.78 component platform as code plus live proof.

**Depends on:** Phases 467, 468, 469

**Requirements:** COOK-02

**Success Criteria:**
1. Cookbook scenarios cover rendering, interaction, customization, layout, and spatial authoring.
2. Each scenario has code anchors, graph proof, demo behavior, docs, and support-boundary text.
3. Demo remains sample/proof surface only.
4. Focused Demo tests verify catalog, documentation, and proof closure.

### Phase 471: v0.78 Release Proof And API Governance

**Bead:** `avalonia-node-map-v78.6`

**Goal:** close v0.78 with supported contracts, docs, public API baseline, proof markers, and clean beads/Dolt/Git handoff.

**Depends on:** Phase 470

**Requirements:** REL-02

**Success Criteria:**
1. Public docs and examples explain supported v0.78 contracts.
2. All v0.78 requirements map to phases and proof markers.
3. Public API baseline changes are intentional and classified.
4. Required .NET 8/9/10 verification passes and beads, Dolt, Git branch, and workspace are clean and pushed.

## Completed Milestones

- **v0.77.0-beta Semantic Authoring And Command Platform** — completed locally 2026-05-01.
- **v0.76.0-beta Professional Canvas Engine And Authoring Workbench** — completed locally 2026-05-01.
- **v0.75.0-beta Cross-Platform High-Performance Desktop Node Graph Library** — completed locally 2026-04-30.
- **v0.74.0-beta Cookbook Scenario Depth And Component Polish** — completed locally 2026-04-30.
- **v0.73.0-beta CI Reliability And Release Gate Recovery** — completed 2026-04-30; GitHub Actions run `25159303518` passed all jobs.
- Earlier completed milestones remain archived under `.planning/milestones/`.

## Next Step

Start Phase 471. Phase 470 completed cookbook code-plus-demo routes for rendering/viewport, customization, and spatial authoring, with source anchors, demo proof, docs anchors, support-boundary text, and focused Demo cookbook tests.

---
*Roadmap updated: 2026-05-01 after completing Phases 467-469*
