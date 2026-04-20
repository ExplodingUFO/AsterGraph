using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Kernel.Internal;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelNodeMutationHost : IGraphEditorKernelNodeMutationHost
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelNodeMutationHost(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        GraphEditorBehaviorOptions IGraphEditorKernelNodeMutationHost.BehaviorOptions => _owner._behaviorOptions;

        INodeCatalog IGraphEditorKernelNodeMutationHost.NodeCatalog => _owner._nodeCatalog;

        GraphDocument IGraphEditorKernelNodeMutationHost.Document => _owner.CreateActiveScopeDocumentSnapshot();

        IReadOnlyList<string> IGraphEditorKernelNodeMutationHost.SelectedNodeIds => _owner._selectedNodeIds;

        GraphEditorPendingConnectionSnapshot IGraphEditorKernelNodeMutationHost.PendingConnection => _owner._pendingConnection;

        GraphPoint IGraphEditorKernelNodeMutationHost.GetViewportCenter()
            => _owner.GetViewportCenter();

        string IGraphEditorKernelNodeMutationHost.CreateNodeId(NodeDefinitionId definitionId)
            => _owner.CreateNodeId(definitionId);

        string IGraphEditorKernelNodeMutationHost.CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey)
            => _owner.CreateNodeId(definitionId, fallbackKey);

        void IGraphEditorKernelNodeMutationHost.UpdateDocument(GraphDocument document)
            => _owner.ApplyActiveScopeDocument(document);

        void IGraphEditorKernelNodeMutationHost.SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
            => _owner.SetSelection(nodeIds, primaryNodeId, updateStatus);

        void IGraphEditorKernelNodeMutationHost.CancelPendingConnection()
            => _owner.CancelPendingConnection();

        bool IGraphEditorKernelNodeMutationHost.CanRemoveConnectionsAsSideEffect()
            => _owner.CanRemoveConnectionsAsSideEffect();

        void IGraphEditorKernelNodeMutationHost.SetStatus(string statusMessage)
            => _owner.CurrentStatusMessage = statusMessage;

        void IGraphEditorKernelNodeMutationHost.MarkDirty(
            string status,
            GraphEditorDocumentChangeKind changeKind,
            IReadOnlyList<string>? nodeIds,
            IReadOnlyList<string>? connectionIds,
            bool preserveStatus)
            => _owner.MarkDirty(status, changeKind, nodeIds, connectionIds, preserveStatus);
    }
}
