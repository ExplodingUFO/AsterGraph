using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

internal sealed class GraphContextMenuBuilder
{
    private readonly IGraphContextMenuHost _editor;
    private readonly Func<string, string, string> _localize;

    public GraphContextMenuBuilder(
        IGraphContextMenuHost editor,
        Func<string, string, string>? localize = null)
    {
        _editor = editor;
        _localize = localize ?? ((_, fallback) => fallback);
    }

    public IReadOnlyList<MenuItemDescriptor> Build(ContextMenuContext context)
        => context.TargetKind switch
        {
            ContextMenuTargetKind.Canvas => BuildCanvasMenu(context),
            ContextMenuTargetKind.Selection => BuildSelectionMenu(),
            ContextMenuTargetKind.Node => BuildNodeMenu(context),
            ContextMenuTargetKind.Port => BuildPortMenu(context),
            ContextMenuTargetKind.Connection => BuildConnectionMenu(context),
            _ => [],
        };

    private IReadOnlyList<MenuItemDescriptor> BuildCanvasMenu(ContextMenuContext context)
    {
        var templates = context.AvailableNodeDefinitions.Count > 0
            ? context.AvailableNodeDefinitions.Select(definition => new NodeTemplateViewModel(definition))
            : _editor.NodeTemplates;
        var permissions = _editor.CommandPermissions;

        var addNodeGroups = templates
            .GroupBy(template => template.Category)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new MenuItemDescriptor(
                $"add-category-{group.Key}",
                group.Key,
                children: group
                    .OrderBy(item => item.Title, StringComparer.Ordinal)
                    .Select(template => new MenuItemDescriptor(
                        $"add-node-{template.Key}",
                        template.Title,
                        new RelayCommand(() => _editor.AddNode(template, context.WorldPosition)),
                        iconKey: "node",
                        isEnabled: permissions.Nodes.AllowCreate))
                    .ToList()))
            .ToList();

        return
        [
            new MenuItemDescriptor("canvas-add-node", L("editor.menu.canvas.addNode", "Add Node"), children: addNodeGroups, iconKey: "add", isEnabled: permissions.Nodes.AllowCreate),
            MenuItemDescriptor.Separator("canvas-sep-1"),
            new MenuItemDescriptor("canvas-fit-view", L("editor.menu.canvas.fitView", "Fit View"), _editor.FitViewCommand, iconKey: "fit"),
            new MenuItemDescriptor("canvas-reset-view", L("editor.menu.canvas.resetView", "Reset View"), _editor.ResetViewCommand, iconKey: "reset"),
            MenuItemDescriptor.Separator("canvas-sep-2"),
            new MenuItemDescriptor(
                "canvas-save",
                L("editor.menu.canvas.saveSnapshot", "Save Snapshot"),
                _editor.SaveCommand,
                iconKey: "save",
                disabledReason: ResolveWorkspaceDisabledReason(
                    permissions.Workspace.AllowSave,
                    _editor.SaveCommand.CanExecute(null),
                    L("editor.menu.canvas.saveSnapshot.disabled.hostDenied", "Snapshot saving is disabled by host permissions."),
                    L("editor.menu.canvas.saveSnapshot.disabled.unavailable", "Snapshot saving is currently unavailable.")),
                isEnabled: permissions.Workspace.AllowSave && _editor.SaveCommand.CanExecute(null)),
            new MenuItemDescriptor("canvas-load", L("editor.menu.canvas.loadSnapshot", "Load Snapshot"), _editor.LoadCommand, iconKey: "load", isEnabled: permissions.Workspace.AllowLoad && _editor.LoadCommand.CanExecute(null)),
            new MenuItemDescriptor("canvas-import-fragment", L("editor.menu.canvas.importFragment", "Import Fragment"), _editor.ImportFragmentCommand, iconKey: "import", isEnabled: _editor.ImportFragmentCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "canvas-paste",
                L("editor.menu.canvas.paste", "Paste"),
                _editor.PasteCommand,
                iconKey: "paste",
                isEnabled: _editor.PasteCommand.CanExecute(null)),
            _editor.HasPendingConnection
                ? new MenuItemDescriptor("canvas-cancel-pending", L("editor.menu.canvas.cancelPendingConnection", "Cancel Pending Connection"), _editor.CancelPendingConnectionCommand, iconKey: "cancel")
                : new MenuItemDescriptor("canvas-cancel-pending", L("editor.menu.canvas.cancelPendingConnection", "Cancel Pending Connection"), iconKey: "cancel", isEnabled: false),
        ];
    }

    private IReadOnlyList<MenuItemDescriptor> BuildSelectionMenu()
    {
        return
        [
            new MenuItemDescriptor(
                "selection-delete",
                _editor.SelectedNodeCount == 1
                    ? L("editor.menu.selection.delete.single", "Delete Selected Node")
                    : LF("editor.menu.selection.delete.multiple", "Delete {0} Selected Nodes", _editor.SelectedNodeCount),
                _editor.DeleteSelectionCommand,
                iconKey: "delete",
                isEnabled: _editor.DeleteSelectionCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "selection-copy",
                _editor.SelectedNodeCount == 1
                    ? L("editor.menu.selection.copy.single", "Copy Selected Node")
                    : LF("editor.menu.selection.copy.multiple", "Copy {0} Selected Nodes", _editor.SelectedNodeCount),
                _editor.CopySelectionCommand,
                iconKey: "copy",
                isEnabled: _editor.CopySelectionCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "selection-export",
                L("editor.menu.selection.exportFragment", "Export Fragment"),
                _editor.ExportSelectionFragmentCommand,
                iconKey: "export",
                isEnabled: _editor.ExportSelectionFragmentCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "selection-paste",
                L("editor.menu.selection.paste", "Paste"),
                _editor.PasteCommand,
                iconKey: "paste",
                isEnabled: _editor.PasteCommand.CanExecute(null)),
            MenuItemDescriptor.Separator("selection-sep-1"),
            new MenuItemDescriptor(
                "selection-align",
                L("editor.menu.selection.align", "Align"),
                iconKey: "align",
                children:
                [
                    new MenuItemDescriptor("selection-align-left", L("editor.menu.selection.align.left", "Left"), _editor.AlignLeftCommand, iconKey: "align-left", isEnabled: _editor.AlignLeftCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-center", L("editor.menu.selection.align.center", "Center"), _editor.AlignCenterCommand, iconKey: "align-center", isEnabled: _editor.AlignCenterCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-right", L("editor.menu.selection.align.right", "Right"), _editor.AlignRightCommand, iconKey: "align-right", isEnabled: _editor.AlignRightCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-top", L("editor.menu.selection.align.top", "Top"), _editor.AlignTopCommand, iconKey: "align-top", isEnabled: _editor.AlignTopCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-middle", L("editor.menu.selection.align.middle", "Middle"), _editor.AlignMiddleCommand, iconKey: "align-middle", isEnabled: _editor.AlignMiddleCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-bottom", L("editor.menu.selection.align.bottom", "Bottom"), _editor.AlignBottomCommand, iconKey: "align-bottom", isEnabled: _editor.AlignBottomCommand.CanExecute(null)),
                ]),
            new MenuItemDescriptor(
                "selection-distribute",
                L("editor.menu.selection.distribute", "Distribute"),
                iconKey: "distribute",
                children:
                [
                    new MenuItemDescriptor("selection-distribute-horizontal", L("editor.menu.selection.distribute.horizontal", "Horizontally"), _editor.DistributeHorizontallyCommand, iconKey: "distribute-horizontal", isEnabled: _editor.DistributeHorizontallyCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-distribute-vertical", L("editor.menu.selection.distribute.vertical", "Vertically"), _editor.DistributeVerticallyCommand, iconKey: "distribute-vertical", isEnabled: _editor.DistributeVerticallyCommand.CanExecute(null)),
                ]),
        ];
    }

    private IReadOnlyList<MenuItemDescriptor> BuildNodeMenu(ContextMenuContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedNodeId))
        {
            return [];
        }

        var node = _editor.FindNode(context.ClickedNodeId);
        if (node is null)
        {
            return [];
        }

        var permissions = _editor.CommandPermissions;
        var connectMenus = node.Outputs
            .Select(port => new MenuItemDescriptor(
                $"node-connect-{node.Id}-{port.Id}",
                port.Label,
                children: BuildCompatibleTargetItems(node.Id, port.Id),
                isEnabled: permissions.Connections.AllowCreate))
            .ToList();

        return
        [
            new MenuItemDescriptor("node-inspect", LF("editor.menu.node.inspect", "Inspect {0}", node.Title), new RelayCommand(() => _editor.SelectNode(node)), iconKey: "inspect"),
            new MenuItemDescriptor("node-center", L("editor.menu.node.centerViewHere", "Center View Here"), new RelayCommand(() => _editor.CenterViewOnNode(node.Id)), iconKey: "center"),
            MenuItemDescriptor.Separator("node-sep-1"),
            new MenuItemDescriptor("node-delete", L("editor.menu.node.deleteNode", "Delete Node"), new RelayCommand(() => _editor.DeleteNodeById(node.Id)), iconKey: "delete", isEnabled: permissions.Nodes.AllowDelete),
            new MenuItemDescriptor("node-duplicate", L("editor.menu.node.duplicateNode", "Duplicate Node"), new RelayCommand(() => _editor.DuplicateNode(node.Id)), iconKey: "duplicate", isEnabled: permissions.Nodes.AllowDuplicate),
            new MenuItemDescriptor(
                "node-disconnect",
                L("editor.menu.node.disconnect", "Disconnect"),
                iconKey: "disconnect",
                isEnabled: permissions.Connections.AllowDisconnect,
                children:
                [
                    new MenuItemDescriptor("node-disconnect-in", L("editor.menu.node.disconnect.incoming", "Incoming"), new RelayCommand(() => _editor.DisconnectIncoming(node.Id)), iconKey: "disconnect", isEnabled: permissions.Connections.AllowDisconnect),
                    new MenuItemDescriptor("node-disconnect-out", L("editor.menu.node.disconnect.outgoing", "Outgoing"), new RelayCommand(() => _editor.DisconnectOutgoing(node.Id)), iconKey: "disconnect", isEnabled: permissions.Connections.AllowDisconnect),
                    new MenuItemDescriptor("node-disconnect-all", L("editor.menu.node.disconnect.all", "All"), new RelayCommand(() => _editor.DisconnectAll(node.Id)), iconKey: "disconnect", isEnabled: permissions.Connections.AllowDisconnect),
                ]),
            new MenuItemDescriptor("node-create-connection", L("editor.menu.node.createConnectionFrom", "Create Connection From"), children: connectMenus, iconKey: "connect", isEnabled: permissions.Connections.AllowCreate),
        ];
    }

    private IReadOnlyList<MenuItemDescriptor> BuildPortMenu(ContextMenuContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedPortNodeId) || string.IsNullOrWhiteSpace(context.ClickedPortId))
        {
            return [];
        }

        var node = _editor.FindNode(context.ClickedPortNodeId);
        var port = node?.GetPort(context.ClickedPortId);
        if (node is null || port is null)
        {
            return [];
        }

        if (port.Direction == PortDirection.Output)
        {
            var canCreateConnection = _editor.CommandPermissions.Connections.AllowCreate;
            var compatibleTargets = BuildCompatibleTargetItems(node.Id, port.Id);
            return
            [
                new MenuItemDescriptor("port-start", L("editor.menu.port.startConnection", "Start Connection"), new RelayCommand(() => _editor.StartConnection(node.Id, port.Id)), iconKey: "connect", isEnabled: canCreateConnection),
                new MenuItemDescriptor("port-compatible-targets", L("editor.menu.port.compatibleTargets", "Compatible Targets"), children: compatibleTargets, iconKey: "compatible", isEnabled: compatibleTargets.Count > 0),
                MenuItemDescriptor.Separator("port-sep-1"),
                new MenuItemDescriptor("port-info", LF("editor.menu.port.typeInfo", "Type: {0}", port.TypeId), iconKey: "type", isEnabled: false),
            ];
        }

        return
        [
            new MenuItemDescriptor("port-break-connections", L("editor.menu.port.breakConnections", "Break Connections"), new RelayCommand(() => _editor.BreakConnectionsForPort(node.Id, port.Id)), iconKey: "disconnect", isEnabled: _editor.CommandPermissions.Connections.AllowDisconnect),
            MenuItemDescriptor.Separator("port-sep-2"),
            new MenuItemDescriptor("port-info", LF("editor.menu.port.typeInfo", "Type: {0}", port.TypeId), iconKey: "type", isEnabled: false),
        ];
    }

    private IReadOnlyList<MenuItemDescriptor> BuildConnectionMenu(ContextMenuContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedConnectionId))
        {
            return [];
        }

        var connection = _editor.FindConnection(context.ClickedConnectionId);
        if (connection is null)
        {
            return [];
        }

        var conversionLabel = connection.ConversionId is null
            ? L("editor.menu.connection.noImplicitConversion", "No implicit conversion")
            : LF("editor.menu.connection.conversion", "Conversion: {0}", connection.ConversionId.Value);

        return
        [
            new MenuItemDescriptor("connection-disconnect", L("editor.menu.connection.disconnectConnection", "Disconnect Connection"), new RelayCommand(() => _editor.DisconnectConnection(connection.Id)), iconKey: "disconnect", isEnabled: _editor.CommandPermissions.Connections.AllowDisconnect),
            new MenuItemDescriptor("connection-conversion", conversionLabel, iconKey: "conversion", isEnabled: false),
        ];
    }

    private IReadOnlyList<MenuItemDescriptor> BuildCompatibleTargetItems(string sourceNodeId, string sourcePortId)
    {
        var targets = _editor.GetCompatibleTargets(sourceNodeId, sourcePortId);
        if (targets.Count == 0)
        {
            return [new MenuItemDescriptor("no-compatible-targets", L("editor.menu.compatibility.noTargets", "No Compatible Targets"), iconKey: "info", isEnabled: false)];
        }

        return targets
            .GroupBy(target => target.Node.Id)
            .OrderBy(group => group.First().Node.Title, StringComparer.Ordinal)
            .Select(group => new MenuItemDescriptor(
                $"compatible-node-{group.Key}",
                GetNodeMenuHeader(group.First().Node),
                children: group
                    .OrderBy(item => item.Port.Label, StringComparer.Ordinal)
                    .Select(target => new MenuItemDescriptor(
                        $"compatible-port-{target.Node.Id}-{target.Port.Id}",
                        target.Compatibility.Kind == PortCompatibilityKind.ImplicitConversion
                            ? LF("editor.menu.compatibility.implicitTarget", "{0} (implicit: {1})", target.Port.Label, target.Compatibility.ConversionId!.Value)
                            : target.Port.Label,
                        new RelayCommand(() => _editor.ConnectPorts(sourceNodeId, sourcePortId, target.Node.Id, target.Port.Id)),
                        iconKey: target.Compatibility.Kind == PortCompatibilityKind.ImplicitConversion ? "conversion" : "connect",
                        isEnabled: _editor.CommandPermissions.Connections.AllowCreate))
                    .ToList()))
            .ToList();
    }

    private string GetNodeMenuHeader(NodeViewModel node)
    {
        var duplicateCount = _editor.Nodes.Count(candidate =>
            candidate.Title.Equals(node.Title, StringComparison.Ordinal));

        return duplicateCount > 1
            ? $"{node.Title} [{node.Id}]"
            : node.Title;
    }

    private string L(string key, string fallback)
        => _localize(key, fallback);

    private string LF(string key, string fallback, params object?[] arguments)
        => string.Format(CultureInfo.InvariantCulture, L(key, fallback), arguments);

    private static string? ResolveWorkspaceDisabledReason(
        bool isAllowedByHost,
        bool canExecute,
        string hostDeniedReason,
        string unavailableReason)
    {
        if (isAllowedByHost && canExecute)
        {
            return null;
        }

        return !isAllowedByHost ? hostDeniedReason : unavailableReason;
    }
}
