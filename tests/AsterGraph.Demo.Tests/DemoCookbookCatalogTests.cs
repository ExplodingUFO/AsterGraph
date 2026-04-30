using System;
using System.IO;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookCatalogTests
{
    [Fact]
    public void CookbookCatalog_HasValidRequiredMetadata()
    {
        var issues = DemoCookbookCatalog.Validate();

        Assert.Empty(issues);
        Assert.Equal(
            DemoCookbookCatalog.Recipes.Count,
            DemoCookbookCatalog.Recipes.Select(recipe => recipe.Id).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void CookbookCatalog_CoversRequiredRecipeCategories()
    {
        Assert.Equal(
            new[]
            {
                DemoCookbookRecipeCategory.StarterHost,
                DemoCookbookRecipeCategory.Authoring,
                DemoCookbookRecipeCategory.PerformanceViewport,
                DemoCookbookRecipeCategory.GroupsSubgraphs,
                DemoCookbookRecipeCategory.PluginTrust,
                DemoCookbookRecipeCategory.DiagnosticsSupport,
                DemoCookbookRecipeCategory.ReviewHelp,
            },
            DemoCookbookCatalog.RequiredCategories);

        foreach (var category in DemoCookbookCatalog.RequiredCategories)
        {
            Assert.Contains(DemoCookbookCatalog.Recipes, recipe => recipe.Category == category);
        }
    }

    [Fact]
    public void CookbookCatalog_ReferencesConcreteCodeDemoAndDocsAnchors()
    {
        var repositoryRoot = GetRepositoryRoot();

        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            Assert.NotEmpty(recipe.CodeAnchors);
            Assert.NotEmpty(recipe.DemoAnchors);
            Assert.NotEmpty(recipe.DocumentationAnchors);
            Assert.NotEmpty(recipe.ScenarioPoints);
            Assert.NotEmpty(recipe.InteractionFacets);
            Assert.NotEmpty(recipe.ProofMarkers);
            AssertRouteClarityIsBounded(recipe);
            Assert.False(string.IsNullOrWhiteSpace(recipe.SupportBoundary), $"{recipe.Id} support boundary is missing.");

            AssertAnchorsExist(repositoryRoot, recipe.Id, nameof(recipe.CodeAnchors), recipe.CodeAnchors);
            AssertAnchorsExist(repositoryRoot, recipe.Id, nameof(recipe.DemoAnchors), recipe.DemoAnchors);
            AssertAnchorsExist(repositoryRoot, recipe.Id, nameof(recipe.DocumentationAnchors), recipe.DocumentationAnchors);
        }
    }

    [Fact]
    public void CookbookCatalog_CoversProfessionalScenarioDepth()
    {
        Assert.Equal(
            new[]
            {
                DemoCookbookScenarioKind.GraphOperations,
                DemoCookbookScenarioKind.NodeMetadata,
                DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                DemoCookbookScenarioKind.SupportEvidence,
                DemoCookbookScenarioKind.HostCodeExample,
            },
            DemoCookbookCatalog.RequiredScenarioKinds);

        var scenarioKinds = DemoCookbookCatalog.Recipes
            .SelectMany(recipe => recipe.ScenarioPoints)
            .Select(point => point.Kind)
            .Distinct()
            .ToArray();

        foreach (var requiredScenarioKind in DemoCookbookCatalog.RequiredScenarioKinds)
        {
            Assert.Contains(requiredScenarioKind, scenarioKinds);
        }

        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            Assert.True(recipe.ScenarioPoints.Count >= 3, $"{recipe.Id} needs professional scenario depth.");
            Assert.All(recipe.ScenarioPoints, point => Assert.False(string.IsNullOrWhiteSpace(point.Label)));
            Assert.All(recipe.ScenarioPoints, point => Assert.False(string.IsNullOrWhiteSpace(point.Evidence)));
            AssertScenarioPointsTieBackToRecipeEvidence(recipe);
            AssertInteractionFacetsTieBackToRecipeEvidence(recipe);
        }
    }

    [Fact]
    public void CookbookCatalog_CoversRequiredProfessionalInteractionFacets()
    {
        Assert.Equal(
            new[]
            {
                DemoCookbookInteractionKind.Selection,
                DemoCookbookInteractionKind.Connection,
                DemoCookbookInteractionKind.LayoutReadability,
                DemoCookbookInteractionKind.Inspection,
                DemoCookbookInteractionKind.ValidationRuntimeFeedback,
            },
            DemoCookbookCatalog.RequiredInteractionKinds);

        var interactionKinds = DemoCookbookCatalog.Recipes
            .SelectMany(recipe => recipe.InteractionFacets)
            .Select(facet => facet.Kind)
            .Distinct()
            .ToArray();

        foreach (var requiredInteractionKind in DemoCookbookCatalog.RequiredInteractionKinds)
        {
            Assert.Contains(requiredInteractionKind, interactionKinds);
        }

        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            Assert.True(recipe.InteractionFacets.Count >= 3, $"{recipe.Id} needs professional interaction facets.");
            Assert.All(recipe.InteractionFacets, facet => Assert.False(string.IsNullOrWhiteSpace(facet.Label)));
            Assert.All(recipe.InteractionFacets, facet => Assert.False(string.IsNullOrWhiteSpace(facet.Evidence)));
            AssertInteractionFacetsTieBackToRecipeEvidence(recipe);
        }
    }

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

    [Fact]
    public void CookbookCatalog_ProofMarkersAreBackedByExistingEvidence()
    {
        var repositoryRoot = GetRepositoryRoot();
        var evidenceText = string.Join(
            Environment.NewLine,
            ReadAllText(repositoryRoot, "src/AsterGraph.Demo"),
            ReadAllText(repositoryRoot, "tools/AsterGraph.ConsumerSample.Avalonia"),
            ReadAllText(repositoryRoot, "tests"),
            ReadAllText(repositoryRoot, "docs/en"),
            ReadAllText(repositoryRoot, "docs/zh-CN"));

        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            foreach (var proofMarker in recipe.ProofMarkers)
            {
                Assert.Contains(proofMarker, evidenceText, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void CookbookCatalog_StaysOutOfDemoViewModelAggregation()
    {
        var repositoryRoot = GetRepositoryRoot();
        var showcaseViewModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs"));

        Assert.DoesNotContain("DemoCookbookCatalog", showcaseViewModel, StringComparison.Ordinal);
        Assert.DoesNotContain("DemoCookbookRecipe", showcaseViewModel, StringComparison.Ordinal);
    }

    private static string ReadAllText(string repositoryRoot, string relativePath)
    {
        var root = Path.Combine(repositoryRoot, relativePath);
        return string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(path => path.EndsWith(".cs", StringComparison.Ordinal) || path.EndsWith(".md", StringComparison.Ordinal))
                .Select(File.ReadAllText));
    }

    private static void AssertAnchorsExist(
        string repositoryRoot,
        string recipeId,
        string anchorSetName,
        IReadOnlyList<DemoCookbookAnchor> anchors)
    {
        for (var index = 0; index < anchors.Count; index++)
        {
            var anchor = anchors[index];
            var path = Path.Combine(repositoryRoot, anchor.Path);

            Assert.False(string.IsNullOrWhiteSpace(anchor.Label), $"{recipeId} {anchorSetName}[{index}] label is missing.");
            Assert.False(string.IsNullOrWhiteSpace(anchor.Path), $"{recipeId} {anchorSetName}[{index}] path is missing.");
            Assert.False(string.IsNullOrWhiteSpace(anchor.Evidence), $"{recipeId} {anchorSetName}[{index}] evidence is missing.");
            Assert.True(File.Exists(path), $"{recipeId} {anchorSetName}[{index}] path does not exist: {anchor.Path}");
            Assert.Contains(anchor.Evidence, File.ReadAllText(path), StringComparison.Ordinal);
        }
    }

    private static void AssertScenarioPointsTieBackToRecipeEvidence(DemoCookbookRecipe recipe)
        => AssertEvidenceBackedItemsTieBackToRecipeEvidence(
            recipe,
            recipe.ScenarioPoints.Select(point => point.Evidence).ToArray());

    private static void AssertInteractionFacetsTieBackToRecipeEvidence(DemoCookbookRecipe recipe)
        => AssertEvidenceBackedItemsTieBackToRecipeEvidence(
            recipe,
            recipe.InteractionFacets.Select(facet => facet.Evidence).ToArray());

    private static void AssertWorkflowStepsTieBackToRecipeEvidence(DemoCookbookRecipe recipe)
        => AssertEvidenceBackedItemsTieBackToRecipeEvidence(
            recipe,
            recipe.WorkflowSteps
                .SelectMany(step => new[] { step.CodeEvidence, step.DemoEvidence, step.ProofMarker })
                .ToArray());

    private static void AssertEvidenceBackedItemsTieBackToRecipeEvidence(
        DemoCookbookRecipe recipe,
        IReadOnlyList<string> evidenceItems)
    {
        var recipeEvidence = recipe.CodeAnchors
            .Concat(recipe.DemoAnchors)
            .Concat(recipe.DocumentationAnchors)
            .Select(anchor => anchor.Evidence)
            .Concat(recipe.ProofMarkers)
            .ToArray();

        foreach (var evidence in evidenceItems)
        {
            Assert.Contains(evidence, recipeEvidence);
        }
    }

    private static void AssertRouteClarityIsBounded(DemoCookbookRecipe recipe)
    {
        Assert.False(string.IsNullOrWhiteSpace(recipe.RouteClarity.SupportedRoute), $"{recipe.Id} route is missing.");
        Assert.False(string.IsNullOrWhiteSpace(recipe.RouteClarity.PackageBoundary), $"{recipe.Id} package boundary is missing.");
        Assert.False(string.IsNullOrWhiteSpace(recipe.RouteClarity.DemoBoundary), $"{recipe.Id} Demo boundary is missing.");
        Assert.Contains("AsterGraph.", recipe.RouteClarity.PackageBoundary, StringComparison.Ordinal);
        Assert.Contains("Demo", recipe.RouteClarity.DemoBoundary, StringComparison.Ordinal);
        Assert.DoesNotContain("fallback", recipe.RouteClarity.SupportedRoute, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sandbox is active", recipe.RouteClarity.DemoBoundary, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Failed to locate repository root from test base directory.");
    }
}
