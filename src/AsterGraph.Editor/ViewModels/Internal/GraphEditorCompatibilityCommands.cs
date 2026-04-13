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
            => _owner._sessionHost.DeleteNodeById(nodeId);

        internal void DuplicateNode(string nodeId)
            => _owner._sessionHost.DuplicateNode(nodeId);

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
