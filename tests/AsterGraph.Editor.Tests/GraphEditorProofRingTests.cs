using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
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
using System.Reflection;
using Xunit;

namespace AsterGraph.Editor.Tests;

[Collection("Avalonia UI")]
public sealed class GraphEditorProofRingTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.proof.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.proof.target");
    private const string SourceNodeId = "tests.proof.source-001";
    private const string TargetNodeId = "tests.proof.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [AvaloniaFact]
    public void FullShellAndStandaloneProof_PreserveHostOptOutPresentationAndMenuContracts()
    {
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            ContextMenuAugmentor = new ProofContextMenuAugmentor(),
            NodePresentationProvider = new ProofPresentationProvider(),
            LocalizationProvider = new ProofLocalizationProvider(),
        });
        var presentation = new AsterGraphPresentationOptions
        {
            NodeVisualPresenter = new DefaultGraphNodeVisualPresenter(),
            ContextMenuPresenter = new RecordingContextMenuPresenter(),
            InspectorPresenter = new ProofInspectorPresenter(),
            MiniMapPresenter = new ProofMiniMapPresenter(),
        };
        var directView = new GraphEditorView
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
            Presentation = presentation,
        };
        var optOutView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = editor,
            EnableDefaultContextMenu = false,
            EnableDefaultCommandShortcuts = false,
            Presentation = presentation,
        });
        var standaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = editor,
            EnableDefaultContextMenu = false,
            EnableDefaultCommandShortcuts = false,
            Presentation = presentation,
        });
        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = directView,
        };
        window.Show();

        try
        {
            editor.SelectSingleNode(editor.FindNode(SourceNodeId), updateStatus: false);
            var menu = editor.BuildContextMenu(new ContextMenuContext(
                ContextMenuTargetKind.Node,
                new GraphPoint(120, 160),
                selectedNodeId: SourceNodeId,
                selectedNodeIds: [SourceNodeId],
                clickedNodeId: SourceNodeId,
                hostContext: new ProofHostContext()));
            var directCanvas = directView.FindControl<NodeCanvas>("PART_NodeCanvas");
            var directInspector = directView.FindControl<GraphInspectorView>("PART_InspectorSurface");
            var directMiniMap = directView.FindControl<GraphMiniMap>("PART_MiniMapSurface");
            var optOutCanvas = optOutView.FindControl<NodeCanvas>("PART_NodeCanvas");

            Assert.NotNull(directCanvas);
            Assert.NotNull(directInspector);
            Assert.NotNull(directMiniMap);
            Assert.NotNull(optOutCanvas);
            Assert.Contains(menu, item => item.Id == "proof.host-action");
            Assert.Equal("Proof Source", editor.InspectorTitle);
            Assert.Equal("Proof subtitle", editor.SelectedNode?.DisplaySubtitle);
            Assert.False(optOutView.EnableDefaultContextMenu);
            Assert.False(optOutView.EnableDefaultCommandShortcuts);
            Assert.False(optOutCanvas.EnableDefaultContextMenu);
            Assert.False(optOutCanvas.EnableDefaultCommandShortcuts);
            Assert.False(standaloneCanvas.EnableDefaultContextMenu);
            Assert.False(standaloneCanvas.EnableDefaultCommandShortcuts);
            Assert.Same(presentation.ContextMenuPresenter, directCanvas.ContextMenuPresenter);
            Assert.Same(presentation.InspectorPresenter, directInspector.InspectorPresenter);
            Assert.Same(presentation.MiniMapPresenter, directMiniMap.MiniMapPresenter);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RuntimeSessionProof_CoversCommandsViewportSelectionAndDiagnostics()
    {
        var workspace = new RecordingWorkspaceService();
        var diagnostics = new RecordingDiagnosticsSink();
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            WorkspaceService = workspace,
            DiagnosticsSink = diagnostics,
        });

        session.Commands.UpdateViewportSize(1280, 720);
        session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        session.Commands.SetNodePositions(
            [
                new NodePositionSnapshot(SourceNodeId, new GraphPoint(180, 200)),
                new NodePositionSnapshot(TargetNodeId, new GraphPoint(520, 200)),
            ],
            updateStatus: false);
        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        session.Commands.SetSelection([TargetNodeId], TargetNodeId, updateStatus: false);
        session.Commands.SaveWorkspace();

        var snapshot = session.Queries.CreateDocumentSnapshot();
        var selection = session.Queries.GetSelectionSnapshot();
        var viewport = session.Queries.GetViewportSnapshot();
        var capabilities = session.Queries.GetCapabilitySnapshot();
        var commandDescriptors = session.Queries.GetCommandDescriptors();
        var canvasMenuDescriptors = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var inspection = session.Diagnostics.CaptureInspectionSnapshot();
        var recentDiagnostics = session.Diagnostics.GetRecentDiagnostics(10);
        var runtimeFields = session.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.Equal(2, snapshot.Nodes.Count);
        Assert.Single(snapshot.Connections);
        Assert.Equal(TargetNodeId, selection.PrimarySelectedNodeId);
        Assert.Equal(1280, viewport.ViewportWidth);
        Assert.Equal(720, viewport.ViewportHeight);
        Assert.True(capabilities.CanSaveWorkspace);
        Assert.True(workspace.Exists());
        Assert.Equal(1, workspace.SaveCalls);
        Assert.Contains(commandDescriptors, descriptor => descriptor.Id == "nodes.add" && descriptor.IsEnabled);
        Assert.Contains(canvasMenuDescriptors, descriptor => descriptor.Id == "canvas-add-node");
        Assert.False(inspection.PendingConnection.HasPendingConnection);
        Assert.Equal(TargetNodeId, inspection.Selection.PrimarySelectedNodeId);
        Assert.Contains(inspection.FeatureDescriptors, descriptor => descriptor.Id == "query.feature-descriptors" && descriptor.IsAvailable);
        Assert.Contains(recentDiagnostics, diagnostic => diagnostic.Code == "workspace.save.succeeded");
        Assert.Contains(diagnostics.Diagnostics, diagnostic => diagnostic.Code == "workspace.save.succeeded");
        Assert.DoesNotContain(runtimeFields, field => field.FieldType == typeof(GraphEditorViewModel));
    }

    [Fact]
    public void RetainedCompatibilityProof_UsesAdapterBackedSessionHost()
    {
        var legacyEditor = new GraphEditorViewModel(
            CreateDocument(),
            CreateCatalog(),
            new ExactCompatibilityService());
        var factoryEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });

        var legacyHost = legacyEditor.Session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(legacyEditor.Session);
        var factoryHost = factoryEditor.Session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(factoryEditor.Session);

        Assert.NotNull(legacyHost);
        Assert.NotNull(factoryHost);
        Assert.IsNotType<GraphEditorViewModel>(legacyHost);
        Assert.IsNotType<GraphEditorViewModel>(factoryHost);
    }

    [Fact]
    public void RuntimeAndRetainedProof_StayAlignedOnSharedDescriptorSignatures()
    {
        var legacyEditor = new GraphEditorViewModel(
            CreateDocument(),
            CreateCatalog(),
            new ExactCompatibilityService());
        var factoryEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });
        var runtimeSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });

        var legacySignature = CaptureSharedSessionSignature(legacyEditor.Session);
        var factorySignature = CaptureSharedSessionSignature(factoryEditor.Session);
        var runtimeSignature = CaptureSharedSessionSignature(runtimeSession);
        var legacyCommandIds = legacyEditor.Session.Queries.GetCommandDescriptors().Select(descriptor => descriptor.Id).ToHashSet(StringComparer.Ordinal);
        var runtimeCommandIds = runtimeSession.Queries.GetCommandDescriptors().Select(descriptor => descriptor.Id).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(legacySignature, factorySignature);
        Assert.Equal(legacySignature, runtimeSignature);
        Assert.True(runtimeCommandIds.IsSubsetOf(legacyCommandIds));
        Assert.Contains("nodes.duplicate", legacyCommandIds);
        Assert.DoesNotContain("nodes.duplicate", runtimeCommandIds);
    }

    [AvaloniaFact]
    public void MigrationProof_RouteSignalsMatchCanonicalAndCompatibilityStory()
    {
        var legacyEditor = new GraphEditorViewModel(
            CreateDocument(),
            CreateCatalog(),
            new ExactCompatibilityService());
        var factoryEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });
        var runtimeSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });
        var legacyDirectView = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };
        var factoryDirectView = new GraphEditorView
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };
        var legacyFactoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var factoryFactoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });

        var legacyDirectSnapshot = CaptureMigrationViewRouteSnapshot(legacyDirectView);
        var factoryDirectSnapshot = CaptureMigrationViewRouteSnapshot(factoryDirectView);
        var legacyFactorySnapshot = CaptureMigrationViewRouteSnapshot(legacyFactoryView);
        var factoryFactorySnapshot = CaptureMigrationViewRouteSnapshot(factoryFactoryView);
        var runtimeSharedCommandIds = CaptureSharedCanonicalCommandIds(runtimeSession);
        var legacySharedCommandIds = CaptureSharedCanonicalCommandIds(legacyEditor.Session);
        var factorySharedCommandIds = CaptureSharedCanonicalCommandIds(factoryEditor.Session);
        var legacyCompatibilityOnlyCommands = CaptureCompatibilityOnlyCommandIds(runtimeSession, legacyEditor.Session);
        var factoryCompatibilityOnlyCommands = CaptureCompatibilityOnlyCommandIds(runtimeSession, factoryEditor.Session);

        Assert.Equal(legacyDirectSnapshot, factoryDirectSnapshot);
        Assert.Equal(legacyDirectSnapshot, legacyFactorySnapshot);
        Assert.Equal(legacyDirectSnapshot, factoryFactorySnapshot);
        Assert.Equal(runtimeSharedCommandIds, legacySharedCommandIds);
        Assert.Equal(runtimeSharedCommandIds, factorySharedCommandIds);
        Assert.Contains("nodes.duplicate", legacyCompatibilityOnlyCommands);
        Assert.Contains("nodes.duplicate", factoryCompatibilityOnlyCommands);
    }

    [AvaloniaFact]
    public void FullShellAndStandaloneProof_UseCanonicalMenuRoutingAcrossSharedAdapters()
    {
        var fullShellPresenter = new RecordingCanonicalContextMenuPresenter();
        var standalonePresenter = new RecordingCanonicalContextMenuPresenter();
        var fullShellEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });
        var standaloneEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });
        var fullShellView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = fullShellEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
            Presentation = new AsterGraphPresentationOptions
            {
                ContextMenuPresenter = fullShellPresenter,
            },
        });
        var standaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = standaloneEditor,
            Presentation = new AsterGraphPresentationOptions
            {
                ContextMenuPresenter = standalonePresenter,
            },
        });
        var fullShellWindow = new Window
        {
            Width = 1440,
            Height = 900,
            Content = fullShellView,
        };
        var standaloneWindow = new Window
        {
            Width = 1440,
            Height = 900,
            Content = standaloneCanvas,
        };
        fullShellWindow.Show();
        standaloneWindow.Show();

        try
        {
            var fullShellCanvas = fullShellView.FindControl<NodeCanvas>("PART_NodeCanvas");
            var fullShellArgs = new ContextRequestedEventArgs();
            var standaloneArgs = new ContextRequestedEventArgs();

            Assert.NotNull(fullShellCanvas);
            Assert.False(fullShellCanvas.AttachPlatformSeams);
            Assert.True(standaloneCanvas.AttachPlatformSeams);

            InvokeCanvasContextRequested(fullShellCanvas, fullShellArgs);
            InvokeCanvasContextRequested(standaloneCanvas, standaloneArgs);

            Assert.True(fullShellArgs.Handled);
            Assert.True(standaloneArgs.Handled);
            Assert.Equal(1, fullShellPresenter.CanonicalOpenCalls);
            Assert.Equal(1, standalonePresenter.CanonicalOpenCalls);
            Assert.Equal(0, fullShellPresenter.CompatibilityOpenCalls);
            Assert.Equal(0, standalonePresenter.CompatibilityOpenCalls);
            Assert.NotNull(fullShellPresenter.LastDescriptors);
            Assert.NotNull(standalonePresenter.LastDescriptors);
            Assert.Contains(fullShellPresenter.LastDescriptors!, descriptor => descriptor.Id == "canvas-add-node");
            Assert.Contains(standalonePresenter.LastDescriptors!, descriptor => descriptor.Id == "canvas-add-node");
            Assert.Same(fullShellEditor.Session.Commands, fullShellPresenter.LastCommands);
            Assert.Same(standaloneEditor.Session.Commands, standalonePresenter.LastCommands);
        }
        finally
        {
            fullShellWindow.Close();
            standaloneWindow.Close();
        }
    }

    private static void InvokeCanvasContextRequested(NodeCanvas canvas, ContextRequestedEventArgs args)
    {
        var method = typeof(NodeCanvas).GetMethod("HandleCanvasContextRequested", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("Could not find NodeCanvas.HandleCanvasContextRequested.");
        method.Invoke(canvas, [canvas, args]);
    }

    private static SessionSharedSignature CaptureSharedSessionSignature(IGraphEditorSession session)
        => new(
            string.Join("|", session.Queries.GetFeatureDescriptors().Select(descriptor => $"{descriptor.Category}:{descriptor.Id}:{descriptor.IsAvailable}")),
            string.Join("|", session.Queries.GetCommandDescriptors()
                .Where(descriptor => SharedCanonicalCommandIds.Contains(descriptor.Id, StringComparer.Ordinal))
                .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
                .Select(descriptor => $"{descriptor.Id}:{descriptor.IsEnabled}")),
            string.Join("|", session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180))).Select(descriptor => descriptor.Id)));

    private static string[] CaptureSharedCanonicalCommandIds(IGraphEditorSession session)
        => session.Queries.GetCommandDescriptors()
            .Where(descriptor => SharedCanonicalCommandIds.Contains(descriptor.Id, StringComparer.Ordinal))
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .Select(descriptor => descriptor.Id)
            .ToArray();

    private static string[] CaptureCompatibilityOnlyCommandIds(IGraphEditorSession canonicalSession, IGraphEditorSession retainedSession)
    {
        var canonicalCommandIds = canonicalSession.Queries.GetCommandDescriptors()
            .Select(descriptor => descriptor.Id)
            .ToHashSet(StringComparer.Ordinal);

        return retainedSession.Queries.GetCommandDescriptors()
            .Select(descriptor => descriptor.Id)
            .Where(id => !canonicalCommandIds.Contains(id))
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
    }

    private static MigrationViewRouteSnapshot CaptureMigrationViewRouteSnapshot(GraphEditorView view)
    {
        var canvas = view.FindControl<NodeCanvas>("PART_NodeCanvas");
        Assert.NotNull(canvas);

        return new(
            view.ChromeMode,
            view.Editor is not null,
            GetAttachPlatformSeams(canvas),
            canvas.EnableDefaultContextMenu,
            canvas.EnableDefaultCommandShortcuts);
    }

    private static bool GetAttachPlatformSeams(NodeCanvas canvas)
    {
        var property = typeof(NodeCanvas).GetProperty("AttachPlatformSeams", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("Could not find NodeCanvas.AttachPlatformSeams.");
        return property.GetValue(canvas) as bool? ?? throw new Xunit.Sdk.XunitException("NodeCanvas.AttachPlatformSeams did not return a bool.");
    }

    private static GraphDocument CreateDocument()
        => new(
            "Proof Ring Graph",
            "Milestone-level host and runtime proof coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Proof Source",
                    "Tests",
                    "Proof",
                    "Produces a float value.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [
                        new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                    ],
                    "#6AD5C4",
                    SourceDefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Proof Target",
                    "Tests",
                    "Proof",
                    "Consumes a float value.",
                    new GraphPoint(420, 160),
                    new GraphSize(240, 160),
                    [
                        new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
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
            "Proof Source",
            "Tests",
            "Proof",
            [],
            [
                new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
            ]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Proof Target",
            "Tests",
            "Proof",
            [
                new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
            ],
            []));
        return catalog;
    }

    private sealed class ProofContextMenuAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => [.. stockItems, new MenuItemDescriptor("proof.host-action", "Proof Host Action")];
    }

    private sealed class ProofPresentationProvider : INodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(NodeViewModel node)
            => new(SubtitleOverride: "Proof subtitle");
    }

    private sealed class ProofLocalizationProvider : IGraphLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => fallback;
    }

    private sealed class ProofInspectorPresenter : IGraphInspectorPresenter
    {
        public Control Create(GraphEditorViewModel? editor)
            => new TextBlock
            {
                Text = $"PROOF INSPECTOR:{editor?.InspectorTitle ?? "<none>"}",
            };
    }

    private sealed class ProofMiniMapPresenter : IGraphMiniMapPresenter
    {
        public Control Create(GraphEditorViewModel? editor)
            => new TextBlock
            {
                Text = $"PROOF MINIMAP:{editor?.Title ?? "<none>"}",
            };
    }

    private sealed class ProofHostContext : IGraphHostContext
    {
        public object Owner => this;

        public object? TopLevel => this;

        public IServiceProvider? Services => null;
    }

    private sealed class RecordingContextMenuPresenter : IGraphContextMenuPresenter
    {
        public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
        {
        }
    }

    private sealed class RecordingCanonicalContextMenuPresenter : IGraphContextMenuPresenter
    {
        public int CompatibilityOpenCalls { get; private set; }

        public int CanonicalOpenCalls { get; private set; }

        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot>? LastDescriptors { get; private set; }

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
            LastDescriptors = descriptors;
            LastCommands = commands;
        }
    }

    private sealed class ExactCompatibilityService : IPortCompatibilityService
    {
        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
            => sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
    }

    private sealed class RecordingWorkspaceService : IGraphWorkspaceService
    {
        public string WorkspacePath => "workspace://proof-ring";

        public int SaveCalls { get; private set; }

        public GraphDocument? SavedDocument { get; private set; }

        public void Save(GraphDocument document)
        {
            SaveCalls++;
            SavedDocument = document;
        }

        public GraphDocument Load()
            => SavedDocument ?? throw new InvalidOperationException("No saved workspace.");

        public bool Exists()
            => SavedDocument is not null;
    }

    private sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
    {
        public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

        public void Publish(GraphEditorDiagnostic diagnostic)
            => Diagnostics.Add(diagnostic);
    }

    private sealed record SessionSharedSignature(
        string FeatureDescriptorSignature,
        string CommandDescriptorSignature,
        string CanvasMenuSignature);

    private sealed record MigrationViewRouteSnapshot(
        GraphEditorViewChromeMode ChromeMode,
        bool EditorAssigned,
        bool CanvasAttachPlatformSeams,
        bool EnableDefaultContextMenu,
        bool EnableDefaultCommandShortcuts);

    private static readonly string[] SharedCanonicalCommandIds =
    [
        "nodes.add",
        "selection.delete",
        "connections.start",
        "connections.connect",
        "connections.cancel",
        "connections.delete",
        "connections.break-port",
        "viewport.fit",
        "viewport.reset",
        "viewport.center-node",
        "workspace.save",
        "workspace.load",
    ];
}
