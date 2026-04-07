using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
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

    private static void InvokeCanvasContextRequested(NodeCanvas canvas, ContextRequestedEventArgs args)
        => InvokeCanvasHandler("HandleCanvasContextRequested", canvas, args);

    private static void InvokeCanvasKeyDown(NodeCanvas canvas, KeyEventArgs args)
        => InvokeCanvasHandler("HandleCanvasKeyDown", canvas, args);

    private static void InvokeCanvasPointerPressed(NodeCanvas canvas, PointerPressedEventArgs args)
        => InvokeCanvasHandler("HandlePointerPressed", canvas, args);

    private static void InvokeCanvasWheelChanged(NodeCanvas canvas, PointerWheelEventArgs args)
        => InvokeCanvasHandler("HandlePointerWheelChanged", canvas, args);

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
                []));

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
                        TargetDefinitionId),
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
        public GraphNodeVisual Create(GraphNodeVisualContext context)
        {
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
            visual.Root.Width = context.Node.Width;
            visual.Root.Height = context.Node.Height;
        }
    }
}
