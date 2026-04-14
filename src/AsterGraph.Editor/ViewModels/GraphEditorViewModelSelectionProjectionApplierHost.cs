using System.Collections.ObjectModel;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelSelectionProjectionApplierHost : IGraphEditorSelectionProjectionApplierHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelSelectionProjectionApplierHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        NodeViewModel? IGraphEditorSelectionProjectionApplierHost.PrimarySelectedNode
            => _owner.SelectedNode;

        IReadOnlyList<NodeViewModel> IGraphEditorSelectionProjectionApplierHost.SelectionNodes
            => _owner.SelectedNodes;

        ObservableCollection<NodeParameterViewModel> IGraphEditorSelectionProjectionApplierHost.SelectedParameters
            => _owner.SelectedNodeParameters;

        INodeCatalog IGraphEditorSelectionProjectionApplierHost.NodeCatalog
            => _owner._nodeCatalog;

        bool IGraphEditorSelectionProjectionApplierHost.EnableBatchParameterEditing
            => _owner.BehaviorOptions.Selection.EnableBatchParameterEditing;

        bool IGraphEditorSelectionProjectionApplierHost.CanEditNodeParameters
            => _owner.CanEditNodeParameters;

        void IGraphEditorSelectionProjectionApplierHost.ApplyParameterValue(NodeParameterViewModel parameter, object? value)
            => _owner.ApplyParameterValue(parameter, value);

        IReadOnlyList<ConnectionViewModel> IGraphEditorSelectionProjectionApplierHost.GetIncomingConnections(NodeViewModel node)
            => _owner._documentProjectionApplier.GetIncomingConnections(node);

        IReadOnlyList<ConnectionViewModel> IGraphEditorSelectionProjectionApplierHost.GetOutgoingConnections(NodeViewModel node)
            => _owner._documentProjectionApplier.GetOutgoingConnections(node);

        NodeViewModel? IGraphEditorSelectionProjectionApplierHost.FindNode(string nodeId)
            => _owner.FindNode(nodeId);

        void IGraphEditorSelectionProjectionApplierHost.ApplyProjectionText(
            string inspectorConnectionsText,
            string inspectorUpstreamText,
            string inspectorDownstreamText,
            string selectionCaptionText)
        {
            _owner._inspectorConnectionsText = inspectorConnectionsText;
            _owner._inspectorUpstreamText = inspectorUpstreamText;
            _owner._inspectorDownstreamText = inspectorDownstreamText;
            _owner._selectionCaptionText = selectionCaptionText;
        }

        void IGraphEditorSelectionProjectionApplierHost.RaiseParameterProjectionPropertyChanges()
        {
            _owner.OnPropertyChanged(nameof(GraphEditorViewModel.HasEditableParameters));
            _owner.OnPropertyChanged(nameof(GraphEditorViewModel.HasBatchEditableParameters));
        }
    }
}
