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
