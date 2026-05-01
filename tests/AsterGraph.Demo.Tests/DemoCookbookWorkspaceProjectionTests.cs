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
            AssertEquivalentInteractionFacets(recipe.InteractionFacets, content.InteractionFacets);
            AssertEquivalentWorkflowSteps(recipe.WorkflowSteps, content.WorkflowSteps);
            Assert.All(content.ComponentShowcaseLines, line => Assert.False(string.IsNullOrWhiteSpace(line)));
            Assert.Equal(recipe.ProofMarkers, content.ProofMarkers);
            Assert.NotEmpty(content.DeferredGaps);
            Assert.Equal(recipe.RouteClarity, content.RouteClarity);
            Assert.Equal(recipe.SupportBoundary, content.SupportBoundary);
        }
    }

    [Fact]
    public void WorkspaceProjection_ProjectsSourceBackedRouteClarity()
    {
        var starter = DemoCookbookWorkspaceProjection.Create("starter-host-route").SelectedRecipe;
        var plugin = DemoCookbookWorkspaceProjection.Create("plugin-trust-route").SelectedRecipe;

        Assert.Contains("AsterGraphHostBuilder.Create", starter.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.Avalonia", starter.RouteClarity.PackageBoundary, StringComparison.Ordinal);
        Assert.Contains("copy the starter host code", starter.RouteClarity.DemoBoundary, StringComparison.Ordinal);
        Assert.Contains("DiscoverPluginCandidates", plugin.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.Contains("PluginTrustPolicy", plugin.RouteClarity.SupportedRoute, StringComparison.Ordinal);
        Assert.DoesNotContain("sandbox is active", plugin.RouteClarity.DemoBoundary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WorkspaceProjection_LabelsSupportedProofAndDeferredRouteDepth()
    {
        var starter = DemoCookbookWorkspaceProjection.Create("starter-host-route").SelectedRecipe;
        var authoring = DemoCookbookWorkspaceProjection.Create("authoring-surface-route").SelectedRecipe;
        var plugin = DemoCookbookWorkspaceProjection.Create("plugin-trust-route").SelectedRecipe;
        var diagnostics = DemoCookbookWorkspaceProjection.Create("diagnostics-support-route").SelectedRecipe;
        var review = DemoCookbookWorkspaceProjection.Create("review-help-route").SelectedRecipe;

        Assert.Equal("Supported SDK route", starter.RouteStatus);
        Assert.Equal("Supported SDK route", authoring.RouteStatus);
        Assert.Contains(starter.DeferredGaps, gap => gap.Contains("WPF", StringComparison.Ordinal));
        Assert.Contains("display guidance", starter.UnavailableActionDescription, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Proof/demo route", plugin.RouteStatus);
        Assert.Equal("Proof/demo route", diagnostics.RouteStatus);
        Assert.Equal("Proof/demo route", review.RouteStatus);
        Assert.Contains(plugin.DeferredGaps, gap => gap.Contains("sandbox", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("no sandbox", plugin.UnavailableActionDescription, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WorkspaceProjection_ProjectsBoundedScenarioGraphCues()
    {
        var content = DemoCookbookWorkspaceProjection.Create("authoring-surface-route").SelectedRecipe;
        var graphCue = content.ScenarioPoints.Single(point => point.Kind == DemoCookbookScenarioKind.GraphOperations);
        var metadataCue = content.ScenarioPoints.Single(point => point.Kind == DemoCookbookScenarioKind.NodeMetadata);

        Assert.StartsWith(content.RecipeId + ":scenario-", graphCue.Key, StringComparison.Ordinal);
        Assert.Equal("Graph operation cue", graphCue.GraphCueLabel);
        Assert.Contains("tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs", graphCue.GraphCueTarget, StringComparison.Ordinal);
        Assert.Contains(graphCue.Evidence, graphCue.GraphCueTarget, StringComparison.Ordinal);
        Assert.Equal("authoring-surface-route / GraphOperations", graphCue.ContentCue);
        Assert.Equal("Node metadata cue", metadataCue.GraphCueLabel);
        Assert.Contains("tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs", metadataCue.GraphCueTarget, StringComparison.Ordinal);
        Assert.DoesNotContain("fallback", content.ScenarioPoints.Select(point => point.ContentCue), StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("script", content.ScenarioPoints.Select(point => point.ContentCue), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void WorkspaceProjection_ScenarioCueTargetsUseOwningEvidenceSource()
    {
        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            var content = DemoCookbookWorkspaceProjection.Create(recipe.Id).SelectedRecipe;

            foreach (var scenario in content.ScenarioPoints)
            {
                var anchor = recipe.CodeAnchors
                    .Concat(recipe.DemoAnchors)
                    .Concat(recipe.DocumentationAnchors)
                    .FirstOrDefault(item => string.Equals(item.Evidence, scenario.Evidence, StringComparison.Ordinal));

                if (anchor is not null)
                {
                    Assert.Equal(anchor.Path + "#" + scenario.Evidence, scenario.GraphCueTarget);
                    continue;
                }

                Assert.Contains(scenario.Evidence, recipe.ProofMarkers);
                Assert.Equal("proof:" + scenario.Evidence, scenario.GraphCueTarget);
            }
        }
    }

    [Fact]
    public void WorkspaceProjection_ProjectsProfessionalInteractionFacets()
    {
        var content = DemoCookbookWorkspaceProjection.Create("review-help-route").SelectedRecipe;
        var validation = content.InteractionFacets.Single(point =>
            point.Kind == DemoCookbookInteractionKind.ValidationRuntimeFeedback);
        var connection = content.InteractionFacets.Single(point =>
            point.Kind == DemoCookbookInteractionKind.Connection);

        Assert.StartsWith(content.RecipeId + ":interaction-", validation.Key, StringComparison.Ordinal);
        Assert.Equal("Validation/runtime feedback focus", validation.FocusLabel);
        Assert.Contains("tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs", validation.FocusTarget, StringComparison.Ordinal);
        Assert.Contains(validation.Evidence, validation.FocusTarget, StringComparison.Ordinal);
        Assert.Equal("Connection focus", connection.FocusLabel);
        Assert.Contains("tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs", connection.FocusTarget, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkspaceProjection_InteractionFocusTargetsUseOwningEvidenceSource()
    {
        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            var content = DemoCookbookWorkspaceProjection.Create(recipe.Id).SelectedRecipe;

            foreach (var facet in content.InteractionFacets)
            {
                var anchor = recipe.CodeAnchors
                    .Concat(recipe.DemoAnchors)
                    .Concat(recipe.DocumentationAnchors)
                    .FirstOrDefault(item => string.Equals(item.Evidence, facet.Evidence, StringComparison.Ordinal));

                if (anchor is not null)
                {
                    Assert.Equal(anchor.Path + "#" + facet.Evidence, facet.FocusTarget);
                    continue;
                }

                Assert.Contains(facet.Evidence, recipe.ProofMarkers);
                Assert.Equal("proof:" + facet.Evidence, facet.FocusTarget);
            }
        }
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
            Assert.False(string.IsNullOrWhiteSpace(actual[index].Key));
            Assert.False(string.IsNullOrWhiteSpace(actual[index].GraphCueLabel));
            Assert.False(string.IsNullOrWhiteSpace(actual[index].GraphCueTarget));
            Assert.False(string.IsNullOrWhiteSpace(actual[index].ContentCue));
        }
    }

    private static void AssertEquivalentInteractionFacets(
        IReadOnlyList<DemoCookbookInteractionFacet> expected,
        IReadOnlyList<DemoCookbookWorkspaceInteractionFacet> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Kind, actual[index].Kind);
            Assert.Equal(expected[index].Label, actual[index].Label);
            Assert.Equal(expected[index].Evidence, actual[index].Evidence);
            Assert.False(string.IsNullOrWhiteSpace(actual[index].Key));
            Assert.False(string.IsNullOrWhiteSpace(actual[index].FocusLabel));
            Assert.False(string.IsNullOrWhiteSpace(actual[index].FocusTarget));
        }
    }

    private static void AssertEquivalentWorkflowSteps(
        IReadOnlyList<DemoCookbookWorkflowStep> expected,
        IReadOnlyList<DemoCookbookWorkspaceWorkflowStep> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Kind, actual[index].Kind);
            Assert.Equal(expected[index].Title, actual[index].Title);
            Assert.Equal(expected[index].CommandId, actual[index].CommandId);
            Assert.Equal(expected[index].ProofMarker, actual[index].ProofMarker);
            Assert.False(string.IsNullOrWhiteSpace(actual[index].Key));
            Assert.False(string.IsNullOrWhiteSpace(actual[index].CodeTarget));
            Assert.False(string.IsNullOrWhiteSpace(actual[index].DemoTarget));
        }
    }
}
