using System;
using System.Collections;
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
    public void MainWindow_RendersPhase20HostMenuControlEntries()
    {
        var window = CreateWindow();

        var viewMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ViewMenu"));
        var behaviorMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_BehaviorMenu"));
        var runtimeMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_RuntimeMenu"));

        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(viewMenu, "显示顶栏").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(viewMenu, "显示节点库").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(viewMenu, "显示检查器").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(viewMenu, "显示状态栏").ToggleType);
        Assert.Equal("打开视图控制", GetMenuItem(viewMenu, "打开视图控制").Header);

        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(behaviorMenu, "只读模式").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(behaviorMenu, "网格吸附").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(behaviorMenu, "对齐辅助线").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(behaviorMenu, "工作区命令").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(behaviorMenu, "片段命令").ToggleType);
        Assert.Equal(MenuItemToggleType.CheckBox, GetMenuItem(behaviorMenu, "宿主菜单扩展").ToggleType);
        Assert.Equal("打开行为控制", GetMenuItem(behaviorMenu, "打开行为控制").Header);

        Assert.Equal("打开运行时摘要", GetMenuItem(runtimeMenu, "打开运行时摘要").Header);
        Assert.Equal("查看最近诊断", GetMenuItem(runtimeMenu, "查看最近诊断").Header);
    }

    [AvaloniaFact]
    public void MainWindow_RendersDedicatedHostDrawerSectionsForControlsAndRuntime()
    {
        var window = CreateWindow();

        Assert.NotNull(window.FindControl<Control>("PART_ViewDrawerControls"));
        Assert.NotNull(window.FindControl<Control>("PART_BehaviorDrawerControls"));
        Assert.NotNull(window.FindControl<Control>("PART_RuntimeSummarySection"));
        Assert.NotNull(window.FindControl<Control>("PART_RuntimeDiagnosticsSection"));
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

    private static MenuItem GetMenuItem(MenuItem parent, string header)
    {
        var matches = GetMenuItems(parent)
            .Where(item => string.Equals(item.Header?.ToString(), header, StringComparison.Ordinal))
            .ToArray();

        return Assert.Single(matches);
    }

    private static MenuItem[] GetMenuItems(MenuItem parent)
        => parent.Items is IEnumerable items
            ? items.OfType<MenuItem>().ToArray()
            : [];
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
