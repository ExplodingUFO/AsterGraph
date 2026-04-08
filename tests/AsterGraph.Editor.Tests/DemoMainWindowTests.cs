using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class DemoMainWindowTests
{
    [AvaloniaFact]
    public void MainWindow_RendersGraphFirstHostMenuShell()
    {
        var window = CreateWindow();

        Assert.NotNull(window.FindControl<Menu>("PART_HostMenu"));
        Assert.Equal("展示", Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ShowcaseMenu")).Header);
        Assert.Equal("视图", Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ViewMenu")).Header);
        Assert.Equal("行为", Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_BehaviorMenu")).Header);
        Assert.Equal("运行时", Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_RuntimeMenu")).Header);
        Assert.Equal("证明", Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ProofMenu")).Header);

        Assert.Single(window.GetVisualDescendants().OfType<GraphEditorView>());
        Assert.NotNull(window.FindControl<Control>("MainGraphEditorView"));
        Assert.Null(window.FindControl<Grid>("MainShellGrid"));
    }

    [AvaloniaFact]
    public void MainWindow_UsesRightSideCompactHostPaneThatStartsClosed()
    {
        var window = CreateWindow();
        var splitView = window.FindControl<SplitView>("PART_HostShellSplitView");

        Assert.NotNull(splitView);
        Assert.Equal(SplitViewDisplayMode.Overlay, splitView!.DisplayMode);
        Assert.Equal(SplitViewPanePlacement.Right, splitView.PanePlacement);
        Assert.False(splitView.IsPaneOpen);
    }

    [AvaloniaFact]
    public void MainWindow_KeepsSingleCanvasFirstEditorComposition()
    {
        var window = CreateWindow();
        var graphEditorView = window.FindControl<GraphEditorView>("MainGraphEditorView");

        Assert.NotNull(graphEditorView);
        Assert.Single(window.GetVisualDescendants().OfType<GraphEditorView>());
        Assert.False(graphEditorView!.IsHeaderChromeVisible);
        Assert.False(graphEditorView.IsLibraryChromeVisible);
        Assert.False(graphEditorView.IsInspectorChromeVisible);
        Assert.False(graphEditorView.IsStatusChromeVisible);
    }

    private static MainWindow CreateWindow()
    {
        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel(),
        };

        window.Show();
        return window;
    }
}

public sealed class DemoMainWindowTestsAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<DemoMainWindowTestsApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public sealed class DemoMainWindowTestsApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
