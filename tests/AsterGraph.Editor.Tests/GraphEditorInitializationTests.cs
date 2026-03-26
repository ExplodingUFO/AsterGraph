using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using Avalonia.Headless.XUnit;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
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
public sealed class GraphEditorInitializationTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.init.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.init.target");
    private const string SourceNodeId = "tests.init.source-001";
    private const string TargetNodeId = "tests.init.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void CreateEditorFactory_ValidatesRequiredInputs()
    {
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.Create(null!));

        var nullDocument = new AsterGraphEditorOptions
        {
            NodeCatalog = new NodeCatalog(),
            CompatibilityService = new RecordingCompatibilityService(),
        };
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.Create(nullDocument));

        var nullCatalog = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            CompatibilityService = new RecordingCompatibilityService(),
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
    public void CreateSessionFactory_ValidatesRequiredInputs()
    {
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.CreateSession(null!));

        var nullDocument = new AsterGraphEditorOptions
        {
            NodeCatalog = new NodeCatalog(),
            CompatibilityService = new RecordingCompatibilityService(),
        };
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.CreateSession(nullDocument));

        var nullCatalog = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            CompatibilityService = new RecordingCompatibilityService(),
        };
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.CreateSession(nullCatalog));

        var nullCompatibility = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = new NodeCatalog(),
        };
        Assert.Throws<ArgumentNullException>(() => AsterGraphEditorFactory.CreateSession(nullCompatibility));
    }

    [Fact]
    public async Task CreateEditorFactory_ForwardsCompositionSeamsUnchanged()
    {
        var workspaceService = new RecordingWorkspaceService("workspace://graph-editor-init");
        var fragmentWorkspaceService = new RecordingFragmentWorkspaceService("fragment://graph-editor-init");
        var fragmentLibraryService = new RecordingFragmentLibraryService("library://graph-editor-init");
        var serializer = new RecordingClipboardPayloadSerializer();
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
        var menuAugmentor = new TestContextMenuAugmentor();
        var presentationProvider = new TestNodePresentationProvider();
        var localizationProvider = new TestLocalizationProvider();

        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new RecordingCompatibilityService(),
            WorkspaceService = workspaceService,
            FragmentWorkspaceService = fragmentWorkspaceService,
            StyleOptions = styleOptions,
            BehaviorOptions = behaviorOptions,
            FragmentLibraryService = fragmentLibraryService,
            ClipboardPayloadSerializer = serializer,
            ContextMenuAugmentor = menuAugmentor,
            NodePresentationProvider = presentationProvider,
            LocalizationProvider = localizationProvider,
        });

        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        await editor.CopySelectionAsync();
        editor.ExportSelectionFragment();
        editor.SaveWorkspace();

        Assert.Same(styleOptions, editor.StyleOptions);
        Assert.Same(behaviorOptions, editor.BehaviorOptions);
        Assert.Same(menuAugmentor, editor.ContextMenuAugmentor);
        Assert.Same(presentationProvider, editor.NodePresentationProvider);
        Assert.Same(localizationProvider, editor.LocalizationProvider);
        Assert.Equal(workspaceService.WorkspacePath, editor.WorkspacePath);
        Assert.Equal(fragmentWorkspaceService.FragmentPath, editor.FragmentPath);
        Assert.Equal(fragmentLibraryService.LibraryPath, editor.FragmentLibraryPath);
        Assert.Equal("#F3B36B", editor.StyleOptions.Shell.HighlightHex);
        Assert.False(editor.BehaviorOptions.View.ShowMiniMap);
        Assert.Equal(1, workspaceService.SaveCalls);
        Assert.Equal(1, serializer.SerializeCalls);
        Assert.Equal(1, fragmentWorkspaceService.SaveCalls);
    }

    [Fact]
    public void CreateEditorFactory_SetsPackageNeutralDefaultPathsAndSharedSession()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "graph-editor-init-defaults", Guid.NewGuid().ToString("N"));
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new RecordingCompatibilityService(),
            StorageRootPath = storageRoot,
        });

        Assert.IsAssignableFrom<IGraphEditorSession>(editor.Session);
        Assert.Equal(GraphEditorStorageDefaults.GetWorkspacePath(storageRoot), editor.WorkspacePath);
        Assert.Equal(GraphEditorStorageDefaults.GetFragmentPath(storageRoot), editor.FragmentPath);
        Assert.Equal(GraphEditorStorageDefaults.GetFragmentLibraryPath(storageRoot), editor.FragmentLibraryPath);
        Assert.Equal("Initialization Test Graph", editor.Session.Queries.CreateDocumentSnapshot().Title);

        editor.Session.Commands.AddNode(TargetDefinitionId, new GraphPoint(360, 220));

        Assert.Equal(3, editor.Nodes.Count);
        Assert.Equal(3, editor.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    [Fact]
    public void CreateEditorFactory_ExposesSessionAndDiagnosticsThroughReturnedFacade()
    {
        var workspaceService = new ThrowingWorkspaceService("workspace://graph-editor-diagnostics");
        var diagnostics = new RecordingDiagnosticsSink();
        GraphEditorRecoverableFailureEventArgs? failure = null;
        var commandIds = new List<string>();
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new RecordingCompatibilityService(),
            WorkspaceService = workspaceService,
            DiagnosticsSink = diagnostics,
        });

        editor.Session.Events.RecoverableFailure += (_, args) => failure = args;
        editor.Session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        editor.Session.Commands.SaveWorkspace();

        Assert.IsAssignableFrom<IGraphEditorSession>(editor.Session);
        Assert.Equal("workspace://graph-editor-diagnostics", editor.WorkspacePath);
        Assert.Equal(1, workspaceService.SaveCalls);
        Assert.Contains("workspace.save", commandIds);
        Assert.NotNull(failure);
        Assert.Equal("workspace.save.failed", failure!.Code);
        Assert.Single(diagnostics.Diagnostics);
        Assert.Equal("workspace.save.failed", diagnostics.Diagnostics[0].Code);
    }

    [Fact]
    public void CreateSessionFactory_ProvidesRuntimeSurfaceWithoutViewConstruction()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "graph-editor-session-init-tests", Guid.NewGuid().ToString("N"));
        var compatibility = new RecordingCompatibilityService();
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = compatibility,
            StorageRootPath = storageRoot,
        });
        var commandIds = new List<string>();
        GraphEditorViewportChangedEventArgs? viewportChanged = null;

        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);
        session.Events.ViewportChanged += (_, args) => viewportChanged = args;

        var before = session.Queries.CreateDocumentSnapshot();
        var compatibleTargets = session.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId);

        using (session.BeginMutation("initialization-batch"))
        {
            session.Commands.AddNode(TargetDefinitionId, new GraphPoint(360, 220));
            session.Commands.PanBy(16, 12);
        }

        var after = session.Queries.CreateDocumentSnapshot();

        Assert.IsAssignableFrom<IGraphEditorSession>(session);
        Assert.Equal("Initialization Test Graph", before.Title);
        Assert.Single(compatibleTargets);
        Assert.Equal(TargetNodeId, compatibleTargets[0].Node.Id);
        Assert.Equal(before.Nodes.Count + 1, after.Nodes.Count);
        Assert.Equal(new[] { "nodes.add", "viewport.pan" }, commandIds);
        Assert.NotNull(viewportChanged);
        Assert.Equal(126, viewportChanged!.PanX);
        Assert.Equal(108, viewportChanged.PanY);
        Assert.True(compatibility.EvaluateCalls > 0);
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

    [AvaloniaFact]
    public void DefaultGraphEditorViewComposition_PreservesExpectedChromeBehavior()
    {
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new RecordingCompatibilityService(),
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
                    SourceNodeId,
                    "Initialization Source",
                    "Tests",
                    "Initialization",
                    "Used to verify runtime and compatibility queries.",
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
                    "Initialization Target",
                    "Tests",
                    "Initialization",
                    "Used to verify compatibility continuity.",
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
        catalog.RegisterDefinition(
            new NodeDefinition(
                SourceDefinitionId,
                "Initialization Source",
                "Tests",
                "Initialization",
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
                "Initialization Target",
                "Tests",
                "Initialization",
                [
                    new PortDefinition(
                        TargetPortId,
                        "Input",
                        new PortTypeId("float"),
                        "#F3B36B"),
                ],
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

    private sealed class ThrowingWorkspaceService(string workspacePath) : IGraphWorkspaceService
    {
        public string WorkspacePath { get; } = workspacePath;

        public int SaveCalls { get; private set; }

        public void Save(GraphDocument document)
        {
            SaveCalls++;
            throw new InvalidOperationException("workspace save exploded");
        }

        public GraphDocument Load()
            => throw new NotSupportedException();

        public bool Exists()
            => false;
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
}
