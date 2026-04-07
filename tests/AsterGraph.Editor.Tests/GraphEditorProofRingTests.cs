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
}
