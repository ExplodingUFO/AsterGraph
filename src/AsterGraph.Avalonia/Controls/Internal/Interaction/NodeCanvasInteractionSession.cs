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

internal readonly record struct NodeCanvasWhiteboardPrimitiveGesture(
    GraphWhiteboardPrimitiveKind Kind,
    GraphPoint StartWorldPosition);

internal enum NodeCanvasSelectionGestureKind
{
    Marquee,
    Lasso,
}

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

    public bool IsLassoSelecting { get; private set; }

    public NodeCanvasSelectionGestureKind SelectionGestureKind { get; private set; }

    public KeyModifiers SelectionModifiers { get; private set; }

    public IReadOnlyList<NodeViewModel> SelectionBaselineNodes { get; private set; } = [];

    public IReadOnlyList<Point> LassoScreenPoints => _lassoScreenPoints;

    public IReadOnlyList<Point> WhiteboardGestureScreenPoints => _whiteboardGestureScreenPoints;

    public IReadOnlyList<GraphWhiteboardPrimitive> WhiteboardPrimitives => _whiteboardPrimitives;

    public GraphWhiteboardPrimitive? ActiveWhiteboardPrimitive { get; private set; }

    public NodeCanvasWhiteboardPrimitiveGesture? WhiteboardPrimitiveGesture { get; private set; }

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

    private readonly List<Point> _lassoScreenPoints = [];
    private readonly List<Point> _whiteboardGestureScreenPoints = [];
    private readonly List<GraphPoint> _whiteboardGestureWorldPoints = [];
    private readonly List<GraphWhiteboardPrimitive> _whiteboardPrimitives = [];
    private int _nextWhiteboardPrimitiveSequence;

    public void BeginCanvasSelection(
        Point startScreenPosition,
        KeyModifiers modifiers,
        IReadOnlyList<NodeViewModel> baselineNodes,
        NodeCanvasSelectionGestureKind gestureKind = NodeCanvasSelectionGestureKind.Marquee)
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
        ClearActiveWhiteboardPrimitive();
        SelectionStartScreenPosition = startScreenPosition;
        IsMarqueeSelecting = false;
        IsLassoSelecting = false;
        SelectionGestureKind = gestureKind;
        SelectionModifiers = modifiers;
        SelectionBaselineNodes = baselineNodes.ToList();
        _lassoScreenPoints.Clear();
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
        IsLassoSelecting = false;
        _lassoScreenPoints.Clear();
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
        ClearActiveWhiteboardPrimitive();
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
        ClearActiveWhiteboardPrimitive();
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        IsLassoSelecting = false;
        _lassoScreenPoints.Clear();
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
        IsLassoSelecting = false;
        _lassoScreenPoints.Clear();
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
        ClearActiveWhiteboardPrimitive();
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
        IsLassoSelecting = false;
        _lassoScreenPoints.Clear();
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
        ClearActiveWhiteboardPrimitive();
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
        IsLassoSelecting = false;
        _lassoScreenPoints.Clear();
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
        ClearActiveWhiteboardPrimitive();
    }

    public void BeginWhiteboardPrimitiveDrawing(
        GraphWhiteboardPrimitiveKind kind,
        Point startScreenPosition,
        GraphPoint startWorldPosition)
    {
        DragNode = null;
        DragGroupId = null;
        DragGroupTitle = null;
        DragGroupOriginPosition = null;
        DragGroupPreviewPosition = null;
        DragGroupMovesMemberNodes = true;
        IsPanning = false;
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        IsLassoSelecting = false;
        SelectionGestureKind = NodeCanvasSelectionGestureKind.Marquee;
        SelectionModifiers = KeyModifiers.None;
        SelectionBaselineNodes = [];
        _lassoScreenPoints.Clear();
        DragStartScreenPosition = null;
        LastPointerPosition = startScreenPosition;
        PointerScreenPosition = startScreenPosition;
        DragSession = null;
        DragGroupDropZones = [];
        HoveredDropGroupId = null;
        NodeResizeSession = null;
        GroupResizeSession = null;
        NodeResizePreview = null;
        GroupResizePreview = null;
        WhiteboardPrimitiveGesture = new NodeCanvasWhiteboardPrimitiveGesture(kind, startWorldPosition);
        _whiteboardGestureScreenPoints.Clear();
        _whiteboardGestureWorldPoints.Clear();
        _whiteboardGestureScreenPoints.Add(startScreenPosition);
        _whiteboardGestureWorldPoints.Add(startWorldPosition);
        ActiveWhiteboardPrimitive = CreateWhiteboardPrimitive(
            kind,
            CreateNextWhiteboardPrimitiveId(),
            startWorldPosition,
            [startWorldPosition],
            GraphWhiteboardPrimitiveEditState.Creating);
    }

    public void UpdateWhiteboardPrimitiveDrawing(Point currentScreenPosition, GraphPoint currentWorldPosition)
    {
        if (WhiteboardPrimitiveGesture is not NodeCanvasWhiteboardPrimitiveGesture gesture)
        {
            return;
        }

        LastPointerPosition = currentScreenPosition;
        PointerScreenPosition = currentScreenPosition;
        AddWhiteboardGesturePoint(currentScreenPosition, currentWorldPosition);
        ActiveWhiteboardPrimitive = CreateWhiteboardPrimitive(
            gesture.Kind,
            ActiveWhiteboardPrimitive?.Id ?? CreateNextWhiteboardPrimitiveId(),
            gesture.StartWorldPosition,
            _whiteboardGestureWorldPoints,
            GraphWhiteboardPrimitiveEditState.Editing);
    }

    public GraphWhiteboardPrimitive? CommitWhiteboardPrimitiveDrawing(Point currentScreenPosition, GraphPoint currentWorldPosition)
    {
        if (WhiteboardPrimitiveGesture is not NodeCanvasWhiteboardPrimitiveGesture gesture)
        {
            return null;
        }

        AddWhiteboardGesturePoint(currentScreenPosition, currentWorldPosition);
        var committed = CreateWhiteboardPrimitive(
            gesture.Kind,
            ActiveWhiteboardPrimitive?.Id ?? CreateNextWhiteboardPrimitiveId(),
            gesture.StartWorldPosition,
            _whiteboardGestureWorldPoints,
            GraphWhiteboardPrimitiveEditState.Committed);
        _whiteboardPrimitives.Add(committed);
        ClearActiveWhiteboardPrimitive();
        return committed;
    }

    public bool TryEraseWhiteboardPrimitive(GraphPoint worldPosition)
    {
        var scene = GraphWhiteboardPrimitiveRendererAdapter.Project(_whiteboardPrimitives);
        var hit = GraphWhiteboardPrimitiveRendererAdapter.HitTest(scene, worldPosition);
        if (hit is null)
        {
            return false;
        }

        ClearActiveWhiteboardPrimitive();
        return _whiteboardPrimitives.RemoveAll(
            primitive => string.Equals(primitive.Id, hit.PrimitiveId, StringComparison.Ordinal)) > 0;
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

    public bool TryBeginLassoSelection(Point currentScreenPosition, double threshold)
    {
        if (SelectionGestureKind is not NodeCanvasSelectionGestureKind.Lasso
            || SelectionStartScreenPosition is null
            || IsPanning
            || DragNode is not null
            || DragGroupId is not null)
        {
            return false;
        }

        if (IsLassoSelecting)
        {
            return true;
        }

        var delta = currentScreenPosition - SelectionStartScreenPosition.Value;
        if (Math.Abs(delta.X) >= threshold || Math.Abs(delta.Y) >= threshold)
        {
            IsLassoSelecting = true;
            _lassoScreenPoints.Clear();
            _lassoScreenPoints.Add(SelectionStartScreenPosition.Value);
            _lassoScreenPoints.Add(currentScreenPosition);
        }

        return IsLassoSelecting;
    }

    public void RecordLassoSelectionPoint(Point currentScreenPosition)
    {
        if (!IsLassoSelecting)
        {
            return;
        }

        if (_lassoScreenPoints.Count == 0 || _lassoScreenPoints[^1] != currentScreenPosition)
        {
            _lassoScreenPoints.Add(currentScreenPosition);
        }
    }

    public void ResetAfterPointerRelease()
    {
        SelectionStartScreenPosition = null;
        IsMarqueeSelecting = false;
        IsLassoSelecting = false;
        SelectionGestureKind = NodeCanvasSelectionGestureKind.Marquee;
        SelectionBaselineNodes = [];
        _lassoScreenPoints.Clear();
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
        ClearActiveWhiteboardPrimitive();
    }

    private void AddWhiteboardGesturePoint(Point screenPoint, GraphPoint worldPoint)
    {
        if (_whiteboardGestureScreenPoints.Count == 0 || _whiteboardGestureScreenPoints[^1] != screenPoint)
        {
            _whiteboardGestureScreenPoints.Add(screenPoint);
        }

        if (_whiteboardGestureWorldPoints.Count == 0 || _whiteboardGestureWorldPoints[^1] != worldPoint)
        {
            _whiteboardGestureWorldPoints.Add(worldPoint);
        }
    }

    private string CreateNextWhiteboardPrimitiveId()
        => $"whiteboard-primitive-{++_nextWhiteboardPrimitiveSequence:000000}";

    private static GraphWhiteboardPrimitive CreateWhiteboardPrimitive(
        GraphWhiteboardPrimitiveKind kind,
        string id,
        GraphPoint startWorldPosition,
        IReadOnlyList<GraphPoint> worldPoints,
        GraphWhiteboardPrimitiveEditState state)
    {
        var geometry = kind is GraphWhiteboardPrimitiveKind.Rectangle
            ? CreateRectangleGeometry(startWorldPosition, worldPoints[^1])
            : CreateFreehandGeometry(worldPoints);
        return new GraphWhiteboardPrimitive(
            id,
            kind,
            geometry,
            GraphWhiteboardPrimitiveStyle.Default,
            ZIndex: 0,
            new GraphWhiteboardPrimitiveEditLifecycle(state, ActiveHandleKey: "pointer"));
    }

    private static GraphWhiteboardPrimitiveGeometry CreateRectangleGeometry(
        GraphPoint startWorldPosition,
        GraphPoint currentWorldPosition)
    {
        var left = Math.Min(startWorldPosition.X, currentWorldPosition.X);
        var top = Math.Min(startWorldPosition.Y, currentWorldPosition.Y);
        var right = Math.Max(startWorldPosition.X, currentWorldPosition.X);
        var bottom = Math.Max(startWorldPosition.Y, currentWorldPosition.Y);
        return new GraphWhiteboardPrimitiveGeometry(
            new GraphPoint(left, top),
            new GraphSize(right - left, bottom - top));
    }

    private static GraphWhiteboardPrimitiveGeometry CreateFreehandGeometry(IReadOnlyList<GraphPoint> worldPoints)
    {
        var minX = worldPoints.Min(point => point.X);
        var minY = worldPoints.Min(point => point.Y);
        var maxX = worldPoints.Max(point => point.X);
        var maxY = worldPoints.Max(point => point.Y);
        return new GraphWhiteboardPrimitiveGeometry(
            new GraphPoint(minX, minY),
            new GraphSize(maxX - minX, maxY - minY),
            worldPoints);
    }

    private void ClearActiveWhiteboardPrimitive()
    {
        ActiveWhiteboardPrimitive = null;
        WhiteboardPrimitiveGesture = null;
        _whiteboardGestureScreenPoints.Clear();
        _whiteboardGestureWorldPoints.Clear();
    }
}
