using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelNodeLayoutCoordinatorHost : IGraphEditorNodeLayoutCoordinatorHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelNodeLayoutCoordinatorHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        GraphEditorCommandPermissions IGraphEditorNodeLayoutCoordinatorHost.CommandPermissions => _owner.CommandPermissions;

        IReadOnlyList<NodeViewModel> IGraphEditorNodeLayoutCoordinatorHost.Nodes => _owner.Nodes;

        IReadOnlyList<NodeViewModel> IGraphEditorNodeLayoutCoordinatorHost.SelectedNodes => _owner.SelectedNodes;

        NodeViewModel? IGraphEditorNodeLayoutCoordinatorHost.FindNode(string nodeId)
            => _owner.FindNode(nodeId);

        void IGraphEditorNodeLayoutCoordinatorHost.SetNodeMoveDisabledStatus()
            => _owner.SetStatus("editor.status.node.move.disabledByPermissions", "Node movement is disabled by host permissions.");

        void IGraphEditorNodeLayoutCoordinatorHost.SetNodeNotFoundStatus(string nodeId)
            => _owner.SetStatus("editor.status.node.notFoundById", "Node '{0}' was not found.", nodeId);

        void IGraphEditorNodeLayoutCoordinatorHost.SetNodePositionNoneProvidedStatus()
            => _owner.SetStatus("editor.status.node.position.noneProvided", "No node positions were provided.");

        void IGraphEditorNodeLayoutCoordinatorHost.SetNodePositionNoMatchesStatus()
            => _owner.SetStatus("editor.status.node.position.noMatches", "No matching nodes were found for the provided positions.");

        void IGraphEditorNodeLayoutCoordinatorHost.ApplyNodePositionUpdates(
            IReadOnlyList<NodePositionSnapshot> positions,
            bool updateStatus)
            => _owner._kernel.SetNodePositions(positions, updateStatus);
    }
}
