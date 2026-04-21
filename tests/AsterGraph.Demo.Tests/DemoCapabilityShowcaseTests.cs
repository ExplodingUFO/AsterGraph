using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Avalonia.Automation;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Core.Models;
using AsterGraph.Demo;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Plugins;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCapabilityShowcaseTests
{
    [Fact]
    public void MainWindowViewModel_ExpandsCapabilityShowcaseToCurrentProductSurface()
    {
        var viewModel = new MainWindowViewModel();

        Assert.True(viewModel.Capabilities.Count >= 9);
        Assert.Contains(viewModel.Capabilities, item => item.Key == "semantic-graph-composition");
        Assert.Contains(viewModel.Capabilities, item => item.Key == "plugin-trust-and-loading");
        Assert.Contains(viewModel.Capabilities, item => item.Key == "automation-execution");
        Assert.Contains(viewModel.Capabilities, item => item.Key == "consumer-host-path");
        Assert.Contains(viewModel.Capabilities, item => item.Key == "history-save-contract");
        Assert.Contains(viewModel.Capabilities, item => item.Key == "tiered-surface-layout");
    }

    [Fact]
    public void MainWindowViewModel_ProjectsPluginCandidatesAndLoadSnapshots()
    {
        var viewModel = new MainWindowViewModel();

        Assert.NotEmpty(viewModel.PluginCandidates);
        Assert.NotEmpty(viewModel.PluginLoadSnapshots);
        Assert.Contains(viewModel.PluginCandidates, candidate => candidate.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Allowed);
        Assert.Contains(viewModel.PluginCandidates, candidate => candidate.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Blocked);
        Assert.Contains(viewModel.PluginLoadSnapshots, snapshot => snapshot.Status == GraphEditorPluginLoadStatus.Loaded);
        Assert.Contains(viewModel.PluginLoadSnapshots, snapshot => snapshot.Status == GraphEditorPluginLoadStatus.Blocked);
        Assert.Contains(viewModel.ConsumerPathLines, line => line.Contains("HostSample", StringComparison.Ordinal));
    }

    [Fact]
    public void MainWindowViewModel_ProjectsPluginProvenanceAndTrustTransparency()
    {
        var viewModel = new MainWindowViewModel(new MainWindowShellOptions(
            StorageRootPath: CreateTempDirectory(),
            EnableStatePersistence: true,
            RestoreLastWorkspaceOnStartup: false));

        Assert.NotEmpty(viewModel.PluginCandidateEntries);
        Assert.Contains(viewModel.PluginCandidateEntries, entry => !string.IsNullOrWhiteSpace(entry.Version));
        Assert.Contains(viewModel.PluginCandidateEntries, entry => !string.IsNullOrWhiteSpace(entry.TargetFramework));
        Assert.Contains(viewModel.PluginCandidateEntries, entry => !string.IsNullOrWhiteSpace(entry.TrustFingerprint));
        Assert.Contains(viewModel.PluginCandidateEntries, entry => entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(viewModel.PluginAllowlistLines, line => line.Contains("allowlist", StringComparison.OrdinalIgnoreCase) || line.Contains("Allowlist", StringComparison.Ordinal));
    }

    [Fact]
    public void MainWindowViewModel_PersistsExportsAndImportsPluginAllowlistDecisions()
    {
        var storageRoot = CreateTempDirectory();
        var viewModel = new MainWindowViewModel(new MainWindowShellOptions(
            StorageRootPath: storageRoot,
            EnableStatePersistence: true,
            RestoreLastWorkspaceOnStartup: false));

        Assert.Contains(viewModel.PluginCandidateEntries, entry => entry.PluginId == "aster.demo.plugin.blocked" && entry.IsBlocked);

        Assert.True(viewModel.TrustPluginCandidate("aster.demo.plugin.blocked"));
        Assert.Contains(viewModel.PluginCandidateEntries, entry => entry.PluginId == "aster.demo.plugin.blocked" && entry.IsAllowed);

        var exportPath = Path.Combine(storageRoot, "plugin-allowlist-export.json");
        Assert.True(viewModel.ExportPluginAllowlist(exportPath));
        Assert.True(File.Exists(exportPath));

        Assert.True(viewModel.BlockPluginCandidate("aster.demo.plugin.blocked"));
        Assert.Contains(viewModel.PluginCandidateEntries, entry => entry.PluginId == "aster.demo.plugin.blocked" && entry.IsBlocked);

        Assert.True(viewModel.ImportPluginAllowlist(exportPath));
        Assert.Contains(viewModel.PluginCandidateEntries, entry => entry.PluginId == "aster.demo.plugin.blocked" && entry.IsAllowed);

        var reloaded = new MainWindowViewModel(new MainWindowShellOptions(
            StorageRootPath: storageRoot,
            EnableStatePersistence: true,
            RestoreLastWorkspaceOnStartup: false));
        Assert.Contains(reloaded.PluginCandidateEntries, entry => entry.PluginId == "aster.demo.plugin.blocked" && entry.IsAllowed);
    }

    [Fact]
    public void MainWindowViewModel_RunsAutomationShowcaseAndCapturesTypedResult()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.RunPluginAutomation();

        Assert.NotNull(viewModel.LastAutomationRequest);
        Assert.NotNull(viewModel.LastAutomationResult);
        var request = viewModel.LastAutomationRequest!;
        var result = viewModel.LastAutomationResult!;

        Assert.Equal(request.RunId, result.RunId);
        Assert.True(result.Succeeded);
        Assert.NotEmpty(result.Steps);
        Assert.NotEmpty(viewModel.AutomationProgressSteps);
        Assert.Contains(viewModel.AutomationResultLines, line => line.Contains(request.RunId, StringComparison.Ordinal));
    }

    [Fact]
    public void DemoProof_Run_EmitsMetricsAndGreenMarkers()
    {
        var result = DemoProof.Run(CreateTempDirectory());

        Assert.True(result.IsOk);
        Assert.True(result.TrustTransparencyOk);
        Assert.True(result.ShellWorkflowOk);
        Assert.True(result.CommandSurfaceOk);
        Assert.True(result.TieredNodeSurfaceOk);
        Assert.True(result.FixedGroupFrameOk);
        Assert.True(result.NonObscuringEditingOk);
        Assert.True(result.VisualSemanticsOk);
        Assert.True(result.CompositeScopeOk);
        Assert.True(result.EdgeNoteOk);
        Assert.True(result.DisconnectFlowOk);
        Assert.True(result.StartupMs >= 0);
        Assert.True(result.InspectorProjectionMs >= 0);
        Assert.True(result.PluginScanMs >= 0);
        Assert.True(result.CommandLatencyMs >= 0);
        foreach (var requiredProofLine in DemoProofContract.CreatePublicSuccessMarkerLines())
        {
            Assert.Contains(result.ProofLines, line => string.Equals(line, requiredProofLine, StringComparison.Ordinal));
        }
        foreach (var metricName in DemoProofContract.NativeMetricNames)
        {
            Assert.Contains(result.MetricLines, line => line.Contains(metricName, StringComparison.Ordinal));
        }
    }

    [AvaloniaFact]
    public void MainWindow_RendersCanonicalDemoMenusAndIntegrationHosts()
    {
        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel(),
        };

        window.Show();

        Assert.NotNull(window.FindControl<MenuItem>("PART_ExtensionsMenu"));
        Assert.NotNull(window.FindControl<MenuItem>("PART_AutomationMenu"));
        Assert.NotNull(window.FindControl<MenuItem>("PART_IntegrationMenu"));
        Assert.NotNull(window.FindControl<ContentControl>("PART_MainGraphEditorHost"));
        Assert.NotNull(window.FindControl<ContentControl>("PART_StandaloneCanvasHost"));
        Assert.NotNull(window.FindControl<ContentControl>("PART_StandaloneInspectorHost"));
        Assert.NotNull(window.FindControl<ContentControl>("PART_StandaloneMiniMapHost"));
        Assert.NotNull(window.FindControl<ContentControl>("PART_CustomInspectorHost"));
        Assert.NotNull(window.FindControl<ContentControl>("PART_CustomMiniMapHost"));
    }

    [AvaloniaFact]
    public void MainWindow_CreatesMainEditorAndStandaloneSurfacesFromLiveEditorSession()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var graphEditorView = Assert.IsType<GraphEditorView>(
            Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_MainGraphEditorHost")).Content);
        var canvas = Assert.IsType<NodeCanvas>(
            Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_StandaloneCanvasHost")).Content);
        var inspector = Assert.IsType<GraphInspectorView>(
            Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_StandaloneInspectorHost")).Content);
        var miniMap = Assert.IsType<GraphMiniMap>(
            Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_StandaloneMiniMapHost")).Content);

        Assert.Same(viewModel.Editor, graphEditorView.Editor);
        Assert.Equal("StandaloneCanvasPreview", canvas.Name);
        Assert.Equal("StandaloneInspectorPreview", inspector.Name);
        Assert.Equal("StandaloneMiniMapPreview", miniMap.Name);
    }

    [AvaloniaFact]
    public void MainWindow_RendersFixedGroupAndSizeDrivenTierShowcaseArtifacts()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var graphEditorView = Assert.IsType<GraphEditorView>(
            Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_MainGraphEditorHost")).Content);
        var canvas = graphEditorView.FindControl<NodeCanvas>("PART_NodeCanvas");

        Assert.NotNull(canvas);
        Assert.Contains(viewModel.Editor.GetNodeGroups(), group => group.Id == "terrain-authoring");
        Assert.Contains(
            viewModel.Editor.GetNodeGroupSnapshots(),
            group => group.Id == "terrain-authoring"
                && group.Position == new GraphPoint(340, 40)
                && group.Size == new GraphSize(292, 446)
                && group.ExtraPadding == default);
        var lightSurface = Assert.Single(viewModel.Editor.Session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == "light");
        var lightNode = Assert.IsType<NodeViewModel>(viewModel.Editor.FindNode("light"));
        Assert.Equal(GraphNodeExpansionState.Collapsed, lightSurface.ExpansionState);
        Assert.Equal("details", lightSurface.ActiveTier.Key);
        Assert.True(lightNode.SurfaceMeasurement.HeightToRevealAdditionalInputs > lightNode.Height);
        Assert.True(lightNode.SurfaceMeasurement.WidthToRevealInputEditors > lightNode.Width);
        Assert.Contains(
            canvas!.GetVisualDescendants().OfType<Border>(),
            border => string.Equals(
                AutomationProperties.GetName(border),
                "Terrain Authoring group",
                StringComparison.Ordinal));
        Assert.Contains(
            canvas.GetVisualDescendants().OfType<Thumb>(),
            thumb => string.Equals(
                AutomationProperties.GetName(thumb),
                "Terrain Authoring group right resize handle",
                StringComparison.Ordinal));
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
