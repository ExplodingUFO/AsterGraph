# Demo Cookbook

Demo cookbook 是面向 AsterGraph 评估者的“代码 + 演示”索引。它把现有源码、可运行 Demo 表面、文档、proof marker 和支持边界放到同一处，但不会把 `src/AsterGraph.Demo` 变成受支持的 package 边界。

当你已经能看到 Demo，但还需要知道“下一步复制哪段代码、跑哪条 proof”时，看这页。

## Recipe 索引

| Recipe | 分类 | 代码锚点 | Demo 锚点 | 文档 | Proof marker |
| --- | --- | --- | --- | --- | --- |
| `starter-host-route` / Starter host route | StarterHost | `tools/AsterGraph.Starter.Avalonia/Program.cs` (`StarterAvaloniaAppBuilder`) | `src/AsterGraph.Demo/DemoGraphFactory.cs` (`AiPipelineScenario`) | [Host Recipe 阶梯](./host-recipe-ladder.md)；catalog path: `docs/en/host-recipe-ladder.md`；evidence: `Host Recipe Ladder` | `FIVE_MINUTE_ONBOARDING_OK` |
| `authoring-surface-route` / Authoring surface route | Authoring | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs` (`CreateEdgeOverlay`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`GetCommandDescriptors`) | `src/AsterGraph.Demo/Presentation/DemoShowcasePresenters.cs` (`CreateReplacementPreviewOptions`)；`src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`scenario tour`) | [Authoring Surface Recipe](./authoring-surface-recipe.md)；catalog path: `docs/en/authoring-surface-recipe.md`；evidence: `AUTHORING_SURFACE_OK:True` | `AUTHORING_SURFACE_OK` |
| `plugin-trust-route` / Plugin trust route | PluginTrust | `src/AsterGraph.Demo/Integration/DemoPluginTrustWorkspace.cs` (`PluginTrustDecision`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`PluginTrustEvidencePanelOk`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Extensions.cs` (`PluginTrust`) | [Plugin Host Recipe](./plugin-host-recipe.md)；catalog path: `docs/en/plugin-host-recipe.md`；evidence: `Proof Marker Expectations` | `PLUGIN_TRUST_EVIDENCE_PANEL_OK` |
| `diagnostics-support-route` / Diagnostics and support route | DiagnosticsSupport | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`RuntimeLogs`)；`src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeProjection.cs` (`RuntimeDiagnosticEntry`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeTimeline.cs` (`RuntimeCommandTimelineEntry`) | [Support Bundle](./support-bundle.md)；catalog path: `docs/en/support-bundle.md`；evidence: `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True` | `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK` |
| `review-help-route` / Review and help route | ReviewHelp | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`RepairHelpReviewLoopOk`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`ValidationFeedback`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`DemoHostMenuGroups.Proof`) | [Feature Catalog](./feature-catalog.md)；catalog path: `docs/en/feature-catalog.md`；evidence: `authoring.repair-help-review` | `REPAIR_HELP_REVIEW_LOOP_OK` |

## 支持边界

- `starter-host-route`: Avalonia is the shipped hosted route; WPF remains validation-only and Demo remains sample/proof surface.
- `authoring-surface-route`: Authoring samples reuse public seams and do not create a second editor/runtime model.
- `plugin-trust-route`: Plugins are trusted in-process extensions; the recipe does not imply sandboxing or untrusted-code isolation.
- `diagnostics-support-route`: Support bundles are local handoff evidence, not telemetry, remote sync, or support-scope expansion.
- `review-help-route`: Review/help evidence stays bounded to existing validation and support-bundle proof; it is not a new workflow engine.

## Scenario depth 锚点

`DEMO_COOKBOOK_SCENARIO_DEPTH_OK` 证明 cookbook 的 scenario points 覆盖专业深度，同时 Demo 仍然只是 sample/proof surface：

- `GraphOperations`: graph 创建、command、overlay 或 review 路线都回链到现有代码 / Demo 锚点。
- `NodeMetadata`: metadata 与 trust evidence 只锚定到 recipe 代码或 proof marker。
- `ValidationRuntimeOverlay`: validation、runtime overlay、support bundle 或 repair/help evidence 都有表达，但不启用 workflow engine。
- `SupportEvidence`: 每条路线的支持声明都绑定到本地 proof 或 docs evidence。
- `HostCodeExample`: 可复制的宿主示例仍锚定到 starter 或 consumer sample 代码。

## 怎么用

1. 如果还没有跑过 SDK，先看 [Quick Start](./quick-start.md)。
2. 运行 `dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline`。
3. 在 Demo 里打开 `Cookbook` 菜单分组，筛选或选择 recipe，再按列出的代码路径和文档路径复制。
4. 用列出的 proof marker 验证后，再把这条 recipe 当作受防守路线使用。

这份 cookbook 只是索引现有资产。它不引入 runtime marketplace、sandbox、workflow execution engine、WPF parity 承诺或 GA support claim。

## 相关文档

- [Demo Guide](./demo-guide.md)
- [Feature Catalog](./feature-catalog.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Recipe 阶梯](./host-recipe-ladder.md)
- [Plugin Host Recipe](./plugin-host-recipe.md)
- [Support Bundle](./support-bundle.md)
