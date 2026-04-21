using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor;
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
    public void HeaderCommandSurface_UsesSharedDescriptorsForToolbarAndPalette()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var saveButton = FindRequiredDescendant<Button>(view, "PART_HeaderCommand_workspace.save");
        var undoButton = FindRequiredDescendant<Button>(view, "PART_HeaderCommand_history.undo");
        var paletteToggle = FindRequiredControl<Button>(view, "PART_OpenCommandPaletteButton");

        Assert.Equal("Save Workspace", Assert.IsType<string>(saveButton.Content));
        Assert.Equal("Undo", Assert.IsType<string>(undoButton.Content));

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");
        var paletteItems = FindRequiredControl<StackPanel>(view, "PART_CommandPaletteItems");

        Assert.True(paletteChrome.IsVisible);
        Assert.Contains(
            paletteItems.Children.OfType<Button>(),
            button => string.Equals(button.Name, "PART_CommandPaletteAction_workspace.save", StringComparison.Ordinal));
        Assert.Contains(
            paletteItems.Children.OfType<Button>(),
            button => string.Equals(button.Name, "PART_CommandPaletteAction_history.undo", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void ShortcutHelp_AndKeyboardRouting_ProjectCommandPaletteFromSharedActionSource()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var shortcutHelp = FindRequiredControl<StackPanel>(view, "PART_ShortcutHelpList");
        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");

        Assert.Contains(
            shortcutHelp.Children.OfType<Border>()
                .Select(item => item.Child)
                .OfType<TextBlock>(),
            item => item.Text?.Contains("Ctrl+Shift+P", StringComparison.Ordinal) == true
                && item.Text.Contains("Command Palette", StringComparison.Ordinal));

        var args = new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
        };

        InvokeViewKeyDown(view, args);

        Assert.True(args.Handled);
        Assert.True(paletteChrome.IsVisible);
    }

    [AvaloniaFact]
    public void CommandShortcutPolicy_OverridesCommandPaletteShortcutAndShortcutHelp()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            CommandShortcutPolicy = new AsterGraphCommandShortcutPolicy
            {
                ShortcutOverrides = new Dictionary<string, string?>
                {
                    ["shell.command-palette"] = "Ctrl+Alt+P",
                },
            },
        });
        var view = (GraphEditorView)window.Content!;
        var shortcutHelp = FindRequiredControl<StackPanel>(view, "PART_ShortcutHelpList");
        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");

        Assert.Contains(
            shortcutHelp.Children.OfType<Border>()
                .Select(item => item.Child)
                .OfType<TextBlock>(),
            item => item.Text?.Contains("Ctrl+Alt+P", StringComparison.Ordinal) == true
                && item.Text.Contains("Command Palette", StringComparison.Ordinal));

        var defaultArgs = new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
        };

        InvokeViewKeyDown(view, defaultArgs);

        Assert.False(defaultArgs.Handled);
        Assert.False(paletteChrome.IsVisible);

        var overrideArgs = new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Alt,
        };

        InvokeViewKeyDown(view, overrideArgs);

        Assert.True(overrideArgs.Handled);
        Assert.True(paletteChrome.IsVisible);
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

    [AvaloniaFact]
    public void DirectGraphEditorView_WithFactoryCreatedEditor_PreservesCompatibilitySurfaceBehavior()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.view.factory"),
                "Factory View Node",
                "Tests",
                "Exercises direct GraphEditorView with a factory-created editor.",
                [],
                []));
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Factory View Graph",
                "Exercises direct GraphEditorView behavior with a factory-created editor.",
                [
                    new GraphNode(
                        "tests.view.factory-001",
                        "Factory View Node",
                        "Tests",
                        "GraphEditorView",
                        "Used by GraphEditorView factory-created editor tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.view.factory")),
                ],
                []),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var view = (GraphEditorView)window.Content!;
        var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");

        Assert.Same(editor, view.Editor);
        Assert.False(canvas.AttachPlatformSeams);
        Assert.True(canvas.EnableDefaultContextMenu);
        Assert.True(canvas.CommandShortcutPolicy.Enabled);
        Assert.NotNull(editor.HostContext);
        Assert.True(editor.HostContext!.TryGetOwner<GraphEditorView>(out var owner));
        Assert.Same(view, owner);
        Assert.True(editor.HostContext.TryGetTopLevel<Window>(out var topLevel));
        Assert.Same(window, topLevel);
    }

    [AvaloniaFact]
    public void StencilLibrary_UsesSessionStencilItems_WhenRetainedTemplatesAreCleared()
    {
        var editor = CreateEditor();
        editor.NodeTemplates.Clear();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var stencilCard = FindRequiredDescendant<Button>(view, "PART_StencilCard_tests-view-node");

        Assert.True(stencilCard.IsEnabled);

        stencilCard.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(2, editor.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    [AvaloniaFact]
    public void NodeCanvasContextMenuSnapshot_UsesSessionDefinitions_WhenRetainedTemplatesAreCleared()
    {
        var editor = CreateEditor();
        editor.NodeTemplates.Clear();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");

        var snapshot = InvokeNodeCanvasMethod("CreateContextMenuSnapshot", canvas);
        var definitions = Assert.IsAssignableFrom<IReadOnlyList<INodeDefinition>>(
            snapshot.GetType().GetProperty("AvailableNodeDefinitions")!.GetValue(snapshot));

        Assert.Contains(definitions, definition => definition.Id == new NodeDefinitionId("tests.view.node"));
    }

    [AvaloniaFact]
    public void FragmentLibrary_UsesSessionSnapshots_WhenRetainedTemplatesAreCleared()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "astergraph-view-fragment-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "View Fragment Graph",
                "Exercises session-backed fragment library UI.",
                [
                    new GraphNode(
                        "tests.view.fragment-node-001",
                        "Fragment View Node",
                        "Tests",
                        "GraphEditorView",
                        "Source node for fragment workflow tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.view.fragment-node")),
                ],
                []),
            NodeCatalog = CreateFragmentCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = storageRoot,
        });
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var templatePath = editor.Session.Commands.TryExportSelectionAsTemplate("View Fragment Template");
        editor.FragmentTemplates.Clear();

        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var templatePicker = FindRequiredControl<ComboBox>(view, "PART_FragmentTemplatePicker");
        var importButton = FindRequiredDescendant<Button>(view, "PART_FragmentTemplateImportButton");

        Assert.Equal(1, templatePicker.ItemCount);

        templatePicker.SelectedIndex = 0;
        importButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(2, editor.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
        Assert.Equal(templatePath, Assert.Single(editor.Session.Queries.GetFragmentTemplateSnapshots()).Path);
    }

    [AvaloniaFact]
    public void CompositeWorkflowChrome_WrapSelectionActionCreatesCompositeShell()
    {
        var editor = CreateSelectionEditor();
        editor.Session.Commands.SetSelection(["tests.view.source-001", "tests.view.target-001"], "tests.view.target-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var wrapSelection = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_composites.wrap-selection");

        Assert.Equal("Wrap Selection To Composite", Assert.IsType<string>(wrapSelection.Content));
        Assert.True(wrapSelection.IsEnabled);

        wrapSelection.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var compositeSnapshot = Assert.Single(editor.Session.Queries.GetCompositeNodeSnapshots());
        var selection = editor.Session.Queries.GetSelectionSnapshot();
        var enterScope = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_scopes.enter");

        Assert.Equal([compositeSnapshot.NodeId], selection.SelectedNodeIds);
        Assert.True(enterScope.IsEnabled);
    }

    [AvaloniaFact]
    public void CompositeWorkflowChrome_ProjectsBreadcrumbsAndScopeNavigation()
    {
        var editor = CreateScopedEditor();
        editor.Session.Commands.SetSelection(["tests.view.composite-001"], "tests.view.composite-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var enterScope = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_scopes.enter");
        Assert.True(enterScope.IsEnabled);

        enterScope.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal("graph-child-001", editor.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId);

        var rootBreadcrumb = FindRequiredDescendant<Button>(view, "PART_ScopeBreadcrumb_graph-root");
        var childBreadcrumb = FindRequiredDescendant<Button>(view, "PART_ScopeBreadcrumb_graph-child-001");
        var exitScope = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_scopes.exit");

        Assert.True(rootBreadcrumb.IsEnabled);
        Assert.False(childBreadcrumb.IsEnabled);
        Assert.True(exitScope.IsEnabled);

        rootBreadcrumb.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal("graph-root", editor.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId);
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

    private static T FindRequiredDescendant<T>(Control root, string name)
        where T : Control
        => root.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal))
            ?? throw new Xunit.Sdk.XunitException($"Could not find descendant control '{name}'.");

    private static void InvokeViewKeyDown(GraphEditorView view, KeyEventArgs args)
    {
        var handler = typeof(GraphEditorView).GetMethod(
            "HandleKeyDown",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(handler);
        handler.Invoke(view, [view, args]);
    }

    private static object InvokeNodeCanvasMethod(string methodName, NodeCanvas canvas)
    {
        var method = typeof(NodeCanvas).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return method!.Invoke(canvas, [])!;
    }

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

    private static INodeCatalog CreateFragmentCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.view.fragment-node"),
                "Fragment View Node",
                "Tests",
                "Exercises session-backed fragment template surfaces.",
                [],
                []));
        return catalog;
    }

    private static GraphEditorViewModel CreateSelectionEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.selection");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Selection View Node",
                "Tests",
                "Exercises composite workflow actions.",
                [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Selection Workflow Graph",
                "Exercises composite workflow shell actions.",
                [
                    new GraphNode(
                        "tests.view.source-001",
                        "View Source",
                        "Tests",
                        "GraphEditorView",
                        "Source node for composite workflow tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        "tests.view.target-001",
                        "View Target",
                        "Tests",
                        "GraphEditorView",
                        "Target node for composite workflow tests.",
                        new GraphPoint(520, 180),
                        new GraphSize(240, 160),
                        [new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private static GraphEditorViewModel CreateScopedEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.scoped");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Scoped View Node",
                "Tests",
                "Exercises scope workflow actions.",
                [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));

        return AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = GraphDocument.CreateScoped(
                "Scoped View Graph",
                "Exercises composite scope navigation chrome.",
                "graph-root",
                [
                    new GraphScope(
                        "graph-root",
                        [
                            new GraphNode(
                                "tests.view.composite-001",
                                "Composite View Node",
                                "Tests",
                                "GraphEditorView",
                                "Composite shell node for scope navigation tests.",
                                new GraphPoint(160, 140),
                                new GraphSize(260, 180),
                                [],
                                [],
                                "#A67CF5",
                                null,
                                [],
                                null,
                                new GraphCompositeNode("graph-child-001", [], [])),
                        ],
                        []),
                    new GraphScope(
                        "graph-child-001",
                        [
                            new GraphNode(
                                "tests.view.child-source-001",
                                "Child Source",
                                "Tests",
                                "GraphEditorView",
                                "Child source node.",
                                new GraphPoint(80, 100),
                                new GraphSize(220, 150),
                                [],
                                [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                                "#6AD5C4",
                                definitionId),
                        ],
                        []),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
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
