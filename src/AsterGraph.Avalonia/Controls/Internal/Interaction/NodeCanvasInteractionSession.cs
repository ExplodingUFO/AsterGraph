using Avalonia;
using Avalonia.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasDragSession(
    IReadOnlyList<NodeViewModel> Nodes,
    IReadOnlyDictionary<string, GraphPoint> OriginPositions,
    NodeBounds OriginBounds);

internal sealed class NodeCanvasInteractionSession
{
    public NodeViewModel? DragNode { get; private set; }

    public bool IsPanning { get; private set; }

    public bool IsMarqueeSelecting { get; private set; }

    public KeyModifiers SelectionModifiers { get; private set; }

    public IReadOnlyList<NodeViewModel> SelectionBaselineNodes { get; private set; } = [];

    public Point? DragStartScreenPosition { get; private set; }

    public NodeCanvasDragSession? DragSession { get; private set; }

    public Point LastPointerPosition { get; private set; }

    public Point? PointerScreenPosition { get; private set; }

    public Point? SelectionStartScreenPosition { get; private set; }

    public void BeginCanvasSelection(Point startScreenPosition, KeyModifiers modifiers, IReadOnlyList<NodeViewModel> baselineNodes)
    {
        DragNode = null;
        IsPanning = false;
        DragStartScreenPosition = null;
        DragSession = null;
        SelectionStartScreenPosition = startScreenPosition;
        IsMarqueeSelecting = false;
        SelectionModifiers = modifiers;
        SelectionBaselineNodes = baselineNodes.ToList();
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
    }

    public void BeginNodeDrag(NodeViewModel node, Point startScreenPosition, NodeCanvasDragSession dragSession)
    {
        DragNode = node;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        DragStartScreenPosition = startScreenPosition;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = dragSession;
    }

    public void BeginPanning(Point startScreenPosition)
    {
        IsPanning = true;
        DragNode = null;
        DragStartScreenPosition = null;
        DragSession = null;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
    }

    public void UpdatePointerPosition(Point currentScreenPosition)
        => PointerScreenPosition = currentScreenPosition;

    public void UpdateLastPointerPosition(Point currentScreenPosition)
        => LastPointerPosition = currentScreenPosition;

    public bool TryBeginMarqueeSelection(Point currentScreenPosition, double threshold)
    {
        if (SelectionStartScreenPosition is null || IsPanning || DragNode is not null)
        {
            return false;
        }

        if (IsMarqueeSelecting)
        {
            return true;
        }

        var delta = currentScreenPosition - SelectionStartScreenPosition.Value;
        if (Math.Abs(delta.X) >= threshold || Math.Abs(delta.Y) >= threshold)
        {
            IsMarqueeSelecting = true;
        }

        return IsMarqueeSelecting;
    }

    public void ResetAfterPointerRelease()
    {
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        SelectionBaselineNodes = [];
        DragNode = null;
        DragStartScreenPosition = null;
        DragSession = null;
        IsPanning = false;
    }
}
