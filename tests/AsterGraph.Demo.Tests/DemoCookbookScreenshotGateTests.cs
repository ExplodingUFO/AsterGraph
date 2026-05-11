using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AsterGraph.Demo.Cookbook;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using AsterGraph.Editor.Services;
using SkiaSharp;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookScreenshotGateTests
{
    private const string ManifestRelativePath = "tests/AsterGraph.Demo.Tests/CookbookScreenshotGateRoutes.json";
    private const string OutputRootRelativePath = "artifacts/test-results/cookbook-screenshot-gate";
    private const string ShellOutputRootRelativePath = "artifacts/test-results/cookbook-shell-visual-gate";
    private const int ShellMinimumBytes = 8192;

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
            AssertPngArtifact(bytes, route.MinimumBytes);

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

    [AvaloniaFact]
    public void CookbookScreenshotGate_CapturesFullWindowShellAndWritesMetadata()
    {
        var repoRoot = GetRepositoryRoot();
        var route = LoadRoutes(repoRoot).Single(candidate =>
            string.Equals(candidate.Id, "cookbook-default-starter-host-ai-pipeline", StringComparison.Ordinal));
        AssertRouteReferencesCatalog(route);

        var outputDirectory = Path.Combine(repoRoot, ShellOutputRootRelativePath, route.Id);
        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, recursive: true);
        }

        Directory.CreateDirectory(outputDirectory);
        var imagePath = Path.Combine(
            outputDirectory,
            Path.GetFileNameWithoutExtension(route.OutputFileName) + "-shell.png");
        var metadataPath = Path.Combine(outputDirectory, "metadata.json");
        var storageRootPath = Path.Combine(
            Path.GetTempPath(),
            "AsterGraph.Demo.Tests",
            nameof(DemoCookbookScreenshotGateTests),
            "shell",
            Guid.NewGuid().ToString("N"));
        var viewModel = new MainWindowViewModel(new MainWindowShellOptions(
            StorageRootPath: storageRootPath,
            EnableStatePersistence: false,
            RestoreLastWorkspaceOnStartup: false,
            InitialScenario: route.Scenario));

        viewModel.SelectLanguage(route.Language);
        viewModel.SelectedCookbookRecipe = viewModel.CookbookRecipes.Single(recipe =>
            string.Equals(recipe.Id, route.RecipeId, StringComparison.Ordinal));
        viewModel.Session.Commands.UpdateViewportSize(route.ViewportWidth, route.ViewportHeight);
        viewModel.Session.Commands.FitToViewport(updateStatus: false);
        viewModel.PreferredWindowWidth = route.ViewportWidth;
        viewModel.PreferredWindowHeight = route.ViewportHeight;
        viewModel.OpenHostMenuGroup(route.HostGroup);

        var window = new MainWindow
        {
            Width = route.ViewportWidth,
            Height = route.ViewportHeight,
            DataContext = viewModel,
        };

        try
        {
            window.Show();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

            var hostMenu = Assert.IsType<Menu>(window.FindControl<Menu>("PART_HostMenu"));
            var shellSplitView = Assert.IsType<SplitView>(window.FindControl<SplitView>("PART_HostShellSplitView"));
            var navigationPanel = Assert.IsType<Border>(window.FindControl<Border>("PART_CookbookWorkspaceNavigationPanel"));
            var graphHost = Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_MainGraphEditorHost"));
            var recipeContentPanel = Assert.IsType<Border>(window.FindControl<Border>("PART_CookbookWorkspaceRecipeContentPanel"));

            Assert.NotNull(hostMenu);
            Assert.True(shellSplitView.IsPaneOpen);
            Assert.True(navigationPanel.IsVisible);
            Assert.NotNull(graphHost.Content);
            Assert.True(recipeContentPanel.IsVisible);

            using var frame = window.CaptureRenderedFrame();
            Assert.NotNull(frame);
            frame!.Save(imagePath);

            var bytes = File.ReadAllBytes(imagePath);
            var imageSize = AssertPngArtifact(bytes, ShellMinimumBytes);
            Assert.True(imageSize.Width >= route.ViewportWidth);
            Assert.True(imageSize.Height >= route.ViewportHeight);
            var pixelInspection = InspectNonBlankPng(imagePath);
            Assert.True(pixelInspection.NonTransparentPixelCount > imageSize.Width * imageSize.Height / 4);
            Assert.True(pixelInspection.DistinctColorCount > 1);

            var metadata = new CookbookShellVisualGateMetadata(
                route.Id,
                route.RecipeId,
                route.Scenario,
                route.HostGroup,
                route.Language,
                route.Theme,
                route.ViewportWidth,
                route.ViewportHeight,
                imageSize.Width,
                imageSize.Height,
                "headless-avalonia-window",
                "full-window-shell",
                ToRepoRelativePath(repoRoot, imagePath),
                ManifestRelativePath,
                viewModel.SelectedHostMenuGroupTitle,
                viewModel.SelectedCookbookRecipe.Title,
                shellSplitView.IsPaneOpen,
                [
                    "PART_HostMenu",
                    "PART_HostShellSplitView",
                    "PART_CookbookWorkspaceNavigationPanel",
                    "PART_MainGraphEditorHost",
                    "PART_CookbookWorkspaceRecipeContentPanel",
                ],
                pixelInspection.NonTransparentPixelCount,
                pixelInspection.DistinctColorCount,
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

            File.WriteAllText(
                metadataPath,
                JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

            var metadataJson = File.ReadAllText(metadataPath);
            Assert.Contains(route.Id, metadataJson, StringComparison.Ordinal);
            Assert.Contains(route.RecipeId, metadataJson, StringComparison.Ordinal);
            Assert.Contains("full-window-shell", metadataJson, StringComparison.Ordinal);
            Assert.Contains("PART_HostMenu", metadataJson, StringComparison.Ordinal);
            Assert.Contains("PART_HostShellSplitView", metadataJson, StringComparison.Ordinal);
            Assert.Contains("PART_CookbookWorkspaceNavigationPanel", metadataJson, StringComparison.Ordinal);
            Assert.Contains("PART_MainGraphEditorHost", metadataJson, StringComparison.Ordinal);
            Assert.Contains(ToRepoRelativePath(repoRoot, imagePath), metadataJson, StringComparison.Ordinal);
            Assert.Contains(ManifestRelativePath, metadataJson, StringComparison.Ordinal);
        }
        finally
        {
            window.Close();
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
    public void CookbookScreenshotGate_IncludesStandaloneBuiltInBatchRoutes()
    {
        var routes = LoadRoutes(GetRepositoryRoot());

        AssertStandaloneBuiltInRoute(
            routes,
            "cookbook-builtin-standalone-controls",
            "builtin-standalone-controls-route",
            "keyboard-navigation-lab",
            "Keyboard Navigation Fixture",
            "key-output");
        AssertStandaloneBuiltInRoute(
            routes,
            "cookbook-builtin-standalone-panel",
            "builtin-standalone-panel-route",
            "host-event-inspector",
            "Host Event Inspector Fixture",
            "event-output");
        AssertStandaloneBuiltInRoute(
            routes,
            "cookbook-builtin-node-toolbar",
            "builtin-node-toolbar-route",
            "selection-marquee-workbench",
            "Selection Rectangle Fixture",
            "select-output");
        AssertStandaloneBuiltInRoute(
            routes,
            "cookbook-builtin-edge-toolbar",
            "builtin-edge-toolbar-route",
            "clipboard-fragment-roundtrip",
            "Clipboard Fragment Roundtrip Fixture",
            "clip-output");
        AssertStandaloneBuiltInRoute(
            routes,
            "cookbook-builtin-node-resizer",
            "builtin-node-resizer-route",
            "validation-prevent-cycle",
            "Validation Prevent Cycle Fixture",
            "validate-output");
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
    public void CookbookScreenshotGate_IncludesLifecycleFixtureBatchRoutes()
    {
        var routes = LoadRoutes(GetRepositoryRoot());

        Assert.Contains(routes, route =>
            route.Id == "cookbook-lifecycle-workspace-save-restore"
            && route.RecipeId == "lifecycle-workspace-save-restore-route"
            && route.Scenario == "workspace-save-restore"
            && route.ExpectedDocumentTitle == "Workspace Save Restore Fixture"
            && route.RequiredNodeIds.Contains("save-output", StringComparer.Ordinal));
        Assert.Contains(routes, route =>
            route.Id == "cookbook-lifecycle-clipboard-fragment"
            && route.RecipeId == "lifecycle-clipboard-fragment-route"
            && route.Scenario == "clipboard-fragment-roundtrip"
            && route.ExpectedDocumentTitle == "Clipboard Fragment Roundtrip Fixture"
            && route.RequiredNodeIds.Contains("clip-output", StringComparer.Ordinal));
        Assert.Contains(routes, route =>
            route.Id == "cookbook-lifecycle-validation-helper"
            && route.RecipeId == "lifecycle-validation-helper-route"
            && route.Scenario == "validation-prevent-cycle"
            && route.ExpectedDocumentTitle == "Validation Prevent Cycle Fixture"
            && route.RequiredNodeIds.Contains("validate-output", StringComparer.Ordinal));
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
            Assert.Contains(ShellOutputRootRelativePath, contents, StringComparison.Ordinal);
            Assert.Contains("full-window", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("CI", contents, StringComparison.Ordinal);
            Assert.Contains("before/after", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("builtin-standalone-controls-route", contents, StringComparison.Ordinal);
            Assert.Contains("builtin-standalone-panel-route", contents, StringComparison.Ordinal);
            Assert.Contains("builtin-node-toolbar-route", contents, StringComparison.Ordinal);
            Assert.Contains("builtin-edge-toolbar-route", contents, StringComparison.Ordinal);
            Assert.Contains("builtin-node-resizer-route", contents, StringComparison.Ordinal);
        }
    }

    private static void AssertStandaloneBuiltInRoute(
        IReadOnlyList<CookbookScreenshotGateRoute> routes,
        string id,
        string recipeId,
        string scenario,
        string documentTitle,
        string requiredNodeId)
    {
        Assert.Contains(routes, route =>
            route.Id == id
            && route.RecipeId == recipeId
            && route.Scenario == scenario
            && route.ExpectedDocumentTitle == documentTitle
            && route.RequiredNodeIds.Contains(requiredNodeId, StringComparer.Ordinal));
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

    private static PngSize AssertPngArtifact(byte[] bytes, int minimumBytes)
    {
        Assert.True(bytes.Length > minimumBytes);
        Assert.Equal(0x89, bytes[0]);
        Assert.Equal(0x50, bytes[1]);
        Assert.Equal(0x4E, bytes[2]);
        Assert.Equal(0x47, bytes[3]);
        Assert.Equal(0x0D, bytes[4]);
        Assert.Equal(0x0A, bytes[5]);
        Assert.Equal(0x1A, bytes[6]);
        Assert.Equal(0x0A, bytes[7]);

        return new PngSize(ReadBigEndianInt32(bytes, 16), ReadBigEndianInt32(bytes, 20));
    }

    private static PngPixelInspection InspectNonBlankPng(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        Assert.NotNull(bitmap);
        Assert.True(bitmap.Width > 0);
        Assert.True(bitmap.Height > 0);

        var nonTransparentPixelCount = 0;
        var colors = new HashSet<uint>();
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.Alpha == 0)
                {
                    continue;
                }

                nonTransparentPixelCount++;
                if (colors.Count <= 32)
                {
                    colors.Add(
                        ((uint)pixel.Alpha << 24)
                        | ((uint)pixel.Red << 16)
                        | ((uint)pixel.Green << 8)
                        | pixel.Blue);
                }
            }
        }

        return new PngPixelInspection(nonTransparentPixelCount, colors.Count);
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset)
        => (bytes[offset] << 24)
           | (bytes[offset + 1] << 16)
           | (bytes[offset + 2] << 8)
           | bytes[offset + 3];

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

    private sealed record CookbookShellVisualGateMetadata(
        string Id,
        string RecipeId,
        string Scenario,
        string HostGroup,
        string Language,
        string Theme,
        int RequestedWindowWidth,
        int RequestedWindowHeight,
        int ActualPixelWidth,
        int ActualPixelHeight,
        string CaptureAdapter,
        string CaptureScope,
        string OutputPath,
        string BaselineSource,
        string SelectedHostMenuGroupTitle,
        string SelectedCookbookRecipeTitle,
        bool IsHostPaneOpen,
        string[] CoveredShellParts,
        int NonTransparentPixelCount,
        int DistinctColorCount,
        string PngSha256);

    private sealed record PngSize(int Width, int Height);

    private sealed record PngPixelInspection(int NonTransparentPixelCount, int DistinctColorCount);
}
