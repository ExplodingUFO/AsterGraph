using System;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Editor.Localization;
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

        var provider = (IGraphLocalizationProvider?)Activator.CreateInstance(providerType!);
        Assert.NotNull(provider);

        Assert.Equal("请选择一个节点", provider!.GetString("editor.inspector.title.none", "fallback"));
        Assert.Equal("添加节点", provider.GetString("editor.menu.canvas.addNode", "fallback"));
        Assert.Equal("fallback", provider.GetString("editor.stats.caption", "fallback"));
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
}
