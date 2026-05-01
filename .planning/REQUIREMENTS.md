# Requirements

## Milestone

v0.78.0-beta Professional Desktop Node Graph Component Platform

## Intent

This milestone takes the next larger product step from semantic authoring into a professional desktop node graph component platform. The scope is canvas interaction architecture, high-performance rendering and viewport behavior, node and edge customization surfaces, layout and spatial authoring workflows, cookbook component showcase, and release contracts. It must preserve the existing session/runtime ownership model and must not add fallback layers, compatibility shims, macro/query scripting, code-generation execution, collaborative sync, marketplace/sandbox claims, or a separate workflow engine.

## Active Requirements

### INT-01 — Professional Canvas Interaction Engine

Users get professional canvas interactions for pan, zoom, select, drag, connect, resize, focus, context menus, and keyboard flows through coherent source-backed ownership.

**Acceptance:**
- Current interaction ownership and hot paths are audited before implementation.
- Interaction state is source-backed where it affects package behavior.
- Stock Avalonia interactions compose canonical session/query/command contracts.
- Focused tests cover success, rejection, keyboard, focus, and gesture boundaries.

### RENDER-01 — High-Performance Rendering And Viewport Pipeline

Users can work with large graphs through measured rendering, viewport, invalidation, minimap, and hit-test behavior.

**Acceptance:**
- Rendering and viewport changes are measured or proof-backed.
- Visible-scene projection, invalidation cadence, minimap cadence, hit testing, and interaction latency remain bounded.
- Claims avoid unsupported background indexing, query-language, or renderer-rewrite promises.
- Tests cover performance-sensitive and large-graph behavior.

### CUSTOM-01 — Professional Node And Edge Customization Surface

Users can customize nodes, ports, edges, overlays, inspectors, and editor affordances through supported package contracts.

**Acceptance:**
- Customization routes are package-owned and documented.
- Node visual, port handle, edge overlay, connection style, inspector/editor, and host-owned extension points stay coherent.
- Demo remains proof/sample only and does not define supported package contracts.
- Tests cover extension behavior and support boundaries.

### SPACE-01 — Layout And Spatial Authoring Workbench

Users can compose layout, alignment, snapping, groups/subgraphs, selection transforms, and spatial editing as one professional workbench flow.

**Acceptance:**
- Layout, alignment, snap, group/subgraph, selection transform, and spatial editing workflows compose existing session contracts.
- Constraints remain coherent across groups, routes, layout plans, and selection transforms.
- Stock Avalonia workbench projects the workflows without UI-only runtime ownership.
- Focused tests cover composition and rejection behavior.

### COOK-02 — Cookbook Component Showcase

Users can learn the component platform through cookbook recipes that pair code, graph proof, demo behavior, and support boundaries.

**Acceptance:**
- Cookbook recipes cover rendering, interaction, customization, layout, and spatial authoring.
- Each recipe has code anchors, graph proof, demo behavior, docs, and support-boundary text.
- Demo remains a sample/proof surface and does not become a package contract boundary.
- Focused Demo tests verify scenario catalog, documentation, and proof closure.

### REL-02 — v0.78 Release Proof And API Governance

Users can verify v0.78 through public API inventory, docs, baseline gates, .NET 8/9/10 CI-sensitive tests, beads, Dolt, and Git handoff.

**Acceptance:**
- Public docs and examples explain supported v0.78 contracts without naming external inspiration projects.
- All v0.78 requirements map to phases and proof markers.
- Public API baseline changes are intentional and classified.
- Beads, Dolt, Git branch, and workspace are clean and pushed at closeout.

## Non-Goals

- Runtime architecture rewrite or replacement renderer.
- Compatibility or fallback layers.
- Separate workflow execution engine, macro/query scripting system, marketplace, remote plugin distribution, plugin sandboxing, or untrusted-code isolation.
- Generated runnable code execution inside the Demo or cookbook.
- Collaborative multi-user editing, CRDT/OT synchronization, cloud storage, or remote session hosting.
- WPF parity, WinUI, MAUI, or web adapter expansion.
- Treating Demo-only scaffolding as a supported package boundary.
- New graph-size, indexing, or background service support claims beyond defended proof evidence.
- Public or planning documentation that names external inspiration projects.

## Phase Mapping

| Requirement | Phases |
|-------------|--------|
| INT-01 | 466 |
| RENDER-01 | 467 |
| CUSTOM-01 | 468 |
| SPACE-01 | 469 |
| COOK-02 | 470 |
| REL-02 | 471 |

**Coverage:**
- v0.78 requirements: 6 total
- Mapped to phases: 6
- Unmapped: 0

---
*Last updated: 2026-05-01 after starting v0.78*
