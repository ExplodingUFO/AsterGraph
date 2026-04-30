# Requirements

## Milestone

v0.77.0-beta Semantic Authoring And Command Platform

## Intent

This milestone takes the next larger product step from professional canvas/workbench depth into semantic authoring. The scope is a canonical command platform, semantic editing operations, reusable templates, advanced selection transforms, navigation/search workflows, cookbook proof, and release contracts. It must preserve the existing session/runtime ownership model and must not add fallback layers, compatibility shims, macro/query scripting, code-generation execution, collaborative sync, marketplace/sandbox claims, or a separate workflow engine.

## Active Requirements

### CMD-01 — Unified Command Platform

Users can discover, project, bind, and execute supported commands through one canonical command registry.

**Acceptance:**
- Current command/input/menu/tool ownership is audited before implementation.
- Command descriptors expose stable identity, grouping, availability, recovery, keybinding metadata, and host ownership.
- Stock Avalonia menus, tool palette, and command surfaces consume the same command registry.
- Keybinding conflicts and disabled/recovery states are queryable and testable.

### EDIT-01 — Semantic Editing Operations

Users can perform supported semantic graph editing operations with explicit undo boundaries.

**Acceptance:**
- Copy, paste, duplicate, delete-with-repair, insert-into-route, reconnect, and batch edit operations use canonical session commands.
- Clipboard payloads are source-backed and serializable without becoming generated runnable code.
- Operations preserve node, port, group, route, selection, and validation invariants.
- Tests cover success and rejection cases with undo/redo behavior.

### TPL-01 — Template Palette And Authoring Presets

Users can provide and apply reusable node, group, and graph-fragment templates through supported package contracts.

**Acceptance:**
- Template/preset contracts are package-owned and host-extensible.
- Palette search/filter returns stable snapshots that the workbench can project.
- Applying templates is undoable and preserves graph invariants.
- Docs distinguish templates from code generation, marketplace behavior, and executable recipes.

### SEL-01 — Advanced Selection Transform And Spatial Editing

Users can transform multi-selection state precisely through source-backed selection and spatial-editing contracts.

**Acceptance:**
- Marquee/lasso-style selection projection, keyboard nudging, constrained move/resize, snap guides, and multi-item transform state are queryable.
- Group, route, layout, and selection constraints stay coherent.
- Stock Avalonia interactions use the shared transform state rather than UI-only state.
- Focused tests cover transform, snap, and rejection behavior.

### NAV-01 — Navigation Search And Focus Workflows

Users can find, jump to, bookmark, and focus graph content through source-backed navigation projections.

**Acceptance:**
- Graph item search returns stable node, group, connection, issue, and scope references.
- Breadcrumb, bookmark, jump-to-node, jump-to-issue, and minimap-aware focus workflows are supported.
- Large-graph navigation behavior is measured or otherwise proof-backed.
- Docs avoid unsupported indexing, query language, or background service claims.

### COOK-01 — Professional Cookbook Authoring Flows

Users can learn v0.77 workflows through cookbook demonstrations that pair code, live workbench behavior, proof markers, and support boundaries.

**Acceptance:**
- Cookbook scenarios cover command registry, editing operations, templates, selection transforms, and navigation workflows.
- Each scenario has code anchors, proof markers, docs, and support-boundary text.
- Demo remains a sample/proof surface and does not become a package contract boundary.
- Focused Demo tests verify scenario catalog, documentation, and proof closure.

### REL-01 — Contracts Documentation And Release Proof

Users can verify the supported v0.77 contracts through docs, public API inventory, baseline gates, and CI-sensitive tests.

**Acceptance:**
- Public docs and examples explain supported v0.77 contracts without naming external inspiration projects.
- All v0.77 requirements map to phases and proof markers.
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
| CMD-01 | 458, 459 |
| EDIT-01 | 460 |
| TPL-01 | 461 |
| SEL-01 | 462 |
| NAV-01 | 463 |
| COOK-01 | 464 |
| REL-01 | 465 |

**Coverage:**
- v0.77 requirements: 7 total
- Mapped to phases: 7
- Unmapped: 0

---
*Last updated: 2026-05-01 after starting v0.77*
