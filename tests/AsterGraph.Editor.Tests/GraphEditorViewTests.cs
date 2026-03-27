using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(AsterGraph.Editor.Tests.GraphEditorViewTestsAppBuilder))]

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorViewTests
{
    [AvaloniaFact]
    public void DefaultChromeMode_KeepsAllChromeSectionsVisible()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        Assert.Equal(GraphEditorViewChromeMode.Default, view.ChromeMode);
        Assert.True(view.IsHeaderChromeVisible);
        Assert.True(view.IsLibraryChromeVisible);
        Assert.True(view.IsInspectorChromeVisible);
        Assert.True(view.IsStatusChromeVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
        Assert.True(FindRequiredControl<Grid>(view, "PART_ShellGrid").ColumnSpacing > 0);
        Assert.True(FindRequiredControl<Grid>(view, "PART_ShellGrid").RowSpacing > 0);
    }

    [AvaloniaFact]
    public void DefaultChromeMode_UsesSeparatedHeaderBadgesAndWrappingToolbar()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var header = FindRequiredControl<Border>(view, "PART_HeaderChrome");
        var badgeStack = FindRequiredControl<StackPanel>(view, "PART_HeaderBadges");
        var toolbar = FindRequiredControl<WrapPanel>(view, "PART_HeaderToolbar");

        Assert.Equal(new Thickness(20), header.Padding);
        Assert.Equal(Orientation.Vertical, badgeStack.Orientation);
        Assert.Equal(Orientation.Horizontal, toolbar.Orientation);
        Assert.Equal(40, toolbar.ItemHeight);
        Assert.Equal(120, toolbar.ItemWidth);
        Assert.True(toolbar.Children.Count >= 7);
    }

    [AvaloniaFact]
    public void IndividualChromeVisibility_TogglesEachRegionIndependently()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            IsHeaderChromeVisible = false,
            IsLibraryChromeVisible = true,
            IsInspectorChromeVisible = false,
            IsStatusChromeVisible = true,
        });
        var view = (GraphEditorView)window.Content!;

        Assert.False(view.IsHeaderChromeVisible);
        Assert.True(view.IsLibraryChromeVisible);
        Assert.False(view.IsInspectorChromeVisible);
        Assert.True(view.IsStatusChromeVisible);

        Assert.False(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
        Assert.NotNull(FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas"));
    }

    [AvaloniaFact]
    public void CanvasOnlyChromeMode_HidesAllShellChromeWithoutLeavingShellSpacing()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var view = (GraphEditorView)window.Content!;
        var shellGrid = FindRequiredControl<Grid>(view, "PART_ShellGrid");

        Assert.False(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
        Assert.Equal(0, shellGrid.ColumnSpacing);
        Assert.Equal(0, shellGrid.RowSpacing);
    }

    [AvaloniaFact]
    public void SwitchingToCanvasOnly_UpdatesVisibilityImmediatelyWithoutRebuildingEditorState()
    {
        var editor = CreateEditor();
        editor.AddNodeToSelection(editor.Nodes[0], updateStatus: false);
        var originalZoom = editor.Zoom;

        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        view.ChromeMode = GraphEditorViewChromeMode.CanvasOnly;

        Assert.Same(editor, view.Editor);
        Assert.Same(editor.Nodes[0], editor.SelectedNode);
        Assert.Equal(originalZoom, editor.Zoom);
        Assert.False(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
    }

    [AvaloniaFact]
    public void CanvasOnly_KeepsMenuChainAndHostContextAvailable()
    {
        var augmentor = new GraphEditorViewHostAwareAugmentor();
        var editor = CreateEditor(augmentor);

        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var view = (GraphEditorView)window.Content!;

        Assert.NotNull(editor.HostContext);
        Assert.True(editor.HostContext!.TryGetOwner<GraphEditorView>(out var owner));
        Assert.Same(view, owner);
        Assert.True(editor.HostContext.TryGetTopLevel<Window>(out var topLevel));
        Assert.Same(window, topLevel);

        var menu = editor.BuildContextMenu(
            new ContextMenuContext(
                ContextMenuTargetKind.Node,
                new GraphPoint(120, 160),
                selectedNodeId: "tests.view.node-001",
                selectedNodeIds: ["tests.view.node-001"],
                clickedNodeId: "tests.view.node-001",
                hostContext: editor.HostContext));
        var hostItem = Assert.Single(menu, item => item.Id == "tests-view-host-item");

        Assert.Equal("Host Item", hostItem.Header);
        Assert.True(augmentor.ReceivedHostOwner);
    }

    private static Window CreateWindow(GraphEditorView view)
    {
        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = view,
        };
        window.Show();
        return window;
    }

    private static T FindRequiredControl<T>(Control root, string name)
        where T : Control
        => root.FindControl<T>(name) ?? throw new Xunit.Sdk.XunitException($"Could not find control '{name}'.");

    private static GraphEditorViewModel CreateEditor(IGraphContextMenuAugmentor? augmentor = null)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.view.node"),
                "View Test Node",
                "Tests",
                "Exercises GraphEditorView chrome switching.",
                [],
                []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "View Test Graph",
                "Exercises GraphEditorView shell chrome.",
                [
                    new GraphNode(
                        "tests.view.node-001",
                        "View Node",
                        "Tests",
                        "GraphEditorView",
                        "Used by GraphEditorView tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.view.node")),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            contextMenuAugmentor: augmentor);
    }

    private sealed class GraphEditorViewHostAwareAugmentor : IGraphContextMenuAugmentor
    {
        public bool ReceivedHostOwner { get; private set; }

        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
        {
            ReceivedHostOwner = context.HostContext.TryGetOwner<GraphEditorView>(out _);
            return
            [
                .. stockItems,
                new MenuItemDescriptor("tests-view-host-item", "Host Item", null),
            ];
        }
    }
}

public sealed class GraphEditorViewTestsAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<GraphEditorViewTestsApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public sealed class GraphEditorViewTestsApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
