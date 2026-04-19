using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Plugins.Internal;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Microsoft.Extensions.Logging;
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
    public void RuntimeSessionProof_CoversProgressiveNodeSurfaceAndEditorOnlyGroups()
    {
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
        });

        session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);

        Assert.True(session.Commands.TrySetNodeWidth(TargetNodeId, 336d, updateStatus: false));
        Assert.True(session.Commands.TrySetNodeExpansionState(TargetNodeId, GraphNodeExpansionState.Expanded));

        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Proof Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));
        Assert.True(session.Commands.TrySetNodeGroupCollapsed(groupId, isCollapsed: true));
        Assert.True(session.Commands.TrySetNodeGroupPosition(groupId, new GraphPoint(72, 88), moveMemberNodes: true, updateStatus: false));

        var surface = Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == TargetNodeId);
        var group = Assert.Single(session.Queries.GetNodeGroups());
        var featureIds = session.Queries.GetFeatureDescriptors()
            .Where(descriptor => descriptor.IsAvailable)
            .Select(descriptor => descriptor.Id)
            .ToHashSet(StringComparer.Ordinal);
        var commandIds = session.Queries.GetCommandDescriptors()
            .Select(descriptor => descriptor.Id)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(336d, surface.Size.Width);
        Assert.Equal(GraphNodeExpansionState.Expanded, surface.ExpansionState);
        Assert.Equal(groupId, surface.GroupId);
        Assert.Equal("Proof Cluster", group.Title);
        Assert.True(group.IsCollapsed);
        Assert.Equal(new GraphPoint(72, 88), group.Position);
        Assert.Equal(
            [SourceNodeId, TargetNodeId],
            group.NodeIds.OrderBy(id => id, StringComparer.Ordinal));
        Assert.Contains("query.node-surface-snapshots", featureIds);
        Assert.Contains("query.node-groups", featureIds);
        Assert.Contains("groups.create", commandIds);
        Assert.Contains("groups.collapse", commandIds);
        Assert.Contains("groups.move", commandIds);
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
#pragma warning disable CS0618
    public void RetainedCompatibilityProof_ProjectsCompatibleTargetsBackToRetainedFacadeInstances()
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

        var legacyTarget = Assert.Single(legacyEditor.GetCompatibleTargets(SourceNodeId, SourcePortId));
        var factoryTarget = Assert.Single(factoryEditor.GetCompatibleTargets(SourceNodeId, SourcePortId));
        var legacyRetainedNode = Assert.IsType<NodeViewModel>(legacyEditor.FindNode(TargetNodeId));
        var factoryRetainedNode = Assert.IsType<NodeViewModel>(factoryEditor.FindNode(TargetNodeId));
        var legacyRetainedPort = Assert.IsType<PortViewModel>(legacyRetainedNode.GetPort(TargetPortId));
        var factoryRetainedPort = Assert.IsType<PortViewModel>(factoryRetainedNode.GetPort(TargetPortId));

        Assert.Same(legacyRetainedNode, legacyTarget.Node);
        Assert.Same(factoryRetainedNode, factoryTarget.Node);
        Assert.Same(legacyRetainedPort, legacyTarget.Port);
        Assert.Same(factoryRetainedPort, factoryTarget.Port);
    }
#pragma warning restore CS0618

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

    [Fact]
    public void RuntimeAndRetainedProof_ExposeEquivalentReadinessSeamDescriptors()
    {
        using var activitySource = new System.Diagnostics.ActivitySource("tests.proof.readiness");
        using var loggerFactory = new NoOpLoggerFactory();
        var options = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            WorkspaceService = new RecordingWorkspaceService(),
            FragmentWorkspaceService = new RecordingFragmentWorkspaceService("fragment://proof-ring"),
            FragmentLibraryService = new RecordingFragmentLibraryService("library://proof-ring"),
            ClipboardPayloadSerializer = new RecordingClipboardPayloadSerializer(),
            ContextMenuAugmentor = new ProofContextMenuAugmentor(),
            NodePresentationProvider = new ProofPresentationProvider(),
            LocalizationProvider = new ProofLocalizationProvider(),
            DiagnosticsSink = new RecordingDiagnosticsSink(),
            Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
            PluginTrustPolicy = new ProofAllowTrustPolicy(),
        };
        var retainedEditor = AsterGraphEditorFactory.Create(options);
        var runtimeSession = AsterGraphEditorFactory.CreateSession(options);
        var retainedDescriptors = CaptureReadinessDescriptors(retainedEditor.Session);
        var runtimeDescriptors = CaptureReadinessDescriptors(runtimeSession);

        Assert.Equal(runtimeDescriptors, retainedDescriptors);
        Assert.Equal(ReadinessFeatureIds.Length, runtimeDescriptors.Count);
        Assert.All(runtimeDescriptors, descriptor => Assert.True(descriptor.IsAvailable));
    }

    [Fact]
    public void RuntimeAndRetainedProof_ExposeEquivalentAutomationExecutionSemantics()
    {
        var retainedEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
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

        var retainedEvidence = ExecuteAutomationProof(retainedEditor.Session, "proof-automation");
        var runtimeEvidence = ExecuteAutomationProof(runtimeSession, "proof-automation");

        AssertAutomationEvidence(retainedEvidence);
        AssertAutomationEvidence(runtimeEvidence);
        Assert.Equal(retainedEvidence.ProgressCommandIds, runtimeEvidence.ProgressCommandIds);
        Assert.Equal(retainedEvidence.GenericCommandIds, runtimeEvidence.GenericCommandIds);
        Assert.Equal(retainedEvidence.DiagnosticCodes, runtimeEvidence.DiagnosticCodes);
        Assert.Equal(CaptureAutomationResultSignature(retainedEvidence.Result), CaptureAutomationResultSignature(runtimeEvidence.Result));
    }

    [Fact]
    public void RuntimeAndRetainedProof_ExposeAlignedHistorySaveContract()
    {
        var workspace = new RecordingWorkspaceService();
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            WorkspaceService = workspace,
        });
        var session = editor.Session;
        var sourceNode = Assert.IsType<NodeViewModel>(editor.FindNode(SourceNodeId));
        var targetNode = Assert.IsType<NodeViewModel>(editor.FindNode(TargetNodeId));
        var originPositions = new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
        {
            [SourceNodeId] = new GraphPoint(sourceNode.X, sourceNode.Y),
            [TargetNodeId] = new GraphPoint(targetNode.X, targetNode.Y),
        };

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);

        editor.BeginHistoryInteraction();
        editor.ApplyDragOffset(originPositions, 42, 18);
        editor.CompleteHistoryInteraction("Proof move complete.");
        var dirtyAfterMove = editor.IsDirty;

        editor.SaveWorkspace();
        var dirtyAfterSave = editor.IsDirty;

        editor.Undo();
        var dirtyAfterUndo = editor.IsDirty;
        var undoneSource = Assert.IsType<NodeViewModel>(editor.FindNode(SourceNodeId));
        var undoneTarget = Assert.IsType<NodeViewModel>(editor.FindNode(TargetNodeId));
        Assert.Single(editor.CreateDocumentSnapshot().Connections);
        Assert.Equal(originPositions[SourceNodeId].X, undoneSource.X);
        Assert.Equal(originPositions[SourceNodeId].Y, undoneSource.Y);
        Assert.Equal(originPositions[TargetNodeId].X, undoneTarget.X);
        Assert.Equal(originPositions[TargetNodeId].Y, undoneTarget.Y);

        editor.Redo();
        var dirtyAfterRedo = editor.IsDirty;
        var redoneSource = Assert.IsType<NodeViewModel>(editor.FindNode(SourceNodeId));
        var redoneTarget = Assert.IsType<NodeViewModel>(editor.FindNode(TargetNodeId));
        Assert.Single(editor.CreateDocumentSnapshot().Connections);
        Assert.Equal(originPositions[SourceNodeId].X + 42, redoneSource.X);
        Assert.Equal(originPositions[SourceNodeId].Y + 18, redoneSource.Y);
        Assert.Equal(originPositions[TargetNodeId].X + 42, redoneTarget.X);
        Assert.Equal(originPositions[TargetNodeId].Y + 18, redoneTarget.Y);

        Assert.True(dirtyAfterMove);
        Assert.False(dirtyAfterSave);
        Assert.True(dirtyAfterUndo);
        Assert.False(dirtyAfterRedo);
        Assert.True(workspace.Exists());
    }

    [Fact]
    public void RuntimeAndRetainedProof_ExposeEquivalentDirectPluginCompositionAndAutomationProof()
    {
        var options = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            ContextMenuAugmentor = new ProofContextMenuAugmentor(),
            NodePresentationProvider = new ProofPresentationProvider(),
            LocalizationProvider = new ProofLocalizationProvider(),
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(
                    new ProofPlugin(),
                    new GraphEditorPluginManifest(
                        "tests.proof.plugin",
                        "Proof Plugin",
                        new GraphEditorPluginManifestProvenance(
                            GraphEditorPluginManifestSourceKind.DirectRegistration,
                            typeof(ProofPlugin).FullName ?? nameof(ProofPlugin)),
                        description: "Proof-ring manifest coverage.",
                        version: "1.0.0",
                        compatibility: new GraphEditorPluginCompatibilityManifest(
                            minimumAsterGraphVersion: "0.0.0",
                            targetFramework: "net9.0",
                            runtimeSurface: "session-first"),
                        capabilitySummary: "menus, automation")),
            ],
            PluginTrustPolicy = new ProofAllowTrustPolicy(),
        };
        var retainedEditor = AsterGraphEditorFactory.Create(options);
        var runtimeSession = AsterGraphEditorFactory.CreateSession(options);
        retainedEditor.RefreshNodePresentations();

        var retainedSnapshots = retainedEditor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = runtimeSession.Queries.GetPluginLoadSnapshots();
        var retainedCanvasDescriptors = retainedEditor.Session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var runtimeCanvasDescriptors = runtimeSession.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var retainedSourceNode = Assert.Single(retainedEditor.Nodes, node => node.Id == SourceNodeId);
        var automationResult = runtimeSession.Automation.Execute(CreatePluginAutomationRunRequest("proof-plugin-automation"));

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        var pluginSnapshot = Assert.Single(runtimeSnapshots);
        var compatibility = GetCompatibility(pluginSnapshot);
        Assert.Equal(GraphEditorPluginLoadSourceKind.Direct, pluginSnapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Loaded, pluginSnapshot.Status);
        Assert.Equal("tests.proof.plugin", pluginSnapshot.Manifest!.Id);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Compatible, compatibility!.Status);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, pluginSnapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginTrustEvaluationSource.HostPolicy, pluginSnapshot.TrustEvaluation.Source);
        Assert.True(pluginSnapshot.ActivationAttempted);
        Assert.Equal("tests.proof.plugin", pluginSnapshot.Descriptor?.Id);
        Assert.Equal(1, pluginSnapshot.Contributions.NodeDefinitionProviderCount);
        Assert.Equal(1, pluginSnapshot.Contributions.ContextMenuAugmentorCount);
        Assert.Equal(1, pluginSnapshot.Contributions.NodePresentationProviderCount);
        Assert.Equal(1, pluginSnapshot.Contributions.LocalizationProviderCount);
        Assert.Equal("Proof Plugin Add Node", Assert.Single(runtimeCanvasDescriptors, descriptor => descriptor.Id == "canvas-add-node").Header);
        Assert.Equal("Proof Plugin Add Node", Assert.Single(retainedCanvasDescriptors, descriptor => descriptor.Id == "canvas-add-node").Header);
        Assert.Contains(runtimeCanvasDescriptors, descriptor => descriptor.Id == "proof-plugin-menu");
        Assert.Contains(retainedCanvasDescriptors, descriptor => descriptor.Id == "proof-plugin-menu");
        Assert.Contains(retainedSourceNode.Presentation.TopRightBadges, badge => badge.Text == "Plugin");
        Assert.Contains(automationResult.Inspection.Document.Nodes, node => node.DefinitionId is { Value: "tests.proof.plugin" });
        Assert.True(automationResult.Succeeded);
        Assert.Equal(3, automationResult.ExecutedStepCount);
    }

    [Fact]
    public void RuntimeAndRetainedProof_ExposeEquivalentStagedPackageSnapshotsAndInspectionParity()
    {
        var packageDirectory = Path.Combine(Path.GetTempPath(), "astergraph-proof-package-tests", Guid.NewGuid().ToString("N"));
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackageWithPluginPayload(
            packageDirectory,
            "AsterGraph.Proof.Stage",
            "1.0.0",
            title: "Proof Stage Package",
            description: "Proof-ring staged package parity coverage.",
            pluginAssemblyPath: GetSamplePluginAssemblyPath(),
            pluginTypeName: "AsterGraph.TestPlugins.SamplePlugin");
        var stageResult = AsterGraphEditorFactory.StagePluginPackage(new GraphEditorPluginPackageStageRequest(
            CreateTrustedPackageCandidate(packagePath),
            Path.Combine(packageDirectory, "staging-root")));
        var registration = Assert.IsType<GraphEditorPluginRegistration>(stageResult.Registration);
        var options = CreatePackageProofOptions(registration);
        var retainedEditor = AsterGraphEditorFactory.Create(options);
        var runtimeSession = AsterGraphEditorFactory.CreateSession(options);

        var retainedSnapshots = retainedEditor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = runtimeSession.Queries.GetPluginLoadSnapshots();
        var retainedInspection = retainedEditor.Session.Diagnostics.CaptureInspectionSnapshot();
        var runtimeInspection = runtimeSession.Diagnostics.CaptureInspectionSnapshot();

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        Assert.Equal(runtimeInspection.PluginLoadSnapshots, retainedInspection.PluginLoadSnapshots);
        var snapshot = Assert.Single(runtimeSnapshots);
        Assert.Equal(GraphEditorPluginLoadSourceKind.Package, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Loaded, snapshot.Status);
        Assert.Equal(packagePath, snapshot.Source);
        Assert.Equal(packagePath, snapshot.PackagePath);
        Assert.NotNull(snapshot.Stage);
        Assert.Equal(stageResult.Stage, snapshot.Stage);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginSignatureStatus.Valid, snapshot.ProvenanceEvidence.Signature.Status);
        Assert.True(snapshot.ActivationAttempted);
        Assert.Equal("tests.sample-plugin", snapshot.Descriptor?.Id);
        Assert.Contains(retainedInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.succeeded");
        Assert.Contains(runtimeInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.succeeded");
    }

    [Fact]
    public void RuntimeAndRetainedProof_ExposeEquivalentUnsignedPackageRefusalSnapshots()
    {
        var packageDirectory = Path.Combine(Path.GetTempPath(), "astergraph-proof-package-tests", Guid.NewGuid().ToString("N"));
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackage(
            packageDirectory,
            "AsterGraph.Proof.Unsigned",
            "1.0.0",
            title: "Proof Unsigned Package",
            description: "Proof-ring unsigned package refusal coverage.");
        var registration = CreateUnsignedPackageRegistration(packagePath);
        var options = CreatePackageProofOptions(registration);
        var retainedEditor = AsterGraphEditorFactory.Create(options);
        var runtimeSession = AsterGraphEditorFactory.CreateSession(options);

        var retainedSnapshots = retainedEditor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = runtimeSession.Queries.GetPluginLoadSnapshots();
        var retainedInspection = retainedEditor.Session.Diagnostics.CaptureInspectionSnapshot();
        var runtimeInspection = runtimeSession.Diagnostics.CaptureInspectionSnapshot();

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        Assert.Equal(runtimeInspection.PluginLoadSnapshots, retainedInspection.PluginLoadSnapshots);
        var snapshot = Assert.Single(runtimeSnapshots);
        Assert.Equal(GraphEditorPluginLoadSourceKind.Package, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Failed, snapshot.Status);
        Assert.Equal(packagePath, snapshot.Source);
        Assert.Equal(packagePath, snapshot.PackagePath);
        Assert.Equal(GraphEditorPluginSignatureStatus.Unsigned, snapshot.ProvenanceEvidence.Signature.Status);
        Assert.False(snapshot.ActivationAttempted);
        Assert.NotNull(snapshot.FailureMessage);
        Assert.Contains("StagePluginPackage", snapshot.FailureMessage, StringComparison.Ordinal);
        Assert.Contains(retainedInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.package-staging-required");
        Assert.Contains(runtimeInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.package-staging-required");
    }

    [Fact]
    public void RuntimeAndRetainedProof_ExposeEquivalentCompatibilityRefusalSnapshots()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var plugin = new ProofTrackingPlugin();
        var options = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            DiagnosticsSink = diagnostics,
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(
                    plugin,
                    new GraphEditorPluginManifest(
                        "tests.proof.incompatible-plugin",
                        "Proof Incompatible Plugin",
                        new GraphEditorPluginManifestProvenance(
                            GraphEditorPluginManifestSourceKind.DirectRegistration,
                            typeof(ProofTrackingPlugin).FullName ?? nameof(ProofTrackingPlugin)),
                        description: "Proof-ring incompatibility coverage.",
                        version: "1.0.0",
                        compatibility: new GraphEditorPluginCompatibilityManifest(
                            minimumAsterGraphVersion: "9999.0.0",
                            targetFramework: "net9.0",
                            runtimeSurface: "session-first"),
                        capabilitySummary: "menus")),
            ],
            PluginTrustPolicy = new ProofAllowTrustPolicy(),
        };
        var retainedEditor = AsterGraphEditorFactory.Create(options);
        var runtimeSession = AsterGraphEditorFactory.CreateSession(options);

        var retainedSnapshots = retainedEditor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = runtimeSession.Queries.GetPluginLoadSnapshots();
        var retainedInspection = retainedEditor.Session.Diagnostics.CaptureInspectionSnapshot();
        var runtimeInspection = runtimeSession.Diagnostics.CaptureInspectionSnapshot();

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        Assert.Equal(runtimeInspection.PluginLoadSnapshots, retainedInspection.PluginLoadSnapshots);
        var snapshot = Assert.Single(runtimeSnapshots);
        var compatibility = GetCompatibility(snapshot);
        Assert.Equal(GraphEditorPluginLoadStatus.Blocked, snapshot.Status);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Incompatible, compatibility!.Status);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.False(snapshot.ActivationAttempted);
        Assert.Contains(retainedInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.incompatible");
        Assert.Contains(runtimeInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.incompatible");
        Assert.Equal(0, plugin.RegisterCallCount);
    }

    [Fact]
    public void RuntimeAndRetainedProof_ExposeEquivalentTrustPolicyRefusalSnapshots()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var plugin = new ProofTrackingPlugin();
        var options = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            DiagnosticsSink = diagnostics,
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(
                    plugin,
                    new GraphEditorPluginManifest(
                        "tests.proof.blocked-plugin",
                        "Proof Blocked Plugin",
                        new GraphEditorPluginManifestProvenance(
                            GraphEditorPluginManifestSourceKind.DirectRegistration,
                            typeof(ProofTrackingPlugin).FullName ?? nameof(ProofTrackingPlugin)),
                        description: "Proof-ring blocked trust coverage.",
                        version: "1.0.0",
                        compatibility: new GraphEditorPluginCompatibilityManifest(
                            minimumAsterGraphVersion: "0.0.0",
                            targetFramework: "net9.0",
                            runtimeSurface: "session-first"),
                        capabilitySummary: "menus")),
            ],
            PluginTrustPolicy = new ProofBlockTrustPolicy("tests.proof.blocked-plugin"),
        };
        var retainedEditor = AsterGraphEditorFactory.Create(options);
        var runtimeSession = AsterGraphEditorFactory.CreateSession(options);

        var retainedSnapshots = retainedEditor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = runtimeSession.Queries.GetPluginLoadSnapshots();
        var retainedInspection = retainedEditor.Session.Diagnostics.CaptureInspectionSnapshot();
        var runtimeInspection = runtimeSession.Diagnostics.CaptureInspectionSnapshot();

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        Assert.Equal(runtimeInspection.PluginLoadSnapshots, retainedInspection.PluginLoadSnapshots);
        var snapshot = Assert.Single(runtimeSnapshots);
        var compatibility = GetCompatibility(snapshot);
        Assert.Equal(GraphEditorPluginLoadStatus.Blocked, snapshot.Status);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Compatible, compatibility!.Status);
        Assert.Equal(GraphEditorPluginTrustDecision.Blocked, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginTrustEvaluationSource.HostPolicy, snapshot.TrustEvaluation.Source);
        Assert.Equal("trust.blocked.proof", snapshot.TrustEvaluation.ReasonCode);
        Assert.False(snapshot.ActivationAttempted);
        Assert.Contains(retainedInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.blocked");
        Assert.Contains(runtimeInspection.RecentDiagnostics, diagnostic => diagnostic.Code == "plugin.load.blocked");
        Assert.Equal(0, plugin.RegisterCallCount);
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

    private static IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> CaptureReadinessDescriptors(IGraphEditorSession session)
        => session.Queries.GetFeatureDescriptors()
            .Where(descriptor => ReadinessFeatureIds.Contains(descriptor.Id, StringComparer.Ordinal))
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();

    private static AutomationExecutionEvidence ExecuteAutomationProof(IGraphEditorSession session, string runId)
    {
        var progressCommandIds = new List<string>();
        var genericCommandIds = new List<string>();
        var startedCount = 0;
        var completedCount = 0;

        session.Events.AutomationStarted += (_, _) => startedCount++;
        session.Events.AutomationProgress += (_, args) => progressCommandIds.Add(args.Step.CommandId);
        session.Events.AutomationCompleted += (_, _) => completedCount++;
        session.Events.CommandExecuted += (_, args) => genericCommandIds.Add(args.CommandId);

        var result = session.Automation.Execute(CreateAutomationRunRequest(runId));
        var diagnosticCodes = session.Diagnostics.GetRecentDiagnostics(16)
            .Select(diagnostic => diagnostic.Code)
            .Where(code => code.StartsWith("automation.", StringComparison.Ordinal))
            .ToArray();

        return new AutomationExecutionEvidence(
            result,
            progressCommandIds.ToArray(),
            genericCommandIds.ToArray(),
            diagnosticCodes,
            startedCount,
            completedCount);
    }

    private static void AssertAutomationEvidence(AutomationExecutionEvidence evidence)
    {
        Assert.Equal(1, evidence.StartedCount);
        Assert.Equal(1, evidence.CompletedCount);
        Assert.Equal(
            ["selection.set", "nodes.move", "viewport.resize", "viewport.pan", "connections.start", "connections.complete"],
            evidence.ProgressCommandIds);
        Assert.Equal(evidence.ProgressCommandIds, evidence.GenericCommandIds);
        Assert.Contains("automation.run.started", evidence.DiagnosticCodes);
        Assert.Contains("automation.run.completed", evidence.DiagnosticCodes);
        Assert.True(evidence.Result.Succeeded);
        Assert.True(evidence.Result.UsedMutationScope);
        Assert.Equal("proof-automation", evidence.Result.MutationLabel);
        Assert.Equal(6, evidence.Result.ExecutedStepCount);
        Assert.Equal(6, evidence.Result.TotalStepCount);
        Assert.Single(evidence.Result.Inspection.Document.Connections);
        Assert.Equal(SourceNodeId, evidence.Result.Inspection.Selection.PrimarySelectedNodeId);
    }

    private static AutomationResultSignature CaptureAutomationResultSignature(GraphEditorAutomationExecutionSnapshot result)
        => new(
            result.Succeeded,
            result.UsedMutationScope,
            result.MutationLabel,
            result.ExecutedStepCount,
            result.TotalStepCount,
            string.Join("|", result.Steps.Select(step => $"{step.CommandId}:{step.Succeeded}:{step.FailureCode ?? string.Empty}")),
            string.Join(
                "|",
                result.Inspection.NodePositions
                    .OrderBy(position => position.NodeId, StringComparer.Ordinal)
                    .Select(position => $"{position.NodeId}:{position.Position.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}:{position.Position.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}")),
            result.Inspection.Document.Connections.Count,
            result.Inspection.Selection.PrimarySelectedNodeId,
            result.Inspection.Viewport.ViewportWidth,
            result.Inspection.Viewport.ViewportHeight);

    private static GraphEditorAutomationRunRequest CreateAutomationRunRequest(string runId)
        => new(
            runId,
            [
                new GraphEditorAutomationStep("select-source", CreateAutomationCommand("selection.set", ("nodeId", SourceNodeId), ("primaryNodeId", SourceNodeId), ("updateStatus", "false"))),
                new GraphEditorAutomationStep("move-source", CreateAutomationCommand("nodes.move", ("position", $"{SourceNodeId}|300|210"), ("updateStatus", "false"))),
                new GraphEditorAutomationStep("resize", CreateAutomationCommand("viewport.resize", ("width", "1280"), ("height", "720"))),
                new GraphEditorAutomationStep("pan", CreateAutomationCommand("viewport.pan", ("deltaX", "12"), ("deltaY", "18"))),
                new GraphEditorAutomationStep("start-connection", CreateAutomationCommand("connections.start", ("sourceNodeId", SourceNodeId), ("sourcePortId", SourcePortId))),
                new GraphEditorAutomationStep("complete-connection", CreateAutomationCommand("connections.complete", ("targetNodeId", TargetNodeId), ("targetPortId", TargetPortId))),
            ]);

    private static GraphEditorAutomationRunRequest CreatePluginAutomationRunRequest(string runId)
        => new(
            runId,
            [
                new GraphEditorAutomationStep("select-source", CreateAutomationCommand("selection.set", ("nodeId", SourceNodeId), ("primaryNodeId", SourceNodeId), ("updateStatus", "false"))),
                new GraphEditorAutomationStep("add-plugin-node", CreateAutomationCommand("nodes.add", ("definitionId", "tests.proof.plugin"), ("worldX", "640"), ("worldY", "240"))),
                new GraphEditorAutomationStep("move-plugin-node", CreateAutomationCommand("nodes.move", ("position", "tests-proof-plugin-001|684|264"), ("updateStatus", "false"))),
            ]);

    private static GraphEditorCommandInvocationSnapshot CreateAutomationCommand(
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            commandId,
            arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToList());

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

    private static GraphEditorPluginCompatibilityEvaluation? GetCompatibility(GraphEditorPluginLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var property = typeof(GraphEditorPluginLoadSnapshot).GetProperty("Compatibility");
        Assert.NotNull(property);
        return Assert.IsType<GraphEditorPluginCompatibilityEvaluation>(property!.GetValue(snapshot));
    }

    private static AsterGraphEditorOptions CreatePackageProofOptions(GraphEditorPluginRegistration registration)
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            DiagnosticsSink = new RecordingDiagnosticsSink(),
            PluginRegistrations = [registration],
        };

    private static GraphEditorPluginRegistration CreateUnsignedPackageRegistration(string packagePath)
    {
        var inspection = AsterGraphPluginPackageArchiveInspector.Inspect(packagePath);
        return GraphEditorPluginRegistration.FromPackagePath(packagePath, inspection.Manifest, inspection.ProvenanceEvidence);
    }

    private static GraphEditorPluginCandidateSnapshot CreateTrustedPackageCandidate(string packagePath)
    {
        var inspection = AsterGraphPluginPackageArchiveInspector.Inspect(packagePath);
        return new GraphEditorPluginCandidateSnapshot(
            GraphEditorPluginCandidateSourceKind.PackageDirectory,
            Path.GetDirectoryName(packagePath) ?? packagePath,
            inspection.Manifest,
            AsterGraphPluginPreloadEvaluator.EvaluateCompatibility(inspection.Manifest),
            new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                "trust.allowed.proof.package",
                $"Allowed manifest '{inspection.Manifest.Id}' for package proof coverage."),
            new GraphEditorPluginProvenanceEvidence(
                inspection.ProvenanceEvidence.PackageIdentity,
                new GraphEditorPluginSignatureEvidence(
                    GraphEditorPluginSignatureStatus.Valid,
                    GraphEditorPluginSignatureKind.Repository,
                    new GraphEditorPluginSignerIdentity("AsterGraph Proof", "PROOF1234"),
                    timestampUtc: new DateTimeOffset(2026, 04, 09, 0, 0, 0, TimeSpan.Zero),
                    timestampAuthority: "tests.timestamp",
                    reasonCode: "signature.valid.proof-package",
                    reasonMessage: "Valid signature fixture for proof package coverage.")),
            packagePath: packagePath);
    }

    private static string GetSamplePluginAssemblyPath()
        => TestPluginArtifactPathHelper.GetSamplePluginAssemblyPath();

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

    private sealed class RecordingFragmentWorkspaceService(string fragmentPath) : IGraphFragmentWorkspaceService
    {
        public string FragmentPath { get; } = fragmentPath;

        public void Save(GraphSelectionFragment fragment, string? path = null)
        {
        }

        public GraphSelectionFragment Load(string? path = null)
            => throw new InvalidOperationException("No fragment snapshot.");

        public bool Exists(string? path = null)
            => false;

        public void Delete(string? path = null)
        {
        }
    }

    private sealed class RecordingFragmentLibraryService(string libraryPath) : IGraphFragmentLibraryService
    {
        public string LibraryPath { get; } = libraryPath;

        public IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates()
            => [];

        public string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
            => Path.Combine(LibraryPath, $"{name ?? "fragment"}.json");

        public GraphSelectionFragment LoadTemplate(string path)
            => throw new InvalidOperationException("No fragment template.");

        public void DeleteTemplate(string path)
        {
        }
    }

    private sealed class RecordingClipboardPayloadSerializer : IGraphClipboardPayloadSerializer
    {
        public string Serialize(GraphSelectionFragment fragment)
            => "serialized-fragment";

        public bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        {
            fragment = null;
            return false;
        }
    }

    private sealed class NoOpLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
            => new NoOpLogger();

        public void Dispose()
        {
        }
    }

    private sealed class NoOpLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }

    private sealed record SessionSharedSignature(
        string FeatureDescriptorSignature,
        string CommandDescriptorSignature,
        string CanvasMenuSignature);

    private sealed record AutomationExecutionEvidence(
        GraphEditorAutomationExecutionSnapshot Result,
        string[] ProgressCommandIds,
        string[] GenericCommandIds,
        string[] DiagnosticCodes,
        int StartedCount,
        int CompletedCount);

    private sealed record AutomationResultSignature(
        bool Succeeded,
        bool UsedMutationScope,
        string? MutationLabel,
        int ExecutedStepCount,
        int TotalStepCount,
        string StepSignature,
        string NodePositionSignature,
        int ConnectionCount,
        string? PrimarySelectedNodeId,
        double ViewportWidth,
        double ViewportHeight);

    private sealed record MigrationViewRouteSnapshot(
        GraphEditorViewChromeMode ChromeMode,
        bool EditorAssigned,
        bool CanvasAttachPlatformSeams,
        bool EnableDefaultContextMenu,
        bool EnableDefaultCommandShortcuts);

    private static readonly string[] SharedCanonicalCommandIds =
    [
        "nodes.add",
        "selection.set",
        "selection.delete",
        "nodes.resize-width",
        "nodes.surface.expand",
        "groups.create",
        "groups.collapse",
        "groups.move",
        "connections.start",
        "connections.complete",
        "connections.connect",
        "connections.cancel",
        "connections.delete",
        "connections.break-port",
        "nodes.move",
        "viewport.pan",
        "viewport.resize",
        "viewport.center",
        "viewport.fit",
        "viewport.reset",
        "viewport.center-node",
        "workspace.save",
        "workspace.load",
    ];

    private static readonly string[] ReadinessFeatureIds =
    [
        "query.plugin-load-snapshots",
        "surface.automation.runner",
        "event.automation.started",
        "event.automation.progress",
        "event.automation.completed",
        "service.workspace",
        "service.fragment-workspace",
        "service.fragment-library",
        "service.clipboard-payload-serializer",
        "service.diagnostics",
        "integration.plugin-loader",
        "integration.plugin-trust-policy",
        "integration.context-menu-augmentor",
        "integration.node-presentation-provider",
        "integration.localization-provider",
        "integration.diagnostics-sink",
        "integration.instrumentation.logger",
        "integration.instrumentation.activity-source",
    ];

    private sealed class ProofPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new(
            "tests.proof.plugin",
            "Proof Plugin",
            "Direct plugin used by proof-ring tests.");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddNodeDefinitionProvider(new ProofPluginNodeDefinitionProvider());
            builder.AddContextMenuAugmentor(new ProofPluginContextMenuAugmentor());
            builder.AddNodePresentationProvider(new ProofPluginPresentationProvider());
            builder.AddLocalizationProvider(new ProofPluginLocalizationProvider());
        }
    }

    private sealed class ProofTrackingPlugin : IGraphEditorPlugin
    {
        public int RegisterCallCount { get; private set; }

        public GraphEditorPluginDescriptor Descriptor { get; } = new(
            "tests.proof.incompatible-plugin",
            "Proof Incompatible Plugin",
            "Direct plugin used by incompatibility proof-ring tests.");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            RegisterCallCount++;
        }
    }

    private sealed class ProofAllowTrustPolicy : IGraphEditorPluginTrustPolicy
    {
        public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                "proof.policy.allow",
                $"Allowed manifest '{context.Manifest.Id}' for proof coverage.");
        }
    }

    private sealed class ProofBlockTrustPolicy(string blockedId) : IGraphEditorPluginTrustPolicy
    {
        public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return StringComparer.Ordinal.Equals(context.Manifest.Id, blockedId)
                ? new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Blocked,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.blocked.proof",
                    $"Blocked manifest '{context.Manifest.Id}' for proof coverage.")
                : new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Allowed,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.allowed.proof",
                    $"Allowed manifest '{context.Manifest.Id}' for proof coverage.");
        }
    }

    private sealed class ProofPluginNodeDefinitionProvider : INodeDefinitionProvider
    {
        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
            =>
            [
                new NodeDefinition(
                    new NodeDefinitionId("tests.proof.plugin"),
                    "Proof Plugin Node",
                    "Tests",
                    "Proof",
                    [new PortDefinition("input", "Input", new PortTypeId("float"), "#F3B36B")],
                    []),
            ];
    }

    private sealed class ProofPluginContextMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
    {
        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
            => context.StockItems
                .Concat(
                [
                    new GraphEditorMenuItemDescriptorSnapshot(
                        "proof-plugin-menu",
                        "Proof Plugin Menu",
                        iconKey: "plugin",
                        isEnabled: false),
                ])
                .ToList();
    }

    private sealed class ProofPluginPresentationProvider : IGraphEditorPluginNodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
            => new(
                TopRightBadges:
                [
                    new NodeAdornmentDescriptor("Plugin", "#6AD5C4"),
                ]);
    }

    private sealed class ProofPluginLocalizationProvider : IGraphEditorPluginLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => key == "editor.menu.canvas.addNode"
                ? "Proof Plugin Add Node"
                : fallback;
    }
}
