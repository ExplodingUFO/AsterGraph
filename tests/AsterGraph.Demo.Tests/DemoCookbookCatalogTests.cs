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
            Assert.NotEmpty(recipe.ProofMarkers);
            Assert.False(string.IsNullOrWhiteSpace(recipe.SupportBoundary), $"{recipe.Id} support boundary is missing.");

            AssertAnchorsExist(repositoryRoot, recipe.Id, nameof(recipe.CodeAnchors), recipe.CodeAnchors);
            AssertAnchorsExist(repositoryRoot, recipe.Id, nameof(recipe.DemoAnchors), recipe.DemoAnchors);
            AssertAnchorsExist(repositoryRoot, recipe.Id, nameof(recipe.DocumentationAnchors), recipe.DocumentationAnchors);
        }
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
