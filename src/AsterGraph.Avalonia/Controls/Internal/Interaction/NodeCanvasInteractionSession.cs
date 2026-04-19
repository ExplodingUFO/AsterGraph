using Avalonia;
using Avalonia.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasDragSession(
    IReadOnlyList<NodeViewModel> Nodes,
    IReadOnlyDictionary<string, GraphPoint> OriginPositions,
    NodeBounds OriginBounds);

internal sealed class NodeCanvasInteractionSession
{
    public NodeViewModel? DragNode { get; private set; }

    public string? DragGroupId { get; private set; }

    public string? DragGroupTitle { get; private set; }

    public GraphPoint? DragGroupOriginPosition { get; private set; }

    public bool IsPanning { get; private set; }

    public bool IsMarqueeSelecting { get; private set; }

    public KeyModifiers SelectionModifiers { get; private set; }

    public IReadOnlyList<NodeViewModel> SelectionBaselineNodes { get; private set; } = [];

    public Point? DragStartScreenPosition { get; private set; }

    public NodeCanvasDragSession? DragSession { get; private set; }

    public IReadOnlyList<GraphEditorNodeGroupSnapshot> DragGroupDropZones { get; private set; } = [];

    public string? HoveredDropGroupId { get; private set; }

    public Point LastPointerPosition { get; private set; }

    public Point? PointerScreenPosition { get; private set; }

    public Point? SelectionStartScreenPosition { get; private set; }

    public void BeginCanvasSelection(Point startScreenPosition, KeyModifiers modifiers, IReadOnlyList<NodeViewModel> baselineNodes)
    {
        DragNode = null;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        IsPanning = false;
        DragStartScreenPosition = null;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        SelectionStartScreenPosition = startScreenPosition;
        IsMarqueeSelecting = false;
        SelectionModifiers = modifiers;
        SelectionBaselineNodes = baselineNodes.ToList();
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
    }

    public void BeginNodeDrag(
        NodeViewModel node,
        Point startScreenPosition,
        NodeCanvasDragSession dragSession,
        IReadOnlyList<GraphEditorNodeGroupSnapshot>? dragGroupDropZones = null)
    {
        DragNode = node;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        DragStartScreenPosition = startScreenPosition;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = dragSession;
        DragGroupDropZones = dragGroupDropZones?.ToList() ?? [];
        HoveredDropGroupId = null;
    }

    public void BeginPanning(Point startScreenPosition)
    {
        IsPanning = true;
        DragNode = null;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        DragStartScreenPosition = null;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
    }

    public void BeginGroupDrag(string groupId, string groupTitle, GraphPoint groupOriginPosition, Point startScreenPosition, NodeCanvasDragSession dragSession)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupTitle);

        DragNode = null;
        DragGroupId = groupId;
        DragGroupTitle = groupTitle;
        DragGroupOriginPosition = groupOriginPosition;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        DragStartScreenPosition = startScreenPosition;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = dragSession;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
    }

    public void UpdatePointerPosition(Point currentScreenPosition)
        => PointerScreenPosition = currentScreenPosition;

    public void UpdateLastPointerPosition(Point currentScreenPosition)
        => LastPointerPosition = currentScreenPosition;

    public bool UpdateHoveredDropGroup(string? groupId)
    {
        if (string.Equals(HoveredDropGroupId, groupId, StringComparison.Ordinal))
        {
            return false;
        }

        HoveredDropGroupId = groupId;
        return true;
    }

    public bool TryBeginMarqueeSelection(Point currentScreenPosition, double threshold)
    {
        if (SelectionStartScreenPosition is null || IsPanning || DragNode is not null || DragGroupId is not null)
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
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        DragStartScreenPosition = null;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        IsPanning = false;
    }
}
