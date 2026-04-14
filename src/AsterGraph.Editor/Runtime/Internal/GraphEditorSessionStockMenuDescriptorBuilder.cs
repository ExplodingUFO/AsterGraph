using System.Globalization;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;

namespace AsterGraph.Editor.Runtime.Internal;

internal sealed class GraphEditorSessionStockMenuDescriptorBuilder
{
    private readonly Func<IReadOnlyCollection<INodeDefinition>> _defaultDefinitionsProvider;
    private readonly Func<GraphDocument> _documentSnapshotFactory;
    private readonly Func<GraphEditorSelectionSnapshot> _selectionSnapshotFactory;
    private readonly Func<string, string, IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot>> _compatibleTargetsFactory;
    private readonly Func<string, string, string> _localize;

    public GraphEditorSessionStockMenuDescriptorBuilder(
        Func<GraphDocument> documentSnapshotFactory,
        Func<GraphEditorSelectionSnapshot> selectionSnapshotFactory,
        Func<string, string, IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot>> compatibleTargetsFactory,
        Func<string, string, string> localize,
        Func<IReadOnlyCollection<INodeDefinition>>? defaultDefinitionsProvider = null)
    {
        _documentSnapshotFactory = documentSnapshotFactory ?? throw new ArgumentNullException(nameof(documentSnapshotFactory));
        _selectionSnapshotFactory = selectionSnapshotFactory ?? throw new ArgumentNullException(nameof(selectionSnapshotFactory));
        _compatibleTargetsFactory = compatibleTargetsFactory ?? throw new ArgumentNullException(nameof(compatibleTargetsFactory));
        _localize = localize ?? throw new ArgumentNullException(nameof(localize));
        _defaultDefinitionsProvider = defaultDefinitionsProvider ?? (() => Array.Empty<INodeDefinition>());
    }

    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Build(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commands);

        return context.TargetKind switch
        {
            ContextMenuTargetKind.Canvas => BuildCanvasMenuDescriptors(context, commands),
            ContextMenuTargetKind.Selection => BuildSelectionMenuDescriptors(context, commands),
            ContextMenuTargetKind.Node => BuildNodeMenuDescriptors(context, commands),
            ContextMenuTargetKind.Port => BuildPortMenuDescriptors(context, commands),
            ContextMenuTargetKind.Connection => BuildConnectionMenuDescriptors(context, commands),
            _ => [],
        };
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildCanvasMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        var definitions = context.AvailableNodeDefinitions.Count > 0
            ? context.AvailableNodeDefinitions
            : _defaultDefinitionsProvider();
        var addNode = GetCommandDescriptor(commands, "nodes.add");
        var fitView = GetCommandDescriptor(commands, "viewport.fit");
        var resetView = GetCommandDescriptor(commands, "viewport.reset");
        var save = GetCommandDescriptor(commands, "workspace.save");
        var load = GetCommandDescriptor(commands, "workspace.load");
        var importFragment = GetCommandDescriptor(commands, "fragments.import");
        var cancelPending = GetCommandDescriptor(commands, "connections.cancel");

        var addNodeGroups = definitions
            .GroupBy(definition => definition.Category)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new GraphEditorMenuItemDescriptorSnapshot(
                $"add-category-{group.Key}",
                group.Key,
                children: group
                    .OrderBy(definition => definition.DisplayName, StringComparer.Ordinal)
                    .Select(definition => new GraphEditorMenuItemDescriptorSnapshot(
                        $"add-node-{definition.Id.Value.Replace(".", "-", StringComparison.Ordinal)}",
                        definition.DisplayName,
                        CreateCommand(
                            "nodes.add",
                            ("definitionId", definition.Id.Value),
                            ("worldX", context.WorldPosition.X.ToString(CultureInfo.InvariantCulture)),
                            ("worldY", context.WorldPosition.Y.ToString(CultureInfo.InvariantCulture))),
                        iconKey: "node",
                        isEnabled: addNode.IsEnabled,
                        disabledReason: addNode.DisabledReason))
                    .ToList()))
            .ToList();

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("canvas-add-node", Localize("editor.menu.canvas.addNode", "Add Node"), children: addNodeGroups, iconKey: "add", isEnabled: addNode.IsEnabled, disabledReason: addNode.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("canvas-sep-1"),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-fit-view", Localize("editor.menu.canvas.fitView", "Fit View"), CreateCommand("viewport.fit"), iconKey: "fit", isEnabled: fitView.IsEnabled, disabledReason: fitView.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-reset-view", Localize("editor.menu.canvas.resetView", "Reset View"), CreateCommand("viewport.reset"), iconKey: "reset", isEnabled: resetView.IsEnabled, disabledReason: resetView.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("canvas-sep-2"),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-save", Localize("editor.menu.canvas.saveSnapshot", "Save Snapshot"), CreateCommand("workspace.save"), iconKey: "save", isEnabled: save.IsEnabled, disabledReason: save.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-load", Localize("editor.menu.canvas.loadSnapshot", "Load Snapshot"), CreateCommand("workspace.load"), iconKey: "load", isEnabled: load.IsEnabled, disabledReason: load.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-import-fragment", Localize("editor.menu.canvas.importFragment", "Import Fragment"), CreateCommand("fragments.import"), iconKey: "import", isEnabled: importFragment.IsEnabled, disabledReason: importFragment.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-cancel-pending", Localize("editor.menu.canvas.cancelPendingConnection", "Cancel Pending Connection"), CreateCommand("connections.cancel"), iconKey: "cancel", isEnabled: cancelPending.IsEnabled, disabledReason: cancelPending.DisabledReason),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildSelectionMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        var selectedCount = context.SelectedNodeIds.Count > 0
            ? context.SelectedNodeIds.Count
            : _selectionSnapshotFactory().SelectedNodeIds.Count;
        var delete = GetCommandDescriptor(commands, "selection.delete");
        var export = GetCommandDescriptor(commands, "fragments.export-selection");
        var alignLeft = GetCommandDescriptor(commands, "layout.align-left");
        var alignCenter = GetCommandDescriptor(commands, "layout.align-center");
        var alignRight = GetCommandDescriptor(commands, "layout.align-right");
        var alignTop = GetCommandDescriptor(commands, "layout.align-top");
        var alignMiddle = GetCommandDescriptor(commands, "layout.align-middle");
        var alignBottom = GetCommandDescriptor(commands, "layout.align-bottom");
        var distributeHorizontal = GetCommandDescriptor(commands, "layout.distribute-horizontal");
        var distributeVertical = GetCommandDescriptor(commands, "layout.distribute-vertical");

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-delete",
                selectedCount == 1
                    ? Localize("editor.menu.selection.delete.single", "Delete Selected Node")
                    : LocalizeFormat("editor.menu.selection.delete.multiple", "Delete {0} Selected Nodes", selectedCount),
                CreateCommand("selection.delete"),
                iconKey: "delete",
                isEnabled: delete.IsEnabled,
                disabledReason: delete.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-export",
                Localize("editor.menu.selection.exportFragment", "Export Fragment"),
                CreateCommand("fragments.export-selection"),
                iconKey: "export",
                isEnabled: export.IsEnabled,
                disabledReason: export.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("selection-sep-1"),
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-align",
                Localize("editor.menu.selection.align", "Align"),
                iconKey: "align",
                children:
                [
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-left", Localize("editor.menu.selection.align.left", "Left"), CreateCommand("layout.align-left"), iconKey: "align-left", isEnabled: alignLeft.IsEnabled, disabledReason: alignLeft.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-center", Localize("editor.menu.selection.align.center", "Center"), CreateCommand("layout.align-center"), iconKey: "align-center", isEnabled: alignCenter.IsEnabled, disabledReason: alignCenter.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-right", Localize("editor.menu.selection.align.right", "Right"), CreateCommand("layout.align-right"), iconKey: "align-right", isEnabled: alignRight.IsEnabled, disabledReason: alignRight.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-top", Localize("editor.menu.selection.align.top", "Top"), CreateCommand("layout.align-top"), iconKey: "align-top", isEnabled: alignTop.IsEnabled, disabledReason: alignTop.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-middle", Localize("editor.menu.selection.align.middle", "Middle"), CreateCommand("layout.align-middle"), iconKey: "align-middle", isEnabled: alignMiddle.IsEnabled, disabledReason: alignMiddle.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-bottom", Localize("editor.menu.selection.align.bottom", "Bottom"), CreateCommand("layout.align-bottom"), iconKey: "align-bottom", isEnabled: alignBottom.IsEnabled, disabledReason: alignBottom.DisabledReason),
                ]),
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-distribute",
                Localize("editor.menu.selection.distribute", "Distribute"),
                iconKey: "distribute",
                children:
                [
                    new GraphEditorMenuItemDescriptorSnapshot("selection-distribute-horizontal", Localize("editor.menu.selection.distribute.horizontal", "Horizontally"), CreateCommand("layout.distribute-horizontal"), iconKey: "distribute-horizontal", isEnabled: distributeHorizontal.IsEnabled, disabledReason: distributeHorizontal.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-distribute-vertical", Localize("editor.menu.selection.distribute.vertical", "Vertically"), CreateCommand("layout.distribute-vertical"), iconKey: "distribute-vertical", isEnabled: distributeVertical.IsEnabled, disabledReason: distributeVertical.DisabledReason),
                ]),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildNodeMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedNodeId))
        {
            return [];
        }

        var document = _documentSnapshotFactory();
        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedNodeId, StringComparison.Ordinal));
        if (node is null)
        {
            return [];
        }

        var inspect = GetCommandDescriptor(commands, "nodes.inspect");
        var center = GetCommandDescriptor(commands, "viewport.center-node");
        var delete = GetCommandDescriptor(commands, "nodes.delete-by-id");
        var duplicate = GetCommandDescriptor(commands, "nodes.duplicate");
        var disconnectIncoming = GetCommandDescriptor(commands, "connections.disconnect-incoming");
        var disconnectOutgoing = GetCommandDescriptor(commands, "connections.disconnect-outgoing");
        var disconnectAll = GetCommandDescriptor(commands, "connections.disconnect-all");
        var connect = GetCommandDescriptor(commands, "connections.connect");
        var connectMenus = node.Outputs
            .Select(port => new GraphEditorMenuItemDescriptorSnapshot(
                $"node-connect-{node.Id}-{port.Id}",
                port.Label,
                children: BuildCompatibleTargetItems(document, node, port, commands),
                isEnabled: connect.IsEnabled,
                disabledReason: connect.DisabledReason))
            .ToList();

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("node-inspect", LocalizeFormat("editor.menu.node.inspect", "Inspect {0}", node.Title), CreateCommand("nodes.inspect", ("nodeId", node.Id)), iconKey: "inspect", isEnabled: inspect.IsEnabled, disabledReason: inspect.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("node-center", Localize("editor.menu.node.centerViewHere", "Center View Here"), CreateCommand("viewport.center-node", ("nodeId", node.Id)), iconKey: "center", isEnabled: center.IsEnabled, disabledReason: center.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("node-sep-1"),
            new GraphEditorMenuItemDescriptorSnapshot("node-delete", Localize("editor.menu.node.deleteNode", "Delete Node"), CreateCommand("nodes.delete-by-id", ("nodeId", node.Id)), iconKey: "delete", isEnabled: delete.IsEnabled, disabledReason: delete.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("node-duplicate", Localize("editor.menu.node.duplicateNode", "Duplicate Node"), CreateCommand("nodes.duplicate", ("nodeId", node.Id)), iconKey: "duplicate", isEnabled: duplicate.IsEnabled, disabledReason: duplicate.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot(
                "node-disconnect",
                Localize("editor.menu.node.disconnect", "Disconnect"),
                iconKey: "disconnect",
                children:
                [
                    new GraphEditorMenuItemDescriptorSnapshot("node-disconnect-in", Localize("editor.menu.node.disconnect.incoming", "Incoming"), CreateCommand("connections.disconnect-incoming", ("nodeId", node.Id)), iconKey: "disconnect", isEnabled: disconnectIncoming.IsEnabled, disabledReason: disconnectIncoming.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("node-disconnect-out", Localize("editor.menu.node.disconnect.outgoing", "Outgoing"), CreateCommand("connections.disconnect-outgoing", ("nodeId", node.Id)), iconKey: "disconnect", isEnabled: disconnectOutgoing.IsEnabled, disabledReason: disconnectOutgoing.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("node-disconnect-all", Localize("editor.menu.node.disconnect.all", "All"), CreateCommand("connections.disconnect-all", ("nodeId", node.Id)), iconKey: "disconnect", isEnabled: disconnectAll.IsEnabled, disabledReason: disconnectAll.DisabledReason),
                ]),
            new GraphEditorMenuItemDescriptorSnapshot("node-create-connection", Localize("editor.menu.node.createConnectionFrom", "Create Connection From"), children: connectMenus, iconKey: "connect", isEnabled: connect.IsEnabled, disabledReason: connect.DisabledReason),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildPortMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedPortNodeId) || string.IsNullOrWhiteSpace(context.ClickedPortId))
        {
            return [];
        }

        var document = _documentSnapshotFactory();
        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedPortNodeId, StringComparison.Ordinal));
        var port = node?.Inputs.Concat(node.Outputs).FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedPortId, StringComparison.Ordinal));
        if (node is null || port is null)
        {
            return [];
        }

        if (port.Direction == PortDirection.Output)
        {
            var start = GetCommandDescriptor(commands, "connections.start");
            var connect = GetCommandDescriptor(commands, "connections.connect");
            var compatibleTargets = BuildCompatibleTargetItems(document, node, port, commands);
            return
            [
                new GraphEditorMenuItemDescriptorSnapshot("port-start", Localize("editor.menu.port.startConnection", "Start Connection"), CreateCommand("connections.start", ("sourceNodeId", node.Id), ("sourcePortId", port.Id)), iconKey: "connect", isEnabled: start.IsEnabled, disabledReason: start.DisabledReason),
                new GraphEditorMenuItemDescriptorSnapshot("port-compatible-targets", Localize("editor.menu.port.compatibleTargets", "Compatible Targets"), children: compatibleTargets, iconKey: "compatible", isEnabled: connect.IsEnabled && compatibleTargets.Count > 0, disabledReason: connect.DisabledReason),
                GraphEditorMenuItemDescriptorSnapshot.Separator("port-sep-1"),
                new GraphEditorMenuItemDescriptorSnapshot("port-info", LocalizeFormat("editor.menu.port.typeInfo", "Type: {0}", port.TypeId), iconKey: "type", isEnabled: false),
            ];
        }

        var breakPort = GetCommandDescriptor(commands, "connections.break-port");
        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("port-break-connections", Localize("editor.menu.port.breakConnections", "Break Connections"), CreateCommand("connections.break-port", ("nodeId", node.Id), ("portId", port.Id)), iconKey: "disconnect", isEnabled: breakPort.IsEnabled, disabledReason: breakPort.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("port-sep-2"),
            new GraphEditorMenuItemDescriptorSnapshot("port-info", LocalizeFormat("editor.menu.port.typeInfo", "Type: {0}", port.TypeId), iconKey: "type", isEnabled: false),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildConnectionMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedConnectionId))
        {
            return [];
        }

        var document = _documentSnapshotFactory();
        var connection = document.Connections.FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedConnectionId, StringComparison.Ordinal));
        if (connection is null)
        {
            return [];
        }

        var delete = GetCommandDescriptor(commands, "connections.delete");
        var conversionLabel = connection.ConversionId is null
            ? Localize("editor.menu.connection.noImplicitConversion", "No implicit conversion")
            : LocalizeFormat("editor.menu.connection.conversion", "Conversion: {0}", connection.ConversionId.Value);

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("connection-delete", Localize("editor.menu.connection.deleteConnection", "Delete Connection"), CreateCommand("connections.delete", ("connectionId", connection.Id)), iconKey: "delete", isEnabled: delete.IsEnabled, disabledReason: delete.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("connection-conversion", conversionLabel, iconKey: "conversion", isEnabled: false),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildCompatibleTargetItems(
        GraphDocument document,
        GraphNode sourceNode,
        GraphPort sourcePort,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        var connect = GetCommandDescriptor(commands, "connections.connect");
        var targets = _compatibleTargetsFactory(sourceNode.Id, sourcePort.Id);
        if (targets.Count == 0)
        {
            return [new GraphEditorMenuItemDescriptorSnapshot("no-compatible-targets", Localize("editor.menu.compatibility.noTargets", "No Compatible Targets"), iconKey: "info", isEnabled: false)];
        }

        return targets
            .GroupBy(target => target.NodeId)
            .OrderBy(group => group.First().NodeTitle, StringComparer.Ordinal)
            .Select(group => new GraphEditorMenuItemDescriptorSnapshot(
                $"compatible-node-{group.Key}",
                GetNodeMenuHeader(document, group.Key),
                children: group
                    .OrderBy(item => item.PortLabel, StringComparer.Ordinal)
                    .Select(target => new GraphEditorMenuItemDescriptorSnapshot(
                        $"compatible-port-{target.NodeId}-{target.PortId}",
                        target.Compatibility.Kind == PortCompatibilityKind.ImplicitConversion
                            ? LocalizeFormat("editor.menu.compatibility.implicitTarget", "{0} (implicit: {1})", target.PortLabel, target.Compatibility.ConversionId!.Value)
                            : target.PortLabel,
                        CreateCommand("connections.connect", ("sourceNodeId", sourceNode.Id), ("sourcePortId", sourcePort.Id), ("targetNodeId", target.NodeId), ("targetPortId", target.PortId)),
                        iconKey: target.Compatibility.Kind == PortCompatibilityKind.ImplicitConversion ? "conversion" : "connect",
                        isEnabled: connect.IsEnabled,
                        disabledReason: connect.DisabledReason))
                    .ToList()))
            .ToList();
    }

    private string GetNodeMenuHeader(GraphDocument document, string nodeId)
    {
        var node = document.Nodes.First(candidate => string.Equals(candidate.Id, nodeId, StringComparison.Ordinal));
        var duplicateCount = document.Nodes.Count(candidate => string.Equals(candidate.Title, node.Title, StringComparison.Ordinal));
        return duplicateCount > 1
            ? $"{node.Title} [{node.Id}]"
            : node.Title;
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

    private static GraphEditorCommandDescriptorSnapshot GetCommandDescriptor(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands,
        string commandId)
        => commands.TryGetValue(commandId, out var descriptor)
            ? descriptor
            : new GraphEditorCommandDescriptorSnapshot(commandId, false);
}
