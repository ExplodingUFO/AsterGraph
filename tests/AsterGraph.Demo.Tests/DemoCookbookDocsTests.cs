using System;
using System.IO;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookDocsTests
{
    [Fact]
    public void DemoCookbookDocs_IndexEveryCatalogRecipeInBothLocales()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        Assert.Contains("# Demo Cookbook", english, StringComparison.Ordinal);
        Assert.Contains("# Demo Cookbook", chinese, StringComparison.Ordinal);
        Assert.Contains("code-plus-demo index", english, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("代码 + 演示", chinese, StringComparison.Ordinal);

        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            AssertRecipeIndexed(english, recipe);
            AssertRecipeIndexed(chinese, recipe);
        }
    }

    [Fact]
    public void DemoCookbookDocs_AreLinkedFromEntryDocs()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var demoGuide = ReadRepoFile("docs/en/demo-guide.md");
        var demoGuideZh = ReadRepoFile("docs/zh-CN/demo-guide.md");

        Assert.Contains("[Demo Cookbook](./demo-cookbook.md)", quickStart, StringComparison.Ordinal);
        Assert.Contains("[Demo Cookbook](./demo-cookbook.md)", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("[Demo Cookbook](./demo-cookbook.md)", demoGuide, StringComparison.Ordinal);
        Assert.Contains("[Demo Cookbook](./demo-cookbook.md)", demoGuideZh, StringComparison.Ordinal);
    }

    [Fact]
    public void DemoCookbookDocs_KeepSupportBoundaryExplicit()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("runtime marketplace", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sandbox", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("workflow execution engine", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("WPF parity", contents, StringComparison.Ordinal);
            Assert.Contains("GA support claim", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DemoCookbookDocs_AnchorScenarioDepthProof()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("DEMO_COOKBOOK_SCENARIO_DEPTH_OK", contents, StringComparison.Ordinal);
            Assert.Contains("sample/proof surface", contents, StringComparison.Ordinal);

            foreach (var scenarioKind in DemoCookbookCatalog.RequiredScenarioKinds)
            {
                Assert.Contains(scenarioKind.ToString(), contents, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void DemoCookbookDocs_DefineRoutePostureAndSourceBackedRoutes()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("DEMO_COOKBOOK_ROUTE_COVERAGE_OK", contents, StringComparison.Ordinal);
            Assert.Contains("Supported SDK route", contents, StringComparison.Ordinal);
            Assert.Contains("Proof/demo route", contents, StringComparison.Ordinal);
            Assert.Contains("Hosted UI route", contents, StringComparison.Ordinal);
            Assert.Contains("Runtime-only route", contents, StringComparison.Ordinal);
            Assert.Contains("Plugin route", contents, StringComparison.Ordinal);
            Assert.Contains("Migration route", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.CreateSession", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphAvaloniaViewFactory.Create", contents, StringComparison.Ordinal);
            Assert.Contains("PluginTrustPolicy", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void HostRecipeDocs_ClarifyDefendedRouteAndConsumerSampleMarkers()
    {
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostRecipeLadder = ReadRepoFile("docs/en/host-recipe-ladder.md");

        Assert.Contains("not another canonical route", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_TRUST_OK:True", hostRecipeLadder, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK:True", hostRecipeLadder, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True", hostRecipeLadder, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True", hostRecipeLadder, StringComparison.Ordinal);
    }

    private static void AssertRecipeIndexed(string contents, DemoCookbookRecipe recipe)
    {
        Assert.Contains(recipe.Id, contents, StringComparison.Ordinal);
        Assert.Contains(recipe.Title, contents, StringComparison.Ordinal);
        Assert.Contains(recipe.Category.ToString(), contents, StringComparison.Ordinal);
        Assert.Contains(recipe.SupportBoundary, contents, StringComparison.Ordinal);
        Assert.Contains(recipe.RouteClarity.SupportedRoute, contents, StringComparison.Ordinal);
        Assert.Contains(recipe.RouteClarity.PackageBoundary, contents, StringComparison.Ordinal);
        Assert.Contains(recipe.RouteClarity.DemoBoundary, contents, StringComparison.Ordinal);

        foreach (var anchor in recipe.CodeAnchors.Concat(recipe.DemoAnchors).Concat(recipe.DocumentationAnchors))
        {
            Assert.Contains(anchor.Path, contents, StringComparison.Ordinal);
            Assert.Contains(anchor.Evidence, contents, StringComparison.Ordinal);
        }

        foreach (var proofMarker in recipe.ProofMarkers)
        {
            Assert.Contains(proofMarker, contents, StringComparison.Ordinal);
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
