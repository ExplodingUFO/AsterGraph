using System.Diagnostics;
using System.Reflection;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

const int NodeCount = 180;
const int SelectionCount = 48;
const int MoveCount = 24;
const string InputPortId = "in";
const string OutputPortId = "out";
const string ScaleAutomationRunId = "scale-proof-automation";
var definitionId = new NodeDefinitionId("scale.node");

var catalog = new NodeCatalog();
catalog.RegisterDefinition(new NodeDefinition(
    definitionId,
    "Scale Node",
    "Scale",
    "Repeatable large-graph validation node.",
    [
        new PortDefinition(InputPortId, "Input", new PortTypeId("float"), "#F3B36B"),
    ],
    [
        new PortDefinition(OutputPortId, "Output", new PortTypeId("float"), "#6AD5C4"),
    ]));

var workspace = new RecordingWorkspaceService();
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = CreateDocument(),
    NodeCatalog = catalog,
    CompatibilityService = new ExactCompatibilityService(),
    WorkspaceService = workspace,
});
var session = editor.Session;

Console.WriteLine($"SCALE_SETUP_OK:{editor.Nodes.Count}:{editor.Connections.Count}");

var selectionWatch = Stopwatch.StartNew();
editor.SetSelection(editor.Nodes.Take(SelectionCount).ToList(), editor.Nodes[SelectionCount - 1], status: null);
selectionWatch.Stop();
Console.WriteLine($"SCALE_SELECTION_OK:{editor.SelectedNodes.Count}:{editor.SelectedNode?.Id}:{selectionWatch.ElapsedMilliseconds}");

var connectionWatch = Stopwatch.StartNew();
var bridgeSourceId = editor.Nodes[NodeCount - 2].Id;
var bridgeTargetId = editor.Nodes[NodeCount - 1].Id;
var existingBridge = editor.Connections.Single(connection => connection.SourceNodeId == bridgeSourceId && connection.TargetNodeId == bridgeTargetId);
session.Commands.DeleteConnection(existingBridge.Id);
var compatibleTargets = session.Queries.GetCompatiblePortTargets(bridgeSourceId, OutputPortId);
session.Commands.StartConnection(bridgeSourceId, OutputPortId);
session.Commands.CompleteConnection(bridgeTargetId, InputPortId);
connectionWatch.Stop();
Console.WriteLine($"SCALE_CONNECTION_OK:{editor.Connections.Count}:{compatibleTargets.Count}:{connectionWatch.ElapsedMilliseconds}");

var historyOrigins = editor.Nodes
    .Take(MoveCount)
    .ToDictionary(
        node => node.Id,
        node => new GraphPoint(node.X, node.Y),
        StringComparer.Ordinal);
var historyWatch = Stopwatch.StartNew();
editor.BeginHistoryInteraction();
editor.ApplyDragOffset(historyOrigins, 42, 18);
editor.CompleteHistoryInteraction("Scale move complete.");
var dirtyAfterMove = editor.IsDirty;
editor.SaveWorkspace();
var dirtyAfterSave = editor.IsDirty;
editor.Undo();
var dirtyAfterUndo = editor.IsDirty;
editor.Redo();
var dirtyAfterRedo = editor.IsDirty;
var historyContractPassed = dirtyAfterMove
    && !dirtyAfterSave
    && dirtyAfterUndo
    && !dirtyAfterRedo;
historyWatch.Stop();
Console.WriteLine($"SCALE_HISTORY_CONTRACT_OK:{historyContractPassed}:{dirtyAfterMove}:{dirtyAfterSave}:{dirtyAfterUndo}:{dirtyAfterRedo}:{historyWatch.ElapsedMilliseconds}");

var viewportWatch = Stopwatch.StartNew();
session.Commands.UpdateViewportSize(1600, 900);
session.Commands.FitToViewport(updateStatus: false);
var viewport = session.Queries.GetViewportSnapshot();
viewportWatch.Stop();
Console.WriteLine($"SCALE_VIEWPORT_OK:{viewport.ViewportWidth}:{viewport.ViewportHeight}:{viewport.Zoom:0.00}:{viewportWatch.ElapsedMilliseconds}");

editor.SelectSingleNode(editor.Nodes[NodeCount / 2], updateStatus: false);
var inspection = session.Diagnostics.CaptureInspectionSnapshot();
Console.WriteLine($"SCALE_INSPECTION_OK:{inspection.Document.Nodes.Count}:{inspection.Document.Connections.Count}:{inspection.Selection.SelectedNodeIds.Count}:{inspection.PendingConnection.HasPendingConnection}");
Console.WriteLine($"SCALE_WORKSPACE_OK:{workspace.Exists()}:{workspace.SaveCalls}");

var automationStartedCount = 0;
var automationCompletedCount = 0;
var automationProgressCommandIds = new List<string>();
var automationGenericCommandIds = new List<string>();
session.Events.AutomationStarted += (_, args) =>
{
    if (string.Equals(args.RunId, ScaleAutomationRunId, StringComparison.Ordinal))
    {
        automationStartedCount++;
    }
};
session.Events.AutomationProgress += (_, args) =>
{
    if (string.Equals(args.RunId, ScaleAutomationRunId, StringComparison.Ordinal))
    {
        automationProgressCommandIds.Add(args.Step.CommandId);
    }
};
session.Events.AutomationCompleted += (_, args) =>
{
    if (string.Equals(args.Result.RunId, ScaleAutomationRunId, StringComparison.Ordinal))
    {
        automationCompletedCount++;
    }
};
session.Events.CommandExecuted += (_, args) =>
{
    if (string.Equals(args.MutationLabel, ScaleAutomationRunId, StringComparison.Ordinal))
    {
        automationGenericCommandIds.Add(args.CommandId);
    }
};

var scaleAutomationResult = session.Automation.Execute(CreateAutomationRunRequest());
var scaleAutomationDiagnostics = session.Diagnostics.GetRecentDiagnostics(16)
    .Where(diagnostic => diagnostic.Code.StartsWith("automation.", StringComparison.Ordinal))
    .Select(diagnostic => diagnostic.Code)
    .ToArray();
var phase25ScaleAutomationOk = scaleAutomationResult.Succeeded
    && automationStartedCount == 1
    && automationCompletedCount == 1
    && automationProgressCommandIds.SequenceEqual(
        ["selection.set", "nodes.add", "nodes.move", "viewport.resize", "connections.start", "connections.complete"],
        StringComparer.Ordinal)
    && automationGenericCommandIds.SequenceEqual(automationProgressCommandIds, StringComparer.Ordinal)
    && scaleAutomationDiagnostics.Contains("automation.run.started", StringComparer.Ordinal)
    && scaleAutomationDiagnostics.Contains("automation.run.completed", StringComparer.Ordinal)
    && scaleAutomationResult.Inspection.Document.Nodes.Count == NodeCount + 1
    && scaleAutomationResult.Inspection.Document.Connections.Count == NodeCount;
Console.WriteLine($"PHASE25_SCALE_AUTOMATION_OK:{phase25ScaleAutomationOk}:{scaleAutomationResult.ExecutedStepCount}:{scaleAutomationResult.Inspection.Document.Nodes.Count}:{scaleAutomationResult.Inspection.Document.Connections.Count}:{scaleAutomationDiagnostics.Length}");

var readinessWorkspace = new RecordingWorkspaceService("workspace://scale-smoke-readiness");
var readinessSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = CreateDocument(),
    NodeCatalog = catalog,
    CompatibilityService = new ExactCompatibilityService(),
    WorkspaceService = readinessWorkspace,
});
var readinessSelectionIds = Enumerable.Range(0, SelectionCount)
    .Select(index => $"scale-node-{index:000}")
    .ToArray();
readinessSession.Commands.UpdateViewportSize(1600, 900);
using (readinessSession.BeginMutation("scale-readiness"))
{
    readinessSession.Commands.SetSelection(readinessSelectionIds, readinessSelectionIds[^1], updateStatus: false);
    readinessSession.Commands.SetNodePositions(
        [
            new NodePositionSnapshot("scale-node-000", new GraphPoint(140, 140)),
            new NodePositionSnapshot("scale-node-001", new GraphPoint(420, 140)),
        ],
        updateStatus: false);
    readinessSession.Commands.PanBy(18, 12);
}
readinessSession.Commands.SaveWorkspace();
var readinessInspection = readinessSession.Diagnostics.CaptureInspectionSnapshot();
var readinessRecentDiagnostics = readinessSession.Diagnostics.GetRecentDiagnostics(10);
var readinessDescriptors = readinessSession.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
var readinessSessionIsKernelFirst = !readinessSession
    .GetType()
    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
    .Any(field => field.FieldType.FullName == "AsterGraph.Editor.ViewModels.GraphEditorViewModel");
var phase18ScaleReadinessOk = readinessDescriptors["query.feature-descriptors"].IsAvailable
    && readinessDescriptors["surface.mutation.batch"].IsAvailable
    && readinessDescriptors["service.workspace"].IsAvailable
    && readinessInspection.Document.Nodes.Count == NodeCount
    && readinessWorkspace.Exists();
Console.WriteLine($"PHASE18_SCALE_READINESS_OK:{readinessSessionIsKernelFirst}:{phase18ScaleReadinessOk}:{readinessInspection.FeatureDescriptors.Count}:{readinessRecentDiagnostics.Count}:{readinessWorkspace.SaveCalls}");

static GraphEditorAutomationRunRequest CreateAutomationRunRequest()
    => new(
        "scale-proof-automation",
        [
            new GraphEditorAutomationStep("select-tail", CreateAutomationCommand("selection.set", ("nodeId", "scale-node-179"), ("primaryNodeId", "scale-node-179"), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("add-node", CreateAutomationCommand("nodes.add", ("definitionId", "scale.node"), ("worldX", "3600"), ("worldY", "3000"))),
            new GraphEditorAutomationStep("move-node", CreateAutomationCommand("nodes.move", ("position", "scale-node-180|3640|3020"), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("resize-viewport", CreateAutomationCommand("viewport.resize", ("width", "1600"), ("height", "900"))),
            new GraphEditorAutomationStep("start-connection", CreateAutomationCommand("connections.start", ("sourceNodeId", "scale-node-179"), ("sourcePortId", "out"))),
            new GraphEditorAutomationStep("complete-connection", CreateAutomationCommand("connections.complete", ("targetNodeId", "scale-node-180"), ("targetPortId", "in"))),
        ]);

static GraphEditorCommandInvocationSnapshot CreateAutomationCommand(
    string commandId,
    params (string Name, string Value)[] arguments)
    => new(
        commandId,
        arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToArray());

GraphDocument CreateDocument()
{
    var nodes = new List<GraphNode>(NodeCount);
    var connections = new List<GraphConnection>(NodeCount - 1);

    for (var index = 0; index < NodeCount; index++)
    {
        var nodeId = $"scale-node-{index:000}";
        nodes.Add(new GraphNode(
            nodeId,
            $"Scale Node {index:000}",
            "Scale",
            "Large Graph",
            "Used to validate repeatable scaling scenarios.",
            new GraphPoint(120 + ((index % 12) * 280), 120 + ((index / 12) * 180)),
            new GraphSize(220, 160),
            [
                new GraphPort(InputPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
            ],
            [
                new GraphPort(OutputPortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
            ],
            index % 2 == 0 ? "#6AD5C4" : "#F3B36B",
            definitionId));

        if (index == 0)
        {
            continue;
        }

        connections.Add(new GraphConnection(
            $"scale-connection-{index - 1:000}",
            $"scale-node-{index - 1:000}",
            OutputPortId,
            nodeId,
            InputPortId,
            $"Scale Edge {index - 1:000}->{index:000}",
            "#6AD5C4"));
    }

    return new GraphDocument(
        "Scale Smoke Graph",
        "Repeatable large-graph validation scenario for v1.1 proof ring.",
        nodes,
        connections);
}

file sealed class ExactCompatibilityService : IPortCompatibilityService
{
    public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
        => sourceType == targetType
            ? PortCompatibilityResult.Exact()
            : PortCompatibilityResult.Rejected();
}

file sealed class RecordingWorkspaceService(string? workspacePath = null) : IGraphWorkspaceService
{
    public string WorkspacePath { get; } = workspacePath ?? "workspace://scale-smoke";

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
