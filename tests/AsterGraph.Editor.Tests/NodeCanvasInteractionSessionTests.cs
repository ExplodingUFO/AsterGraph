using Avalonia;
using Avalonia.Input;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Avalonia.Controls.Internal;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasInteractionSessionTests
{
    [Fact]
    public void BeginCanvasSelection_CapturesSelectionStateAndClearsDrag()
    {
        var session = new NodeCanvasInteractionSession();
        var baseline = new[] { CreateNode("node-001") };

        session.BeginCanvasSelection(new Point(10, 20), KeyModifiers.Shift, baseline);

        Assert.False(session.IsPanning);
        Assert.False(session.IsMarqueeSelecting);
        Assert.Equal(KeyModifiers.Shift, session.SelectionModifiers);
        Assert.Equal(new Point(10, 20), session.SelectionStartScreenPosition);
        Assert.Equal(new Point(10, 20), session.LastPointerPosition);
        Assert.Equal(new Point(10, 20), session.PointerScreenPosition);
        Assert.Single(session.SelectionBaselineNodes);
        Assert.Null(session.DragNode);
        Assert.Null(session.DragStartScreenPosition);
        Assert.Null(session.DragSession);
    }

    [Fact]
    public void BeginNodeDrag_CapturesDragStateAndClearsSelectionStart()
    {
        var session = new NodeCanvasInteractionSession();
        var dragNode = CreateNode("node-002");
        var dragSession = new NodeCanvasDragSession(
            [dragNode],
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [dragNode.Id] = new GraphPoint(100, 200),
            },
            new NodeBounds(100, 200, 220, 160));

        session.BeginCanvasSelection(new Point(10, 20), KeyModifiers.Control, [CreateNode("node-001")]);
        session.BeginNodeDrag(dragNode, new Point(30, 40), dragSession);

        Assert.Equal(dragNode, session.DragNode);
        Assert.Equal(new Point(30, 40), session.DragStartScreenPosition);
        Assert.Equal(new Point(30, 40), session.LastPointerPosition);
        Assert.Equal(new Point(30, 40), session.PointerScreenPosition);
        Assert.Equal(dragSession, session.DragSession);
        Assert.Null(session.SelectionStartScreenPosition);
        Assert.False(session.IsPanning);
        Assert.False(session.IsMarqueeSelecting);
    }

    [Fact]
    public void TryBeginMarqueeSelection_OnlyActivatesAfterThreshold()
    {
        var session = new NodeCanvasInteractionSession();
        session.BeginCanvasSelection(new Point(10, 10), KeyModifiers.None, []);

        Assert.False(session.TryBeginMarqueeSelection(new Point(12, 13), 6));
        Assert.False(session.IsMarqueeSelecting);

        Assert.True(session.TryBeginMarqueeSelection(new Point(20, 10), 6));
        Assert.True(session.IsMarqueeSelecting);
    }

    [Fact]
    public void TryBeginLassoSelection_WithLassoGestureKind_RecordsStartAndMoveAfterThreshold()
    {
        var session = new NodeCanvasInteractionSession();
        session.BeginCanvasSelection(
            new Point(10, 10),
            KeyModifiers.None,
            [],
            NodeCanvasSelectionGestureKind.Lasso);

        Assert.Equal(NodeCanvasSelectionGestureKind.Lasso, session.SelectionGestureKind);
        Assert.False(session.TryBeginLassoSelection(new Point(12, 13), 6));
        Assert.False(session.IsLassoSelecting);
        Assert.Empty(session.LassoScreenPoints);

        Assert.True(session.TryBeginLassoSelection(new Point(20, 10), 6));
        Assert.True(session.IsLassoSelecting);
        Assert.False(session.IsMarqueeSelecting);
        Assert.Equal(
            [new Point(10, 10), new Point(20, 10)],
            session.LassoScreenPoints);

        session.RecordLassoSelectionPoint(new Point(24, 18));

        Assert.Equal(
            [new Point(10, 10), new Point(20, 10), new Point(24, 18)],
            session.LassoScreenPoints);
    }

    [Fact]
    public void BeginWhiteboardPrimitiveDrawing_WithRectangle_CreatesCreatingPreviewAndNormalizesUpdateGeometry()
    {
        var session = new NodeCanvasInteractionSession();

        session.BeginWhiteboardPrimitiveDrawing(
            GraphWhiteboardPrimitiveKind.Rectangle,
            new Point(60, 80),
            new GraphPoint(20, 30));

        Assert.NotNull(session.ActiveWhiteboardPrimitive);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Rectangle, session.ActiveWhiteboardPrimitive.Kind);
        Assert.Equal(new GraphPoint(20, 30), session.ActiveWhiteboardPrimitive.Geometry.Origin);
        Assert.Equal(new GraphSize(0, 0), session.ActiveWhiteboardPrimitive.Geometry.Size);
        Assert.Empty(session.ActiveWhiteboardPrimitive.Geometry.Points);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Creating, session.ActiveWhiteboardPrimitive.EditLifecycle.State);
        Assert.Equal([new Point(60, 80)], session.WhiteboardGestureScreenPoints);
        Assert.Null(session.SelectionStartScreenPosition);

        session.UpdateWhiteboardPrimitiveDrawing(new Point(20, 40), new GraphPoint(0, 10));

        Assert.NotNull(session.ActiveWhiteboardPrimitive);
        Assert.Equal(new GraphPoint(0, 10), session.ActiveWhiteboardPrimitive.Geometry.Origin);
        Assert.Equal(new GraphSize(20, 20), session.ActiveWhiteboardPrimitive.Geometry.Size);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Editing, session.ActiveWhiteboardPrimitive.EditLifecycle.State);
        Assert.Equal([new Point(60, 80), new Point(20, 40)], session.WhiteboardGestureScreenPoints);
    }

    [Fact]
    public void CommitWhiteboardPrimitiveDrawing_WithFreehand_StoresCommittedPrimitiveAndClearsActiveGesture()
    {
        var session = new NodeCanvasInteractionSession();

        session.BeginWhiteboardPrimitiveDrawing(
            GraphWhiteboardPrimitiveKind.Freehand,
            new Point(10, 20),
            new GraphPoint(4, 8));
        session.UpdateWhiteboardPrimitiveDrawing(new Point(18, 28), new GraphPoint(8, 12));

        var committed = session.CommitWhiteboardPrimitiveDrawing(new Point(6, 32), new GraphPoint(2, 14));

        Assert.NotNull(committed);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Freehand, committed.Kind);
        Assert.Equal(new GraphPoint(2, 8), committed.Geometry.Origin);
        Assert.Equal(new GraphSize(6, 6), committed.Geometry.Size);
        Assert.Equal(
            [new GraphPoint(4, 8), new GraphPoint(8, 12), new GraphPoint(2, 14)],
            committed.Geometry.Points);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Committed, committed.EditLifecycle.State);
        Assert.Null(session.ActiveWhiteboardPrimitive);
        Assert.Empty(session.WhiteboardGestureScreenPoints);
        Assert.Equal([committed], session.WhiteboardPrimitives);
    }

    [Fact]
    public void ResetAfterPointerRelease_CancelsActiveWhiteboardPrimitiveWithoutClearingCommittedPrimitives()
    {
        var session = new NodeCanvasInteractionSession();

        session.BeginWhiteboardPrimitiveDrawing(
            GraphWhiteboardPrimitiveKind.Rectangle,
            new Point(10, 20),
            new GraphPoint(10, 20));
        var committed = session.CommitWhiteboardPrimitiveDrawing(new Point(30, 50), new GraphPoint(30, 50));
        Assert.NotNull(committed);

        session.BeginWhiteboardPrimitiveDrawing(
            GraphWhiteboardPrimitiveKind.Freehand,
            new Point(12, 18),
            new GraphPoint(12, 18));
        session.UpdateWhiteboardPrimitiveDrawing(new Point(20, 24), new GraphPoint(20, 24));

        session.ResetAfterPointerRelease();

        Assert.Null(session.ActiveWhiteboardPrimitive);
        Assert.Empty(session.WhiteboardGestureScreenPoints);
        Assert.Equal([committed], session.WhiteboardPrimitives);
    }

    [Fact]
    public void TryEraseWhiteboardPrimitive_RemovesTopmostHitPrimitiveWithoutClearingOthers()
    {
        var session = new NodeCanvasInteractionSession();

        session.BeginWhiteboardPrimitiveDrawing(
            GraphWhiteboardPrimitiveKind.Rectangle,
            new Point(10, 20),
            new GraphPoint(10, 20));
        var lower = session.CommitWhiteboardPrimitiveDrawing(new Point(90, 68), new GraphPoint(90, 68));
        Assert.NotNull(lower);

        session.BeginWhiteboardPrimitiveDrawing(
            GraphWhiteboardPrimitiveKind.Rectangle,
            new Point(24, 28),
            new GraphPoint(24, 28));
        var upper = session.CommitWhiteboardPrimitiveDrawing(new Point(60, 52), new GraphPoint(60, 52));
        Assert.NotNull(upper);

        var erased = session.TryEraseWhiteboardPrimitive(new GraphPoint(30, 34));

        Assert.True(erased);
        Assert.Equal([lower], session.WhiteboardPrimitives);
        Assert.Null(session.ActiveWhiteboardPrimitive);
        Assert.Null(session.WhiteboardPrimitiveGesture);
        Assert.False(session.TryEraseWhiteboardPrimitive(new GraphPoint(500, 500)));
        Assert.Equal([lower], session.WhiteboardPrimitives);
    }

    [Fact]
    public void BeginPanning_ClearsOtherInteractionModes()
    {
        var session = new NodeCanvasInteractionSession();
        var dragNode = CreateNode("node-003");
        var dragSession = new NodeCanvasDragSession(
            [dragNode],
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [dragNode.Id] = new GraphPoint(50, 60),
            },
            new NodeBounds(50, 60, 220, 160));

        session.BeginNodeDrag(dragNode, new Point(5, 6), dragSession);
        session.BeginPanning(new Point(40, 50));

        Assert.True(session.IsPanning);
        Assert.False(session.IsMarqueeSelecting);
        Assert.Null(session.DragNode);
        Assert.Null(session.DragStartScreenPosition);
        Assert.Null(session.DragSession);
        Assert.Null(session.SelectionStartScreenPosition);
        Assert.Equal(new Point(40, 50), session.LastPointerPosition);
        Assert.Equal(new Point(40, 50), session.PointerScreenPosition);
    }

    [Fact]
    public void ResetAfterPointerRelease_ClearsTransientInteractionState()
    {
        var session = new NodeCanvasInteractionSession();
        var dragNode = CreateNode("node-004");
        var dragSession = new NodeCanvasDragSession(
            [dragNode],
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [dragNode.Id] = new GraphPoint(70, 80),
            },
            new NodeBounds(70, 80, 220, 160));

        session.BeginCanvasSelection(new Point(1, 2), KeyModifiers.Shift, [CreateNode("node-005")]);
        session.TryBeginMarqueeSelection(new Point(20, 20), 6);
        session.BeginNodeDrag(dragNode, new Point(30, 40), dragSession);
        session.ResetAfterPointerRelease();

        Assert.False(session.IsPanning);
        Assert.False(session.IsMarqueeSelecting);
        Assert.Empty(session.SelectionBaselineNodes);
        Assert.Null(session.SelectionStartScreenPosition);
        Assert.Null(session.DragNode);
        Assert.Null(session.DragStartScreenPosition);
        Assert.Null(session.DragSession);
    }

    private static NodeViewModel CreateNode(string id)
        => new(new GraphNode(
            id,
            id,
            "Tests",
            "Interaction",
            "Test node",
            new GraphPoint(0, 0),
            new GraphSize(220, 160),
            [],
            [],
            "#55D8C1",
            new NodeDefinitionId("tests.interaction.node")));
}
