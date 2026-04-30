# Demo Cookbook

Demo cookbook 是面向 AsterGraph 评估者的“代码 + 演示”索引。它把现有源码、可运行 Demo 表面、文档、proof marker 和支持边界放到同一处，但不会把 `src/AsterGraph.Demo` 变成受支持的 package 边界。

当你已经能看到 Demo，但还需要知道“下一步复制哪段代码、跑哪条 proof”时，看这页。

## Recipe 索引

| Recipe | 分类 | 代码锚点 | Demo 锚点 | 文档 | Proof marker |
| --- | --- | --- | --- | --- | --- |
| `starter-host-route` / Starter host route | StarterHost | `tools/AsterGraph.Starter.Avalonia/Program.cs` (`CreateRuntimeSurface`, `CreateHostBuilder`)；`src/AsterGraph.Avalonia/Hosting/AsterGraphHostBuilder.cs` (`BuildAvaloniaView`)；`src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` (`CreateSession(AsterGraphEditorOptions)`) | `src/AsterGraph.Demo/DemoGraphFactory.cs` (`AiPipelineScenario`) | [Host Recipe 阶梯](./host-recipe-ladder.md)；catalog path: `docs/en/host-recipe-ladder.md`；evidence: `Host Recipe Ladder` | `FIVE_MINUTE_ONBOARDING_OK` |
| `authoring-surface-route` / Authoring surface route | Authoring | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs` (`CreatePresentationOptions`, `CreateEdgeOverlay`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleNodeVisualPresenter.cs` (`Create`, `Update`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleConnectionOverlay.cs` (`GetConnectionGeometrySnapshots`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`GetCommandDescriptors`) | `src/AsterGraph.Demo/Presentation/DemoShowcasePresenters.cs` (`CreateReplacementPreviewOptions`)；`src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`scenario tour`) | [Authoring Surface Recipe](./authoring-surface-recipe.md)；catalog path: `docs/en/authoring-surface-recipe.md`；evidence: `AUTHORING_SURFACE_OK:True`、`CUSTOM_EXTENSION_SURFACE_OK:True` | `AUTHORING_SURFACE_OK`、`CUSTOM_EXTENSION_SURFACE_OK` |
| `plugin-trust-route` / Plugin trust route | PluginTrust | `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` (`DiscoverPluginCandidates`, `StagePluginPackage`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`RouteBoundaryLines`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`PluginTrustEvidencePanelOk`) | `src/AsterGraph.Demo/Integration/DemoPluginTrustWorkspace.cs` (`PluginTrustDecision`)；`src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Extensions.cs` (`PluginTrust`) | [Plugin Host Recipe](./plugin-host-recipe.md)；catalog path: `docs/en/plugin-host-recipe.md`；evidence: `Proof Marker Expectations` | `PLUGIN_TRUST_EVIDENCE_PANEL_OK` |
| `diagnostics-support-route` / Diagnostics and support route | DiagnosticsSupport | `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` (`RuntimeOverlayProvider`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`GetRuntimeOverlaySnapshot`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`RuntimeLogs`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeProjection.cs` (`RuntimeDiagnosticEntry`)；`src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeTimeline.cs` (`RuntimeCommandTimelineEntry`) | [Support Bundle](./support-bundle.md)；catalog path: `docs/en/support-bundle.md`；evidence: `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True` | `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK` |
| `review-help-route` / Review and help route | ReviewHelp | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`RepairHelpReviewLoopOk`)；`tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`ValidationFeedback`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`DemoHostMenuGroups.Proof`) | [Feature Catalog](./feature-catalog.md)；catalog path: `docs/en/feature-catalog.md`；evidence: `authoring.repair-help-review` | `REPAIR_HELP_REVIEW_LOOP_OK` |

## Route posture

`DEMO_COOKBOOK_ROUTE_COVERAGE_OK` 证明 cookbook 明确区分路线姿态：

- `Supported SDK route`: `starter-host-route` 和 `authoring-surface-route`。
- `Proof/demo route`: `plugin-trust-route`、`diagnostics-support-route` 和 `review-help-route`。
- `Hosted UI route`: 复制 `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`，或薄封装 `AsterGraphHostBuilder` 组合。
- `Runtime-only route`: 宿主持有自己的 UI 时使用 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`。
- `Plugin route`: 使用 `DiscoverPluginCandidates(...)` / `StagePluginPackage(...)` 加宿主自有 `PluginTrustPolicy`；它仍是 trusted in-process，不是 sandbox。
- `Migration route`: retained `GraphEditorViewModel` / `GraphEditorView` 是迁移桥，不是新的 primary runtime model。

## Source-backed route clarity

- `starter-host-route`: Shipped Avalonia route: AsterGraphHostBuilder.Create(...).BuildAvaloniaView() via StarterAvaloniaAppBuilder. `AsterGraph.Avalonia` composes the hosted UI on top of `AsterGraph.Editor` session/runtime surfaces. Demo scenario launch is inspection/proof only; copy the starter host code instead of Demo ViewModel code.
- `authoring-surface-route`: Hosted Avalonia authoring route: AsterGraphHostBuilder.UsePresentation(...) with IGraphEditorSession.Queries.GetCommandDescriptors(). Supported seams live in `AsterGraph.Avalonia` hosting and `AsterGraph.Editor` session/query/command contracts. ConsumerSample is the copyable recipe; Demo presenters are visual proof only and do not define package contracts. ConsumerSample is the copyable recipe for custom node presenter lifecycle, port/target anchors, geometry-snapshot edge overlays, inspector metadata, and runtime decorations.
- `plugin-trust-route`: Plugin route: AsterGraphEditorFactory.DiscoverPluginCandidates(...) with host-owned PluginTrustPolicy before loading. Supported APIs live in `AsterGraph.Editor` plugin discovery, trust, and registration contracts. Demo trust workspace is an evidence surface only; it does not sandbox or isolate untrusted plugin code.
- `diagnostics-support-route`: Runtime diagnostics route: AsterGraphEditorOptions.RuntimeOverlayProvider plus IGraphEditorSession.Queries.GetRuntimeOverlaySnapshot(). Supported APIs live in `AsterGraph.Editor` runtime overlay/query contracts and ConsumerSample local support-bundle code. Demo runtime timeline is a local projection only; it does not add telemetry or remote sync.
- `review-help-route`: Review/help route: IGraphEditorSession validation feedback and ConsumerSample support-bundle proof. Supported seams stay in `AsterGraph.Editor` session validation, repair, and support evidence contracts. Demo proof panels are review evidence only; they do not add a workflow engine or macro scheduler.

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

## Professional interaction facets

`DEMO_COOKBOOK_PROFESSIONAL_INTERACTION_OK` 证明 cookbook 显示的是有边界的交互 facet。它和 `DEMO_COOKBOOK_VISUAL_HIERARCHY_OK`、`DEMO_COOKBOOK_NAVIGATION_FEEDBACK_OK`、`DEMO_COOKBOOK_DETAIL_READABILITY_OK`、`DEMO_COOKBOOK_INTERACTION_STATES_OK` 一起闭环。

- `Selection`: preset launch selects a ready graph (`AiPipelineScenario`)；authoring projects selection-owned commands (`GetCommandDescriptors`)。
- `Connection`: authoring keeps connection overlays visible (`CreateEdgeOverlay`)；support review keeps command effects visible (`RuntimeCommandTimelineEntry`)；repair/help covers connection handoff (`RepairHelpReviewLoopOk`)。
- `LayoutReadability`: first-run route copy stays separate in the ladder (`Host Recipe Ladder`)；plugin route boundaries stay explicit (`RouteBoundaryLines`)。
- `Inspection`: hosted entry points、trust decisions、support logs 和 proof panels 都保持可检查（`CreateRuntimeSurface`、`PluginTrustDecision`、`RuntimeLogs`、`DemoHostMenuGroups.Proof`）。
- `ValidationRuntimeFeedback`: extension、runtime 和 review feedback 都保持本地、source-backed（`PluginTrust`、`RuntimeDiagnosticEntry`、`ValidationFeedback`）。

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
