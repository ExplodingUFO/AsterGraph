using System;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookWorkspaceProjectionTests
{
    [Fact]
    public void WorkspaceProjection_DefaultsToFirstRecipeAndBuildsLeftNavigation()
    {
        var snapshot = DemoCookbookWorkspaceProjection.Create();

        Assert.Equal(DemoCookbookCatalog.RequiredCategories.Count, snapshot.NavigationGroups.Count);
        Assert.Equal(DemoCookbookCatalog.Recipes[0].Id, snapshot.SelectedRecipe.RecipeId);
        Assert.Equal(
            DemoCookbookCatalog.Recipes.Count,
            snapshot.NavigationGroups.SelectMany(group => group.Recipes).Count());
        Assert.Single(
            snapshot.NavigationGroups.SelectMany(group => group.Recipes),
            item => item.IsSelected);

        foreach (var category in DemoCookbookCatalog.RequiredCategories)
        {
            var group = snapshot.NavigationGroups.Single(item => item.Category == category);
            Assert.NotEmpty(group.DisplayName);
            Assert.All(group.Recipes, recipe => Assert.Equal(category, recipe.Category));
        }
    }

    [Fact]
    public void WorkspaceProjection_ProjectsEveryRecipeIntoGraphAboveCodeContent()
    {
        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            var snapshot = DemoCookbookWorkspaceProjection.Create(recipe.Id);
            var content = snapshot.SelectedRecipe;

            Assert.Equal(recipe.Id, content.RecipeId);
            Assert.Equal(recipe.Title, content.Title);
            Assert.Equal(recipe.Summary, content.Summary);
            Assert.Equal(recipe.Category, content.Category);
            Assert.False(string.IsNullOrWhiteSpace(content.RouteStatus));
            Assert.False(string.IsNullOrWhiteSpace(content.RouteStatusDescription));
            Assert.False(string.IsNullOrWhiteSpace(content.UnavailableActionDescription));
            AssertEquivalentAnchors(recipe.DemoAnchors, content.GraphAnchors);
            AssertEquivalentAnchors(recipe.CodeAnchors, content.CodeExamples);
            AssertEquivalentAnchors(recipe.DocumentationAnchors, content.DocumentationLinks);
            AssertEquivalentScenarioPoints(recipe.ScenarioPoints, content.ScenarioPoints);
            Assert.Equal(recipe.ProofMarkers, content.ProofMarkers);
            Assert.NotEmpty(content.DeferredGaps);
            Assert.Equal(recipe.SupportBoundary, content.SupportBoundary);
        }
    }

    [Fact]
    public void WorkspaceProjection_LabelsSupportedProofAndDeferredRouteDepth()
    {
        var starter = DemoCookbookWorkspaceProjection.Create("starter-host-route").SelectedRecipe;
        var plugin = DemoCookbookWorkspaceProjection.Create("plugin-trust-route").SelectedRecipe;

        Assert.Equal("Supported SDK route", starter.RouteStatus);
        Assert.Contains(starter.DeferredGaps, gap => gap.Contains("WPF", StringComparison.Ordinal));
        Assert.Contains("display guidance", starter.UnavailableActionDescription, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Proof/demo route", plugin.RouteStatus);
        Assert.Contains(plugin.DeferredGaps, gap => gap.Contains("sandbox", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("no sandbox", plugin.UnavailableActionDescription, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WorkspaceProjection_LeftNavigationCanUseFilteredRecipeSetWithoutChangingSelection()
    {
        var selectedRecipe = DemoCookbookCatalog.Recipes.Single(recipe => recipe.Id == "plugin-trust-route");
        var filteredRecipes = DemoCookbookCatalog.Recipes
            .Where(recipe => recipe.Category == DemoCookbookRecipeCategory.DiagnosticsSupport)
            .ToArray();

        var snapshot = DemoCookbookWorkspaceProjection.Create(selectedRecipe.Id, filteredRecipes);

        var group = Assert.Single(snapshot.NavigationGroups);
        Assert.Equal(DemoCookbookRecipeCategory.DiagnosticsSupport, group.Category);
        Assert.Equal(filteredRecipes.Select(recipe => recipe.Id), group.Recipes.Select(recipe => recipe.RecipeId));
        Assert.DoesNotContain(group.Recipes, recipe => recipe.IsSelected);
        Assert.Equal(selectedRecipe.Id, snapshot.SelectedRecipe.RecipeId);
    }

    [Fact]
    public void WorkspaceProjection_RejectsUnknownRecipeId()
    {
        Assert.Throws<InvalidOperationException>(() => DemoCookbookWorkspaceProjection.Create("missing-recipe"));
    }

    private static void AssertEquivalentAnchors(
        IReadOnlyList<DemoCookbookAnchor> expected,
        IReadOnlyList<DemoCookbookWorkspaceAnchor> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Label, actual[index].Label);
            Assert.Equal(expected[index].Path, actual[index].Path);
            Assert.Equal(expected[index].Evidence, actual[index].Evidence);
        }
    }

    private static void AssertEquivalentScenarioPoints(
        IReadOnlyList<DemoCookbookScenarioPoint> expected,
        IReadOnlyList<DemoCookbookWorkspaceScenarioPoint> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Kind, actual[index].Kind);
            Assert.Equal(expected[index].Label, actual[index].Label);
            Assert.Equal(expected[index].Evidence, actual[index].Evidence);
        }
    }
}
