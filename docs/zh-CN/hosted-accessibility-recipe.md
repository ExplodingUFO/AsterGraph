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

## Manual Assistive-Technology Evidence Package

`ACCESSIBILITY_MANUAL_AT_EVIDENCE_PACKAGE` 是 Phase 516 / GitHub #152 / `avalonia-node-map-821` 的证据记录。它把 Phase 505 的计划转成有边界的 platform-equivalent package，同时把 headless automation proof 和 live assistive-technology observations 分开。

| Evidence lane | Hosted route states exercised | Observed result | Boundary |
| --- | --- | --- | --- |
| 2026-05-12 fresh Demo proof：`dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --configuration Release --nologo -- --proof` | hosted Demo route、command surface、native interaction accessibility checks 和 proof metrics | 输出 `NATIVE_INTERACTION_A11Y_OK:True`、`COMMAND_SURFACE_OK:True`、`DEMO_OK:True` 以及 `HOST_NATIVE_METRIC:*` 行 | 只算 platform-equivalent headless automation proof；live screen-reader observations were not performed |
| Focused headless accessibility tests | `GraphEditorView`、`NodeCanvas`、`GraphInspectorView`、`PART_CommandPaletteSearchBox`、`PART_ParameterSearchBox`、投影出来的 command buttons、投影出来的 node/edge tools、validation focus buttons 和 export/status text | accessible names、focusability、focus recovery、command-surface projection 和 validation focus routes 继续由 `GraphEditorViewTests`、`NodeCanvasStandaloneTests` 和 `AccessibilityManualValidationDocsTests` 守住 | dynamic validation/export/status announcements 仍然是 not observed by a live screen reader |
| Live assistive-technology pass | Narrator、NVDA 和 VoiceOver | Phase 516 中 not observed | GitHub #156 / `avalonia-node-map-1pd` 负责 live screen-reader announcement validation，完成前不扩大声明 |

这份 evidence package 继续保持 no live-region/runtime behavior change、no UI change、no public API change、no retained API removal，并且 no broad screen-reader certification claim。后续如果补 Narrator、NVDA、VoiceOver 或 platform-equivalent notes，也必须留在同一条 hosted route 上，记录实际 announcements 和 focus transitions，而不是替换 headless proof。

## Live Assistive-Technology Platform-Equivalent Evidence

`ACCESSIBILITY_LIVE_AT_UIA_EVIDENCE` 是 Phase 517 / GitHub #156 / `avalonia-node-map-1pd` 的证据记录。它记录一次针对 hosted Demo route 的 Windows UI Automation platform-equivalent check，但不声明 live screen-reader speech output。

| Evidence lane | Hosted route states exercised | Observed result | Boundary |
| --- | --- | --- | --- |
| 2026-05-12 Windows UI Automation platform-equivalent check：`src/AsterGraph.Demo/bin/Release/net9.0/AsterGraph.Demo.exe --scenario validation-prevent-cycle` | `validation-prevent-cycle` scenario 中的 `GraphEditorView`、`NodeCanvas`、projected host command buttons 以及可见 node/port names | UIA 暴露了 `GraphEditorView`、`NodeCanvas`、projected host command buttons 和部分 node/port names | 只算 platform-equivalent evidence；live screen-reader speech output was not observed |
| 同一次 UIA run 观察到的 gap | `PART_CommandPaletteSearchBox`、`PART_ParameterSearchBox`、validation/export/status surfaces 和 usable live-region metadata | 这些 targets 在 observed initial validation scenario state 中 not exposed | dynamic validation/export/status announcements 仍未证明，后续由 GitHub #158 / `avalonia-node-map-g0u` 追踪 |

这份 Phase 517 evidence 继续保持 no live-region/runtime behavior change、no UI change、no public API change、no retained API removal，并且 no broad screen-reader certification claim。GitHub #158 / `avalonia-node-map-g0u` 负责后续 dynamic validation/export/status announcements 的 runtime proof。

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
