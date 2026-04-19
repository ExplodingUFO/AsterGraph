using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Automation;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasStandaloneTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.canvas.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.canvas.target");
    private const string SourceNodeId = "tests.canvas.source-001";
    private const string TargetNodeId = "tests.canvas.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [AvaloniaFact]
    public void StandaloneCanvasFactory_BindsSharedEditorAndKeepsDefaultInteractionsEnabled()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var allText = string.Join(
                "\n",
                canvas.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            Assert.Same(editor, canvas.ViewModel);
            Assert.True(canvas.EnableDefaultContextMenu);
            Assert.True(canvas.EnableDefaultCommandShortcuts);
            Assert.True(canvas.Focusable);
            Assert.Contains("Canvas Source", allText);
            Assert.Null(canvas.NodeVisualPresenter);
            Assert.Null(canvas.ContextMenuPresenter);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CanvasContextRequest_UsesDefaultContextMenuOnlyWhenEnabled()
    {
        var enabledEditor = CreateEditor();
        var enabledMenuPresenter = new RecordingContextMenuPresenter();
        var (enabledWindow, enabledCanvas) = CreateStandaloneCanvasWindow(
            enabledEditor,
            enableDefaultContextMenu: true,
            presentation: new AsterGraphPresentationOptions
            {
                ContextMenuPresenter = enabledMenuPresenter,
            });
        var enabledArgs = new ContextRequestedEventArgs();
        try
        {
            InvokeCanvasContextRequested(enabledCanvas, enabledArgs);

            Assert.True(enabledArgs.Handled);
            Assert.Equal(1, enabledMenuPresenter.OpenCalls);
            Assert.NotNull(enabledMenuPresenter.LastTarget);
            Assert.NotNull(enabledMenuPresenter.LastDescriptors);
            Assert.NotEmpty(enabledMenuPresenter.LastDescriptors!);

            var disabledEditor = CreateEditor();
            var disabledMenuPresenter = new RecordingContextMenuPresenter();
            var (disabledWindow, disabledCanvas) = CreateStandaloneCanvasWindow(
                disabledEditor,
                enableDefaultContextMenu: false,
                presentation: new AsterGraphPresentationOptions
                {
                    ContextMenuPresenter = disabledMenuPresenter,
                });
            var disabledArgs = new ContextRequestedEventArgs();
            try
            {
                InvokeCanvasContextRequested(disabledCanvas, disabledArgs);

                Assert.False(disabledArgs.Handled);
                Assert.Equal(0, disabledMenuPresenter.OpenCalls);
            }
            finally
            {
                disabledWindow.Close();
            }
        }
        finally
        {
            enabledWindow.Close();
        }
    }

    [AvaloniaFact]
    public void CanvasContextRequest_UsesCanonicalDescriptorPresenterOverload_WhenAvailable()
    {
        var editor = CreateEditor();
        var presenter = new CanonicalRecordingContextMenuPresenter();
        var (window, canvas) = CreateStandaloneCanvasWindow(
            editor,
            enableDefaultContextMenu: true,
            presentation: new AsterGraphPresentationOptions
            {
                ContextMenuPresenter = presenter,
            });
        var args = new ContextRequestedEventArgs();

        try
        {
            InvokeCanvasContextRequested(canvas, args);

            Assert.True(args.Handled);
            Assert.Equal(1, presenter.CanonicalOpenCalls);
            Assert.Equal(0, presenter.CompatibilityOpenCalls);
            Assert.NotNull(presenter.LastCanonicalDescriptors);
            Assert.Contains(presenter.LastCanonicalDescriptors!, descriptor => descriptor.Id == "canvas-add-node");
            Assert.NotNull(presenter.LastCommands);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void EscapeShortcut_CancelsPendingConnectionOnlyWhenDefaultShortcutsEnabled()
    {
        var enabledEditor = CreateEditor();
        enabledEditor.StartConnection(SourceNodeId, SourcePortId);
        var (enabledWindow, enabledCanvas) = CreateStandaloneCanvasWindow(enabledEditor, enableDefaultCommandShortcuts: true);
        var enabledArgs = new KeyEventArgs
        {
            Key = Key.Escape,
        };
        try
        {
            InvokeCanvasKeyDown(enabledCanvas, enabledArgs);

            Assert.True(enabledArgs.Handled);
            Assert.False(enabledEditor.HasPendingConnection);

            var disabledEditor = CreateEditor();
            disabledEditor.StartConnection(SourceNodeId, SourcePortId);
            var (disabledWindow, disabledCanvas) = CreateStandaloneCanvasWindow(disabledEditor, enableDefaultCommandShortcuts: false);
            var disabledArgs = new KeyEventArgs
            {
                Key = Key.Escape,
            };
            try
            {
                InvokeCanvasKeyDown(disabledCanvas, disabledArgs);

                Assert.False(disabledArgs.Handled);
                Assert.True(disabledEditor.HasPendingConnection);
            }
            finally
            {
                disabledWindow.Close();
            }
        }
        finally
        {
            enabledWindow.Close();
        }
    }

    [AvaloniaFact]
    public void DeleteShortcut_RemovesSelectionOnlyWhenDefaultShortcutsEnabled()
    {
        var enabledEditor = CreateEditor();
        enabledEditor.SelectSingleNode(enabledEditor.Nodes[0], updateStatus: false);
        var (enabledWindow, enabledCanvas) = CreateStandaloneCanvasWindow(enabledEditor, enableDefaultCommandShortcuts: true);
        var enabledArgs = new KeyEventArgs
        {
            Key = Key.Delete,
        };
        try
        {
            InvokeCanvasKeyDown(enabledCanvas, enabledArgs);

            Assert.True(enabledArgs.Handled);
            Assert.Single(enabledEditor.Nodes);
            Assert.Equal(TargetNodeId, enabledEditor.Nodes[0].Id);

            var disabledEditor = CreateEditor();
            disabledEditor.SelectSingleNode(disabledEditor.Nodes[0], updateStatus: false);
            var (disabledWindow, disabledCanvas) = CreateStandaloneCanvasWindow(disabledEditor, enableDefaultCommandShortcuts: false);
            var disabledArgs = new KeyEventArgs
            {
                Key = Key.Delete,
            };
            try
            {
                InvokeCanvasKeyDown(disabledCanvas, disabledArgs);

                Assert.False(disabledArgs.Handled);
                Assert.Equal(2, disabledEditor.Nodes.Count);
                Assert.Equal(SourceNodeId, disabledEditor.SelectedNode?.Id);
            }
            finally
            {
                disabledWindow.Close();
            }
        }
        finally
        {
            enabledWindow.Close();
        }
    }

    [AvaloniaFact]
    public void ManualPortClick_CanReconnectSameEndpointsAfterDeletingConnection()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            ClickPortButton(canvas, SourcePortId, PortDirection.Output);
            Assert.True(editor.HasPendingConnection);

            ClickPortButton(canvas, TargetPortId, PortDirection.Input);
            var initialConnection = Assert.Single(editor.Connections);
            Assert.False(editor.HasPendingConnection);

            var deleted = editor.Session.Commands.TryExecuteCommand(
                new GraphEditorCommandInvocationSnapshot(
                    "connections.delete",
                    [
                        new GraphEditorCommandArgumentSnapshot("connectionId", initialConnection.Id),
                    ]));

            Assert.True(deleted);
            Assert.Empty(editor.Connections);

            ClickPortButton(canvas, SourcePortId, PortDirection.Output);
            Assert.True(editor.HasPendingConnection);

            ClickPortButton(canvas, TargetPortId, PortDirection.Input);
            var reconnected = Assert.Single(editor.Connections);
            Assert.False(editor.HasPendingConnection);
            Assert.Equal(SourceNodeId, reconnected.SourceNodeId);
            Assert.Equal(SourcePortId, reconnected.SourcePortId);
            Assert.Equal(TargetNodeId, reconnected.TargetNodeId);
            Assert.Equal(TargetPortId, reconnected.TargetPortId);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneCanvas_CustomNodeVisualPresenter_ChangesVisualTreeAndPublishesPortAnchors()
    {
        var editor = CreateEditor();
        var customPresenter = new CustomNodeVisualPresenter();
        var (window, canvas) = CreateStandaloneCanvasWindow(
            editor,
            presentation: new AsterGraphPresentationOptions
            {
                NodeVisualPresenter = customPresenter,
            });

        try
        {
            var allText = string.Join(
                "\n",
                canvas.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            var sourceNode = editor.Nodes[0];
            var sourcePort = sourceNode.Outputs[0];
            var anchor = InvokeCanvasMethod<GraphPoint>("GetPortAnchor", canvas, sourceNode, sourcePort);

            Assert.Same(customPresenter, canvas.NodeVisualPresenter);
            Assert.Contains("CUSTOM NODE VISUAL:Canvas Source", allText);
            Assert.InRange(anchor.X, sourceNode.X + 36.5, sourceNode.X + 37.5);
            Assert.InRange(anchor.Y, sourceNode.Y + 24.5, sourceNode.Y + 25.5);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneCanvas_CustomNodeVisualPresenter_UpdatesWhenSelectionAndPresentationChange()
    {
        var editor = CreateEditor();
        var customPresenter = new CustomNodeVisualPresenter();
        var (window, canvas) = CreateStandaloneCanvasWindow(
            editor,
            presentation: new AsterGraphPresentationOptions
            {
                NodeVisualPresenter = customPresenter,
            });

        try
        {
            var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var afterInitialRender = customPresenter.UpdateCalls;

            editor.SelectSingleNode(sourceNode, updateStatus: false);

            Assert.True(customPresenter.UpdateCalls >= afterInitialRender + editor.Nodes.Count);
            var afterSelection = customPresenter.UpdateCalls;

            sourceNode.UpdatePresentation(new NodePresentationState(
                StatusBar: new NodeStatusBarDescriptor("Ready", "#7FE7D7")));

            Assert.True(customPresenter.UpdateCalls >= afterSelection + 1);
            Assert.Equal(sourceNode.Id, customPresenter.LastUpdatedNodeId);
            Assert.True(customPresenter.LastUpdatedNodeWasSelected);
            Assert.Equal(sourceNode.Height, customPresenter.LastUpdatedNodeHeight);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void WheelViewportGestures_CanBeDisabledForHostCooperativeScrolling()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var args = new PointerWheelEventArgs(
            canvas,
            null!,
            canvas,
            new Point(24, 24),
            0UL,
            new PointerPointProperties(),
            KeyModifiers.None,
            new Vector(0, 1));

        try
        {
            canvas.EnableDefaultWheelViewportGestures = false;
            InvokeCanvasWheelChanged(canvas, args);

            Assert.False(args.Handled);
            Assert.Equal(110, editor.PanX);
            Assert.Equal(96, editor.PanY);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AltLeftDragPanning_Property_CanBeDisabled()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            Assert.True(canvas.EnableAltLeftDragPanning);
            canvas.EnableAltLeftDragPanning = false;
            Assert.False(canvas.EnableAltLeftDragPanning);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AltLeftDragPanning_RoutesThroughCanvasHandlers_AndReleasesPointerCapture()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);
        var pressedArgs = CreatePointerPressedArgs(canvas, pointer, new Point(80, 80), KeyModifiers.Alt);
        var movedArgs = CreatePointerMovedArgs(canvas, pointer, new Point(140, 170), KeyModifiers.Alt);
        var releasedArgs = CreatePointerReleasedArgs(canvas, pointer, new Point(140, 170), KeyModifiers.Alt);

        try
        {
            InvokeCanvasPointerPressed(canvas, pressedArgs);
            Assert.True(pressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);

            InvokeCanvasPointerMoved(canvas, movedArgs);
            Assert.True(movedArgs.Handled);
            Assert.Equal(170, editor.PanX);
            Assert.Equal(186, editor.PanY);

            InvokeCanvasPointerReleased(canvas, releasedArgs);
            Assert.Null(pointer.Captured);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MarqueeSelection_RoutesThroughCanvasHandlers_AndSelectsHitNodes()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);
        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var start = InvokeCanvasMethod<GraphPoint>("WorldToScreen", canvas, sourceNode.X - 24, sourceNode.Y - 24);
        var finish = InvokeCanvasMethod<GraphPoint>("WorldToScreen", canvas, sourceNode.X + sourceNode.Width + 24, sourceNode.Y + sourceNode.Height + 24);
        var pressedArgs = CreatePointerPressedArgs(canvas, pointer, new Point(start.X, start.Y), KeyModifiers.None);
        var movedArgs = CreatePointerMovedArgs(canvas, pointer, new Point(finish.X, finish.Y), KeyModifiers.None);
        var releasedArgs = CreatePointerReleasedArgs(canvas, pointer, new Point(finish.X, finish.Y), KeyModifiers.None);

        try
        {
            InvokeCanvasPointerPressed(canvas, pressedArgs);
            Assert.True(pressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);

            InvokeCanvasPointerMoved(canvas, movedArgs);
            Assert.True(movedArgs.Handled);

            InvokeCanvasPointerReleased(canvas, releasedArgs);

            var selected = Assert.Single(editor.SelectedNodes);
            Assert.Equal(SourceNodeId, selected.Id);
            Assert.Equal(SourceNodeId, editor.SelectedNode?.Id);
            Assert.Null(pointer.Captured);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DoubleTappedNode_TogglesExpandedSurfaceState_AndRendersInlineSection()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes.Single(node => node.Id == TargetNodeId), updateStatus: false);
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);
        var pointerArgs = CreatePointerPressedArgs(canvas, pointer, new Point(420, 160), KeyModifiers.None);

        try
        {
            var nodeRoot = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(border => string.Equals(
                    AutomationProperties.GetName(border),
                    "Canvas Target node",
                    StringComparison.Ordinal));

            nodeRoot.RaiseEvent(new TappedEventArgs(InputElement.DoubleTappedEvent, pointerArgs)
            {
                Source = nodeRoot,
            });

            var surface = editor.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == TargetNodeId);

            Assert.Equal(GraphNodeExpansionState.Expanded, surface.ExpansionState);
            Assert.True(editor.Nodes.Single(node => node.Id == TargetNodeId).Height > 160d);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ExpandedNode_RendersInlineInputEditor_WhenInputIsUnconnected()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes.Single(node => node.Id == TargetNodeId), updateStatus: false);
        Assert.True(editor.Session.Commands.TrySetNodeExpansionState(TargetNodeId, GraphNodeExpansionState.Expanded));
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var inlineEditor = canvas.GetVisualDescendants()
                .OfType<TextBox>()
                .SingleOrDefault(control => control.DataContext is NodeParameterViewModel parameter
                                            && string.Equals(parameter.Key, "gain", StringComparison.Ordinal));

            Assert.NotNull(inlineEditor);
            Assert.Equal("0.35", inlineEditor!.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ExpandedNode_ShowsConnectedOverrideMessage_WhenInlineInputReceivesUpstreamValue()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes.Single(node => node.Id == TargetNodeId), updateStatus: false);
        editor.Session.Commands.StartConnection(SourceNodeId, SourcePortId);
        editor.Session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        Assert.True(editor.Session.Commands.TrySetNodeExpansionState(TargetNodeId, GraphNodeExpansionState.Expanded));
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var allText = string.Join(
                "\n",
                canvas.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            Assert.Contains("Connected input overrides the local literal.", allText, StringComparison.Ordinal);
            Assert.DoesNotContain(
                canvas.GetVisualDescendants().OfType<TextBox>(),
                control => control.DataContext is NodeParameterViewModel parameter
                           && string.Equals(parameter.Key, "gain", StringComparison.Ordinal));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ResizeThumb_UpdatesPersistedNodeWidth()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var thumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target resize handle",
                    StringComparison.Ordinal));

            thumb.RaiseEvent(new VectorEventArgs
            {
                RoutedEvent = Thumb.DragDeltaEvent,
                Vector = new Vector(80, 0),
                Source = thumb,
            });

            var target = editor.Nodes.Single(node => node.Id == TargetNodeId);
            Assert.Equal(320d, target.Width);

            var surface = editor.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == TargetNodeId);
            Assert.Equal(320d, surface.Size.Width);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupHeader_RendersAtPersistedPosition_SelectsMembers_AndTogglesCollapse()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));
        Assert.True(editor.Session.Commands.TrySetNodeGroupPosition(groupId!, new GraphPoint(80, 60), moveMemberNodes: false, updateStatus: false));

        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);
        var pointerArgs = CreatePointerPressedArgs(canvas, pointer, new Point(120, 88), KeyModifiers.None);

        try
        {
            var groupSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group",
                    StringComparison.Ordinal));
            var groupHeader = canvas.GetVisualDescendants()
                .OfType<Button>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group header",
                    StringComparison.Ordinal));

            Assert.Equal(80d, Canvas.GetLeft(groupSurface));
            Assert.Equal(60d, Canvas.GetTop(groupSurface));

            groupHeader.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)
            {
                Source = groupHeader,
            });

            Assert.Equal(
                [SourceNodeId, TargetNodeId],
                editor.SelectedNodes.Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal));

            groupHeader.RaiseEvent(new TappedEventArgs(InputElement.DoubleTappedEvent, pointerArgs)
            {
                Source = groupHeader,
            });

            Assert.True(editor.Session.Queries.GetNodeGroups().Single().IsCollapsed);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupSurface_UsesResolvedBounds_WhenMemberNodeExpands()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var groupSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group",
                    StringComparison.Ordinal));
            var initialSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());

            Assert.Equal(initialSnapshot.Size.Height, groupSurface.Height);

            Assert.True(editor.Session.Commands.TrySetNodeExpansionState(TargetNodeId, GraphNodeExpansionState.Expanded));

            var expandedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            Assert.True(expandedSnapshot.Size.Height > initialSnapshot.Size.Height);
            Assert.Equal(expandedSnapshot.Size.Height, groupSurface.Height);
            Assert.Equal(expandedSnapshot.Position.Y, Canvas.GetTop(groupSurface));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupResizeThumb_UpdatesPersistedPadding_AndResolvedSurfaceWidth()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var groupSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group",
                    StringComparison.Ordinal));
            var thumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group right resize handle",
                    StringComparison.Ordinal));
            var initialSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());

            thumb.RaiseEvent(new VectorEventArgs
            {
                RoutedEvent = Thumb.DragDeltaEvent,
                Vector = new Vector(36, 0),
                Source = thumb,
            });

            var group = Assert.Single(editor.Session.Queries.GetNodeGroups());
            var resizedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());

            Assert.Equal(new GraphPadding(24, 44, 60, 28), group.ExtraPadding);
            Assert.Equal(initialSnapshot.Size.Width + 36d, resizedSnapshot.Size.Width);
            Assert.Equal(resizedSnapshot.Size.Width, groupSurface.Width);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupDrag_MovesMemberNodes_PreservesPadding_AndSupportsUndoRedo()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));
        Assert.True(editor.Session.Commands.TrySetNodeGroupExtraPadding(groupId!, new GraphPadding(36, 52, 30, 24), updateStatus: false));

        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var targetNode = editor.Nodes.Single(node => node.Id == TargetNodeId);
        var initialSource = new GraphPoint(sourceNode.X, sourceNode.Y);
        var initialTarget = new GraphPoint(targetNode.X, targetNode.Y);
        var initialSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());

        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var groupSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group",
                    StringComparison.Ordinal));
            var pressedArgs = CreatePointerPressedArgs(groupSurface, canvas, pointer, new Point(120, 88), KeyModifiers.None);
            var movedArgs = CreatePointerMovedArgs(canvas, pointer, new Point(164, 110), KeyModifiers.None);
            var releasedArgs = CreatePointerReleasedArgs(canvas, pointer, new Point(164, 110), KeyModifiers.None);

            groupSurface.RaiseEvent(pressedArgs);
            Assert.Same(canvas, pointer.Captured);

            InvokeCanvasPointerMoved(canvas, movedArgs);
            InvokeCanvasPointerReleased(canvas, releasedArgs);

            var movedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var movedGroup = Assert.Single(editor.Session.Queries.GetNodeGroups());
            Assert.Equal(new GraphPadding(36, 52, 30, 24), movedGroup.ExtraPadding);
            Assert.Equal(initialSource.X + 50d, sourceNode.X);
            Assert.Equal(initialSource.Y + 25d, sourceNode.Y);
            Assert.Equal(initialTarget.X + 50d, targetNode.X);
            Assert.Equal(initialTarget.Y + 25d, targetNode.Y);
            Assert.Equal(initialSnapshot.Position.X + 50d, movedSnapshot.Position.X);
            Assert.Equal(initialSnapshot.Position.Y + 25d, movedSnapshot.Position.Y);

            editor.Undo();

            var undoneSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var undoneSource = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var undoneTarget = editor.Nodes.Single(node => node.Id == TargetNodeId);
            Assert.Equal(initialSource.X, undoneSource.X);
            Assert.Equal(initialSource.Y, undoneSource.Y);
            Assert.Equal(initialTarget.X, undoneTarget.X);
            Assert.Equal(initialTarget.Y, undoneTarget.Y);
            Assert.Equal(initialSnapshot.Position, undoneSnapshot.Position);

            editor.Redo();

            var redoneSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var redoneSource = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var redoneTarget = editor.Nodes.Single(node => node.Id == TargetNodeId);
            Assert.Equal(initialSource.X + 50d, redoneSource.X);
            Assert.Equal(initialSource.Y + 25d, redoneSource.Y);
            Assert.Equal(initialTarget.X + 50d, redoneTarget.X);
            Assert.Equal(initialTarget.Y + 25d, redoneTarget.Y);
            Assert.Equal(initialSnapshot.Position.X + 50d, redoneSnapshot.Position.X);
            Assert.Equal(initialSnapshot.Position.Y + 25d, redoneSnapshot.Position.Y);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeDrag_AttachesNodeToGroup_WhenDroppedInside_AndDetaches_WhenDroppedOutside()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var targetSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target node",
                    StringComparison.Ordinal));

            var initialGroup = Assert.Single(editor.GetNodeGroupSnapshots());
            var targetNode = editor.FindNode(TargetNodeId)!;
            var desiredAttachedX = initialGroup.Position.X + ((initialGroup.Size.Width - targetNode.Width) / 2d);
            var desiredAttachedY = initialGroup.Position.Y + ((initialGroup.Size.Height - targetNode.Height) / 2d);
            var attachStart = new Point(targetNode.X + 20d, targetNode.Y + 20d);
            var attachDestination = new Point(
                attachStart.X + ((desiredAttachedX - targetNode.X) * editor.Zoom),
                attachStart.Y + ((desiredAttachedY - targetNode.Y) * editor.Zoom));

            targetSurface.RaiseEvent(CreatePointerPressedArgs(targetSurface, canvas, pointer, attachStart, KeyModifiers.None));
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, attachDestination, KeyModifiers.None));
            targetNode = editor.FindNode(TargetNodeId)!;
            Assert.InRange(targetNode.X, desiredAttachedX - 2d, desiredAttachedX + 2d);
            Assert.InRange(targetNode.Y, desiredAttachedY - 2d, desiredAttachedY + 2d);
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, attachDestination, KeyModifiers.None));

            targetNode = editor.FindNode(TargetNodeId)!;
            var attachedGroup = Assert.Single(editor.GetNodeGroups());
            Assert.Equal(groupId, targetNode.GroupId);
            Assert.Equal(
                [SourceNodeId, TargetNodeId],
                attachedGroup.NodeIds.OrderBy(id => id, StringComparer.Ordinal).ToArray());

            var attachedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var detachStart = new Point(targetNode.X + 20d, targetNode.Y + 20d);
            var detachDestination = new Point(
                detachStart.X + (((attachedSnapshot.Position.X + attachedSnapshot.Size.Width + 160d) - targetNode.X) * editor.Zoom),
                detachStart.Y);

            targetSurface.RaiseEvent(CreatePointerPressedArgs(targetSurface, canvas, pointer, detachStart, KeyModifiers.None));
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, detachDestination, KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, detachDestination, KeyModifiers.None));

            targetNode = editor.FindNode(TargetNodeId)!;
            var remainingGroup = Assert.Single(editor.GetNodeGroups());
            Assert.Null(targetNode.GroupId);
            Assert.Equal([SourceNodeId], remainingGroup.NodeIds);
        }
        finally
        {
            window.Close();
        }
    }

    private static void InvokeCanvasContextRequested(NodeCanvas canvas, ContextRequestedEventArgs args)
        => InvokeCanvasHandler("HandleCanvasContextRequested", canvas, args);

    private static void InvokeCanvasKeyDown(NodeCanvas canvas, KeyEventArgs args)
        => InvokeCanvasHandler("HandleCanvasKeyDown", canvas, args);

    private static void InvokeCanvasPointerPressed(NodeCanvas canvas, PointerPressedEventArgs args)
        => InvokeCanvasHandler("HandlePointerPressed", canvas, args);

    private static void InvokeCanvasPointerMoved(NodeCanvas canvas, PointerEventArgs args)
        => InvokeCanvasHandler("HandlePointerMoved", canvas, args);

    private static void InvokeCanvasPointerReleased(NodeCanvas canvas, PointerReleasedEventArgs args)
        => InvokeCanvasHandler("HandlePointerReleased", canvas, args);

    private static void InvokeCanvasWheelChanged(NodeCanvas canvas, PointerWheelEventArgs args)
        => InvokeCanvasHandler("HandlePointerWheelChanged", canvas, args);

    private static PointerPressedEventArgs CreatePointerPressedArgs(NodeCanvas canvas, global::Avalonia.Input.Pointer pointer, Point position, KeyModifiers modifiers)
        => CreatePointerPressedArgs(canvas, canvas, pointer, position, modifiers);

    private static PointerPressedEventArgs CreatePointerPressedArgs(
        InputElement source,
        NodeCanvas canvas,
        global::Avalonia.Input.Pointer pointer,
        Point position,
        KeyModifiers modifiers)
        => new(
            source,
            pointer,
            canvas,
            position,
            0UL,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton | ToRawModifiers(modifiers), PointerUpdateKind.LeftButtonPressed),
            modifiers,
            1);

    private static PointerEventArgs CreatePointerMovedArgs(NodeCanvas canvas, global::Avalonia.Input.Pointer pointer, Point position, KeyModifiers modifiers)
        => new(
            InputElement.PointerMovedEvent,
            canvas,
            pointer,
            canvas,
            position,
            0UL,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton | ToRawModifiers(modifiers), PointerUpdateKind.Other),
            modifiers);

    private static PointerReleasedEventArgs CreatePointerReleasedArgs(NodeCanvas canvas, global::Avalonia.Input.Pointer pointer, Point position, KeyModifiers modifiers)
        => new(
            canvas,
            pointer,
            canvas,
            position,
            0UL,
            new PointerPointProperties(ToRawModifiers(modifiers), PointerUpdateKind.LeftButtonReleased),
            modifiers,
            MouseButton.Left);

    private static RawInputModifiers ToRawModifiers(KeyModifiers modifiers)
    {
        var raw = RawInputModifiers.None;
        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            raw |= RawInputModifiers.Alt;
        }

        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            raw |= RawInputModifiers.Control;
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            raw |= RawInputModifiers.Shift;
        }

        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            raw |= RawInputModifiers.Meta;
        }

        return raw;
    }

    private static void ClickPortButton(NodeCanvas canvas, string portId, PortDirection direction)
    {
        var button = canvas.GetVisualDescendants()
            .OfType<Button>()
            .Single(control => control.DataContext is PortViewModel port
                               && string.Equals(port.Id, portId, StringComparison.Ordinal)
                               && port.Direction == direction);

        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static void InvokeCanvasHandler(string methodName, NodeCanvas canvas, object args)
    {
        var method = typeof(NodeCanvas).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Could not find NodeCanvas handler '{methodName}'.");
        method.Invoke(canvas, [canvas, args]);
    }

    private static T InvokeCanvasMethod<T>(string methodName, NodeCanvas canvas, params object[] args)
    {
        var method = typeof(NodeCanvas).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Could not find NodeCanvas method '{methodName}'.");
        return Assert.IsType<T>(method.Invoke(canvas, args));
    }

    private static Window CreateWindow(Control content)
    {
        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = content,
        };
        window.Show();
        return window;
    }

    private static (Window Window, NodeCanvas Canvas) CreateStandaloneCanvasWindow(
        GraphEditorViewModel editor,
        bool enableDefaultContextMenu = true,
        bool enableDefaultCommandShortcuts = true,
        AsterGraphPresentationOptions? presentation = null)
    {
        var canvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = editor,
            EnableDefaultContextMenu = enableDefaultContextMenu,
            EnableDefaultCommandShortcuts = enableDefaultCommandShortcuts,
            Presentation = presentation,
        });

        return (CreateWindow(canvas), canvas);
    }

    private static GraphEditorViewModel CreateEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                SourceDefinitionId,
                "Canvas Source",
                "Tests",
                "Standalone canvas source node.",
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
                "Canvas Target",
                "Tests",
                "Standalone canvas target node.",
                [
                    new PortDefinition(
                        TargetPortId,
                        "Input",
                        new PortTypeId("float"),
                        "#F3B36B",
                        inlineParameterKey: "gain"),
                ],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "gain",
                        "Gain",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Inline input literal shown when the input is unconnected.",
                        defaultValue: 0.35d),
                ]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Standalone Canvas Graph",
                "Regression coverage for standalone canvas surface composition.",
                [
                    new GraphNode(
                        SourceNodeId,
                        "Canvas Source",
                        "Tests",
                        "Standalone Canvas",
                        "Used to start pending connections and selection commands.",
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
                        "Canvas Target",
                        "Tests",
                        "Standalone Canvas",
                        "Used to verify delete shortcuts and shared editor binding.",
                        new GraphPoint(420, 160),
                        new GraphSize(240, 160),
                        [
                            new GraphPort(
                                TargetPortId,
                                "Input",
                                PortDirection.Input,
                                "float",
                                "#F3B36B",
                                new PortTypeId("float"),
                                "gain"),
                        ],
                        [],
                        "#F3B36B",
                        TargetDefinitionId,
                        [
                            new GraphParameterValue("gain", new PortTypeId("float"), 0.35d),
                        ]),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            styleOptions: GraphEditorStyleOptions.Default);
    }

    private sealed class RecordingContextMenuPresenter : IGraphContextMenuPresenter
    {
        public int OpenCalls { get; private set; }

        public Control? LastTarget { get; private set; }

        public IReadOnlyList<MenuItemDescriptor>? LastDescriptors { get; private set; }

        public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
        {
            OpenCalls++;
            LastTarget = target;
            LastDescriptors = descriptors;
        }
    }

    private sealed class CanonicalRecordingContextMenuPresenter : IGraphContextMenuPresenter
    {
        public int CompatibilityOpenCalls { get; private set; }

        public int CanonicalOpenCalls { get; private set; }

        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot>? LastCanonicalDescriptors { get; private set; }

        public IGraphEditorCommands? LastCommands { get; private set; }

        public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
            => CompatibilityOpenCalls++;

        public void Open(
            Control target,
            IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> descriptors,
            IGraphEditorCommands commands,
            ContextMenuStyleOptions style)
        {
            CanonicalOpenCalls++;
            LastCanonicalDescriptors = descriptors;
            LastCommands = commands;
        }
    }

    private sealed class CustomNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        public int CreateCalls { get; private set; }

        public int UpdateCalls { get; private set; }

        public string? LastUpdatedNodeId { get; private set; }

        public bool LastUpdatedNodeWasSelected { get; private set; }

        public double LastUpdatedNodeHeight { get; private set; }

        public GraphNodeVisual Create(GraphNodeVisualContext context)
        {
            CreateCalls++;
            var surface = new Border
            {
                Width = context.Node.Width,
                Height = context.Node.Height,
                Background = Brushes.Transparent,
            };
            var layout = new Canvas
            {
                Width = context.Node.Width,
                Height = context.Node.Height,
            };

            var title = new TextBlock
            {
                Text = $"CUSTOM NODE VISUAL:{context.Node.Title}",
            };
            Canvas.SetLeft(title, 8);
            Canvas.SetTop(title, 8);
            layout.Children.Add(title);

            var portAnchors = new Dictionary<string, Control>(StringComparer.Ordinal);
            var allPorts = context.Node.Inputs.Concat(context.Node.Outputs).ToList();
            for (var index = 0; index < allPorts.Count; index++)
            {
                var port = allPorts[index];
                var anchor = new Border
                {
                    Width = 10,
                    Height = 10,
                    Background = Brush.Parse(port.AccentHex),
                    DataContext = port,
                };
                Canvas.SetLeft(anchor, 32 + (index * 24));
                Canvas.SetTop(anchor, 20 + (index * 18));
                layout.Children.Add(anchor);
                portAnchors[port.Id] = anchor;
            }

            surface.PointerPressed += (_, args) =>
            {
                if (args.Source is StyledElement { DataContext: PortViewModel })
                {
                    return;
                }

                context.BeginNodeDrag(context.Node, args);
            };

            surface.Child = layout;
            return new GraphNodeVisual(surface, portAnchors);
        }

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        {
            UpdateCalls++;
            LastUpdatedNodeId = context.Node.Id;
            LastUpdatedNodeWasSelected = context.Node.IsSelected;
            LastUpdatedNodeHeight = context.Node.Height;
            visual.Root.Width = context.Node.Width;
            visual.Root.Height = context.Node.Height;
        }
    }
}
