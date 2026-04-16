using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoDiagnosticsProjectionTests
{
    [Fact]
    public void MainWindowViewModel_ProjectsInspectionSnapshotAndRecentDiagnosticsFromEditorSession()
    {
        var viewModel = new MainWindowViewModel();

        var inspection = viewModel.Editor.Session.Diagnostics.CaptureInspectionSnapshot();
        var diagnostics = viewModel.Editor.Session.Diagnostics.GetRecentDiagnostics(10);

        Assert.Equal("IGraphEditorSession", viewModel.RuntimeSessionInterfaceName);
        Assert.Equal("Editor.Session.Diagnostics", viewModel.RuntimeDiagnosticsSourceName);
        Assert.Equal("CaptureInspectionSnapshot", nameof(viewModel.Editor.Session.Diagnostics.CaptureInspectionSnapshot));
        Assert.Equal("GetRecentDiagnostics", nameof(viewModel.Editor.Session.Diagnostics.GetRecentDiagnostics));

        Assert.Equal(inspection.Document.Title, viewModel.RuntimeDocumentTitle);
        Assert.Equal(inspection.Document.Nodes.Count, viewModel.RuntimeNodeCount);
        Assert.Equal(inspection.Selection.SelectedNodeIds.Count, viewModel.RuntimeSelectedNodeCount);
        Assert.Equal(inspection.Viewport.Zoom, viewModel.RuntimeViewportZoom);
        Assert.Equal(inspection.PendingConnection.HasPendingConnection, viewModel.RuntimeHasPendingConnection);
        Assert.Equal(diagnostics.Count, viewModel.RecentDiagnostics.Count);

        if (diagnostics.Count > 0)
        {
            var projected = viewModel.RecentDiagnostics[0];
            var actual = diagnostics[0];

            Assert.Equal(actual.Code, projected.Code);
            Assert.Equal(actual.Operation, projected.Operation);
            Assert.Equal(actual.Message, projected.Message);
            Assert.Equal(actual.Severity, projected.Severity);
        }
    }

    [Fact]
    public void MainWindowViewModel_KeepsStatusMessageSeparateFromMachineReadableDiagnostics()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.Editor.Session.Commands.SaveWorkspace();

        var diagnostics = viewModel.Editor.Session.Diagnostics.GetRecentDiagnostics(10);
        var latestDiagnostic = Assert.Single(diagnostics, diagnostic => diagnostic.Code == "workspace.save.succeeded");
        var projectedDiagnostic = Assert.Single(viewModel.RecentDiagnostics, diagnostic => diagnostic.Code == "workspace.save.succeeded");

        Assert.Equal("以下诊断直接来自 Editor.Session.Diagnostics，用于确认共享运行时状态。", viewModel.RuntimeDiagnosticsSummary);
        Assert.Equal(viewModel.Editor.StatusMessage, viewModel.CompatibilityStatusMessage);
        Assert.Equal(latestDiagnostic.Message, projectedDiagnostic.Message);
        Assert.Equal(latestDiagnostic.Code, projectedDiagnostic.Code);
        Assert.Equal(latestDiagnostic.Operation, projectedDiagnostic.Operation);
        Assert.NotEqual(viewModel.CompatibilityStatusMessage, projectedDiagnostic.Code);
        Assert.NotEqual(viewModel.CompatibilityStatusMessage, projectedDiagnostic.Operation);
    }

    [Fact]
    public void MainWindowViewModel_RuntimeSignalLinesStayAlignedWithInspectionSnapshot()
    {
        var viewModel = new MainWindowViewModel();
        var inspection = viewModel.Editor.Session.Diagnostics.CaptureInspectionSnapshot();
        var runtimeSignalLines = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            viewModel.GetType().GetProperty("RuntimeSignalLines")?.GetValue(viewModel));

        Assert.Contains(runtimeSignalLines, line => line == $"文档标题：{inspection.Document.Title}");
        Assert.Contains(runtimeSignalLines, line => line == $"节点数量：{inspection.Document.Nodes.Count}");
        Assert.Contains(runtimeSignalLines, line => line == $"连线数量：{inspection.Document.Connections.Count}");
        Assert.Contains(runtimeSignalLines, line => line == $"当前选择：{inspection.Selection.SelectedNodeIds.Count}");
    }

    [AvaloniaFact]
    public void MainWindow_RendersRuntimeDiagnosticsHelperFromCanonicalPath()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var helper = window.FindControl<TextBlock>("RuntimeDiagnosticsHelperText");

        Assert.NotNull(helper);
        Assert.Equal("以下诊断直接来自 Editor.Session.Diagnostics，用于确认共享运行时状态。", helper!.Text);
    }
}
