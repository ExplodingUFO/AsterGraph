using System.Windows;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Wpf.Controls;
using AsterGraph.Wpf.Hosting;

namespace AsterGraph.Starter.Wpf;

public sealed class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        HostedStarterApplication.Run();
        return 0;
    }
}

public static class HostedStarterApplication
{
    public static void Run()
    {
        var app = new Application();
        app.Run(StarterWpfWindowFactory.Create());
    }
}

public static class StarterWpfWindowFactory
{
    private const string SourceNodeId = "starter-ui-source-001";
    private const string TargetNodeId = "starter-ui-target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    public static Window Create()
        => CreateRuntimeSurface().Window;

    public static StarterWpfRuntimeSurface CreateRuntimeSurface()
    {
        var editor = CreateEditor();
        var hostedView = AsterGraphWpfViewFactory.Create(new AsterGraphWpfViewOptions
        {
            Editor = editor,
            ApplyHostServices = true,
        });

        return new StarterWpfRuntimeSurface(
            editor,
            editor.Session,
            hostedView,
            new Window
            {
                Title = "AsterGraph Starter (WPF)",
                Width = 1280,
                Height = 900,
                Content = hostedView,
            });
    }

    public static GraphEditorViewModel CreateEditor()
    {
        var definitionId = new NodeDefinitionId("starter.ui.node");
        INodeCatalog catalog = CreateCatalog(definitionId);

        return AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
    }

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Starter Node",
            "Starter",
            "Minimal shipped WPF starter scaffold node definition.",
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
            "Starter WPF Graph",
            "Minimal shipped WPF starter scaffold.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Starter Source",
                    "Starter",
                    "Source",
                    "Source node for the starter WPF scaffold.",
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
                    "Target node for the starter WPF scaffold.",
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

public sealed record StarterWpfRuntimeSurface(
    GraphEditorViewModel Editor,
    IGraphEditorSession Session,
    GraphEditorView View,
    Window Window);
