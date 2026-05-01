# AsterGraph

## What This Is

AsterGraph is a modular .NET desktop node graph SDK organized around an editor kernel, a scene and interaction layer, and UI adapters. Avalonia is the shipped hosted adapter today. The product direction is to turn the prerelease beta line into a platform-grade embedded graph editor for .NET hosts without collapsing host/runtime boundaries.

## Core Value

External hosts can embed AsterGraph and get a high-performance, definition-driven desktop node graph library with professional canvas depth, composable interactions, customizable nodes and edges, designer workbench UX, inspection, runtime feedback, local plugin trust, professional examples, and proof-backed cross-platform desktop verification.

## Current Milestone

v0.78.0-beta Professional Desktop Node Graph Component Platform

**Goal:** Take the next larger step from semantic authoring into a professional desktop node graph component platform: canvas interaction architecture, rendering/viewport performance, node and edge customization, spatial authoring workflows, cookbook showcase, and release-grade contracts.

**Target features:**
- **M0 — Canvas Interaction Audit:** map pan, zoom, select, drag, connect, resize, context menu, focus, keyboard, and command interaction ownership before implementation.
- **M1 — Rendering And Viewport Pipeline:** strengthen large-graph rendering, visible-scene projection, invalidation, minimap cadence, hit testing, and latency evidence.
- **M2 — Node And Edge Customization Surface:** deepen supported node visual, port handle, edge overlay, connection style, inspector/editor, and host-owned extension points.
- **M3 — Layout And Spatial Authoring Workbench:** productize layout, alignment, snapping, group/subgraph, selection transform, and spatial editing workflows.
- **M4 — Cookbook Component Showcase:** turn the Avalonia cookbook into a component showcase with code, graph proof, demo behavior, and support boundaries.
- **M5 — Release Proof And API Governance:** close public API inventory, docs, proof markers, net8/net9/net10 gates, beads, Dolt, and Git handoff.

**Status:** Active locally. Epic bead: `avalonia-node-map-v78`.

## Current State

v0.73.0-beta CI Reliability And Release Gate Recovery is complete (2026-04-30). It restored the core CI lanes, added explicit net8/net9/net10 framework coverage, repaired the release-validation ScaleSmoke budget failure, and verified GitHub Actions run `25159303518` green across all jobs.

v0.74.0-beta Cookbook Scenario Depth And Component Polish is complete locally (2026-04-30). It aligned cookbook scenario coverage, selected graph/context cues, source-backed route clarity, professional interaction facets, docs, and Demo proof markers without widening runtime architecture.

v0.75.0-beta Cross-Platform High-Performance Desktop Node Graph Library is complete locally (2026-04-30). It closed library-grade rendering/viewport, interaction, customization, packaging, professional examples, and release proof gates.

v0.76.0-beta Professional Canvas Engine And Authoring Workbench is complete locally (2026-05-01). It shifted from proof-backed library surface into deeper professional canvas capabilities: virtualized scene/indexing, edge routing, groups/subgraphs, layout services, designer workbench UX, extension contracts, and release proof.

v0.77.0-beta Semantic Authoring And Command Platform is complete locally (2026-05-01). It turned the professional canvas/workbench foundation into a host-extensible authoring platform with semantic commands, advanced editing operations, reusable templates, precise selection transforms, navigation/search workflows, cookbook proof, and public API release proof.

v0.78.0-beta Professional Desktop Node Graph Component Platform is active (2026-05-01). It should turn the semantic authoring platform into a more complete reusable desktop node graph component with stronger interaction architecture, rendering/viewport performance, customization seams, spatial authoring workflows, cookbook showcase, and release proof.

## Requirements

### Validated

- Public repo hygiene, bilingual docs, semver-aligned public proof flow, and four published package boundaries are established.
- Session-first runtime ownership and the canonical `CreateSession(...)` / `Create(...)` adoption routes are shipped.
- Avalonia is the shipped hosted adapter; WPF remains validation-only and partial.
- Plugin discovery, load-state inspection, and host-owned trust policy exist for trusted in-process plugins.
- `AsterGraphHostBuilder` provides a thin hosted facade that delegates to the existing canonical editor/session and Avalonia view factories.
- README, quick-start docs, ConsumerSample, Demo scenario launch, public versioning docs, workbench defaults, authoring builders, runtime overlay, layout/readability, local plugin, large-graph UX, adapter-2 validation, adoption/API stabilization, interaction reliability, discoverability, adopter-driven workbench evidence, CI reliability, cookbook depth, and v0.75 library-grade proof are defended by tests/proof markers.
- v0.75 established rendering/viewport performance proof, interaction contracts, extension seams, host packaging proof, professional examples, public API gate alignment, and release closure markers.
- v0.76 established virtualized scene/indexing, route geometry/evidence, groups/subgraphs, layout services, designer workbench UX, navigator/outline queries, public API baseline alignment, and release proof.

### Active

- **INT-01:** Users get professional canvas interactions for pan, zoom, select, drag, connect, resize, focus, context menus, and keyboard flows through coherent source-backed ownership.
- **RENDER-01:** Users can work with large graphs through measured rendering, viewport, invalidation, minimap, and hit-test behavior.
- **CUSTOM-01:** Users can customize nodes, ports, edges, overlays, inspectors, and editor affordances through supported package contracts.
- **SPACE-01:** Users can compose layout, alignment, snapping, groups/subgraphs, selection transforms, and spatial editing as one professional workbench flow.
- **COOK-02:** Users can learn the component platform through cookbook recipes that pair code, graph proof, demo behavior, and support boundaries.
- **REL-02:** Users can verify v0.78 through public API inventory, docs, baseline gates, .NET 8/9/10 CI-sensitive tests, beads, Dolt, and Git handoff.

### Out of Scope

- Runtime architecture rewrites or replacing the existing renderer/session ownership model.
- Compatibility or fallback layers.
- Treating Demo-only scaffolding as a supported package boundary.
- WPF parity, WinUI, MAUI, web adapter expansion, marketplace, remote plugins, plugin sandboxing, or execution engines.
- Generated runnable code execution inside the Demo.
- Macro/query/scripting systems for commands or discovery.
- New graph-size support claims beyond defended evidence.
- Naming external inspiration projects in project docs, public docs, or planning artifacts.
- Collaborative multi-user editing, CRDT/OT synchronization, cloud storage, or remote session hosting.

### Deferred

- Packaged analyzer work for retained/compatibility-only usage.
- Generated per-release API diff publishing.
- Broader virtualization architecture beyond the defended v0.76 performance envelope.
- WPF support expansion until real adopter evidence exists.
- Marketplace, remote plugin distribution, unload lifecycle, and sandboxed plugin execution.
- Rich in-app code preview, clipboard integration, generated recipe pages, and screenshot validation for every recipe.

## Context

- The public package line remains `0.11.0-beta`; local planning labels are internal and must not be confused with NuGet/package SemVer.
- v0.73 restored CI reliability and net8/net9/net10 GitHub coverage.
- v0.74 completed the cookbook proof surface: scenario coverage, graph/content cues, source-backed routes, professional interaction facets, docs, and proof markers.
- v0.75 completed the first library-grade step: rendering/viewport proof, interaction contracts, extension seams, host packaging, professional examples, and release gate closure.
- v0.76 completed professional canvas depth: scalable scene indexing, edge routing, groups/subgraphs, layout services, workbench-grade authoring UX, and public API proof.
- The new bottleneck is component-platform depth: professional interaction architecture, high-performance rendering/viewport behavior, deeper customization seams, spatial authoring workflows, cookbook showcase quality, and release proof.
- Demo cookbook still matters, but it now proves professional scenarios for the SDK rather than defining supported package contracts.
- Supported SDK contracts stay in packages, public APIs, docs, ConsumerSample, starter templates, and proof markers.
- Performance claims must stay tied to measured tests and proof output.

## Constraints

- **Public repo hygiene:** `.planning`, AI workflow traces, and local GSD artifacts remain untracked unless a phase explicitly force-adds its handoff files.
- **Prerelease boundary:** preserve the four-package split plus canonical `CreateSession(...)` and `Create(...)` routes.
- **No fallback layers:** do not hide missing behavior behind compatibility, polling, or degradation paths.
- **Host boundary:** Demo may prove behavior, but package APIs and supported samples define the contract.
- **Trust wording:** trusted in-process plugins only; do not imply sandboxing or untrusted-code isolation.
- **Performance claims:** every large-graph, viewport, render, layout, and interaction claim must be backed by repeatable evidence.
- **Cross-platform wording:** defend Windows/Linux/macOS host routes only where tests or CI evidence exist.
- **No external-name leakage:** milestone docs must describe target capabilities directly and must not name the external inspiration project or its packages.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Keep AsterGraph framed as `Editor Kernel + Scene/Interaction + UI Adapter` | This matches the existing architecture and keeps Avalonia from becoming the product boundary | Active |
| Keep `CreateSession(...)`, `IGraphEditorSession`, DTO/snapshot queries, diagnostics, automation, and plugin inspection as the canonical route | Host portability is blocked more by surface ambiguity than by another runtime rewrite | Active |
| Treat retained view-model/view APIs as migration support | They remain useful, but they should not become the primary public extension story | Active |
| Make v0.74 Cookbook Scenario Depth And Component Polish | The cookbook/professional Demo needed direct alignment between graph behavior, source code, routes, and proof evidence | Completed 2026-04-30 |
| Make v0.75 Cross-Platform High-Performance Desktop Node Graph Library | The product bottleneck was library-grade rendering, interaction, customization, packaging, examples, and cross-platform proof | Completed 2026-04-30 |
| Make v0.76 Professional Canvas Engine And Authoring Workbench | The product bottleneck was professional canvas depth: virtualized scene/indexing, routing, groups/subgraphs, layout services, designer workbench UX, extension contracts, and proof | Completed 2026-05-01 |
| Make v0.77 Semantic Authoring And Command Platform | The product bottleneck was host-extensible semantic authoring: commands, editing operations, templates, selection transforms, navigation/search, cookbook proof, and release contracts | Completed 2026-05-01 |
| Make v0.78 Professional Desktop Node Graph Component Platform | The next bottleneck is component-platform depth: interaction engine quality, rendering/viewport performance, customization seams, spatial authoring, cookbook showcase, and release contracts | Active |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition:**
1. Requirements invalidated? Move them to deferred or out-of-scope with a reason.
2. Requirements validated? Move them into the validated section with phase references.
3. New requirements emerged? Add to Active.
4. Decisions to log? Add to Key Decisions.
5. "What This Is" still accurate? Update it if drifted.

**After each milestone:**
1. Full review of all sections.
2. Core Value check: verify the milestone still advanced the right product promise.
3. Audit deferred items: keep only intentionally unresolved follow-ups.
4. Update Context with the newest shipped state and next bottleneck.

---
*Last updated: 2026-05-01 after starting `v0.78.0-beta Professional Desktop Node Graph Component Platform`*
