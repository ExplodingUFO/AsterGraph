using System;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookLifecycleFixtureBatchTests
{
    [Fact]
    public void CookbookCatalog_AddsLifecycleFixtureRecipeBatch()
    {
        Assert.True(DemoCookbookCatalog.Recipes.Count >= 20);
        AssertLifecycleFixtureRecipe(
            "lifecycle-workspace-save-restore-route",
            DemoCookbookRecipeCategory.DiagnosticsSupport,
            "LIFECYCLE_WORKSPACE_SAVE_RESTORE_FIXTURE_OK",
            "workspace-save-restore");
        AssertLifecycleFixtureRecipe(
            "lifecycle-clipboard-fragment-route",
            DemoCookbookRecipeCategory.Authoring,
            "LIFECYCLE_CLIPBOARD_FRAGMENT_FIXTURE_OK",
            "clipboard-fragment-roundtrip");
        AssertLifecycleFixtureRecipe(
            "lifecycle-validation-helper-route",
            DemoCookbookRecipeCategory.ReviewHelp,
            "LIFECYCLE_VALIDATION_HELPER_FIXTURE_OK",
            "validation-prevent-cycle");
    }

    private static void AssertLifecycleFixtureRecipe(
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
