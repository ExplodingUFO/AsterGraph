namespace AsterGraph.Demo.Cookbook;

public static class DemoCookbookCatalog
{
    public static IReadOnlyList<DemoCookbookRecipeCategory> RequiredCategories { get; } =
    [
        DemoCookbookRecipeCategory.StarterHost,
        DemoCookbookRecipeCategory.Authoring,
        DemoCookbookRecipeCategory.PluginTrust,
        DemoCookbookRecipeCategory.DiagnosticsSupport,
        DemoCookbookRecipeCategory.ReviewHelp,
    ];

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
            ["REPAIR_HELP_REVIEW_LOOP_OK"],
            "Review/help evidence stays bounded to existing validation and support-bundle proof; it is not a new workflow engine."),
    ];

    public static IReadOnlyList<string> Validate()
    {
        var issues = new List<string>();
        var ids = new HashSet<string>(StringComparer.Ordinal);

        foreach (var recipe in Recipes)
        {
            if (!ids.Add(recipe.Id))
            {
                issues.Add($"Duplicate recipe id: {recipe.Id}");
            }

            AddIfMissing(issues, recipe.Id, nameof(recipe.Title), recipe.Title);
            AddIfMissing(issues, recipe.Id, nameof(recipe.Summary), recipe.Summary);
            AddIfMissing(issues, recipe.Id, nameof(recipe.CodeAnchors), recipe.CodeAnchors);
            AddIfMissing(issues, recipe.Id, nameof(recipe.DemoAnchors), recipe.DemoAnchors);
            AddIfMissing(issues, recipe.Id, nameof(recipe.DocumentationAnchors), recipe.DocumentationAnchors);
            AddIfMissing(issues, recipe.Id, nameof(recipe.ProofMarkers), recipe.ProofMarkers);
            AddIfMissing(issues, recipe.Id, nameof(recipe.SupportBoundary), recipe.SupportBoundary);

            AddAnchorIssues(issues, recipe.Id, nameof(recipe.CodeAnchors), recipe.CodeAnchors);
            AddAnchorIssues(issues, recipe.Id, nameof(recipe.DemoAnchors), recipe.DemoAnchors);
            AddAnchorIssues(issues, recipe.Id, nameof(recipe.DocumentationAnchors), recipe.DocumentationAnchors);

            foreach (var proofMarker in recipe.ProofMarkers)
            {
                AddIfMissing(issues, recipe.Id, nameof(recipe.ProofMarkers), proofMarker);
            }
        }

        foreach (var requiredCategory in RequiredCategories)
        {
            if (!Recipes.Any(recipe => recipe.Category == requiredCategory))
            {
                issues.Add($"Missing required category: {requiredCategory}");
            }
        }

        return issues;
    }

    private static void AddIfMissing(ICollection<string> issues, string recipeId, string fieldName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add($"{recipeId} is missing {fieldName}.");
        }
    }

    private static void AddIfMissing<T>(
        ICollection<string> issues,
        string recipeId,
        string fieldName,
        IReadOnlyCollection<T> values)
    {
        if (values.Count == 0)
        {
            issues.Add($"{recipeId} is missing {fieldName}.");
        }
    }

    private static void AddAnchorIssues(
        ICollection<string> issues,
        string recipeId,
        string fieldName,
        IReadOnlyList<DemoCookbookAnchor> anchors)
    {
        for (var index = 0; index < anchors.Count; index++)
        {
            var anchor = anchors[index];
            AddIfMissing(issues, recipeId, $"{fieldName}[{index}].{nameof(anchor.Label)}", anchor.Label);
            AddIfMissing(issues, recipeId, $"{fieldName}[{index}].{nameof(anchor.Path)}", anchor.Path);
            AddIfMissing(issues, recipeId, $"{fieldName}[{index}].{nameof(anchor.Evidence)}", anchor.Evidence);
        }
    }
}
