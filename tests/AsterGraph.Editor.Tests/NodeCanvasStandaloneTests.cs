using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
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
    private static readonly NodeDefinitionId AdaptiveDefinitionId = new("tests.canvas.adaptive");
    private static readonly NodeDefinitionId OutputDefinitionId = new("tests.canvas.output");
    private const string SourceNodeId = "tests.canvas.source-001";
    private const string TargetNodeId = "tests.canvas.target-001";
    private const string AdaptiveNodeId = "tests.canvas.adaptive-001";
    private const string OutputNodeId = "tests.canvas.output-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";
    private const string AdaptiveOutputPortId = "result";
    private const string OutputInputPortId = "viewport";
    private const string RequiredParameterKey = "required-input";
    private const string OptionalParameterKey = "optional-gain";

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
    public void ManualPortClick_CanReconnectSameEndpointsAfterDisconnectingConnection()
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

            var disconnected = editor.Session.Commands.TryExecuteCommand(
                new GraphEditorCommandInvocationSnapshot(
                    "connections.disconnect",
                    [
                        new GraphEditorCommandArgumentSnapshot("connectionId", initialConnection.Id),
                    ]));

            Assert.True(disconnected);
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
    public void ResizedNode_ResolvesRicherSurfaceTier()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes.Single(node => node.Id == TargetNodeId), updateStatus: false);
        Assert.True(editor.Session.Commands.TrySetNodeSize(TargetNodeId, new GraphSize(420d, 260d), updateStatus: false));
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var surface = editor.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == TargetNodeId);

            Assert.Equal("parameter-editors", surface.ActiveTier.Key);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DefaultNodeVisual_UsesRenderedMinimumHeightBeyondPersistedSurface()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var target = editor.Nodes.Single(node => node.Id == TargetNodeId);
            var targetSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target node",
                    StringComparison.Ordinal));

            Assert.True(targetSurface.Height > target.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StatusBarPresentation_IncreasesRenderedHeightWithoutChangingPersistedHeight()
    {
        var editor = CreateEditor();
        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var persistedHeight = sourceNode.Height;
        sourceNode.UpdatePresentation(new NodePresentationState(
            StatusBar: new NodeStatusBarDescriptor("Ready", "#7FE7D7")));
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var sourceSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Source node",
                    StringComparison.Ordinal));

            Assert.Equal(persistedHeight, sourceNode.Height);
            Assert.True(sourceSurface.Height > persistedHeight);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ParameterRail_UsesTierThresholdsForEndpointAndEditorVisibility()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            Assert.DoesNotContain(
                canvas.GetVisualDescendants().OfType<Button>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target parameter endpoint Gain",
                    StringComparison.Ordinal));

            Assert.True(editor.Session.Commands.TrySetNodeSize(TargetNodeId, new GraphSize(320d, 210d), updateStatus: false));

            var railEndpoint = Assert.Single(
                canvas.GetVisualDescendants().OfType<Button>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target parameter endpoint Gain",
                    StringComparison.Ordinal));
            Assert.Equal("Canvas Target parameter endpoint Gain", AutomationProperties.GetName(railEndpoint));
            Assert.DoesNotContain(
                canvas.GetVisualDescendants().OfType<NodeParameterEditorHost>(),
                control => string.Equals(control.Parameter?.Key, "gain", StringComparison.Ordinal));

            Assert.True(editor.Session.Commands.TrySetNodeSize(TargetNodeId, new GraphSize(420d, 260d), updateStatus: false));

            railEndpoint = Assert.Single(
                canvas.GetVisualDescendants().OfType<Button>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target parameter endpoint Gain",
                    StringComparison.Ordinal));
            var editorHost = Assert.Single(
                canvas.GetVisualDescendants().OfType<NodeParameterEditorHost>(),
                control => string.Equals(control.Parameter?.Key, "gain", StringComparison.Ordinal));
            Assert.Equal("gain", editorHost.Parameter?.Key);
            Assert.Equal("Canvas Target parameter endpoint Gain", AutomationProperties.GetName(railEndpoint));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ParameterRail_ConnectedEndpointShowsSourceBindingAndConnectionPath()
    {
        var editor = CreateEditor();
        Assert.True(editor.Session.Commands.TrySetNodeSize(TargetNodeId, new GraphSize(420d, 260d), updateStatus: false));
        editor.ConnectToTarget(
            SourceNodeId,
            SourcePortId,
            new GraphConnectionTargetRef(TargetNodeId, "gain", GraphConnectionTargetKind.Parameter));
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var connected = Assert.Single(
                editor.Connections,
                connection => connection.TargetKind == GraphConnectionTargetKind.Parameter);
            Assert.Equal("gain", connected.TargetPortId);

            Assert.Single(canvas.GetVisualDescendants().OfType<global::Avalonia.Controls.Shapes.Path>());
            Assert.DoesNotContain(
                canvas.GetVisualDescendants().OfType<NodeParameterEditorHost>(),
                control => string.Equals(control.Parameter?.Key, "gain", StringComparison.Ordinal));
            var bindingSummary = Assert.Single(
                canvas.GetVisualDescendants().OfType<Control>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target parameter binding Gain",
                    StringComparison.Ordinal));
            var summaryText = Assert.IsType<TextBlock>(
                bindingSummary.GetVisualDescendants().OfType<TextBlock>().First(block => !string.IsNullOrWhiteSpace(block.Text)));
            Assert.Contains("Canvas Source", summaryText.Text, StringComparison.Ordinal);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneCanvas_PrimarySelectionProjectsDistinctSelectedAndInspectedNodeClasses()
    {
        var editor = CreateEditor();
        var source = editor.FindNode(SourceNodeId)!;
        var target = editor.FindNode(TargetNodeId)!;
        editor.SetSelection([source, target], target, status: null);
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var sourceSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Source node",
                    StringComparison.Ordinal));
            var targetSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target node",
                    StringComparison.Ordinal));

            Assert.Contains("astergraph-node-selected", sourceSurface.Classes);
            Assert.DoesNotContain("astergraph-node-inspected", sourceSurface.Classes);
            Assert.DoesNotContain("astergraph-node-editing", sourceSurface.Classes);

            Assert.Contains("astergraph-node-selected", targetSurface.Classes);
            Assert.Contains("astergraph-node-inspected", targetSurface.Classes);
            Assert.DoesNotContain("astergraph-node-editing", targetSurface.Classes);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneCanvas_InspectorEditingFocus_ProjectsEditingNodeClass()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
        editor.SelectSingleNode(target, updateStatus: false);
        editor.SetInspectorEditingParameter("gain");
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var targetSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target node",
                    StringComparison.Ordinal));

            Assert.Contains("astergraph-node-selected", targetSurface.Classes);
            Assert.Contains("astergraph-node-inspected", targetSurface.Classes);
            Assert.Contains("astergraph-node-editing", targetSurface.Classes);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneCanvas_PortTypeCue_UsesCompactUppercaseToken()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var outputButton = canvas.GetVisualDescendants()
                .OfType<Button>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Source output Result",
                    StringComparison.Ordinal));
            var outputText = string.Join(
                " ",
                outputButton.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            Assert.Contains("FLOAT", outputText, StringComparison.Ordinal);
            Assert.DoesNotContain("float", outputText, StringComparison.Ordinal);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneCanvas_OutputNode_ProjectsDedicatedOutputChrome()
    {
        var editor = CreateOutputEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var outputSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Viewport Terminal node",
                    StringComparison.Ordinal));
            var outputText = string.Join(
                " ",
                outputSurface.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            Assert.Contains("astergraph-node-output", outputSurface.Classes);
            Assert.Contains("Output", outputText, StringComparison.Ordinal);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AdaptiveSurface_DefaultSize_ShowsRequiredParameterEndpointOnly()
    {
        var editor = CreateAdaptiveSurfaceEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            Assert.Contains(
                canvas.GetVisualDescendants().OfType<Button>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Adaptive Surface parameter endpoint Required Input",
                    StringComparison.Ordinal));
            Assert.DoesNotContain(
                canvas.GetVisualDescendants().OfType<Button>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Adaptive Surface parameter endpoint Optional Gain",
                    StringComparison.Ordinal));
            Assert.DoesNotContain(
                canvas.GetVisualDescendants().OfType<NodeParameterEditorHost>(),
                control => string.Equals(control.Parameter?.Key, OptionalParameterKey, StringComparison.Ordinal));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AdaptiveSurface_HeightAndWidthProgressivelyRevealOptionalEndpointAndInlineEditors()
    {
        var editor = CreateAdaptiveSurfaceEditor();
        var adaptiveNode = editor.Nodes.Single(node => node.Id == AdaptiveNodeId);
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            Assert.True(editor.Session.Commands.TrySetNodeSize(
                AdaptiveNodeId,
                new GraphSize(adaptiveNode.Width, adaptiveNode.Height + 96d),
                updateStatus: false));

            Assert.Contains(
                canvas.GetVisualDescendants().OfType<Button>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Adaptive Surface parameter endpoint Required Input",
                    StringComparison.Ordinal));
            Assert.Contains(
                canvas.GetVisualDescendants().OfType<Button>(),
                control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Adaptive Surface parameter endpoint Optional Gain",
                    StringComparison.Ordinal));
            Assert.DoesNotContain(
                canvas.GetVisualDescendants().OfType<NodeParameterEditorHost>(),
                control => string.Equals(control.Parameter?.Key, OptionalParameterKey, StringComparison.Ordinal));

            Assert.True(editor.Session.Commands.TrySetNodeSize(
                AdaptiveNodeId,
                new GraphSize(adaptiveNode.Width + 180d, adaptiveNode.Height + 96d),
                updateStatus: false));

            var editorHost = Assert.Single(
                canvas.GetVisualDescendants().OfType<NodeParameterEditorHost>(),
                control => string.Equals(control.Parameter?.Key, OptionalParameterKey, StringComparison.Ordinal));
            Assert.Equal(OptionalParameterKey, editorHost.Parameter?.Key);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BorderResizeHandles_PointerPress_CapturesCanvasForResizeSession()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var widthThumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target width resize handle",
                    StringComparison.Ordinal));
            var pressedArgs = CreatePointerPressedArgs(widthThumb, canvas, pointer, new Point(655, 220), KeyModifiers.None);

            widthThumb.RaiseEvent(pressedArgs);

            Assert.True(pressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeSurfaceHover_NearRightEdge_UsesResizeCursorBeforePress()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
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
            var hoverPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width - 3d,
                target.Y + (targetSurface.Height / 2d));

            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, hoverPoint, KeyModifiers.None));

            AssertCursor(StandardCursorType.SizeWestEast, targetSurface.Cursor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeSurfaceHover_LeavingResizeHotspot_ClearsResizeCursor()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
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
            var edgeHoverPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width - 3d,
                target.Y + (targetSurface.Height / 2d));
            var interiorPoint = WorldToScreenPoint(
                canvas,
                target.X + 32d,
                target.Y + 32d);

            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, edgeHoverPoint, KeyModifiers.None));
            AssertCursor(StandardCursorType.SizeWestEast, targetSurface.Cursor);

            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, interiorPoint, KeyModifiers.None));

            Assert.Null(targetSurface.Cursor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupSurfaceHover_NearBottomEdge_UsesResizeCursorBeforePress()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var groupSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
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
            var hoverPoint = WorldToScreenPoint(
                canvas,
                groupSnapshot.Position.X + (groupSurface.Width / 2d),
                groupSnapshot.Position.Y + groupSurface.Height - 3d);

            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, hoverPoint, KeyModifiers.None));

            AssertCursor(StandardCursorType.SizeNorthSouth, groupSurface.Cursor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ResizeFeedbackPolicy_HoverNodeEdge_UsesHostOverrideCursor()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
        var policy = new RecordingResizeFeedbackPolicy(new Cursor(StandardCursorType.Hand));
        var (window, canvas) = CreateStandaloneCanvasWindow(
            editor,
            presentation: new AsterGraphPresentationOptions
            {
                ResizeFeedbackPolicy = policy,
            });
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var targetSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target node",
                    StringComparison.Ordinal));
            var hoverPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width - 3d,
                target.Y + (targetSurface.Height / 2d));

            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, hoverPoint, KeyModifiers.None));

            AssertCursor(StandardCursorType.Hand, targetSurface.Cursor);
            Assert.Equal(GraphResizeFeedbackSurfaceKind.Node, Assert.Single(policy.Contexts).SurfaceKind);
            Assert.Equal(GraphResizeFeedbackHandle.RightEdge, policy.Contexts[0].Handle);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ResizeFeedbackPolicy_RepeatedHoverSameNodeEdge_ResolvesCursorOnce()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
        var policy = new RecordingResizeFeedbackPolicy(new Cursor(StandardCursorType.Hand));
        var (window, canvas) = CreateStandaloneCanvasWindow(
            editor,
            presentation: new AsterGraphPresentationOptions
            {
                ResizeFeedbackPolicy = policy,
            });
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var targetSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target node",
                    StringComparison.Ordinal));
            var hoverPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width - 3d,
                target.Y + (targetSurface.Height / 2d));

            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, hoverPoint, KeyModifiers.None));
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, hoverPoint, KeyModifiers.None));

            Assert.Single(policy.Contexts);
            AssertCursor(StandardCursorType.Hand, targetSurface.Cursor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ResizeThumbs_DeferCursorToSharedResizeFeedbackSurface()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var (window, canvas) = CreateStandaloneCanvasWindow(editor);

        try
        {
            var widthThumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target width resize handle",
                    StringComparison.Ordinal));
            var groupRightThumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group right resize handle",
                    StringComparison.Ordinal));

            Assert.Null(widthThumb.Cursor);
            Assert.Null(groupRightThumb.Cursor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BorderResizeHandles_PointerPress_PreservesResizeCursorDuringNodeResizeSession()
    {
        var editor = CreateEditor();
        var policy = new RecordingResizeFeedbackPolicy(new Cursor(StandardCursorType.Hand));
        var (window, canvas) = CreateStandaloneCanvasWindow(
            editor,
            presentation: new AsterGraphPresentationOptions
            {
                ResizeFeedbackPolicy = policy,
            });
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var widthThumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target width resize handle",
                    StringComparison.Ordinal));
            var pressedArgs = CreatePointerPressedArgs(widthThumb, canvas, pointer, new Point(655, 220), KeyModifiers.None);

            widthThumb.RaiseEvent(pressedArgs);

            Assert.True(pressedArgs.Handled);
            AssertCursor(StandardCursorType.Hand, canvas.Cursor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BorderResizeHandles_PointerRelease_ClearsActiveResizeCursor()
    {
        var editor = CreateEditor();
        var policy = new RecordingResizeFeedbackPolicy(new Cursor(StandardCursorType.Hand));
        var (window, canvas) = CreateStandaloneCanvasWindow(
            editor,
            presentation: new AsterGraphPresentationOptions
            {
                ResizeFeedbackPolicy = policy,
            });
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var widthThumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target width resize handle",
                    StringComparison.Ordinal));
            var pressedArgs = CreatePointerPressedArgs(widthThumb, canvas, pointer, new Point(655, 220), KeyModifiers.None);

            widthThumb.RaiseEvent(pressedArgs);
            AssertCursor(StandardCursorType.Hand, canvas.Cursor);

            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, new Point(655, 220), KeyModifiers.None));

            Assert.Null(canvas.Cursor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BorderResizeHandles_PointerGesture_UpdatePersistedNodeWidthAndHeight()
    {
        var editor = CreateEditor();
        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var thumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target width resize handle",
                    StringComparison.Ordinal));
            var heightThumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Target height resize handle",
                    StringComparison.Ordinal));
            var widthPressedArgs = CreatePointerPressedArgs(thumb, canvas, pointer, new Point(655, 220), KeyModifiers.None);
            var widthDelta = 48d * editor.Zoom;
            var heightDelta = 36d * editor.Zoom;

            thumb.RaiseEvent(widthPressedArgs);
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, new Point(655 + widthDelta, 220), KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, new Point(655 + widthDelta, 220), KeyModifiers.None));

            heightThumb.RaiseEvent(CreatePointerPressedArgs(heightThumb, canvas, pointer, new Point(540, 350), KeyModifiers.None));
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, new Point(540, 350 + heightDelta), KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, new Point(540, 350 + heightDelta), KeyModifiers.None));

            var target = editor.Nodes.Single(node => node.Id == TargetNodeId);
            Assert.True(target.Width > 280d);
            Assert.True(target.Height >= 195d);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeSurfaceBorderPress_NearRightEdge_ResizesWidth_InsteadOfDragging()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
        var initialPosition = new GraphPoint(target.X, target.Y);
        var initialSize = new GraphSize(target.Width, target.Height);
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
            var pressPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width - 3d,
                target.Y + (targetSurface.Height / 2d));
            var movedPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width + 39d,
                target.Y + (targetSurface.Height / 2d));
            var pressedArgs = CreatePointerPressedArgs(targetSurface, canvas, pointer, pressPoint, KeyModifiers.None);

            targetSurface.RaiseEvent(pressedArgs);
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            target = editor.FindNode(TargetNodeId)!;
            Assert.True(pressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);
            Assert.True(target.Width > initialSize.Width + 30d);
            Assert.Equal(initialPosition.X, target.X);
            Assert.Equal(initialPosition.Y, target.Y);

            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            target = editor.FindNode(TargetNodeId)!;
            Assert.True(target.Width > initialSize.Width + 30d);
            Assert.Equal(initialPosition.X, target.X);
            Assert.Equal(initialPosition.Y, target.Y);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeSurfaceBorderPress_NearBottomEdge_ResizesHeight_InsteadOfDragging()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
        var initialPosition = new GraphPoint(target.X, target.Y);
        var initialSize = new GraphSize(target.Width, target.Height);
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
            var pressPoint = WorldToScreenPoint(
                canvas,
                target.X + (targetSurface.Width / 2d),
                target.Y + targetSurface.Height - 3d);
            var movedPoint = WorldToScreenPoint(
                canvas,
                target.X + (targetSurface.Width / 2d),
                target.Y + targetSurface.Height + 33d);
            var pressedArgs = CreatePointerPressedArgs(targetSurface, canvas, pointer, pressPoint, KeyModifiers.None);

            targetSurface.RaiseEvent(pressedArgs);
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            target = editor.FindNode(TargetNodeId)!;
            Assert.True(pressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);
            Assert.True(target.Height >= initialSize.Height + 30d);
            Assert.Equal(initialPosition.X, target.X);
            Assert.Equal(initialPosition.Y, target.Y);

            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            target = editor.FindNode(TargetNodeId)!;
            Assert.True(target.Height >= initialSize.Height + 30d);
            Assert.Equal(initialPosition.X, target.X);
            Assert.Equal(initialPosition.Y, target.Y);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeSurfaceBorderPress_NearBottomRightCorner_ResizesBothAxes_InsteadOfDragging()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
        var initialPosition = new GraphPoint(target.X, target.Y);
        var initialSize = new GraphSize(target.Width, target.Height);
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
            var pressPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width - 4d,
                target.Y + targetSurface.Height - 4d);
            var movedPoint = WorldToScreenPoint(
                canvas,
                target.X + targetSurface.Width + 36d,
                target.Y + targetSurface.Height + 28d);
            var pressedArgs = CreatePointerPressedArgs(targetSurface, canvas, pointer, pressPoint, KeyModifiers.None);

            targetSurface.RaiseEvent(pressedArgs);
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            target = editor.FindNode(TargetNodeId)!;
            Assert.True(pressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);
            Assert.True(target.Width > initialSize.Width + 30d);
            Assert.True(target.Height >= initialSize.Height + 24d);
            Assert.Equal(initialPosition.X, target.X);
            Assert.Equal(initialPosition.Y, target.Y);

            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            target = editor.FindNode(TargetNodeId)!;
            Assert.True(target.Width > initialSize.Width + 30d);
            Assert.True(target.Height >= initialSize.Height + 24d);
            Assert.Equal(initialPosition.X, target.X);
            Assert.Equal(initialPosition.Y, target.Y);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeSurfaceInteriorPress_AwayFromResizeEdges_PreservesDragBehavior()
    {
        var editor = CreateEditor();
        var target = editor.FindNode(TargetNodeId)!;
        var initialPosition = new GraphPoint(target.X, target.Y);
        var initialSize = new GraphSize(target.Width, target.Height);
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
            var pressPoint = WorldToScreenPoint(canvas, target.X + 24d, target.Y + 24d);
            var movedPoint = WorldToScreenPoint(canvas, target.X + 72d, target.Y + 44d);
            var pressedArgs = CreatePointerPressedArgs(targetSurface, canvas, pointer, pressPoint, KeyModifiers.None);

            targetSurface.RaiseEvent(pressedArgs);
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, movedPoint, KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            target = editor.FindNode(TargetNodeId)!;
            Assert.True(pressedArgs.Handled);
            Assert.Equal(initialSize.Width, target.Width);
            Assert.Equal(initialSize.Height, target.Height);
            Assert.InRange(target.X, initialPosition.X + 47d, initialPosition.X + 49d);
            Assert.InRange(target.Y, initialPosition.Y + 19d, initialPosition.Y + 21d);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupHeader_IsNotButton_AndSupportsSelectionAndCollapse()
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
                .OfType<Control>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group header",
                    StringComparison.Ordinal));

            Assert.Equal(80d, Canvas.GetLeft(groupSurface));
            Assert.Equal(60d, Canvas.GetTop(groupSurface));
            Assert.False(groupHeader is Button);
            Assert.True(groupSurface.ClipToBounds);
            var headerBorder = Assert.IsType<Border>(groupHeader);
            Assert.True(headerBorder.CornerRadius.TopLeft > 0d && headerBorder.CornerRadius.TopRight > 0d);
            Assert.Equal("Canvas Cluster", Assert.IsType<TextBlock>(headerBorder.Child).Text);

            groupHeader.RaiseEvent(new TappedEventArgs(InputElement.TappedEvent, pointerArgs)
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
    public void GroupBodyPress_DoesNotStartDrag()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));
        Assert.True(editor.Session.Commands.TrySetNodeGroupPosition(groupId!, new GraphPoint(80, 60), moveMemberNodes: false, updateStatus: false));

        var sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
        var targetNode = editor.Nodes.Single(node => node.Id == TargetNodeId);
        var initialSource = new GraphPoint(sourceNode.X, sourceNode.Y);
        var initialTarget = new GraphPoint(targetNode.X, targetNode.Y);

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
            var initialSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var pressedPoint = WorldToScreenPoint(
                canvas,
                initialSnapshot.Position.X + (initialSnapshot.Size.Width / 2d),
                initialSnapshot.Position.Y + 84d);
            var movedPoint = WorldToScreenPoint(
                canvas,
                initialSnapshot.Position.X + (initialSnapshot.Size.Width / 2d) + 42d,
                initialSnapshot.Position.Y + 110d);
            var pressedArgs = CreatePointerPressedArgs(
                groupSurface,
                canvas,
                pointer,
                pressedPoint,
                KeyModifiers.None);

            groupSurface.RaiseEvent(pressedArgs);
            Assert.Same(canvas, pointer.Captured);

            InvokeCanvasPointerMoved(
                canvas,
                CreatePointerMovedArgs(canvas, pointer, movedPoint, KeyModifiers.None));
            InvokeCanvasPointerReleased(
                canvas,
                CreatePointerReleasedArgs(canvas, pointer, movedPoint, KeyModifiers.None));

            var unchangedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            sourceNode = editor.Nodes.Single(node => node.Id == SourceNodeId);
            targetNode = editor.Nodes.Single(node => node.Id == TargetNodeId);
            Assert.Equal(initialSnapshot.Position, unchangedSnapshot.Position);
            Assert.Equal(initialSource, new GraphPoint(sourceNode.X, sourceNode.Y));
            Assert.Equal(initialTarget, new GraphPoint(targetNode.X, targetNode.Y));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupSurface_KeepsFixedFrame_WhenMemberNodeResizes()
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

            Assert.True(editor.Session.Commands.TrySetNodeSize(TargetNodeId, new GraphSize(360d, 240d), updateStatus: false));

            var expandedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            Assert.Equal(initialSnapshot.Size.Height, expandedSnapshot.Size.Height);
            Assert.Equal(expandedSnapshot.Size.Height, groupSurface.Height);
            Assert.Equal(expandedSnapshot.Position.Y, Canvas.GetTop(groupSurface));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupResizeHandles_PointerGesture_UpdatesPersistedFixedFrame_AndResolvedSurfaceBounds()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

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
            var thumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group right resize handle",
                    StringComparison.Ordinal));
            var initialSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var initialGroup = Assert.Single(editor.Session.Queries.GetNodeGroups());
            var widthDelta = 36d * editor.Zoom;
            var heightDelta = 24d * editor.Zoom;

            thumb.RaiseEvent(CreatePointerPressedArgs(thumb, canvas, pointer, new Point(652, 240), KeyModifiers.None));
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, new Point(652 + widthDelta, 240), KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, new Point(652 + widthDelta, 240), KeyModifiers.None));

            var bottomThumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group bottom resize handle",
                    StringComparison.Ordinal));
            bottomThumb.RaiseEvent(CreatePointerPressedArgs(bottomThumb, canvas, pointer, new Point(520, 338), KeyModifiers.None));
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, new Point(520, 338 + heightDelta), KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, new Point(520, 338 + heightDelta), KeyModifiers.None));

            var group = Assert.Single(editor.Session.Queries.GetNodeGroups());
            var resizedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            groupSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group",
                    StringComparison.Ordinal));

            Assert.Equal(initialGroup.ExtraPadding, group.ExtraPadding);
            Assert.True(resizedSnapshot.Size.Width > initialSnapshot.Size.Width + 30d);
            Assert.True(resizedSnapshot.Size.Height > initialSnapshot.Size.Height + 20d);
            Assert.InRange(groupSurface.Width, resizedSnapshot.Size.Width - 1d, resizedSnapshot.Size.Width + 1d);
            Assert.InRange(groupSurface.Height, resizedSnapshot.Size.Height - 1d, resizedSnapshot.Size.Height + 1d);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupSurfaceBorderPress_NearRightAndBottomEdges_ResizesFrame_InsteadOfSelectionOnly()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

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
            var rightEdgePress = WorldToScreenPoint(
                canvas,
                initialSnapshot.Position.X + groupSurface.Width - 4d,
                initialSnapshot.Position.Y + (groupSurface.Height / 2d));
            var rightMovedPoint = WorldToScreenPoint(
                canvas,
                initialSnapshot.Position.X + groupSurface.Width + 32d,
                initialSnapshot.Position.Y + (groupSurface.Height / 2d));
            var rightPressedArgs = CreatePointerPressedArgs(groupSurface, canvas, pointer, rightEdgePress, KeyModifiers.None);

            groupSurface.RaiseEvent(rightPressedArgs);
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, rightMovedPoint, KeyModifiers.None));

            var resizedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            Assert.True(rightPressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);
            Assert.True(resizedSnapshot.Size.Width > initialSnapshot.Size.Width + 30d);
            Assert.Equal(initialSnapshot.Position, resizedSnapshot.Position);

            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, rightMovedPoint, KeyModifiers.None));

            groupSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group",
                    StringComparison.Ordinal));
            var bottomEdgePress = WorldToScreenPoint(
                canvas,
                initialSnapshot.Position.X + (groupSurface.Width / 2d),
                initialSnapshot.Position.Y + groupSurface.Height - 4d);
            var bottomMovedPoint = WorldToScreenPoint(
                canvas,
                initialSnapshot.Position.X + (groupSurface.Width / 2d),
                initialSnapshot.Position.Y + groupSurface.Height + 24d);
            var bottomPressedArgs = CreatePointerPressedArgs(groupSurface, canvas, pointer, bottomEdgePress, KeyModifiers.None);

            groupSurface.RaiseEvent(bottomPressedArgs);
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, bottomMovedPoint, KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, bottomMovedPoint, KeyModifiers.None));

            resizedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            Assert.True(bottomPressedArgs.Handled);
            Assert.True(resizedSnapshot.Size.Width > initialSnapshot.Size.Width + 30d);
            Assert.True(resizedSnapshot.Size.Height > initialSnapshot.Size.Height + 20d);
            Assert.Equal(initialSnapshot.Position, resizedSnapshot.Position);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GroupResizeHandlePointerPress_CapturesCanvasForResizeSession()
    {
        var editor = CreateEditor();
        editor.Session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Canvas Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var (window, canvas) = CreateStandaloneCanvasWindow(editor);
        var pointer = new global::Avalonia.Input.Pointer(1, PointerType.Mouse, isPrimary: true);

        try
        {
            var thumb = canvas.GetVisualDescendants()
                .OfType<Thumb>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group right resize handle",
                    StringComparison.Ordinal));
            var pressedArgs = CreatePointerPressedArgs(thumb, canvas, pointer, new Point(652, 240), KeyModifiers.None);

            thumb.RaiseEvent(pressedArgs);

            Assert.True(pressedArgs.Handled);
            Assert.Same(canvas, pointer.Captured);
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
            var groupHeader = canvas.GetVisualDescendants()
                .OfType<Control>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group header",
                    StringComparison.Ordinal));
            var pressedPoint = WorldToScreenPoint(canvas, initialSnapshot.Position.X + 40d, initialSnapshot.Position.Y + 28d);
            var movedPoint = WorldToScreenPoint(canvas, initialSnapshot.Position.X + 90d, initialSnapshot.Position.Y + 53d);
            var pressedArgs = CreatePointerPressedArgs(groupHeader, canvas, pointer, pressedPoint, KeyModifiers.None);
            var movedArgs = CreatePointerMovedArgs(canvas, pointer, movedPoint, KeyModifiers.None);
            var releasedArgs = CreatePointerReleasedArgs(canvas, pointer, movedPoint, KeyModifiers.None);

            groupHeader.RaiseEvent(pressedArgs);
            Assert.Same(canvas, pointer.Captured);

            InvokeCanvasPointerMoved(canvas, movedArgs);

            var previewSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var groupSurface = canvas.GetVisualDescendants()
                .OfType<Border>()
                .Single(control => string.Equals(
                    AutomationProperties.GetName(control),
                    "Canvas Cluster group",
                    StringComparison.Ordinal));
            var previewSource = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var previewTarget = editor.Nodes.Single(node => node.Id == TargetNodeId);
            Assert.Equal(initialSource.X + 50d, previewSource.X);
            Assert.Equal(initialSource.Y + 25d, previewSource.Y);
            Assert.Equal(initialTarget.X + 50d, previewTarget.X);
            Assert.Equal(initialTarget.Y + 25d, previewTarget.Y);
            Assert.Equal(initialSnapshot.Position, previewSnapshot.Position);
            Assert.Equal(initialSnapshot.Position.X + 50d, Canvas.GetLeft(groupSurface));
            Assert.Equal(initialSnapshot.Position.Y + 25d, Canvas.GetTop(groupSurface));

            InvokeCanvasPointerReleased(canvas, releasedArgs);

            var movedSnapshot = Assert.Single(editor.GetNodeGroupSnapshots());
            var movedGroup = Assert.Single(editor.Session.Queries.GetNodeGroups());
            var movedSource = editor.Nodes.Single(node => node.Id == SourceNodeId);
            var movedTarget = editor.Nodes.Single(node => node.Id == TargetNodeId);
            Assert.Equal(new GraphPadding(36, 52, 30, 24), movedGroup.ExtraPadding);
            Assert.Equal(initialSource.X + 50d, movedSource.X);
            Assert.Equal(initialSource.Y + 25d, movedSource.Y);
            Assert.Equal(initialTarget.X + 50d, movedTarget.X);
            Assert.Equal(initialTarget.Y + 25d, movedTarget.Y);
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
    public void NodeDrag_DroppedInGroupHeader_DoesNotAttachToGroup()
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
            var headerDestination = new GraphPoint(
                initialGroup.ContentPosition.X - 12d,
                initialGroup.Position.Y + 8d);
            var attachStart = new Point(targetNode.X + 20d, targetNode.Y + 20d);
            var headerScreenDestination = new Point(
                attachStart.X + ((headerDestination.X - targetNode.X) * editor.Zoom),
                attachStart.Y + ((headerDestination.Y - targetNode.Y) * editor.Zoom));

            targetSurface.RaiseEvent(CreatePointerPressedArgs(targetSurface, canvas, pointer, attachStart, KeyModifiers.None));
            InvokeCanvasPointerMoved(canvas, CreatePointerMovedArgs(canvas, pointer, headerScreenDestination, KeyModifiers.None));
            InvokeCanvasPointerReleased(canvas, CreatePointerReleasedArgs(canvas, pointer, headerScreenDestination, KeyModifiers.None));

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

    [AvaloniaFact]
    public void NodeDrag_AttachesNodeToGroup_WhenDroppedInContentArea_AndDetaches_WhenDroppedOutside()
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
            var desiredAttachedX = initialGroup.ContentPosition.X + ((initialGroup.ContentSize.Width - targetNode.Width) / 2d);
            var desiredAttachedY = initialGroup.ContentPosition.Y + ((initialGroup.ContentSize.Height - targetNode.Height) / 2d);
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

    private static Point WorldToScreenPoint(NodeCanvas canvas, double worldX, double worldY)
    {
        var screenPoint = InvokeCanvasMethod<GraphPoint>("WorldToScreen", canvas, worldX, worldY);
        return new Point(screenPoint.X, screenPoint.Y);
    }

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
                        "#F3B36B"),
                ],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "gain",
                        "Gain",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Parameter value shown when the input is unconnected.",
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
                                new PortTypeId("float")),
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

    private static GraphEditorViewModel CreateAdaptiveSurfaceEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                AdaptiveDefinitionId,
                "Adaptive Surface",
                "Tests",
                "Standalone canvas node used to verify adaptive parameter rail thresholds.",
                [],
                [
                    new PortDefinition(
                        AdaptiveOutputPortId,
                        "Result",
                        new PortTypeId("float"),
                        "#6AD5C4"),
                ],
                parameters:
                [
                    new NodeParameterDefinition(
                        RequiredParameterKey,
                        "Required Input",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        isRequired: true,
                        description: "Required parameter that should remain visible at the baseline size."),
                    new NodeParameterDefinition(
                        OptionalParameterKey,
                        "Optional Gain",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Optional parameter that should appear after the node is expanded.",
                        defaultValue: 0.5d),
                ]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Adaptive Surface Graph",
                "Regression coverage for adaptive node sizing and parameter disclosure.",
                [
                    new GraphNode(
                        AdaptiveNodeId,
                        "Adaptive Surface",
                        "Tests",
                        "Adaptive Canvas",
                        "Verifies baseline required inputs and progressive optional parameter disclosure.",
                        new GraphPoint(180, 180),
                        new GraphSize(300, 180),
                        [],
                        [
                            new GraphPort(
                                AdaptiveOutputPortId,
                                "Result",
                                PortDirection.Output,
                                "float",
                                "#6AD5C4",
                                new PortTypeId("float")),
                        ],
                        "#6AD5C4",
                        AdaptiveDefinitionId,
                        [
                            new GraphParameterValue(OptionalParameterKey, new PortTypeId("float"), 0.5d),
                        ]),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            styleOptions: GraphEditorStyleOptions.Default);
    }

    private static GraphEditorViewModel CreateOutputEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                OutputDefinitionId,
                "Viewport Terminal",
                "Output",
                "Final Preview",
                [
                    new PortDefinition(
                        OutputInputPortId,
                        "Viewport",
                        new PortTypeId("color"),
                        "#FFF1AA"),
                ],
                [],
                description: "Terminal output node used to verify dedicated output chrome."));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Output Canvas Graph",
                "Regression coverage for output-node emphasis.",
                [
                    new GraphNode(
                        OutputNodeId,
                        "Viewport Terminal",
                        "Output",
                        "Final Preview",
                        "Terminal output node used to verify dedicated output chrome.",
                        new GraphPoint(220, 180),
                        new GraphSize(240, 160),
                        [
                            new GraphPort(
                                OutputInputPortId,
                                "Viewport",
                                PortDirection.Input,
                                "color",
                                "#FFF1AA",
                                new PortTypeId("color")),
                        ],
                        [],
                        "#FFF1AA",
                        OutputDefinitionId),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            styleOptions: GraphEditorStyleOptions.Default);
    }

    private static void AssertCursor(StandardCursorType expected, Cursor? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.ToString(), actual!.ToString());
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

    private sealed class RecordingResizeFeedbackPolicy : IGraphResizeFeedbackPolicy
    {
        private readonly Cursor _cursor;

        public RecordingResizeFeedbackPolicy(Cursor cursor)
        {
            _cursor = cursor;
        }

        public List<GraphResizeFeedbackContext> Contexts { get; } = [];

        public Cursor? ResolveCursor(GraphResizeFeedbackContext context)
        {
            Contexts.Add(context);
            return _cursor;
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
