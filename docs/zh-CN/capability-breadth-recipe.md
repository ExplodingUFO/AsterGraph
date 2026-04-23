# Capability Breadth Recipe

当你希望把 searchable grouped stencil、SVG/PNG/JPEG export breadth，以及共享的 node/edge quick tools 作为一条可复制的 hosted 路线一起接起来时，就看这份 recipe。
把它和 `ConsumerSample.Avalonia` 配在一起，因为这就是当前 beta evaluator ladder 里受防守的 hosted proof route。

## 它覆盖什么

- stock hosted Avalonia surface 上的 searchable grouped stencil
- canonical session command 路线上的 scene export breadth
- 由同一份 tool descriptor 投影出来的共享 node/edge quick tools

## 直接照抄这条 Hosted 路线

- 第 1 步：通过 `ConsumerSample.Avalonia`、`IGraphEditorSession.Queries.GetNodeTemplateSnapshots()` 和 stock hosted library chrome 投影 searchable grouped stencil。
- 第 2 步：通过 `IGraphEditorSession.Commands.TryExportSceneAsSvg(...)`、`IGraphEditorSession.Commands.TryExportSceneAsImage(...)` 和 `GraphEditorSceneImageExportFormat.Png` 收口 SVG 加 raster export breadth。
- 第 3 步：通过 `AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(...)`、`IGraphEditorSession.Queries.GetToolDescriptors(...)` 和 `AsterGraph.ConsumerSample.Avalonia -- --proof` 投影共享 node/edge quick tools。

## Proof 合同

用下面这条命令验证路线：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

预期 capability-breadth proof marker：

- `CAPABILITY_BREADTH_STENCIL_OK:True`
- `CAPABILITY_BREADTH_EXPORT_OK:True`
- `CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True`
- `CAPABILITY_BREADTH_OK:True`

## 相关文档

- [Consumer Sample](./consumer-sample.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Host Integration](./host-integration.md)
