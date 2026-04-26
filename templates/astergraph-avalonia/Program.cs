using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;

namespace AsterGraphAvaloniaHost;

public sealed class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "AsterGraph Avalonia Host";
        Width = 1280;
        Height = 900;
        Content = CreateGraphView();
    }

    private static Control CreateGraphView()
    {
        var definitionId = new NodeDefinitionId("sample.node");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Sample Node",
            "Sample",
            "A native Avalonia host-owned node definition.",
            [new PortDefinition("in", "Input", new PortTypeId("signal"), "#f3b36b")],
            [new PortDefinition("out", "Output", new PortTypeId("signal"), "#6ad5c4")]));

        var document = new GraphDocument(
            "AsterGraph Host",
            "Native Avalonia starter graph.",
            [
                new GraphNode(
                    "source",
                    "Source",
                    "Sample",
                    "Source",
                    "Starter source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort("out", "Output", PortDirection.Output, "signal", "#6ad5c4", new PortTypeId("signal"))],
                    "#6ad5c4",
                    definitionId),
                new GraphNode(
                    "target",
                    "Target",
                    "Sample",
                    "Target",
                    "Starter target node.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [new GraphPort("in", "Input", PortDirection.Input, "signal", "#f3b36b", new PortTypeId("signal"))],
                    [],
                    "#f3b36b",
                    definitionId),
            ],
            [new GraphConnection("connection", "source", "out", "target", "in", "Starter Connection", "#6ad5c4")]);

        return AsterGraphHostBuilder
            .Create()
            .UseDocument(document)
            .UseCatalog(catalog)
            .UseDefaultCompatibility()
            .BuildAvaloniaView();
    }
}

public static class Program
{
    public static void Main(string[] args)
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .StartWithClassicDesktopLifetime(args);
}
