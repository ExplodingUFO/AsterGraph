using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Avalonia.Presentation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AsterGraph.Demo.Cookbook;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using SkiaSharp;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookScreenshotGateTests
{
    private const string ManifestRelativePath = "tests/AsterGraph.Demo.Tests/CookbookScreenshotGateRoutes.json";
    private const string ShellStateManifestRelativePath = "tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json";
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
        var routes = LoadRoutes(repoRoot);
        var shellStates = LoadShellStates(repoRoot);
        Assert.NotEmpty(shellStates);

        foreach (var shellState in shellStates)
        {
            AssertShellStateReferencesRoute(shellState, routes);
            var route = routes.Single(candidate => string.Equals(candidate.Id, shellState.RouteId, StringComparison.Ordinal));
            AssertRouteReferencesCatalog(route);

            var outputDirectory = Path.Combine(repoRoot, ShellOutputRootRelativePath, shellState.Id);
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }

            Directory.CreateDirectory(outputDirectory);
            CaptureShellVisualState(repoRoot, route, shellState, outputDirectory);
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
    public void CookbookScreenshotGate_IncludesSelectedRuntimeClosedShellState()
    {
        var shellStates = LoadShellStates(GetRepositoryRoot());
        var shellState = Assert.Single(shellStates, state =>
            string.Equals(state.Id, "shell-runtime-diagnostics-closed", StringComparison.Ordinal));

        Assert.Equal("cookbook-default-starter-host-ai-pipeline", shellState.RouteId);
        Assert.Equal("runtime", shellState.HostGroup);
        Assert.Equal("en", shellState.Language);
        Assert.Equal("canonical-dark", shellState.Theme);
        Assert.False(shellState.ExpectedPaneOpen);
        Assert.Equal(
            [
                "PART_HostMenu",
                "PART_HostShellSplitView",
                "PART_MainGraphEditorHost",
            ],
            shellState.RequiredShellParts);
        Assert.Equal("shell-runtime-diagnostics-closed.png", shellState.OutputFileName);
        Assert.Equal(8, shellStates.Count);
    }

    [Fact]
    public void CookbookScreenshotGate_IncludesPhase508HostMenuFlyoutShellState()
    {
        var shellStates = LoadShellStates(GetRepositoryRoot());
        var shellState = Assert.Single(shellStates, state =>
            string.Equals(state.Id, "shell-cookbook-default-view-menu-flyout", StringComparison.Ordinal));

        Assert.Equal("cookbook-default-starter-host-ai-pipeline", shellState.RouteId);
        Assert.Equal("cookbook", shellState.HostGroup);
        Assert.Equal("en", shellState.Language);
        Assert.Equal("canonical-dark", shellState.Theme);
        Assert.True(shellState.ExpectedPaneOpen);
        Assert.Equal("PART_ViewMenu", shellState.FlyoutMenuPartName);
        Assert.Equal(
            [
                "Show header",
                "Show library",
                "Show inspector",
                "Show status bar",
                "Show mini map",
                "Open view controls",
            ],
            shellState.RequiredFlyoutHeaders);
        Assert.Contains("PART_ViewMenu", shellState.RequiredShellParts, StringComparer.Ordinal);
        Assert.Equal("shell-cookbook-default-view-menu-flyout.png", shellState.OutputFileName);
    }

    [Fact]
    public void CookbookScreenshotGate_IncludesPhase509HostCommandTooltipPopupShellState()
    {
        var shellStates = LoadShellStates(GetRepositoryRoot());
        var shellState = Assert.Single(shellStates, state =>
            string.Equals(state.Id, "shell-cookbook-default-host-command-tooltip-popup", StringComparison.Ordinal));

        Assert.Equal("cookbook-default-starter-host-ai-pipeline", shellState.RouteId);
        Assert.Equal("cookbook", shellState.HostGroup);
        Assert.Equal("en", shellState.Language);
        Assert.Equal("canonical-dark", shellState.Theme);
        Assert.True(shellState.ExpectedPaneOpen);
        Assert.Equal("PART_HostCommand_history.undo", shellState.PopupTargetPartName);
        Assert.Equal(
            [
                "Nothing to undo yet.",
            ],
            shellState.RequiredPopupText);
        Assert.Contains("PART_CommandRail", shellState.RequiredShellParts, StringComparer.Ordinal);
        Assert.Equal("shell-cookbook-default-host-command-tooltip-popup.png", shellState.OutputFileName);
    }

    [Fact]
    public void CookbookScreenshotGate_IncludesPhase510CanvasContextMenuShellState()
    {
        var shellStates = LoadShellStates(GetRepositoryRoot());
        var shellState = Assert.Single(shellStates, state =>
            string.Equals(state.Id, "shell-cookbook-default-canvas-context-menu", StringComparison.Ordinal));

        Assert.Equal("cookbook-default-starter-host-ai-pipeline", shellState.RouteId);
        Assert.Equal("cookbook", shellState.HostGroup);
        Assert.Equal("en", shellState.Language);
        Assert.Equal("canonical-dark", shellState.Theme);
        Assert.True(shellState.ExpectedPaneOpen);
        Assert.Equal("PART_NodeCanvas", shellState.ContextMenuTargetPartName);
        Assert.Equal(
            [
                "Add Node",
                "Fit View",
                "Reset View",
            ],
            shellState.RequiredContextMenuHeaders);
        Assert.Contains("PART_NodeCanvas", shellState.RequiredShellParts, StringComparer.Ordinal);
        Assert.Equal("shell-cookbook-default-canvas-context-menu.png", shellState.OutputFileName);
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
            Assert.Contains("CookbookShellVisualGateStates.json", contents, StringComparison.Ordinal);
            Assert.Contains(OutputRootRelativePath, contents, StringComparison.Ordinal);
            Assert.Contains(ShellOutputRootRelativePath, contents, StringComparison.Ordinal);
            Assert.Contains("full-window", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("CI", contents, StringComparison.Ordinal);
            Assert.Contains("before/after", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("shell-runtime-diagnostics-open", contents, StringComparison.Ordinal);
            Assert.Contains("shell-runtime-diagnostics-closed", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-open-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-closed", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-view-menu-flyout", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-flyout-state", contents, StringComparison.Ordinal);
            Assert.Contains("PART_ViewMenu", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-host-command-tooltip-popup", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-popup-state", contents, StringComparison.Ordinal);
            Assert.Contains("PART_HostCommand_history.undo", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-canvas-context-menu", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-context-menu-state", contents, StringComparison.Ordinal);
            Assert.Contains("PART_NodeCanvas", contents, StringComparison.Ordinal);
            Assert.Contains("language/theme", contents, StringComparison.OrdinalIgnoreCase);
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

    private static void AssertShellStateReferencesRoute(
        CookbookShellVisualGateState shellState,
        IReadOnlyList<CookbookScreenshotGateRoute> routes)
    {
        Assert.False(string.IsNullOrWhiteSpace(shellState.Id));
        Assert.Contains(routes, route => string.Equals(route.Id, shellState.RouteId, StringComparison.Ordinal));
        Assert.Contains(shellState.HostGroup, new[] { "cookbook", "runtime" });
        Assert.Contains(shellState.Language, new[] { "en", "zh-CN" });
        Assert.False(string.IsNullOrWhiteSpace(shellState.Theme));
        Assert.NotEmpty(shellState.RequiredShellParts);
        Assert.All(shellState.RequiredShellParts, part => Assert.StartsWith("PART_", part, StringComparison.Ordinal));
        Assert.StartsWith("shell-", shellState.OutputFileName, StringComparison.Ordinal);
        Assert.EndsWith(".png", shellState.OutputFileName, StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(shellState.FlyoutMenuPartName))
        {
            Assert.StartsWith("PART_", shellState.FlyoutMenuPartName, StringComparison.Ordinal);
            Assert.NotEmpty(shellState.RequiredFlyoutHeaders);
            Assert.All(shellState.RequiredFlyoutHeaders, header => Assert.False(string.IsNullOrWhiteSpace(header)));
        }

        if (!string.IsNullOrWhiteSpace(shellState.PopupTargetPartName))
        {
            Assert.StartsWith("PART_", shellState.PopupTargetPartName, StringComparison.Ordinal);
            Assert.NotEmpty(shellState.RequiredPopupText);
            Assert.All(shellState.RequiredPopupText, text => Assert.False(string.IsNullOrWhiteSpace(text)));
        }

        if (!string.IsNullOrWhiteSpace(shellState.ContextMenuTargetPartName))
        {
            Assert.StartsWith("PART_", shellState.ContextMenuTargetPartName, StringComparison.Ordinal);
            Assert.NotEmpty(shellState.RequiredContextMenuHeaders);
            Assert.All(shellState.RequiredContextMenuHeaders, header => Assert.False(string.IsNullOrWhiteSpace(header)));
        }
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

    private static IReadOnlyList<CookbookShellVisualGateState> LoadShellStates(string repoRoot)
    {
        var manifestPath = Path.Combine(repoRoot, ShellStateManifestRelativePath);
        Assert.True(File.Exists(manifestPath), $"Missing Cookbook shell visual state manifest: {ShellStateManifestRelativePath}");

        var states = JsonSerializer.Deserialize<CookbookShellVisualGateState[]>(
            File.ReadAllText(manifestPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return states ?? [];
    }

    private static void CaptureShellVisualState(
        string repoRoot,
        CookbookScreenshotGateRoute route,
        CookbookShellVisualGateState shellState,
        string outputDirectory)
    {
        var imagePath = Path.Combine(outputDirectory, shellState.OutputFileName);
        var metadataPath = Path.Combine(outputDirectory, "metadata.json");
        var storageRootPath = Path.Combine(
            Path.GetTempPath(),
            "AsterGraph.Demo.Tests",
            nameof(DemoCookbookScreenshotGateTests),
            "shell",
            shellState.Id,
            Guid.NewGuid().ToString("N"));
        var viewModel = new MainWindowViewModel(new MainWindowShellOptions(
            StorageRootPath: storageRootPath,
            EnableStatePersistence: false,
            RestoreLastWorkspaceOnStartup: false,
            InitialScenario: route.Scenario));

        viewModel.SelectLanguage(shellState.Language);
        viewModel.SelectedCookbookRecipe = viewModel.CookbookRecipes.Single(recipe =>
            string.Equals(recipe.Id, route.RecipeId, StringComparison.Ordinal));
        viewModel.Session.Commands.UpdateViewportSize(route.ViewportWidth, route.ViewportHeight);
        viewModel.Session.Commands.FitToViewport(updateStatus: false);
        viewModel.PreferredWindowWidth = route.ViewportWidth;
        viewModel.PreferredWindowHeight = route.ViewportHeight;
        viewModel.OpenHostMenuGroup(shellState.HostGroup);
        if (!shellState.ExpectedPaneOpen)
        {
            viewModel.CloseHostPane();
        }

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

            var shellSplitView = Assert.IsType<SplitView>(window.FindControl<SplitView>("PART_HostShellSplitView"));
            Assert.Equal(shellState.ExpectedPaneOpen, shellSplitView.IsPaneOpen);
            foreach (var partName in shellState.RequiredShellParts)
            {
                var control = Assert.IsAssignableFrom<Control>(FindShellControl(window, partName));
                Assert.True(control.IsVisible, $"{shellState.Id} expected visible shell part: {partName}");
            }

            var openedFlyout = OpenRequestedFlyout(window, shellState);
            var openedPopup = OpenRequestedPopup(window, shellState);
            var openedContextMenu = OpenRequestedContextMenu(window, shellState);

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
                shellState.Id,
                route.Id,
                route.RecipeId,
                route.Scenario,
                shellState.HostGroup,
                shellState.Language,
                shellState.Theme,
                route.ViewportWidth,
                route.ViewportHeight,
                imageSize.Width,
                imageSize.Height,
                "headless-avalonia-window",
                ResolveShellCaptureScope(openedFlyout, openedPopup, openedContextMenu),
                ToRepoRelativePath(repoRoot, imagePath),
                ShellStateManifestRelativePath,
                viewModel.SelectedHostMenuGroupTitle,
                viewModel.SelectedCookbookRecipe.Title,
                shellSplitView.IsPaneOpen,
                shellState.RequiredShellParts,
                openedFlyout?.Name,
                openedFlyout?.IsSubMenuOpen ?? false,
                shellState.RequiredFlyoutHeaders,
                openedPopup?.Name,
                openedPopup is not null && ToolTip.GetIsOpen(openedPopup),
                shellState.RequiredPopupText,
                openedContextMenu?.TargetPartName,
                openedContextMenu?.IsOpen ?? false,
                openedContextMenu?.CoveredHeaders ?? [],
                pixelInspection.NonTransparentPixelCount,
                pixelInspection.DistinctColorCount,
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

            File.WriteAllText(
                metadataPath,
                JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

            var metadataJson = File.ReadAllText(metadataPath);
            Assert.Contains(shellState.Id, metadataJson, StringComparison.Ordinal);
            Assert.Contains(route.Id, metadataJson, StringComparison.Ordinal);
            Assert.Contains(route.RecipeId, metadataJson, StringComparison.Ordinal);
            Assert.Contains(shellState.Language, metadataJson, StringComparison.Ordinal);
            Assert.Contains(shellState.Theme, metadataJson, StringComparison.Ordinal);
            Assert.Contains(ResolveShellCaptureScope(openedFlyout, openedPopup, openedContextMenu), metadataJson, StringComparison.Ordinal);
            Assert.Contains(ToRepoRelativePath(repoRoot, imagePath), metadataJson, StringComparison.Ordinal);
            Assert.Contains(ShellStateManifestRelativePath, metadataJson, StringComparison.Ordinal);
            foreach (var partName in shellState.RequiredShellParts)
            {
                Assert.Contains(partName, metadataJson, StringComparison.Ordinal);
            }

            if (!string.IsNullOrWhiteSpace(shellState.FlyoutMenuPartName))
            {
                Assert.Contains("full-window-shell-flyout-state", metadataJson, StringComparison.Ordinal);
                Assert.Contains(shellState.FlyoutMenuPartName, metadataJson, StringComparison.Ordinal);
                Assert.Contains("\"IsFlyoutOpen\": true", metadataJson, StringComparison.Ordinal);
                Assert.All(shellState.RequiredFlyoutHeaders, header => Assert.Contains(header, metadataJson, StringComparison.Ordinal));
            }

            if (!string.IsNullOrWhiteSpace(shellState.PopupTargetPartName))
            {
                Assert.Contains("full-window-shell-popup-state", metadataJson, StringComparison.Ordinal);
                Assert.Contains(shellState.PopupTargetPartName, metadataJson, StringComparison.Ordinal);
                Assert.Contains("\"IsPopupOpen\": true", metadataJson, StringComparison.Ordinal);
                Assert.All(shellState.RequiredPopupText, text => Assert.Contains(text, metadataJson, StringComparison.Ordinal));
            }

            if (!string.IsNullOrWhiteSpace(shellState.ContextMenuTargetPartName))
            {
                Assert.Contains("full-window-shell-context-menu-state", metadataJson, StringComparison.Ordinal);
                Assert.Contains(shellState.ContextMenuTargetPartName, metadataJson, StringComparison.Ordinal);
                Assert.Contains("\"IsContextMenuOpen\": true", metadataJson, StringComparison.Ordinal);
                Assert.All(shellState.RequiredContextMenuHeaders, header => Assert.Contains(header, metadataJson, StringComparison.Ordinal));
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static string ResolveShellCaptureScope(
        MenuItem? openedFlyout,
        Control? openedPopup,
        OpenContextMenuResult? openedContextMenu)
    {
        if (openedPopup is not null)
        {
            return "full-window-shell-popup-state";
        }

        if (openedContextMenu is not null)
        {
            return "full-window-shell-context-menu-state";
        }

        return openedFlyout is null
            ? "full-window-shell-state"
            : "full-window-shell-flyout-state";
    }

    private static MenuItem? OpenRequestedFlyout(MainWindow window, CookbookShellVisualGateState shellState)
    {
        if (string.IsNullOrWhiteSpace(shellState.FlyoutMenuPartName))
        {
            return null;
        }

        var menuItem = Assert.IsType<MenuItem>(window.FindControl<MenuItem>(shellState.FlyoutMenuPartName));
        Assert.True(menuItem.HasSubMenu, $"{shellState.Id} expected flyout-capable menu: {shellState.FlyoutMenuPartName}");
        var actualHeaders = menuItem.Items
            .OfType<MenuItem>()
            .Select(item => item.Header?.ToString() ?? string.Empty)
            .ToArray();

        foreach (var requiredHeader in shellState.RequiredFlyoutHeaders)
        {
            Assert.Contains(requiredHeader, actualHeaders, StringComparer.Ordinal);
        }

        menuItem.Open();
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.True(menuItem.IsSubMenuOpen, $"{shellState.Id} expected open flyout: {shellState.FlyoutMenuPartName}");
        return menuItem;
    }

    private static Control? OpenRequestedPopup(MainWindow window, CookbookShellVisualGateState shellState)
    {
        if (string.IsNullOrWhiteSpace(shellState.PopupTargetPartName))
        {
            return null;
        }

        var target = Assert.IsAssignableFrom<Control>(FindShellControl(window, shellState.PopupTargetPartName));
        var tip = ToolTip.GetTip(target);
        Assert.NotNull(tip);
        var tipText = tip.ToString() ?? string.Empty;
        foreach (var requiredText in shellState.RequiredPopupText)
        {
            Assert.Contains(requiredText, tipText, StringComparison.Ordinal);
        }

        ToolTip.SetIsOpen(target, true);
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.True(ToolTip.GetIsOpen(target), $"{shellState.Id} expected open tooltip popup: {shellState.PopupTargetPartName}");
        return target;
    }

    private static OpenContextMenuResult? OpenRequestedContextMenu(MainWindow window, CookbookShellVisualGateState shellState)
    {
        if (string.IsNullOrWhiteSpace(shellState.ContextMenuTargetPartName))
        {
            return null;
        }

        var target = Assert.IsType<NodeCanvas>(FindShellControl(window, shellState.ContextMenuTargetPartName));
        var presenter = new RecordingContextMenuPresenter();
        target.ContextMenuPresenter = presenter;

        var args = new ContextRequestedEventArgs
        {
            RoutedEvent = Control.ContextRequestedEvent,
        };
        target.RaiseEvent(args);
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.True(args.Handled, $"{shellState.Id} expected handled context menu request: {shellState.ContextMenuTargetPartName}");
        Assert.True(presenter.IsOpen, $"{shellState.Id} expected open context menu: {shellState.ContextMenuTargetPartName}");
        foreach (var requiredHeader in shellState.RequiredContextMenuHeaders)
        {
            Assert.Contains(requiredHeader, presenter.CoveredHeaders, StringComparer.Ordinal);
        }

        return new OpenContextMenuResult(
            shellState.ContextMenuTargetPartName,
            presenter.IsOpen,
            presenter.CoveredHeaders);
    }

    private static Control? FindShellControl(MainWindow window, string partName)
        => window.FindControl<Control>(partName)
           ?? window.GetVisualDescendants()
               .OfType<Control>()
               .SingleOrDefault(control => string.Equals(control.Name, partName, StringComparison.Ordinal));

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

    private sealed record CookbookShellVisualGateState(
        string Id,
        string RouteId,
        string HostGroup,
        string Language,
        string Theme,
        bool ExpectedPaneOpen,
        string? FlyoutMenuPartName,
        string[] RequiredFlyoutHeaders,
        string? PopupTargetPartName,
        string[] RequiredPopupText,
        string? ContextMenuTargetPartName,
        string[] RequiredContextMenuHeaders,
        string[] RequiredShellParts,
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
        string RouteId,
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
        string? FlyoutMenuPartName,
        bool IsFlyoutOpen,
        string[] CoveredFlyoutHeaders,
        string? PopupTargetPartName,
        bool IsPopupOpen,
        string[] CoveredPopupText,
        string? ContextMenuTargetPartName,
        bool IsContextMenuOpen,
        string[] CoveredContextMenuHeaders,
        int NonTransparentPixelCount,
        int DistinctColorCount,
        string PngSha256);

    private sealed record PngSize(int Width, int Height);

    private sealed record PngPixelInspection(int NonTransparentPixelCount, int DistinctColorCount);

    private sealed record OpenContextMenuResult(
        string TargetPartName,
        bool IsOpen,
        string[] CoveredHeaders);

    private sealed class RecordingContextMenuPresenter : IGraphContextMenuPresenter
    {
        private readonly GraphContextMenuPresenter _inner = new();

        public bool IsOpen { get; private set; }

        public string[] CoveredHeaders { get; private set; } = [];

        public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
        {
            CoveredHeaders = descriptors
                .Where(descriptor => !descriptor.IsSeparator)
                .Select(descriptor => descriptor.Header)
                .ToArray();
            _inner.Open(target, descriptors, style);
            IsOpen = true;
        }

        public void Open(
            Control target,
            IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> descriptors,
            IGraphEditorCommands commands,
            ContextMenuStyleOptions style)
        {
            CoveredHeaders = descriptors
                .Where(descriptor => !descriptor.IsSeparator)
                .Select(descriptor => descriptor.Header)
                .ToArray();
            _inner.Open(target, descriptors, commands, style);
            IsOpen = true;
        }
    }
}
