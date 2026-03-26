using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
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
            Assert.Same(editor, canvas.ViewModel);
            Assert.True(canvas.EnableDefaultContextMenu);
            Assert.True(canvas.EnableDefaultCommandShortcuts);
            Assert.True(canvas.Focusable);
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
        var (enabledWindow, enabledCanvas) = CreateStandaloneCanvasWindow(enabledEditor, enableDefaultContextMenu: true);
        var enabledArgs = new ContextRequestedEventArgs();
        try
        {
            InvokeCanvasContextRequested(enabledCanvas, enabledArgs);

            Assert.True(enabledArgs.Handled);

            var disabledEditor = CreateEditor();
            var (disabledWindow, disabledCanvas) = CreateStandaloneCanvasWindow(disabledEditor, enableDefaultContextMenu: false);
            var disabledArgs = new ContextRequestedEventArgs();
            try
            {
                InvokeCanvasContextRequested(disabledCanvas, disabledArgs);

                Assert.False(disabledArgs.Handled);
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

    private static void InvokeCanvasContextRequested(NodeCanvas canvas, ContextRequestedEventArgs args)
        => InvokeCanvasHandler("HandleCanvasContextRequested", canvas, args);

    private static void InvokeCanvasKeyDown(NodeCanvas canvas, KeyEventArgs args)
        => InvokeCanvasHandler("HandleCanvasKeyDown", canvas, args);

    private static void InvokeCanvasHandler(string methodName, NodeCanvas canvas, object args)
    {
        var method = typeof(NodeCanvas).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Could not find NodeCanvas handler '{methodName}'.");
        method.Invoke(canvas, [canvas, args]);
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
        bool enableDefaultCommandShortcuts = true)
    {
        var canvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = editor,
            EnableDefaultContextMenu = enableDefaultContextMenu,
            EnableDefaultCommandShortcuts = enableDefaultCommandShortcuts,
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
}
