using System.Collections.ObjectModel;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelSelectionStateSynchronizerHost :
        IGraphEditorSelectionStateSynchronizerHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelSelectionStateSynchronizerHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        IReadOnlyList<NodeViewModel> IGraphEditorSelectionStateSynchronizerHost.AllNodes => _owner.Nodes;

        ObservableCollection<NodeViewModel> IGraphEditorSelectionStateSynchronizerHost.SelectionNodes => _owner.SelectedNodes;

        NodeViewModel? IGraphEditorSelectionStateSynchronizerHost.PrimarySelectedNode
        {
            get => _owner.SelectedNode;
            set => _owner.SelectedNode = value;
        }

        bool IGraphEditorSelectionStateSynchronizerHost.IsSelectionTrackingSuspended
            => _owner._suspendSelectionTracking;

        void IGraphEditorSelectionStateSynchronizerHost.SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode)
            => _owner.SetSelection(nodes, primaryNode);

        void IGraphEditorSelectionStateSynchronizerHost.RefreshSelectionProjection()
            => _owner.RefreshSelectionProjection();

        void IGraphEditorSelectionStateSynchronizerHost.NotifySelectionChanged()
            => _owner.NotifySelectionChanged();

        void IGraphEditorSelectionStateSynchronizerHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();
    }
}
