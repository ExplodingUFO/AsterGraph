using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorDiagnosticsInspectionTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.diagnostics.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.diagnostics.target");
    private const string SourceNodeId = "diagnostics-source-001";
    private const string TargetNodeId = "diagnostics-target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";
    private const int RecentDiagnosticsCapacity = 32;

    [Fact]
    public void GraphEditorViewModel_SessionDiagnostics_CapturesInspectionSnapshotAndOperationHistory()
    {
        var harness = CreateHarness();
        var editor = CreateEditor(harness);

        SelectSourceNode(editor);
        editor.StartConnection(SourceNodeId, SourcePortId);
        editor.ExportSelectionFragment();
        editor.Session.Commands.SaveWorkspace();

        var snapshot = editor.Session.Diagnostics.CaptureInspectionSnapshot();

        Assert.Equal(editor.Title, snapshot.Document.Title);
        Assert.Equal(editor.Nodes.Count, snapshot.Document.Nodes.Count);
        Assert.Equal(SourceNodeId, snapshot.Selection.PrimarySelectedNodeId);
        Assert.Contains(SourceNodeId, snapshot.Selection.SelectedNodeIds);
        Assert.Equal(editor.Zoom, snapshot.Viewport.Zoom);
        Assert.Equal(editor.PanX, snapshot.Viewport.PanX);
        Assert.Equal(editor.PanY, snapshot.Viewport.PanY);
        Assert.Equal(editor.CanSaveWorkspace, snapshot.Capabilities.CanSaveWorkspace);
        Assert.True(snapshot.PendingConnection.HasPendingConnection);
        Assert.Equal(SourceNodeId, snapshot.PendingConnection.SourceNodeId);
        Assert.Equal(SourcePortId, snapshot.PendingConnection.SourcePortId);
        Assert.Equal(editor.StatusMessage, snapshot.Status.Message);
        Assert.Equal(editor.Nodes.Count, snapshot.NodePositions.Count);
        Assert.Equal("fragment.export.succeeded", snapshot.RecentDiagnostics[^2].Code);
        Assert.Equal(GraphEditorDiagnosticSeverity.Info, snapshot.RecentDiagnostics[^2].Severity);
        Assert.Equal("workspace.save.succeeded", snapshot.RecentDiagnostics[^1].Code);
        Assert.Equal(GraphEditorDiagnosticSeverity.Info, snapshot.RecentDiagnostics[^1].Severity);
    }

    [Fact]
    public void GraphEditorViewModel_SessionDiagnostics_TracksHostSeamFailuresInRecentHistory()
    {
        var harness = CreateHarness(contextMenuAugmentor: new ThrowingAugmentor());
        var editor = CreateEditor(harness);

        _ = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(120, 80)));

        var recent = editor.Session.Diagnostics.GetRecentDiagnostics();
        var diagnostic = Assert.Single(recent);

        Assert.Equal("contextmenu.augment.failed", diagnostic.Code);
        Assert.Equal("contextmenu.augment", diagnostic.Operation);
        Assert.Equal(GraphEditorDiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_DiagnosticsHistory_IsBoundedAndRetainsSupportSignals()
    {
        var harness = CreateHarness(workspaceService: new SequencedWorkspaceService("workspace://diagnostics", failSaveCount: 1));
        var session = CreateSession(harness);

        Assert.False(session.Commands.LoadWorkspace());
        var initial = session.Diagnostics.GetRecentDiagnostics();
        Assert.Single(initial);
        Assert.Equal("workspace.load.missing", initial[0].Code);
        Assert.Equal(GraphEditorDiagnosticSeverity.Warning, initial[0].Severity);

        session.Commands.SaveWorkspace();
        var afterFailure = session.Diagnostics.GetRecentDiagnostics();
        Assert.Contains(afterFailure, diagnostic => diagnostic.Code == "workspace.save.failed" && diagnostic.Severity == GraphEditorDiagnosticSeverity.Error);

        for (var index = 0; index < 40; index++)
        {
            session.Commands.SaveWorkspace();
        }

        var recent = session.Diagnostics.GetRecentDiagnostics(100);

        Assert.Equal(RecentDiagnosticsCapacity, recent.Count);
        Assert.Equal("workspace.save.succeeded", recent[^1].Code);
        Assert.Equal(GraphEditorDiagnosticSeverity.Info, recent[^1].Severity);
        Assert.DoesNotContain(recent, diagnostic => diagnostic.Code == "workspace.load.missing");
        Assert.DoesNotContain(recent, diagnostic => diagnostic.Code == "workspace.save.failed");
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_PendingConnectionFlowsIntoInspectionSnapshot()
    {
        var harness = CreateHarness();
        var session = CreateSession(harness);
        var pendingSnapshots = new List<GraphEditorPendingConnectionSnapshot>();

        session.Events.PendingConnectionChanged += (_, args) => pendingSnapshots.Add(args.PendingConnection);
        session.Commands.StartConnection(SourceNodeId, SourcePortId);

        var inspection = session.Diagnostics.CaptureInspectionSnapshot();

        Assert.Single(pendingSnapshots);
        Assert.True(pendingSnapshots[0].HasPendingConnection);
        Assert.Equal(SourceNodeId, pendingSnapshots[0].SourceNodeId);
        Assert.Equal(SourcePortId, pendingSnapshots[0].SourcePortId);
        Assert.True(inspection.PendingConnection.HasPendingConnection);
        Assert.Equal(SourceNodeId, inspection.PendingConnection.SourceNodeId);
        Assert.Equal(SourcePortId, inspection.PendingConnection.SourcePortId);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_PendingConnectionParity_HoldsAcrossCancelAndBatchedComplete()
    {
        var harness = CreateHarness();
        var session = CreateSession(harness);
        var pendingSnapshots = new List<GraphEditorPendingConnectionSnapshot>();

        session.Events.PendingConnectionChanged += (_, args) => pendingSnapshots.Add(args.PendingConnection);

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CancelPendingConnection();

        var afterCancel = session.Diagnostics.CaptureInspectionSnapshot();
        Assert.Equal(2, pendingSnapshots.Count);
        Assert.True(pendingSnapshots[0].HasPendingConnection);
        Assert.False(pendingSnapshots[1].HasPendingConnection);
        Assert.False(afterCancel.PendingConnection.HasPendingConnection);

        pendingSnapshots.Clear();

        using (session.BeginMutation("pending-batch"))
        {
            session.Commands.StartConnection(SourceNodeId, SourcePortId);
            session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        }

        var afterBatch = session.Diagnostics.CaptureInspectionSnapshot();
        Assert.Empty(pendingSnapshots);
        Assert.False(afterBatch.PendingConnection.HasPendingConnection);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_InspectionSnapshot_ExposesPluginCompatibilityRefusalAndDiagnostic()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var plugin = new IncompatibleTrackingPlugin();
        var manifest = new GraphEditorPluginManifest(
            "tests.diagnostics.incompatible-plugin",
            "Diagnostics Incompatible Plugin",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.DirectRegistration,
                typeof(IncompatibleTrackingPlugin).FullName ?? nameof(IncompatibleTrackingPlugin)),
            version: "1.0.0",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "9999.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "menus");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            DiagnosticsSink = diagnostics,
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(plugin, manifest),
            ],
        });

        var inspection = session.Diagnostics.CaptureInspectionSnapshot();
        var snapshot = Assert.Single(inspection.PluginLoadSnapshots);
        var compatibility = GetCompatibility(snapshot);

        Assert.Equal(GraphEditorPluginLoadStatus.Blocked, snapshot.Status);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Incompatible, compatibility!.Status);
        Assert.Equal("compatibility.astergraph.minimum-version", compatibility.ReasonCode);
        Assert.NotNull(snapshot.TrustEvaluation);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginTrustEvaluationSource.ImplicitAllow, snapshot.TrustEvaluation.Source);
        Assert.Equal("trust.policy.not-configured", snapshot.TrustEvaluation.ReasonCode);
        Assert.Equal(GraphEditorPluginProvenanceEvidence.NotProvided, snapshot.ProvenanceEvidence);
        Assert.Equal(GraphEditorPluginSignatureStatus.NotProvided, snapshot.ProvenanceEvidence.Signature.Status);
        Assert.Null(snapshot.ProvenanceEvidence.PackageIdentity);
        Assert.False(snapshot.ActivationAttempted);
        Assert.Null(snapshot.Descriptor);
        Assert.Null(snapshot.FailureMessage);
        Assert.Contains(inspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.incompatible");
        Assert.Contains(diagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.incompatible");
        Assert.Equal(0, plugin.RegisterCallCount);
    }

    private static void SelectSourceNode(GraphEditorViewModel editor)
        => editor.SelectSingleNode(Assert.Single(editor.Nodes, node => node.Id == SourceNodeId), updateStatus: false);

    private static GraphEditorViewModel CreateEditor(DiagnosticsHarness harness)
        => AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = harness.CompatibilityService,
            WorkspaceService = harness.WorkspaceService,
            FragmentWorkspaceService = harness.FragmentWorkspaceService,
            FragmentLibraryService = harness.FragmentLibraryService,
            ClipboardPayloadSerializer = harness.ClipboardPayloadSerializer,
            DiagnosticsSink = harness.DiagnosticsSink,
            ContextMenuAugmentor = harness.ContextMenuAugmentor,
        });

    private static IGraphEditorSession CreateSession(DiagnosticsHarness harness)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = harness.CompatibilityService,
            WorkspaceService = harness.WorkspaceService,
            FragmentWorkspaceService = harness.FragmentWorkspaceService,
            FragmentLibraryService = harness.FragmentLibraryService,
            ClipboardPayloadSerializer = harness.ClipboardPayloadSerializer,
            DiagnosticsSink = harness.DiagnosticsSink,
            ContextMenuAugmentor = harness.ContextMenuAugmentor,
        });

    private static DiagnosticsHarness CreateHarness(
        IGraphWorkspaceService? workspaceService = null,
        IGraphFragmentWorkspaceService? fragmentWorkspaceService = null,
        IGraphContextMenuAugmentor? contextMenuAugmentor = null)
        => new(
            new ExactCompatibilityService(),
            workspaceService ?? new SequencedWorkspaceService("workspace://diagnostics"),
            fragmentWorkspaceService ?? new RecordingFragmentWorkspaceService("fragment://diagnostics"),
            new RecordingFragmentLibraryService("library://diagnostics"),
            new RecordingClipboardPayloadSerializer(),
            new RecordingDiagnosticsSink(),
            contextMenuAugmentor);

    private static GraphEditorPluginCompatibilityEvaluation? GetCompatibility(GraphEditorPluginLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var property = typeof(GraphEditorPluginLoadSnapshot).GetProperty("Compatibility");
        Assert.NotNull(property);
        return Assert.IsType<GraphEditorPluginCompatibilityEvaluation>(property!.GetValue(snapshot));
    }

    private static GraphDocument CreateDocument()
        => new(
            "Diagnostics Graph",
            "Phase 5 inspection regression coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Diagnostics Source",
                    "Tests",
                    "Diagnostics",
                    "Produces a float output.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [
                        new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
                    ],
                    "#55D8C1",
                    SourceDefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Diagnostics Target",
                    "Tests",
                    "Diagnostics",
                    "Consumes a float input.",
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
            "Diagnostics Source",
            "Tests",
            "Diagnostics",
            [],
            [
                new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#55D8C1"),
            ]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Diagnostics Target",
            "Tests",
            "Diagnostics",
            [
                new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
            ],
            []));
        return catalog;
    }

    private sealed record DiagnosticsHarness(
        ExactCompatibilityService CompatibilityService,
        IGraphWorkspaceService WorkspaceService,
        IGraphFragmentWorkspaceService FragmentWorkspaceService,
        IGraphFragmentLibraryService FragmentLibraryService,
        IGraphClipboardPayloadSerializer ClipboardPayloadSerializer,
        RecordingDiagnosticsSink DiagnosticsSink,
        IGraphContextMenuAugmentor? ContextMenuAugmentor);

    private sealed class SequencedWorkspaceService(string workspacePath, int failSaveCount = 0) : IGraphWorkspaceService
    {
        private int _remainingFailSaveCount = failSaveCount;

        public string WorkspacePath { get; } = workspacePath;

        public int SaveCalls { get; private set; }

        public GraphDocument? LastSaved { get; private set; }

        public void Save(GraphDocument document)
        {
            SaveCalls++;
            if (_remainingFailSaveCount > 0)
            {
                _remainingFailSaveCount--;
                throw new InvalidOperationException("diagnostics save failed on purpose");
            }

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

        public GraphSelectionFragment? LastSaved { get; private set; }

        public void Save(GraphSelectionFragment fragment, string? path = null)
            => LastSaved = fragment;

        public GraphSelectionFragment Load(string? path = null)
            => LastSaved ?? throw new InvalidOperationException("No fragment saved.");

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
        public string Serialize(GraphSelectionFragment fragment)
            => "diagnostics-fragment";

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

    private sealed class ExactCompatibilityService : IPortCompatibilityService
    {
        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
            => sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
    }

    private sealed class ThrowingAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => throw new InvalidOperationException("augmentor exploded");
    }

    private sealed class IncompatibleTrackingPlugin : IGraphEditorPlugin
    {
        public int RegisterCallCount { get; private set; }

        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.diagnostics.incompatible-plugin", "Diagnostics Incompatible Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            RegisterCallCount++;
        }
    }
}
