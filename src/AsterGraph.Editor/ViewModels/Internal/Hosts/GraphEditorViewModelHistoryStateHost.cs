using AsterGraph.Core.Models;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelHistoryStateHost : IGraphEditorHistoryStateHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelHistoryStateHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        IReadOnlyList<NodeViewModel> IGraphEditorHistoryStateHost.SelectedNodes => _owner.SelectedNodes;

        NodeViewModel? IGraphEditorHistoryStateHost.SelectedNode => _owner.SelectedNode;

        string? IGraphEditorHistoryStateHost.LastSavedDocumentSignature => _owner._lastSavedDocumentSignature;

        bool IGraphEditorHistoryStateHost.IsHistoryTrackingSuspended
        {
            get => _owner._suspendHistoryTracking;
            set => _owner._suspendHistoryTracking = value;
        }

        bool IGraphEditorHistoryStateHost.IsDirtyTrackingSuspended => _owner._suspendDirtyTracking;

        GraphDocument IGraphEditorHistoryStateHost.CreateViewModelDocumentSnapshot()
            => _owner.CreateViewModelDocumentSnapshot();

        NodeViewModel? IGraphEditorHistoryStateHost.FindNode(string nodeId)
            => _owner.FindNode(nodeId);

        void IGraphEditorHistoryStateHost.LoadDocumentCore(GraphDocument document, string status, bool markClean, bool resetHistory)
            => _owner.LoadDocument(document, status, markClean, resetHistory);

        void IGraphEditorHistoryStateHost.SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode, string? status)
            => _owner.SetSelection(nodes, primaryNode, status);

        void IGraphEditorHistoryStateHost.SetStatusMessage(string status)
            => _owner.StatusMessage = status;

        void IGraphEditorHistoryStateHost.SetDirtyState(bool isDirty)
            => _owner.IsDirty = isDirty;

        void IGraphEditorHistoryStateHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();
    }
}
