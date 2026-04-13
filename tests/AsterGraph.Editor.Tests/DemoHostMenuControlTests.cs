using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class DemoHostMenuControlTests
{
    [Fact]
    public void MainWindowViewModel_RuntimeGroupIncludesConnectionCountAndDiagnosticsSummary()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.OpenHostMenuGroup("运行时");

        Assert.Contains(viewModel.SelectedHostMenuGroupLines, line => line.StartsWith("文档标题：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedHostMenuGroupLines, line => line.StartsWith("节点数量：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedHostMenuGroupLines, line => line.StartsWith("连线数量：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedHostMenuGroupLines, line => line.StartsWith("当前选择：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedHostMenuGroupLines, line => line.StartsWith("视口缩放：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedHostMenuGroupLines, line => line.StartsWith("最近诊断：", StringComparison.Ordinal));
        Assert.Same(viewModel.Editor, viewModel.Editor);
    }

    [Fact]
    public void MainWindowViewModel_RuntimeProjectionPropertiesMirrorCurrentEditorSession()
    {
        var viewModel = new MainWindowViewModel();
        var inspection = viewModel.Editor.Session.Diagnostics.CaptureInspectionSnapshot();

        Assert.Equal(inspection.Document.Title, viewModel.RuntimeDocumentTitle);
        Assert.Equal(inspection.Document.Nodes.Count, viewModel.RuntimeNodeCount);
        Assert.Equal(inspection.Document.Connections.Count, viewModel.RuntimeConnectionCount);
        Assert.Equal(inspection.Selection.SelectedNodeIds.Count, viewModel.RuntimeSelectedNodeCount);
        Assert.Equal(inspection.Viewport.Zoom, viewModel.RuntimeViewportZoom);
    }

    [Fact]
    public void MainWindowViewModel_ExposesDedicatedPhase21ConfigurationAndRuntimeSignalRows()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.OpenHostMenuGroup("证明");

        var currentConfigurationLines = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            viewModel.GetType().GetProperty("CurrentConfigurationLines")?.GetValue(viewModel));
        var ownershipProofLines = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            viewModel.GetType().GetProperty("OwnershipProofLines")?.GetValue(viewModel));

        Assert.Contains(currentConfigurationLines, line => line.StartsWith("显示顶栏：", StringComparison.Ordinal));
        Assert.Contains(currentConfigurationLines, line => line.StartsWith("只读模式：", StringComparison.Ordinal));
        Assert.Contains(currentConfigurationLines, line => line.StartsWith("当前分组：", StringComparison.Ordinal));
        Assert.Contains(ownershipProofLines, line => line.Contains("Editor.Session", StringComparison.Ordinal));

        viewModel.OpenHostMenuGroup("运行时");

        var runtimeSignalLines = Assert.IsAssignableFrom<IReadOnlyList<string>>(
            viewModel.GetType().GetProperty("RuntimeSignalLines")?.GetValue(viewModel));

        Assert.Contains(runtimeSignalLines, line => line.StartsWith("文档标题：", StringComparison.Ordinal));
        Assert.Contains(runtimeSignalLines, line => line.StartsWith("节点数量：", StringComparison.Ordinal));
        Assert.Contains(runtimeSignalLines, line => line.StartsWith("连线数量：", StringComparison.Ordinal));
        Assert.Contains(runtimeSignalLines, line => line.StartsWith("当前状态：", StringComparison.Ordinal));
    }

    [Fact]
    public void MainWindowViewModel_BehaviorTogglesStillDriveEditorBehaviorAndPermissions()
    {
        var viewModel = new MainWindowViewModel();
        var originalEditor = viewModel.Editor;

        viewModel.IsGridSnappingEnabled = false;
        viewModel.IsAlignmentGuidesEnabled = false;
        viewModel.IsReadOnlyEnabled = true;
        viewModel.AreWorkspaceCommandsEnabled = false;
        viewModel.AreFragmentCommandsEnabled = false;
        viewModel.AreHostMenuExtensionsEnabled = false;

        Assert.False(viewModel.Editor.BehaviorOptions.DragAssist.EnableGridSnapping);
        Assert.False(viewModel.Editor.BehaviorOptions.DragAssist.EnableAlignmentGuides);
        Assert.False(viewModel.Editor.CommandPermissions.Workspace.AllowSave);
        Assert.False(viewModel.Editor.CommandPermissions.Workspace.AllowLoad);
        Assert.False(viewModel.Editor.CommandPermissions.Fragments.AllowImport);
        Assert.False(viewModel.Editor.CommandPermissions.Fragments.AllowExport);
        Assert.False(viewModel.Editor.CommandPermissions.Host.AllowContextMenuExtensions);
        Assert.Same(originalEditor, viewModel.Editor);
    }

    [AvaloniaFact]
    public void MainWindow_ViewChromeBindingsReachTheLiveGraphEditorView()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var graphEditorView = Assert.IsType<GraphEditorView>(window.FindControl<GraphEditorView>("MainGraphEditorView"));

        viewModel.IsHeaderChromeVisible = true;
        viewModel.IsLibraryChromeVisible = true;
        viewModel.IsInspectorChromeVisible = true;
        viewModel.IsStatusChromeVisible = true;

        Assert.True(graphEditorView.IsHeaderChromeVisible);
        Assert.True(graphEditorView.IsLibraryChromeVisible);
        Assert.True(graphEditorView.IsInspectorChromeVisible);
        Assert.True(graphEditorView.IsStatusChromeVisible);
    }

    [AvaloniaFact]
    public void MainWindow_MenuCheckItemsWriteThroughToHostState()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var viewMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ViewMenu"));
        var behaviorMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_BehaviorMenu"));
        var graphEditorView = Assert.IsType<GraphEditorView>(window.FindControl<GraphEditorView>("MainGraphEditorView"));

        var headerChromeMenuItem = GetMenuItem(viewMenu, "显示顶栏");
        var gridSnappingMenuItem = GetMenuItem(behaviorMenu, "网格吸附");

        Assert.False(viewModel.IsHeaderChromeVisible);
        Assert.True(viewModel.IsGridSnappingEnabled);

        headerChromeMenuItem.IsChecked = true;
        gridSnappingMenuItem.IsChecked = false;

        Assert.True(viewModel.IsHeaderChromeVisible);
        Assert.True(graphEditorView.IsHeaderChromeVisible);
        Assert.False(viewModel.IsGridSnappingEnabled);
        Assert.False(viewModel.Editor.BehaviorOptions.DragAssist.EnableGridSnapping);

        headerChromeMenuItem.IsChecked = false;
        gridSnappingMenuItem.IsChecked = true;

        Assert.False(viewModel.IsHeaderChromeVisible);
        Assert.False(graphEditorView.IsHeaderChromeVisible);
        Assert.True(viewModel.IsGridSnappingEnabled);
        Assert.True(viewModel.Editor.BehaviorOptions.DragAssist.EnableGridSnapping);
    }

    [AvaloniaFact]
    public void MainWindow_SameGroupCanReopenAfterPaneCloses()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var splitView = Assert.IsType<SplitView>(window.FindControl<SplitView>("PART_HostShellSplitView"));

        viewModel.OpenHostMenuGroup("视图");

        Assert.True(viewModel.IsHostPaneOpen);
        Assert.True(splitView.IsPaneOpen);

        viewModel.CloseHostPane();

        Assert.False(viewModel.IsHostPaneOpen);
        Assert.False(splitView.IsPaneOpen);

        viewModel.OpenHostMenuGroup("视图");

        Assert.True(viewModel.IsHostPaneOpen);
        Assert.True(splitView.IsPaneOpen);

        splitView.IsPaneOpen = false;

        Assert.False(splitView.IsPaneOpen);
        Assert.False(viewModel.IsHostPaneOpen);

        viewModel.OpenHostMenuGroup("视图");

        Assert.True(viewModel.IsHostPaneOpen);
        Assert.True(splitView.IsPaneOpen);
    }

    private static MenuItem GetMenuItem(MenuItem parent, string header)
    {
        var matches = parent.Items?
            .OfType<MenuItem>()
            .Where(item => string.Equals(item.Header?.ToString(), header, StringComparison.Ordinal))
            .ToArray();

        Assert.NotNull(matches);
        return Assert.Single(matches);
    }
}
