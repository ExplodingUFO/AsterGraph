using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelCompatibilityCommandHost : IGraphEditorCompatibilityCommandHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelCompatibilityCommandHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        IGraphEditorSession IGraphEditorCompatibilityCommandHost.Session => _owner.Session;

        GraphEditorViewModel IGraphEditorCompatibilityCommandHost.CompatibilityEditor => _owner;

        IGraphContextMenuAugmentor? IGraphEditorCompatibilityCommandHost.ContextMenuAugmentor => _owner.ContextMenuAugmentor;

        GraphEditorCommandPermissions IGraphEditorCompatibilityCommandHost.CommandPermissions => _owner.CommandPermissions;

        string IGraphEditorCompatibilityCommandHost.SetStatus(string key, string fallback, params object?[] arguments)
        {
            var status = _owner.StatusText(key, fallback, arguments);
            _owner.StatusMessage = status;
            return status;
        }

        void IGraphEditorCompatibilityCommandHost.PublishRecoverableFailure(string code, string operation, string message, Exception? exception)
            => _owner.PublishRecoverableFailure(code, operation, message, exception);

        NodeViewModel? IGraphEditorCompatibilityCommandHost.FindNode(string nodeId)
            => _owner.FindNode(nodeId);

        ConnectionViewModel? IGraphEditorCompatibilityCommandHost.FindConnection(string connectionId)
            => _owner.FindConnection(connectionId);

        int IGraphEditorCompatibilityCommandHost.CountConnectionsForNode(string nodeId)
            => _owner.Connections.Count(connection => connection.SourceNodeId == nodeId || connection.TargetNodeId == nodeId);

        bool IGraphEditorCompatibilityCommandHost.CanRemoveConnectionsAsSideEffect()
            => _owner.CanRemoveConnectionsAsSideEffect();

        void IGraphEditorCompatibilityCommandHost.DeleteNodeByIdCore(string nodeId)
            => _owner._kernel.DeleteNodeById(nodeId);

        void IGraphEditorCompatibilityCommandHost.DuplicateNodeCore(string nodeId)
            => _owner._kernel.DuplicateNode(nodeId);

        void IGraphEditorCompatibilityCommandHost.DisconnectIncomingCore(string nodeId)
            => _owner._kernel.DisconnectIncoming(nodeId);

        void IGraphEditorCompatibilityCommandHost.DisconnectOutgoingCore(string nodeId)
            => _owner._kernel.DisconnectOutgoing(nodeId);

        void IGraphEditorCompatibilityCommandHost.DisconnectAllCore(string nodeId)
            => _owner._kernel.DisconnectAll(nodeId);

        void IGraphEditorCompatibilityCommandHost.BreakConnectionsForPortCore(string nodeId, string portId)
            => _owner._kernel.BreakConnectionsForPort(nodeId, portId);

        void IGraphEditorCompatibilityCommandHost.DeleteConnectionCore(string connectionId)
            => _owner._kernel.DeleteConnection(connectionId);

        void IGraphEditorCompatibilityCommandHost.DisconnectConnectionCore(string connectionId)
            => _owner._kernel.DisconnectConnection(connectionId);
    }
}
