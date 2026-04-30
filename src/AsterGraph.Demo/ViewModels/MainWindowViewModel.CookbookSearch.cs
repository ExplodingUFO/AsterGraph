using AsterGraph.Demo.Cookbook;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    private bool MatchesCookbookSearch(DemoCookbookRecipe recipe, string searchText)
        => Contains(recipe.Id, searchText)
           || Contains(FormatCookbookCategory(recipe.Category), searchText)
           || Contains(recipe.Title, searchText)
           || Contains(recipe.Summary, searchText)
           || ContainsAny(recipe.CodeAnchors, searchText)
           || ContainsAny(recipe.DemoAnchors, searchText)
           || ContainsAny(recipe.DocumentationAnchors, searchText)
           || ContainsAny(recipe.ScenarioPoints, searchText)
           || recipe.ProofMarkers.Any(marker => Contains(marker, searchText))
           || Contains(recipe.SupportBoundary, searchText);

    private static bool Contains(string value, string searchText)
        => value.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsAny(IReadOnlyList<DemoCookbookAnchor> anchors, string searchText)
        => anchors.Any(anchor =>
            Contains(anchor.Label, searchText)
            || Contains(anchor.Path, searchText)
            || Contains(anchor.Evidence, searchText));

    private static bool ContainsAny(IReadOnlyList<DemoCookbookScenarioPoint> scenarioPoints, string searchText)
        => scenarioPoints.Any(point =>
            Contains(point.Kind.ToString(), searchText)
            || Contains(point.Label, searchText)
            || Contains(point.Evidence, searchText));
}
