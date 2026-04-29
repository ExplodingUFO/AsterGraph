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
            "tools/AsterGraph.Starter.Avalonia/Program.cs",
            "dotnet run --project src/AsterGraph.Demo -- --scenario ai-pipeline",
            "docs/en/host-recipe-ladder.md",
            "FIVE_MINUTE_ONBOARDING_OK",
            "Avalonia is the shipped hosted route; WPF remains validation-only and Demo remains sample/proof surface."),
        new DemoCookbookRecipe(
            "authoring-surface-route",
            DemoCookbookRecipeCategory.Authoring,
            "Authoring surface route",
            "Copy custom presentation, node-side parameter editing, command projection, and authoring seams from ConsumerSample.",
            "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs",
            "Demo Tour panel / AI pipeline scenario custom nodes and parameter editing steps",
            "docs/en/authoring-surface-recipe.md",
            "AUTHORING_SURFACE_OK",
            "Authoring samples reuse public seams and do not create a second editor/runtime model."),
        new DemoCookbookRecipe(
            "plugin-trust-route",
            DemoCookbookRecipeCategory.PluginTrust,
            "Plugin trust route",
            "Inspect trusted in-process plugin discovery, allowlist decisions, manifest validation, and load snapshots.",
            "src/AsterGraph.Demo/Integration/DemoPluginShowcase.cs",
            "Demo Extensions panel / plugin trust and loading",
            "docs/en/plugin-host-recipe.md",
            "PLUGIN_TRUST_EVIDENCE_PANEL_OK",
            "Plugins are trusted in-process extensions; the recipe does not imply sandboxing or untrusted-code isolation."),
        new DemoCookbookRecipe(
            "diagnostics-support-route",
            DemoCookbookRecipeCategory.DiagnosticsSupport,
            "Diagnostics and support route",
            "Capture support-bundle and runtime diagnostics evidence from ConsumerSample and Demo runtime projections.",
            "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs",
            "Demo Runtime panel / diagnostics and support evidence",
            "docs/en/support-bundle.md",
            "RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK",
            "Support bundles are local handoff evidence, not telemetry, remote sync, or support-scope expansion."),
        new DemoCookbookRecipe(
            "review-help-route",
            DemoCookbookRecipeCategory.ReviewHelp,
            "Review and help route",
            "Trace validation repair, contextual help, and review-loop proof markers from existing support-bundle evidence.",
            "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs",
            "Demo Proof panel / repair and review evidence",
            "docs/en/feature-catalog.md",
            "REPAIR_HELP_REVIEW_LOOP_OK",
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
            AddIfMissing(issues, recipe.Id, nameof(recipe.CodePath), recipe.CodePath);
            AddIfMissing(issues, recipe.Id, nameof(recipe.DemoPath), recipe.DemoPath);
            AddIfMissing(issues, recipe.Id, nameof(recipe.DocumentationPath), recipe.DocumentationPath);
            AddIfMissing(issues, recipe.Id, nameof(recipe.ProofMarker), recipe.ProofMarker);
            AddIfMissing(issues, recipe.Id, nameof(recipe.SupportBoundary), recipe.SupportBoundary);
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
}
