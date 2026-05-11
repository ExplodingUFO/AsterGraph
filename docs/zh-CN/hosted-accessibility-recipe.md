# Hosted Accessibility Recipe

当你想在 canonical Avalonia 路线上复制一条宿主可控的键盘、焦点和可访问性 handoff 时，就用这份 recipe。
当前 beta evaluator ladder 上，`src/AsterGraph.Demo` 是这条 recipe 的 defended hosted proof route。

## 它覆盖什么

- hosted editor shell、canvas、搜索面和 shipped inspector chrome 上的 baseline automation name
- command palette 关闭后回到打开它的宿主 surface 的 focus round-trip
- 从同一套 action descriptor 投影出来的 command/tool 按钮可访问名称
- 同一条 hosted route 上的 authoring-surface metadata 和 edge text editor
- 一条继续留在 `src/AsterGraph.Demo`、`release validation lane` 和受限 support-bundle 记录上的 screen-reader-ready 评估路径

## 照这条 Hosted Path 复制

- 第 1 步：给 `GraphEditorView`、`NodeCanvas`、`GraphInspectorView`、`PART_StencilSearchBox`、`PART_OpenCommandPaletteButton`、`PART_CommandPaletteSearchBox` 和 `PART_ParameterSearchBox` 保留稳定的 automation name。
- 第 2 步：把 command palette 的键盘流程继续留在共享 hosted view 路线上，让 `Control+Shift+P` 打开 palette，并在关闭时把焦点还给先前的 host surface。
- 第 3 步：让 header command、palette command、node quick tool 和 edge quick tool 的可访问名称继续来自同一套 action descriptor，而不是再造一条只给 accessibility 用的动作模型。
- 第 4 步：把 selected-node parameter metadata 和 connection text editor 保留在同一条 hosted authoring route 上，最后用 `src/AsterGraph.Demo -- --proof` 收口。

## Screen-Reader-Ready 评估路径

- 先运行 `src/AsterGraph.Demo -- --proof`，并在受防守的 hosted 路线上守住 `HOSTED_ACCESSIBILITY_BASELINE_OK:True`、`HOSTED_ACCESSIBILITY_FOCUS_OK:True`、`HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`、`HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True` 和 `HOSTED_ACCESSIBILITY_OK:True`。
- 再按 [Beta Support Bundle](./support-bundle.md) 从 `src/AsterGraph.Demo` 生成本地证据附件；这份 support bundle 仍然是这条路线的受限 intake 附件。
- 在真实宿主 proof 之后运行 `release validation lane`，并把 `HOST_SAMPLE_AUTOMATION_OK:True`、`HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True` 和 `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True` 保留在同一条受限 intake 记录上。
- 如果你要做 Narrator、NVDA 或 VoiceOver 这类本地 screen-reader-ready 检查，就继续盯住同一批具名 hosted surface 和 control：`GraphEditorView`、`NodeCanvas`、`GraphInspectorView`、`PART_CommandPaletteSearchBox`、`PART_ParameterSearchBox`，以及投影出来的 command 按钮和 node/edge tool。
- 这条路径只提供本地评估证据，不扩大支持承诺，也不意味着 screen-reader 认证。

## Manual Assistive-Technology Validation Checklist

`ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN` 把 manual assistive-technology validation 与现有 headless automation proof 分开。只有在 `HOSTED_ACCESSIBILITY_OK:True` 和 release validation lane 通过之后才开始；这些证据能证明 automation names、focus routes、keyboard flow 和 command surfaces，但 live announcements 仍然属于 unverified live screen-reader behavior。

- Narrator on Windows：打开 hosted Demo route，按 Tab 经过 `GraphEditorView`、`NodeCanvas`、`GraphInspectorView`、`PART_CommandPaletteSearchBox` 和投影出来的 command buttons；记录 names、focus movement、command-palette close、validation focus buttons 以及 export/status text 是否按预期播报。
- NVDA on Windows：复跑同一条具名路线，并记录 browse/focus modes 是否暴露同样的 command labels、selected-node parameter metadata、edge text editor names 和 validation target help text。
- VoiceOver on macOS，或当前 host 上最接近的平台等价检查：复跑同一批具名 surfaces，并记录 focus order、command labels 与 status announcements 的差异。
- 把 manual notes 附在 Demo support bundle 旁边；如果失败，把它作为后续 implementation tracker 的 adopter evidence。

这份 checklist 只是 planning evidence：no live-region/runtime behavior change、no UI change、no public API change、no retained API removal，并且 no broad screen-reader certification claim。

## Proof Contract

用下面这条命令验证 hosted route：

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof
```

预期 hosted-accessibility proof marker：

- `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`
- `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`

## 相关文档

- [Demo Guide](./demo-guide.md)
- [Authoring Surface Recipe](./authoring-surface-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
