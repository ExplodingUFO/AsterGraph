using AsterGraph.Core.Models;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal sealed class GraphEditorCompatibilityCommands
    {
        private readonly GraphEditorViewModel _owner;

        internal GraphEditorCompatibilityCommands(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        internal IReadOnlyList<MenuItemDescriptor> BuildContextMenu(ContextMenuContext context)
        {
            var stockItemDescriptors = _owner.Session.Queries.BuildContextMenuDescriptors(context);
            var stockItems = GraphContextMenuCompatibilityAdapter.Adapt(stockItemDescriptors, _owner.Session.Commands);
            if (_owner.ContextMenuAugmentor is null)
            {
                return stockItems;
            }

            try
            {
                return _owner.ContextMenuAugmentor.Augment(
                    new GraphContextMenuAugmentationContext(
                        _owner.Session,
                        context,
                        stockItems,
                        stockItemDescriptors,
                        _owner.CommandPermissions,
                        _owner));
            }
            catch (Exception exception)
            {
                _owner.SetStatus(
                    "editor.status.menu.augmentorFailed",
                    "Context menu augmentor failed: {0}. Using stock menu.",
                    exception.GetType().Name);
                _owner.PublishRecoverableFailure(
                    "contextmenu.augment.failed",
                    "contextmenu.augment",
                    _owner.StatusMessage ?? exception.Message,
                    exception);
                return stockItems;
            }
        }

        internal void DeleteNodeById(string nodeId)
        {
            if (!_owner.CommandPermissions.Nodes.AllowDelete)
            {
                _owner.SetStatus("editor.status.node.delete.disabledByPermissions", "Node deletion is disabled by host permissions.");
                return;
            }

            var node = _owner.FindNode(nodeId);
            if (node is null)
            {
                return;
            }

            var remainingSelection = _owner.SelectedNodes.Where(selected => !ReferenceEquals(selected, node)).ToList();
            var removedConnections = _owner.Connections
                .Where(connection => connection.SourceNodeId == node.Id || connection.TargetNodeId == node.Id)
                .ToList();

            if (removedConnections.Count > 0 && !_owner.CanRemoveConnectionsAsSideEffect())
            {
                _owner.SetStatus("editor.status.node.delete.singleConnectedRequiresPermission", "Deleting a connected node requires delete or disconnect permission for the affected links.");
                return;
            }

            foreach (var connection in removedConnections)
            {
                _owner.Connections.Remove(connection);
            }

            _owner.Nodes.Remove(node);
            if (_owner.PendingSourceNode?.Id == node.Id)
            {
                _owner.CancelPendingConnection();
            }

            _owner.SetSelection(remainingSelection, remainingSelection.LastOrDefault());
            _owner.MarkDirty(_owner.StatusText("editor.status.node.deletedSingle", "Deleted {0}.", node.Title));
            _owner.NotifyDocumentChanged(
                GraphEditorDocumentChangeKind.NodesRemoved,
                nodeIds: [node.Id],
                connectionIds: removedConnections.Select(connection => connection.Id).ToList());
        }

        internal void DuplicateNode(string nodeId)
        {
            if (!_owner.CommandPermissions.Nodes.AllowDuplicate)
            {
                _owner.SetStatus("editor.status.node.duplicate.disabledByPermissions", "Node duplication is disabled by host permissions.");
                return;
            }

            var node = _owner.FindNode(nodeId);
            if (node is null)
            {
                return;
            }

            var duplicate = new NodeViewModel(node.ToModel() with
            {
                Id = _owner.CreateNodeId(node.DefinitionId, node.Id),
                Position = new GraphPoint(node.X + 48, node.Y + 48),
            });

            _owner.ApplyNodePresentation(duplicate);
            _owner.Nodes.Add(duplicate);
            _owner.SelectSingleNode(duplicate);
            _owner.MarkDirty(_owner.StatusText("editor.status.node.duplicated", "Duplicated {0}.", node.Title));
            _owner.NotifyDocumentChanged(GraphEditorDocumentChangeKind.NodesAdded, nodeIds: [duplicate.Id]);
        }

        internal void DisconnectIncoming(string nodeId)
        {
            if (!_owner.CommandPermissions.Connections.AllowDisconnect)
            {
                _owner.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _owner._kernel.DisconnectIncoming(nodeId);
        }

        internal void DisconnectOutgoing(string nodeId)
        {
            if (!_owner.CommandPermissions.Connections.AllowDisconnect)
            {
                _owner.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _owner._kernel.DisconnectOutgoing(nodeId);
        }

        internal void DisconnectAll(string nodeId)
        {
            if (!_owner.CommandPermissions.Connections.AllowDisconnect)
            {
                _owner.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _owner._kernel.DisconnectAll(nodeId);
        }

        internal void BreakConnectionsForPort(string nodeId, string portId)
        {
            if (!_owner.CommandPermissions.Connections.AllowDisconnect)
            {
                _owner.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _owner._kernel.BreakConnectionsForPort(nodeId, portId);
        }

        internal void DeleteConnection(string connectionId)
        {
            if (!_owner.CommandPermissions.Connections.AllowDelete)
            {
                _owner.SetStatus("editor.status.connection.delete.disabledByPermissions", "Connection deletion is disabled by host permissions.");
                return;
            }

            var connection = _owner.FindConnection(connectionId);
            if (connection is null)
            {
                return;
            }

            _owner._kernel.DeleteConnection(connectionId);
        }
    }
}
