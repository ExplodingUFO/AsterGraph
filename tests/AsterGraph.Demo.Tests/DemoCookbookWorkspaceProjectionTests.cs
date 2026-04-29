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
            AssertEquivalentAnchors(recipe.DemoAnchors, content.GraphAnchors);
            AssertEquivalentAnchors(recipe.CodeAnchors, content.CodeExamples);
            AssertEquivalentAnchors(recipe.DocumentationAnchors, content.DocumentationLinks);
            Assert.Equal(recipe.ProofMarkers, content.ProofMarkers);
            Assert.Equal(recipe.SupportBoundary, content.SupportBoundary);
        }
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
}
