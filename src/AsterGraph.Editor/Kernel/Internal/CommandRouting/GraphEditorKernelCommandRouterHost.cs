using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Kernel.Internal;
using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelCommandRouterHost : IGraphEditorKernelCommandRouterHost
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelCommandRouterHost(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        GraphEditorBehaviorOptions IGraphEditorKernelCommandRouterHost.BehaviorOptions => _owner._behaviorOptions;

        GraphDocument IGraphEditorKernelCommandRouterHost.Document => _owner._document;

        int IGraphEditorKernelCommandRouterHost.SelectedNodeCount => _owner._selectedNodeIds.Count;

        bool IGraphEditorKernelCommandRouterHost.CanUndo
            => _owner._historyService.CanUndo && _owner._behaviorOptions.History.EnableUndoRedo && _owner._behaviorOptions.Commands.History.AllowUndo;

        bool IGraphEditorKernelCommandRouterHost.CanRedo
            => _owner._historyService.CanRedo && _owner._behaviorOptions.History.EnableUndoRedo && _owner._behaviorOptions.Commands.History.AllowRedo;

        bool IGraphEditorKernelCommandRouterHost.CanEditSelectedNodeParameters
            => _owner._behaviorOptions.Commands.Nodes.AllowEditParameters && _owner.HasSharedSelectionDefinitionWithParameters();

        GraphEditorPendingConnectionSnapshot IGraphEditorKernelCommandRouterHost.PendingConnection => _owner._pendingConnection;

        double IGraphEditorKernelCommandRouterHost.ViewportWidth => _owner._viewportWidth;

        double IGraphEditorKernelCommandRouterHost.ViewportHeight => _owner._viewportHeight;

        bool IGraphEditorKernelCommandRouterHost.WorkspaceExists => _owner._workspaceService.Exists();

        void IGraphEditorKernelCommandRouterHost.Undo()
            => _owner.Undo();

        void IGraphEditorKernelCommandRouterHost.Redo()
            => _owner.Redo();

        void IGraphEditorKernelCommandRouterHost.AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition)
            => _owner.AddNode(definitionId, preferredWorldPosition);

        void IGraphEditorKernelCommandRouterHost.SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
            => _owner.SetSelection(nodeIds, primaryNodeId, updateStatus);

        void IGraphEditorKernelCommandRouterHost.DeleteSelection()
            => _owner.DeleteSelection();

        void IGraphEditorKernelCommandRouterHost.SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus)
            => _owner.SetNodePositions(positions, updateStatus);

        string IGraphEditorKernelCommandRouterHost.TryCreateNodeGroupFromSelection(string title)
            => _owner.TryCreateNodeGroupFromSelection(title);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupCollapsed(string groupId, bool isCollapsed)
            => _owner.TrySetNodeGroupCollapsed(groupId, isCollapsed);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes, bool updateStatus)
            => _owner.TrySetNodeGroupPosition(groupId, position, moveMemberNodes, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus)
            => _owner.TrySetNodeGroupExtraPadding(groupId, extraPadding, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetSelectedNodeParameterValue(string parameterKey, object? value)
            => _owner.TrySetSelectedNodeParameterValue(parameterKey, value);

        void IGraphEditorKernelCommandRouterHost.StartConnection(string sourceNodeId, string sourcePortId)
            => _owner.StartConnection(sourceNodeId, sourcePortId);

        void IGraphEditorKernelCommandRouterHost.CompleteConnection(string targetNodeId, string targetPortId)
            => _owner.CompleteConnection(targetNodeId, targetPortId);

        void IGraphEditorKernelCommandRouterHost.CancelPendingConnection()
            => _owner.CancelPendingConnection();

        void IGraphEditorKernelCommandRouterHost.DeleteConnection(string connectionId)
            => _owner.DeleteConnection(connectionId);

        void IGraphEditorKernelCommandRouterHost.BreakConnectionsForPort(string nodeId, string portId)
            => _owner.BreakConnectionsForPort(nodeId, portId);

        void IGraphEditorKernelCommandRouterHost.DisconnectIncoming(string nodeId)
            => _owner.DisconnectIncoming(nodeId);

        void IGraphEditorKernelCommandRouterHost.DisconnectOutgoing(string nodeId)
            => _owner.DisconnectOutgoing(nodeId);

        void IGraphEditorKernelCommandRouterHost.DisconnectAll(string nodeId)
            => _owner.DisconnectAll(nodeId);

        void IGraphEditorKernelCommandRouterHost.FitToViewport(bool updateStatus)
            => _owner.FitToViewport(updateStatus);

        void IGraphEditorKernelCommandRouterHost.PanBy(double deltaX, double deltaY)
            => _owner.PanBy(deltaX, deltaY);

        void IGraphEditorKernelCommandRouterHost.UpdateViewportSize(double width, double height)
            => _owner.UpdateViewportSize(width, height);

        void IGraphEditorKernelCommandRouterHost.ResetView(bool updateStatus)
            => _owner.ResetView(updateStatus);

        void IGraphEditorKernelCommandRouterHost.CenterViewOnNode(string nodeId)
            => _owner.CenterViewOnNode(nodeId);

        void IGraphEditorKernelCommandRouterHost.CenterViewAt(GraphPoint worldPoint, bool updateStatus)
            => _owner.CenterViewAt(worldPoint, updateStatus);

        void IGraphEditorKernelCommandRouterHost.SaveWorkspace()
            => _owner.SaveWorkspace();

        bool IGraphEditorKernelCommandRouterHost.LoadWorkspace()
            => _owner.LoadWorkspace();
    }
}
