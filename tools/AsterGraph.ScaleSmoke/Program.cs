using System.Diagnostics;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Services;

const int NodeCount = 180;
const int SelectionCount = 48;
const int MoveCount = 24;
const string InputPortId = "in";
const string OutputPortId = "out";
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
historyWatch.Stop();
Console.WriteLine($"SCALE_HISTORY_OK:{dirtyAfterMove}:{dirtyAfterSave}:{dirtyAfterUndo}:{dirtyAfterRedo}:{historyWatch.ElapsedMilliseconds}");

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

file sealed class RecordingWorkspaceService : IGraphWorkspaceService
{
    public string WorkspacePath => "workspace://scale-smoke";

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
