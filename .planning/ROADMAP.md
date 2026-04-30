# Roadmap

## Current Milestone

**v0.77.0-beta Semantic Authoring And Command Platform** — active locally 2026-05-01.

**Goal:** take the next larger step from professional canvas/workbench depth into a semantic authoring platform with unified command discovery/execution, advanced editing operations, reusable templates, selection transforms, navigation/search workflows, cookbook proof, and release contracts.

**Focus:** command platform, keybinding/menu/tool projection, semantic editing, clipboard, templates, selection transforms, navigation/search, cookbook flows, contract proof.

**Known inputs:**
- The user wants the next milestone to take a larger product step.
- v0.76 completed professional canvas/workbench depth and public API release proof.
- New work must keep supported contracts in package APIs, samples, docs, proof markers, and CI-sensitive tests.
- Planning and docs must not name external inspiration projects or packages.

**Epic bead:** `avalonia-node-map-48w`

**Phase numbering:** continues from v0.76 and starts at **Phase 458**.

## Phases

- [x] **Phase 458: Command Platform Architecture And Input Audit** — mapped command descriptors, input routing, menu/tool surfaces, undo boundaries, and host extension seams before implementation.
- [x] **Phase 459: Unified Command Registry And Keybinding Surface** — exposed command discovery, keybinding metadata, menu/tool projection, disabled recovery states, and execution through canonical session boundaries.
- [x] **Phase 460: Semantic Editing Operations And Clipboard Model** — supported source-backed copy/paste payloads, groups/connections, delete-with-repair, insert-into-route, reconnect, and semantic command routing.
- [x] **Phase 461: Template Palette And Reusable Authoring Presets** — added host-provided node/fragment templates, searchable palette projection, and undoable reusable preset application.
- [x] **Phase 462: Advanced Selection Transform And Spatial Editing** — added source-backed selection transform snapshots, rectangle projection, constrained selection move, and snap guide projection.
- [ ] **Phase 463: Viewport Navigation Search And Focus Workflows** — add search, jump, breadcrumb, bookmark, minimap-aware focus, and measured large-graph navigation evidence.
- [ ] **Phase 464: Professional Cookbook Authoring Flows** — demonstrate command, editing, template, selection, and navigation workflows as code plus live proof.
- [ ] **Phase 465: v0.77 Contracts Documentation And Release Proof** — close supported contracts, docs, examples, public API baseline, CI-sensitive tests, beads, Dolt, and Git handoff.

## Phase Details

### Phase 458: Command Platform Architecture And Input Audit

**Bead:** `avalonia-node-map-48w.1`

**Goal:** define the supported command/input architecture before implementation so later command work stays narrow and source-backed.

**Depends on:** v0.77 milestone start

**Requirements:** CMD-01

**Success Criteria:**
1. Current command descriptors, input routing, menu/tool surfaces, and undo boundaries are mapped with file/class evidence.
2. Host-owned versus stock workbench-owned command responsibilities are explicit.
3. Later implementation phases have narrow write scopes and dependency order.
4. No macro/query scripting system, fallback layer, compatibility shim, or second command runtime is planned.

### Phase 459: Unified Command Registry And Keybinding Surface

**Bead:** `avalonia-node-map-48w.2`

**Goal:** make command discovery, keybinding metadata, menu/tool projection, disabled recovery states, and execution consistent across package and workbench surfaces.

**Depends on:** Phase 458

**Requirements:** CMD-01

**Success Criteria:**
1. Supported command registry contracts expose stable identity, grouping, availability, keybinding metadata, and recovery hints.
2. Stock Avalonia command surfaces consume the same registry rather than local command lists.
3. Keybinding conflicts are detectable and testable.
4. Commands execute through canonical session boundaries with explicit undo/redo behavior.

### Phase 460: Semantic Editing Operations And Clipboard Model

**Bead:** `avalonia-node-map-48w.3`

**Goal:** add supported semantic editing operations and clipboard payloads without turning the Demo into a generator or workflow engine.

**Depends on:** Phase 459

**Requirements:** EDIT-01

**Success Criteria:**
1. Copy, paste, duplicate, delete-with-repair, insert-into-route, reconnect, and semantic batch edit operations have command contracts.
2. Clipboard payloads are source-backed, serializable, and invariant-preserving.
3. Node, port, group, route, selection, validation, and undo boundaries are tested.
4. Docs avoid generated runnable code and workflow execution claims.

### Phase 461: Template Palette And Reusable Authoring Presets

**Bead:** `avalonia-node-map-48w.4`

**Goal:** introduce reusable authoring templates and palette projection as host-extensible package contracts.

**Depends on:** Phase 459

**Requirements:** TPL-01

**Success Criteria:**
1. Hosts can provide node, group, and graph-fragment templates through supported contracts.
2. Search/filter palette projection returns stable snapshots for the workbench.
3. Applying templates is undoable and preserves graph invariants.
4. Docs distinguish templates from code generation, marketplace, and executable recipe systems.

### Phase 462: Advanced Selection Transform And Spatial Editing

**Bead:** `avalonia-node-map-48w.5`

**Goal:** make multi-selection and spatial editing feel professional while keeping transform state source-backed and testable.

**Depends on:** Phase 459

**Requirements:** SEL-01

**Success Criteria:**
1. Marquee/lasso-style selection projection, keyboard nudging, constrained move/resize, and snap guides are queryable.
2. Stock Avalonia interactions use shared transform state.
3. Group, route, layout, and selection constraints stay coherent.
4. Focused tests cover transform, snap, and rejection behavior.

### Phase 463: Viewport Navigation Search And Focus Workflows

**Bead:** `avalonia-node-map-48w.6`

**Goal:** add source-backed search and navigation workflows for large professional graphs.

**Depends on:** Phase 459

**Requirements:** NAV-01

**Success Criteria:**
1. Graph item search returns stable node, group, connection, issue, and scope references.
2. Breadcrumb, bookmark, jump-to-node, jump-to-issue, and minimap-aware focus workflows are supported.
3. Large-graph navigation behavior is measured or proof-backed.
4. Docs avoid unsupported indexing, query language, or background service claims.

### Phase 464: Professional Cookbook Authoring Flows

**Bead:** `avalonia-node-map-48w.7`

**Goal:** make the Avalonia cookbook demonstrate v0.77 professional authoring flows as code plus live proof.

**Depends on:** Phases 460, 461, 462, 463

**Requirements:** COOK-01

**Success Criteria:**
1. Cookbook scenarios cover command registry, editing operations, templates, selection transforms, and navigation workflows.
2. Each scenario has code anchors, proof markers, docs, and support-boundary text.
3. Demo remains sample/proof surface only.
4. Focused Demo tests verify catalog, documentation, and proof closure.

### Phase 465: v0.77 Contracts Documentation And Release Proof

**Bead:** `avalonia-node-map-48w.8`

**Goal:** close v0.77 with supported contracts, docs, public API baseline, proof markers, and clean beads/Dolt/Git handoff.

**Depends on:** Phase 464

**Requirements:** REL-01

**Success Criteria:**
1. Public docs and examples explain supported v0.77 contracts.
2. All v0.77 requirements map to phases and proof markers.
3. Public API baseline changes are intentional and classified.
4. Required verification passes and beads, Dolt, Git branch, and workspace are clean and pushed.

## Completed Milestones

- **v0.76.0-beta Professional Canvas Engine And Authoring Workbench** — completed locally 2026-05-01.
- **v0.75.0-beta Cross-Platform High-Performance Desktop Node Graph Library** — completed locally 2026-04-30.
- **v0.74.0-beta Cookbook Scenario Depth And Component Polish** — completed locally 2026-04-30.
- **v0.73.0-beta CI Reliability And Release Gate Recovery** — completed 2026-04-30; GitHub Actions run `25159303518` passed all jobs.
- Earlier completed milestones remain archived under `.planning/milestones/`.

## Next Step

Continue Phase 463. Phase 464 remains blocked until Phase 463 search/focus workflows close.

---
*Roadmap updated: 2026-05-01 after completing Phase 462*
