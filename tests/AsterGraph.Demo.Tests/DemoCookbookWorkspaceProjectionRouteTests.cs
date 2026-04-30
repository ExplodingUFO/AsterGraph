using System;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookWorkspaceProjectionRouteTests
{
    [Fact]
    public void WorkspaceProjection_ProjectsV077WorkflowStepTargets()
    {
        var recipe = DemoCookbookCatalog.Recipes.Single(item => item.Id == "v077-authoring-platform-route");
        var content = DemoCookbookWorkspaceProjection.Create(recipe.Id).SelectedRecipe;

        Assert.Equal(recipe.WorkflowSteps.Count, content.WorkflowSteps.Count);
        Assert.Contains(content.WorkflowSteps, step =>
            step.Kind == DemoCookbookWorkflowKind.CommandRegistry
            && step.CommandId == "query.command-registry"
            && step.CodeTarget.Contains("src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs#GetCommandRegistry", StringComparison.Ordinal));
        Assert.Contains(content.WorkflowSteps, step =>
            step.Kind == DemoCookbookWorkflowKind.NavigationFocus
            && step.CommandId == "viewport.focus-search-result"
            && step.DemoTarget.Contains("tests/AsterGraph.Editor.Tests/GraphEditorNavigationFocusWorkflowTests.cs#Commands_ViewportBookmarksCaptureActivateAndRemoveCurrentScopeViewport", StringComparison.Ordinal));
        Assert.All(content.WorkflowSteps, step =>
        {
            Assert.StartsWith(recipe.Id + ":workflow-", step.Key, StringComparison.Ordinal);
            Assert.Contains(step.ProofMarker, recipe.ProofMarkers);
            Assert.False(string.IsNullOrWhiteSpace(step.CodeTarget));
            Assert.False(string.IsNullOrWhiteSpace(step.DemoTarget));
        });
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
}
