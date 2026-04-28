using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using Xunit;

namespace AsterGraph.HelloWorld.Tests;

public sealed class StarterAvaloniaScaffoldTests
{
    [Fact]
    public void StarterAvaloniaRuntimeSurface_ExposesSessionFirstOwnership()
    {
        StarterAvaloniaHeadlessEnvironment.EnsureInitialized();
        var surface = AsterGraph.Starter.Avalonia.StarterAvaloniaWindowFactory.CreateRuntimeSurface();

        Assert.Same(surface.Editor.Session, surface.Session);
        Assert.Same(surface.Editor, surface.View.Editor);
    }

    [Fact]
    public void StarterAvaloniaRuntimeSurface_ComposesShippedUiFactory()
    {
        StarterAvaloniaHeadlessEnvironment.EnsureInitialized();
        var surface = AsterGraph.Starter.Avalonia.StarterAvaloniaWindowFactory.CreateRuntimeSurface();

        Assert.IsType<GraphEditorView>(surface.View);
        Assert.Same(surface.View, surface.Window.Content);
        Assert.Equal(GraphEditorViewChromeMode.Default, surface.View.ChromeMode);
    }

    [Fact]
    public void StarterAvaloniaHostBuilder_PreservesEditorAndViewOptions()
    {
        StarterAvaloniaHeadlessEnvironment.EnsureInitialized();
        var builder = AsterGraph.Starter.Avalonia.StarterAvaloniaWindowFactory.CreateHostBuilder();
        var editor = builder.BuildEditor();
        var viewOptions = builder.BuildViewOptions(editor);

        Assert.Same(editor, viewOptions.Editor);
        Assert.Equal(GraphEditorViewChromeMode.Default, viewOptions.ChromeMode);
        Assert.Equal(AsterGraphWorkbenchOptions.Default, viewOptions.Workbench);
        Assert.True(viewOptions.Workbench.ShowHeaderChrome);
        Assert.True(viewOptions.Workbench.ShowNodePalette);
        Assert.True(viewOptions.Workbench.ShowInspector);
        Assert.True(viewOptions.Workbench.ShowStatus);
        Assert.Equal(AsterGraphWorkbenchLayoutPreset.Authoring, viewOptions.Workbench.LayoutPreset);
        Assert.Equal(AsterGraphWorkbenchPanelState.Default, viewOptions.Workbench.PanelState);
        Assert.Equal(AsterGraphWorkbenchPerformanceMode.Balanced, viewOptions.Workbench.PerformanceMode);
        Assert.Equal(AsterGraphWorkbenchPerformanceMode.Balanced, viewOptions.Workbench.PerformancePolicy.Mode);
        Assert.True(viewOptions.EnableDefaultContextMenu);
        Assert.Equal(AsterGraphCommandShortcutPolicy.Default, viewOptions.CommandShortcutPolicy);
        Assert.True(viewOptions.Workbench.EnableDefaultWheelViewportGestures);
        Assert.True(viewOptions.Workbench.EnableAltLeftDragPanning);
        Assert.Null(viewOptions.Presentation);
        Assert.NotNull(editor.Session);
    }

    [Fact]
    public void StarterAvaloniaHostBuilder_ProjectsWorkbenchOptionsToHostedView()
    {
        StarterAvaloniaHeadlessEnvironment.EnsureInitialized();
        var builder = AsterGraph.Starter.Avalonia.StarterAvaloniaWindowFactory
            .CreateHostBuilder()
            .UseWorkbench(new AsterGraphWorkbenchOptions
            {
                ShowHeaderChrome = false,
                ShowNodePalette = false,
                ShowInspector = false,
                ShowStatus = false,
                EnableDefaultWheelViewportGestures = false,
                EnableAltLeftDragPanning = false,
                PerformanceMode = AsterGraphWorkbenchPerformanceMode.Throughput,
                PanelState = AsterGraphWorkbenchPanelState.Default with
                {
                    StencilVisible = true,
                    InspectorVisible = true,
                },
            })
            .UseDefaultContextMenu(false)
            .UseCommandShortcutPolicy(AsterGraphCommandShortcutPolicy.Disabled);
        var editor = builder.BuildEditor();
        var viewOptions = builder.BuildViewOptions(editor);
        var view = AsterGraphAvaloniaViewFactory.Create(viewOptions);

        Assert.False(view.IsHeaderChromeVisible);
        Assert.False(view.IsLibraryChromeVisible);
        Assert.False(view.IsInspectorChromeVisible);
        Assert.False(view.IsStatusChromeVisible);
        Assert.False(view.EnableDefaultContextMenu);
        Assert.Equal(AsterGraphCommandShortcutPolicy.Disabled, view.CommandShortcutPolicy);
        Assert.False(view.EnableDefaultWheelViewportGestures);
        Assert.False(view.EnableAltLeftDragPanning);
        Assert.Equal(AsterGraphWorkbenchPerformanceMode.Throughput, view.WorkbenchPerformanceMode);
        Assert.Equal(48, view.CurrentWorkbenchPerformancePolicy.StencilCardsPerSectionLimit);
        Assert.False(view.CurrentWorkbenchPerformancePolicy.ProjectMiniMapContinuously);
        Assert.Same(editor, view.Editor);
    }

    [Fact]
    public void WorkbenchPerformancePolicies_DefineQualityBalancedAndThroughputProjection()
    {
        var quality = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Quality);
        var balanced = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Balanced);
        var throughput = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Throughput);

        Assert.Equal(int.MaxValue, quality.StencilCardsPerSectionLimit);
        Assert.Equal(128, balanced.StencilCardsPerSectionLimit);
        Assert.Equal(48, throughput.StencilCardsPerSectionLimit);
        Assert.True(quality.ProjectAdvancedInspectorByDefault);
        Assert.False(balanced.ProjectAdvancedInspectorByDefault);
        Assert.False(throughput.ProjectHoveredToolbars);
        Assert.True(throughput.CommandRefreshBatchMilliseconds > balanced.CommandRefreshBatchMilliseconds);
    }

    [Fact]
    public void StarterAvaloniaHostBuilder_ProjectsLayoutPresetPanelStateToHostedView()
    {
        StarterAvaloniaHeadlessEnvironment.EnsureInitialized();
        var panelState = AsterGraphWorkbenchPanelState.Default with
        {
            StencilVisible = false,
            StencilWidth = 288,
            InspectorVisible = true,
            InspectorWidth = 360,
            MiniMapVisible = false,
            RuntimePanelVisible = true,
            ExportPanelVisible = false,
            PluginPanelVisible = true,
        };
        var builder = AsterGraph.Starter.Avalonia.StarterAvaloniaWindowFactory
            .CreateHostBuilder()
            .UseWorkbench(AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.Debugging) with
            {
                PanelState = panelState,
            });
        var editor = builder.BuildEditor();
        var viewOptions = builder.BuildViewOptions(editor);
        var view = AsterGraphAvaloniaViewFactory.Create(viewOptions);

        Assert.Equal(AsterGraphWorkbenchLayoutPreset.Debugging, viewOptions.Workbench.LayoutPreset);
        Assert.Equal(panelState, viewOptions.Workbench.PanelState);
        Assert.False(view.IsLibraryChromeVisible);
        Assert.True(view.IsInspectorChromeVisible);
        Assert.Equal(AsterGraphWorkbenchOptions.Default, viewOptions.Workbench.ResetLayout());
    }
}

file static class StarterAvaloniaHeadlessEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        StarterAvaloniaTestAppBuilder.BuildAvaloniaApp().SetupWithoutStarting();
        _initialized = true;
    }
}

file sealed class StarterAvaloniaTestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}

file static class StarterAvaloniaTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<StarterAvaloniaTestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
