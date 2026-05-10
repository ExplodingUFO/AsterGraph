using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using AsterGraph.Demo.Cookbook;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Editor.Services;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookScreenshotGateTests
{
    private const string ManifestRelativePath = "tests/AsterGraph.Demo.Tests/CookbookScreenshotGateRoutes.json";
    private const string OutputRootRelativePath = "artifacts/test-results/cookbook-screenshot-gate";

    [Fact]
    public void CookbookScreenshotGate_CapturesManifestRoutesAndWritesMetadata()
    {
        var repoRoot = GetRepositoryRoot();
        var routes = LoadRoutes(repoRoot);

        Assert.NotEmpty(routes);

        foreach (var route in routes)
        {
            AssertRouteReferencesCatalog(route);

            var outputDirectory = Path.Combine(repoRoot, OutputRootRelativePath, route.Id);
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }

            Directory.CreateDirectory(outputDirectory);
            var imagePath = Path.Combine(outputDirectory, route.OutputFileName);
            var metadataPath = Path.Combine(outputDirectory, "metadata.json");
            var storageRootPath = Path.Combine(
                Path.GetTempPath(),
                "AsterGraph.Demo.Tests",
                nameof(DemoCookbookScreenshotGateTests),
                Guid.NewGuid().ToString("N"));
            var viewModel = new MainWindowViewModel(new MainWindowShellOptions(
                StorageRootPath: storageRootPath,
                EnableStatePersistence: false,
                RestoreLastWorkspaceOnStartup: false,
                InitialScenario: route.Scenario));

            viewModel.SelectLanguage(route.Language);
            viewModel.SelectedHostMenuGroup = route.HostGroup;
            viewModel.SelectedCookbookRecipe = viewModel.CookbookRecipes.Single(recipe =>
                string.Equals(recipe.Id, route.RecipeId, StringComparison.Ordinal));
            viewModel.Session.Commands.UpdateViewportSize(route.ViewportWidth, route.ViewportHeight);
            viewModel.Session.Commands.FitToViewport(updateStatus: false);

            var scene = viewModel.Session.Queries.GetSceneSnapshot();
            Assert.Equal(route.ExpectedDocumentTitle, scene.Document.Title);
            Assert.True(
                scene.Document.Nodes.Count >= route.MinimumNodeCount,
                $"{route.Id} should capture at least {route.MinimumNodeCount} nodes.");
            Assert.True(
                scene.Document.Connections.Count >= route.MinimumConnectionCount,
                $"{route.Id} should capture at least {route.MinimumConnectionCount} connections.");
            foreach (var requiredNodeId in route.RequiredNodeIds)
            {
                Assert.Contains(scene.Document.Nodes, node => string.Equals(node.Id, requiredNodeId, StringComparison.Ordinal));
            }

            var exportService = new GraphSceneImageExportService(outputDirectory);
            var writtenPath = exportService.Export(
                scene,
                GraphEditorSceneImageExportFormat.Png,
                imagePath,
                new GraphEditorSceneImageExportOptions
                {
                    Scope = GraphEditorSceneImageExportScope.FullScene,
                    Scale = route.Scale,
                    BackgroundHex = route.BackgroundHex,
                });
            var bytes = File.ReadAllBytes(writtenPath);
            var metadata = new CookbookScreenshotGateMetadata(
                route.Id,
                route.RecipeId,
                route.Scenario,
                route.HostGroup,
                route.Language,
                route.Theme,
                route.ViewportWidth,
                route.ViewportHeight,
                scene.Viewport.Zoom,
                scene.Viewport.PanX,
                scene.Viewport.PanY,
                route.BackgroundHex,
                route.Scale,
                ToRepoRelativePath(repoRoot, writtenPath),
                ManifestRelativePath,
                scene.Document.Title,
                scene.Document.Nodes.Count,
                scene.Document.Connections.Count,
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

            File.WriteAllText(
                metadataPath,
                JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

            Assert.Equal(imagePath, writtenPath);
            Assert.True(File.Exists(writtenPath));
            Assert.True(bytes.Length > route.MinimumBytes);
            Assert.Equal(0x89, bytes[0]);
            Assert.Equal(0x50, bytes[1]);
            Assert.Equal(0x4E, bytes[2]);
            Assert.Equal(0x47, bytes[3]);

            var metadataJson = File.ReadAllText(metadataPath);
            Assert.Contains(route.Id, metadataJson, StringComparison.Ordinal);
            Assert.Contains(route.RecipeId, metadataJson, StringComparison.Ordinal);
            Assert.Contains(route.Theme, metadataJson, StringComparison.Ordinal);
            Assert.Contains(route.ViewportWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), metadataJson, StringComparison.Ordinal);
            Assert.Contains(route.ViewportHeight.ToString(System.Globalization.CultureInfo.InvariantCulture), metadataJson, StringComparison.Ordinal);
            Assert.Contains(ToRepoRelativePath(repoRoot, writtenPath), metadataJson, StringComparison.Ordinal);
            Assert.Contains(ManifestRelativePath, metadataJson, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CookbookScreenshotGate_IncludesBuiltInBatchRoutes()
    {
        var routes = LoadRoutes(GetRepositoryRoot());

        Assert.Contains(routes, route =>
            route.Id == "cookbook-builtin-minimap-workbench"
            && route.RecipeId == "builtin-minimap-workbench-route"
            && route.Scenario == "minimap-workbench"
            && route.ExpectedDocumentTitle == "MiniMap Workbench Surface"
            && route.RequiredNodeIds.Contains("minimap-output", StringComparer.Ordinal));
        Assert.Contains(routes, route =>
            route.Id == "cookbook-builtin-background-grid"
            && route.RecipeId == "builtin-background-grid-route"
            && route.Scenario == "background-grid-density"
            && route.ExpectedDocumentTitle == "Background Grid Density"
            && route.RequiredNodeIds.Contains("grid-output", StringComparer.Ordinal));
        Assert.Contains(routes, route =>
            route.Id == "cookbook-builtin-hosted-controls"
            && route.RecipeId == "builtin-hosted-controls-route"
            && route.Scenario == "hosted-controls-panel"
            && route.ExpectedDocumentTitle == "Hosted Controls Panel Composition"
            && route.RequiredNodeIds.Contains("panel-output", StringComparer.Ordinal));
    }

    [Fact]
    public void CookbookScreenshotGate_IncludesInteractionFixtureBatchRoutes()
    {
        var routes = LoadRoutes(GetRepositoryRoot());

        Assert.Contains(routes, route =>
            route.Id == "cookbook-interaction-selection-marquee"
            && route.RecipeId == "interaction-selection-marquee-route"
            && route.Scenario == "selection-marquee-workbench"
            && route.ExpectedDocumentTitle == "Selection Rectangle Fixture"
            && route.RequiredNodeIds.Contains("select-output", StringComparer.Ordinal));
        Assert.Contains(routes, route =>
            route.Id == "cookbook-interaction-keyboard-navigation"
            && route.RecipeId == "interaction-keyboard-navigation-route"
            && route.Scenario == "keyboard-navigation-lab"
            && route.ExpectedDocumentTitle == "Keyboard Navigation Fixture"
            && route.RequiredNodeIds.Contains("key-output", StringComparer.Ordinal));
        Assert.Contains(routes, route =>
            route.Id == "cookbook-interaction-host-event-inspector"
            && route.RecipeId == "interaction-host-event-inspector-route"
            && route.Scenario == "host-event-inspector"
            && route.ExpectedDocumentTitle == "Host Event Inspector Fixture"
            && route.RequiredNodeIds.Contains("event-output", StringComparer.Ordinal));
    }

    [Fact]
    public void CookbookScreenshotGate_DocumentationNamesCommandArtifactsAndCiPosture()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("DemoCookbookScreenshotGateTests", contents, StringComparison.Ordinal);
            Assert.Contains("CookbookScreenshotGateRoutes.json", contents, StringComparison.Ordinal);
            Assert.Contains(OutputRootRelativePath, contents, StringComparison.Ordinal);
            Assert.Contains("CI", contents, StringComparison.Ordinal);
            Assert.Contains("before/after", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertRouteReferencesCatalog(CookbookScreenshotGateRoute route)
    {
        Assert.False(string.IsNullOrWhiteSpace(route.Id));
        Assert.False(string.IsNullOrWhiteSpace(route.RecipeId));
        Assert.Contains(DemoCookbookCatalog.Recipes, recipe =>
            string.Equals(recipe.Id, route.RecipeId, StringComparison.Ordinal));
        Assert.Contains(route.HostGroup, new[] { "cookbook" });
        Assert.Contains(route.Language, new[] { "en", "zh-CN" });
        Assert.StartsWith("cookbook-", route.OutputFileName, StringComparison.Ordinal);
        Assert.EndsWith(".png", route.OutputFileName, StringComparison.OrdinalIgnoreCase);
        Assert.True(route.ViewportWidth >= 960);
        Assert.True(route.ViewportHeight >= 640);
        Assert.True(route.Scale > 0d);
        Assert.True(route.MinimumBytes >= 1024);
        Assert.False(string.IsNullOrWhiteSpace(route.ExpectedDocumentTitle));
        Assert.True(route.MinimumNodeCount > 0);
        Assert.True(route.MinimumConnectionCount > 0);
        Assert.NotEmpty(route.RequiredNodeIds);
    }

    private static IReadOnlyList<CookbookScreenshotGateRoute> LoadRoutes(string repoRoot)
    {
        var manifestPath = Path.Combine(repoRoot, ManifestRelativePath);
        Assert.True(File.Exists(manifestPath), $"Missing Cookbook screenshot gate manifest: {ManifestRelativePath}");

        var routes = JsonSerializer.Deserialize<CookbookScreenshotGateRoute[]>(
            File.ReadAllText(manifestPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return routes ?? [];
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string ToRepoRelativePath(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace('\\', '/');

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

    private sealed record CookbookScreenshotGateRoute(
        string Id,
        string RecipeId,
        string Scenario,
        string HostGroup,
        string Language,
        string Theme,
        int ViewportWidth,
        int ViewportHeight,
        string BackgroundHex,
        double Scale,
        int MinimumBytes,
        string ExpectedDocumentTitle,
        int MinimumNodeCount,
        int MinimumConnectionCount,
        string[] RequiredNodeIds,
        string OutputFileName);

    private sealed record CookbookScreenshotGateMetadata(
        string Id,
        string RecipeId,
        string Scenario,
        string HostGroup,
        string Language,
        string Theme,
        int ViewportWidth,
        int ViewportHeight,
        double ActualViewportZoom,
        double ActualViewportPanX,
        double ActualViewportPanY,
        string BackgroundHex,
        double Scale,
        string OutputPath,
        string BaselineSource,
        string DocumentTitle,
        int NodeCount,
        int ConnectionCount,
        string PngSha256);
}
