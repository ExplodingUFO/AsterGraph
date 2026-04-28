using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Starter.Avalonia;

public sealed class StarterAvaloniaApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = StarterAvaloniaWindowFactory.Create();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public static class StarterAvaloniaAppBuilder
{
    public static AppBuilder BuildDesktopApp()
        => AppBuilder.Configure<StarterAvaloniaApp>()
            .UsePlatformDetect();
}

public static class Program
{
    public static void Main(string[] args)
        => StarterAvaloniaAppBuilder.BuildDesktopApp().StartWithClassicDesktopLifetime(args);
}

public static class StarterAvaloniaWindowFactory
{
    private const string SourceNodeId = "starter-ui-source-001";
    private const string TargetNodeId = "starter-ui-target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    public static Window Create()
        => CreateRuntimeSurface().Window;

    public static StarterAvaloniaRuntimeSurface CreateRuntimeSurface()
    {
        var builder = CreateHostBuilder();
        var editor = builder.BuildEditor();
        var view = AsterGraphAvaloniaViewFactory.Create(builder.BuildViewOptions(editor));

        return new StarterAvaloniaRuntimeSurface(
            editor,
            editor.Session,
            view,
            new Window
            {
                Title = "AsterGraph Starter (Avalonia)",
                Width = 1280,
                Height = 900,
                Content = view,
            });
    }

    public static GraphEditorViewModel CreateEditor()
        => CreateHostBuilder().BuildEditor();

    public static AsterGraphHostBuilder CreateHostBuilder()
    {
        var definitionId = new NodeDefinitionId("starter.ui.node");
        INodeCatalog catalog = CreateCatalog(definitionId);

        return AsterGraphHostBuilder
            .Create()
            .UseDocument(CreateDocument(definitionId))
            .UseCatalog(catalog)
            .UseDefaultCompatibility()
            .UseDefaultWorkbench()
            .UseChromeMode(GraphEditorViewChromeMode.Default);
    }

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Starter Node",
            "Starter",
            "Minimal shipped Avalonia starter scaffold node definition.",
            [
                new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
            ],
            [
                new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
            ]));
        return catalog;
    }

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Starter Avalonia Graph",
            "Minimal shipped Avalonia starter scaffold.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Starter Source",
                    "Starter",
                    "Source",
                    "Source node for the starter Avalonia scaffold.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [
                        new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                    ],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "Starter Target",
                    "Starter",
                    "Target",
                    "Target node for the starter Avalonia scaffold.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [
                        new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                    ],
                    [],
                    "#F3B36B",
                    definitionId),
            ],
            [
                new GraphConnection(
                    "starter-ui-connection-001",
                    SourceNodeId,
                    SourcePortId,
                    TargetNodeId,
                    TargetPortId,
                    "Starter UI Connection",
                    "#6AD5C4"),
            ]);
}

public sealed record StarterAvaloniaRuntimeSurface(
    GraphEditorViewModel Editor,
    IGraphEditorSession Session,
    GraphEditorView View,
    Window Window);
