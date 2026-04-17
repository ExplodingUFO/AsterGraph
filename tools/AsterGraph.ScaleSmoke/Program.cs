using System.Diagnostics;
using System.Reflection;
using AsterGraph.ScaleSmoke;
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

const string InputPortId = "in";
const string OutputPortId = "out";
const string ScaleAutomationRunId = "scale-proof-automation";
var tier = ScaleSmokeTier.Parse(args);
var definitionId = new NodeDefinitionId("scale.node");

var setupWatch = Stopwatch.StartNew();
var catalog = CreateCatalog(definitionId);
var workspace = new RecordingWorkspaceService();
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = CreateDocument(tier, definitionId),
    NodeCatalog = catalog,
    CompatibilityService = new ExactCompatibilityService(),
    WorkspaceService = workspace,
});
var session = editor.Session;
setupWatch.Stop();

Console.WriteLine($"SCALE_TIER_INFO:{tier.Id}:{tier.NodeCount}:{tier.SelectionCount}:{tier.MoveCount}:{tier.EnforceBudgets}");
Console.WriteLine($"SCALE_SETUP_OK:{editor.Nodes.Count}:{editor.Connections.Count}");

var selectionWatch = Stopwatch.StartNew();
editor.SetSelection(editor.Nodes.Take(tier.SelectionCount).ToList(), editor.Nodes[tier.SelectionCount - 1], status: null);
selectionWatch.Stop();
Console.WriteLine($"SCALE_SELECTION_OK:{editor.SelectedNodes.Count}:{editor.SelectedNode?.Id}:{selectionWatch.ElapsedMilliseconds}");

var connectionWatch = Stopwatch.StartNew();
var bridgeSourceId = editor.Nodes[tier.NodeCount - 2].Id;
var bridgeTargetId = editor.Nodes[tier.NodeCount - 1].Id;
var existingBridge = editor.Connections.Single(connection => connection.SourceNodeId == bridgeSourceId && connection.TargetNodeId == bridgeTargetId);
session.Commands.DeleteConnection(existingBridge.Id);
var compatibleTargets = session.Queries.GetCompatiblePortTargets(bridgeSourceId, OutputPortId);
session.Commands.StartConnection(bridgeSourceId, OutputPortId);
session.Commands.CompleteConnection(bridgeTargetId, InputPortId);
connectionWatch.Stop();
Console.WriteLine($"SCALE_CONNECTION_OK:{editor.Connections.Count}:{compatibleTargets.Count}:{connectionWatch.ElapsedMilliseconds}");

var historyOrigins = editor.Nodes
    .Take(tier.MoveCount)
    .ToDictionary(
        node => node.Id,
        node => new GraphPoint(node.X, node.Y),
        StringComparer.Ordinal);
var historyWatch = Stopwatch.StartNew();
editor.BeginHistoryInteraction();
editor.ApplyDragOffset(historyOrigins, 42, 18);
editor.CompleteHistoryInteraction("Scale move complete.");
var dirtyAfterMove = editor.IsDirty;
var saveWatch = Stopwatch.StartNew();
editor.SaveWorkspace();
saveWatch.Stop();
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

var reloadWatch = Stopwatch.StartNew();
var reloadedDocument = workspace.Load();
var reloadedSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = reloadedDocument,
    NodeCatalog = catalog,
    CompatibilityService = new ExactCompatibilityService(),
    WorkspaceService = new RecordingWorkspaceService("workspace://scale-smoke-reloaded"),
});
var reloadedSnapshot = reloadedSession.Queries.CreateDocumentSnapshot();
reloadWatch.Stop();
var reloadOk = reloadedSnapshot.Nodes.Count == tier.NodeCount
    && reloadedSnapshot.Connections.Count == editor.Connections.Count;

editor.SelectSingleNode(editor.Nodes[tier.NodeCount / 2], updateStatus: false);
var inspection = session.Diagnostics.CaptureInspectionSnapshot();
Console.WriteLine($"SCALE_INSPECTION_OK:{inspection.Document.Nodes.Count}:{inspection.Document.Connections.Count}:{inspection.Selection.SelectedNodeIds.Count}:{inspection.PendingConnection.HasPendingConnection}");
Console.WriteLine($"SCALE_WORKSPACE_OK:{workspace.Exists()}:{workspace.SaveCalls}:{reloadOk}:{saveWatch.ElapsedMilliseconds}:{reloadWatch.ElapsedMilliseconds}");

var metrics = new ScaleSmokeMetrics(
    setupWatch.ElapsedMilliseconds,
    selectionWatch.ElapsedMilliseconds,
    connectionWatch.ElapsedMilliseconds,
    historyWatch.ElapsedMilliseconds,
    viewportWatch.ElapsedMilliseconds,
    saveWatch.ElapsedMilliseconds,
    reloadWatch.ElapsedMilliseconds);
var budgetEvaluation = tier.Evaluate(metrics);
Console.WriteLine($"SCALE_PERF_METRICS:{tier.Id}:{metrics.SetupMs}:{metrics.SelectionMs}:{metrics.ConnectionMs}:{metrics.HistoryMs}:{metrics.ViewportMs}:{metrics.SaveMs}:{metrics.ReloadMs}");
Console.WriteLine($"SCALE_PERFORMANCE_BUDGET_OK:{tier.Id}:{budgetEvaluation.Passed}:{budgetEvaluation.FailureSummary}");

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

var scaleAutomationResult = session.Automation.Execute(CreateAutomationRunRequest(tier));
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
    && scaleAutomationResult.Inspection.Document.Nodes.Count == tier.NodeCount + 1
    && scaleAutomationResult.Inspection.Document.Connections.Count == tier.NodeCount;
Console.WriteLine($"PHASE25_SCALE_AUTOMATION_OK:{phase25ScaleAutomationOk}:{scaleAutomationResult.ExecutedStepCount}:{scaleAutomationResult.Inspection.Document.Nodes.Count}:{scaleAutomationResult.Inspection.Document.Connections.Count}:{scaleAutomationDiagnostics.Length}");

var readinessWorkspace = new RecordingWorkspaceService("workspace://scale-smoke-readiness");
var readinessSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = CreateDocument(tier, definitionId),
    NodeCatalog = catalog,
    CompatibilityService = new ExactCompatibilityService(),
    WorkspaceService = readinessWorkspace,
});
var readinessSelectionIds = Enumerable.Range(0, tier.SelectionCount)
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
    && readinessInspection.Document.Nodes.Count == tier.NodeCount
    && readinessWorkspace.Exists();
Console.WriteLine($"PHASE18_SCALE_READINESS_OK:{readinessSessionIsKernelFirst}:{phase18ScaleReadinessOk}:{readinessInspection.FeatureDescriptors.Count}:{readinessRecentDiagnostics.Count}:{readinessWorkspace.SaveCalls}");

if (tier.EnforceBudgets && !budgetEvaluation.Passed)
{
    throw new InvalidOperationException($"ScaleSmoke performance budget failed for tier '{tier.Id}': {budgetEvaluation.FailureSummary}");
}

GraphEditorAutomationRunRequest CreateAutomationRunRequest(ScaleSmokeTier tier)
{
    var tailNodeId = $"scale-node-{tier.NodeCount - 1:000}";
    var nextNodeId = $"scale-node-{tier.NodeCount:000}";

    return new GraphEditorAutomationRunRequest(
        ScaleAutomationRunId,
        [
            new GraphEditorAutomationStep("select-tail", CreateAutomationCommand("selection.set", ("nodeId", tailNodeId), ("primaryNodeId", tailNodeId), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("add-node", CreateAutomationCommand("nodes.add", ("definitionId", "scale.node"), ("worldX", "3600"), ("worldY", "3000"))),
            new GraphEditorAutomationStep("move-node", CreateAutomationCommand("nodes.move", ("position", $"{nextNodeId}|3640|3020"), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("resize-viewport", CreateAutomationCommand("viewport.resize", ("width", "1600"), ("height", "900"))),
            new GraphEditorAutomationStep("start-connection", CreateAutomationCommand("connections.start", ("sourceNodeId", tailNodeId), ("sourcePortId", "out"))),
            new GraphEditorAutomationStep("complete-connection", CreateAutomationCommand("connections.complete", ("targetNodeId", nextNodeId), ("targetPortId", "in"))),
        ]);
}

GraphEditorCommandInvocationSnapshot CreateAutomationCommand(
    string commandId,
    params (string Name, string Value)[] arguments)
    => new(
        commandId,
        arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToArray());

NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
{
    var catalog = new NodeCatalog();
    catalog.RegisterDefinition(new NodeDefinition(
        definitionId,
        "Scale Node",
        "Scale",
        "Repeatable larger-graph validation node.",
        [
            new PortDefinition(InputPortId, "Input", new PortTypeId("float"), "#F3B36B"),
        ],
        [
            new PortDefinition(OutputPortId, "Output", new PortTypeId("float"), "#6AD5C4"),
        ]));
    return catalog;
}

GraphDocument CreateDocument(ScaleSmokeTier selectedTier, NodeDefinitionId definitionId)
{
    var nodes = new List<GraphNode>(selectedTier.NodeCount);
    var connections = new List<GraphConnection>(selectedTier.NodeCount - 1);

    for (var index = 0; index < selectedTier.NodeCount; index++)
    {
        var nodeId = $"scale-node-{index:000}";
        nodes.Add(new GraphNode(
            nodeId,
            $"Scale Node {index:000}",
            "Scale",
            "Large Graph",
            "Used to validate repeatable scale, history, and reload scenarios.",
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
        $"Scale Smoke Graph ({selectedTier.Id})",
        "Repeatable larger-graph validation scenario for the public alpha proof ring.",
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
