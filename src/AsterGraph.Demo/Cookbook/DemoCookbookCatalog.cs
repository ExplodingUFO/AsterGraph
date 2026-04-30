namespace AsterGraph.Demo.Cookbook;

public static partial class DemoCookbookCatalog
{
    public static IReadOnlyList<DemoCookbookRecipeCategory> RequiredCategories { get; } =
    [
        DemoCookbookRecipeCategory.StarterHost,
        DemoCookbookRecipeCategory.Authoring,
        DemoCookbookRecipeCategory.PluginTrust,
        DemoCookbookRecipeCategory.DiagnosticsSupport,
        DemoCookbookRecipeCategory.ReviewHelp,
    ];

    public static IReadOnlyList<DemoCookbookScenarioKind> RequiredScenarioKinds { get; } =
    [
        DemoCookbookScenarioKind.GraphOperations,
        DemoCookbookScenarioKind.NodeMetadata,
        DemoCookbookScenarioKind.ValidationRuntimeOverlay,
        DemoCookbookScenarioKind.SupportEvidence,
        DemoCookbookScenarioKind.HostCodeExample,
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
            AddIfMissing(issues, recipe.Id, nameof(recipe.ScenarioPoints), recipe.ScenarioPoints);
            AddIfMissing(issues, recipe.Id, nameof(recipe.ProofMarkers), recipe.ProofMarkers);
            AddIfMissing(issues, recipe.Id, nameof(recipe.SupportBoundary), recipe.SupportBoundary);

            AddAnchorIssues(issues, recipe.Id, nameof(recipe.CodeAnchors), recipe.CodeAnchors);
            AddAnchorIssues(issues, recipe.Id, nameof(recipe.DemoAnchors), recipe.DemoAnchors);
            AddAnchorIssues(issues, recipe.Id, nameof(recipe.DocumentationAnchors), recipe.DocumentationAnchors);
            AddScenarioPointIssues(issues, recipe);

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

        foreach (var requiredScenarioKind in RequiredScenarioKinds)
        {
            if (!Recipes.Any(recipe => recipe.ScenarioPoints.Any(point => point.Kind == requiredScenarioKind)))
            {
                issues.Add($"Missing required scenario kind: {requiredScenarioKind}");
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

    private static void AddScenarioPointIssues(ICollection<string> issues, DemoCookbookRecipe recipe)
    {
        var evidenceAnchors = recipe.CodeAnchors
            .Concat(recipe.DemoAnchors)
            .Concat(recipe.DocumentationAnchors)
            .Select(anchor => anchor.Evidence)
            .Concat(recipe.ProofMarkers)
            .ToArray();

        for (var index = 0; index < recipe.ScenarioPoints.Count; index++)
        {
            var point = recipe.ScenarioPoints[index];
            AddIfMissing(issues, recipe.Id, $"{nameof(recipe.ScenarioPoints)}[{index}].{nameof(point.Label)}", point.Label);
            AddIfMissing(issues, recipe.Id, $"{nameof(recipe.ScenarioPoints)}[{index}].{nameof(point.Evidence)}", point.Evidence);

            if (!evidenceAnchors.Contains(point.Evidence, StringComparer.Ordinal))
            {
                issues.Add($"{recipe.Id} {nameof(recipe.ScenarioPoints)}[{index}] evidence is not tied to a recipe anchor or proof marker: {point.Evidence}");
            }
        }
    }
}
