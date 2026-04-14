using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal interface IGraphEditorCompatibilityCommandHost
    {
        IGraphEditorSession Session { get; }

        GraphEditorViewModel CompatibilityEditor { get; }

        IGraphContextMenuAugmentor? ContextMenuAugmentor { get; }

        GraphEditorCommandPermissions CommandPermissions { get; }

        string SetStatus(string key, string fallback, params object?[] arguments);

        void PublishRecoverableFailure(string code, string operation, string message, Exception? exception = null);

        NodeViewModel? FindNode(string nodeId);

        ConnectionViewModel? FindConnection(string connectionId);

        int CountConnectionsForNode(string nodeId);

        bool CanRemoveConnectionsAsSideEffect();

        void DeleteNodeByIdCore(string nodeId);

        void DuplicateNodeCore(string nodeId);

        void DisconnectIncomingCore(string nodeId);

        void DisconnectOutgoingCore(string nodeId);

        void DisconnectAllCore(string nodeId);

        void BreakConnectionsForPortCore(string nodeId, string portId);

        void DeleteConnectionCore(string connectionId);
    }

    internal sealed class GraphEditorCompatibilityCommands
    {
        private readonly IGraphEditorCompatibilityCommandHost _host;

        internal GraphEditorCompatibilityCommands(IGraphEditorCompatibilityCommandHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        internal IReadOnlyList<MenuItemDescriptor> BuildContextMenu(ContextMenuContext context)
        {
            var stockItemDescriptors = _host.Session.Queries.BuildContextMenuDescriptors(context);
            var stockItems = GraphContextMenuCompatibilityAdapter.Adapt(stockItemDescriptors, _host.Session.Commands);
            if (_host.ContextMenuAugmentor is null)
            {
                return stockItems;
            }

            try
            {
                return _host.ContextMenuAugmentor.Augment(
                    new GraphContextMenuAugmentationContext(
                        _host.Session,
                        context,
                        stockItems,
                        stockItemDescriptors,
                        _host.CommandPermissions,
                        _host.CompatibilityEditor));
            }
            catch (Exception exception)
            {
                var status = _host.SetStatus(
                    "editor.status.menu.augmentorFailed",
                    "Context menu augmentor failed: {0}. Using stock menu.",
                    exception.GetType().Name);
                _host.PublishRecoverableFailure(
                    "contextmenu.augment.failed",
                    "contextmenu.augment",
                    status,
                    exception);
                return stockItems;
            }
        }

        internal void DeleteNodeById(string nodeId)
        {
            if (!_host.CommandPermissions.Nodes.AllowDelete)
            {
                _host.SetStatus("editor.status.node.delete.disabledByPermissions", "Node deletion is disabled by host permissions.");
                return;
            }

            var node = _host.FindNode(nodeId);
            if (node is null)
            {
                return;
            }

            if (_host.CountConnectionsForNode(node.Id) > 0 && !_host.CanRemoveConnectionsAsSideEffect())
            {
                _host.SetStatus("editor.status.node.delete.singleConnectedRequiresPermission", "Deleting a connected node requires delete or disconnect permission for the affected links.");
                return;
            }

            _host.DeleteNodeByIdCore(nodeId);
            _host.SetStatus("editor.status.node.deletedSingle", "Deleted {0}.", node.Title);
        }

        internal void DuplicateNode(string nodeId)
        {
            if (!_host.CommandPermissions.Nodes.AllowDuplicate)
            {
                _host.SetStatus("editor.status.node.duplicate.disabledByPermissions", "Node duplication is disabled by host permissions.");
                return;
            }

            var node = _host.FindNode(nodeId);
            if (node is null)
            {
                return;
            }

            _host.DuplicateNodeCore(nodeId);
            _host.SetStatus("editor.status.node.duplicated", "Duplicated {0}.", node.Title);
        }

        internal void DisconnectIncoming(string nodeId)
        {
            if (!_host.CommandPermissions.Connections.AllowDisconnect)
            {
                _host.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _host.DisconnectIncomingCore(nodeId);
        }

        internal void DisconnectOutgoing(string nodeId)
        {
            if (!_host.CommandPermissions.Connections.AllowDisconnect)
            {
                _host.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _host.DisconnectOutgoingCore(nodeId);
        }

        internal void DisconnectAll(string nodeId)
        {
            if (!_host.CommandPermissions.Connections.AllowDisconnect)
            {
                _host.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _host.DisconnectAllCore(nodeId);
        }

        internal void BreakConnectionsForPort(string nodeId, string portId)
        {
            if (!_host.CommandPermissions.Connections.AllowDisconnect)
            {
                _host.SetStatus("editor.status.connection.disconnect.disabledByPermissions", "Disconnect is disabled by host permissions.");
                return;
            }

            _host.BreakConnectionsForPortCore(nodeId, portId);
        }

        internal void DeleteConnection(string connectionId)
        {
            if (!_host.CommandPermissions.Connections.AllowDelete)
            {
                _host.SetStatus("editor.status.connection.delete.disabledByPermissions", "Connection deletion is disabled by host permissions.");
                return;
            }

            var connection = _host.FindConnection(connectionId);
            if (connection is null)
            {
                return;
            }

            _host.DeleteConnectionCore(connectionId);
        }
    }
}
