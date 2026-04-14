using Avalonia;
using Avalonia.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
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
            && _host.InteractionSession.TryBeginMarqueeSelection(currentScreenPosition, selectionDragThreshold))
        {
            _host.UpdateMarqueeSelection(currentScreenPosition, finalize: false);
            return true;
        }

        var handled = false;
        if (_host.InteractionSession.IsPanning || _host.InteractionSession.DragNode is not null)
        {
            if (_host.InteractionSession.DragNode is not null
                && _host.InteractionSession.DragSession is NodeCanvasDragSession dragSession
                && _host.InteractionSession.DragStartScreenPosition is Point dragStart)
            {
                var rawDelta = currentScreenPosition - dragStart;
                var adjustedDelta = _host.ApplyDragAssist(
                    dragSession,
                    rawDelta.X / _host.ViewModel.Zoom,
                    rawDelta.Y / _host.ViewModel.Zoom);
                _host.ViewModel.ApplyDragOffset(dragSession.OriginPositions, adjustedDelta.X, adjustedDelta.Y);
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

        _host.InteractionSession.ResetAfterPointerRelease();
        _host.HideGuideAdorners();
    }
}
