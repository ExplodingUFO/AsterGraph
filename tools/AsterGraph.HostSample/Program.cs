using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

const string SourceNodeId = "host-sample-source-001";
const string TargetNodeId = "host-sample-target-001";
const string SourcePortId = "out";
const string TargetPortId = "in";
var definitionId = new NodeDefinitionId("host.sample.node");

HostSampleAvaloniaEnvironment.EnsureInitialized();

var catalog = CreateCatalog(definitionId);
var runtimeResult = await VerifyRuntimeOnlyRouteAsync(catalog, definitionId);
var hostedUiResult = VerifyHostedUiRoute(catalog, definitionId);
var allOk = runtimeResult.IsOk && hostedUiResult.IsOk;

Console.WriteLine("HOST_SAMPLE_PATHS:CreateSession:Create:AsterGraphAvaloniaViewFactory");
Console.WriteLine($"HOST_SAMPLE_RUNTIME_OK:{runtimeResult.IsOk}:{runtimeResult.FeatureDescriptorCount}:{runtimeResult.SaveCalls}:{runtimeResult.ConnectionCount}");
Console.WriteLine($"HOST_SAMPLE_AUTOMATION_OK:{runtimeResult.AutomationOk}:{runtimeResult.AutomationStepCount}");
Console.WriteLine($"HOST_SAMPLE_CLIPBOARD_OK:{runtimeResult.ClipboardOk}:{runtimeResult.PastedNodeCount}");
Console.WriteLine($"HOST_SAMPLE_EXPORT_OK:{runtimeResult.ExportOk}:{runtimeResult.ExportPath}");
Console.WriteLine($"HOST_SAMPLE_RECONNECT_OK:{hostedUiResult.ReconnectOk}");
Console.WriteLine($"HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:{hostedUiResult.AccessibilityBaselineOk}");
Console.WriteLine($"HOST_SAMPLE_HOSTED_UI_OK:{hostedUiResult.IsOk}:{hostedUiResult.ChromeMode}:{hostedUiResult.EnableDefaultContextMenu}:{hostedUiResult.CommandShortcutPolicyEnabled}:{hostedUiResult.ConnectionCount}");
Console.WriteLine($"HOST_SAMPLE_OK:{allOk}");

if (!allOk)
{
    throw new InvalidOperationException("Host sample validation failed.");
}

static async Task<RouteResult> VerifyRuntimeOnlyRouteAsync(INodeCatalog catalog, NodeDefinitionId definitionId)
{
    var workspace = new RecordingWorkspaceService("workspace://host-sample/runtime");
    var clipboard = new RecordingTextClipboardBridge();
    var exportPath = Path.Combine(HostSampleTempPaths.CreateDirectory(), "host-sample-scene.svg");
    var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
    {
        Document = CreateDocument(definitionId),
        NodeCatalog = catalog,
        CompatibilityService = new ExactCompatibilityService(),
        WorkspaceService = workspace,
        SceneSvgExportService = new GraphSceneSvgExportService(exportPath),
        ClipboardPayloadSerializer = new GraphClipboardPayloadSerializer(),
        PlatformServices = new GraphEditorPlatformServices
        {
            TextClipboardBridge = clipboard,
        },
    });

    var automationRunIds = new List<string>();
    var automationProgressCommands = new List<string>();
    var automationCompletedStates = new List<bool>();
    session.Events.AutomationStarted += (_, args) => automationRunIds.Add(args.RunId);
    session.Events.AutomationProgress += (_, args) => automationProgressCommands.Add(args.Step.CommandId);
    session.Events.AutomationCompleted += (_, args) => automationCompletedStates.Add(args.Result.Succeeded);

    var automationResult = session.Automation.Execute(new GraphEditorAutomationRunRequest(
        "host-sample-automation",
        [
            CreateAutomationStep("select-source", "selection.set", ("nodeId", SourceNodeId), ("primaryNodeId", SourceNodeId), ("updateStatus", "false")),
            CreateAutomationStep("move-source", "nodes.move", ("position", $"{SourceNodeId}|180|180"), ("updateStatus", "false")),
            CreateAutomationStep("start-connection", "connections.start", ("sourceNodeId", SourceNodeId), ("sourcePortId", SourcePortId)),
            CreateAutomationStep("complete-connection", "connections.complete", ("targetNodeId", TargetNodeId), ("targetPortId", TargetPortId)),
        ]));
    var copied = await session.Commands.TryCopySelectionAsync();
    session.Commands.SaveWorkspace();
    var exported = session.Commands.TryExportSceneAsSvg();

    var pasteSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
    {
        Document = new GraphDocument("Clipboard Target", "Host sample clipboard target.", [], []),
        NodeCatalog = catalog,
        CompatibilityService = new ExactCompatibilityService(),
        ClipboardPayloadSerializer = new GraphClipboardPayloadSerializer(),
        PlatformServices = new GraphEditorPlatformServices
        {
            TextClipboardBridge = clipboard,
        },
    });
    var pasted = await pasteSession.Commands.TryPasteSelectionAsync();
    var snapshot = session.Queries.CreateDocumentSnapshot();
    var pastedSnapshot = pasteSession.Queries.CreateDocumentSnapshot();
    var featureDescriptors = session.Queries.GetFeatureDescriptors();
    var diagnostics = session.Diagnostics.GetRecentDiagnostics(20)
        .Select(diagnostic => diagnostic.Code)
        .ToArray();
    var automationOk = automationResult.Succeeded
        && automationResult.ExecutedStepCount == 4
        && automationResult.TotalStepCount == 4
        && automationRunIds.Count == 1
        && string.Equals(automationRunIds[0], "host-sample-automation", StringComparison.Ordinal)
        && automationCompletedStates.Count == 1
        && automationCompletedStates[0]
        && automationProgressCommands.SequenceEqual(
            ["selection.set", "nodes.move", "connections.start", "connections.complete"],
            StringComparer.Ordinal)
        && automationResult.Inspection.Document.Connections.Count == 1
        && automationResult.Inspection.Selection.PrimarySelectedNodeId == SourceNodeId
        && automationResult.Inspection.NodePositions.Any(position =>
            position.NodeId == SourceNodeId && position.Position == new GraphPoint(180, 180))
        && diagnostics.Contains("automation.run.started", StringComparer.Ordinal)
        && diagnostics.Contains("automation.run.completed", StringComparer.Ordinal);
    var isOk = snapshot.Connections.Count == 1
        && copied
        && pasted
        && pastedSnapshot.Nodes.Count == 1
        && featureDescriptors.Count > 0
        && workspace.SaveCalls == 1
        && workspace.Exists()
        && exported
        && File.Exists(exportPath)
        && automationOk;

    return new RouteResult(
        isOk,
        snapshot.Connections.Count,
        featureDescriptors.Count,
        workspace.SaveCalls,
        copied && pasted,
        pastedSnapshot.Nodes.Count,
        exported,
        exportPath,
        GraphEditorViewChromeMode.Default,
        EnableDefaultContextMenu: true,
        CommandShortcutPolicyEnabled: true,
        AutomationOk: automationOk,
        AutomationStepCount: automationResult.ExecutedStepCount);
}

static RouteResult VerifyHostedUiRoute(INodeCatalog catalog, NodeDefinitionId definitionId)
{
    var workspace = new RecordingWorkspaceService("workspace://host-sample/hosted-ui");
    var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
    {
        Document = CreateDocument(definitionId),
        NodeCatalog = catalog,
        CompatibilityService = new ExactCompatibilityService(),
        WorkspaceService = workspace,
    });
    var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
    {
        Editor = editor,
        ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        EnableDefaultContextMenu = false,
        CommandShortcutPolicy = AsterGraphCommandShortcutPolicy.Disabled,
    });
    var window = new Window
    {
        Width = 1280,
        Height = 720,
        Content = view,
    };

    try
    {
        window.Show();
        FlushUi();

        editor.ConnectPorts(SourceNodeId, SourcePortId, TargetNodeId, TargetPortId);
        var initialConnections = editor.Session.Queries.CreateDocumentSnapshot().Connections;
        var reconnectStarted = initialConnections.Count == 1
            && editor.Session.Commands.TryReconnectConnection(initialConnections[0].Id, updateStatus: false);
        var pendingAfterReconnect = editor.Session.Queries.GetPendingConnectionSnapshot();
        var connectionsAfterReconnect = editor.Session.Queries.CreateDocumentSnapshot().Connections.Count;
        editor.Session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        editor.SaveWorkspace();

        var snapshot = editor.Session.Queries.CreateDocumentSnapshot();
        var pendingAfterComplete = editor.Session.Queries.GetPendingConnectionSnapshot();
        var canvas = FindNamed<NodeCanvas>(view, "PART_NodeCanvas");
        var accessibilityBaselineOk = view.Focusable
            && string.Equals(AutomationProperties.GetName(view), "Graph editor host", StringComparison.Ordinal)
            && canvas is not null
            && canvas.Focusable
            && string.Equals(AutomationProperties.GetName(canvas), "Graph canvas", StringComparison.Ordinal);
        var reconnectOk = reconnectStarted
            && connectionsAfterReconnect == 0
            && pendingAfterReconnect.HasPendingConnection
            && pendingAfterReconnect.SourceNodeId == SourceNodeId
            && pendingAfterReconnect.SourcePortId == SourcePortId
            && !pendingAfterComplete.HasPendingConnection
            && snapshot.Connections.Count == 1;
        var isOk = view.Editor == editor
            && reconnectOk
            && workspace.SaveCalls == 1
            && workspace.Exists()
            && view.ChromeMode == GraphEditorViewChromeMode.CanvasOnly
            && !view.EnableDefaultContextMenu
            && !view.CommandShortcutPolicy.Enabled
            && accessibilityBaselineOk;

        return new RouteResult(
            isOk,
            snapshot.Connections.Count,
            editor.Session.Queries.GetFeatureDescriptors().Count,
            workspace.SaveCalls,
            ClipboardOk: true,
            PastedNodeCount: 0,
            ExportOk: true,
            ExportPath: "-",
            view.ChromeMode,
            view.EnableDefaultContextMenu,
            view.CommandShortcutPolicy.Enabled,
            ReconnectOk: reconnectOk,
            AccessibilityBaselineOk: accessibilityBaselineOk);
    }
    finally
    {
        window.Close();
    }
}

static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
{
    var catalog = new NodeCatalog();
    catalog.RegisterDefinition(new NodeDefinition(
        definitionId,
        "Host Sample Node",
        "Host Sample",
        "Minimal host sample node definition.",
        [
            new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
        ],
        [
            new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
        ]));
    return catalog;
}

static GraphDocument CreateDocument(NodeDefinitionId definitionId)
    => new(
        "Host Sample Graph",
        "Minimal canonical host-integration sample.",
        [
            new GraphNode(
                SourceNodeId,
                "Host Sample Source",
                "Host Sample",
                "Canonical Source",
                "Source node for canonical host integration.",
                new GraphPoint(120, 160),
                new GraphSize(240, 160),
                [
                    new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                ],
                [
                    new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                ],
                "#6AD5C4",
                definitionId),
            new GraphNode(
                TargetNodeId,
                "Host Sample Target",
                "Host Sample",
                "Canonical Target",
                "Target node for canonical host integration.",
                new GraphPoint(460, 160),
                new GraphSize(240, 160),
                [
                    new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                ],
                [
                    new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                ],
                "#F3B36B",
                definitionId),
        ],
        []);

static GraphEditorAutomationStep CreateAutomationStep(
    string stepId,
    string commandId,
    params (string Name, string Value)[] arguments)
    => new(stepId, CreateCommand(commandId, arguments));

static GraphEditorCommandInvocationSnapshot CreateCommand(
    string commandId,
    params (string Name, string Value)[] arguments)
    => new(
        commandId,
        arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToArray());

static T? FindNamed<T>(Control root, string name)
    where T : Control
    => root.GetVisualDescendants()
        .OfType<T>()
        .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));

static void FlushUi()
    => Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

file sealed class ExactCompatibilityService : IPortCompatibilityService
{
    public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
        => sourceType == targetType
            ? PortCompatibilityResult.Exact()
            : PortCompatibilityResult.Rejected();
}

file sealed class RecordingWorkspaceService(string workspacePath) : IGraphWorkspaceService
{
    public string WorkspacePath { get; } = workspacePath;

    public int SaveCalls { get; private set; }

    public GraphDocument? SavedDocument { get; private set; }

    public void Save(GraphDocument document)
    {
        SaveCalls++;
        SavedDocument = document;
    }

    public GraphDocument Load()
        => SavedDocument ?? throw new InvalidOperationException("No saved workspace.");

    public bool Exists()
        => SavedDocument is not null;
}

file readonly record struct RouteResult(
    bool IsOk,
    int ConnectionCount,
    int FeatureDescriptorCount,
    int SaveCalls,
    bool ClipboardOk,
    int PastedNodeCount,
    bool ExportOk,
    string ExportPath,
    GraphEditorViewChromeMode ChromeMode,
    bool EnableDefaultContextMenu,
    bool CommandShortcutPolicyEnabled,
    bool ReconnectOk = true,
    bool AutomationOk = true,
    int AutomationStepCount = 0,
    bool AccessibilityBaselineOk = true);

file sealed class RecordingTextClipboardBridge : IGraphTextClipboardBridge
{
    private string? _text;

    public Task<string?> ReadTextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_text);

    public Task WriteTextAsync(string text, CancellationToken cancellationToken = default)
    {
        _text = text;
        return Task.CompletedTask;
    }
}

file static class HostSampleAvaloniaEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        HostSampleAppBuilder.BuildAvaloniaApp().SetupWithoutStarting();
        _initialized = true;
    }
}

file sealed class HostSampleAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<HostSampleApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

file sealed class HostSampleApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}

file static class HostSampleTempPaths
{
    public static string CreateDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AsterGraph.HostSample", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
