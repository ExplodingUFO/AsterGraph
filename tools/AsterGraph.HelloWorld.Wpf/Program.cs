using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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

            foreach (var line in result.ProofLines)
            {
                Console.WriteLine(line);
            }

            foreach (var line in result.MetricLines)
            {
                Console.WriteLine(line);
            }
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
    bool AccessibilityBaselineOk,
    bool AccessibilityFocusOk,
    bool AccessibilityCommandSurfaceOk,
    bool AccessibilityAuthoringSurfaceOk,
    bool Adapter2PerformanceBaselineOk,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs)
{
    public bool HostedAccessibilityOk =>
        AccessibilityBaselineOk
        && AccessibilityFocusOk
        && AccessibilityCommandSurfaceOk
        && AccessibilityAuthoringSurfaceOk;

    public bool IsOk => CommandSurfaceOk && HostedAccessibilityOk && Adapter2PerformanceBaselineOk;

    public IReadOnlyList<string> ProofLines =>
        [
            $"COMMAND_SURFACE_OK:{CommandSurfaceOk}",
            $"HOSTED_ACCESSIBILITY_BASELINE_OK:{AccessibilityBaselineOk}",
            $"HOSTED_ACCESSIBILITY_FOCUS_OK:{AccessibilityFocusOk}",
            $"HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:{AccessibilityCommandSurfaceOk}",
            $"HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:{AccessibilityAuthoringSurfaceOk}",
            $"HOSTED_ACCESSIBILITY_OK:{HostedAccessibilityOk}",
            $"ADAPTER2_PERFORMANCE_BASELINE_OK:{Adapter2PerformanceBaselineOk}",
            $"HELLOWORLD_WPF_OK:{IsOk}",
        ];

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
        var (accessibilityBaselineOk, accessibilityFocusOk, accessibilityCommandSurfaceOk, accessibilityAuthoringSurfaceOk) =
            EvaluateAccessibilitySurface();
        var commandSurfaceOk = undoDescriptor.CanExecute
            && session.Queries.CreateDocumentSnapshot().Nodes.Count == nodeCountBeforeUndo;
        var adapter2PerformanceBaselineOk = startupMs > 0d
            && IsFiniteNonNegative(inspectorProjectionMs)
            && IsFiniteNonNegative(pluginScanMs)
            && IsFiniteNonNegative(commandLatencyMs);

        return new HostedHelloWorldProofResult(
            commandSurfaceOk,
            accessibilityBaselineOk,
            accessibilityFocusOk,
            accessibilityCommandSurfaceOk,
            accessibilityAuthoringSurfaceOk,
            adapter2PerformanceBaselineOk,
            startupMs,
            inspectorProjectionMs,
            pluginScanMs,
            commandLatencyMs);
    }

    private static bool IsFiniteNonNegative(double value)
        => double.IsFinite(value) && value >= 0d;

    private static (bool BaselineOk, bool FocusOk, bool CommandSurfaceOk, bool AuthoringSurfaceOk) EvaluateAccessibilitySurface()
        => RunInSta(() =>
        {
            var surface = HostedHelloWorldWindowFactory.CreateRuntimeSurface();
            var window = surface.Window;
            window.Width = 200;
            window.Height = 120;
            window.ShowInTaskbar = false;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;

            try
            {
                window.Show();
                window.Activate();
                window.Dispatcher.Invoke(() =>
                {
                }, DispatcherPriority.Background);

                var baselineOk = surface.View.Focusable
                    && string.Equals(AutomationProperties.GetName(surface.View), "Graph editor host", StringComparison.Ordinal)
                    && HasNamedRegion(surface.View, "PART_CommandBar", "Graph editor command bar")
                    && HasNamedRegion(surface.View, "PART_NodeTemplatesRegion", "Node templates")
                    && HasNamedRegion(surface.View, "PART_NodesRegion", "Node list")
                    && HasNamedRegion(surface.View, "PART_InspectorSummaryRegion", "Inspector summary");
                var focusOk = surface.View.Focus()
                    && surface.View.IsKeyboardFocusWithin
                    && surface.View.FindName("PART_UndoCommandButton") is Button undoButton
                    && undoButton.Focusable
                    && undoButton.IsTabStop
                    && HasKeyBinding(surface.View.InputBindings, Key.Z, ModifierKeys.Control, surface.Editor.UndoCommand)
                    && HasKeyBinding(surface.View.InputBindings, Key.Delete, ModifierKeys.None, surface.Editor.DeleteSelectionCommand);
                var commandSurfaceOk = HasNamedCommand(surface.View, "PART_SaveCommandButton", "Save command", surface.Editor.SaveCommand)
                    && HasNamedCommand(surface.View, "PART_UndoCommandButton", "Undo command", surface.Editor.UndoCommand)
                    && HasNamedCommand(surface.View, "PART_DeleteSelectionCommandButton", "Delete command", surface.Editor.DeleteSelectionCommand);
                var authoringSurfaceOk = HasNamedRegion(surface.View, "PART_NodeTemplatesList", "Node templates list")
                    && HasNamedRegion(surface.View, "PART_NodeList", "Node list")
                    && HasNamedRegion(surface.View, "PART_InspectorConnectionsSummary", "Connections summary");

                return (baselineOk, focusOk, commandSurfaceOk, authoringSurfaceOk);
            }
            finally
            {
                window.Close();
            }
        });

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static bool HasNamedRegion(FrameworkElement root, string name, string automationName)
        => root.FindName(name) is FrameworkElement element
            && string.Equals(AutomationProperties.GetName(element), automationName, StringComparison.Ordinal);

    private static bool HasNamedCommand(FrameworkElement root, string name, string automationName, ICommand expectedCommand)
        => root.FindName(name) is Button button
            && string.Equals(AutomationProperties.GetName(button), automationName, StringComparison.Ordinal)
            && ReferenceEquals(button.Command, expectedCommand);

    private static bool HasKeyBinding(InputBindingCollection inputBindings, Key key, ModifierKeys modifiers, ICommand expectedCommand)
        => inputBindings
            .OfType<KeyBinding>()
            .Any(binding =>
                binding.Key == key
                && binding.Modifiers == modifiers
                && ReferenceEquals(binding.Command, expectedCommand));

    private static T RunInSta<T>(Func<T> action)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            return action();
        }

        Exception? failure = null;
        T result = default!;
        using var started = new ManualResetEventSlim(false);
        using var completed = new ManualResetEventSlim(false);

        var thread = new Thread(() =>
        {
            started.Set();
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        started.Wait();
        completed.Wait();
        thread.Join();

        if (failure is not null)
        {
            throw new InvalidOperationException("WPF accessibility baseline evaluation failed.", failure);
        }

        return result;
    }
}
