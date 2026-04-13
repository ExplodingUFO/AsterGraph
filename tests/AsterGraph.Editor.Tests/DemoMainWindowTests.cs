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

        Assert.Equal("查看运行时状态", GetMenuItem(runtimeMenu, "查看运行时状态").Header);
        Assert.Equal("查看运行时诊断", GetMenuItem(runtimeMenu, "查看运行时诊断").Header);
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
    public void MainWindow_RendersPhase21ProofFocusedShellCopy()
    {
        var window = CreateWindow();

        var showcaseMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ShowcaseMenu"));
        var proofMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ProofMenu"));

        Assert.Equal("打开展示摘要", GetMenuItem(showcaseMenu, "打开展示摘要").Header);
        Assert.Equal("查看宿主边界", GetMenuItem(proofMenu, "查看宿主边界").Header);
        Assert.Null(FindMenuItem(showcaseMenu, "打开展示面板"));
        Assert.Null(FindMenuItem(proofMenu, "查看证明要点"));

        Assert.Equal("宿主控制抽屉", window.FindControl<TextBlock>("PART_HostDrawerCaptionText")?.Text);
        Assert.Equal("实时 SDK 会话", window.FindControl<TextBlock>("PART_GraphIntroTitleText")?.Text);
        Assert.Equal("宿主控制", window.FindControl<TextBlock>("PART_HostOwnershipBadgeText")?.Text);
        Assert.Equal("共享运行时", window.FindControl<TextBlock>("PART_RuntimeOwnershipBadgeText")?.Text);
        Assert.Equal("当前分组 · 展示", window.FindControl<TextBlock>("PART_ActiveHostGroupBadgeText")?.Text);
    }

    [AvaloniaFact]
    public void MainWindow_RendersDedicatedPhase21RuntimeAndProofSections()
    {
        var window = CreateWindow();

        Assert.NotNull(window.FindControl<Control>("PART_RuntimeConfigurationSection"));
        Assert.NotNull(window.FindControl<Control>("PART_RuntimeSignalsSection"));
        Assert.NotNull(window.FindControl<Control>("PART_ProofConfigurationSection"));
        Assert.NotNull(window.FindControl<Control>("PART_ProofOwnershipSection"));
        Assert.NotNull(window.FindControl<Control>("PART_ProofRuntimeSignalsSection"));
    }

    [AvaloniaFact]
    public void MainWindow_RendersOperatorFacingRuntimeAndProofCopy()
    {
        var window = CreateWindow();

        var runtimeMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_RuntimeMenu"));
        var proofMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_ProofMenu"));

        Assert.Equal("查看运行时状态", GetMenuItem(runtimeMenu, "查看运行时状态").Header);
        Assert.Equal("查看运行时诊断", GetMenuItem(runtimeMenu, "查看运行时诊断").Header);
        Assert.Equal("查看宿主边界", GetMenuItem(proofMenu, "查看宿主边界").Header);

        Assert.Equal("宿主开关快照", GetTextBlockText(window, "PART_RuntimeConfigurationHeading"));
        Assert.Equal("共享运行时状态", GetTextBlockText(window, "PART_RuntimeSignalsHeading"));
        Assert.Equal("最近诊断", GetTextBlockText(window, "PART_RuntimeDiagnosticsHeading"));
        Assert.Equal("宿主控制快照", GetTextBlockText(window, "PART_ProofConfigurationHeading"));
        Assert.Equal("边界证据", GetTextBlockText(window, "PART_ProofOwnershipHeading"));
        Assert.Equal("共享运行时证据", GetTextBlockText(window, "PART_ProofRuntimeSignalsHeading"));
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

    private static MenuItem? FindMenuItem(MenuItem parent, string header)
        => GetMenuItems(parent)
            .SingleOrDefault(item => string.Equals(item.Header?.ToString(), header, StringComparison.Ordinal));

    private static string? GetTextBlockText(MainWindow window, string name)
        => window.FindControl<TextBlock>(name)?.Text;

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
