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

public sealed class GraphEditorMigrationCompatibilityTests
{
    [Fact]
    public void LegacyGraphEditorViewModelConstructor_RemainsSupportedAlongsideNewInitializationApis()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);

        Assert.Same(harness.StyleOptions, legacyEditor.StyleOptions);
        Assert.Same(harness.BehaviorOptions, legacyEditor.BehaviorOptions);
        Assert.Same(harness.MenuAugmentor, legacyEditor.ContextMenuAugmentor);
        Assert.Same(harness.PresentationProvider, legacyEditor.NodePresentationProvider);
        Assert.Same(harness.LocalizationProvider, legacyEditor.LocalizationProvider);
        Assert.Equal(harness.WorkspaceService.WorkspacePath, legacyEditor.WorkspacePath);
        Assert.Equal(harness.FragmentWorkspaceService.FragmentPath, legacyEditor.FragmentPath);
        Assert.Equal(harness.FragmentLibraryService.LibraryPath, legacyEditor.FragmentLibraryPath);

        var snapshot = CaptureSnapshot(legacyEditor);
        Assert.Equal("Migration subtitle", snapshot.NodeSubtitle);
        Assert.Equal("Migration description", snapshot.NodeDescription);
        Assert.Equal("Localized stats 1/0/88", snapshot.StatsCaption);
        Assert.Contains("tests.migration.host-action", snapshot.MenuIds);
    }

    [Fact]
    public void NewInitializationApis_CreateEditorStateEquivalentToLegacyConstructorPath()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        Assert.Equal(CaptureSnapshot(legacyEditor), CaptureSnapshot(factoryEditor));
    }

    [Fact]
    public void GraphEditorView_RemainsCompatibilityFacadeDuringStagedMigration()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        var legacyView = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };

        var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });

        Assert.Same(legacyEditor, legacyView.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, legacyView.ChromeMode);
        Assert.Same(factoryEditor, factoryView.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, factoryView.ChromeMode);
    }

    [Fact]
    public void StagedMigrationPath_AllowsHostsToAdoptNewApisWithoutImmediateRewrite()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        var legacyEverywhere = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };
        var factoryEditorOnly = new GraphEditorView
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };
        var factoryViewOnly = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var factoryEverywhere = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });

        AssertViewBindings(legacyEverywhere, legacyEditor);
        AssertViewBindings(factoryEditorOnly, factoryEditor);
        AssertViewBindings(factoryViewOnly, legacyEditor);
        AssertViewBindings(factoryEverywhere, factoryEditor);
    }

    private static void AssertViewBindings(GraphEditorView view, GraphEditorViewModel expectedEditor)
    {
        Assert.Same(expectedEditor, view.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, view.ChromeMode);
    }

    private static EditorParitySnapshot CaptureSnapshot(GraphEditorViewModel editor)
    {
        var menuIds = editor.BuildContextMenu(CreateMenuContext()).Select(item => item.Id).ToArray();
        var firstNode = Assert.Single(editor.Nodes);

        return new EditorParitySnapshot(
            editor.Title,
            editor.Description,
            editor.StyleOptions.Shell.HighlightHex,
            editor.BehaviorOptions.View.ShowMiniMap,
            editor.StatsCaption,
            firstNode.DisplaySubtitle,
            firstNode.DisplayDescription,
            menuIds,
            editor.WorkspacePath,
            editor.FragmentPath,
            editor.FragmentLibraryPath);
    }

    private static ContextMenuContext CreateMenuContext()
        => new(
            ContextMenuTargetKind.Canvas,
            new GraphPoint(320, 180));

    private static GraphEditorViewModel CreateLegacyEditor(MigrationHarness harness)
        => new(
            harness.Document,
            harness.Catalog,
            harness.CompatibilityService,
            harness.WorkspaceService,
            harness.FragmentWorkspaceService,
            harness.StyleOptions,
            harness.BehaviorOptions,
            harness.FragmentLibraryService,
            harness.MenuAugmentor,
            harness.PresentationProvider,
            harness.LocalizationProvider);

    private static GraphEditorViewModel CreateFactoryEditor(MigrationHarness harness)
        => AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = harness.Document,
            NodeCatalog = harness.Catalog,
            CompatibilityService = harness.CompatibilityService,
            WorkspaceService = harness.WorkspaceService,
            FragmentWorkspaceService = harness.FragmentWorkspaceService,
            StyleOptions = harness.StyleOptions,
            BehaviorOptions = harness.BehaviorOptions,
            FragmentLibraryService = harness.FragmentLibraryService,
            ContextMenuAugmentor = harness.MenuAugmentor,
            NodePresentationProvider = harness.PresentationProvider,
            LocalizationProvider = harness.LocalizationProvider,
        });

    private static MigrationHarness CreateHarness()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "astergraph-migration-tests",
            Guid.NewGuid().ToString("N"));
        var definitionId = new NodeDefinitionId("tests.migration.node");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Migration Node",
            "Tests",
            "Compatibility",
            [],
            []));

        return new MigrationHarness(
            CreateDocument(definitionId),
            catalog,
            new DefaultPortCompatibilityService(),
            new GraphWorkspaceService(Path.Combine(root, "workspace.json")),
            new GraphFragmentWorkspaceService(Path.Combine(root, "fragment.json")),
            GraphEditorStyleOptions.Default with
            {
                Shell = GraphEditorStyleOptions.Default.Shell with
                {
                    HighlightHex = "#F3B36B",
                },
            },
            GraphEditorBehaviorOptions.Default with
            {
                View = GraphEditorBehaviorOptions.Default.View with
                {
                    ShowMiniMap = false,
                },
            },
            new GraphFragmentLibraryService(Path.Combine(root, "fragments")),
            new MigrationContextMenuAugmentor(),
            new MigrationNodePresentationProvider(),
            new MigrationLocalizationProvider());
    }

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Migration Graph",
            "Compatibility regression coverage.",
            [
                new GraphNode(
                    "tests.migration.node-001",
                    "Migration Node",
                    "Tests",
                    "Compatibility",
                    "Migration parity node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [],
                    "#6AD5C4",
                    definitionId),
            ],
            []);

    private sealed record MigrationHarness(
        GraphDocument Document,
        NodeCatalog Catalog,
        DefaultPortCompatibilityService CompatibilityService,
        GraphWorkspaceService WorkspaceService,
        GraphFragmentWorkspaceService FragmentWorkspaceService,
        GraphEditorStyleOptions StyleOptions,
        GraphEditorBehaviorOptions BehaviorOptions,
        GraphFragmentLibraryService FragmentLibraryService,
        MigrationContextMenuAugmentor MenuAugmentor,
        MigrationNodePresentationProvider PresentationProvider,
        MigrationLocalizationProvider LocalizationProvider);

    private sealed record EditorParitySnapshot(
        string Title,
        string Description,
        string HighlightHex,
        bool ShowMiniMap,
        string StatsCaption,
        string NodeSubtitle,
        string NodeDescription,
        IReadOnlyList<string> MenuIds,
        string WorkspacePath,
        string FragmentPath,
        string FragmentLibraryPath);

    private sealed class MigrationContextMenuAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => [.. stockItems, new MenuItemDescriptor("tests.migration.host-action", "Host Action")];
    }

    private sealed class MigrationNodePresentationProvider : INodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(NodeViewModel node)
            => new(
                SubtitleOverride: "Migration subtitle",
                DescriptionOverride: "Migration description");
    }

    private sealed class MigrationLocalizationProvider : IGraphLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => key == "editor.stats.caption"
                ? "Localized stats {0}/{1}/{2:0}"
                : fallback;
    }
}
