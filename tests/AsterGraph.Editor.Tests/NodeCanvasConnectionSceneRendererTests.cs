using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Viewport;
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
    public void GetPortAnchor_UsesPreviewSizeWhenRenderedAnchorIsUnavailable()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: false);
        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var sourcePort = sourceNode.Outputs.Single(port => port.Id == SourcePortId);
        var previewSize = new GraphSize(sourceNode.Width + 140d, sourceNode.Height + 80d);
        var context = CreateSceneContext(
            editor,
            connectionLayer: new Canvas(),
            nodeLayer: new Canvas(),
            coordinateRoot: new Grid(),
            nodeVisuals: new Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual>(),
            resolveNodePreviewSize: node => string.Equals(node.Id, sourceNode.Id, StringComparison.Ordinal) ? previewSize : null);

        var anchor = renderer.GetPortAnchor(context, sourceNode, sourcePort);

        var expected = PortAnchorCalculator.GetAnchor(
            new NodeBounds(sourceNode.X, sourceNode.Y, previewSize.Width, previewSize.Height),
            sourcePort.Direction,
            sourcePort.Index,
            sourcePort.Total);
        Assert.Equal(expected, anchor);
    }

    [AvaloniaFact]
    public void GetPortAnchor_PrefersPreviewGeometryWhenVisualBoundsLagBehindResizePreview()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: false);
        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var sourcePort = sourceNode.Outputs.Single(port => port.Id == SourcePortId);
        var previewSize = new GraphSize(sourceNode.Width + 160d, sourceNode.Height + 90d);
        var hostedScene = CreateHostedScene(editor);

        try
        {
            var context = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals,
                resolveNodePreviewSize: node => string.Equals(node.Id, sourceNode.Id, StringComparison.Ordinal) ? previewSize : null);

            var anchor = renderer.GetPortAnchor(context, sourceNode, sourcePort);

            var expected = PortAnchorCalculator.GetAnchor(
                new NodeBounds(sourceNode.X, sourceNode.Y, previewSize.Width, previewSize.Height),
                sourcePort.Direction,
                sourcePort.Index,
                sourcePort.Total);
            Assert.Equal(expected, anchor);
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
            Assert.Equal("FLOAT", label.Text);
            var chipHelp = Assert.IsType<string>(ToolTip.GetTip(chip));
            Assert.Contains("Renderer Source.Result -> Renderer Target.Input", chipHelp, StringComparison.Ordinal);
            Assert.Contains("Float Flow", chipHelp, StringComparison.Ordinal);

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

    [AvaloniaFact]
    public void RenderConnections_LeftPressedPath_SelectsConnectionThroughCanonicalCommandRoute()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: true);
        var hostedScene = CreateHostedScene(editor);
        var pointer = new global::Avalonia.Input.Pointer(0, PointerType.Mouse, true);

        try
        {
            renderer.RenderConnections(CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals));

            var path = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<global::Avalonia.Controls.Shapes.Path>());
            var args = CreatePointerPressedArgs(path, hostedScene.CoordinateRoot, pointer, new Point(360, 220), KeyModifiers.None);

            path.RaiseEvent(args);

            var selection = editor.Session.Queries.GetSelectionSnapshot();
            Assert.True(args.Handled);
            Assert.Empty(selection.SelectedNodeIds);
            Assert.Equal([ConnectionId], selection.SelectedConnectionIds);
            Assert.Equal(ConnectionId, selection.PrimarySelectedConnectionId);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_UsesLiveNodeAnchors_WhenGeometrySnapshotLagsDuringDrag()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: true);
        var hostedScene = CreateHostedScene(editor);

        try
        {
            var context = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals);
            var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var sourcePort = sourceNode.Outputs.Single(port => port.Id == SourcePortId);
            var targetNode = editor.Nodes.Single(node => node.Id == TargetNodeId);
            var staleGeometry = Assert.Single(editor.Session.Queries.GetConnectionGeometrySnapshots());

            editor.ApplyDragOffset(
                new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
                {
                    [sourceNode.Id] = new(sourceNode.X, sourceNode.Y),
                },
                84d,
                36d);

            renderer.RenderConnections(context);

            var path = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<global::Avalonia.Controls.Shapes.Path>());
            var chip = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<Border>());
            var expectedSource = renderer.GetPortAnchor(context, sourceNode, sourcePort);
            var expectedTarget = renderer.GetConnectionTargetAnchor(
                context,
                targetNode,
                new GraphConnectionTargetRef(targetNode.Id, TargetPortId, GraphConnectionTargetKind.Port));
            var expectedMidX = (expectedSource.X + expectedTarget.X) / 2d;
            var expectedMidY = (expectedSource.Y + expectedTarget.Y) / 2d;
            var expectedBounds = CreateRouteGeometry(
                expectedSource,
                staleGeometry.Route,
                staleGeometry.RouteStyle,
                expectedTarget).Bounds;

            Assert.NotEqual(staleGeometry.Source.Position, expectedSource);
            Assert.Equal(expectedBounds.X, path.Data!.Bounds.X, 6);
            Assert.Equal(expectedBounds.Y, path.Data.Bounds.Y, 6);
            Assert.Equal(expectedBounds.Width, path.Data.Bounds.Width, 6);
            Assert.Equal(expectedBounds.Height, path.Data.Bounds.Height, 6);
            Assert.Equal(expectedMidX + GraphEditorStyleOptions.Default.Connection.LabelOffsetX, Canvas.GetLeft(chip), 6);
            Assert.Equal(expectedMidY + GraphEditorStyleOptions.Default.Connection.LabelOffsetY, Canvas.GetTop(chip), 6);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_UsesPersistedRouteVertices_WhenPresentationRouteIsPresent()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(
            includeConnection: true,
            route: new GraphConnectionRoute(
            [
                new GraphPoint(360d, 120d),
                new GraphPoint(420d, 300d),
            ]));
        var hostedScene = CreateHostedScene(editor);

        try
        {
            var context = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals);

            renderer.RenderConnections(context);

            var expectedGeometry = Assert.Single(editor.Session.Queries.GetConnectionGeometrySnapshots());
            var path = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<global::Avalonia.Controls.Shapes.Path>());
            var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var sourcePort = sourceNode.Outputs.Single(port => port.Id == SourcePortId);
            var targetNode = editor.Nodes.Single(node => node.Id == TargetNodeId);
            var expectedSource = renderer.GetPortAnchor(context, sourceNode, sourcePort);
            var expectedTarget = renderer.GetConnectionTargetAnchor(
                context,
                targetNode,
                new GraphConnectionTargetRef(targetNode.Id, TargetPortId, GraphConnectionTargetKind.Port));
            var expectedBounds = CreateRouteGeometry(
                expectedSource,
                expectedGeometry.Route,
                expectedGeometry.RouteStyle,
                expectedTarget).Bounds;

            Assert.Equal(
                new GraphConnectionRoute(
                [
                    new GraphPoint(360d, 120d),
                    new GraphPoint(420d, 300d),
                ]),
                expectedGeometry.Route);
            Assert.Equal(expectedBounds.X, path.Data!.Bounds.X, 6);
            Assert.Equal(expectedBounds.Y, path.Data.Bounds.Y, 6);
            Assert.Equal(expectedBounds.Width, path.Data.Bounds.Width, 6);
            Assert.Equal(expectedBounds.Height, path.Data.Bounds.Height, 6);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_RendersEdgeNoteText_WhenPresentationNoteIsPresent()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: true, noteText: "Preview branch");
        var hostedScene = CreateHostedScene(editor);

        try
        {
            renderer.RenderConnections(CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals));

            var chip = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<Border>());
            var label = Assert.IsType<TextBlock>(chip.Child);
            Assert.Equal("Preview branch", label.Text);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_DoubleTappedChip_CommitsEditedNoteText()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: true);
        var hostedScene = CreateHostedScene(editor);
        var pointer = new global::Avalonia.Input.Pointer(0, PointerType.Mouse, true);

        try
        {
            renderer.RenderConnections(CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals));

            var chip = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<Border>());
            var pointerArgs = CreatePointerPressedArgs(chip, hostedScene.CoordinateRoot, pointer, new Point(760, 300), KeyModifiers.None);

            chip.RaiseEvent(new TappedEventArgs(InputElement.DoubleTappedEvent, pointerArgs)
            {
                Source = chip,
            });

            var editorBox = Assert.IsType<TextBox>(chip.Child);
            editorBox.Text = "Preview branch";
            editorBox.RaiseEvent(new RoutedEventArgs(InputElement.LostFocusEvent)
            {
                Source = editorBox,
            });

            Assert.Equal(
                "Preview branch",
                Assert.Single(editor.CreateDocumentSnapshot().Connections).Presentation?.NoteText);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_RelatedInspectionFocus_UsesEmphasizedConnectionStroke()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(includeConnection: true);
        editor.SelectSingleNode(editor.FindNode(TargetNodeId)!, updateStatus: false);
        var hostedScene = CreateHostedScene(editor);

        try
        {
            renderer.RenderConnections(CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals));

            var connectionPath = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<global::Avalonia.Controls.Shapes.Path>());
            var chip = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<Border>());
            var chipLabel = Assert.IsType<TextBlock>(chip.Child);
            Assert.True(connectionPath.StrokeThickness > GraphEditorStyleOptions.Default.Connection.Thickness);
            Assert.Contains("FLOAT", chipLabel.Text, StringComparison.Ordinal);
            Assert.Contains("Float Flow", chipLabel.Text, StringComparison.Ordinal);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_CustomConnectionStyle_PreservesRouteSelectionAndPendingPreviewSemantics()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateEditor(
            includeConnection: true,
            route: new GraphConnectionRoute(
            [
                new GraphPoint(345d, 132d),
                new GraphPoint(390d, 286d),
            ]));
        editor.StartConnection(SourceNodeId, SourcePortId);
        var hostedScene = CreateHostedScene(editor);
        var pointer = new global::Avalonia.Input.Pointer(0, PointerType.Mouse, true);
        var style = GraphEditorStyleOptions.Default.Connection with
        {
            Thickness = 7.25,
            PreviewThickness = 4.5,
            StrokeOpacity = 0.67,
            PreviewStrokeOpacity = 0.31,
            LabelBackgroundHex = "#102030",
            LabelBackgroundOpacity = 0.73,
            LabelBorderThickness = 3,
            LabelCornerRadius = 5,
            LabelHorizontalPadding = 13,
            LabelVerticalPadding = 6,
            LabelFontSize = 14,
            LabelOffsetX = 18,
            LabelOffsetY = -22,
            LabelBorderOpacity = 0.58,
            LabelForegroundHex = "#F4E8B8",
            LabelForegroundOpacity = 0.91,
        };

        try
        {
            var context = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals,
                pointerScreenPosition: new Point(760, 300),
                resolveConnectionStyle: _ => style);

            renderer.RenderConnections(context);

            var expectedGeometry = Assert.Single(editor.Session.Queries.GetConnectionGeometrySnapshots());
            var paths = hostedScene.ConnectionLayer.Children
                .OfType<global::Avalonia.Controls.Shapes.Path>()
                .ToList();
            Assert.Equal(2, paths.Count);
            var committedPath = paths[0];
            var previewPath = paths[1];
            var chip = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<Border>());
            var chipLabel = Assert.IsType<TextBlock>(chip.Child);
            var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var sourcePort = sourceNode.Outputs.Single(port => port.Id == SourcePortId);
            var targetNode = editor.Nodes.Single(node => node.Id == TargetNodeId);
            var expectedSource = renderer.GetPortAnchor(context, sourceNode, sourcePort);
            var expectedTarget = renderer.GetConnectionTargetAnchor(
                context,
                targetNode,
                new GraphConnectionTargetRef(targetNode.Id, TargetPortId, GraphConnectionTargetKind.Port));
            var expectedBounds = CreateRouteGeometry(
                expectedSource,
                expectedGeometry.Route,
                expectedGeometry.RouteStyle,
                expectedTarget).Bounds;
            var expectedLabelAnchor = ConnectionPathBuilder.ResolveSegmentMidpoint(
                expectedSource,
                expectedGeometry.Route,
                expectedTarget,
                expectedGeometry.Route.Vertices.Count / 2);
            var previewEnd = editor.ScreenToWorld(new GraphPoint(760, 300));
            var expectedPreviewBounds = CreateRouteGeometry(
                expectedSource,
                GraphConnectionRoute.Empty,
                GraphEditorConnectionRouteStyle.Bezier,
                previewEnd).Bounds;

            Assert.Equal(style.Thickness, committedPath.StrokeThickness);
            Assert.Equal(style.PreviewThickness, previewPath.StrokeThickness);
            Assert.Equal(expectedBounds.X, committedPath.Data!.Bounds.X, 6);
            Assert.Equal(expectedBounds.Y, committedPath.Data.Bounds.Y, 6);
            Assert.Equal(expectedBounds.Width, committedPath.Data.Bounds.Width, 6);
            Assert.Equal(expectedBounds.Height, committedPath.Data.Bounds.Height, 6);
            Assert.Equal(expectedPreviewBounds.X, previewPath.Data!.Bounds.X, 6);
            Assert.Equal(expectedPreviewBounds.Y, previewPath.Data.Bounds.Y, 6);
            Assert.Equal(expectedPreviewBounds.Width, previewPath.Data.Bounds.Width, 6);
            Assert.Equal(expectedPreviewBounds.Height, previewPath.Data.Bounds.Height, 6);
            Assert.Equal(expectedLabelAnchor.X + style.LabelOffsetX, Canvas.GetLeft(chip), 6);
            Assert.Equal(expectedLabelAnchor.Y + style.LabelOffsetY, Canvas.GetTop(chip), 6);
            Assert.Equal(new Thickness(style.LabelBorderThickness), chip.BorderThickness);
            Assert.Equal(new CornerRadius(style.LabelCornerRadius), chip.CornerRadius);
            Assert.Equal(new Thickness(style.LabelHorizontalPadding, style.LabelVerticalPadding), chip.Padding);
            Assert.Equal(style.LabelFontSize, chipLabel.FontSize);

            var committedArgs = CreatePointerPressedArgs(
                committedPath,
                hostedScene.CoordinateRoot,
                pointer,
                new Point(360, 220),
                KeyModifiers.None);
            committedPath.RaiseEvent(committedArgs);

            var selected = editor.Session.Queries.GetSelectionSnapshot();
            Assert.True(committedArgs.Handled);
            Assert.Equal([ConnectionId], selected.SelectedConnectionIds);
            Assert.Equal(ConnectionId, selected.PrimarySelectedConnectionId);

            var previewArgs = CreatePointerPressedArgs(
                previewPath,
                hostedScene.CoordinateRoot,
                pointer,
                new Point(620, 260),
                KeyModifiers.None);
            previewPath.RaiseEvent(previewArgs);

            var afterPreviewPress = editor.Session.Queries.GetSelectionSnapshot();
            Assert.False(previewArgs.Handled);
            Assert.Equal(selected.SelectedConnectionIds, afterPreviewPress.SelectedConnectionIds);
            Assert.Equal(selected.PrimarySelectedConnectionId, afterPreviewPress.PrimarySelectedConnectionId);
            Assert.True(editor.HasPendingConnection);
        }
        finally
        {
            hostedScene.Window.Close();
        }
    }

    [AvaloniaFact]
    public void RenderConnections_VisibleSceneBudgetScopesCommittedConnectionsButPreservesPendingPreview()
    {
        var renderer = new NodeCanvasConnectionSceneRenderer();
        var editor = CreateCadenceEditor(hiddenConnectionCount: 12);
        editor.UpdateViewportSize(640d, 420d);
        editor.StartConnection("visible-source", SourcePortId);
        var hostedScene = CreateHostedScene(editor);

        try
        {
            var projection = ViewportVisibleSceneProjector.Project(
                editor.CreateDocumentSnapshot(),
                editor.Session.Queries.GetViewportSnapshot());
            var context = CreateSceneContext(
                editor,
                hostedScene.ConnectionLayer,
                hostedScene.NodeLayer,
                hostedScene.CoordinateRoot,
                hostedScene.NodeVisuals,
                pointerScreenPosition: new Point(520, 220),
                visibleSceneProjection: projection,
                applyVisibleSceneBudget: true);

            renderer.RenderConnections(context);

            var paths = hostedScene.ConnectionLayer.Children
                .OfType<global::Avalonia.Controls.Shapes.Path>()
                .ToList();
            var chip = Assert.Single(hostedScene.ConnectionLayer.Children.OfType<Border>());
            var label = Assert.IsType<TextBlock>(chip.Child);

            Assert.Equal(1, projection.VisibleConnections);
            Assert.Equal(13, projection.TotalConnections);
            Assert.Equal(2, paths.Count);
            Assert.Equal(3, hostedScene.ConnectionLayer.Children.Count);
            Assert.Equal("FLOAT", label.Text);
            Assert.True(editor.HasPendingConnection);
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
        Func<Control, ContextMenuContext, bool>? openContextMenu = null,
        Func<NodeViewModel, GraphSize?>? resolveNodePreviewSize = null,
        Func<ConnectionViewModel, ConnectionStyleOptions>? resolveConnectionStyle = null,
        ViewportVisibleSceneProjection? visibleSceneProjection = null,
        bool applyVisibleSceneBudget = false)
        => new(
            editor,
            connectionLayer,
            nodeLayer,
            coordinateRoot,
            nodeVisuals,
            editor.Session.Queries.GetConnectionGeometrySnapshots()
                .ToDictionary(snapshot => snapshot.ConnectionId, StringComparer.Ordinal),
            editor.Session.Queries.GetHierarchyStateSnapshot(),
            visibleSceneProjection,
            applyVisibleSceneBudget,
            pointerScreenPosition,
            resolveConnectionStyle ?? (_ => GraphEditorStyleOptions.Default.Connection),
            () => new NodeCanvasContextMenuSnapshot(
                editor.SelectedNode?.Id,
                editor.SelectedNodes.Select(node => node.Id).ToList(),
                []),
            (_, _) => new GraphPoint(42, 24),
            openContextMenu ?? ((_, _) => false),
            resolveNodePreviewSize ?? (_ => null));

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

    private static GraphEditorViewModel CreateCadenceEditor(int hiddenConnectionCount)
    {
        var catalog = CreateConnectionCatalog();
        var nodes = new List<GraphNode>
        {
            CreateConnectionNode("visible-source", SourceDefinitionId, new GraphPoint(120d, 160d), hasInput: false, hasOutput: true),
            CreateConnectionNode("visible-target", TargetDefinitionId, new GraphPoint(420d, 160d), hasInput: true, hasOutput: false),
        };
        var connections = new List<GraphConnection>
        {
            new(
                "visible-connection",
                "visible-source",
                SourcePortId,
                "visible-target",
                TargetPortId,
                "Visible Flow",
                "#6AD5C4"),
        };

        for (var index = 0; index < hiddenConnectionCount; index++)
        {
            var sourceId = $"hidden-source-{index:00}";
            var targetId = $"hidden-target-{index:00}";
            var offset = index * 220d;
            nodes.Add(CreateConnectionNode(sourceId, SourceDefinitionId, new GraphPoint(2200d + offset, 1800d), hasInput: false, hasOutput: true));
            nodes.Add(CreateConnectionNode(targetId, TargetDefinitionId, new GraphPoint(2320d + offset, 1800d), hasInput: true, hasOutput: false));
            connections.Add(new GraphConnection(
                $"hidden-connection-{index:00}",
                sourceId,
                SourcePortId,
                targetId,
                TargetPortId,
                $"Hidden Flow {index:00}",
                "#6AD5C4"));
        }

        return new GraphEditorViewModel(
            new GraphDocument(
                "Connection Cadence Graph",
                "Regression coverage for visible-scene connection rendering cadence.",
                nodes,
                connections),
            catalog,
            new DefaultPortCompatibilityService(),
            styleOptions: GraphEditorStyleOptions.Default);
    }

    private static GraphEditorViewModel CreateEditor(bool includeConnection, string? noteText = null, GraphConnectionRoute? route = null)
    {
        var catalog = CreateConnectionCatalog();

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
                    "#6AD5C4",
                    Presentation: string.IsNullOrWhiteSpace(noteText) && route is null
                        ? null
                        : new GraphEdgePresentation(noteText, route)),
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

    private static NodeCatalog CreateConnectionCatalog()
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
        return catalog;
    }

    private static GraphNode CreateConnectionNode(
        string id,
        NodeDefinitionId definitionId,
        GraphPoint position,
        bool hasInput,
        bool hasOutput)
        => new(
            id,
            id,
            "Tests",
            "Connection Cadence",
            "Connection cadence proof node.",
            position,
            new GraphSize(240d, 160d),
            hasInput
                ?
                [
                    new GraphPort(
                        TargetPortId,
                        "Input",
                        PortDirection.Input,
                        "float",
                        "#F3B36B",
                        new PortTypeId("float")),
                ]
                : [],
            hasOutput
                ?
                [
                    new GraphPort(
                        SourcePortId,
                        "Result",
                        PortDirection.Output,
                        "float",
                        "#6AD5C4",
                        new PortTypeId("float")),
                ]
                : [],
            "#6AD5C4",
            definitionId);

    private sealed record HostedScene(
        Window Window,
        Grid CoordinateRoot,
        Canvas ConnectionLayer,
        Canvas NodeLayer,
        IReadOnlyDictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals);

    private static global::Avalonia.Media.Geometry CreateBezierGeometry(BezierConnection curve)
        => global::Avalonia.Media.Geometry.Parse(
            $"M {curve.Start.X:0.##},{curve.Start.Y:0.##} " +
            $"C {curve.Control1.X:0.##},{curve.Control1.Y:0.##} " +
            $"{curve.Control2.X:0.##},{curve.Control2.Y:0.##} " +
            $"{curve.End.X:0.##},{curve.End.Y:0.##}");

    private static global::Avalonia.Media.Geometry CreateRouteGeometry(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphEditorConnectionRouteStyle routeStyle,
        GraphPoint end)
    {
        var segments = ConnectionPathBuilder.BuildRoute(start, route, end, routeStyle);
        var commands = string.Join(
            " ",
            segments.Select(segment =>
                $"C {segment.Control1.X:0.##},{segment.Control1.Y:0.##} " +
                $"{segment.Control2.X:0.##},{segment.Control2.Y:0.##} " +
                $"{segment.End.X:0.##},{segment.End.Y:0.##}"));
        return global::Avalonia.Media.Geometry.Parse($"M {start.X:0.##},{start.Y:0.##} {commands}");
    }

    private sealed class StubNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        public GraphNodeVisual Create(GraphNodeVisualContext context)
            => throw new NotSupportedException();

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        {
        }
    }

    private static PointerPressedEventArgs CreatePointerPressedArgs(
        InputElement source,
        InputElement root,
        global::Avalonia.Input.Pointer pointer,
        Point position,
        KeyModifiers modifiers)
        => new(
            source,
            pointer,
            root,
            position,
            0UL,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            modifiers,
            1);
}
