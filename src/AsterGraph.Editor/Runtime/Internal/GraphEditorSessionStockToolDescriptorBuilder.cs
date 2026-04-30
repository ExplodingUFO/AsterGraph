using System.Globalization;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime.Internal;

internal sealed class GraphEditorSessionStockToolDescriptorBuilder
{
    private readonly Func<GraphDocument> _activeScopeDocumentSnapshotFactory;
    private readonly Func<IReadOnlyList<GraphEditorNodeSurfaceSnapshot>> _nodeSurfaceSnapshotFactory;
    private readonly Func<GraphEditorSelectionSnapshot> _selectionSnapshotFactory;
    private readonly Func<string, string, string> _localize;

    public GraphEditorSessionStockToolDescriptorBuilder(
        Func<GraphDocument> activeScopeDocumentSnapshotFactory,
        Func<GraphEditorSelectionSnapshot> selectionSnapshotFactory,
        Func<IReadOnlyList<GraphEditorNodeSurfaceSnapshot>> nodeSurfaceSnapshotFactory,
        Func<string, string, string> localize)
    {
        _activeScopeDocumentSnapshotFactory = activeScopeDocumentSnapshotFactory ?? throw new ArgumentNullException(nameof(activeScopeDocumentSnapshotFactory));
        _selectionSnapshotFactory = selectionSnapshotFactory ?? throw new ArgumentNullException(nameof(selectionSnapshotFactory));
        _nodeSurfaceSnapshotFactory = nodeSurfaceSnapshotFactory ?? throw new ArgumentNullException(nameof(nodeSurfaceSnapshotFactory));
        _localize = localize ?? throw new ArgumentNullException(nameof(localize));
    }

    public IReadOnlyList<GraphEditorToolDescriptorSnapshot> Build(
        GraphEditorToolContextSnapshot context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commands);

        return context.Kind switch
        {
            GraphEditorToolContextKind.Selection => BuildSelectionTools(context, commands),
            GraphEditorToolContextKind.Node => BuildNodeTools(context, commands),
            GraphEditorToolContextKind.Connection => BuildConnectionTools(context, commands),
            _ => [],
        };
    }

    private IReadOnlyList<GraphEditorToolDescriptorSnapshot> BuildSelectionTools(
        GraphEditorToolContextSnapshot context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        var selectedNodeIds = context.SelectedNodeIds.Count > 0
            ? context.SelectedNodeIds
            : _selectionSnapshotFactory().SelectedNodeIds;
        if (selectedNodeIds.Count == 0)
        {
            return [];
        }

        var createGroup = GetCommandDescriptor(commands, "groups.create");
        var wrapComposite = GetCommandDescriptor(commands, "composites.wrap-selection");
        var alignLeft = GetCommandDescriptor(commands, "layout.align-left");
        var alignCenter = GetCommandDescriptor(commands, "layout.align-center");
        var alignRight = GetCommandDescriptor(commands, "layout.align-right");
        var alignTop = GetCommandDescriptor(commands, "layout.align-top");
        var alignMiddle = GetCommandDescriptor(commands, "layout.align-middle");
        var alignBottom = GetCommandDescriptor(commands, "layout.align-bottom");
        var distributeHorizontal = GetCommandDescriptor(commands, "layout.distribute-horizontal");
        var distributeVertical = GetCommandDescriptor(commands, "layout.distribute-vertical");
        var snapSelection = GetCommandDescriptor(commands, "layout.snap-selection");

        return
        [
            new GraphEditorToolDescriptorSnapshot(
                "selection-create-group",
                GraphEditorToolContextKind.Selection,
                createGroup,
                CreateCommand("groups.create", ("title", "Group")),
                order: 0),
            new GraphEditorToolDescriptorSnapshot(
                "selection-wrap-composite",
                GraphEditorToolContextKind.Selection,
                wrapComposite,
                CreateCommand("composites.wrap-selection"),
                order: 10),
            new GraphEditorToolDescriptorSnapshot(
                "selection-align-left",
                GraphEditorToolContextKind.Selection,
                alignLeft,
                CreateCommand("layout.align-left"),
                order: 20),
            new GraphEditorToolDescriptorSnapshot(
                "selection-align-center",
                GraphEditorToolContextKind.Selection,
                alignCenter,
                CreateCommand("layout.align-center"),
                order: 30),
            new GraphEditorToolDescriptorSnapshot(
                "selection-align-right",
                GraphEditorToolContextKind.Selection,
                alignRight,
                CreateCommand("layout.align-right"),
                order: 40),
            new GraphEditorToolDescriptorSnapshot(
                "selection-align-top",
                GraphEditorToolContextKind.Selection,
                alignTop,
                CreateCommand("layout.align-top"),
                order: 50),
            new GraphEditorToolDescriptorSnapshot(
                "selection-align-middle",
                GraphEditorToolContextKind.Selection,
                alignMiddle,
                CreateCommand("layout.align-middle"),
                order: 60),
            new GraphEditorToolDescriptorSnapshot(
                "selection-align-bottom",
                GraphEditorToolContextKind.Selection,
                alignBottom,
                CreateCommand("layout.align-bottom"),
                order: 70),
            new GraphEditorToolDescriptorSnapshot(
                "selection-distribute-horizontal",
                GraphEditorToolContextKind.Selection,
                distributeHorizontal,
                CreateCommand("layout.distribute-horizontal"),
                order: 80),
            new GraphEditorToolDescriptorSnapshot(
                "selection-distribute-vertical",
                GraphEditorToolContextKind.Selection,
                distributeVertical,
                CreateCommand("layout.distribute-vertical"),
                order: 90),
            new GraphEditorToolDescriptorSnapshot(
                "selection-snap-grid",
                GraphEditorToolContextKind.Selection,
                snapSelection,
                CreateCommand("layout.snap-selection", ("gridSize", "20")),
                order: 100),
        ];
    }

    private IReadOnlyList<GraphEditorToolDescriptorSnapshot> BuildNodeTools(
        GraphEditorToolContextSnapshot context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.NodeId))
        {
            return [];
        }

        var document = _activeScopeDocumentSnapshotFactory();
        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, context.NodeId, StringComparison.Ordinal));
        if (node is null)
        {
            return [];
        }

        var toggleExpansion = GetCommandDescriptor(commands, "nodes.surface.expand");
        var inspect = GetCommandDescriptor(commands, "nodes.inspect");
        var center = GetCommandDescriptor(commands, "viewport.center-node");
        var delete = GetCommandDescriptor(commands, "nodes.delete-by-id");
        var duplicate = GetCommandDescriptor(commands, "nodes.duplicate");
        var disconnectIncoming = GetCommandDescriptor(commands, "connections.disconnect-incoming");
        var disconnectOutgoing = GetCommandDescriptor(commands, "connections.disconnect-outgoing");
        var disconnectAll = GetCommandDescriptor(commands, "connections.disconnect-all");
        var nodeSurface = _nodeSurfaceSnapshotFactory()
            .FirstOrDefault(snapshot => string.Equals(snapshot.NodeId, node.Id, StringComparison.Ordinal));
        var nextExpansionState = nodeSurface?.ExpansionState == GraphNodeExpansionState.Expanded
            ? GraphNodeExpansionState.Collapsed
            : GraphNodeExpansionState.Expanded;
        var items = new List<GraphEditorToolDescriptorSnapshot>
        {
            new(
                "node-inspect",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(inspect, LocalizeFormat("editor.tool.node.inspect", "Inspect {0}", node.Title)),
                CreateCommand("nodes.inspect", ("nodeId", node.Id)),
                order: 0),
            new(
                "node-center",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(center, Localize("editor.tool.node.center", "Center View Here")),
                CreateCommand("viewport.center-node", ("nodeId", node.Id)),
                order: 10),
            new(
                "node-toggle-surface-expansion",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(
                    toggleExpansion,
                    nextExpansionState == GraphNodeExpansionState.Expanded
                        ? Localize("editor.tool.node.expand", "Expand Node Card")
                        : Localize("editor.tool.node.collapse", "Collapse Node Card")),
                CreateCommand(
                    "nodes.surface.expand",
                    ("nodeId", node.Id),
                    ("expansionState", nextExpansionState.ToString())),
                order: 20),
            new(
                "node-delete",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(delete, Localize("editor.tool.node.delete", "Delete Node")),
                CreateCommand("nodes.delete-by-id", ("nodeId", node.Id)),
                order: 30),
            new(
                "node-duplicate",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(duplicate, Localize("editor.tool.node.duplicate", "Duplicate Node")),
                CreateCommand("nodes.duplicate", ("nodeId", node.Id)),
                order: 40),
            new(
                "node-disconnect-incoming",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(disconnectIncoming, Localize("editor.tool.node.disconnectIncoming", "Disconnect Incoming")),
                CreateCommand("connections.disconnect-incoming", ("nodeId", node.Id)),
                order: 50),
            new(
                "node-disconnect-outgoing",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(disconnectOutgoing, Localize("editor.tool.node.disconnectOutgoing", "Disconnect Outgoing")),
                CreateCommand("connections.disconnect-outgoing", ("nodeId", node.Id)),
                order: 60),
            new(
                "node-disconnect-all",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(disconnectAll, Localize("editor.tool.node.disconnectAll", "Disconnect All")),
                CreateCommand("connections.disconnect-all", ("nodeId", node.Id)),
                order: 70),
        };

        if (node.Composite is not null)
        {
            var enterScope = GetCommandDescriptor(commands, "scopes.enter");
            items.Add(new GraphEditorToolDescriptorSnapshot(
                "node-enter-composite-scope",
                GraphEditorToolContextKind.Node,
                CreateContextualDescriptor(enterScope, Localize("editor.tool.node.enterCompositeScope", "Enter Composite Scope")),
                CreateCommand("scopes.enter", ("compositeNodeId", node.Id)),
                order: 80));
        }

        return items;
    }

    private IReadOnlyList<GraphEditorToolDescriptorSnapshot> BuildConnectionTools(
        GraphEditorToolContextSnapshot context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.ConnectionId))
        {
            return [];
        }

        var document = _activeScopeDocumentSnapshotFactory();
        var connection = document.Connections.FirstOrDefault(candidate => string.Equals(candidate.Id, context.ConnectionId, StringComparison.Ordinal));
        if (connection is null)
        {
            return [];
        }

        var reconnect = GetCommandDescriptor(commands, "connections.reconnect");
        var disconnect = GetCommandDescriptor(commands, "connections.disconnect");
        var clearNote = GetCommandDescriptor(commands, "connections.note.set");

        var items = new List<GraphEditorToolDescriptorSnapshot>
        {
            new GraphEditorToolDescriptorSnapshot(
                "connection-reconnect",
                GraphEditorToolContextKind.Connection,
                reconnect,
                CreateCommand("connections.reconnect", ("connectionId", connection.Id)),
                order: 0),
            new GraphEditorToolDescriptorSnapshot(
                "connection-disconnect",
                GraphEditorToolContextKind.Connection,
                disconnect,
                CreateCommand("connections.disconnect", ("connectionId", connection.Id)),
                order: 10),
        };

        if (!string.IsNullOrWhiteSpace(connection.Presentation?.NoteText))
        {
            items.Add(new GraphEditorToolDescriptorSnapshot(
                "connection-clear-note",
                GraphEditorToolContextKind.Connection,
                CreateContextualDescriptor(clearNote, Localize("editor.tool.connection.clearNote", "Clear Connection Note")),
                CreateCommand("connections.note.set", ("connectionId", connection.Id), ("text", string.Empty)),
                order: 20));
        }

        return items;
    }

    private string Localize(string key, string fallback)
        => _localize(key, fallback);

    private string LocalizeFormat(string key, string fallback, params object?[] arguments)
        => string.Format(CultureInfo.InvariantCulture, Localize(key, fallback), arguments);

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            commandId,
            arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToList());

    private static GraphEditorCommandDescriptorSnapshot CreateContextualDescriptor(
        GraphEditorCommandDescriptorSnapshot descriptor,
        string title)
        => new(
            descriptor.Id,
            title,
            descriptor.Group,
            descriptor.IconKey,
            descriptor.DefaultShortcut,
            descriptor.Source,
            descriptor.IsEnabled,
            descriptor.DisabledReason,
            descriptor.RecoveryHint,
            descriptor.RecoveryCommandId);

    private static GraphEditorCommandDescriptorSnapshot GetCommandDescriptor(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands,
        string commandId)
        => commands.TryGetValue(commandId, out var descriptor)
            ? descriptor
            : new GraphEditorCommandDescriptorSnapshot(commandId, false);
}
