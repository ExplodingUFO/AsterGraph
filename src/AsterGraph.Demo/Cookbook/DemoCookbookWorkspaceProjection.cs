namespace AsterGraph.Demo.Cookbook;

public sealed record DemoCookbookWorkspaceSnapshot(
    IReadOnlyList<DemoCookbookWorkspaceNavigationGroup> NavigationGroups,
    DemoCookbookWorkspaceRecipeContent SelectedRecipe);

public sealed record DemoCookbookWorkspaceNavigationGroup(
    DemoCookbookRecipeCategory Category,
    string DisplayName,
    IReadOnlyList<DemoCookbookWorkspaceNavigationItem> Recipes);

public sealed record DemoCookbookWorkspaceNavigationItem(
    string RecipeId,
    string Title,
    DemoCookbookRecipeCategory Category,
    bool IsSelected);

public sealed record DemoCookbookWorkspaceRecipeContent(
    string RecipeId,
    string Title,
    string Summary,
    DemoCookbookRecipeCategory Category,
    IReadOnlyList<DemoCookbookWorkspaceAnchor> GraphAnchors,
    IReadOnlyList<DemoCookbookWorkspaceAnchor> CodeExamples,
    IReadOnlyList<DemoCookbookWorkspaceAnchor> DocumentationLinks,
    IReadOnlyList<string> ProofMarkers,
    string SupportBoundary);

public sealed record DemoCookbookWorkspaceAnchor(
    string Label,
    string Path,
    string Evidence);

public static class DemoCookbookWorkspaceProjection
{
    public static DemoCookbookWorkspaceSnapshot Create(string? selectedRecipeId = null)
    {
        var recipes = DemoCookbookCatalog.Recipes;
        if (recipes.Count == 0)
        {
            throw new InvalidOperationException("Demo cookbook workspace requires at least one recipe.");
        }

        var selectedRecipe = string.IsNullOrWhiteSpace(selectedRecipeId)
            ? recipes[0]
            : recipes.Single(recipe => string.Equals(recipe.Id, selectedRecipeId, StringComparison.Ordinal));

        return new DemoCookbookWorkspaceSnapshot(
            CreateNavigationGroups(recipes, selectedRecipe.Id),
            CreateRecipeContent(selectedRecipe));
    }

    private static IReadOnlyList<DemoCookbookWorkspaceNavigationGroup> CreateNavigationGroups(
        IReadOnlyList<DemoCookbookRecipe> recipes,
        string selectedRecipeId)
        => DemoCookbookCatalog.RequiredCategories
            .Select(category => new DemoCookbookWorkspaceNavigationGroup(
                category,
                FormatCategory(category),
                recipes
                    .Where(recipe => recipe.Category == category)
                    .Select(recipe => new DemoCookbookWorkspaceNavigationItem(
                        recipe.Id,
                        recipe.Title,
                        recipe.Category,
                        string.Equals(recipe.Id, selectedRecipeId, StringComparison.Ordinal)))
                    .ToArray()))
            .ToArray();

    private static DemoCookbookWorkspaceRecipeContent CreateRecipeContent(DemoCookbookRecipe recipe)
        => new(
            recipe.Id,
            recipe.Title,
            recipe.Summary,
            recipe.Category,
            ConvertAnchors(recipe.DemoAnchors),
            ConvertAnchors(recipe.CodeAnchors),
            ConvertAnchors(recipe.DocumentationAnchors),
            recipe.ProofMarkers.ToArray(),
            recipe.SupportBoundary);

    private static IReadOnlyList<DemoCookbookWorkspaceAnchor> ConvertAnchors(IReadOnlyList<DemoCookbookAnchor> anchors)
        => anchors
            .Select(anchor => new DemoCookbookWorkspaceAnchor(anchor.Label, anchor.Path, anchor.Evidence))
            .ToArray();

    private static string FormatCategory(DemoCookbookRecipeCategory category)
        => category switch
        {
            DemoCookbookRecipeCategory.StarterHost => "Starter Host",
            DemoCookbookRecipeCategory.Authoring => "Authoring",
            DemoCookbookRecipeCategory.PluginTrust => "Plugin Trust",
            DemoCookbookRecipeCategory.DiagnosticsSupport => "Diagnostics Support",
            DemoCookbookRecipeCategory.ReviewHelp => "Review Help",
            _ => category.ToString(),
        };
}
