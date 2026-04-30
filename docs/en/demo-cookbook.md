# Demo Cookbook

The Demo cookbook is the copyable code-plus-demo index for AsterGraph evaluators. It connects existing source files, runnable Demo surfaces, docs, proof markers, and support boundaries without making `src/AsterGraph.Demo` part of the supported package boundary.

Use it when you want to move from "I can see the Demo" to "I know which code and proof path to copy next."

## Recipe Index

| Recipe | Category | Code anchors | Demo anchors | Docs | Proof markers |
| --- | --- | --- | --- | --- | --- |
| `starter-host-route` / Starter host route | StarterHost | `tools/AsterGraph.Starter.Avalonia/Program.cs` (`StarterAvaloniaAppBuilder`) | `src/AsterGraph.Demo/DemoGraphFactory.cs` (`AiPipelineScenario`) | [Host Recipe Ladder](./host-recipe-ladder.md); catalog path: `docs/en/host-recipe-ladder.md` | `FIVE_MINUTE_ONBOARDING_OK` |
| `authoring-surface-route` / Authoring surface route | Authoring | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs` (`CreateEdgeOverlay`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs` (`GetCommandDescriptors`) | `src/AsterGraph.Demo/Presentation/DemoShowcasePresenters.cs` (`CreateReplacementPreviewOptions`); `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`scenario tour`) | [Authoring Surface Recipe](./authoring-surface-recipe.md); catalog path: `docs/en/authoring-surface-recipe.md`; evidence: `AUTHORING_SURFACE_OK:True` | `AUTHORING_SURFACE_OK` |
| `plugin-trust-route` / Plugin trust route | PluginTrust | `src/AsterGraph.Demo/Integration/DemoPluginTrustWorkspace.cs` (`PluginTrustDecision`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`PluginTrustEvidencePanelOk`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Extensions.cs` (`PluginTrust`) | [Plugin Host Recipe](./plugin-host-recipe.md); catalog path: `docs/en/plugin-host-recipe.md`; evidence: `Proof Marker Expectations` | `PLUGIN_TRUST_EVIDENCE_PANEL_OK` |
| `diagnostics-support-route` / Diagnostics and support route | DiagnosticsSupport | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`RuntimeLogs`); `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeProjection.cs` (`RuntimeDiagnosticEntry`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeTimeline.cs` (`RuntimeCommandTimelineEntry`) | [Support Bundle](./support-bundle.md); catalog path: `docs/en/support-bundle.md`; evidence: `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True` | `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK` |
| `review-help-route` / Review and help route | ReviewHelp | `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs` (`RepairHelpReviewLoopOk`); `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs` (`ValidationFeedback`) | `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs` (`DemoHostMenuGroups.Proof`) | [Feature Catalog](./feature-catalog.md); catalog path: `docs/en/feature-catalog.md`; evidence: `authoring.repair-help-review` | `REPAIR_HELP_REVIEW_LOOP_OK` |

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
