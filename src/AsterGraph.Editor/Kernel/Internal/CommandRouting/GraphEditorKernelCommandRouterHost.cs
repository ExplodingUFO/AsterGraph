using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Kernel.Internal;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;

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

        GraphDocument IGraphEditorKernelCommandRouterHost.Document => _owner.CreateActiveScopeDocumentSnapshot();

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

        bool IGraphEditorKernelCommandRouterHost.CanNavigateToParentGraphScope
            => _owner.GetScopeNavigationSnapshot().CanNavigateToParent;

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

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus)
            => _owner.TrySetNodeSize(nodeId, size, updateStatus);

        string IGraphEditorKernelCommandRouterHost.TryCreateNodeGroupFromSelection(string title)
            => _owner.TryCreateNodeGroupFromSelection(title);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupCollapsed(string groupId, bool isCollapsed)
            => _owner.TrySetNodeGroupCollapsed(groupId, isCollapsed);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes, bool updateStatus)
            => _owner.TrySetNodeGroupPosition(groupId, position, moveMemberNodes, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus)
            => _owner.TrySetNodeGroupSize(groupId, size, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus)
            => _owner.TrySetNodeGroupExtraPadding(groupId, extraPadding, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus)
            => _owner.TrySetNodeGroupMemberships(changes, updateStatus);

        string IGraphEditorKernelCommandRouterHost.TryPromoteNodeGroupToComposite(string groupId, string? title, bool updateStatus)
            => _owner.TryPromoteNodeGroupToComposite(groupId, title, updateStatus);

        string IGraphEditorKernelCommandRouterHost.TryWrapSelectionToComposite(string? title, bool updateStatus)
            => _owner.TryWrapSelectionToComposite(title, updateStatus);

        string IGraphEditorKernelCommandRouterHost.TryExposeCompositePort(string compositeNodeId, string childNodeId, string childPortId, string? label, bool updateStatus)
            => _owner.TryExposeCompositePort(compositeNodeId, childNodeId, childPortId, label, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus)
            => _owner.TryUnexposeCompositePort(compositeNodeId, boundaryPortId, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus)
            => _owner.TryEnterCompositeChildGraph(compositeNodeId, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TryReturnToParentGraphScope(bool updateStatus)
            => _owner.TryReturnToParentGraphScope(updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetSelectedNodeParameterValue(string parameterKey, object? value)
            => _owner.TrySetSelectedNodeParameterValue(parameterKey, value);

        void IGraphEditorKernelCommandRouterHost.StartConnection(string sourceNodeId, string sourcePortId)
            => _owner.StartConnection(sourceNodeId, sourcePortId);

        void IGraphEditorKernelCommandRouterHost.CompleteConnection(GraphConnectionTargetRef target)
            => _owner.CompleteConnection(target);

        void IGraphEditorKernelCommandRouterHost.CancelPendingConnection()
            => _owner.CancelPendingConnection();

        void IGraphEditorKernelCommandRouterHost.DeleteConnection(string connectionId)
            => _owner.DeleteConnection(connectionId);

        bool IGraphEditorKernelCommandRouterHost.TryReconnectConnection(string connectionId, bool updateStatus)
            => _owner.TryReconnectConnection(connectionId, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus)
            => _owner.TrySetConnectionNoteText(connectionId, noteText, updateStatus);

        void IGraphEditorKernelCommandRouterHost.DisconnectConnection(string connectionId)
            => _owner.DisconnectConnection(connectionId);

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
