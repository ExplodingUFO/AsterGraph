using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Kernel.Internal;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelConnectionMutationHost : IGraphEditorKernelConnectionMutationHost
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelConnectionMutationHost(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        GraphEditorBehaviorOptions IGraphEditorKernelConnectionMutationHost.BehaviorOptions => _owner._behaviorOptions;

        IPortCompatibilityService IGraphEditorKernelConnectionMutationHost.CompatibilityService => _owner._compatibilityService;

        GraphDocument IGraphEditorKernelConnectionMutationHost.Document => _owner.CreateActiveScopeDocumentSnapshot();

        GraphEditorPendingConnectionSnapshot IGraphEditorKernelConnectionMutationHost.PendingConnection => _owner._pendingConnection;

        string IGraphEditorKernelConnectionMutationHost.CreateConnectionId()
            => _owner.CreateConnectionId();

        void IGraphEditorKernelConnectionMutationHost.UpdateDocument(GraphDocument document)
            => _owner.ApplyActiveScopeDocument(document);

        void IGraphEditorKernelConnectionMutationHost.SetPendingConnection(GraphEditorPendingConnectionSnapshot pendingConnection)
        {
            _owner._pendingConnection = pendingConnection;
            _owner.PendingConnectionChanged?.Invoke(_owner, new GraphEditorPendingConnectionChangedEventArgs(_owner._pendingConnection));
        }

        void IGraphEditorKernelConnectionMutationHost.SetStatus(string statusMessage)
            => _owner.CurrentStatusMessage = statusMessage;

        void IGraphEditorKernelConnectionMutationHost.MarkDirty(
            string status,
            GraphEditorDocumentChangeKind changeKind,
            IReadOnlyList<string>? nodeIds,
            IReadOnlyList<string>? connectionIds,
            bool preserveStatus)
            => _owner.MarkDirty(status, changeKind, nodeIds, connectionIds, preserveStatus);

        bool IGraphEditorKernelConnectionMutationHost.CanReplaceIncomingConnection()
            => _owner.CanReplaceIncomingConnection();
    }
}
