using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasConnectionSceneRendererTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.connection-renderer.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.connection-renderer.target");
    private const string SourceNodeId = "tests.connection-renderer.source-001";
    private const string TargetNodeId = "tests.connection-renderer.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";
    private const string ConnectionId = "conn-001";

    [AvaloniaFact]
    public void GetPortAnchor_PrefersRenderedAnchorBeforeNodeFallback()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: false);
        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var sourcePort = sourceNode.Outputs.Single(port => port.Id == SourcePortId);
        var hostedScene = CreateHostedScene(editor);

        try
        {
            var context = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals);
            var anchor = renderer.GetPortAnchor(context, sourceNode, sourcePort);

            Assert.InRange(anchor.X, sourceNode.X + 36.5, sourceNode.X + 37.5);
            Assert.InRange(anchor.Y, sourceNode.Y + 24.5, sourceNode.Y + 25.5);

            var fallbackContext = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                new Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual>());
            var fallback = renderer.GetPortAnchor(fallbackContext, sourceNode, sourcePort);

            Assert.Equal(sourceNode.GetPortAnchor(sourcePort), fallback);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_RendersCommittedLabelPendingPreviewAndConnectionContextMenu()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: true);
        editor.StartConnection(SourceNodeId, SourcePortId);
        var hostedScene = CreateHostedScene(editor);
        var openedContexts = new List<ContextMenuContext>();

        try
        {
            var context = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals,
                pointerScreenPosition: new Point(760, 300),
                openContextMenu: (_, menuContext) =>
                {
                    openedContexts.Add(menuContext);
                    return true;
                });

            renderer.RenderConnections(context);

            Assert.Equal(3, hostedScene.ConnectionLayer.Children.Count);
            Assert.Equal(2, hostedScene.ConnectionLayer.Children.OfType<global::Avalonia.Controls.Shapes.Path>().Count());

            var chip = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<Border>());
            var label = Assert.IsType<TextBlock>(chip.Child);
            Assert.Equal("Float Flow", label.Text);

            var args = new ContextRequestedEventArgs
            {
                RoutedEvent = Control.ContextRequestedEvent,
            };
            chip.RaiseEvent(args);

            Assert.True(args.Handled);
            var menuContext = Assert.Single(openedContexts);
            Assert.Equal(ContextMenuTargetKind.Connection, menuContext.TargetKind);
            Assert.Equal(ConnectionId, menuContext.ClickedConnectionId);
            Assert.Equal(ConnectionId, menuContext.SelectedConnectionId);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    private static NodeCanvasConnectionSceneContext CreateSceneContext(
        GraphEditorViewModel editor,
        Canvas connectionLayer,
        Canvas nodeLayer,
        Control coordinateRoot,
        IReadOnlyDictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> nodeVisuals,
        Point? pointerScreenPosition = null,
        Func<Control, ContextMenuContext, bool>? openContextMenu = null)
        => new(
            editor,
            connectionLayer,
            nodeLayer,
            coordinateRoot,
            nodeVisuals,
            pointerScreenPosition,
            connection => GraphEditorStyleOptions.Default.Connection,
            () => new NodeCanvasContextMenuSnapshot(
                editor.SelectedNode?.Id,
                editor.SelectedNodes.Select(node => node.Id).ToList(),
                []),
            (_, _) => new GraphPoint(42, 24),
            openContextMenu ?? ((_, _) => false));

    private static HostedScene CreateHostedScene(GraphEditorViewModel editor)
    {
        var coordinateRoot = new Grid
        {
            Width = 1440,
            Height = 900,
        };
        var connectionLayer = new Canvas();
        var nodeLayer = new Canvas();
        coordinateRoot.Children.Add(connectionLayer);
        coordinateRoot.Children.Add(nodeLayer);

        var nodeVisuals = new Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual>();
        foreach (var node in editor.Nodes)
        {
            var rendered = CreateRenderedVisual(node);
            nodeVisuals[node] = rendered;
            nodeLayer.Children.Add(rendered.Root);
            Canvas.SetLeft(rendered.Root, node.X);
            Canvas.SetTop(rendered.Root, node.Y);
        }

        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = coordinateRoot,
        };
        window.Show();
        return new HostedScene(window, coordinateRoot, connectionLayer, nodeLayer, nodeVisuals);
    }

    private static NodeCanvasRenderedNodeVisual CreateRenderedVisual(NodeViewModel node)
    {
        var root = new Canvas
        {
            Width = node.Width,
            Height = node.Height,
        };
        var portAnchors = new Dictionary<string, Control>(StringComparer.Ordinal);
        var allPorts = node.Inputs.Concat(node.Outputs).ToList();
        for (var index = 0; index < allPorts.Count; index++)
        {
            var port = allPorts[index];
            var anchor = new Border
            {
                Width = 10,
                Height = 10,
                DataContext = port,
            };
            Canvas.SetLeft(anchor, 32 + (index * 24));
            Canvas.SetTop(anchor, 20 + (index * 18));
            root.Children.Add(anchor);
            portAnchors[port.Id] = anchor;
        }

        return new NodeCanvasRenderedNodeVisual(
            root,
            new StubNodeVisualPresenter(),
            new GraphNodeVisual(root, portAnchors));
    }

    private static GraphEditorViewModel CreateEditor(bool includeConnection)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                SourceDefinitionId,
                "Renderer Source",
                "Tests",
                "Connection renderer source node.",
                [],
                [
                    new PortDefinition(
                        SourcePortId,
                        "Result",
                        new PortTypeId("float"),
                        "#6AD5C4"),
                ]));
        catalog.RegisterDefinition(
            new NodeDefinition(
                TargetDefinitionId,
                "Renderer Target",
                "Tests",
                "Connection renderer target node.",
                [
                    new PortDefinition(
                        TargetPortId,
                        "Input",
                        new PortTypeId("float"),
                        "#F3B36B"),
                ],
                []));

        var connections = includeConnection
            ? new[]
            {
                new GraphConnection(
                    ConnectionId,
                    SourceNodeId,
                    SourcePortId,
                    TargetNodeId,
                    TargetPortId,
                    "Float Flow",
                    "#6AD5C4"),
            }
            : [];

        return new GraphEditorViewModel(
            new GraphDocument(
                "Connection Renderer Graph",
                "Regression coverage for NodeCanvas connection-scene rendering extraction.",
                [
                    new GraphNode(
                        SourceNodeId,
                        "Renderer Source",
                        "Tests",
                        "Connection Renderer",
                        "Used as the preview source.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [
                            new GraphPort(
                                SourcePortId,
                                "Result",
                                PortDirection.Output,
                                "float",
                                "#6AD5C4",
                                new PortTypeId("float")),
                        ],
                        "#6AD5C4",
                        SourceDefinitionId),
                    new GraphNode(
                        TargetNodeId,
                        "Renderer Target",
                        "Tests",
                        "Connection Renderer",
                        "Used as the committed connection target.",
                        new GraphPoint(420, 160),
                        new GraphSize(240, 160),
                        [
                            new GraphPort(
                                TargetPortId,
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
                connections),
            catalog,
            new DefaultPortCompatibilityService(),
            styleOptions: GraphEditorStyleOptions.Default);
    }

    private sealed record HostedScene(
        Window Window,
        Grid CoordinateRoot,
        Canvas ConnectionLayer,
        Canvas NodeLayer,
        IReadOnlyDictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals);

    private sealed class StubNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        public GraphNodeVisual Create(GraphNodeVisualContext context)
            => throw new NotSupportedException();

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        {
        }
    }
}
