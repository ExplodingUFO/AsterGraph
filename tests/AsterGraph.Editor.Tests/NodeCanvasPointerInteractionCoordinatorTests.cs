using Avalonia;
using Avalonia.Input;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Geometry;
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

        var host = new TestPointerInteractionHost(editor);
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
                new NodeBounds(sourceNode.X, sourceNode.Y, sourceNode.Width, sourceNode.Height)));
        var coordinator = new NodeCanvasPointerInteractionCoordinator(host);

        var handled = coordinator.HandleMoved(new Point(74, 62), selectionDragThreshold: 6);

        Assert.True(handled);
        Assert.Equal(initialSourcePosition.X + 50d, sourceNode.X);
        Assert.Equal(initialSourcePosition.Y + 25d, sourceNode.Y);
        Assert.Equal(0, documentChangedCount);

        var previewSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.Equal(initialGroupPosition, previewSnapshot.Position);

        coordinator.HandleReleased(new Point(74, 62));

        var movedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
        Assert.Equal(initialGroupPosition.X + 50d, movedSnapshot.Position.X);
        Assert.Equal(initialGroupPosition.Y + 25d, movedSnapshot.Position.Y);
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

        public NodeCanvasInteractionSession InteractionSession { get; } = new();

        public int HideSelectionAdornerCalls { get; private set; }

        public int HideGuideAdornerCalls { get; private set; }

        public int RenderConnectionsCalls { get; private set; }

        public int UpdateGroupVisualsCalls { get; private set; }

        public int UpdateMarqueeSelectionCalls { get; private set; }

        public Point? LastMarqueePoint { get; private set; }

        public bool LastMarqueeFinalize { get; private set; }

        public int ApplyDragAssistCalls { get; private set; }

        public GraphPoint DragAssistResult { get; set; } = new(0, 0);

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

        public void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize)
        {
            UpdateMarqueeSelectionCalls++;
            LastMarqueePoint = currentScreenPosition;
            LastMarqueeFinalize = finalize;
        }

        public GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY)
        {
            ApplyDragAssistCalls++;
            return DragAssistResult;
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
