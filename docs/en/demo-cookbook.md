# Demo Cookbook

The Demo cookbook is the copyable code-plus-demo index for AsterGraph evaluators. It connects existing source files, runnable Demo surfaces, docs, proof markers, and support boundaries without making `src/AsterGraph.Demo` part of the supported package boundary.

Use it when you want to move from "I can see the Demo" to "I know which code and proof path to copy next."

## Recipe Index

| Recipe | Category | Code anchors | Demo anchors | Docs | Proof markers |
| --- | --- | --- | --- | --- | --- |
| `starter-host-route` / Starter host route | StarterHost | `tools/AsterGraph.Starter.Avalonia/Program.cs` (`CreateRuntimeSurface`, `CreateHostBuilder`); `src/AsterGraph.Avalonia/Hosting/AsterGraphHostBuilder.cs` (`BuildAvaloniaView`); `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` (`CreateSession(AsterGraphEditorOptions)`) | `src/AsterGraph.Demo/DemoGraphFactory.cs` (`AiPipelineScenario`) | [Host Recipe Ladder](./host-recipe-ladder.md); catalog path: `docs/en/host-recipe-ladder.md` | `FIVE_MINUTE_ONBOARDING_OK` |
| `authoring-surface-route` / Authoring surface route | Authoring | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs` (`CreatePresentationOptions`, `CreateEdgeOverlay`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleNodeVisualPresenter.cs` (`Create`, `Update`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleConnectionOverlay.cs` (`GetConnectionGeometrySnapshots`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`GetCommandDescriptors`) | `src/AsterGraph.Demo/Presentation/DemoShowcasePresenters.cs` (`CreateReplacementPreviewOptions`); `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`scenario tour`) | [Authoring Surface Recipe](./authoring-surface-recipe.md); catalog path: `docs/en/authoring-surface-recipe.md`; evidence: `AUTHORING_SURFACE_OK:True`, `CUSTOM_EXTENSION_SURFACE_OK:True` | `AUTHORING_SURFACE_OK`, `CUSTOM_EXTENSION_SURFACE_OK` |
| `plugin-trust-route` / Plugin trust route | PluginTrust | `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` (`DiscoverPluginCandidates`, `StagePluginPackage`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`RouteBoundaryLines`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`PluginTrustEvidencePanelOk`) | `src/AsterGraph.Demo/Integration/DemoPluginTrustWorkspace.cs` (`PluginTrustDecision`); `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Extensions.cs` (`PluginTrust`) | [Plugin Host Recipe](./plugin-host-recipe.md); catalog path: `docs/en/plugin-host-recipe.md`; evidence: `Proof Marker Expectations` | `PLUGIN_TRUST_EVIDENCE_PANEL_OK` |
| `diagnostics-support-route` / Diagnostics and support route | DiagnosticsSupport | `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` (`RuntimeOverlayProvider`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`GetRuntimeOverlaySnapshot`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`RuntimeLogs`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeProjection.cs` (`RuntimeDiagnosticEntry`); `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeTimeline.cs` (`RuntimeCommandTimelineEntry`) | [Support Bundle](./support-bundle.md); catalog path: `docs/en/support-bundle.md`; evidence: `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True` | `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK` |
| `review-help-route` / Review and help route | ReviewHelp | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`RepairHelpReviewLoopOk`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`ValidationFeedback`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`DemoHostMenuGroups.Proof`) | [Feature Catalog](./feature-catalog.md); catalog path: `docs/en/feature-catalog.md`; evidence: `authoring.repair-help-review` | `REPAIR_HELP_REVIEW_LOOP_OK` |

## Route Posture

`DEMO_COOKBOOK_ROUTE_COVERAGE_OK` proves the cookbook keeps route posture explicit:

- `Supported SDK route`: `starter-host-route` and `authoring-surface-route`.
- `Proof/demo route`: `plugin-trust-route`, `diagnostics-support-route`, and `review-help-route`.
- `Hosted UI route`: copy `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` or thin `AsterGraphHostBuilder` composition.
- `Runtime-only route`: use `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession` when the host owns its UI.
- `Plugin route`: use `DiscoverPluginCandidates(...)` / `StagePluginPackage(...)` plus host-owned `PluginTrustPolicy`; it remains trusted in-process, not sandboxed.
- `Migration route`: retained `GraphEditorViewModel` / `GraphEditorView` is a migration bridge, not a new primary runtime model.

## Source-Backed Route Clarity

- `starter-host-route`: Shipped Avalonia route: AsterGraphHostBuilder.Create(...).BuildAvaloniaView() via StarterAvaloniaAppBuilder. `AsterGraph.Avalonia` composes the hosted UI on top of `AsterGraph.Editor` session/runtime surfaces. Demo scenario launch is inspection/proof only; copy the starter host code instead of Demo ViewModel code.
- `authoring-surface-route`: Hosted Avalonia authoring route: AsterGraphHostBuilder.UsePresentation(...) with IGraphEditorSession.Queries.GetCommandDescriptors(). Supported seams live in `AsterGraph.Avalonia` hosting and `AsterGraph.Editor` session/query/command contracts. ConsumerSample is the copyable recipe; Demo presenters are visual proof only and do not define package contracts. ConsumerSample is the copyable recipe for custom node presenter lifecycle, port/target anchors, geometry-snapshot edge overlays, inspector metadata, and runtime decorations.
- `plugin-trust-route`: Plugin route: AsterGraphEditorFactory.DiscoverPluginCandidates(...) with host-owned PluginTrustPolicy before loading. Supported APIs live in `AsterGraph.Editor` plugin discovery, trust, and registration contracts. Demo trust workspace is an evidence surface only; it does not sandbox or isolate untrusted plugin code.
- `diagnostics-support-route`: Runtime diagnostics route: AsterGraphEditorOptions.RuntimeOverlayProvider plus IGraphEditorSession.Queries.GetRuntimeOverlaySnapshot(). Supported APIs live in `AsterGraph.Editor` runtime overlay/query contracts and ConsumerSample local support-bundle code. Demo runtime timeline is a local projection only; it does not add telemetry or remote sync.
- `review-help-route`: Review/help route: IGraphEditorSession validation feedback and ConsumerSample support-bundle proof. Supported seams stay in `AsterGraph.Editor` session validation, repair, and support evidence contracts. Demo proof panels are review evidence only; they do not add a workflow engine or macro scheduler.

## Support Boundaries

- `starter-host-route`: Avalonia is the shipped hosted route; WPF remains validation-only and Demo remains sample/proof surface.
- `authoring-surface-route`: Authoring samples reuse public seams and do not create a second editor/runtime model.
- `plugin-trust-route`: Plugins are trusted in-process extensions; the recipe does not imply sandboxing or untrusted-code isolation.
- `diagnostics-support-route`: Support bundles are local handoff evidence, not telemetry, remote sync, or support-scope expansion.
- `review-help-route`: Review/help evidence stays bounded to existing validation and support-bundle proof; it is not a new workflow engine.

## Scenario Depth Anchors

`DEMO_COOKBOOK_SCENARIO_DEPTH_OK` proves the cookbook's scenario points cover the required professional depth while keeping Demo bounded as a sample/proof surface:

- `GraphOperations`: graph creation, command, overlay, or review paths are tied back to existing code/demo anchors.
- `NodeMetadata`: metadata and trust evidence stay anchored to recipe code or proof markers.
- `ValidationRuntimeOverlay`: validation, runtime overlay, support bundle, or repair/help evidence is represented without enabling a workflow engine.
- `SupportEvidence`: each route keeps support claims tied to local proof or docs evidence.
- `HostCodeExample`: host-copy examples remain anchored to starter or consumer sample code.

## Professional Interaction Facets

`DEMO_COOKBOOK_PROFESSIONAL_INTERACTION_OK` proves the cookbook exposes bounded interaction facets. It is paired with `DEMO_COOKBOOK_VISUAL_HIERARCHY_OK`, `DEMO_COOKBOOK_NAVIGATION_FEEDBACK_OK`, `DEMO_COOKBOOK_DETAIL_READABILITY_OK`, and `DEMO_COOKBOOK_INTERACTION_STATES_OK`.

- `Selection`: preset launch selects a ready graph (`AiPipelineScenario`); authoring projects selection-owned commands (`GetCommandDescriptors`).
- `Connection`: authoring keeps connection overlays visible (`CreateEdgeOverlay`); support review keeps command effects visible (`RuntimeCommandTimelineEntry`); repair/help covers connection handoff (`RepairHelpReviewLoopOk`).
- `LayoutReadability`: first-run route copy stays separate in the ladder (`Host Recipe Ladder`); plugin route boundaries stay explicit (`RouteBoundaryLines`).
- `Inspection`: hosted entry points, trust decisions, support logs, and proof panels remain inspectable (`CreateRuntimeSurface`, `PluginTrustDecision`, `RuntimeLogs`, `DemoHostMenuGroups.Proof`).
- `ValidationRuntimeFeedback`: extension, runtime, and review feedback stay local and source-backed (`PluginTrust`, `RuntimeDiagnosticEntry`, `ValidationFeedback`).

## How To Use It

1. Start with [Quick Start](./quick-start.md) if you have not run the SDK before.
2. Launch `dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline`.
3. Open the Demo `Cookbook` menu group, filter or select the recipe, then use the listed code path and docs path as the copy target.
4. Verify with the listed proof marker before treating the recipe as defended.

The cookbook indexes existing assets. It does not introduce a runtime marketplace, sandbox, workflow execution engine, WPF parity promise, or GA support claim.

## Related Docs

- [Demo Guide](./demo-guide.md)
- [Feature Catalog](./feature-catalog.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Recipe Ladder](./host-recipe-ladder.md)
- [Plugin Host Recipe](./plugin-host-recipe.md)
- [Support Bundle](./support-bundle.md)
