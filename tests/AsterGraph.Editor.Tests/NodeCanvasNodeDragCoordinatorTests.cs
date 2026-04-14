using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasNodeDragCoordinatorTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.nodedrag.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.nodedrag.target");

    [AvaloniaFact]
    public void BeginNodeDrag_WhenLeftButtonIsNotPressed_IgnoresInteraction()
    {
        var editor = CreateEditor();
        var sourceNode = editor.Nodes[0];
        var host = new TestNodeDragHost(editor, [sourceNode]);
        var coordinator = new NodeCanvasNodeDragCoordinator(host);

        var result = coordinator.BeginNodeDrag(sourceNode, new Point(24, 36), isLeftButtonPressed: false, KeyModifiers.None);

        Assert.False(result.Handled);
        Assert.False(result.CapturePointer);
        Assert.Null(host.InteractionSession.DragNode);
        Assert.Null(host.InteractionSession.DragStartScreenPosition);
        Assert.Empty(editor.SelectedNodes);
    }

    [AvaloniaFact]
    public void BeginNodeDrag_WithControlModifier_TogglesSelectionWithoutCapturingPointer()
    {
        var editor = CreateEditor();
        var sourceNode = editor.Nodes[0];
        var host = new TestNodeDragHost(editor, [sourceNode]);
        var coordinator = new NodeCanvasNodeDragCoordinator(host);

        var result = coordinator.BeginNodeDrag(sourceNode, new Point(24, 36), isLeftButtonPressed: true, KeyModifiers.Control);

        Assert.True(result.Handled);
        Assert.False(result.CapturePointer);
        var selection = Assert.Single(editor.SelectedNodes);
        Assert.Same(sourceNode, selection);
        Assert.Null(host.InteractionSession.DragNode);
        Assert.Equal(0, host.HideSelectionAdornerCalls);
        Assert.Equal(0, host.HideGuideAdornerCalls);
    }

    [AvaloniaFact]
    public void BeginNodeDrag_WithPlainLeftPress_SelectsNodeReordersVisualAndStartsDragSession()
    {
        var editor = CreateEditor();
        var sourceNode = editor.Nodes[0];
        var targetNode = editor.Nodes[1];
        var host = new TestNodeDragHost(editor, [sourceNode, targetNode]);
        var coordinator = new NodeCanvasNodeDragCoordinator(host);

        var result = coordinator.BeginNodeDrag(sourceNode, new Point(64, 72), isLeftButtonPressed: true, KeyModifiers.None);

        Assert.True(result.Handled);
        Assert.True(result.CapturePointer);
        Assert.Same(sourceNode, editor.SelectedNode);
        Assert.Same(sourceNode, host.InteractionSession.DragNode);
        Assert.Equal(new Point(64, 72), host.InteractionSession.DragStartScreenPosition);
        Assert.NotNull(host.InteractionSession.DragSession);
        Assert.Equal(1, host.HideSelectionAdornerCalls);
        Assert.Equal(1, host.HideGuideAdornerCalls);
        Assert.Same(sourceNode, host.LastCreateDragSessionNodes!.Single());
        Assert.Same(host.NodeVisuals[sourceNode].Root, host.NodeLayer!.Children[^1]);
    }

    private static GraphEditorViewModel CreateEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                SourceDefinitionId,
                "Node Drag Source",
                "Tests",
                "Node drag test source node.",
                [],
                [
                    new PortDefinition(
                        "out",
                        "Result",
                        new PortTypeId("float"),
                        "#6AD5C4"),
                ]));
        catalog.RegisterDefinition(
            new NodeDefinition(
                TargetDefinitionId,
                "Node Drag Target",
                "Tests",
                "Node drag test target node.",
                [
                    new PortDefinition(
                        "in",
                        "Input",
                        new PortTypeId("float"),
                        "#F3B36B"),
                ],
                []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Node Drag Graph",
                "Regression coverage for node drag coordination extraction.",
                [
                    new GraphNode(
                        "tests.nodedrag.source-001",
                        "Node Drag Source",
                        "Tests",
                        "Drag",
                        "Source node for drag coordinator tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [
                            new GraphPort(
                                "out",
                                "Result",
                                PortDirection.Output,
                                "float",
                                "#6AD5C4",
                                new PortTypeId("float")),
                        ],
                        "#6AD5C4",
                        SourceDefinitionId),
                    new GraphNode(
                        "tests.nodedrag.target-001",
                        "Node Drag Target",
                        "Tests",
                        "Drag",
                        "Target node for drag coordinator tests.",
                        new GraphPoint(420, 160),
                        new GraphSize(240, 160),
                        [
                            new GraphPort(
                                "in",
                                "Input",
                                PortDirection.Input,
                                "float",
                                "#F3B36B",
                                new PortTypeId("float")),
                        ],
                        [],
                        "#F3B36B",
                        TargetDefinitionId),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private sealed class TestNodeDragHost : INodeCanvasNodeDragHost
    {
        public TestNodeDragHost(GraphEditorViewModel editor, IReadOnlyList<NodeViewModel> nodes)
        {
            ViewModel = editor;
            NodeLayer = new Canvas();

            foreach (var node in nodes)
            {
                var root = new Border
                {
                    Width = node.Width,
                    Height = node.Height,
                };
                var visual = new GraphNodeVisual(root, new Dictionary<string, Control>(StringComparer.Ordinal));
                NodeVisuals[node] = new NodeCanvasRenderedNodeVisual(root, new TestGraphNodeVisualPresenter(), visual);
                NodeLayer.Children.Add(root);
            }
        }

        public GraphEditorViewModel? ViewModel { get; }

        public Canvas? NodeLayer { get; }

        public Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals { get; } = new();

        public NodeCanvasInteractionSession InteractionSession { get; } = new();

        public IReadOnlyList<NodeViewModel>? LastCreateDragSessionNodes { get; private set; }

        public int HideSelectionAdornerCalls { get; private set; }

        public int HideGuideAdornerCalls { get; private set; }

        public void FocusCanvas()
        {
        }

        public void HideSelectionAdorner()
            => HideSelectionAdornerCalls++;

        public void HideGuideAdorners()
            => HideGuideAdornerCalls++;

        public void BringNodeVisualToFront(NodeViewModel node)
        {
            if (NodeLayer is null || !NodeVisuals.TryGetValue(node, out var visual))
            {
                return;
            }

            NodeLayer.Children.Remove(visual.Root);
            NodeLayer.Children.Add(visual.Root);
        }

        public NodeCanvasDragSession CreateDragSession(IReadOnlyList<NodeViewModel> nodes)
        {
            LastCreateDragSessionNodes = nodes.ToList();
            var originPositions = nodes.ToDictionary(
                node => node.Id,
                node => new GraphPoint(node.X, node.Y),
                StringComparer.Ordinal);
            var first = nodes[0];
            return new NodeCanvasDragSession(
                nodes,
                originPositions,
                new NodeBounds(first.X, first.Y, first.Width, first.Height));
        }
    }

    private sealed class TestGraphNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        public GraphNodeVisual Create(GraphNodeVisualContext context)
            => throw new NotSupportedException();

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        {
        }
    }
}
