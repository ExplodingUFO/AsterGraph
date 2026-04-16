using System;
using System.Linq;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class GraphEditorLocalizationDemoTests
{
    [Fact]
    public void DemoGraphLocalizationProvider_RemainsCanonicalRuntimeCaptionSource()
    {
        var providerType = typeof(MainWindowViewModel).GetNestedType("DemoGraphLocalizationProvider", System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(providerType);
        Assert.True(typeof(IGraphLocalizationProvider).IsAssignableFrom(providerType));

        var zhProvider = (IGraphLocalizationProvider?)Activator.CreateInstance(providerType!, "zh-CN");
        var enProvider = (IGraphLocalizationProvider?)Activator.CreateInstance(providerType!, "en");
        Assert.NotNull(zhProvider);
        Assert.NotNull(enProvider);

        Assert.Equal("请选择一个节点", zhProvider!.GetString("editor.inspector.title.none", "fallback"));
        Assert.Equal("添加节点", zhProvider.GetString("editor.menu.canvas.addNode", "fallback"));
        Assert.Equal("Select a node", enProvider!.GetString("editor.inspector.title.none", "fallback"));
        Assert.Equal("Add Node", enProvider.GetString("editor.menu.canvas.addNode", "fallback"));
        Assert.Equal("fallback", enProvider.GetString("editor.stats.caption", "fallback"));
    }

    [Fact]
    public void DemoShowcase_PreservesEnglishTechnicalIdentifiers()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Contains(viewModel.StandaloneSurfaceLines, line => line.Contains("AsterGraphCanvasViewFactory", StringComparison.Ordinal));
        Assert.Contains(viewModel.StandaloneSurfaceLines, line => line.Contains("AsterGraphInspectorViewFactory", StringComparison.Ordinal));
        Assert.Contains(viewModel.StandaloneSurfaceLines, line => line.Contains("AsterGraphMiniMapViewFactory", StringComparison.Ordinal));
        Assert.Contains(viewModel.PresentationLines, line => line.Contains("AsterGraphPresentationOptions", StringComparison.Ordinal));
    }

    [Fact]
    public void MainWindowViewModel_SwitchesHostShellAndRuntimeLocalizationTogether()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal("展示", viewModel.SelectedHostMenuGroupTitle);
        Assert.Equal("宿主控制抽屉", viewModel.HostDrawerCaption);
        Assert.Equal("请选择一个节点", viewModel.Editor.InspectorTitle);
        Assert.Equal("添加节点", AssertAddNodeCaption(viewModel.Editor));

        viewModel.SelectLanguage("en");

        Assert.Equal("Showcase", viewModel.SelectedHostMenuGroupTitle);
        Assert.Equal("Host Controls Drawer", viewModel.HostDrawerCaption);
        Assert.Equal("Select a node", viewModel.Editor.InspectorTitle);
        Assert.Equal("Add Node", AssertAddNodeCaption(viewModel.Editor));
        Assert.Contains(viewModel.LocalizationProofLines, line => line.Contains("Host override", StringComparison.Ordinal));
    }

    private static string AssertAddNodeCaption(GraphEditorViewModel editor)
        => Assert.Single(
                editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0))),
                item => item.Id == "canvas-add-node")
            .Header;
}
