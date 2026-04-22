using System;
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

        var inspection = viewModel.Session.Diagnostics.CaptureInspectionSnapshot();
        var diagnostics = viewModel.Session.Diagnostics.GetRecentDiagnostics(10);

        Assert.Equal("IGraphEditorSession", viewModel.RuntimeSessionInterfaceName);
        Assert.Same(viewModel.Editor.Session, viewModel.Session);
        Assert.Equal("Session.Diagnostics", viewModel.RuntimeDiagnosticsSourceName);
        Assert.Equal("CaptureInspectionSnapshot", nameof(viewModel.Session.Diagnostics.CaptureInspectionSnapshot));
        Assert.Equal("GetRecentDiagnostics", nameof(viewModel.Session.Diagnostics.GetRecentDiagnostics));

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
    public void MainWindowViewModel_RuntimeInspectionSurfaceProjectsAllCanonicalStateSections()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.Session.Commands.SetSelection(["light"], "light", updateStatus: false);

        var surface = viewModel.RuntimeInspectionSurface;

        Assert.Equal("运行时检查", surface.Heading);
        Assert.Equal("文档", surface.Document.Heading);
        Assert.Equal("选择", surface.Selection.Heading);
        Assert.Equal("视口", surface.Viewport.Heading);
        Assert.Equal("能力", surface.Capabilities.Heading);
        Assert.Equal("待完成连线", surface.PendingConnection.Heading);
        Assert.Equal("特性描述", surface.FeatureDescriptors.Heading);
        Assert.Equal("性能与观测", surface.PerfInstrumentation.Heading);
        Assert.Equal("命令时间线", surface.CommandTimeline.Heading);
        Assert.Equal("最近诊断", surface.RecentDiagnostics.Heading);
        Assert.Equal("插件信任与加载", surface.PluginLoads.Heading);

        Assert.Contains(surface.Document.Lines, line => line.StartsWith("文档标题：", StringComparison.Ordinal));
        Assert.Contains(surface.Document.Lines, line => line.StartsWith("节点数量：", StringComparison.Ordinal));
        Assert.Contains(surface.Document.Lines, line => line.StartsWith("连线数量：", StringComparison.Ordinal));
        Assert.Contains(surface.Selection.Lines, line => line.StartsWith("当前选择数量：", StringComparison.Ordinal));
        Assert.Contains(surface.Selection.Lines, line => line.StartsWith("当前选择标识：", StringComparison.Ordinal));
        Assert.Contains(surface.Viewport.Lines, line => line.StartsWith("视口缩放：", StringComparison.Ordinal));
        Assert.Contains(surface.Capabilities.Lines, line => line.StartsWith("可保存工作区：", StringComparison.Ordinal));
        Assert.Contains(surface.Capabilities.Lines, line => line.StartsWith("可加载工作区：", StringComparison.Ordinal));
        Assert.Contains(surface.PendingConnection.Lines, line => line.StartsWith("待完成连线：", StringComparison.Ordinal));
        Assert.NotEmpty(surface.FeatureDescriptors.Lines);
        Assert.Contains(surface.PerfInstrumentation.Lines, line => line.StartsWith("诊断记录器：", StringComparison.Ordinal));
        Assert.Contains(surface.PerfInstrumentation.Lines, line => line.StartsWith("当前状态：", StringComparison.Ordinal));
        Assert.Contains(surface.CommandTimeline.Lines, line => line.Contains("selection.set", StringComparison.Ordinal));
        Assert.NotEmpty(surface.RecentDiagnostics.Lines);
        Assert.Contains(surface.PluginLoads.Lines, line => line.Contains("状态：", StringComparison.Ordinal));
        Assert.Contains(surface.PluginLoads.Lines, line => line.Contains("信任：", StringComparison.Ordinal));
    }

    [Fact]
    public void MainWindowViewModel_RuntimeInspectionSurfaceReprojectsWithLanguageChanges()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.Session.Commands.SetSelection(["light"], "light", updateStatus: false);

        viewModel.SelectLanguage("en");

        var surface = viewModel.RuntimeInspectionSurface;

        Assert.Equal("Runtime inspection", surface.Heading);
        Assert.Equal("Document", surface.Document.Heading);
        Assert.Equal("Selection", surface.Selection.Heading);
        Assert.Equal("Viewport", surface.Viewport.Heading);
        Assert.Equal("Capabilities", surface.Capabilities.Heading);
        Assert.Equal("Pending connection", surface.PendingConnection.Heading);
        Assert.Equal("Feature descriptors", surface.FeatureDescriptors.Heading);
        Assert.Equal("Performance and instrumentation", surface.PerfInstrumentation.Heading);
        Assert.Equal("Command timeline", surface.CommandTimeline.Heading);
        Assert.Equal("Recent diagnostics", surface.RecentDiagnostics.Heading);
        Assert.Equal("Plugin trust and load", surface.PluginLoads.Heading);

        Assert.Contains(surface.Document.Lines, line => line.StartsWith("Document title: ", StringComparison.Ordinal));
        Assert.Contains(surface.Capabilities.Lines, line => line.StartsWith("Can save workspace: ", StringComparison.Ordinal));
        Assert.Contains(surface.PerfInstrumentation.Lines, line => line.StartsWith("Diagnostics logger: ", StringComparison.Ordinal));
        Assert.Contains(surface.CommandTimeline.Lines, line => line.Contains("Command", StringComparison.Ordinal));
        Assert.Contains(surface.PluginLoads.Lines, line => line.Contains("State: ", StringComparison.Ordinal));
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
        Assert.Equal("以下诊断直接来自 Session.Diagnostics，用于确认共享运行时状态。", helper!.Text);
    }

    [AvaloniaFact]
    public void MainWindow_RendersRuntimeInspectionSurfaceInsideTheRuntimeDrawer()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        viewModel.OpenHostMenuGroup("运行时");

        var inspectionSection = window.FindControl<StackPanel>("PART_RuntimeInspectionSection");
        var inspectionHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionHeading");
        var inspectionDocumentHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionDocumentHeading");
        var inspectionDocumentLines = window.FindControl<ItemsControl>("PART_RuntimeInspectionDocumentLines");
        var inspectionSelectionHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionSelectionHeading");
        var inspectionCapabilityHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionCapabilityHeading");
        var inspectionFeatureHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionFeatureHeading");
        var inspectionPerfHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionPerfHeading");
        var inspectionPerfLines = window.FindControl<ItemsControl>("PART_RuntimeInspectionPerfLines");
        var inspectionCommandTimelineHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionCommandTimelineHeading");
        var inspectionCommandTimelineLines = window.FindControl<ItemsControl>("PART_RuntimeInspectionCommandTimelineLines");
        var inspectionDiagnosticsHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionDiagnosticsHeading");
        var inspectionPluginHeading = window.FindControl<TextBlock>("PART_RuntimeInspectionPluginHeading");

        viewModel.Session.Commands.SetSelection(["light"], "light", updateStatus: false);

        Assert.NotNull(inspectionSection);
        Assert.True(inspectionSection!.IsVisible);
        Assert.NotNull(inspectionHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.Heading, inspectionHeading!.Text);
        Assert.NotNull(inspectionDocumentHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.Document.Heading, inspectionDocumentHeading!.Text);
        Assert.NotNull(inspectionDocumentLines);
        Assert.Same(viewModel.RuntimeInspectionSurface.Document.Lines, inspectionDocumentLines!.ItemsSource);
        Assert.NotNull(inspectionSelectionHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.Selection.Heading, inspectionSelectionHeading!.Text);
        Assert.NotNull(inspectionCapabilityHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.Capabilities.Heading, inspectionCapabilityHeading!.Text);
        Assert.NotNull(inspectionFeatureHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.FeatureDescriptors.Heading, inspectionFeatureHeading!.Text);
        Assert.NotNull(inspectionPerfHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.PerfInstrumentation.Heading, inspectionPerfHeading!.Text);
        Assert.NotNull(inspectionPerfLines);
        Assert.Same(viewModel.RuntimeInspectionSurface.PerfInstrumentation.Lines, inspectionPerfLines!.ItemsSource);
        Assert.NotNull(inspectionCommandTimelineHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.CommandTimeline.Heading, inspectionCommandTimelineHeading!.Text);
        Assert.NotNull(inspectionCommandTimelineLines);
        Assert.Same(viewModel.RuntimeInspectionSurface.CommandTimeline.Lines, inspectionCommandTimelineLines!.ItemsSource);
        Assert.NotNull(inspectionDiagnosticsHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.RecentDiagnostics.Heading, inspectionDiagnosticsHeading!.Text);
        Assert.NotNull(inspectionPluginHeading);
        Assert.Equal(viewModel.RuntimeInspectionSurface.PluginLoads.Heading, inspectionPluginHeading!.Text);
    }

}
