using Avalonia;
using Avalonia.Input;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Scene;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasPointerInteractionHost
{
    GraphEditorViewModel? ViewModel { get; }

    bool EnableAltLeftDragPanning { get; }

    NodeCanvasInteractionSession InteractionSession { get; }

    void FocusCanvas();

    void HideSelectionAdorner();

    void HideGuideAdorners();

    void RenderConnections();

    void UpdateNodeVisual(NodeViewModel node);

    void UpdateGroupVisuals();

    void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize);

    GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY);

    NodeCanvasGroupResizePreview ApplyGroupResizeAssist(
        GraphEditorNodeGroupSnapshot group,
        NodeCanvasGroupResizeEdge edge,
        GraphPoint proposedPosition,
        GraphSize proposedSize,
        GraphSize minimumSize);

    void UpdateResizeFeedback(Point currentScreenPosition);

    void ClearResizeFeedback();
}

internal readonly record struct NodeCanvasPointerPressedResult(bool Handled, bool CapturePointer);

internal sealed class NodeCanvasPointerInteractionCoordinator
{
    private readonly INodeCanvasPointerInteractionHost _host;

    public NodeCanvasPointerInteractionCoordinator(INodeCanvasPointerInteractionHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public NodeCanvasPointerPressedResult HandlePressed(
        bool isAlreadyHandled,
        Point currentScreenPosition,
        bool isLeftButtonPressed,
        bool isMiddleButtonPressed,
        KeyModifiers modifiers)
    {
        if (_host.ViewModel is null || isAlreadyHandled)
        {
            return default;
        }

        _host.FocusCanvas();
        _host.InteractionSession.UpdateLastPointerPosition(currentScreenPosition);
        _host.InteractionSession.UpdatePointerPosition(currentScreenPosition);

        var route = GraphEditorPointerInputRouter.RoutePressed(new GraphEditorPointerInputContext(
            isAlreadyHandled,
            isLeftButtonPressed,
            isMiddleButtonPressed,
            MapModifiers(modifiers),
            _host.EnableAltLeftDragPanning,
            _host.ViewModel.HasPendingConnection));

        if (route.Kind is GraphEditorPointerPressRouteKind.BeginPanning)
        {
            _host.ClearResizeFeedback();
            _host.InteractionSession.BeginPanning(currentScreenPosition);
            _host.HideSelectionAdorner();
            _host.HideGuideAdorners();
            return new NodeCanvasPointerPressedResult(Handled: true, CapturePointer: true);
        }

        if (route.Kind is GraphEditorPointerPressRouteKind.Ignore)
        {
            return default;
        }

        _host.ClearResizeFeedback();
        if (route.CancelPendingConnection)
        {
            _host.ViewModel.CancelPendingConnection("Connection preview cancelled.");
            _host.RenderConnections();
        }

        _host.InteractionSession.BeginCanvasSelection(
            currentScreenPosition,
            modifiers,
            _host.ViewModel.SelectedNodes.ToList());
        _host.HideSelectionAdorner();
        _host.HideGuideAdorners();
        return new NodeCanvasPointerPressedResult(Handled: true, CapturePointer: true);
    }

    private static GraphEditorInputModifiers MapModifiers(KeyModifiers modifiers)
    {
        var mapped = GraphEditorInputModifiers.None;
        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            mapped |= GraphEditorInputModifiers.Shift;
        }

        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            mapped |= GraphEditorInputModifiers.Control;
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            mapped |= GraphEditorInputModifiers.Alt;
        }

        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            mapped |= GraphEditorInputModifiers.Meta;
        }

        return mapped;
    }

    public bool HandleMoved(Point currentScreenPosition, double selectionDragThreshold)
    {
        if (_host.ViewModel is null)
        {
            return false;
        }

        _host.InteractionSession.UpdatePointerPosition(currentScreenPosition);

        if (_host.InteractionSession.NodeResizeSession is NodeCanvasNodeResizeSession nodeResizeSession)
        {
            return HandleNodeResizeMove(nodeResizeSession, currentScreenPosition);
        }

        if (_host.InteractionSession.GroupResizeSession is NodeCanvasGroupResizeSession groupResizeSession)
        {
            return HandleGroupResizeMove(groupResizeSession, currentScreenPosition);
        }

        if (_host.InteractionSession.SelectionStartScreenPosition is not null
            && !_host.InteractionSession.IsPanning
            && _host.InteractionSession.DragNode is null
            && _host.InteractionSession.DragGroupId is null
            && _host.InteractionSession.TryBeginMarqueeSelection(currentScreenPosition, selectionDragThreshold))
        {
            _host.ClearResizeFeedback();
            _host.UpdateMarqueeSelection(currentScreenPosition, finalize: false);
            return true;
        }

        var handled = false;
        if (_host.InteractionSession.IsPanning
            || _host.InteractionSession.DragNode is not null
            || _host.InteractionSession.DragGroupId is not null)
        {
            _host.ClearResizeFeedback();
                if (_host.InteractionSession.DragNode is not null)
                {
                    if (_host.InteractionSession.DragSession is NodeCanvasDragSession dragSession
                        && _host.InteractionSession.DragStartScreenPosition is Point dragStart)
                    {
                    var rawDelta = currentScreenPosition - dragStart;
                    var adjustedDelta = _host.ApplyDragAssist(
                        dragSession,
                        rawDelta.X / _host.ViewModel.Zoom,
                        rawDelta.Y / _host.ViewModel.Zoom);
                    _host.ViewModel.ApplyDragOffset(dragSession.OriginPositions, adjustedDelta.X, adjustedDelta.Y);
                    var currentDragNodes = ResolveCurrentDragNodes(dragSession.Nodes);
                    _host.InteractionSession.UpdateDragGroupDropZones(
                        _host.ViewModel.Session.Queries.GetHierarchyStateSnapshot().NodeGroups);
                    var autoExpanded = TryAutoExpandCollapsedDropGroup(currentDragNodes);
                    _host.InteractionSession.UpdateDragGroupDropZones(
                        _host.ViewModel.Session.Queries.GetHierarchyStateSnapshot().NodeGroups);
                    if (autoExpanded)
                    {
                        _host.UpdateGroupVisuals();
                    }

                    if (_host.InteractionSession.DragNode is not null
                        && _host.InteractionSession.UpdateHoveredDropGroup(
                            ResolveHoveredDropGroupId(currentDragNodes, _host.InteractionSession.DragGroupDropZones)))
                    {
                        _host.UpdateGroupVisuals();
                    }
                }
            }
            else if (_host.InteractionSession.DragGroupId is not null)
            {
                if (_host.InteractionSession.DragGroupOriginPosition is GraphPoint groupOriginPosition
                    && _host.InteractionSession.DragSession is NodeCanvasDragSession dragSession
                    && _host.InteractionSession.DragStartScreenPosition is Point dragStart)
                {
                    var rawDelta = currentScreenPosition - dragStart;
                    var adjustedDelta = _host.ApplyDragAssist(
                        dragSession,
                        rawDelta.X / _host.ViewModel.Zoom,
                        rawDelta.Y / _host.ViewModel.Zoom);
                    var requestedPosition = new GraphPoint(
                        groupOriginPosition.X + adjustedDelta.X,
                        groupOriginPosition.Y + adjustedDelta.Y);
                    var previewChanged = _host.InteractionSession.UpdateDragGroupPreviewPosition(requestedPosition);
                    if (_host.InteractionSession.DragGroupMovesMemberNodes)
                    {
                        _host.ViewModel.ApplyDragOffset(dragSession.OriginPositions, adjustedDelta.X, adjustedDelta.Y);
                    }
                    else if (previewChanged)
                    {
                        _host.UpdateGroupVisuals();
                    }
                }

                handled = true;
            }
            else if (_host.InteractionSession.IsPanning)
            {
                var delta = currentScreenPosition - _host.InteractionSession.LastPointerPosition;
                _host.InteractionSession.UpdateLastPointerPosition(currentScreenPosition);
                _host.ViewModel.PanBy(delta.X, delta.Y);
            }

            handled = true;
        }

        if (_host.ViewModel.HasPendingConnection)
        {
            _host.RenderConnections();
        }

        if (!handled)
        {
            _host.UpdateResizeFeedback(currentScreenPosition);
        }

        return handled;
    }

    public void HandleReleased(Point currentScreenPosition)
    {
        if (_host.InteractionSession.DragNode is not null
            && _host.InteractionSession.DragSession is NodeCanvasDragSession dragSession
            && _host.ViewModel is not null)
        {
            ApplyDraggedNodeGroupMembership(_host.ViewModel, ResolveCurrentDragNodes(dragSession.Nodes));
        }

        if (_host.InteractionSession.DragGroupId is not null
            && _host.InteractionSession.DragGroupOriginPosition is GraphPoint groupOriginPosition
            && _host.InteractionSession.DragStartScreenPosition is Point dragStart
            && _host.ViewModel is not null)
        {
            var rawDelta = currentScreenPosition - dragStart;
            var requestedPosition = _host.InteractionSession.DragGroupPreviewPosition
                ?? new GraphPoint(
                    groupOriginPosition.X + (rawDelta.X / _host.ViewModel.Zoom),
                    groupOriginPosition.Y + (rawDelta.Y / _host.ViewModel.Zoom));
            _host.ViewModel.Session.Commands.TrySetNodeGroupPosition(
                _host.InteractionSession.DragGroupId,
                requestedPosition,
                moveMemberNodes: _host.InteractionSession.DragGroupMovesMemberNodes,
                updateStatus: false);
        }

        if (_host.InteractionSession.NodeResizeSession is NodeCanvasNodeResizeSession activeNodeResizeSession
            && _host.InteractionSession.NodeResizePreview is NodeCanvasNodeResizePreview nodeResizePreview
            && _host.ViewModel is not null)
        {
            var node = _host.ViewModel.FindNode(activeNodeResizeSession.Node.Id) ?? activeNodeResizeSession.Node;
            _host.ViewModel.TrySetNodeSize(node, nodeResizePreview.Size, updateStatus: false);
        }

        if (_host.InteractionSession.GroupResizeSession is NodeCanvasGroupResizeSession activeGroupResizeSession
            && _host.InteractionSession.GroupResizePreview is NodeCanvasGroupResizePreview groupResizePreview
            && _host.ViewModel is not null)
        {
            _host.ViewModel.TrySetNodeGroupFrame(
                activeGroupResizeSession.GroupId,
                groupResizePreview.Position,
                groupResizePreview.Size,
                updateStatus: false);
        }

        if (_host.InteractionSession.SelectionStartScreenPosition is not null)
        {
            if (_host.InteractionSession.IsMarqueeSelecting)
            {
                _host.UpdateMarqueeSelection(currentScreenPosition, finalize: true);
            }
            else
            {
                _host.ViewModel?.ClearSelection();
            }

            _host.HideSelectionAdorner();
        }

        if (_host.InteractionSession.DragNode is not null)
        {
            _host.ViewModel?.CompleteHistoryInteraction(
                _host.ViewModel is not null && _host.ViewModel.HasMultipleSelection
                    ? "Moved selection."
                    : $"Moved {_host.InteractionSession.DragNode.Title}.");
        }
        else if (_host.InteractionSession.NodeResizeSession is NodeCanvasNodeResizeSession nodeResizeSession)
        {
            _host.ViewModel?.CompleteHistoryInteraction($"Resized {nodeResizeSession.Node.Title}.");
        }
        else if (_host.InteractionSession.DragGroupId is not null && !string.IsNullOrWhiteSpace(_host.InteractionSession.DragGroupTitle))
        {
            _host.ViewModel?.CompleteHistoryInteraction($"Moved {_host.InteractionSession.DragGroupTitle} group.");
        }
        else if (_host.InteractionSession.GroupResizeSession is NodeCanvasGroupResizeSession groupResizeSession)
        {
            _host.ViewModel?.CompleteHistoryInteraction($"Resized {groupResizeSession.GroupTitle} group.");
        }

        if (_host.InteractionSession.UpdateHoveredDropGroup(null))
        {
            _host.UpdateGroupVisuals();
        }

        _host.InteractionSession.ResetAfterPointerRelease();
        _host.HideGuideAdorners();
        _host.ClearResizeFeedback();
        _host.UpdateResizeFeedback(currentScreenPosition);
    }

    public bool HandlePointerCaptureLost()
    {
        if (!HasActivePointerInteraction())
        {
            return false;
        }

        _host.InteractionSession.ResetAfterPointerRelease();
        _host.HideGuideAdorners();
        _host.ClearResizeFeedback();
        return true;
    }

    private bool HandleNodeResizeMove(NodeCanvasNodeResizeSession resizeSession, Point currentScreenPosition)
    {
        if (_host.ViewModel is null || _host.InteractionSession.DragStartScreenPosition is not Point resizeStart)
        {
            return false;
        }

        var node = _host.ViewModel.FindNode(resizeSession.Node.Id) ?? resizeSession.Node;
        var delta = currentScreenPosition - resizeStart;
        var deltaX = delta.X / _host.ViewModel.Zoom;
        var deltaY = delta.Y / _host.ViewModel.Zoom;
        var minimumWidth = node.SurfaceMeasurement.BaselineSize.Width;
        var minimumHeight = node.SurfaceMeasurement.BaselineSize.Height;

        var nextWidth = resizeSession.HandleKind is GraphNodeResizeHandleKind.Right or GraphNodeResizeHandleKind.BottomRight
            ? Math.Max(minimumWidth, resizeSession.OriginSize.Width + deltaX)
            : resizeSession.OriginSize.Width;
        var nextHeight = resizeSession.HandleKind is GraphNodeResizeHandleKind.Bottom or GraphNodeResizeHandleKind.BottomRight
            ? Math.Max(minimumHeight, resizeSession.OriginSize.Height + deltaY)
            : resizeSession.OriginSize.Height;
        var nextSize = new GraphSize(nextWidth, nextHeight);
        if (!_host.InteractionSession.UpdateNodeResizePreview(node.Id, nextSize))
        {
            return false;
        }

        _host.UpdateNodeVisual(node);
        _host.RenderConnections();
        return true;
    }

    private bool HandleGroupResizeMove(NodeCanvasGroupResizeSession resizeSession, Point currentScreenPosition)
    {
        if (_host.ViewModel is null || _host.InteractionSession.DragStartScreenPosition is not Point resizeStart)
        {
            return false;
        }

        var currentGroup = _host.ViewModel.GetNodeGroupSnapshots()
            .FirstOrDefault(group => string.Equals(group.Id, resizeSession.GroupId, StringComparison.Ordinal));
        if (currentGroup is null)
        {
            return false;
        }

        var delta = currentScreenPosition - resizeStart;
        var deltaX = delta.X / _host.ViewModel.Zoom;
        var deltaY = delta.Y / _host.ViewModel.Zoom;
        var minimumSize = ResolveMinimumGroupSize(currentGroup);
        var nextPosition = resizeSession.OriginPosition;
        var nextSize = resizeSession.OriginSize;

        switch (resizeSession.Edge)
        {
            case NodeCanvasGroupResizeEdge.Left:
            {
                var proposedWidth = Math.Max(minimumSize.Width, resizeSession.OriginSize.Width - deltaX);
                nextPosition = new GraphPoint(
                    resizeSession.OriginPosition.X + (resizeSession.OriginSize.Width - proposedWidth),
                    resizeSession.OriginPosition.Y);
                nextSize = new GraphSize(proposedWidth, resizeSession.OriginSize.Height);
                break;
            }

            case NodeCanvasGroupResizeEdge.Top:
            {
                var proposedHeight = Math.Max(minimumSize.Height, resizeSession.OriginSize.Height - deltaY);
                nextPosition = new GraphPoint(
                    resizeSession.OriginPosition.X,
                    resizeSession.OriginPosition.Y + (resizeSession.OriginSize.Height - proposedHeight));
                nextSize = new GraphSize(resizeSession.OriginSize.Width, proposedHeight);
                break;
            }

            case NodeCanvasGroupResizeEdge.Right:
                nextSize = new GraphSize(
                    Math.Max(minimumSize.Width, resizeSession.OriginSize.Width + deltaX),
                    resizeSession.OriginSize.Height);
                break;

            case NodeCanvasGroupResizeEdge.Bottom:
                nextSize = new GraphSize(
                    resizeSession.OriginSize.Width,
                    Math.Max(minimumSize.Height, resizeSession.OriginSize.Height + deltaY));
                break;
        }

        var adjustedPreview = _host.ApplyGroupResizeAssist(
            currentGroup,
            resizeSession.Edge,
            nextPosition,
            nextSize,
            minimumSize);
        if (!_host.InteractionSession.UpdateGroupResizePreview(
                resizeSession.GroupId,
                adjustedPreview.Position,
                adjustedPreview.Size))
        {
            return false;
        }

        _host.UpdateGroupVisuals();
        _host.RenderConnections();
        return true;
    }

    private static GraphSize ResolveMinimumGroupSize(GraphEditorNodeGroupSnapshot group)
        => new(
            Math.Max(
                NodeCanvasGroupChromeMetrics.MinimumWidth,
                group.ExtraPadding.Left + group.ExtraPadding.Right + 48d),
            group.IsCollapsed
                ? NodeCanvasGroupChromeMetrics.HeaderHeight
                : Math.Max(
                    NodeCanvasGroupChromeMetrics.MinimumExpandedHeight,
                    group.ExtraPadding.Top + group.ExtraPadding.Bottom + 24d));

    private IReadOnlyList<NodeViewModel> ResolveCurrentDragNodes(IReadOnlyList<NodeViewModel> nodes)
    {
        if (_host.ViewModel is null)
        {
            return nodes;
        }

        return nodes
            .Select(node => _host.ViewModel.FindNode(node.Id) ?? node)
            .ToList();
    }

    private bool TryAutoExpandCollapsedDropGroup(IReadOnlyList<NodeViewModel> nodes)
    {
        if (_host.ViewModel is null)
        {
            return false;
        }

        var groupId = ResolveAutoExpandGroupId(nodes, _host.InteractionSession.DragGroupDropZones);
        if (string.IsNullOrWhiteSpace(groupId)
            || !_host.ViewModel.Session.Commands.TrySetNodeGroupCollapsed(groupId, isCollapsed: false))
        {
            return false;
        }

        return true;
    }

    private static void ApplyDraggedNodeGroupMembership(
        GraphEditorViewModel viewModel,
        IReadOnlyList<NodeViewModel> nodes)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var dropZones = viewModel.Session.Queries.GetHierarchyStateSnapshot().NodeGroups;
        var changes = new List<GraphEditorNodeGroupMembershipChange>();
        foreach (var node in nodes)
        {
            var targetGroupId = ResolveDropGroupId(node, dropZones);
            if (string.Equals(node.GroupId, targetGroupId, StringComparison.Ordinal))
            {
                continue;
            }

            changes.Add(new GraphEditorNodeGroupMembershipChange(node.Id, targetGroupId));
        }

        if (changes.Count == 0)
        {
            return;
        }

        viewModel.Session.Commands.TrySetNodeGroupMemberships(changes, updateStatus: false);
    }

    private static string? ResolveHoveredDropGroupId(
        IReadOnlyList<NodeViewModel> nodes,
        IReadOnlyList<GraphEditorNodeGroupSnapshot> dropZones)
    {
        string? hoveredGroupId = null;
        foreach (var node in nodes)
        {
            var targetGroupId = ResolveDropGroupId(node, dropZones);
            if (string.IsNullOrWhiteSpace(targetGroupId))
            {
                return null;
            }

            if (hoveredGroupId is null)
            {
                hoveredGroupId = targetGroupId;
                continue;
            }

            if (!string.Equals(hoveredGroupId, targetGroupId, StringComparison.Ordinal))
            {
                return null;
            }
        }

        return hoveredGroupId;
    }

    private static string? ResolveAutoExpandGroupId(
        IReadOnlyList<NodeViewModel> nodes,
        IReadOnlyList<GraphEditorNodeGroupSnapshot> dropZones)
    {
        string? hoveredGroupId = null;
        foreach (var node in nodes)
        {
            var targetGroupId = ResolveCollapsedHeaderOverlapGroupId(node, dropZones);
            if (string.IsNullOrWhiteSpace(targetGroupId))
            {
                return null;
            }

            if (hoveredGroupId is null)
            {
                hoveredGroupId = targetGroupId;
                continue;
            }

            if (!string.Equals(hoveredGroupId, targetGroupId, StringComparison.Ordinal))
            {
                return null;
            }
        }

        return hoveredGroupId;
    }

    private static string? ResolveDropGroupId(
        NodeViewModel node,
        IReadOnlyList<GraphEditorNodeGroupSnapshot> dropZones)
        => dropZones
            .Where(group => IsNodeWithinGroupBounds(node, group))
            .OrderBy(group => group.Size.Width * group.Size.Height)
            .ThenBy(group => group.Id, StringComparer.Ordinal)
            .Select(group => group.Id)
            .FirstOrDefault();

    private static string? ResolveCollapsedHeaderOverlapGroupId(
        NodeViewModel node,
        IReadOnlyList<GraphEditorNodeGroupSnapshot> dropZones)
        => dropZones
            .Where(group => group.IsCollapsed && DoesNodeIntersectCollapsedGroupHeader(node, group))
            .OrderBy(group => group.Size.Width * group.Size.Height)
            .ThenBy(group => group.Id, StringComparer.Ordinal)
            .Select(group => group.Id)
            .FirstOrDefault();

    private static bool IsNodeWithinGroupBounds(NodeViewModel node, GraphEditorNodeGroupSnapshot group)
    {
        if (group.IsCollapsed)
        {
            return false;
        }

        if (group.ContentSize.Width <= 0d || group.ContentSize.Height <= 0d)
        {
            return false;
        }

        var right = node.X + node.Width;
        var bottom = node.Y + node.Height;
        var groupRight = group.ContentPosition.X + group.ContentSize.Width;
        var groupBottom = group.ContentPosition.Y + group.ContentSize.Height;

        return node.X >= group.ContentPosition.X
               && node.Y >= group.ContentPosition.Y
               && right <= groupRight
               && bottom <= groupBottom;
    }

    private static bool DoesNodeIntersectCollapsedGroupHeader(NodeViewModel node, GraphEditorNodeGroupSnapshot group)
    {
        var nodeRight = node.X + node.Width;
        var nodeBottom = node.Y + node.Height;
        var groupRight = group.Position.X + NodeCanvasGroupChromeMetrics.ResolveRenderedWidth(group);
        var groupBottom = group.Position.Y + NodeCanvasGroupChromeMetrics.HeaderHeight;

        return node.X < groupRight
               && nodeRight > group.Position.X
               && node.Y < groupBottom
               && nodeBottom > group.Position.Y;
    }

    private bool HasActivePointerInteraction()
        => _host.InteractionSession.SelectionStartScreenPosition is not null
           || _host.InteractionSession.IsPanning
           || _host.InteractionSession.DragNode is not null
           || _host.InteractionSession.DragGroupId is not null
           || _host.InteractionSession.NodeResizeSession is not null
           || _host.InteractionSession.GroupResizeSession is not null;
}
