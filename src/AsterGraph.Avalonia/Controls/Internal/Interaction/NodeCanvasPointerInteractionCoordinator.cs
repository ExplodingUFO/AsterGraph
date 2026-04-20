using Avalonia;
using Avalonia.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
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

    void UpdateGroupVisuals();

    void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize);

    GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY);
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

        if (isMiddleButtonPressed
            || (_host.EnableAltLeftDragPanning && isLeftButtonPressed && modifiers.HasFlag(KeyModifiers.Alt)))
        {
            _host.InteractionSession.BeginPanning(currentScreenPosition);
            _host.HideSelectionAdorner();
            _host.HideGuideAdorners();
            return new NodeCanvasPointerPressedResult(Handled: true, CapturePointer: true);
        }

        if (!isLeftButtonPressed)
        {
            return default;
        }

        if (_host.ViewModel.HasPendingConnection)
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

    public bool HandleMoved(Point currentScreenPosition, double selectionDragThreshold)
    {
        if (_host.ViewModel is null)
        {
            return false;
        }

        _host.InteractionSession.UpdatePointerPosition(currentScreenPosition);

        if (_host.InteractionSession.SelectionStartScreenPosition is not null
            && !_host.InteractionSession.IsPanning
            && _host.InteractionSession.DragNode is null
            && _host.InteractionSession.DragGroupId is null
            && _host.InteractionSession.TryBeginMarqueeSelection(currentScreenPosition, selectionDragThreshold))
        {
            _host.UpdateMarqueeSelection(currentScreenPosition, finalize: false);
            return true;
        }

        var handled = false;
        if (_host.InteractionSession.IsPanning
            || _host.InteractionSession.DragNode is not null
            || _host.InteractionSession.DragGroupId is not null)
        {
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

                    if (_host.InteractionSession.DragNode is not null
                        && _host.InteractionSession.UpdateHoveredDropGroup(
                            ResolveHoveredDropGroupId(dragSession.Nodes, _host.InteractionSession.DragGroupDropZones)))
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
                    var deltaX = rawDelta.X / _host.ViewModel.Zoom;
                    var deltaY = rawDelta.Y / _host.ViewModel.Zoom;
                    var requestedPosition = new GraphPoint(groupOriginPosition.X + deltaX, groupOriginPosition.Y + deltaY);
                    _host.InteractionSession.UpdateDragGroupPreviewPosition(requestedPosition);
                    _host.ViewModel.ApplyDragOffset(dragSession.OriginPositions, deltaX, deltaY);
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

        return handled;
    }

    public void HandleReleased(Point currentScreenPosition)
    {
        if (_host.InteractionSession.DragNode is not null
            && _host.InteractionSession.DragSession is NodeCanvasDragSession dragSession)
        {
            ApplyDraggedNodeGroupMembership(dragSession.Nodes, _host.InteractionSession.DragGroupDropZones);
        }

        if (_host.InteractionSession.DragGroupId is not null
            && _host.InteractionSession.DragGroupOriginPosition is GraphPoint groupOriginPosition
            && _host.InteractionSession.DragStartScreenPosition is Point dragStart
            && _host.ViewModel is not null)
        {
            var rawDelta = currentScreenPosition - dragStart;
            var requestedPosition = new GraphPoint(
                groupOriginPosition.X + (rawDelta.X / _host.ViewModel.Zoom),
                groupOriginPosition.Y + (rawDelta.Y / _host.ViewModel.Zoom));
            _host.ViewModel.TrySetNodeGroupPosition(
                _host.InteractionSession.DragGroupId,
                requestedPosition,
                moveMemberNodes: true,
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
        else if (_host.InteractionSession.DragGroupId is not null && !string.IsNullOrWhiteSpace(_host.InteractionSession.DragGroupTitle))
        {
            _host.ViewModel?.CompleteHistoryInteraction($"Moved {_host.InteractionSession.DragGroupTitle} group.");
        }

        if (_host.InteractionSession.UpdateHoveredDropGroup(null))
        {
            _host.UpdateGroupVisuals();
        }

        _host.InteractionSession.ResetAfterPointerRelease();
        _host.HideGuideAdorners();
    }

    private static void ApplyDraggedNodeGroupMembership(
        IReadOnlyList<NodeViewModel> nodes,
        IReadOnlyList<GraphEditorNodeGroupSnapshot> dropZones)
    {
        foreach (var node in nodes)
        {
            var targetGroupId = ResolveDropGroupId(node, dropZones);
            if (string.Equals(node.GroupId, targetGroupId, StringComparison.Ordinal))
            {
                continue;
            }

            node.Surface = node.Surface with
            {
                GroupId = targetGroupId,
            };
        }
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

    private static string? ResolveDropGroupId(
        NodeViewModel node,
        IReadOnlyList<GraphEditorNodeGroupSnapshot> dropZones)
        => dropZones
            .Where(group => IsNodeWithinGroupBounds(node, group))
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
}
