using System;
using System.IO;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookProfessionalLibraryCoverageTests
{
    [Fact]
    public void CookbookCatalog_CoversProfessionalDesktopNodeGraphLibraryPillars()
    {
        var allText = string.Join(
            Environment.NewLine,
            DemoCookbookCatalog.Recipes.SelectMany(recipe => new[]
            {
                recipe.Id,
                recipe.Category.ToString(),
                recipe.Title,
                recipe.Summary,
                recipe.SupportBoundary,
                recipe.RouteClarity.SupportedRoute,
                recipe.RouteClarity.PackageBoundary,
                recipe.RouteClarity.DemoBoundary,
            })
            .Concat(DemoCookbookCatalog.Recipes.SelectMany(recipe => recipe.CodeAnchors.Select(anchor => anchor.Evidence)))
            .Concat(DemoCookbookCatalog.Recipes.SelectMany(recipe => recipe.DemoAnchors.Select(anchor => anchor.Evidence)))
            .Concat(DemoCookbookCatalog.Recipes.SelectMany(recipe => recipe.DocumentationAnchors.Select(anchor => anchor.Evidence)))
            .Concat(DemoCookbookCatalog.Recipes.SelectMany(recipe => recipe.ProofMarkers)));

        foreach (var pillar in new[]
        {
            "BuildAvaloniaView",
            "CreateEdgeOverlay",
            "ToBudgetMarker",
            "ToMiniMapBudgetMarker",
            "SCALE_PERFORMANCE_BUDGET_OK",
            "MINIMAP_LIGHTWEIGHT_PROJECTION_OK",
            "PluginTrustPolicy",
            "RuntimeOverlayProvider",
            "RepairHelpReviewLoopOk",
        })
        {
            Assert.Contains(pillar, allText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CookbookDocs_SurfaceProfessionalDesktopNodeGraphLibraryPillarsInBothLocales()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("performance-viewport-route", contents, StringComparison.Ordinal);
            Assert.Contains("PerformanceViewport", contents, StringComparison.Ordinal);
            Assert.Contains("ViewportVisibleSceneProjector.Project", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphWorkbenchPerformancePolicy.FromMode", contents, StringComparison.Ordinal);
            Assert.Contains("ScaleSmoke", contents, StringComparison.Ordinal);
            Assert.Contains("MINIMAP_LIGHTWEIGHT_PROJECTION_OK", contents, StringComparison.Ordinal);
            Assert.Contains("PROJECTION_PERFORMANCE_EVIDENCE_OK", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK", contents, StringComparison.Ordinal);
            Assert.Contains("does not add a background graph index, second renderer, or runtime execution mode", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

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
