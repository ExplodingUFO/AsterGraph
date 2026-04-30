namespace AsterGraph.Demo.Cookbook;

public static partial class DemoCookbookCatalog
{
    public static IReadOnlyList<DemoCookbookRecipe> Recipes { get; } =
    [
        new DemoCookbookRecipe(
            "starter-host-route",
            DemoCookbookRecipeCategory.StarterHost,
            "Starter host route",
            "Start from the hosted Avalonia route, then launch the Demo scenario for the fuller walkthrough.",
            [
                new DemoCookbookAnchor(
                    "Starter Avalonia host entry",
                    "tools/AsterGraph.Starter.Avalonia/Program.cs",
                    "StarterAvaloniaAppBuilder"),
            ],
            [
                new DemoCookbookAnchor(
                    "AI pipeline scenario launch",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "AiPipelineScenario"),
            ],
            [
                new DemoCookbookAnchor(
                    "Host recipe ladder",
                    "docs/en/host-recipe-ladder.md",
                    "Host Recipe Ladder"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "Host startup copies the shipped Avalonia boot route.",
                    "StarterAvaloniaAppBuilder"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The AI pipeline preset opens a ready graph for onboarding.",
                    "AiPipelineScenario"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The onboarding proof marker bounds this route to supported host setup.",
                    "FIVE_MINUTE_ONBOARDING_OK"),
            ],
            ["FIVE_MINUTE_ONBOARDING_OK"],
            "Avalonia is the shipped hosted route; WPF remains validation-only and Demo remains sample/proof surface."),
        new DemoCookbookRecipe(
            "authoring-surface-route",
            DemoCookbookRecipeCategory.Authoring,
            "Authoring surface route",
            "Copy custom presentation, node-side parameter editing, command projection, and authoring seams from ConsumerSample.",
            [
                new DemoCookbookAnchor(
                    "ConsumerSample authoring recipe",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs",
                    "CreateEdgeOverlay"),
                new DemoCookbookAnchor(
                    "ConsumerSample host command projection",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs",
                    "GetCommandDescriptors"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo authoring presenter surface",
                    "src/AsterGraph.Demo/Presentation/DemoShowcasePresenters.cs",
                    "CreateReplacementPreviewOptions"),
                new DemoCookbookAnchor(
                    "Demo tour proof lines",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs",
                    "scenario tour"),
            ],
            [
                new DemoCookbookAnchor(
                    "Authoring surface recipe",
                    "docs/en/authoring-surface-recipe.md",
                    "AUTHORING_SURFACE_OK:True"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Edge overlays and previews show authoring operations on the graph surface.",
                    "CreateEdgeOverlay"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Hosted command and parameter surfaces stay tied to ConsumerSample metadata.",
                    "GetCommandDescriptors"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The authoring recipe is backed by an explicit proof marker.",
                    "AUTHORING_SURFACE_OK"),
            ],
            ["AUTHORING_SURFACE_OK"],
            "Authoring samples reuse public seams and do not create a second editor/runtime model."),
        new DemoCookbookRecipe(
            "plugin-trust-route",
            DemoCookbookRecipeCategory.PluginTrust,
            "Plugin trust route",
            "Inspect trusted in-process plugin discovery, allowlist decisions, manifest validation, and load snapshots.",
            [
                new DemoCookbookAnchor(
                    "Demo plugin trust policy",
                    "src/AsterGraph.Demo/Integration/DemoPluginTrustWorkspace.cs",
                    "PluginTrustDecision"),
                new DemoCookbookAnchor(
                    "ConsumerSample plugin proof",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs",
                    "PluginTrustEvidencePanelOk"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo extensions panel projection",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Extensions.cs",
                    "PluginTrust"),
            ],
            [
                new DemoCookbookAnchor(
                    "Plugin host recipe",
                    "docs/en/plugin-host-recipe.md",
                    "Proof Marker Expectations"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Plugin manifests and trust decisions stay attached to trusted extension metadata.",
                    "PluginTrustDecision"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "The Demo extensions projection exposes trusted in-process plugin state.",
                    "PluginTrust"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The ConsumerSample proof panel evidence backs the trust route.",
                    "PLUGIN_TRUST_EVIDENCE_PANEL_OK"),
            ],
            ["PLUGIN_TRUST_EVIDENCE_PANEL_OK"],
            "Plugins are trusted in-process extensions; the recipe does not imply sandboxing or untrusted-code isolation."),
        new DemoCookbookRecipe(
            "diagnostics-support-route",
            DemoCookbookRecipeCategory.DiagnosticsSupport,
            "Diagnostics and support route",
            "Capture support-bundle and runtime diagnostics evidence from ConsumerSample and Demo runtime projections.",
            [
                new DemoCookbookAnchor(
                    "ConsumerSample support bundle",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs",
                    "RuntimeLogs"),
                new DemoCookbookAnchor(
                    "Demo runtime diagnostics projection",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeProjection.cs",
                    "RuntimeDiagnosticEntry"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo runtime timeline",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeTimeline.cs",
                    "RuntimeCommandTimelineEntry"),
            ],
            [
                new DemoCookbookAnchor(
                    "Support bundle docs",
                    "docs/en/support-bundle.md",
                    "RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Runtime diagnostics and command timeline state are captured as local overlays.",
                    "RuntimeDiagnosticEntry"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Support bundle runtime logs provide handoff evidence without telemetry claims.",
                    "RuntimeLogs"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The runtime timeline links graph commands to captured diagnostics.",
                    "RuntimeCommandTimelineEntry"),
            ],
            ["RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK"],
            "Support bundles are local handoff evidence, not telemetry, remote sync, or support-scope expansion."),
        new DemoCookbookRecipe(
            "review-help-route",
            DemoCookbookRecipeCategory.ReviewHelp,
            "Review and help route",
            "Trace validation repair, contextual help, and review-loop proof markers from existing support-bundle evidence.",
            [
                new DemoCookbookAnchor(
                    "ConsumerSample repair/help proof",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs",
                    "RepairHelpReviewLoopOk"),
                new DemoCookbookAnchor(
                    "ConsumerSample validation feedback bundle",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs",
                    "ValidationFeedback"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo proof surface projection",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs",
                    "DemoHostMenuGroups.Proof"),
            ],
            [
                new DemoCookbookAnchor(
                    "Feature catalog review/help row",
                    "docs/en/feature-catalog.md",
                    "authoring.repair-help-review"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Validation feedback links repair suggestions to concrete graph issues.",
                    "ValidationFeedback"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The repair/help proof marker closes the review-loop scenario.",
                    "REPAIR_HELP_REVIEW_LOOP_OK"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "ConsumerSample proof code is the host-facing example for repair/help handoff.",
                    "RepairHelpReviewLoopOk"),
            ],
            ["REPAIR_HELP_REVIEW_LOOP_OK"],
            "Review/help evidence stays bounded to existing validation and support-bundle proof; it is not a new workflow engine."),
    ];
}
