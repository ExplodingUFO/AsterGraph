using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

internal sealed class GraphContextMenuBuilder
{
    private readonly IGraphContextMenuHost _editor;

    public GraphContextMenuBuilder(IGraphContextMenuHost editor)
    {
        _editor = editor;
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
            new MenuItemDescriptor("canvas-add-node", "Add Node", children: addNodeGroups, iconKey: "add", isEnabled: permissions.Nodes.AllowCreate),
            MenuItemDescriptor.Separator("canvas-sep-1"),
            new MenuItemDescriptor("canvas-fit-view", "Fit View", _editor.FitViewCommand, iconKey: "fit"),
            new MenuItemDescriptor("canvas-reset-view", "Reset View", _editor.ResetViewCommand, iconKey: "reset"),
            MenuItemDescriptor.Separator("canvas-sep-2"),
            new MenuItemDescriptor("canvas-save", "Save Snapshot", _editor.SaveCommand, iconKey: "save", isEnabled: permissions.Workspace.AllowSave && _editor.SaveCommand.CanExecute(null)),
            new MenuItemDescriptor("canvas-load", "Load Snapshot", _editor.LoadCommand, iconKey: "load", isEnabled: permissions.Workspace.AllowLoad && _editor.LoadCommand.CanExecute(null)),
            new MenuItemDescriptor("canvas-import-fragment", "Import Fragment", _editor.ImportFragmentCommand, iconKey: "import", isEnabled: _editor.ImportFragmentCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "canvas-paste",
                "Paste",
                _editor.PasteCommand,
                iconKey: "paste",
                isEnabled: _editor.PasteCommand.CanExecute(null)),
            _editor.HasPendingConnection
                ? new MenuItemDescriptor("canvas-cancel-pending", "Cancel Pending Connection", _editor.CancelPendingConnectionCommand, iconKey: "cancel")
                : new MenuItemDescriptor("canvas-cancel-pending", "Cancel Pending Connection", iconKey: "cancel", isEnabled: false),
        ];
    }

    private IReadOnlyList<MenuItemDescriptor> BuildSelectionMenu()
    {
        return
        [
            new MenuItemDescriptor(
                "selection-delete",
                _editor.SelectedNodeCount == 1 ? "Delete Selected Node" : $"Delete {_editor.SelectedNodeCount} Selected Nodes",
                _editor.DeleteSelectionCommand,
                iconKey: "delete",
                isEnabled: _editor.DeleteSelectionCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "selection-copy",
                _editor.SelectedNodeCount == 1 ? "Copy Selected Node" : $"Copy {_editor.SelectedNodeCount} Selected Nodes",
                _editor.CopySelectionCommand,
                iconKey: "copy",
                isEnabled: _editor.CopySelectionCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "selection-export",
                "Export Fragment",
                _editor.ExportSelectionFragmentCommand,
                iconKey: "export",
                isEnabled: _editor.ExportSelectionFragmentCommand.CanExecute(null)),
            new MenuItemDescriptor(
                "selection-paste",
                "Paste",
                _editor.PasteCommand,
                iconKey: "paste",
                isEnabled: _editor.PasteCommand.CanExecute(null)),
            MenuItemDescriptor.Separator("selection-sep-1"),
            new MenuItemDescriptor(
                "selection-align",
                "Align",
                iconKey: "align",
                children:
                [
                    new MenuItemDescriptor("selection-align-left", "Left", _editor.AlignLeftCommand, iconKey: "align-left", isEnabled: _editor.AlignLeftCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-center", "Center", _editor.AlignCenterCommand, iconKey: "align-center", isEnabled: _editor.AlignCenterCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-right", "Right", _editor.AlignRightCommand, iconKey: "align-right", isEnabled: _editor.AlignRightCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-top", "Top", _editor.AlignTopCommand, iconKey: "align-top", isEnabled: _editor.AlignTopCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-middle", "Middle", _editor.AlignMiddleCommand, iconKey: "align-middle", isEnabled: _editor.AlignMiddleCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-align-bottom", "Bottom", _editor.AlignBottomCommand, iconKey: "align-bottom", isEnabled: _editor.AlignBottomCommand.CanExecute(null)),
                ]),
            new MenuItemDescriptor(
                "selection-distribute",
                "Distribute",
                iconKey: "distribute",
                children:
                [
                    new MenuItemDescriptor("selection-distribute-horizontal", "Horizontally", _editor.DistributeHorizontallyCommand, iconKey: "distribute-horizontal", isEnabled: _editor.DistributeHorizontallyCommand.CanExecute(null)),
                    new MenuItemDescriptor("selection-distribute-vertical", "Vertically", _editor.DistributeVerticallyCommand, iconKey: "distribute-vertical", isEnabled: _editor.DistributeVerticallyCommand.CanExecute(null)),
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
            new MenuItemDescriptor("node-inspect", $"Inspect {node.Title}", new RelayCommand(() => _editor.SelectNode(node)), iconKey: "inspect"),
            new MenuItemDescriptor("node-center", "Center View Here", new RelayCommand(() => _editor.CenterViewOnNode(node.Id)), iconKey: "center"),
            MenuItemDescriptor.Separator("node-sep-1"),
            new MenuItemDescriptor("node-delete", "Delete Node", new RelayCommand(() => _editor.DeleteNodeById(node.Id)), iconKey: "delete", isEnabled: permissions.Nodes.AllowDelete),
            new MenuItemDescriptor("node-duplicate", "Duplicate Node", new RelayCommand(() => _editor.DuplicateNode(node.Id)), iconKey: "duplicate", isEnabled: permissions.Nodes.AllowDuplicate),
            new MenuItemDescriptor(
                "node-disconnect",
                "Disconnect",
                iconKey: "disconnect",
                isEnabled: permissions.Connections.AllowDisconnect,
                children:
                [
                    new MenuItemDescriptor("node-disconnect-in", "Incoming", new RelayCommand(() => _editor.DisconnectIncoming(node.Id)), iconKey: "disconnect", isEnabled: permissions.Connections.AllowDisconnect),
                    new MenuItemDescriptor("node-disconnect-out", "Outgoing", new RelayCommand(() => _editor.DisconnectOutgoing(node.Id)), iconKey: "disconnect", isEnabled: permissions.Connections.AllowDisconnect),
                    new MenuItemDescriptor("node-disconnect-all", "All", new RelayCommand(() => _editor.DisconnectAll(node.Id)), iconKey: "disconnect", isEnabled: permissions.Connections.AllowDisconnect),
                ]),
            new MenuItemDescriptor("node-create-connection", "Create Connection From", children: connectMenus, iconKey: "connect", isEnabled: permissions.Connections.AllowCreate),
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
                new MenuItemDescriptor("port-start", "Start Connection", new RelayCommand(() => _editor.StartConnection(node.Id, port.Id)), iconKey: "connect", isEnabled: canCreateConnection),
                new MenuItemDescriptor("port-compatible-targets", "Compatible Targets", children: compatibleTargets, iconKey: "compatible", isEnabled: compatibleTargets.Count > 0),
                MenuItemDescriptor.Separator("port-sep-1"),
                new MenuItemDescriptor("port-info", $"Type: {port.TypeId}", iconKey: "type", isEnabled: false),
            ];
        }

        return
        [
            new MenuItemDescriptor("port-break-connections", "Break Connections", new RelayCommand(() => _editor.BreakConnectionsForPort(node.Id, port.Id)), iconKey: "disconnect", isEnabled: _editor.CommandPermissions.Connections.AllowDisconnect),
            MenuItemDescriptor.Separator("port-sep-2"),
            new MenuItemDescriptor("port-info", $"Type: {port.TypeId}", iconKey: "type", isEnabled: false),
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
            ? "No implicit conversion"
            : $"Conversion: {connection.ConversionId.Value}";

        return
        [
            new MenuItemDescriptor("connection-delete", "Delete Connection", new RelayCommand(() => _editor.DeleteConnection(connection.Id)), iconKey: "delete", isEnabled: _editor.CommandPermissions.Connections.AllowDelete),
            new MenuItemDescriptor("connection-conversion", conversionLabel, iconKey: "conversion", isEnabled: false),
        ];
    }

    private IReadOnlyList<MenuItemDescriptor> BuildCompatibleTargetItems(string sourceNodeId, string sourcePortId)
    {
        var targets = _editor.GetCompatibleTargets(sourceNodeId, sourcePortId);
        if (targets.Count == 0)
        {
            return [new MenuItemDescriptor("no-compatible-targets", "No Compatible Targets", iconKey: "info", isEnabled: false)];
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
                            ? $"{target.Port.Label} (implicit: {target.Compatibility.ConversionId!.Value})"
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

}
