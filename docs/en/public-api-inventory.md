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
| `AsterGraph.Abstractions` | Node definitions, port definitions, provider/plugin-facing contracts, identifiers, metadata DTOs used by the canonical route, and thin definition builders that produce those DTOs. | None. | None. | None currently published as a primary support tier. | Implementation helpers not exposed through package docs. |
| `AsterGraph.Core` | Graph document, serialization-oriented model contracts, group container/collapse semantics, compatibility rule inputs, thin implicit-conversion rule builder, and shared data types used by editor/session composition. | None. | None. | Legacy conversion/compatibility helpers only where a newer runtime-first route exists. | Core internals and persistence implementation details. |
| `AsterGraph.Editor` | `AsterGraphEditorFactory.CreateSession(...)`, `IGraphEditorSession`, `IGraphEditorCommands`, `IGraphEditorQueries`, DTO/snapshot queries, diagnostics, automation, validation snapshots, runtime overlay snapshots/providers, layout plan snapshots/providers, viewport visible scene projection snapshots/projector, hierarchy/group snapshots including collapsed-boundary connection projection, navigator/outline snapshots, plugin discovery/inspection, export services including raster export progress/cancel options. | `AsterGraphEditorFactory.Create(...)` as a hosted composition helper that still exposes the retained facade. | `GraphEditorViewModel`, `GraphEditorViewModel.Session`, retained menu/context-menu hooks used by migrating hosts. | `IGraphEditorQueries.GetCompatibleTargets(...)`, `CompatiblePortTarget`, older MVVM-shaped helpers where `GetCompatiblePortTargets(...)` or command/query snapshots exist. | `Runtime.Internal`, `Kernel.Internal`, projection/apply internals, proof-only helpers. |
| `AsterGraph.Avalonia` | Adapter projection over the canonical editor/session route and hosted factories that consume the same runtime owner. | `AsterGraphAvaloniaViewFactory.Create(...)`, `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, `AsterGraphMiniMapViewFactory`, `AsterGraphHostBuilder`, `AsterGraphWorkbenchOptions`, hosted workbench layout/panel-state options, and hosted performance policy budget markers. | `GraphEditorView` embedding for hosts that still use the retained editor facade. | Adapter-specific compatibility glue only when it bridges existing hosts to the canonical route. | Control internals, templates, interaction session internals, visual-only implementation details. |

## Route Mapping

| Host route | Support tier | Primary symbols | Notes |
| --- | --- | --- | --- |
| Runtime-only/custom UI | Stable canonical | `AsterGraphEditorFactory.CreateSession(...)`, `IGraphEditorSession`, `IGraphEditorCommands.TryCreateConnectedNodeFromPendingConnection(...)`, `IGraphEditorCommands.TryInsertNodeIntoConnection(...)`, `IGraphEditorCommands.TryDeleteSelectionAndReconnect(...)`, `IGraphEditorCommands.TryDetachSelectionFromConnections(...)`, `IGraphEditorCommands.SetConnectionSelection(...)`, `IGraphEditorCommands.FitSelectionToViewport(...)`, `IGraphEditorCommands.FocusSelection(...)`, `IGraphEditorCommands.FocusCurrentScope(...)`, `IGraphEditorCommands.TryFocusValidationIssue(...)`, `IGraphEditorCommands.TryDeleteSelectedConnections()`, `IGraphEditorCommands.TrySliceConnections(...)`, `IGraphEditorCommands.PreviewLayoutPlan(...)`, `IGraphEditorCommands.TryApplyLayoutPlan(...)`, `IGraphEditorCommands.TryApplyLayoutRequest(...)`, `IGraphEditorCommands.TryApplySelectionLayout(...)`, `IGraphEditorCommands.TrySnapSelectedNodesToGrid(...)`, `IGraphEditorCommands.TrySnapAllNodesToGrid(...)`, `GraphSelectionLayoutOperation`, `IGraphEditorQueries.GetSelectedNodeConnectionIds()`, `IGraphEditorQueries.GetValidationSnapshot()`, `IGraphEditorQueries.GetRuntimeOverlaySnapshot()`, `IGraphEditorQueries.CreateLayoutPlan(...)`, `IGraphEditorQueries.GetNavigatorOutlineSnapshot()`, `GraphEditorNavigatorOutlineSnapshot`, `GraphEditorNavigatorOutlineItemSnapshot`, `GraphEditorNavigatorOutlineItemKind`, `GraphEditorCompatiblePortTargetSnapshot.PortHandleId`, `GraphEditorCompatiblePortTargetSnapshot.PortGroupName`, `GraphEditorCompatiblePortTargetSnapshot.ConnectionHint`, `GraphEditorPendingConnectionSnapshot.TargetNodeId`, `GraphEditorPendingConnectionSnapshot.TargetPortId`, `GraphEditorPendingConnectionSnapshot.IsTargetCompatible`, `GraphEditorPendingConnectionSnapshot.ValidationMessage`, `GraphEditorSelectionSnapshot.SelectedConnectionIds`, `GraphEditorValidationSnapshot`, `GraphEditorValidationIssueSnapshot`, `GraphEditorValidationIssueSnapshot.HelpTarget`, `GraphEditorValidationHelpTargetSnapshot`, `IGraphRuntimeOverlayProvider`, `GraphEditorRuntimeOverlaySnapshot`, `IGraphLayoutProvider`, `GraphLayoutPlan`, `GraphEditorSceneImageExportOptions`, `GraphEditorSceneImageExportProgressSnapshot`, `GraphEditorSceneImageExportScope` | Default for new custom UI and native shell work; authoring-productivity commands, validation/readiness feedback, metadata-backed validation help targets, port handle/group/hint metadata, pending invalid-target feedback, viewport selection/focus commands, wire-selection snapshots, source-backed navigator/outline projection, host-owned runtime feedback overlays, provider-owned layout plan preview/apply, selection layout, snap-to-grid mutations, and raster export progress/cancel/scope evidence stay on the canonical session route. |
| v0.77 authoring platform route | Stable canonical | `IGraphEditorQueries.GetCommandRegistry()`, `GraphEditorCommandRegistryEntrySnapshot`, `IGraphEditorCommands.TryDeleteSelectionAndReconnect(...)`, `IGraphEditorCommands.TryDetachSelectionFromConnections(...)`, `IGraphEditorCommands.TryApplyFragmentTemplatePreset(...)`, `IGraphEditorQueries.GetFragmentTemplateSnapshots()`, `GraphEditorFragmentTemplateSnapshot`, `IGraphEditorQueries.GetSelectionTransformSnapshot(...)`, `GraphEditorSelectionTransformQuery`, `GraphEditorSelectionTransformSnapshot`, `GraphEditorSelectionTransformItemSnapshot`, `IGraphEditorCommands.TryApplySelectionLayout(...)`, `IGraphEditorCommands.TrySnapSelectedNodesToGrid(...)`, `IGraphEditorQueries.SearchGraphItems(...)`, `GraphEditorGraphItemSearchQuery`, `GraphEditorGraphItemSearchSnapshot`, `GraphEditorGraphItemSearchResultSnapshot`, `GraphEditorGraphItemSearchResultKind`, `IGraphEditorQueries.GetViewportBookmarks()`, `GraphEditorViewportBookmarkCollectionSnapshot`, `GraphEditorViewportBookmarkSnapshot`, `IGraphEditorCommands.TryFocusGraphItemSearchResult(...)`, `IGraphEditorCommands.TryAddViewportBookmark(...)`, `IGraphEditorCommands.TryRemoveViewportBookmark(...)`, `IGraphEditorCommands.TryActivateViewportBookmark(...)` | v0.77 canonical route for command registry projection, semantic editing, fragment template presets, selection transform/snap, graph search, bookmark, focus, and cookbook proof. These APIs stay on the session command/query route and are proven by the editor contract tests and Demo cookbook route; they do not add generated runnable code, workflow execution, macro/query language, marketplace, sandbox, fallback behavior, or compatibility-layer expansion. |
| v0.79 advanced canvas operations route | Stable canonical | `IGraphEditorQueries.GetSelectionRectangleSnapshot(...)`, `GraphEditorSelectionRectangleSnapshot`, `GraphEditorSelectionRectangleSnapshot.NodeIds`, `GraphEditorSelectionRectangleSnapshot.ConnectionIds`, `IGraphEditorCommands.SelectAll(...)`, `IGraphEditorCommands.SelectNone(...)`, `IGraphEditorCommands.InvertSelection(...)` | v0.79 canonical route for selection rectangle queries, marquee-backed selection, and bulk selection commands. These APIs stay on the session command/query route and are proven by editor contract tests and Avalonia overlay tests; they do not add a spatial index, alternate selection model, or generated runnable code. |
| Shipped Avalonia UI | Supported hosted helper | `AsterGraphEditorFactory.Create(...)`, `AsterGraphAvaloniaViewFactory.Create(...)` | Uses the same runtime owner; not a second runtime model. |
| Thin hosted builder | Supported hosted helper | `AsterGraphHostBuilder.Create(...).UseDefaultWorkbench().BuildAvaloniaView()`, plus `UseBehaviorOptions(...)`, `UseContextMenuAugmentor(...)`, `UseNodePresentationProvider(...)`, `UseToolProvider(...)`, `UseRuntimeOverlayProvider(...)`, and `UseLayoutProvider(...)` | Reduces common Avalonia setup boilerplate while delegating to canonical factories; the pass-throughs expose existing `AsterGraphEditorOptions` seams and do not create a new runtime model. `AsterGraphWorkbenchOptions` only controls stock hosted chrome. |
| Hosted customization surface | Supported hosted helper | `AsterGraphPresentationOptions`, `IGraphNodeVisualPresenter`, `GraphNodeVisual.PortAnchors`, `GraphNodeVisual.ConnectionTargetAnchors`, `INodeParameterEditorRegistry`, `IGraphEditorQueries.GetConnectionGeometrySnapshots()`, `GraphEditorConnectionRouteStyle`, `GraphEditorConnectionRouteEvidenceSnapshot`, `IGraphRuntimeOverlayProvider`, `IGraphEditorQueries.GetRuntimeOverlaySnapshot()` | The supported custom node, handle/anchor, edge overlay, parameter-editor, inspector, and runtime decoration route. Custom edge visuals use stock connection styling or host-owned overlays from geometry snapshots, including route style and bounded obstacle/crossing evidence; `NodeCanvas` internal layers, including `OverlayLayer`, are not public API. |
| Hosted performance mode | Supported hosted helper | `AsterGraphWorkbenchPerformanceMode`, `AsterGraphWorkbenchPerformancePolicy`, `AsterGraphWorkbenchOptions.PerformanceMode` | Tunes stock Avalonia workbench projection only; it is not a runtime graph contract or execution mode. |
| Viewport projection proof | Stable canonical | `ViewportVisibleSceneProjection`, `ViewportVisibleSceneProjector`, `ViewportVisibleSceneProjector.DefaultOverscanWorldUnits`, `ViewportVisibleSceneProjection.ToBudgetMarker(...)`, `ViewportVisibleSceneProjection.ToInvalidationBudgetMarker(...)`, `AsterGraphWorkbenchPerformancePolicy.ToMiniMapBudgetMarker()`, `AsterGraphWorkbenchPerformancePolicy.MiniMapRefreshCadence` | Provides host-readable viewport and minimap budget evidence for large graph validation. It reports counts, invalidation diff markers, and world bounds only; it is not a rendering engine, query language, or background indexing service. |
| Hierarchy and group projection | Stable canonical | `GraphNodeGroup.IsContainer`, `GraphNodeGroup.ProjectsMemberNodes`, `GraphEditorNodeGroupSnapshot.IsContainer`, `GraphEditorNodeGroupSnapshot.ProjectsMemberNodes`, `GraphEditorHierarchyStateSnapshot.Connections`, `GraphEditorHierarchyConnectionSnapshot` | Publishes collapsed-container visibility and boundary-connection projection for hosts that build custom UI from the session route. It does not add a second group model or adapter-specific runtime API. |
| Hosted workbench layout state | Supported hosted helper | `AsterGraphWorkbenchLayoutPreset`, `AsterGraphWorkbenchPanelState`, `AsterGraphWorkbenchOptions.ForPreset(...)`, `AsterGraphWorkbenchOptions.ResetLayout()` | Persists and resets stock Avalonia workbench preset/panel state only; it is not a runtime layout engine, execution mode, or WPF parity promise. |
| Thin definition builders | Stable canonical | `NodeDefinitionBuilder`, `PortDefinitionBuilder`, `PortDefinition.HandleId`, `PortDefinition.ConnectionHint`, `GraphPort.HandleId`, `GraphPort.ConnectionHint`, `NodeParameterDefinitionBuilder`, `ImplicitConversionRuleBuilder` | Convenience constructors and stable port/handle metadata only; each builder terminates in existing DTOs and does not create a second authoring schema or runtime model. |
| Retained migration bridge | Retained migration | `GraphEditorViewModel`, `GraphEditorView`, `GraphEditorViewModel.Session` | Only for older hosts migrating in batches. |
| Legacy compatible-target query | Compatibility-only | `IGraphEditorQueries.GetCompatibleTargets(...)`, `CompatiblePortTarget` | Prefer `GetCompatiblePortTargets(...)` and `GraphEditorCompatiblePortTargetSnapshot`. |

## Release Handoff

- Release handoff: stable canonical surfaces are the default route, retained migration surfaces are bridge-only, and compatibility-only or obsolete surfaces must keep replacement guidance visible.
- Keep `PUBLIC_API_SURFACE_OK`, `PUBLIC_API_SCOPE_OK`, and `PUBLIC_API_GUIDANCE_OK` in the same release proof block as `ASTERGRAPH_TEMPLATE_SMOKE_OK` and `TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK`.
- Release-candidate proof markers: `API_RELEASE_CANDIDATE_PROOF_OK:True`, `PUBLIC_API_GUIDANCE_HANDOFF_OK:True`, and `RELEASE_BOUNDARY_STABILITY_OK:True`.
- v0.61 API stabilization markers: `PUBLIC_API_DIFF_GATE_OK:True`, `PUBLIC_API_USAGE_GUIDANCE_OK:True`, and `PUBLIC_API_STABILITY_SCOPE_OK:True`.
- v0.61 adoption/API handoff markers: `ADOPTION_API_STABILIZATION_HANDOFF_OK:True`, `ADOPTION_API_SCOPE_BOUNDARY_OK:True`, and `V061_MILESTONE_PROOF_OK:True`.
- v0.75 customization handoff markers: `CUSTOM_EXTENSION_NODE_PRESENTER_LIFECYCLE_OK:True`, `CUSTOM_EXTENSION_ANCHOR_SURFACE_OK:True`, `CUSTOM_EXTENSION_EDGE_OVERLAY_OK:True`, `CUSTOM_EXTENSION_RUNTIME_INSPECTOR_OK:True`, `CUSTOM_EXTENSION_SCOPE_BOUNDARY_OK:True`, and `CUSTOM_EXTENSION_SURFACE_OK:True`.
- v0.79 advanced operations handoff markers: `SELECTION_RECTANGLE_QUERY_OK:True`, `SELECTION_RECTANGLE_MARQUEE_OK:True`, `SELECTION_INVERT_ALL_NONE_OK:True`, `CANVAS_KEYBOARD_NAVIGATION_OK:True`, `ARROW_KEY_NUDGE_OK:True`, `ARROW_KEY_NEAREST_NODE_OK:True`, `AUTOMATION_PEER_SURFACE_OK:True`, `HOST_EVENT_SUBSCRIPTION_OK:True`, `EVENT_BATCHING_CADENCE_OK:True`, and `EVENT_MEMORY_LEAK_OK:True`.
- Do not add analyzer, adapter, or compatibility promises from this handoff; it only publishes the current package guidance and proof markers.

## Drift Rules

- New public host-facing symbols must be classified in this inventory before release.
- Public API changes must update `eng/public-api-baseline.txt` intentionally; unclassified drift fails the release gate.
- Stable canonical additions must be reflected in [Host Integration](./host-integration.md) or [Extension Contracts](./extension-contracts.md).
- Retained migration and compatibility-only additions must include replacement guidance.
- Compatibility-only APIs must be marked obsolete when a canonical replacement exists.
- Internal implementation details must not be promoted by README, quick-start, or release notes.

## Baseline Gate

`eng/validate-public-api-surface.ps1` is scoped to the four supported public packages: `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`. It intentionally excludes demos, samples, WPF validation adapters, tools, and tests from the package support contract.

Check the current baseline:

```powershell
.\eng\validate-public-api-surface.ps1 -Configuration Release -Framework net9.0
```

Regenerate the baseline after an intentional public API change:

```powershell
.\eng\validate-public-api-surface.ps1 -Configuration Release -Framework net9.0 -UpdateBaseline
```

The release proof must include `PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia` next to `PUBLIC_API_SURFACE_OK`, `PUBLIC_API_GUIDANCE_OK`, `PUBLIC_API_DIFF_GATE_OK:True`, `PUBLIC_API_USAGE_GUIDANCE_OK:True`, `PUBLIC_API_STABILITY_SCOPE_OK:True`, `ADOPTION_API_STABILIZATION_HANDOFF_OK:True`, `ADOPTION_API_SCOPE_BOUNDARY_OK:True`, and `V061_MILESTONE_PROOF_OK:True`.

## Maintainer Checklist

Before a release:

1. Check that package docs, README, quick-start, and release notes use the same route names.
2. Check that retained and compatibility-only APIs still point to canonical replacements.
3. Check that `AsterGraphHostBuilder` stays documented as a thin hosted helper, not a runtime model.
4. Check that WPF wording stays validation-only unless the adapter support matrix changes.
