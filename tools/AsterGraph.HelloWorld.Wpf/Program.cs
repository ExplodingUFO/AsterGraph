using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Wpf.Controls;
using AsterGraph.Wpf.Hosting;

public sealed class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Any(static arg => string.Equals(arg, "--proof", StringComparison.OrdinalIgnoreCase)))
        {
            var result = HostedHelloWorldProof.Run();

            Console.WriteLine($"COMMAND_SURFACE_OK:{result.CommandSurfaceOk}");
            foreach (var line in result.MetricLines)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine($"HELLOWORLD_WPF_OK:{result.IsOk}");
            return 0;
        }

        HostedHelloWorldApplication.Run();
        return 0;
    }
}

public static class HostedHelloWorldApplication
{
    public static void Run()
    {
        var app = new Application();
        app.Run(HostedHelloWorldWindowFactory.Create());
    }
}

public static class HostedHelloWorldWindowFactory
{
    private const string SourceNodeId = "hello-ui-source-001";
    private const string TargetNodeId = "hello-ui-target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    public static Window Create()
        => CreateRuntimeSurface().Window;

    public static HostedHelloWorldRuntimeSurface CreateRuntimeSurface()
    {
        var editor = CreateEditor();
        var hostedView = AsterGraphWpfViewFactory.Create(new AsterGraphWpfViewOptions
        {
            Editor = editor,
            ApplyHostServices = true,
        });

        return new HostedHelloWorldRuntimeSurface(
            editor,
            editor.Session,
            hostedView,
            new Window
            {
                Title = "AsterGraph HelloWorld (WPF)",
                Width = 1280,
                Height = 900,
                Content = hostedView,
            });
    }

    public static GraphEditorViewModel CreateEditor()
    {
        var definitionId = new NodeDefinitionId("hello.ui.node");
        INodeCatalog catalog = CreateCatalog(definitionId);

        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        editor.Session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        return editor;
    }

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Hello World Node",
            "Hello",
            "Minimal hosted-UI node definition for WPF bootstrap.",
            [
                new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
            ],
            [
                new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
            ],
            parameters:
            [
                new NodeParameterDefinition(
                    "threshold",
                    "Threshold",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    description: "Controls when the demo source emits a stronger pulse.",
                    defaultValue: 0.5d,
                    constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
                    groupName: "Behavior"),
                new NodeParameterDefinition(
                    "mode",
                    "Mode",
                    new PortTypeId("enum"),
                    ParameterEditorKind.Enum,
                    description: "Changes how the sample node responds to threshold.",
                    defaultValue: "steady",
                    constraints: new ParameterConstraints(
                        AllowedOptions:
                        [
                            new ParameterOptionDefinition("steady", "Steady"),
                            new ParameterOptionDefinition("pulse", "Pulse"),
                        ]),
                    groupName: "Behavior"),
            ]));
        return catalog;
    }

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Hello World UI Graph",
            "Minimal hosted-UI sample for WPF bootstrap.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Hello UI Source",
                    "Hello",
                    "Source",
                    "Source node for the minimal hosted-UI sample.",
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
                    "Hello UI Target",
                    "Hello",
                    "Target",
                    "Target node for the minimal hosted-UI sample.",
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
                    "hello-ui-connection-001",
                    SourceNodeId,
                    SourcePortId,
                    TargetNodeId,
                    TargetPortId,
                    "Hello UI Connection",
                    "#6AD5C4"),
            ]);
}

public sealed record HostedHelloWorldRuntimeSurface(
    GraphEditorViewModel Editor,
    IGraphEditorSession Session,
    GraphEditorView View,
    Window Window);

public sealed record HostedHelloWorldProofResult(
    bool CommandSurfaceOk,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs)
{
    public bool IsOk => CommandSurfaceOk;

    public IReadOnlyList<string> MetricLines =>
        [
            FormatMetric("startup_ms", StartupMs),
            FormatMetric("inspector_projection_ms", InspectorProjectionMs),
            FormatMetric("plugin_scan_ms", PluginScanMs),
            FormatMetric("command_latency_ms", CommandLatencyMs),
        ];

    private static string FormatMetric(string name, double value)
        => $"HOST_NATIVE_METRIC:{name}={value.ToString("0.###", CultureInfo.InvariantCulture)}";
}

public static class HostedHelloWorldProof
{
    public static HostedHelloWorldProofResult Run()
    {
        GraphEditorViewModel? editor = null;
        var startupMs = MeasureMilliseconds(() => editor = HostedHelloWorldWindowFactory.CreateEditor());
        if (editor is null)
        {
            throw new InvalidOperationException("HelloWorld editor was not created.");
        }

        var session = editor.Session;
        var inspectorProjectionMs = MeasureMilliseconds(() => session.Queries.GetSelectedNodeParameterSnapshots().ToArray());
        var pluginScanMs = MeasureMilliseconds(() => AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions()).ToArray());

        var nodeCountBeforeUndo = session.Queries.CreateDocumentSnapshot().Nodes.Count;
        session.Commands.AddNode(editor.NodeTemplates[0].DefinitionId, new GraphPoint(720, 220));
        var undoDescriptor = session.Queries
            .GetCommandDescriptors()
            .Single(descriptor => string.Equals(descriptor.Id, "history.undo", StringComparison.Ordinal));
        var commandLatencyMs = MeasureMilliseconds(() => session.Commands.Undo());
        var commandSurfaceOk = undoDescriptor.CanExecute
            && session.Queries.CreateDocumentSnapshot().Nodes.Count == nodeCountBeforeUndo;

        return new HostedHelloWorldProofResult(
            commandSurfaceOk,
            startupMs,
            inspectorProjectionMs,
            pluginScanMs,
            commandLatencyMs);
    }

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }
}
