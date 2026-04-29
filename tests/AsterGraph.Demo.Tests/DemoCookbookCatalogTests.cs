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
        foreach (var category in DemoCookbookCatalog.RequiredCategories)
        {
            Assert.Contains(DemoCookbookCatalog.Recipes, recipe => recipe.Category == category);
        }
    }

    [Fact]
    public void CookbookCatalog_ReferencesExistingCodeAndDocsPaths()
    {
        var repositoryRoot = GetRepositoryRoot();

        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            Assert.True(
                File.Exists(Path.Combine(repositoryRoot, recipe.CodePath)),
                $"{recipe.Id} code path does not exist: {recipe.CodePath}");
            Assert.True(
                File.Exists(Path.Combine(repositoryRoot, recipe.DocumentationPath)),
                $"{recipe.Id} docs path does not exist: {recipe.DocumentationPath}");
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
            Assert.Contains(recipe.ProofMarker, evidenceText, StringComparison.Ordinal);
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
