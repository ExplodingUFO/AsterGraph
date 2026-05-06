using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Kernel.Internal;
using AsterGraph.Editor.Kernel.Internal.Layout;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using System.Threading;

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

        GraphEditorSelectionSnapshot IGraphEditorKernelCommandRouterHost.Selection => _owner.GetSelectionSnapshot();

        int IGraphEditorKernelCommandRouterHost.SelectedNodeCount => _owner._selectedNodeIds.Count;

        bool IGraphEditorKernelCommandRouterHost.CanUndo
            => _owner._historyService.CanUndo && _owner._behaviorOptions.History.EnableUndoRedo && _owner._behaviorOptions.Commands.History.AllowUndo;

        bool IGraphEditorKernelCommandRouterHost.CanRedo
            => _owner._historyService.CanRedo && _owner._behaviorOptions.History.EnableUndoRedo && _owner._behaviorOptions.Commands.History.AllowRedo;

        bool IGraphEditorKernelCommandRouterHost.CanCopySelection
            => _owner._clipboardCoordinator.CanCopySelection;

        bool IGraphEditorKernelCommandRouterHost.CanPaste
            => _owner._clipboardCoordinator.CanPaste;

        bool IGraphEditorKernelCommandRouterHost.CanEditSelectedNodeParameters
            => _owner._behaviorOptions.Commands.Nodes.AllowEditParameters && _owner.HasSharedSelectionDefinitionWithParameters();

        bool IGraphEditorKernelCommandRouterHost.CanAlignSelection
            => _owner.CanAlignSelection();

        bool IGraphEditorKernelCommandRouterHost.CanDistributeSelection
            => _owner.CanDistributeSelection();

        GraphEditorPendingConnectionSnapshot IGraphEditorKernelCommandRouterHost.PendingConnection => _owner._pendingConnection;

        double IGraphEditorKernelCommandRouterHost.ViewportWidth => _owner._viewportWidth;

        double IGraphEditorKernelCommandRouterHost.ViewportHeight => _owner._viewportHeight;

        bool IGraphEditorKernelCommandRouterHost.WorkspaceExists => _owner._workspaceService.Exists();

        bool IGraphEditorKernelCommandRouterHost.FragmentWorkspaceExists => _owner._fragmentWorkspaceService.Exists();

        bool IGraphEditorKernelCommandRouterHost.CanExportSceneAsSvg
            => _owner._sceneSvgExportCoordinator.CanExport;

        bool IGraphEditorKernelCommandRouterHost.CanExportSceneAsImage
            => _owner._sceneImageExportCoordinator.CanExport;

        bool IGraphEditorKernelCommandRouterHost.CanNavigateToParentGraphScope
            => _owner.GetScopeNavigationSnapshot().CanNavigateToParent;

        void IGraphEditorKernelCommandRouterHost.SetStatus(string statusMessage)
            => _owner.CurrentStatusMessage = statusMessage;

        void IGraphEditorKernelCommandRouterHost.Undo()
            => _owner.Undo();

        void IGraphEditorKernelCommandRouterHost.Redo()
            => _owner.Redo();

        void IGraphEditorKernelCommandRouterHost.AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition)
            => _owner.AddNode(definitionId, preferredWorldPosition);

        bool IGraphEditorKernelCommandRouterHost.TryInsertNodeIntoConnection(
            string connectionId,
            NodeDefinitionId definitionId,
            string inputTargetId,
            GraphConnectionTargetKind inputTargetKind,
            string outputPortId,
            GraphPoint? preferredWorldPosition)
            => _owner.TryInsertNodeIntoConnection(
                connectionId,
                definitionId,
                inputTargetId,
                inputTargetKind,
                outputPortId,
                preferredWorldPosition);

        void IGraphEditorKernelCommandRouterHost.SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
            => _owner.SetSelection(nodeIds, primaryNodeId, updateStatus);

        void IGraphEditorKernelCommandRouterHost.SelectAll(bool updateStatus)
            => _owner.SelectAll(updateStatus);

        void IGraphEditorKernelCommandRouterHost.SelectNone(bool updateStatus)
            => _owner.SelectNone(updateStatus);

        void IGraphEditorKernelCommandRouterHost.InvertSelection(bool updateStatus)
            => _owner.InvertSelection(updateStatus);

        void IGraphEditorKernelCommandRouterHost.SetConnectionSelection(IReadOnlyList<string> connectionIds, string? primaryConnectionId, bool updateStatus)
            => _owner.SetConnectionSelection(connectionIds, primaryConnectionId, updateStatus);

        void IGraphEditorKernelCommandRouterHost.DeleteNodeById(string nodeId)
            => _owner.DeleteNodeById(nodeId);

        void IGraphEditorKernelCommandRouterHost.DuplicateNode(string nodeId)
            => _owner.DuplicateNode(nodeId);

        void IGraphEditorKernelCommandRouterHost.DeleteSelection()
            => _owner.DeleteSelection();

        bool IGraphEditorKernelCommandRouterHost.TryDeleteSelectionAndReconnect()
            => _owner.TryDeleteSelectionAndReconnect();

        bool IGraphEditorKernelCommandRouterHost.TryDetachSelectionFromConnections()
            => _owner.TryDetachSelectionFromConnections();

        bool IGraphEditorKernelCommandRouterHost.TryDeleteSelectedConnections()
            => _owner.TryDeleteSelectedConnections();

        bool IGraphEditorKernelCommandRouterHost.TrySliceConnections(GraphPoint start, GraphPoint end)
            => _owner.TrySliceConnections(start, end);

        Task<bool> IGraphEditorKernelCommandRouterHost.TryCopySelectionAsync(CancellationToken cancellationToken)
            => _owner.TryCopySelectionAsync(cancellationToken);

        Task<bool> IGraphEditorKernelCommandRouterHost.TryPasteSelectionAsync(CancellationToken cancellationToken)
            => _owner.TryPasteSelectionAsync(cancellationToken);

        bool IGraphEditorKernelCommandRouterHost.TryExportSelectionFragment(string? path)
            => _owner.TryExportSelectionFragment(path);

        bool IGraphEditorKernelCommandRouterHost.TryImportFragment(string? path)
            => _owner.TryImportFragment(path);

        bool IGraphEditorKernelCommandRouterHost.TryClearWorkspaceFragment(string? path)
            => _owner.TryClearWorkspaceFragment(path);

        string IGraphEditorKernelCommandRouterHost.TryExportSelectionAsTemplate(string? name)
            => _owner.TryExportSelectionAsTemplate(name);

        bool IGraphEditorKernelCommandRouterHost.TryApplyFragmentTemplatePreset(string path)
            => _owner.TryApplyFragmentTemplatePreset(path);

        bool IGraphEditorKernelCommandRouterHost.TryExportSceneAsSvg(string? path)
            => _owner.TryExportSceneAsSvg(path);

        bool IGraphEditorKernelCommandRouterHost.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat format,
            string? path,
            GraphEditorSceneImageExportOptions? options)
            => _owner.TryExportSceneAsImage(format, path, options);

        void IGraphEditorKernelCommandRouterHost.SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus)
            => _owner.SetNodePositions(positions, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus)
            => _owner.TrySetNodeSize(nodeId, size, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetNodeExpansionState(string nodeId, GraphNodeExpansionState expansionState)
            => _owner.TrySetNodeExpansionState(nodeId, expansionState);

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

        bool IGraphEditorKernelCommandRouterHost.TryApplyLayoutPlan(GraphLayoutPlan plan, bool updateStatus)
            => _owner.TryApplyLayoutPlan(plan, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TryApplySelectionLayout(GraphSelectionLayoutOperation operation, bool updateStatus)
            => _owner.TryApplySelectionLayout(operation, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySnapSelectedNodesToGrid(double gridSize, bool updateStatus)
            => _owner.TrySnapSelectedNodesToGrid(gridSize, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySnapAllNodesToGrid(double gridSize, bool updateStatus)
            => _owner.TrySnapAllNodesToGrid(gridSize, updateStatus);

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

        bool IGraphEditorKernelCommandRouterHost.TrySetConnectionLabel(string connectionId, string? label, bool updateStatus)
            => _owner.TrySetConnectionLabel(connectionId, label, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus)
            => _owner.TrySetConnectionNoteText(connectionId, noteText, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TryInsertConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus)
            => _owner.TryInsertConnectionRouteVertex(connectionId, vertexIndex, position, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TryMoveConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus)
            => _owner.TryMoveConnectionRouteVertex(connectionId, vertexIndex, position, updateStatus);

        bool IGraphEditorKernelCommandRouterHost.TryRemoveConnectionRouteVertex(string connectionId, int vertexIndex, bool updateStatus)
            => _owner.TryRemoveConnectionRouteVertex(connectionId, vertexIndex, updateStatus);

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

        void IGraphEditorKernelCommandRouterHost.FitSelectionToViewport(bool updateStatus)
            => _owner.FitSelectionToViewport(updateStatus);

        void IGraphEditorKernelCommandRouterHost.FocusSelection(bool updateStatus)
            => _owner.FocusSelection(updateStatus);

        void IGraphEditorKernelCommandRouterHost.FocusCurrentScope(bool updateStatus)
            => _owner.FocusCurrentScope(updateStatus);

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
