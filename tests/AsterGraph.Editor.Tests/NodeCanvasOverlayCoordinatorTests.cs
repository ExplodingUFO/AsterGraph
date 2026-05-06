using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasOverlayCoordinatorTests
{
    [AvaloniaFact]
    public void CreateDragSession_ComputesSelectionBoundsAcrossNodes()
    {
        var host = new TestOverlayHost();
        var coordinator = new NodeCanvasOverlayCoordinator(host);
        var first = CreateNode("node-001", 10, 20, 180, 160);
        var second = CreateNode("node-002", 260, 140, 200, 180);

        var session = coordinator.CreateDragSession([first, second]);

        Assert.Equal(2, session.Nodes.Count);
        Assert.Equal(new GraphPoint(10, 20), session.OriginPositions[first.Id]);
        Assert.Equal(new GraphPoint(260, 140), session.OriginPositions[second.Id]);
        var expectedRight = Math.Max(first.X + first.Width, second.X + second.Width);
        var expectedBottom = Math.Max(first.Y + first.Height, second.Y + second.Height);
        Assert.Equal(
            new NodeBounds(10, 20, expectedRight - 10, expectedBottom - 20),
            session.OriginBounds);
    }

    [AvaloniaFact]
    public void ShowGuideAdorners_UsesScreenProjectionAndBounds()
    {
        var host = new TestOverlayHost
        {
            StyleOptions = GraphEditorStyleOptions.Default with
            {
                Canvas = GraphEditorStyleOptions.Default.Canvas with
                {
                    GuideThickness = 4,
                },
            },
            Bounds = new Size(640, 360),
            ScreenProjector = (x, y) => new GraphPoint(x + 8, y + 12),
        };
        var coordinator = new NodeCanvasOverlayCoordinator(host);

        coordinator.ApplyGuideAdornerStyle();
        coordinator.ShowGuideAdorners(120, 90);

        Assert.True(host.VerticalGuideAdorner!.IsVisible);
        Assert.Equal(360, host.VerticalGuideAdorner.Height);
        Assert.Equal(126, Canvas.GetLeft(host.VerticalGuideAdorner));
        Assert.Equal(0, Canvas.GetTop(host.VerticalGuideAdorner));

        Assert.True(host.HorizontalGuideAdorner!.IsVisible);
        Assert.Equal(640, host.HorizontalGuideAdorner.Width);
        Assert.Equal(0, Canvas.GetLeft(host.HorizontalGuideAdorner));
        Assert.Equal(100, Canvas.GetTop(host.HorizontalGuideAdorner));

        coordinator.HideGuideAdorners();

        Assert.False(host.VerticalGuideAdorner.IsVisible);
        Assert.False(host.HorizontalGuideAdorner.IsVisible);
    }

    [AvaloniaFact]
    public void ApplyDragAssist_WhenAssistDisabled_ReturnsRawDeltaAndHidesGuides()
    {
        var host = new TestOverlayHost
        {
            BehaviorOptions = GraphEditorBehaviorOptions.Default with
            {
                DragAssist = GraphEditorBehaviorOptions.Default.DragAssist with
                {
                    EnableGridSnapping = false,
                    EnableAlignmentGuides = false,
                },
            },
        };
        host.VerticalGuideAdorner!.IsVisible = true;
        host.HorizontalGuideAdorner!.IsVisible = true;
        var coordinator = new NodeCanvasOverlayCoordinator(host);
        var node = CreateNode("node-003", 40, 60, 220, 160);
        var dragSession = coordinator.CreateDragSession([node]);

        var adjusted = coordinator.ApplyDragAssist(dragSession, 14, -6);

        Assert.Equal(new GraphPoint(14, -6), adjusted);
        Assert.False(host.VerticalGuideAdorner.IsVisible);
        Assert.False(host.HorizontalGuideAdorner.IsVisible);
    }

    [AvaloniaFact]
    public void UpdateMarqueeSelection_WithControlModifier_TogglesAgainstBaselineAndSetsFinalizeStatus()
    {
        var host = new TestOverlayHost();
        var coordinator = new NodeCanvasOverlayCoordinator(host);
        var first = CreateNode("node-001", 10, 20, 180, 160);
        var second = CreateNode("node-002", 240, 120, 180, 160);
        host.HitNodes = [first, second];
        host.Nodes = [first, second];
        host.InteractionSession.BeginCanvasSelection(new Point(10, 15), KeyModifiers.Control, [first]);

        coordinator.UpdateMarqueeSelection(new Point(70, 55), finalize: true);

        Assert.True(host.SelectionAdorner!.IsVisible);
        Assert.Equal(60, host.SelectionAdorner.Width);
        Assert.Equal(40, host.SelectionAdorner.Height);
        Assert.Equal(10, Canvas.GetLeft(host.SelectionAdorner));
        Assert.Equal(15, Canvas.GetTop(host.SelectionAdorner));
        var selection = Assert.Single(host.LastSelection!);
        Assert.Same(second, selection);
        Assert.Same(second, host.LastPrimaryNode);
        Assert.Equal($"Selected {second.Title}.", host.LastStatus);
    }

    [AvaloniaFact]
    public void UpdateMarqueeSelection_WithFinalizeTrue_UsesBackendSelectionRectangleQuery()
    {
        var host = new TestOverlayHost();
        var coordinator = new NodeCanvasOverlayCoordinator(host);
        var first = CreateNode("node-020", 10, 20, 180, 160);
        var second = CreateNode("node-021", 240, 120, 180, 160);
        host.HitNodes = [first, second];
        host.Nodes = [first, second];
        host.InteractionSession.BeginCanvasSelection(new Point(10, 15), KeyModifiers.None, []);

        coordinator.UpdateMarqueeSelection(new Point(70, 55), finalize: true);

        Assert.Equal(1, host.GetSelectionRectangleSnapshotCalls);
        Assert.Equal(2, host.LastSelection!.Count);
        Assert.Equal("Selected 2 nodes.", host.LastStatus);
    }

    [AvaloniaFact]
    public void UpdateMarqueeSelection_WithFinalizeFalse_UsesFastFrontendPath()
    {
        var host = new TestOverlayHost();
        var coordinator = new NodeCanvasOverlayCoordinator(host);
        var first = CreateNode("node-030", 10, 20, 180, 160);
        host.HitNodes = [first];
        host.Nodes = [first];
        host.InteractionSession.BeginCanvasSelection(new Point(10, 15), KeyModifiers.None, []);

        coordinator.UpdateMarqueeSelection(new Point(70, 55), finalize: false);

        Assert.Equal(0, host.GetSelectionRectangleSnapshotCalls);
        Assert.Single(host.LastSelection!);
    }

    [AvaloniaFact]
    public void UpdateMarqueeSelection_WithShiftModifier_UnionsBaselineAndHitNodes()
    {
        var host = new TestOverlayHost();
        var coordinator = new NodeCanvasOverlayCoordinator(host);
        var first = CreateNode("node-010", 20, 30, 180, 160);
        var second = CreateNode("node-011", 280, 150, 180, 160);
        host.HitNodes = [second];
        host.InteractionSession.BeginCanvasSelection(new Point(5, 8), KeyModifiers.Shift, [first]);

        coordinator.UpdateMarqueeSelection(new Point(45, 48), finalize: false);

        Assert.NotNull(host.LastSelection);
        Assert.Collection(
            host.LastSelection!,
            node => Assert.Same(first, node),
            node => Assert.Same(second, node));
        Assert.Same(second, host.LastPrimaryNode);
        Assert.Null(host.LastStatus);
    }

    private static NodeViewModel CreateNode(string id, double x, double y, double width, double height)
        => new(new GraphNode(
            id,
            id,
            "Tests",
            "Overlay",
            "Overlay test node",
            new GraphPoint(x, y),
            new GraphSize(width, height),
            [],
            [],
            "#55D8C1",
            new NodeDefinitionId("tests.overlay.node")));

    private sealed class TestOverlayHost : INodeCanvasOverlayHost
    {
        public GraphEditorStyleOptions StyleOptions { get; set; } = GraphEditorStyleOptions.Default;

        public GraphEditorBehaviorOptions BehaviorOptions { get; set; } = GraphEditorBehaviorOptions.Default;

        public double Zoom { get; set; } = 1;

        public IReadOnlyList<NodeViewModel> Nodes { get; set; } = [];

        public IReadOnlyList<GraphEditorNodeGroupSnapshot> GroupSnapshots { get; set; } = [];

        public Size Bounds { get; set; } = new(800, 600);

        public Border? SelectionAdorner { get; set; } = new();

        public Border? VerticalGuideAdorner { get; set; } = new();

        public Border? HorizontalGuideAdorner { get; set; } = new();

        public NodeCanvasInteractionSession InteractionSession { get; } = new();

        public IReadOnlyList<NodeViewModel> SelectedNodes { get; private set; } = [];

        public NodeViewModel? SelectedNode { get; private set; }

        public IReadOnlyList<NodeViewModel> HitNodes { get; set; } = [];

        public IReadOnlyList<NodeViewModel>? LastSelection { get; private set; }

        public NodeViewModel? LastPrimaryNode { get; private set; }

        public string? LastStatus { get; private set; }

        public Func<double, double, GraphPoint> ScreenProjector { get; set; } = static (x, y) => new GraphPoint(x, y);

        public Func<GraphPoint, GraphPoint> WorldProjector { get; set; } = static point => point;

        public GraphPoint WorldToScreen(double x, double y)
            => ScreenProjector(x, y);

        public GraphPoint ScreenToWorld(GraphPoint point)
            => WorldProjector(point);

        public IReadOnlyList<NodeViewModel> GetNodesInRectangle(GraphPoint firstCorner, GraphPoint secondCorner)
            => HitNodes;

        public int GetSelectionRectangleSnapshotCalls { get; private set; }

        public GraphEditorSelectionRectangleSnapshot GetSelectionRectangleSnapshot(GraphPoint firstCorner, GraphPoint secondCorner)
        {
            GetSelectionRectangleSnapshotCalls++;
            return new(Nodes.Select(n => n.Id).ToList(), []);
        }

        public void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode, string? status = null)
        {
            LastSelection = nodes.ToList();
            LastPrimaryNode = primaryNode;
            LastStatus = status;
            SelectedNodes = nodes.ToList();
            SelectedNode = primaryNode;
        }
    }
}
