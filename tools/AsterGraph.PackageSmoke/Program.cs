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

var definitionId = new NodeDefinitionId("smoke.node");
var catalog = new NodeCatalog();
catalog.RegisterDefinition(new NodeDefinition(
    definitionId,
    "Smoke Node",
    "Smoke",
    "Migration",
    [],
    []));

var document = new GraphDocument(
    "Package Smoke Graph",
    "Migration-stage smoke validation.",
    [
        new GraphNode(
            "smoke-node-001",
            "Smoke Node",
            "Smoke",
            "Migration",
            "Verifies both legacy and factory host paths.",
            new GraphPoint(64, 96),
            new GraphSize(220, 160),
            [],
            [],
            "#6AD5C4",
            definitionId),
    ],
    []);

var root = Path.Combine(
    Path.GetTempPath(),
    "astergraph-package-smoke",
    Guid.NewGuid().ToString("N"));
var workspaceService = new GraphWorkspaceService(Path.Combine(root, "workspace.json"));
var fragmentWorkspaceService = new GraphFragmentWorkspaceService(Path.Combine(root, "fragment.json"));
var fragmentLibraryService = new GraphFragmentLibraryService(Path.Combine(root, "fragments"));
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
        ShowMiniMap = false,
    },
};
var menuAugmentor = new SmokeContextMenuAugmentor();
var presentationProvider = new SmokeNodePresentationProvider();
var localizationProvider = new SmokeLocalizationProvider();
var compatibilityService = new DefaultPortCompatibilityService();

var legacyEditor = new GraphEditorViewModel(
    document,
    catalog,
    compatibilityService,
    workspaceService,
    fragmentWorkspaceService,
    styleOptions,
    behaviorOptions,
    fragmentLibraryService,
    menuAugmentor,
    presentationProvider,
    localizationProvider);
var legacyView = new GraphEditorView
{
    Editor = legacyEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
};

var factoryEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    WorkspaceService = workspaceService,
    FragmentWorkspaceService = fragmentWorkspaceService,
    StyleOptions = styleOptions,
    BehaviorOptions = behaviorOptions,
    FragmentLibraryService = fragmentLibraryService,
    ContextMenuAugmentor = menuAugmentor,
    NodePresentationProvider = presentationProvider,
    LocalizationProvider = localizationProvider,
});
var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = factoryEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
});

PrintEditorMarker("LEGACY_EDITOR_OK", legacyEditor);
PrintViewMarker("LEGACY_VIEW_OK", legacyView);
PrintEditorMarker("FACTORY_EDITOR_OK", factoryEditor);
PrintViewMarker("FACTORY_VIEW_OK", factoryView);

static void PrintEditorMarker(string marker, GraphEditorViewModel editor)
{
    var menu = editor.BuildContextMenu(new ContextMenuContext(
        ContextMenuTargetKind.Canvas,
        new GraphPoint(32, 48)));
    var node = editor.Nodes.Single();

    if (node.DisplaySubtitle != "Smoke subtitle")
    {
        throw new InvalidOperationException($"Unexpected presentation state for {marker}.");
    }

    if (!menu.Any(item => item.Id == "smoke.host-action"))
    {
        throw new InvalidOperationException($"Missing host menu augmentor for {marker}.");
    }

    Console.WriteLine($"{marker}:{editor.Title}:{editor.StatsCaption}:{node.DisplaySubtitle}:{editor.StyleOptions.Shell.HighlightHex}");
}

static void PrintViewMarker(string marker, GraphEditorView view)
{
    if (view.Editor is null)
    {
        throw new InvalidOperationException($"View binding was not assigned for {marker}.");
    }

    Console.WriteLine($"{marker}:{view.ChromeMode}:{view.Editor.Title}");
}

sealed class SmokeContextMenuAugmentor : IGraphContextMenuAugmentor
{
    public IReadOnlyList<MenuItemDescriptor> Augment(
        GraphEditorViewModel editor,
        ContextMenuContext context,
        IReadOnlyList<MenuItemDescriptor> stockItems)
        => [.. stockItems, new MenuItemDescriptor("smoke.host-action", "Smoke Host Action")];
}

sealed class SmokeNodePresentationProvider : INodePresentationProvider
{
    public NodePresentationState GetNodePresentation(NodeViewModel node)
        => new(SubtitleOverride: "Smoke subtitle");
}

sealed class SmokeLocalizationProvider : IGraphLocalizationProvider
{
    public string GetString(string key, string fallback)
        => key == "editor.stats.caption"
            ? "Smoke stats {0}/{1}/{2:0}"
            : fallback;
}
