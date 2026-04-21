using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSurfaceCompositionTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.surface.node");

    [AvaloniaFact]
    public void FullShell_ComposesStandaloneSurfacesAgainstSameEditor()
    {
        var editor = CreateEditor();
        using var scope = CreateWindowScope(
            AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
            {
                Editor = editor,
            }));
        var view = Assert.IsType<GraphEditorView>(scope.Window.Content);
        var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");
        var inspector = FindRequiredControl<GraphInspectorView>(view, "PART_InspectorSurface");
        var miniMap = FindRequiredControl<GraphMiniMap>(view, "PART_MiniMapSurface");

        Assert.Same(editor, view.Editor);
        Assert.Same(editor, canvas.ViewModel);
        Assert.Same(editor, inspector.Editor);
        Assert.Same(editor, miniMap.ViewModel);
    }

    [AvaloniaFact]
    public void StandaloneComposition_ReusesSameEditorStateWithoutShellChrome()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,*,Auto"),
            ColumnSpacing = 16,
            Children =
            {
                AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions { Editor = editor }),
                AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions { Editor = editor }),
                AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions { Editor = editor }),
            },
        };
        Grid.SetColumn(layout.Children[1], 1);
        Grid.SetColumn(layout.Children[2], 2);

        using var scope = CreateWindowScope(layout);
        var canvas = Assert.IsType<NodeCanvas>(layout.Children[0]);
        var inspector = Assert.IsType<GraphInspectorView>(layout.Children[1]);
        var miniMap = Assert.IsType<GraphMiniMap>(layout.Children[2]);
        var inspectorText = string.Join(
            "\n",
            inspector.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(block => block.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text)));

        Assert.Same(canvas.ViewModel, inspector.Editor);
        Assert.Same(canvas.ViewModel, miniMap.ViewModel);
        Assert.Contains("Surface Composition Node", inspectorText);
    }

    [AvaloniaFact]
    public void StandaloneCanvasOptOut_StillKeepsMenuAndSessionBackedStateAvailable()
    {
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
        var canvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = editor,
            EnableDefaultContextMenu = false,
            CommandShortcutPolicy = AsterGraphCommandShortcutPolicy.Disabled,
        });
        var menu = editor.BuildContextMenu(
            new ContextMenuContext(
                ContextMenuTargetKind.Canvas,
                new GraphPoint(120, 160)));

        Assert.False(canvas.EnableDefaultContextMenu);
        Assert.False(canvas.CommandShortcutPolicy.Enabled);
        Assert.NotNull(editor.Session);
        Assert.NotEmpty(menu);
    }

    [AvaloniaFact]
    public void FullShell_ForwardsCustomInspectorAndMiniMapPresenters()
    {
        var editor = CreateEditor();
        var customInspector = new RecordingInspectorPresenter();
        var customMiniMap = new RecordingMiniMapPresenter();

        using var scope = CreateWindowScope(
            AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
            {
                Editor = editor,
                Presentation = new AsterGraphPresentationOptions
                {
                    InspectorPresenter = customInspector,
                    MiniMapPresenter = customMiniMap,
                },
            }));
        var view = Assert.IsType<GraphEditorView>(scope.Window.Content);
        var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");
        var allText = string.Join(
            "\n",
            view.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(block => block.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text)));

        Assert.Same(editor, canvas.ViewModel);
        Assert.Contains("CUSTOM SHELL INSPECTOR", allText);
        Assert.Contains("CUSTOM SHELL MINIMAP", allText);
        Assert.Same(editor, customInspector.LastEditor);
        Assert.Same(editor, customMiniMap.LastEditor);
    }

    private static WindowScope CreateWindowScope(Control content)
    {
        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = content,
        };
        window.Show();
        return new WindowScope(window);
    }

    private static T FindRequiredControl<T>(Control root, string name)
        where T : Control
        => root.FindControl<T>(name) ?? throw new Xunit.Sdk.XunitException($"Could not find control '{name}'.");

    private static GraphEditorViewModel CreateEditor()
        => AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static GraphDocument CreateDocument()
        => new(
            "Surface Composition Graph",
            "Regression coverage for GraphEditorView composition.",
            [
                new GraphNode(
                    "tests.surface.node-001",
                    "Surface Composition Node",
                    "Tests",
                    "Composition",
                    "Used to verify full shell and standalone surface composition.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [],
                    "#6AD5C4",
                    DefinitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                DefinitionId,
                "Surface Composition Node",
                "Tests",
                "Composition",
                [],
                []));
        return catalog;
    }

    private sealed class WindowScope(Window window) : IDisposable
    {
        public Window Window { get; } = window;

        public void Dispose()
            => Window.Close();
    }

    private sealed class RecordingInspectorPresenter : IGraphInspectorPresenter
    {
        public GraphEditorViewModel? LastEditor { get; private set; }

        public Control Create(GraphEditorViewModel? editor)
        {
            LastEditor = editor;
            return new TextBlock
            {
                Text = "CUSTOM SHELL INSPECTOR",
            };
        }
    }

    private sealed class RecordingMiniMapPresenter : IGraphMiniMapPresenter
    {
        public GraphEditorViewModel? LastEditor { get; private set; }

        public Control Create(GraphEditorViewModel? editor)
        {
            LastEditor = editor;
            return new TextBlock
            {
                Text = "CUSTOM SHELL MINIMAP",
            };
        }
    }
}
