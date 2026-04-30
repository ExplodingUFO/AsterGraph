using System;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookV077CatalogTests
{
    [Fact]
    public void CookbookCatalog_CoversV077AuthoringWorkflowSteps()
    {
        var recipe = Assert.Single(DemoCookbookCatalog.Recipes, item => item.Id == "v077-authoring-platform-route");

        Assert.Equal(
            new[]
            {
                DemoCookbookWorkflowKind.CommandRegistry,
                DemoCookbookWorkflowKind.SemanticEditing,
                DemoCookbookWorkflowKind.TemplatePreset,
                DemoCookbookWorkflowKind.SelectionTransform,
                DemoCookbookWorkflowKind.NavigationFocus,
            },
            recipe.WorkflowSteps.Select(step => step.Kind).ToArray());

        Assert.Equal(
            new[]
            {
                "query.command-registry",
                "selection.delete-reconnect",
                "fragments.apply-template-preset",
                "selection.transform.move",
                "viewport.focus-search-result",
            },
            recipe.WorkflowSteps.Select(step => step.CommandId).ToArray());

        AssertWorkflowStepsTieBackToRecipeEvidence(recipe);
        Assert.All(recipe.WorkflowSteps, step => Assert.Contains(step.ProofMarker, recipe.ProofMarkers));
        Assert.DoesNotContain("workflow engine", recipe.SupportBoundary, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertWorkflowStepsTieBackToRecipeEvidence(DemoCookbookRecipe recipe)
    {
        var recipeEvidence = recipe.CodeAnchors
            .Concat(recipe.DemoAnchors)
            .Concat(recipe.DocumentationAnchors)
            .Select(anchor => anchor.Evidence)
            .Concat(recipe.ProofMarkers)
            .ToArray();

        var workflowEvidence = recipe.WorkflowSteps
            .SelectMany(step => new[] { step.CodeEvidence, step.DemoEvidence, step.ProofMarker })
            .ToArray();

        foreach (var evidence in workflowEvidence)
        {
            Assert.Contains(evidence, recipeEvidence);
        }
    }
}
