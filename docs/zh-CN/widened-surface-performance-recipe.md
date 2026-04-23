# Widened Surface Performance Recipe

当你希望把扩展后的 authoring surface 收成一条可复制的宿主调优 handoff，而不是再开一条新的 proof lane 时，就看这份 recipe。
它把 `ConsumerSample.Avalonia` 的宿主指标和 `ScaleSmoke` 的 defended `large` 预算收在同一条路线上。

## 它覆盖什么

- canonical `ConsumerSample.Avalonia -- --proof` 路线上的宿主 widened-surface 指标
- `large` 层级上受防守的 stencil、command-surface refresh 和 quick-tool authoring 预算
- 继续把 widened route 绑定到 `ScaleSmoke` 的 `large` export/reload 预算

## 直接照抄这条 Hosted 调优路径

- 第 1 步：运行 `ConsumerSample.Avalonia -- --proof`，守住 `WIDENED_SURFACE_PERFORMANCE_OK:True`，并查看 `HOST_NATIVE_METRIC:stencil_search_ms`、`HOST_NATIVE_METRIC:command_surface_refresh_ms`、`HOST_NATIVE_METRIC:node_tool_projection_ms` 和 `HOST_NATIVE_METRIC:edge_tool_projection_ms`。
- 第 2 步：运行 `ScaleSmoke -- --tier large`，并在同一条 `ScaleSmoke` 路线上继续守住 `SCALE_AUTHORING_BUDGET_OK:large:True:none` 和 `SCALE_EXPORT_BUDGET_OK:large:True:none`。

## Proof 合同

用下面这条命令验证宿主路线：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

用下面这条命令验证 defended 的 large-tier 预算：

```powershell
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo -- --tier large
```

预期 proof marker：

- `WIDENED_SURFACE_PERFORMANCE_OK:True`
- `HOST_NATIVE_METRIC:stencil_search_ms=...`
- `HOST_NATIVE_METRIC:command_surface_refresh_ms=...`
- `HOST_NATIVE_METRIC:node_tool_projection_ms=...`
- `HOST_NATIVE_METRIC:edge_tool_projection_ms=...`
- `SCALE_AUTHORING_BUDGET_OK:large:True:none`
- `SCALE_EXPORT_BUDGET_OK:large:True:none`

## 相关文档

- [Consumer Sample](./consumer-sample.md)
- [Capability Breadth Recipe](./capability-breadth-recipe.md)
- [ScaleSmoke 基线](./scale-baseline.md)
