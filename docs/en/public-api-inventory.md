# Public API Inventory

This inventory is the maintainer-facing map for the public AsterGraph package surface. It does not create a new runtime model; it classifies the already published surface so hosts can tell which APIs are canonical, hosted composition helpers, retained migration bridges, compatibility-only shims, or internal implementation details.

Use this page with [Host Integration](./host-integration.md) and [Extension Contracts](./extension-contracts.md). If the documents disagree, fix the docs before widening the public surface.

## Support Tiers

| Tier | Meaning | Consumer guidance |
| --- | --- | --- |
| Stable canonical | Runtime/session contracts that define the supported host integration route. | Use for new work and custom UI/native shell integration. |
| Supported hosted helper | Thin composition helpers over the canonical route and shipped Avalonia adapter. | Use when the stock Avalonia route is enough. |
| Retained migration | Existing MVVM/view surfaces kept so older hosts can migrate in batches. | Use only as a bridge while moving toward the canonical route. |
| Compatibility-only | Obsolete or legacy-shaped helpers that have canonical replacements. | Do not use for new work; follow the replacement guidance. |
| Internal-only | Implementation details, internal namespaces, tests, samples, and proof tools. | Not a public support contract. |

## Package Inventory

| Package | Stable canonical | Supported hosted helper | Retained migration | Compatibility-only | Internal-only boundary |
| --- | --- | --- | --- | --- | --- |
| `AsterGraph.Abstractions` | Node definitions, port definitions, provider/plugin-facing contracts, identifiers, metadata DTOs used by the canonical route. | None. | None. | None currently published as a primary support tier. | Implementation helpers not exposed through package docs. |
| `AsterGraph.Core` | Graph document, serialization-oriented model contracts, compatibility rule inputs, and shared data types used by editor/session composition. | None. | None. | Legacy conversion/compatibility helpers only where a newer runtime-first route exists. | Core internals and persistence implementation details. |
| `AsterGraph.Editor` | `AsterGraphEditorFactory.CreateSession(...)`, `IGraphEditorSession`, `IGraphEditorCommands`, `IGraphEditorQueries`, DTO/snapshot queries, diagnostics, automation, runtime overlay snapshots/providers, layout plan snapshots/providers, plugin discovery/inspection, export services including raster export progress/cancel options. | `AsterGraphEditorFactory.Create(...)` as a hosted composition helper that still exposes the retained facade. | `GraphEditorViewModel`, `GraphEditorViewModel.Session`, retained menu/context-menu hooks used by migrating hosts. | `IGraphEditorQueries.GetCompatibleTargets(...)`, `CompatiblePortTarget`, older MVVM-shaped helpers where `GetCompatiblePortTargets(...)` or command/query snapshots exist. | `Runtime.Internal`, `Kernel.Internal`, projection/apply internals, proof-only helpers. |
| `AsterGraph.Avalonia` | Adapter projection over the canonical editor/session route and hosted factories that consume the same runtime owner. | `AsterGraphAvaloniaViewFactory.Create(...)`, `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, `AsterGraphMiniMapViewFactory`, `AsterGraphHostBuilder`. | `GraphEditorView` embedding for hosts that still use the retained editor facade. | Adapter-specific compatibility glue only when it bridges existing hosts to the canonical route. | Control internals, templates, interaction session internals, visual-only implementation details. |

## Route Mapping

| Host route | Support tier | Primary symbols | Notes |
| --- | --- | --- | --- |
| Runtime-only/custom UI | Stable canonical | `AsterGraphEditorFactory.CreateSession(...)`, `IGraphEditorSession`, `IGraphEditorCommands.TryCreateConnectedNodeFromPendingConnection(...)`, `IGraphEditorCommands.TryInsertNodeIntoConnection(...)`, `IGraphEditorCommands.TryDeleteSelectionAndReconnect(...)`, `IGraphEditorCommands.TryDetachSelectionFromConnections(...)`, `IGraphEditorCommands.SetConnectionSelection(...)`, `IGraphEditorCommands.TryDeleteSelectedConnections()`, `IGraphEditorCommands.TrySliceConnections(...)`, `IGraphEditorQueries.GetSelectedNodeConnectionIds()`, `IGraphEditorQueries.GetRuntimeOverlaySnapshot()`, `IGraphEditorQueries.CreateLayoutPlan(...)`, `GraphEditorSelectionSnapshot.SelectedConnectionIds`, `IGraphRuntimeOverlayProvider`, `GraphEditorRuntimeOverlaySnapshot`, `IGraphLayoutProvider`, `GraphLayoutPlan`, `GraphEditorSceneImageExportOptions`, `GraphEditorSceneImageExportProgressSnapshot`, `GraphEditorSceneImageExportScope` | Default for new custom UI and native shell work; authoring-productivity commands, wire-selection snapshots, host-owned runtime feedback overlays, provider-owned layout plans, and raster export progress/cancel/scope evidence stay on the canonical session route. |
| Shipped Avalonia UI | Supported hosted helper | `AsterGraphEditorFactory.Create(...)`, `AsterGraphAvaloniaViewFactory.Create(...)` | Uses the same runtime owner; not a second runtime model. |
| Thin hosted builder | Supported hosted helper | `AsterGraphHostBuilder.Create(...).BuildAvaloniaView()` | Reduces common Avalonia setup boilerplate while delegating to canonical factories. |
| Retained migration bridge | Retained migration | `GraphEditorViewModel`, `GraphEditorView`, `GraphEditorViewModel.Session` | Only for older hosts migrating in batches. |
| Legacy compatible-target query | Compatibility-only | `IGraphEditorQueries.GetCompatibleTargets(...)`, `CompatiblePortTarget` | Prefer `GetCompatiblePortTargets(...)` and `GraphEditorCompatiblePortTargetSnapshot`. |

## Drift Rules

- New public host-facing symbols must be classified in this inventory before release.
- Public API changes must update `eng/public-api-baseline.txt` intentionally; unclassified drift fails the release gate.
- Stable canonical additions must be reflected in [Host Integration](./host-integration.md) or [Extension Contracts](./extension-contracts.md).
- Retained migration and compatibility-only additions must include replacement guidance.
- Compatibility-only APIs must be marked obsolete when a canonical replacement exists.
- Internal implementation details must not be promoted by README, quick-start, or release notes.

## Maintainer Checklist

Before a release:

1. Check that package docs, README, quick-start, and release notes use the same route names.
2. Check that retained and compatibility-only APIs still point to canonical replacements.
3. Check that `AsterGraphHostBuilder` stays documented as a thin hosted helper, not a runtime model.
4. Check that WPF wording stays validation-only unless the adapter support matrix changes.
