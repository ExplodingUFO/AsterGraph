# Widened Surface Performance Recipe

Use this recipe when you want one copyable hosted tuning handoff for the widened authoring surface instead of a new proof lane.
Pair it with `ConsumerSample.Avalonia` for hosted metrics and with `ScaleSmoke` for the defended large-tier budgets.

## What It Covers

- hosted widened-surface metrics on the canonical `ConsumerSample.Avalonia -- --proof` route
- defended `large` authoring budgets for stencil, command-surface refresh, and quick tools
- defended `large` export and reload budgets that keep the widened route tied to `ScaleSmoke`

## Copy This Hosted Tuning Path

- Step 1: Run `ConsumerSample.Avalonia -- --proof`, keep `WIDENED_SURFACE_PERFORMANCE_OK:True`, and review `HOST_NATIVE_METRIC:stencil_search_ms`, `HOST_NATIVE_METRIC:command_surface_refresh_ms`, `HOST_NATIVE_METRIC:node_tool_projection_ms`, and `HOST_NATIVE_METRIC:edge_tool_projection_ms`.
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
