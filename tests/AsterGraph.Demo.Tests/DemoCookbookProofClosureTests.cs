using System;
using System.IO;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookProofClosureTests
{
    [Fact]
    public void DemoCookbookProof_Run_EmitsGreenClosureMarkers()
    {
        var result = DemoCookbookProof.Run();
        var proofLines = DemoCookbookProof.CreateProofLines();

        Assert.True(result.IsOk);
        Assert.True(result.WorkspaceLayoutOk);
        Assert.True(result.RouteCoverageOk);
        Assert.Equal(DemoCookbookCatalog.Recipes.Count, result.RecipeCount);
        Assert.Equal(DemoCookbookCatalog.RequiredCategories.Count, result.RequiredCategoryCount);

        foreach (var markerId in DemoCookbookProof.PublicSuccessMarkerIds)
        {
            Assert.Contains(proofLines, line => string.Equals(line, $"{markerId}:True", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void DemoCookbookProof_PublicMarkersAreUniqueAndScoped()
    {
        Assert.Equal(
            DemoCookbookProof.PublicSuccessMarkerIds.Count,
            DemoCookbookProof.PublicSuccessMarkerIds.Distinct(StringComparer.Ordinal).Count());

        Assert.All(
            DemoCookbookProof.PublicSuccessMarkerIds,
            markerId => Assert.StartsWith("DEMO_COOKBOOK_", markerId, StringComparison.Ordinal));
    }

    [Fact]
    public void CookbookClosure_StaysInNarrowOwnedFiles()
    {
        var repositoryRoot = GetRepositoryRoot();
        var ownedFiles = new[]
        {
            "src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.cs",
            "src/AsterGraph.Demo/Cookbook/DemoCookbookProof.cs",
            "src/AsterGraph.Demo/Cookbook/DemoCookbookWorkspaceProjection.cs",
            "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Cookbook.cs",
            "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.CookbookDetails.cs",
            "docs/en/demo-cookbook.md",
            "docs/zh-CN/demo-cookbook.md",
            "tests/AsterGraph.Demo.Tests/DemoCookbookCatalogTests.cs",
            "tests/AsterGraph.Demo.Tests/DemoCookbookNavigationTests.cs",
            "tests/AsterGraph.Demo.Tests/DemoCookbookDocsTests.cs",
            "tests/AsterGraph.Demo.Tests/DemoCookbookProofClosureTests.cs",
            "tests/AsterGraph.Demo.Tests/DemoCookbookVisualBaselineTests.cs",
            "tests/AsterGraph.Demo.Tests/DemoCookbookWorkspaceProjectionTests.cs",
        };

        foreach (var relativePath in ownedFiles)
        {
            var fullPath = Path.Combine(repositoryRoot, relativePath);
            Assert.True(File.Exists(fullPath), $"Missing cookbook closure file: {relativePath}");
            Assert.True(
                File.ReadLines(fullPath).Count() <= 260,
                $"Cookbook file is too large for narrow ownership: {relativePath}");
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
