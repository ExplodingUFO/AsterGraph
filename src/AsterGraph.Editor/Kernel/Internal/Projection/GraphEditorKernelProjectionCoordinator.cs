using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelProjectionCoordinator
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelProjectionCoordinator(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
            => new(
                _owner._historyService.CanUndo && _owner._behaviorOptions.History.EnableUndoRedo && _owner._behaviorOptions.Commands.History.AllowUndo,
                _owner._historyService.CanRedo && _owner._behaviorOptions.History.EnableUndoRedo && _owner._behaviorOptions.Commands.History.AllowRedo,
                _owner._clipboardCoordinator.CanCopySelection,
                _owner._clipboardCoordinator.CanPaste,
                _owner._behaviorOptions.Commands.Workspace.AllowSave,
                _owner._behaviorOptions.Commands.Workspace.AllowLoad)
            {
                CanSetSelection = true,
                CanMoveNodes = _owner._behaviorOptions.Commands.Nodes.AllowMove,
                CanAlignSelection = _owner.CanAlignSelection(),
                CanDistributeSelection = _owner.CanDistributeSelection(),
                CanEditNodeParameters = _owner._behaviorOptions.Commands.Nodes.AllowEditParameters && _owner.HasSharedSelectionDefinitionWithParameters(),
                CanCreateConnections = _owner._behaviorOptions.Commands.Connections.AllowCreate,
                CanDeleteConnections = _owner._behaviorOptions.Commands.Connections.AllowDelete,
                CanBreakConnections = _owner._behaviorOptions.Commands.Connections.AllowDisconnect,
                CanUpdateViewport = true,
                CanFitToViewport = _owner._document.Nodes.Count > 0 && _owner._viewportWidth > 0 && _owner._viewportHeight > 0,
                CanCenterViewport = _owner._viewportWidth > 0 && _owner._viewportHeight > 0,
            };

        public IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors()
            =>
            [
                new GraphEditorFeatureDescriptorSnapshot("query.feature-descriptors", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.document-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.selection-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.viewport-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.node-positions", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.pending-connection-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.compatible-port-target-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.registered-node-definitions", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.shared-selection-definition", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.selected-node-parameter-snapshots", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.compatible-target-mvvm-shim", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("service.workspace", "service", true),
                new GraphEditorFeatureDescriptorSnapshot("service.diagnostics", "service", true),
                new GraphEditorFeatureDescriptorSnapshot("surface.parameter-editing", "surface", true),
                new GraphEditorFeatureDescriptorSnapshot("surface.mutation.batch", "surface", true),
            ];
    }
}
