namespace AsterGraph.Demo.Cookbook;

public sealed record DemoCookbookProofResult(
    bool ContractOk,
    bool RequiredCategoriesOk,
    bool SupportBoundaryOk,
    bool WorkspaceLayoutOk,
    bool RouteCoverageOk,
    int RecipeCount,
    int RequiredCategoryCount)
{
    public bool IsOk =>
        ContractOk
        && RequiredCategoriesOk
        && SupportBoundaryOk
        && WorkspaceLayoutOk
        && RouteCoverageOk;
}

public static class DemoCookbookProof
{
    public static IReadOnlyList<string> PublicSuccessMarkerIds { get; } =
    [
        "DEMO_COOKBOOK_CONTRACT_OK",
        "DEMO_COOKBOOK_REQUIRED_CATEGORIES_OK",
        "DEMO_COOKBOOK_SUPPORT_BOUNDARY_OK",
        "DEMO_COOKBOOK_WORKSPACE_LAYOUT_OK",
        "DEMO_COOKBOOK_ROUTE_COVERAGE_OK",
    ];

    public static DemoCookbookProofResult Run()
    {
        var contractOk = DemoCookbookCatalog.Validate().Count == 0;
        var requiredCategoriesOk = DemoCookbookCatalog.RequiredCategories.All(category =>
            DemoCookbookCatalog.Recipes.Any(recipe => recipe.Category == category));
        var supportBoundaryOk = DemoCookbookCatalog.Recipes.All(recipe =>
            !string.IsNullOrWhiteSpace(recipe.SupportBoundary)
            && !recipe.SupportBoundary.Contains("runtime marketplace", StringComparison.OrdinalIgnoreCase)
            && !recipe.SupportBoundary.Contains("GA support", StringComparison.OrdinalIgnoreCase));
        var workspaceSnapshots = DemoCookbookCatalog.Recipes
            .Select(recipe => DemoCookbookWorkspaceProjection.Create(recipe.Id))
            .ToArray();
        var workspaceLayoutOk = workspaceSnapshots.All(snapshot =>
            snapshot.NavigationGroups.Count == DemoCookbookCatalog.RequiredCategories.Count
            && snapshot.SelectedRecipe.GraphAnchors.Count > 0
            && snapshot.SelectedRecipe.CodeExamples.Count > 0
            && snapshot.SelectedRecipe.DocumentationLinks.Count > 0
            && snapshot.SelectedRecipe.ProofMarkers.Count > 0
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.SupportBoundary));
        var routeCoverageOk = workspaceSnapshots.All(snapshot =>
            !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteStatus)
            && !string.IsNullOrWhiteSpace(snapshot.SelectedRecipe.RouteStatusDescription)
            && snapshot.SelectedRecipe.DeferredGaps.Count > 0)
            && workspaceSnapshots.Any(snapshot => string.Equals(snapshot.SelectedRecipe.RouteStatus, "Supported SDK route", StringComparison.Ordinal))
            && workspaceSnapshots.Any(snapshot => string.Equals(snapshot.SelectedRecipe.RouteStatus, "Proof/demo route", StringComparison.Ordinal));

        return new DemoCookbookProofResult(
            contractOk,
            requiredCategoriesOk,
            supportBoundaryOk,
            workspaceLayoutOk,
            routeCoverageOk,
            DemoCookbookCatalog.Recipes.Count,
            DemoCookbookCatalog.RequiredCategories.Count);
    }

    public static IReadOnlyList<string> CreateProofLines()
    {
        var result = Run();

        return
        [
            $"DEMO_COOKBOOK_CONTRACT_OK:{result.ContractOk}",
            $"DEMO_COOKBOOK_REQUIRED_CATEGORIES_OK:{result.RequiredCategoriesOk}",
            $"DEMO_COOKBOOK_SUPPORT_BOUNDARY_OK:{result.SupportBoundaryOk}",
            $"DEMO_COOKBOOK_WORKSPACE_LAYOUT_OK:{result.WorkspaceLayoutOk}",
            $"DEMO_COOKBOOK_ROUTE_COVERAGE_OK:{result.RouteCoverageOk}",
            $"DEMO_COOKBOOK_RECIPE_COUNT:{result.RecipeCount}",
            $"DEMO_COOKBOOK_REQUIRED_CATEGORY_COUNT:{result.RequiredCategoryCount}",
        ];
    }
}
