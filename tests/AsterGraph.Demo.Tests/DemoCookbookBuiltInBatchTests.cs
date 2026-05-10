using System;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookBuiltInBatchTests
{
    [Fact]
    public void CookbookCatalog_AddsBuiltInWorkbenchRecipeBatch()
    {
        Assert.True(DemoCookbookCatalog.Recipes.Count >= 14);
        AssertBuiltInRecipe(
            "builtin-minimap-workbench-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "MINIMAP_LIGHTWEIGHT_PROJECTION_OK",
            "minimap-workbench");
        AssertBuiltInRecipe(
            "builtin-background-grid-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "GRID_BACKGROUND_DENSITY_OK",
            "background-grid-density");
        AssertBuiltInRecipe(
            "builtin-hosted-controls-route",
            DemoCookbookRecipeCategory.Authoring,
            "HOSTED_CONTROLS_PANEL_COMPOSITION_OK",
            "hosted-controls-panel");
    }

    private static void AssertBuiltInRecipe(
        string id,
        DemoCookbookRecipeCategory category,
        string proofMarker,
        string scenarioId)
    {
        var recipe = Assert.Single(DemoCookbookCatalog.Recipes, candidate =>
            string.Equals(candidate.Id, id, StringComparison.Ordinal));

        Assert.Equal(category, recipe.Category);
        Assert.Contains(recipe.DemoAnchors, anchor =>
            anchor.Path == "src/AsterGraph.Demo/DemoGraphFactory.cs"
            && anchor.Evidence == scenarioId);
        Assert.Contains(recipe.ProofMarkers, marker => marker == proofMarker);
        Assert.Contains(scenarioId, recipe.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.DoesNotContain("React Flow", recipe.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("React Flow", recipe.RouteClarity.SupportedRoute, StringComparison.OrdinalIgnoreCase);
    }
}
