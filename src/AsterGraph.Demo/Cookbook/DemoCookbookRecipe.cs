namespace AsterGraph.Demo.Cookbook;

public sealed record DemoCookbookRecipe(
    string Id,
    DemoCookbookRecipeCategory Category,
    string Title,
    string Summary,
    string CodePath,
    string DemoPath,
    string DocumentationPath,
    string ProofMarker,
    string SupportBoundary);
