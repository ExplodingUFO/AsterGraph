# Widened Surface Performance Recipe

Use this recipe when you want one copyable hosted tuning handoff for the widened authoring surface instead of a new proof lane.
Pair it with `ConsumerSample.Avalonia` for hosted metrics and with `ScaleSmoke` for the defended large-tier budgets.

## What It Covers

- hosted widened-surface metrics on the canonical `ConsumerSample.Avalonia -- --proof` route
- defended `large` authoring budgets for stencil, command-surface refresh, and quick tools
- defended `large` export and reload budgets that keep the widened route tied to `ScaleSmoke`

## Copy This Hosted Tuning Path

- Step 1: Run `ConsumerSample.Avalonia -- --proof`, keep `WIDENED_SURFACE_PERFORMANCE_OK:True`, and review `HOST_NATIVE_METRIC:stencil_search_ms`, `HOST_NATIVE_METRIC:command_surface_refresh_ms`, `HOST_NATIVE_METRIC:node_tool_projection_ms`, and `HOST_NATIVE_METRIC:edge_tool_projection_ms`.
- Step 1a: Keep hosted workbench performance mode on the same route: default `Balanced`, optional `Quality` / `Throughput`, and the proof markers `WORKBENCH_PERFORMANCE_MODE_OK:True`, `BALANCED_MODE_DEFAULT_OK:True`, `WORKBENCH_LOD_POLICY_OK:True`, and `PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True`.
- Step 1b: Keep the v0.59 large-graph UX baseline on this hosted route: `LARGE_GRAPH_UX_POLICY_OK:True`, `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`, and `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`. These markers aggregate existing hosted evidence; they are not a new graph-size support tier.
- Step 1c: Keep selected/hovered affordances under hosted viewport LOD policy: `VIEWPORT_LOD_POLICY_OK:True`, `SELECTED_HOVERED_ADORNER_SCOPE_OK:True`, `LARGE_GRAPH_BALANCED_UX_OK:True`, and `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`.
- Step 1d: Keep edge interaction proof bounded to existing edge quick-tool, toolbar, and geometry evidence: `EDGE_INTERACTION_CACHE_OK:True`, `EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`, `SELECTED_EDGE_FEEDBACK_OK:True`, and `EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`. These markers are not a runtime renderer contract.
- Step 1e: Keep mini-map and inspector projection evidence narrow: `MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True`, `INSPECTOR_NARROW_PROJECTION_OK:True`, `LARGE_GRAPH_PANEL_SCOPE_OK:True`, and `PROJECTION_PERFORMANCE_EVIDENCE_OK:True`. These markers are not a broad graph subscription contract.
- Step 1f: Keep the v0.59 Large Graph UX handoff markers tied to the phase 371-374 proof clusters: `LARGE_GRAPH_UX_HANDOFF_OK:True`, `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`, and `V059_MILESTONE_PROOF_OK:True`.
- Step 2: Run `ScaleSmoke -- --tier large` and keep `SCALE_AUTHORING_BUDGET_OK:large:True:none` plus `SCALE_EXPORT_BUDGET_OK:large:True:none` on the same `ScaleSmoke` route.

## Proof Contract

Validate the hosted route with:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

Validate the defended large-tier budget with:

```powershell
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo -- --tier large
```

Expected proof markers:

- `WIDENED_SURFACE_PERFORMANCE_OK:True`
- `WORKBENCH_PERFORMANCE_MODE_OK:True`
- `BALANCED_MODE_DEFAULT_OK:True`
- `WORKBENCH_LOD_POLICY_OK:True`
- `PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True`
- `LARGE_GRAPH_UX_POLICY_OK:True`
- `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`
- `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`
- `VIEWPORT_LOD_POLICY_OK:True`
- `SELECTED_HOVERED_ADORNER_SCOPE_OK:True`
- `LARGE_GRAPH_BALANCED_UX_OK:True`
- `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`
- `EDGE_INTERACTION_CACHE_OK:True`
- `EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`
- `SELECTED_EDGE_FEEDBACK_OK:True`
- `EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`
- `MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True`
- `INSPECTOR_NARROW_PROJECTION_OK:True`
- `LARGE_GRAPH_PANEL_SCOPE_OK:True`
- `PROJECTION_PERFORMANCE_EVIDENCE_OK:True`
- `LARGE_GRAPH_UX_HANDOFF_OK:True`
- `V059_MILESTONE_PROOF_OK:True`
- `HOST_NATIVE_METRIC:stencil_search_ms=...`
- `HOST_NATIVE_METRIC:command_surface_refresh_ms=...`
- `HOST_NATIVE_METRIC:node_tool_projection_ms=...`
- `HOST_NATIVE_METRIC:edge_tool_projection_ms=...`
- `SCALE_AUTHORING_BUDGET_OK:large:True:none`
- `SCALE_EXPORT_BUDGET_OK:large:True:none`

## Related Docs

- [Consumer Sample](./consumer-sample.md)
- [Capability Breadth Recipe](./capability-breadth-recipe.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
