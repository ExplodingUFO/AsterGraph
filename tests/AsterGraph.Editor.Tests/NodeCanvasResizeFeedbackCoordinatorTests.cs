using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasResizeFeedbackCoordinatorTests
{
    [AvaloniaFact]
    public void Update_WithDenseOverlappingNodeLayer_AppliesResizeHoverToTopmostSurface()
    {
        var editor = CreateEditor();
        editor.Zoom = 1d;
        editor.PanX = 0d;
        editor.PanY = 0d;
        var nodeLayer = new Canvas();
        var surfaces = new Dictionary<Control, NodeViewModel>();

        for (var index = 0; index < 600; index++)
        {
            var surface = new Border
            {
                Width = 240d,
                Height = 160d,
            };
            surface.Arrange(new Rect(0d, 0d, 240d, 160d));
            nodeLayer.Children.Add(surface);
            surfaces[surface] = CreateNode($"tests.resize.dense-{index:000}");
        }

        var host = new TestResizeFeedbackHost(editor, nodeLayer, surfaces);
        var coordinator = new NodeCanvasResizeFeedbackCoordinator(host);
        var topmostSurface = Assert.IsAssignableFrom<Control>(nodeLayer.Children[^1]);

        coordinator.Update(new Point(237d, 80d));

        AssertCursor(StandardCursorType.SizeWestEast, topmostSurface.Cursor);
        Assert.All(nodeLayer.Children.Take(nodeLayer.Children.Count - 1).OfType<Control>(), surface => Assert.Null(surface.Cursor));
    }

    [AvaloniaFact]
    public void Update_WithRotatedNodeLayer_MapsWorldPointIntoNodeLocalResizeHandle()
    {
        var editor = CreateEditor();
        editor.Zoom = 1d;
        editor.PanX = 0d;
        editor.PanY = 0d;
        var nodeLayer = new Canvas();
        var rotatedNode = CreateNode("tests.resize.rotated-001", rotationDegrees: 90d);
        var surface = new Border
        {
            Width = 240d,
            Height = 160d,
        };
        surface.Arrange(new Rect(0d, 0d, 240d, 160d));
        nodeLayer.Children.Add(surface);

        var host = new TestResizeFeedbackHost(
            editor,
            nodeLayer,
            new Dictionary<Control, NodeViewModel>
            {
                [surface] = rotatedNode,
            });
        var coordinator = new NodeCanvasResizeFeedbackCoordinator(host);
        var rotatedRightEdgeWorldPoint = GraphNodeRotationGeometry.TransformLocalToWorld(
            new GraphPoint(rotatedNode.X, rotatedNode.Y),
            new GraphSize(rotatedNode.Width, rotatedNode.Height),
            rotatedNode.RotationDegrees,
            new GraphPoint(236d, 80d));

        coordinator.Update(new Point(rotatedRightEdgeWorldPoint.X, rotatedRightEdgeWorldPoint.Y));

        AssertCursor(StandardCursorType.SizeWestEast, surface.Cursor);
    }

    private static GraphEditorViewModel CreateEditor()
        => new(
            new GraphDocument(
                "Dense Resize Feedback Graph",
                "Regression coverage for dense resize-feedback hit testing.",
                [],
                []),
            new NodeCatalog(),
            new DefaultPortCompatibilityService());

    private static NodeViewModel CreateNode(string id, double rotationDegrees = 0d)
        => new(new GraphNode(
            id,
            "Dense Node",
            "Tests",
            "Resize Feedback",
            "Dense hit-test node.",
            new GraphPoint(0d, 0d),
            new GraphSize(240d, 160d),
            [],
            [],
            "#6AD5C4",
            new NodeDefinitionId("tests.resize.dense"),
            Surface: new GraphNodeSurfaceState(RotationDegrees: rotationDegrees)));

    private static void AssertCursor(StandardCursorType expected, Cursor? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.ToString(), actual!.ToString());
    }

    private sealed class TestResizeFeedbackHost : INodeCanvasResizeFeedbackHost
    {
        public TestResizeFeedbackHost(
            GraphEditorViewModel viewModel,
            Canvas nodeLayer,
            IReadOnlyDictionary<Control, NodeViewModel> resizeFeedbackNodeSurfaces)
        {
            ViewModel = viewModel;
            NodeLayer = nodeLayer;
            ResizeFeedbackNodeSurfaces = resizeFeedbackNodeSurfaces;
        }

        public GraphEditorViewModel? ViewModel { get; }

        public Control Root { get; } = new Border();

        public Canvas? NodeLayer { get; }

        public Canvas? GroupLayer => null;

        public IReadOnlyDictionary<Control, NodeViewModel> ResizeFeedbackNodeSurfaces { get; }

        public IReadOnlyDictionary<Border, string> ResizeFeedbackGroupSurfaces { get; } =
            new Dictionary<Border, string>();

        public IReadOnlyDictionary<string, GraphEditorNodeGroupSnapshot> ResizeFeedbackGroupSnapshots { get; } =
            new Dictionary<string, GraphEditorNodeGroupSnapshot>();

        public IGraphResizeFeedbackPolicy? ResizeFeedbackPolicy => null;
    }
}
