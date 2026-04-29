namespace AsterGraph.Demo.Cookbook;

public sealed record DemoCookbookProofResult(
    bool ContractOk,
    bool RequiredCategoriesOk,
    bool SupportBoundaryOk,
    int RecipeCount,
    int RequiredCategoryCount)
{
    public bool IsOk => ContractOk && RequiredCategoriesOk && SupportBoundaryOk;
}

public static class DemoCookbookProof
{
    public static IReadOnlyList<string> PublicSuccessMarkerIds { get; } =
    [
        "DEMO_COOKBOOK_CONTRACT_OK",
        "DEMO_COOKBOOK_REQUIRED_CATEGORIES_OK",
        "DEMO_COOKBOOK_SUPPORT_BOUNDARY_OK",
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

        return new DemoCookbookProofResult(
            contractOk,
            requiredCategoriesOk,
            supportBoundaryOk,
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
            $"DEMO_COOKBOOK_RECIPE_COUNT:{result.RecipeCount}",
            $"DEMO_COOKBOOK_REQUIRED_CATEGORY_COUNT:{result.RequiredCategoryCount}",
        ];
    }
}
