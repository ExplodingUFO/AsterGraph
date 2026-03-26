using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

[Collection("Avalonia UI")]
public sealed class GraphEditorMigrationCompatibilityTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.migration.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.migration.target");
    private const string SourceNodeId = "tests.migration.source-001";
    private const string TargetNodeId = "tests.migration.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

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
        Assert.IsAssignableFrom<IGraphEditorSession>(legacyEditor.Session);

        var snapshot = CaptureSnapshot(legacyEditor);
        Assert.Equal("Migration subtitle", snapshot.NodeSubtitle);
        Assert.Equal("Migration description", snapshot.NodeDescription);
        Assert.Equal("Localized stats 2/0/88", snapshot.StatsCaption);
        Assert.Contains("tests.migration.host-action", snapshot.MenuSignature);
        Assert.Equal(1, snapshot.CompatibleTargetCount);
    }

    [Fact]
    public void NewInitializationApis_CreateEditorStateEquivalentToLegacyConstructorPath()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        Assert.Equal(CaptureSnapshot(legacyEditor), CaptureSnapshot(factoryEditor));
    }

    [AvaloniaFact]
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

    [AvaloniaFact]
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

    [AvaloniaFact]
    public void StagedMigrationPath_PreservesPresentationReplacementAcrossLegacyAndFactoryViews()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var presentation = new AsterGraphPresentationOptions
        {
            NodeVisualPresenter = new RecordingNodeVisualPresenter(),
            ContextMenuPresenter = new RecordingContextMenuPresenter(),
            InspectorPresenter = new RecordingInspectorPresenter(),
            MiniMapPresenter = new RecordingMiniMapPresenter(),
        };

        var legacyView = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
            Presentation = presentation,
        };
        var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
            Presentation = presentation,
        });

        AssertPresentationBindings(legacyView, presentation);
        AssertPresentationBindings(factoryView, presentation);
    }

    [Fact]
    public async Task RuntimeSession_ServiceSeamsAndCompatibilityService_RemainAvailableAcrossMigrationPaths()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var factorySession = CreateFactorySession(harness);
        var commandIds = new List<string>();

        factorySession.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        SelectSourceNode(legacyEditor);
        SelectSourceNode(factoryEditor);

        await legacyEditor.CopySelectionAsync();
        await factoryEditor.CopySelectionAsync();
        legacyEditor.ExportSelectionFragment();
        factoryEditor.ExportSelectionFragment();
        factorySession.Commands.SaveWorkspace();

        var legacySelection = legacyEditor.Session.Queries.GetSelectionSnapshot();
        var factorySelection = factoryEditor.Session.Queries.GetSelectionSnapshot();
        var legacyCompatible = legacyEditor.Session.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId);
        var factoryCompatible = factoryEditor.Session.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId);
        var sessionCompatible = factorySession.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId);

        Assert.IsAssignableFrom<IGraphEditorSession>(legacyEditor.Session);
        Assert.IsAssignableFrom<IGraphEditorSession>(factoryEditor.Session);
        Assert.Equal(legacySelection.SelectedNodeIds, factorySelection.SelectedNodeIds);
        Assert.Single(legacyCompatible);
        Assert.Single(factoryCompatible);
        Assert.Single(sessionCompatible);
        Assert.Equal(TargetNodeId, legacyCompatible[0].Node.Id);
        Assert.Equal(TargetNodeId, factoryCompatible[0].Node.Id);
        Assert.Equal(TargetNodeId, sessionCompatible[0].Node.Id);
        Assert.Equal(1, harness.WorkspaceService.SaveCalls);
        Assert.Equal(2, harness.FragmentWorkspaceService.SaveCalls);
        Assert.Equal(2, harness.ClipboardPayloadSerializer.SerializeCalls);
        Assert.Contains("workspace.save", commandIds);
        Assert.True(harness.CompatibilityService.EvaluateCalls > 0);

        factorySession.Commands.AddNode(TargetDefinitionId, new GraphPoint(640, 220));

        Assert.Contains("nodes.add", commandIds);
        Assert.Equal(3, factorySession.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    private static void AssertViewBindings(GraphEditorView view, GraphEditorViewModel expectedEditor)
    {
        Assert.Same(expectedEditor, view.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, view.ChromeMode);
    }

    private static void AssertPresentationBindings(GraphEditorView view, AsterGraphPresentationOptions presentation)
    {
        var canvas = view.FindControl<NodeCanvas>("PART_NodeCanvas");
        var inspector = view.FindControl<GraphInspectorView>("PART_InspectorSurface");
        var miniMap = view.FindControl<GraphMiniMap>("PART_MiniMapSurface");

        Assert.NotNull(canvas);
        Assert.NotNull(inspector);
        Assert.NotNull(miniMap);
        Assert.Same(presentation, view.Presentation);
        Assert.Same(presentation.NodeVisualPresenter, canvas.NodeVisualPresenter);
        Assert.Same(presentation.ContextMenuPresenter, canvas.ContextMenuPresenter);
        Assert.Same(presentation.InspectorPresenter, inspector.InspectorPresenter);
        Assert.Same(presentation.MiniMapPresenter, miniMap.MiniMapPresenter);
    }

    private static void SelectSourceNode(GraphEditorViewModel editor)
        => editor.SelectSingleNode(Assert.Single(editor.Nodes, node => node.Id == SourceNodeId), updateStatus: false);

    private static EditorParitySnapshot CaptureSnapshot(GraphEditorViewModel editor)
    {
        var menuSignature = string.Join(
            "|",
            editor.BuildContextMenu(CreateMenuContext()).Select(item => item.Id));
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == SourceNodeId);

        return new EditorParitySnapshot(
            editor.Title,
            editor.Description,
            editor.StyleOptions.Shell.HighlightHex,
            editor.BehaviorOptions.View.ShowMiniMap,
            editor.StatsCaption,
            sourceNode.DisplaySubtitle,
            sourceNode.DisplayDescription,
            menuSignature,
            editor.WorkspacePath,
            editor.FragmentPath,
            editor.FragmentLibraryPath,
            editor.Session.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId).Count);
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
            harness.LocalizationProvider,
            harness.ClipboardPayloadSerializer,
            harness.DiagnosticsSink);

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
            ClipboardPayloadSerializer = harness.ClipboardPayloadSerializer,
            ContextMenuAugmentor = harness.MenuAugmentor,
            NodePresentationProvider = harness.PresentationProvider,
            LocalizationProvider = harness.LocalizationProvider,
            DiagnosticsSink = harness.DiagnosticsSink,
        });

    private static IGraphEditorSession CreateFactorySession(MigrationHarness harness)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = harness.Document,
            NodeCatalog = harness.Catalog,
            CompatibilityService = harness.CompatibilityService,
            WorkspaceService = harness.WorkspaceService,
            FragmentWorkspaceService = harness.FragmentWorkspaceService,
            StyleOptions = harness.StyleOptions,
            BehaviorOptions = harness.BehaviorOptions,
            FragmentLibraryService = harness.FragmentLibraryService,
            ClipboardPayloadSerializer = harness.ClipboardPayloadSerializer,
            ContextMenuAugmentor = harness.MenuAugmentor,
            NodePresentationProvider = harness.PresentationProvider,
            LocalizationProvider = harness.LocalizationProvider,
            DiagnosticsSink = harness.DiagnosticsSink,
        });

    private static MigrationHarness CreateHarness()
    {
        var compatibility = new RecordingCompatibilityService();
        return new MigrationHarness(
            CreateDocument(),
            CreateCatalog(),
            compatibility,
            new RecordingWorkspaceService("workspace://migration"),
            new RecordingFragmentWorkspaceService("fragment://migration"),
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
            new RecordingFragmentLibraryService("library://migration"),
            new RecordingClipboardPayloadSerializer(),
            new RecordingDiagnosticsSink(),
            new MigrationContextMenuAugmentor(),
            new MigrationNodePresentationProvider(),
            new MigrationLocalizationProvider());
    }

    private static GraphDocument CreateDocument()
        => new(
            "Migration Graph",
            "Compatibility regression coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Migration Source",
                    "Tests",
                    "Compatibility",
                    "Migration parity source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [
                        new GraphPort(
                            SourcePortId,
                            "Output",
                            PortDirection.Output,
                            "float",
                            "#6AD5C4",
                            new PortTypeId("float")),
                    ],
                    "#6AD5C4",
                    SourceDefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Migration Target",
                    "Tests",
                    "Compatibility",
                    "Migration parity target node.",
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
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Migration Source",
            "Tests",
            "Compatibility",
            [],
            [
                new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
            ]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Migration Target",
            "Tests",
            "Compatibility",
            [
                new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
            ],
            []));
        return catalog;
    }

    private sealed record MigrationHarness(
        GraphDocument Document,
        NodeCatalog Catalog,
        RecordingCompatibilityService CompatibilityService,
        RecordingWorkspaceService WorkspaceService,
        RecordingFragmentWorkspaceService FragmentWorkspaceService,
        GraphEditorStyleOptions StyleOptions,
        GraphEditorBehaviorOptions BehaviorOptions,
        RecordingFragmentLibraryService FragmentLibraryService,
        RecordingClipboardPayloadSerializer ClipboardPayloadSerializer,
        RecordingDiagnosticsSink DiagnosticsSink,
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
        string MenuSignature,
        string WorkspacePath,
        string FragmentPath,
        string FragmentLibraryPath,
        int CompatibleTargetCount);

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

    private sealed class RecordingWorkspaceService(string workspacePath) : IGraphWorkspaceService
    {
        public string WorkspacePath { get; } = workspacePath;

        public int SaveCalls { get; private set; }

        public GraphDocument? LastSaved { get; private set; }

        public void Save(GraphDocument document)
        {
            SaveCalls++;
            LastSaved = document;
        }

        public GraphDocument Load()
            => LastSaved ?? throw new InvalidOperationException("No saved workspace.");

        public bool Exists()
            => LastSaved is not null;
    }

    private sealed class RecordingFragmentWorkspaceService(string fragmentPath) : IGraphFragmentWorkspaceService
    {
        public string FragmentPath { get; } = fragmentPath;

        public int SaveCalls { get; private set; }

        public GraphSelectionFragment? LastSaved { get; private set; }

        public void Save(GraphSelectionFragment fragment, string? path = null)
        {
            SaveCalls++;
            LastSaved = fragment;
        }

        public GraphSelectionFragment Load(string? path = null)
            => LastSaved ?? throw new InvalidOperationException("No saved fragment.");

        public bool Exists(string? path = null)
            => LastSaved is not null;

        public void Delete(string? path = null)
            => LastSaved = null;
    }

    private sealed class RecordingFragmentLibraryService(string libraryPath) : IGraphFragmentLibraryService
    {
        public string LibraryPath { get; } = libraryPath;

        public IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates()
            => [];

        public string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
            => Path.Combine(LibraryPath, $"{name ?? "fragment"}.json");

        public GraphSelectionFragment LoadTemplate(string path)
            => throw new NotSupportedException();

        public void DeleteTemplate(string path)
        {
        }
    }

    private sealed class RecordingClipboardPayloadSerializer : IGraphClipboardPayloadSerializer
    {
        public int SerializeCalls { get; private set; }

        public string Serialize(GraphSelectionFragment fragment)
        {
            SerializeCalls++;
            return "serialized-fragment";
        }

        public bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        {
            fragment = null;
            return false;
        }
    }

    private sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
    {
        public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

        public void Publish(GraphEditorDiagnostic diagnostic)
            => Diagnostics.Add(diagnostic);
    }

    private sealed class RecordingCompatibilityService : IPortCompatibilityService
    {
        public int EvaluateCalls { get; private set; }

        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
        {
            EvaluateCalls++;
            return sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
        }
    }

    private sealed class RecordingNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        private readonly DefaultGraphNodeVisualPresenter _stockPresenter = new();

        public GraphNodeVisual Create(GraphNodeVisualContext context)
            => _stockPresenter.Create(context);

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
            => _stockPresenter.Update(visual, context);
    }

    private sealed class RecordingContextMenuPresenter : IGraphContextMenuPresenter
    {
        public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
            => throw new NotSupportedException();
    }

    private sealed class RecordingInspectorPresenter : IGraphInspectorPresenter
    {
        public Control Create(GraphEditorViewModel? editor)
            => new TextBlock
            {
                Text = $"MIGRATION INSPECTOR:{editor?.InspectorTitle ?? "<none>"}",
            };
    }

    private sealed class RecordingMiniMapPresenter : IGraphMiniMapPresenter
    {
        public Control Create(GraphEditorViewModel? editor)
            => new TextBlock
            {
                Text = $"MIGRATION MINIMAP:{editor?.Title ?? "<none>"}",
            };
    }
}
