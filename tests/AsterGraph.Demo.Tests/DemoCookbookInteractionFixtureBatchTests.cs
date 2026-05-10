using System;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookInteractionFixtureBatchTests
{
    [Fact]
    public void CookbookCatalog_AddsInteractionFixtureRecipeBatch()
    {
        Assert.True(DemoCookbookCatalog.Recipes.Count >= 17);
        AssertInteractionFixtureRecipe(
            "interaction-selection-marquee-route",
            DemoCookbookRecipeCategory.Authoring,
            "INTERACTION_SELECTION_MARQUEE_FIXTURE_OK",
            "selection-marquee-workbench");
        AssertInteractionFixtureRecipe(
            "interaction-keyboard-navigation-route",
            DemoCookbookRecipeCategory.Authoring,
            "INTERACTION_KEYBOARD_NAVIGATION_FIXTURE_OK",
            "keyboard-navigation-lab");
        AssertInteractionFixtureRecipe(
            "interaction-host-event-inspector-route",
            DemoCookbookRecipeCategory.ReviewHelp,
            "INTERACTION_HOST_EVENT_INSPECTOR_FIXTURE_OK",
            "host-event-inspector");
    }

    private static void AssertInteractionFixtureRecipe(
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
        Assert.Contains("screenshot-gate coverage only", recipe.RouteClarity.DemoBoundary, StringComparison.Ordinal);
        Assert.DoesNotContain("React Flow", recipe.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("React Flow", recipe.RouteClarity.SupportedRoute, StringComparison.OrdinalIgnoreCase);
    }
}
