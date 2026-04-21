using Avalonia;
using Avalonia.Input;
using AsterGraph.Core.Models;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasDragSession(
    IReadOnlyList<NodeViewModel> Nodes,
    IReadOnlyDictionary<string, GraphPoint> OriginPositions,
    NodeBounds OriginBounds);

internal readonly record struct NodeCanvasNodeResizeSession(
    NodeViewModel Node,
    GraphNodeResizeHandleKind HandleKind,
    GraphSize OriginSize);

internal readonly record struct NodeCanvasNodeResizePreview(
    string NodeId,
    GraphSize Size);

internal readonly record struct NodeCanvasGroupResizeSession(
    string GroupId,
    string GroupTitle,
    NodeCanvasGroupResizeEdge Edge,
    GraphPoint OriginPosition,
    GraphSize OriginSize);

internal readonly record struct NodeCanvasGroupResizePreview(
    string GroupId,
    GraphPoint Position,
    GraphSize Size);

internal sealed class NodeCanvasInteractionSession
{
    public NodeViewModel? DragNode { get; private set; }

    public string? DragGroupId { get; private set; }

    public string? DragGroupTitle { get; private set; }

    public GraphPoint? DragGroupOriginPosition { get; private set; }

    public GraphPoint? DragGroupPreviewPosition { get; private set; }

    public bool DragGroupMovesMemberNodes { get; private set; } = true;

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

    public NodeCanvasNodeResizeSession? NodeResizeSession { get; private set; }

    public NodeCanvasGroupResizeSession? GroupResizeSession { get; private set; }

    public NodeCanvasNodeResizePreview? NodeResizePreview { get; private set; }

    public NodeCanvasGroupResizePreview? GroupResizePreview { get; private set; }

    public void BeginCanvasSelection(Point startScreenPosition, KeyModifiers modifiers, IReadOnlyList<NodeViewModel> baselineNodes)
    {
        DragNode = null;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        DragGroupPreviewPosition = null;
        DragGroupMovesMemberNodes = true;
        IsPanning = false;
        DragStartScreenPosition = null;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        NodeResizeSession = null;
        GroupResizeSession = null;
        NodeResizePreview = null;
        GroupResizePreview = null;
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
        DragGroupPreviewPosition = null;
        DragGroupMovesMemberNodes = true;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        DragStartScreenPosition = startScreenPosition;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = dragSession;
        DragGroupDropZones = dragGroupDropZones?.ToList() ?? [];
        HoveredDropGroupId = null;
        NodeResizeSession = null;
        GroupResizeSession = null;
        NodeResizePreview = null;
        GroupResizePreview = null;
    }

    public void BeginPanning(Point startScreenPosition)
    {
        IsPanning = true;
        DragNode = null;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        DragGroupPreviewPosition = null;
        DragGroupMovesMemberNodes = true;
        DragStartScreenPosition = null;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        NodeResizeSession = null;
        GroupResizeSession = null;
        NodeResizePreview = null;
        GroupResizePreview = null;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
    }

    public void BeginGroupDrag(
        string groupId,
        string groupTitle,
        GraphPoint groupOriginPosition,
        Point startScreenPosition,
        NodeCanvasDragSession dragSession,
        bool moveMemberNodes = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupTitle);

        DragNode = null;
        DragGroupId = groupId;
        DragGroupTitle = groupTitle;
        DragGroupOriginPosition = groupOriginPosition;
        DragGroupPreviewPosition = groupOriginPosition;
        DragGroupMovesMemberNodes = moveMemberNodes;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        DragStartScreenPosition = startScreenPosition;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = dragSession;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        NodeResizeSession = null;
        GroupResizeSession = null;
        NodeResizePreview = null;
        GroupResizePreview = null;
    }

    public void BeginNodeResize(NodeViewModel node, GraphNodeResizeHandleKind handleKind, Point startScreenPosition)
    {
        ArgumentNullException.ThrowIfNull(node);

        DragNode = null;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        DragGroupPreviewPosition = null;
        DragGroupMovesMemberNodes = true;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        DragStartScreenPosition = startScreenPosition;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        NodeResizeSession = new NodeCanvasNodeResizeSession(
            node,
            handleKind,
            new GraphSize(node.Width, node.Height));
        GroupResizeSession = null;
        NodeResizePreview = new NodeCanvasNodeResizePreview(node.Id, new GraphSize(node.Width, node.Height));
        GroupResizePreview = null;
    }

    public void BeginGroupResize(
        string groupId,
        string groupTitle,
        NodeCanvasGroupResizeEdge edge,
        GraphPoint groupOriginPosition,
        GraphSize groupOriginSize,
        Point startScreenPosition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupTitle);

        DragNode = null;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        DragGroupPreviewPosition = null;
        DragGroupMovesMemberNodes = true;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        DragStartScreenPosition = startScreenPosition;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        NodeResizeSession = null;
        GroupResizeSession = new NodeCanvasGroupResizeSession(
            groupId,
            groupTitle,
            edge,
            groupOriginPosition,
            groupOriginSize);
        NodeResizePreview = null;
        GroupResizePreview = new NodeCanvasGroupResizePreview(groupId, groupOriginPosition, groupOriginSize);
    }

    public void UpdatePointerPosition(Point currentScreenPosition)
        => PointerScreenPosition = currentScreenPosition;

    public void UpdateLastPointerPosition(Point currentScreenPosition)
        => LastPointerPosition = currentScreenPosition;

    public bool UpdateDragGroupPreviewPosition(GraphPoint? position)
    {
        if (DragGroupPreviewPosition == position)
        {
            return false;
        }

        DragGroupPreviewPosition = position;
        return true;
    }

    public bool UpdateHoveredDropGroup(string? groupId)
    {
        if (string.Equals(HoveredDropGroupId, groupId, StringComparison.Ordinal))
        {
            return false;
        }

        HoveredDropGroupId = groupId;
        return true;
    }

    public void UpdateDragGroupDropZones(IReadOnlyList<GraphEditorNodeGroupSnapshot> dropZones)
    {
        ArgumentNullException.ThrowIfNull(dropZones);
        DragGroupDropZones = dropZones.ToList();
    }

    public bool UpdateNodeResizePreview(string nodeId, GraphSize size)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var preview = new NodeCanvasNodeResizePreview(nodeId, size);
        if (NodeResizePreview == preview)
        {
            return false;
        }

        NodeResizePreview = preview;
        return true;
    }

    public bool UpdateGroupResizePreview(string groupId, GraphPoint position, GraphSize size)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var preview = new NodeCanvasGroupResizePreview(groupId, position, size);
        if (GroupResizePreview == preview)
        {
            return false;
        }

        GroupResizePreview = preview;
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
        DragGroupPreviewPosition = null;
        DragGroupMovesMemberNodes = true;
        DragStartScreenPosition = null;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        IsPanning = false;
        NodeResizeSession = null;
        GroupResizeSession = null;
        NodeResizePreview = null;
        GroupResizePreview = null;
    }
}
