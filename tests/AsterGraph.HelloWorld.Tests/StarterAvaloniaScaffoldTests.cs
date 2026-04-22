using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using AsterGraph.Avalonia.Controls;
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
