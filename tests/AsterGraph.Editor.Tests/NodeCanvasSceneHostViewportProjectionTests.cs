using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasSceneHostViewportProjectionTests
{
    private static readonly NodeDefinitionId NodeDefinitionId = new("tests.scene-host.viewport");

    [AvaloniaFact]
    public void RebuildScene_BoundsNodeAndGroupVisualsToVisibleProjectionBudget()
    {
        var editor = CreateEditor();
        editor.UpdateViewportSize(640d, 420d);
        var host = new TestSceneHost(editor);
        var sceneHost = new NodeCanvasSceneHost(host);

        sceneHost.RebuildScene();

        Assert.NotNull(sceneHost.LastVisibleSceneProjection);
        Assert.Equal(
            "VISIBLE_SCENE_PROJECTION:scene-host:nodes=2/3:connections=1/2:groups=1/2:overscan=96",
            sceneHost.LastVisibleSceneProjection.ToBudgetMarker("scene-host"));
        Assert.Equal(["node-a", "node-b"], host.NodeVisuals.Keys.Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal(["group-visible"], host.GroupVisuals.Keys.OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal(2, host.NodeLayer!.Children.Count);
        Assert.Single(host.GroupLayer!.Children);
    }

    [AvaloniaFact]
    public void UpdateViewportTransform_ReportsVisibleSceneInvalidationDiffMarker()
    {
        var editor = CreateEditor();
        editor.UpdateViewportSize(640d, 420d);
        var host = new TestSceneHost(editor);
        var sceneHost = new NodeCanvasSceneHost(host);

        sceneHost.UpdateViewportTransform();
        editor.PanBy(-720d, -560d);
        sceneHost.UpdateViewportTransform();

        Assert.NotNull(sceneHost.LastVisibleSceneProjection);
        Assert.Equal(
            "VISIBLE_SCENE_INVALIDATION:nodes=3:connections=2:groups=2:total=7",
            sceneHost.LastVisibleSceneInvalidationMarker);
    }

    private static GraphEditorViewModel CreateEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            NodeDefinitionId,
            "Viewport Node",
            "Tests",
            "Scene host viewport projection node.",
            [],
            [
                new PortDefinition("out", "Out", new PortTypeId("float"), "#6AD5C4"),
            ]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Scene Host Viewport Projection",
                "Regression coverage for scene-host visible-scene invalidation markers.",
                [
                    CreateNode("node-a", 40d, 40d),
                    CreateNode("node-b", 360d, 180d),
                    CreateNode("node-c", 980d, 760d),
                ],
                [
                    CreateConnection("connection-visible", "node-a", "node-b"),
                    CreateConnection("connection-hidden", "node-c", "node-missing"),
                ],
                [
                    new GraphNodeGroup("group-visible", "Visible", new GraphPoint(20d, 20d), new GraphSize(580d, 340d), ["node-a", "node-b"]),
                    new GraphNodeGroup("group-hidden", "Hidden", new GraphPoint(900d, 700d), new GraphSize(180d, 140d), ["node-c"]),
                ]),
            catalog,
            new DefaultPortCompatibilityService(),
            styleOptions: GraphEditorStyleOptions.Default);
    }

    private static GraphNode CreateNode(string id, double x, double y)
        => new(
            id,
            id,
            "Tests",
            "Projection",
            "Viewport projection node.",
            new GraphPoint(x, y),
            new GraphSize(180d, 120d),
            [],
            [
                new GraphPort("out", "Out", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
            ],
            "#6AD5C4",
            NodeDefinitionId);

    private static GraphConnection CreateConnection(string id, string sourceNodeId, string targetNodeId)
        => new(
            id,
            sourceNodeId,
            "out",
            targetNodeId,
            "out",
            id,
            "#6AD5C4");

    private sealed class TestSceneHost : INodeCanvasSceneHost
    {
        private readonly Grid _coordinateRoot = new();
        private readonly StubNodeVisualPresenter _stockNodeVisualPresenter = new();
        private readonly NodeCanvasContextMenuCoordinator _contextMenuCoordinator;

        public TestSceneHost(GraphEditorViewModel viewModel)
        {
            ViewModel = viewModel;
            SceneRoot = new Grid();
            GroupLayer = new Canvas();
            ConnectionLayer = new Canvas();
            NodeLayer = new Canvas();
            InteractionSession = new NodeCanvasInteractionSession();
            _contextMenuCoordinator = new NodeCanvasContextMenuCoordinator(new TestContextMenuHost(viewModel), _coordinateRoot);
        }

        public GraphEditorViewModel? ViewModel { get; }

        public Grid? SceneRoot { get; }

        public Canvas? GroupLayer { get; }

        public Canvas? ConnectionLayer { get; }

        public Canvas? NodeLayer { get; }

        public Control CoordinateRoot => _coordinateRoot;

        public GridBackground? BackgroundGrid => null;

        public Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals { get; } = [];

        public Dictionary<string, NodeCanvasRenderedGroupVisual> GroupVisuals { get; } = [];

        public IGraphNodeVisualPresenter? NodeVisualPresenter => null;

        public IGraphNodeBodyPresenter? NodeBodyPresenter => null;

        public IGraphNodeVisualPresenter StockNodeVisualPresenter => _stockNodeVisualPresenter;

        public INodeParameterEditorRegistry? NodeParameterEditorRegistry => null;

        public NodeCanvasInteractionSession InteractionSession { get; }

        public NodeCanvasContextMenuCoordinator ContextMenuCoordinator => _contextMenuCoordinator;

        public void FocusCanvas()
        {
        }

        public void BeginNodeDrag(NodeViewModel node, PointerPressedEventArgs args)
            => throw new NotSupportedException();

        public void BeginNodeResize(NodeViewModel node, GraphNodeResizeHandleKind handleKind, PointerPressedEventArgs args)
            => throw new NotSupportedException();

        public void BeginGroupDrag(GraphEditorNodeGroupSnapshot group, PointerPressedEventArgs args)
            => throw new NotSupportedException();

        public void BeginGroupResize(string groupId, string groupTitle, NodeCanvasGroupResizeEdge edge, PointerPressedEventArgs args)
            => throw new NotSupportedException();

        public void ActivatePort(NodeViewModel node, PortViewModel port)
            => throw new NotSupportedException();
    }

    private sealed class TestContextMenuHost : INodeCanvasContextMenuHost
    {
        public TestContextMenuHost(GraphEditorViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public GraphEditorViewModel? ViewModel { get; }

        public bool EnableDefaultContextMenu => false;

        public IGraphContextMenuPresenter? ContextMenuPresenter => null;

        public IGraphContextMenuPresenter StockContextMenuPresenter { get; } = new StubContextMenuPresenter();

        public void FocusCanvas()
        {
        }

        public GraphPoint ResolveWorldPosition(ContextRequestedEventArgs args, Control relativeTo)
            => new(0d, 0d);

        public NodeCanvasContextMenuSnapshot CreateContextMenuSnapshot()
            => new(null, [], []);
    }

    private sealed class StubNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        public GraphNodeVisual Create(GraphNodeVisualContext context)
        {
            var root = new Border
            {
                Width = context.Node.Width,
                Height = context.Node.Height,
            };

            return new GraphNodeVisual(root, new Dictionary<string, Control>(StringComparer.Ordinal));
        }

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        {
        }
    }

    private sealed class StubContextMenuPresenter : IGraphContextMenuPresenter
    {
        public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
        {
        }
    }
}
