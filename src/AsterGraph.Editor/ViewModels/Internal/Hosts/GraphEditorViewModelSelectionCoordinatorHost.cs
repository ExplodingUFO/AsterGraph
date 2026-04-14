using System.Collections.ObjectModel;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelSelectionCoordinatorHost : IGraphEditorSelectionCoordinatorHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelSelectionCoordinatorHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        IReadOnlyList<NodeViewModel> IGraphEditorSelectionCoordinatorHost.AllNodes => _owner.Nodes;

        ObservableCollection<NodeViewModel> IGraphEditorSelectionCoordinatorHost.SelectionNodes => _owner.SelectedNodes;

        NodeViewModel? IGraphEditorSelectionCoordinatorHost.PrimarySelectedNode
        {
            get => _owner.SelectedNode;
            set => _owner.SelectedNode = value;
        }

        bool IGraphEditorSelectionCoordinatorHost.HasPendingConnection => _owner.HasPendingConnection;

        bool IGraphEditorSelectionCoordinatorHost.IsApplyingKernelProjection => _owner._isApplyingKernelProjection;

        bool IGraphEditorSelectionCoordinatorHost.IsSelectionTrackingSuspended
        {
            get => _owner._suspendSelectionTracking;
            set => _owner._suspendSelectionTracking = value;
        }

        string IGraphEditorSelectionCoordinatorHost.StatusText(string key, string fallback)
            => _owner.StatusText(key, fallback);

        string IGraphEditorSelectionCoordinatorHost.StatusText(string key, string fallback, params object?[] arguments)
            => _owner.StatusText(key, fallback, arguments);

        void IGraphEditorSelectionCoordinatorHost.SetStatusMessage(string status)
            => _owner.StatusMessage = status;

        void IGraphEditorSelectionCoordinatorHost.SetKernelSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
            => _owner._kernel.SetSelection(nodeIds, primaryNodeId, updateStatus);

        void IGraphEditorSelectionCoordinatorHost.RefreshSelectionProjection()
            => _owner.RefreshSelectionProjection();

        void IGraphEditorSelectionCoordinatorHost.NotifySelectionChanged()
            => _owner.NotifySelectionChanged();

        void IGraphEditorSelectionCoordinatorHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();
    }
}
