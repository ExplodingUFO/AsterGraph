using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorInitializationTests
{
    [Fact]
    public void CreateEditorFactory_ValidatesRequiredInputs()
    {
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.Create(null!));

        var nullDocument = new AsterGraphEditorOptions
        {
            NodeCatalog = new NodeCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.Create(nullDocument));

        var nullCatalog = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.Create(nullCatalog));

        var nullCompatibility = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = new NodeCatalog(),
        };
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.Create(nullCompatibility));
    }

    [Fact]
    public void CreateEditorFactory_ForwardsCompositionSeamsUnchanged()
    {
        var workspaceService = new GraphWorkspaceService(Path.Combine(Path.GetTempPath(), "graph-editor-init-tests", "workspace.json"));
        var fragmentWorkspaceService = new GraphFragmentWorkspaceService(Path.Combine(Path.GetTempPath(), "graph-editor-init-tests", "fragment.json"));
        var fragmentLibraryService = new GraphFragmentLibraryService(Path.Combine(Path.GetTempPath(), "graph-editor-init-tests", "fragments"));
        var styleOptions = GraphEditorStyleOptions.Default with
        {
            Shell = GraphEditorStyleOptions.Default.Shell with
            {
                HighlightHex = "#F3B36B",
            },
        };
        var behaviorOptions = GraphEditorBehaviorOptions.Default with
        {
            View = GraphEditorBehaviorOptions.Default.View with
            {
                ZoomStep = 0.15,
            },
        };
        var menuAugmentor = new TestContextMenuAugmentor();
        var presentationProvider = new TestNodePresentationProvider();
        var localizationProvider = new TestLocalizationProvider();

        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            WorkspaceService = workspaceService,
            FragmentWorkspaceService = fragmentWorkspaceService,
            StyleOptions = styleOptions,
            BehaviorOptions = behaviorOptions,
            FragmentLibraryService = fragmentLibraryService,
            ContextMenuAugmentor = menuAugmentor,
            NodePresentationProvider = presentationProvider,
            LocalizationProvider = localizationProvider,
        });

        Assert.Same(styleOptions, editor.StyleOptions);
        Assert.Same(behaviorOptions, editor.BehaviorOptions);
        Assert.Same(menuAugmentor, editor.ContextMenuAugmentor);
        Assert.Same(presentationProvider, editor.NodePresentationProvider);
        Assert.Same(localizationProvider, editor.LocalizationProvider);
        Assert.Equal(workspaceService.WorkspacePath, editor.WorkspacePath);
        Assert.Equal(fragmentWorkspaceService.FragmentPath, editor.FragmentPath);
        Assert.Equal(fragmentLibraryService.LibraryPath, editor.FragmentLibraryPath);
        Assert.Equal("#F3B36B", editor.StyleOptions.Shell.HighlightHex);
        Assert.Equal(0.15, editor.BehaviorOptions.View.ZoomStep);
    }

    [Fact]
    public void CreateAvaloniaViewFactory_ValidatesRequiredInputs()
    {
        Assert.Throws<ArgumentNullException>(() => AsterGraphAvaloniaViewFactory.Create(null!));

        var options = new AsterGraphAvaloniaViewOptions
        {
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };

        Assert.Throws<ArgumentNullException>(() => AsterGraphAvaloniaViewFactory.Create(options));
    }

    [Fact]
    public void DefaultGraphEditorViewComposition_PreservesExpectedChromeBehavior()
    {
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });

        Assert.Same(editor, view.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, view.ChromeMode);
    }

    private static GraphDocument CreateDocument()
        => new(
            "Initialization Test Graph",
            "Regression coverage for host-facing initialization factories.",
            [
                new GraphNode(
                    "tests.init.node-001",
                    "Initialization Node",
                    "Tests",
                    "Initialization",
                    "Used to verify host-facing initialization flows.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [],
                    "#6AD5C4",
                    new NodeDefinitionId("tests.init.node")),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.init.node"),
                "Initialization Node",
                "Tests",
                "Initialization",
                [],
                []));
        return catalog;
    }

    private sealed class TestContextMenuAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => stockItems;
    }

    private sealed class TestNodePresentationProvider : INodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(NodeViewModel node)
            => NodePresentationState.Empty;
    }

    private sealed class TestLocalizationProvider : IGraphLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => fallback;
    }
}
