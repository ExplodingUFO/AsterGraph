using Avalonia;
using Avalonia.Input;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
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

public sealed class NodeCanvasPointerInteractionCoordinatorTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.pointer.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.pointer.target");
    private const string SourceNodeId = "tests.pointer.source-001";
    private const string SourcePortId = "out";

    [Fact]
    public void HandlePressed_WithLeftPressAndPendingConnection_CancelsPreviewAndBeginsSelection()
    {
        var editor = CreateEditor();
        editor.StartConnection(SourceNodeId, SourcePortId);
        var host = new TestPointerInteractionHost(editor);
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var result = coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(32, 48),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);

        Assert.True(result.Handled);
        Assert.True(result.CapturePointer);
        Assert.False(editor.HasPendingConnection);
        Assert.Equal(1, host.RenderConnectionsCalls);
        Assert.Equal(new Point(32, 48), host.InteractionSession.SelectionStartScreenPosition);
        Assert.Equal(1, host.HideSelectionAdornerCalls);
        Assert.Equal(1, host.HideGuideAdornerCalls);
    }

    [Fact]
    public void HandlePressed_WithAltLeftDragPanning_StartsPanningWithoutCanvasSelection()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor);
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var result = coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(40, 64),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.Alt);

        Assert.True(result.Handled);
        Assert.True(result.CapturePointer);
        Assert.True(host.InteractionSession.IsPanning);
        Assert.Null(host.InteractionSession.SelectionStartScreenPosition);
        Assert.Equal(1, host.HideSelectionAdornerCalls);
        Assert.Equal(1, host.HideGuideAdornerCalls);
    }

    [Fact]
    public void HandlePressed_WithRectangleWhiteboardMode_BeginsPrimitiveDrawingWithoutCanvasSelection()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor)
        {
            WhiteboardDrawingMode = NodeCanvasWhiteboardDrawingMode.Rectangle,
        };
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var result = coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(32, 48),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);

        Assert.True(result.Handled);
        Assert.True(result.CapturePointer);
        Assert.Null(host.InteractionSession.SelectionStartScreenPosition);
        Assert.False(host.InteractionSession.IsMarqueeSelecting);
        Assert.NotNull(host.InteractionSession.ActiveWhiteboardPrimitive);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Rectangle, host.InteractionSession.ActiveWhiteboardPrimitive.Kind);
        Assert.Equal(editor.ScreenToWorld(new GraphPoint(32, 48)), host.InteractionSession.ActiveWhiteboardPrimitive.Geometry.Origin);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Creating, host.InteractionSession.ActiveWhiteboardPrimitive.EditLifecycle.State);
        Assert.Equal(1, host.HideSelectionAdornerCalls);
        Assert.Equal(1, host.HideGuideAdornerCalls);
    }

    [Fact]
    public void HandleMoved_AndReleased_WithRectangleWhiteboardMode_CommitsWorldSpacePrimitive()
    {
        var editor = CreateEditor();
        editor.PanBy(20, 30);
        editor.ZoomAt(2, new GraphPoint(20, 30));
        var host = new TestPointerInteractionHost(editor)
        {
            WhiteboardDrawingMode = NodeCanvasWhiteboardDrawingMode.Rectangle,
        };
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(60, 80),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);

        var handled = coordinator.HandleMoved(new Point(100, 140), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.NotNull(host.InteractionSession.ActiveWhiteboardPrimitive);
        var expectedStartWorld = editor.ScreenToWorld(new GraphPoint(60, 80));
        var expectedEndWorld = editor.ScreenToWorld(new GraphPoint(100, 140));
        var expectedOrigin = new GraphPoint(
            Math.Min(expectedStartWorld.X, expectedEndWorld.X),
            Math.Min(expectedStartWorld.Y, expectedEndWorld.Y));
        var expectedSize = new GraphSize(
            Math.Abs(expectedEndWorld.X - expectedStartWorld.X),
            Math.Abs(expectedEndWorld.Y - expectedStartWorld.Y));
        Assert.Equal(
            expectedOrigin,
            host.InteractionSession.ActiveWhiteboardPrimitive.Geometry.Origin);
        Assert.Equal(expectedSize, host.InteractionSession.ActiveWhiteboardPrimitive.Geometry.Size);
        Assert.Equal(0, host.UpdateMarqueeSelectionCalls);
        Assert.Equal(0, host.UpdateLassoSelectionCalls);

        coordinator.HandleReleased(new Point(100, 140));

        var primitive = Assert.Single(host.InteractionSession.WhiteboardPrimitives);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Rectangle, primitive.Kind);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Committed, primitive.EditLifecycle.State);
        Assert.Equal(expectedSize, primitive.Geometry.Size);
        Assert.Null(host.InteractionSession.ActiveWhiteboardPrimitive);
        Assert.Equal(1, host.ClearLassoFeedbackCalls);
        Assert.Equal(3, host.ClearResizeFeedbackCalls);
    }

    [Fact]
    public void HandleReleased_WithRectangleWhiteboardModeWithoutDrag_CancelsPreviewWithoutCommit()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor)
        {
            WhiteboardDrawingMode = NodeCanvasWhiteboardDrawingMode.Rectangle,
        };
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(40, 64),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);

        coordinator.HandleReleased(new Point(40, 64));

        Assert.Empty(host.InteractionSession.WhiteboardPrimitives);
        Assert.Null(host.InteractionSession.ActiveWhiteboardPrimitive);
        Assert.Empty(host.InteractionSession.WhiteboardGestureScreenPoints);
    }

    [Fact]
    public void HandleMoved_AndReleased_WithFreehandWhiteboardMode_CommitsCollectedWorldPoints()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor)
        {
            WhiteboardDrawingMode = NodeCanvasWhiteboardDrawingMode.Freehand,
        };
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(10, 20),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);
        Assert.True(coordinator.HandleMoved(new Point(16, 28), selectionDragThreshold: 6));
        Assert.True(coordinator.HandleMoved(new Point(22, 24), selectionDragThreshold: 6));

        coordinator.HandleReleased(new Point(30, 34));

        var expectedPoints = new[]
        {
            editor.ScreenToWorld(new GraphPoint(10, 20)),
            editor.ScreenToWorld(new GraphPoint(16, 28)),
            editor.ScreenToWorld(new GraphPoint(22, 24)),
            editor.ScreenToWorld(new GraphPoint(30, 34)),
        };
        var expectedOrigin = new GraphPoint(
            expectedPoints.Min(point => point.X),
            expectedPoints.Min(point => point.Y));
        var expectedSize = new GraphSize(
            expectedPoints.Max(point => point.X) - expectedOrigin.X,
            expectedPoints.Max(point => point.Y) - expectedOrigin.Y);
        var primitive = Assert.Single(host.InteractionSession.WhiteboardPrimitives);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Freehand, primitive.Kind);
        Assert.Equal(expectedPoints, primitive.Geometry.Points);
        Assert.Equal(expectedOrigin, primitive.Geometry.Origin);
        Assert.Equal(expectedSize, primitive.Geometry.Size);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Committed, primitive.EditLifecycle.State);
    }

    [Fact]
    public void HandleMoved_AndReleased_WithFreehandWhiteboardModeBelowDragThreshold_CancelsPreviewWithoutCommit()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor)
        {
            WhiteboardDrawingMode = NodeCanvasWhiteboardDrawingMode.Freehand,
        };
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(10, 20),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);

        Assert.True(coordinator.HandleMoved(new Point(14, 23), selectionDragThreshold: 6));
        coordinator.HandleReleased(new Point(14, 23));

        Assert.Empty(host.InteractionSession.WhiteboardPrimitives);
        Assert.Null(host.InteractionSession.ActiveWhiteboardPrimitive);
        Assert.Empty(host.InteractionSession.WhiteboardGestureScreenPoints);
    }

    [Fact]
    public void HandlePointerCaptureLost_WithActiveWhiteboardPrimitive_CancelsPreviewWithoutCommit()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor)
        {
            WhiteboardDrawingMode = NodeCanvasWhiteboardDrawingMode.Freehand,
        };
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(10, 20),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);
        coordinator.HandleMoved(new Point(18, 28), selectionDragThreshold: 6);

        var reset = coordinator.HandlePointerCaptureLost();

        Assert.True(reset);
        Assert.Null(host.InteractionSession.ActiveWhiteboardPrimitive);
        Assert.Empty(host.InteractionSession.WhiteboardGestureScreenPoints);
        Assert.Empty(host.InteractionSession.WhiteboardPrimitives);
        Assert.Equal(2, host.HideGuideAdornerCalls);
        Assert.Equal(1, host.ClearLassoFeedbackCalls);
        Assert.Equal(3, host.ClearResizeFeedbackCalls);
    }

    [Fact]
    public void HandlePressed_WithWhiteboardModeAndAltLeftDragPanning_StartsPanningInsteadOfDrawing()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor)
        {
            WhiteboardDrawingMode = NodeCanvasWhiteboardDrawingMode.Rectangle,
        };
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var result = coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(40, 64),
            isLeftButtonPressed: true,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.Alt);

        Assert.True(result.Handled);
        Assert.True(result.CapturePointer);
        Assert.True(host.InteractionSession.IsPanning);
        Assert.Null(host.InteractionSession.ActiveWhiteboardPrimitive);
        Assert.Empty(host.InteractionSession.WhiteboardPrimitives);
    }

    [Fact]
    public void HandlePressed_WithSecondaryPress_DoesNotClearResizeFeedback()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor);
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var result = coordinator.HandlePressed(
            isAlreadyHandled: false,
            currentScreenPosition: new Point(40, 64),
            isLeftButtonPressed: false,
            isMiddleButtonPressed: false,
            modifiers: KeyModifiers.None);

        Assert.False(result.Handled);
        Assert.False(result.CapturePointer);
        Assert.Equal(0, host.ClearResizeFeedbackCalls);
    }

    [Fact]
    public void HandleMoved_WhenCanvasSelectionCrossesThreshold_TriggersMarqueeUpdate()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginCanvasSelection(new Point(10, 10), KeyModifiers.None, []);
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(24, 26), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(1, host.UpdateMarqueeSelectionCalls);
        Assert.Equal(new Point(24, 26), host.LastMarqueePoint);
        Assert.False(host.LastMarqueeFinalize);
        Assert.True(host.InteractionSession.IsMarqueeSelecting);
    }

    [Fact]
    public void HandleMoved_WhenCanvasSelectionUsesLassoGesture_RecordsLassoPointAndSkipsMarqueeUpdate()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginCanvasSelection(
            new Point(10, 10),
            KeyModifiers.None,
            [],
            NodeCanvasSelectionGestureKind.Lasso);
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(24, 26), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(0, host.UpdateMarqueeSelectionCalls);
        Assert.Equal(0, host.UpdateLassoSelectionCalls);
        Assert.Equal(1, host.UpdateLassoFeedbackCalls);
        Assert.Equal(
            [new Point(10, 10), new Point(24, 26)],
            host.LastLassoFeedbackPoints);
        Assert.False(host.LastLassoFeedbackFinalize);
        Assert.Equal(
            [new Point(10, 10), new Point(24, 26)],
            host.InteractionSession.LassoScreenPoints);
        Assert.True(host.InteractionSession.IsLassoSelecting);
        Assert.False(host.InteractionSession.IsMarqueeSelecting);
    }

    [Fact]
    public void HandleMoved_WhenPanning_PansViewportAndUpdatesLastPointer()
    {
        var editor = CreateEditor();
        var initialPanX = editor.PanX;
        var initialPanY = editor.PanY;
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginPanning(new Point(12, 18));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(40, 54), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(initialPanX + 28, editor.PanX);
        Assert.Equal(initialPanY + 36, editor.PanY);
        Assert.Equal(new Point(40, 54), host.InteractionSession.LastPointerPosition);
        Assert.Equal(0, host.UpdateResizeFeedbackCalls);
        Assert.Equal(1, host.ClearResizeFeedbackCalls);
    }

    [Fact]
    public void HandleMoved_WhenDraggingNode_AppliesDragAssistAndOffsetsNode()
    {
        var editor = CreateEditor();
        var node = editor.Nodes[0];
        var host = new TestPointerInteractionHost(editor)
        {
            DragAssistResult = new GraphPoint(18, -6),
        };
        host.InteractionSession.BeginNodeDrag(
            node,
            new Point(30, 40),
            new NodeCanvasDragSession(
                [node],
                new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
                {
                    [node.Id] = new GraphPoint(node.X, node.Y),
                },
                new NodeBounds(node.X, node.Y, node.Width, node.Height)));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(72, 90), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(1, host.ApplyDragAssistCalls);
        Assert.Equal(138, node.X);
        Assert.Equal(154, node.Y);
        Assert.Equal(0, host.UpdateResizeFeedbackCalls);
        Assert.Equal(1, host.ClearResizeFeedbackCalls);
    }

    [Fact]
    public void HandleMoved_WhenDraggingGroup_AppliesDragAssistAndOffsetsPreview()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Pointer Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var groupSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        var node = editor.Nodes[0];
        var origin = new GraphPoint(node.X, node.Y);
        var host = new TestPointerInteractionHost(editor)
        {
            DragAssistResult = new GraphPoint(24, -12),
        };
        host.InteractionSession.BeginGroupDrag(
            groupSnapshot.Id,
            groupSnapshot.Title,
            groupSnapshot.Position,
            new Point(30, 40),
            new NodeCanvasDragSession(
                [node],
                new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
                {
                    [node.Id] = origin,
                },
                new NodeBounds(groupSnapshot.Position.X, groupSnapshot.Position.Y, groupSnapshot.Size.Width, groupSnapshot.Size.Height)));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(72, 90), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(1, host.ApplyDragAssistCalls);
        Assert.Equal(groupSnapshot.Position.X + 24d, host.InteractionSession.DragGroupPreviewPosition?.X);
        Assert.Equal(groupSnapshot.Position.Y - 12d, host.InteractionSession.DragGroupPreviewPosition?.Y);
        Assert.Equal(origin.X + 24d, node.X);
        Assert.Equal(origin.Y - 12d, node.Y);
    }

    [Fact]
    public void HandleMoved_WhenNodeResizeSession_StoresPreviewWithoutPersistingNodeMutation()
    {
        var editor = CreateEditor();
        var node = editor.Nodes[1];
        var originalSize = new GraphSize(node.Width, node.Height);
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginNodeResize(node, GraphNodeResizeHandleKind.Right, new Point(30, 40));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(72, 40), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(originalSize, new GraphSize(node.Width, node.Height));
        Assert.NotNull(host.InteractionSession.NodeResizePreview);
        Assert.True(host.InteractionSession.NodeResizePreview.Value.Size.Width > originalSize.Width + 30d);
        Assert.Equal(originalSize.Height, host.InteractionSession.NodeResizePreview.Value.Size.Height);
        Assert.Equal(1, host.UpdateNodeVisualCalls);
        Assert.Equal(1, host.RenderConnectionsCalls);
    }

    [Fact]
    public void HandleMoved_WhenGroupResizeSession_StoresPreviewWithoutPersistingGroupMutation()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Pointer Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var originalSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginGroupResize(
            originalSnapshot.Id,
            originalSnapshot.Title,
            NodeCanvasGroupResizeEdge.Right,
            originalSnapshot.Position,
            originalSnapshot.Size,
            new Point(30, 40));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(78, 40), selectionDragThreshold: 6);

        Assert.True(handled);
        var currentSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.Equal(originalSnapshot.Size, currentSnapshot.Size);
        Assert.NotNull(host.InteractionSession.GroupResizePreview);
        Assert.True(host.InteractionSession.GroupResizePreview.Value.Size.Width > originalSnapshot.Size.Width + 40d);
        Assert.Equal(originalSnapshot.Position, host.InteractionSession.GroupResizePreview.Value.Position);
        Assert.Equal(1, host.UpdateGroupVisualsCalls);
        Assert.Equal(1, host.RenderConnectionsCalls);
    }

    [Fact]
    public void HandleMoved_WhenGroupTopResizeSession_StoresPreviewPositionAndHeightWithoutPersistingMutation()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Pointer Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var originalSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginGroupResize(
            originalSnapshot.Id,
            originalSnapshot.Title,
            NodeCanvasGroupResizeEdge.Top,
            originalSnapshot.Position,
            originalSnapshot.Size,
            new Point(30, 40));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(30, 6), selectionDragThreshold: 6);

        Assert.True(handled);
        var currentSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.Equal(originalSnapshot.Size, currentSnapshot.Size);
        Assert.NotNull(host.InteractionSession.GroupResizePreview);
        Assert.True(host.InteractionSession.GroupResizePreview.Value.Position.Y < originalSnapshot.Position.Y);
        Assert.True(host.InteractionSession.GroupResizePreview.Value.Size.Height > originalSnapshot.Size.Height + 30d);
        Assert.Equal(originalSnapshot.Size.Width, host.InteractionSession.GroupResizePreview.Value.Size.Width);
        Assert.Equal(1, host.UpdateGroupVisualsCalls);
        Assert.Equal(1, host.RenderConnectionsCalls);
    }

    [Fact]
    public void HandleMoved_AndReleased_WhenDraggingNode_UseGroupContentAreaForAttachment()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Pointer Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var targetNode = editor.Nodes[1];
        var groupSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        var origin = new GraphPoint(targetNode.X, targetNode.Y);
        var host = new TestPointerInteractionHost(editor)
        {
            DragAssistResult = new GraphPoint(0, 0),
        };
        host.InteractionSession.BeginNodeDrag(
            targetNode,
            new Point(30, 40),
            new NodeCanvasDragSession(
                [targetNode],
                new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
                {
                    [targetNode.Id] = origin,
                },
                new NodeBounds(targetNode.X, targetNode.Y, targetNode.Width, targetNode.Height)),
            [groupSnapshot]);
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var headerOffset = new GraphPoint(
            (groupSnapshot.ContentPosition.X - 12d) - origin.X,
            (groupSnapshot.Position.Y + 8d) - origin.Y);
        host.DragAssistResult = headerOffset;

        var handled = coordinator.HandleMoved(new Point(48, 60), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Null(host.InteractionSession.HoveredDropGroupId);
        Assert.Equal(0, host.UpdateGroupVisualsCalls);

        var contentOffset = new GraphPoint(
            groupSnapshot.ContentPosition.X - origin.X,
            groupSnapshot.ContentPosition.Y - origin.Y);
        host.DragAssistResult = contentOffset;

        handled = coordinator.HandleMoved(new Point(72, 84), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(groupId, host.InteractionSession.HoveredDropGroupId);
        Assert.Equal(1, host.UpdateGroupVisualsCalls);

        coordinator.HandleReleased(new Point(72, 84));

        var attachedNode = Assert.Single(editor.Nodes, candidate => candidate.Id == targetNode.Id);
        Assert.Equal(groupId, attachedNode.GroupId);
        Assert.Equal(2, host.UpdateGroupVisualsCalls);
        Assert.Null(host.InteractionSession.HoveredDropGroupId);
    }

    [Fact]
    public void HandleMoved_WhenDraggingGroup_UsesTransientPreviewUntilRelease()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Pointer Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var groupSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var initialGroupPosition = groupSnapshot.Position;
        var initialSourcePosition = new GraphPoint(sourceNode.X, sourceNode.Y);
        var documentChangedCount = 0;
        editor.Session.Events.DocumentChanged += (_, _) => documentChangedCount++;

        var expectedDelta = new GraphPoint(44d / editor.Zoom, 22d / editor.Zoom);
        var host = new TestPointerInteractionHost(editor)
        {
            DragAssistResult = expectedDelta,
        };
        host.InteractionSession.BeginGroupDrag(
            groupId!,
            groupSnapshot.Title,
            groupSnapshot.Position,
            new Point(30, 40),
            new NodeCanvasDragSession(
                [sourceNode],
                new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
                {
                    [sourceNode.Id] = initialSourcePosition,
                },
                new NodeBounds(groupSnapshot.Position.X, groupSnapshot.Position.Y, groupSnapshot.Size.Width, groupSnapshot.Size.Height)));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(74, 62), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(initialSourcePosition.X + expectedDelta.X, sourceNode.X);
        Assert.Equal(initialSourcePosition.Y + expectedDelta.Y, sourceNode.Y);
        Assert.Equal(0, documentChangedCount);

        var previewSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.Equal(initialGroupPosition, previewSnapshot.Position);

        coordinator.HandleReleased(new Point(74, 62));

        var movedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.Equal(initialGroupPosition.X + expectedDelta.X, movedSnapshot.Position.X);
        Assert.Equal(initialGroupPosition.Y + expectedDelta.Y, movedSnapshot.Position.Y);
        Assert.True(documentChangedCount > 0);
    }

    [Fact]
    public void HandleReleased_AfterMarqueeSelection_FinalizesSelectionAndResetsSession()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginCanvasSelection(new Point(8, 12), KeyModifiers.None, []);
        host.InteractionSession.TryBeginMarqueeSelection(new Point(20, 24), threshold: 6);
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandleReleased(new Point(48, 60));

        Assert.Equal(1, host.UpdateMarqueeSelectionCalls);
        Assert.Equal(new Point(48, 60), host.LastMarqueePoint);
        Assert.True(host.LastMarqueeFinalize);
        Assert.Null(host.InteractionSession.SelectionStartScreenPosition);
        Assert.False(host.InteractionSession.IsMarqueeSelecting);
        Assert.Equal(1, host.HideSelectionAdornerCalls);
        Assert.Equal(1, host.HideGuideAdornerCalls);
    }

    [Fact]
    public void HandleReleased_AfterLassoSelection_FinalizesLassoSelectionAndResetsSession()
    {
        var editor = CreateEditor();
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginCanvasSelection(
            new Point(8, 12),
            KeyModifiers.None,
            [],
            NodeCanvasSelectionGestureKind.Lasso);
        host.InteractionSession.TryBeginLassoSelection(new Point(20, 24), threshold: 6);
        host.InteractionSession.RecordLassoSelectionPoint(new Point(32, 28));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandleReleased(new Point(48, 60));

        Assert.Equal(0, host.UpdateMarqueeSelectionCalls);
        Assert.Equal(1, host.UpdateLassoSelectionCalls);
        Assert.True(host.LastLassoFinalize);
        Assert.Equal(1, host.UpdateLassoFeedbackCalls);
        Assert.True(host.LastLassoFeedbackFinalize);
        Assert.Equal(1, host.ClearLassoFeedbackCalls);
        Assert.Equal(
            [new Point(8, 12), new Point(20, 24), new Point(32, 28), new Point(48, 60)],
            host.LastLassoPoints);
        Assert.Null(host.InteractionSession.SelectionStartScreenPosition);
        Assert.False(host.InteractionSession.IsLassoSelecting);
        Assert.Equal(1, host.HideSelectionAdornerCalls);
        Assert.Equal(1, host.HideGuideAdornerCalls);
    }

    [Fact]
    public void HandleReleased_AfterNodeDrag_CommitsHistoryInteractionBoundary()
    {
        var editor = CreateEditor();
        var node = editor.Nodes[0];
        var origin = new GraphPoint(node.X, node.Y);
        var host = new TestPointerInteractionHost(editor)
        {
            DragAssistResult = new GraphPoint(24, 12),
        };
        editor.BeginHistoryInteraction();
        host.InteractionSession.BeginNodeDrag(
            node,
            new Point(20, 30),
            new NodeCanvasDragSession(
                [node],
                new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
                {
                    [node.Id] = origin,
                },
                new NodeBounds(node.X, node.Y, node.Width, node.Height)));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        coordinator.HandleMoved(new Point(56, 72), selectionDragThreshold: 6);
        Assert.False(editor.CanUndo);

        coordinator.HandleReleased(new Point(56, 72));

        Assert.True(editor.CanUndo);
        var movedNode = Assert.Single(editor.Nodes, candidate => candidate.Id == node.Id);
        Assert.Equal(origin.X + 24, movedNode.X);
        Assert.Equal(origin.Y + 12, movedNode.Y);

        editor.Undo();
        var restoredNode = Assert.Single(editor.Nodes, candidate => candidate.Id == node.Id);
        Assert.Equal(origin.X, restoredNode.X);
        Assert.Equal(origin.Y, restoredNode.Y);
    }

    [Fact]
    public void HandleReleased_AfterLeftEdgeGroupResize_RestoresOriginalFrameWithSingleUndo()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Pointer Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var originalSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        var host = new TestPointerInteractionHost(editor);
        editor.BeginHistoryInteraction();
        host.InteractionSession.BeginGroupResize(
            originalSnapshot.Id,
            originalSnapshot.Title,
            NodeCanvasGroupResizeEdge.Left,
            originalSnapshot.Position,
            originalSnapshot.Size,
            new Point(60, 40));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);
        var movedPoint = new Point(24, 40);

        coordinator.HandleMoved(movedPoint, selectionDragThreshold: 6);
        coordinator.HandleReleased(movedPoint);

        var resizedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.True(resizedSnapshot.Position.X < originalSnapshot.Position.X);
        Assert.True(resizedSnapshot.Size.Width > originalSnapshot.Size.Width);

        editor.Undo();

        var restoredSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.Equal(originalSnapshot.Position, restoredSnapshot.Position);
        Assert.Equal(originalSnapshot.Size, restoredSnapshot.Size);
    }

    [Fact]
    public void HandlePointerCaptureLost_WhenActiveInteractionExists_ResetsSession()
    {
        var editor = CreateEditor();
        var node = editor.Nodes[0];
        var origin = new GraphPoint(node.X, node.Y);
        var host = new TestPointerInteractionHost(editor);
        host.InteractionSession.BeginNodeDrag(
            node,
            new Point(30, 40),
            new NodeCanvasDragSession(
                [node],
                new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
                {
                    [node.Id] = origin,
                },
                new NodeBounds(node.X, node.Y, node.Width, node.Height)));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var reset = coordinator.HandlePointerCaptureLost();

        Assert.True(reset);
        Assert.Null(host.InteractionSession.DragNode);
        Assert.False(host.InteractionSession.IsPanning);
        Assert.Null(host.InteractionSession.SelectionStartScreenPosition);
        Assert.Null(host.InteractionSession.DragStartScreenPosition);
        Assert.True(host.InteractionSession.DragSession is null);
        Assert.Equal(1, host.HideGuideAdornerCalls);
        Assert.Equal(1, host.ClearLassoFeedbackCalls);
        Assert.Equal(1, host.ClearResizeFeedbackCalls);

        Assert.False(coordinator.HandlePointerCaptureLost());
    }

    private static GraphEditorViewModel CreateEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                SourceDefinitionId,
                "Pointer Source",
                "Tests",
                "Pointer interaction source node.",
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
                "Pointer Target",
                "Tests",
                "Pointer interaction target node.",
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
                "Pointer Interaction Graph",
                "Regression coverage for node canvas pointer interaction extraction.",
                [
                    new GraphNode(
                        SourceNodeId,
                        "Pointer Source",
                        "Tests",
                        "Pointer",
                        "Source node for pointer coordinator tests.",
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
                        "tests.pointer.target-001",
                        "Pointer Target",
                        "Tests",
                        "Pointer",
                        "Target node for pointer coordinator tests.",
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

    private sealed class TestPointerInteractionHost : INodeCanvasPointerInteractionHost
    {
        public TestPointerInteractionHost(GraphEditorViewModel editor)
        {
            ViewModel = editor;
        }

        public GraphEditorViewModel? ViewModel { get; }

        public bool EnableAltLeftDragPanning { get; init; } = true;

        public NodeCanvasSelectionGestureKind SelectionGestureKind { get; init; } = NodeCanvasSelectionGestureKind.Marquee;

        public NodeCanvasWhiteboardDrawingMode WhiteboardDrawingMode { get; init; } = NodeCanvasWhiteboardDrawingMode.None;

        public NodeCanvasInteractionSession InteractionSession { get; } = new();

        public int HideSelectionAdornerCalls { get; private set; }

        public int HideGuideAdornerCalls { get; private set; }

        public int RenderConnectionsCalls { get; private set; }

        public int UpdateGroupVisualsCalls { get; private set; }

        public int UpdateNodeVisualCalls { get; private set; }

        public int UpdateMarqueeSelectionCalls { get; private set; }

        public Point? LastMarqueePoint { get; private set; }

        public bool LastMarqueeFinalize { get; private set; }

        public int UpdateLassoSelectionCalls { get; private set; }

        public IReadOnlyList<Point> LastLassoPoints { get; private set; } = [];

        public bool LastLassoFinalize { get; private set; }

        public int UpdateLassoFeedbackCalls { get; private set; }

        public IReadOnlyList<Point> LastLassoFeedbackPoints { get; private set; } = [];

        public bool LastLassoFeedbackFinalize { get; private set; }

        public int ClearLassoFeedbackCalls { get; private set; }

        public int ApplyDragAssistCalls { get; private set; }

        public GraphPoint DragAssistResult { get; set; } = new(0, 0);

        public int ApplyGroupResizeAssistCalls { get; private set; }

        public int UpdateResizeFeedbackCalls { get; private set; }

        public int ClearResizeFeedbackCalls { get; private set; }

        public void FocusCanvas()
        {
        }

        public void HideSelectionAdorner()
            => HideSelectionAdornerCalls++;

        public void HideGuideAdorners()
            => HideGuideAdornerCalls++;

        public void RenderConnections()
            => RenderConnectionsCalls++;

        public void UpdateGroupVisuals()
            => UpdateGroupVisualsCalls++;

        public void UpdateNodeVisual(NodeViewModel node)
            => UpdateNodeVisualCalls++;

        public void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize)
        {
            UpdateMarqueeSelectionCalls++;
            LastMarqueePoint = currentScreenPosition;
            LastMarqueeFinalize = finalize;
        }

        public void UpdateLassoSelection(IReadOnlyList<Point> screenPoints, bool finalize)
        {
            UpdateLassoSelectionCalls++;
            LastLassoPoints = screenPoints.ToList();
            LastLassoFinalize = finalize;
        }

        public void UpdateLassoFeedback(IReadOnlyList<Point> screenPoints, bool finalize)
        {
            UpdateLassoFeedbackCalls++;
            LastLassoFeedbackPoints = screenPoints.ToList();
            LastLassoFeedbackFinalize = finalize;
        }

        public void ClearLassoFeedback()
        {
            ClearLassoFeedbackCalls++;
        }

        public GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY)
        {
            ApplyDragAssistCalls++;
            return DragAssistResult;
        }

        public NodeCanvasGroupResizePreview ApplyGroupResizeAssist(
            GraphEditorNodeGroupSnapshot group,
            NodeCanvasGroupResizeEdge edge,
            GraphPoint proposedPosition,
            GraphSize proposedSize,
            GraphSize minimumSize)
        {
            ApplyGroupResizeAssistCalls++;
            return new NodeCanvasGroupResizePreview(group.Id, proposedPosition, proposedSize);
        }

        public void UpdateResizeFeedback(Point currentScreenPosition)
        {
            UpdateResizeFeedbackCalls++;
        }

        public void ClearResizeFeedback()
        {
            ClearResizeFeedbackCalls++;
        }
    }
}
