using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Microsoft.Extensions.Logging;

var runtimeCompatibility = new RecordingCompatibilityService();
var runtimeDiagnostics = new RecordingDiagnosticsSink();
using var runtimeLoggerFactory = new HostSampleSupport.RecordingLoggerFactory();
using var runtimeActivitySource = new ActivitySource("AsterGraph.HostSample.Runtime");
var runtimeActivities = new List<string>();
using var runtimeListener = HostSampleSupport.CreateListener(runtimeActivitySource.Name, runtimeActivities);
var runtimeWorkspace = new ThrowingWorkspaceService("workspace://host-sample/runtime");
var catalog = new NodeCatalog();
catalog.RegisterProvider(new HostSampleNodeDefinitionProvider());
var document = CreateDocument();
var style = GraphEditorStyleOptions.Default with
{
    Shell = GraphEditorStyleOptions.Default.Shell with
    {
        HighlightHex = "#F3B36B",
        LibraryPanelWidth = 312,
    },
    ContextMenu = GraphEditorStyleOptions.Default.ContextMenu with
    {
        BackgroundHex = "#102332",
    },
};
var permissions = GraphEditorCommandPermissions.Default with
{
    Host = new HostCommandPermissions
    {
        AllowContextMenuExtensions = true,
    },
};
var viewBehavior = GraphEditorBehaviorOptions.Default with
{
    Commands = permissions,
};
var runtimeBehavior = GraphEditorBehaviorOptions.Default;
const int ReadinessFeatureCount = 17;
const string HostSamplePluginDefinitionIdValue = "host.sample.plugin";
const string HostSamplePluginMenuId = "host-sample-plugin-menu";
const string HostSampleAutomationRunId = "host-sample-automation";
const string HostSampleTrustedManifestId = "host.sample.trusted-plugin";
const string HostSampleBlockedManifestId = "host.sample.blocked-plugin";
var blockedPlugin = new HostSampleBlockedPlugin();
var pluginTrustPolicy = new HostSampleManifestTrustPolicy(HostSampleBlockedManifestId);
var pluginRegistrations =
    new[]
    {
        GraphEditorPluginRegistration.FromPlugin(
            new HostSampleProofPlugin(),
            CreateHostSampleManifest(
                HostSampleTrustedManifestId,
                "Host Sample Trusted Plugin",
                typeof(HostSampleProofPlugin),
                "menus, automation")),
        GraphEditorPluginRegistration.FromPlugin(
            blockedPlugin,
            CreateHostSampleManifest(
                HostSampleBlockedManifestId,
                "Host Sample Blocked Plugin",
                typeof(HostSampleBlockedPlugin),
                "menus")),
    };

var runtimeSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = runtimeCompatibility,
    WorkspaceService = runtimeWorkspace,
    ContextMenuAugmentor = new HostSampleAugmentor(),
    NodePresentationProvider = new HostSamplePresentationProvider(),
    LocalizationProvider = new HostSampleLocalizationProvider(),
    DiagnosticsSink = runtimeDiagnostics,
    Instrumentation = new GraphEditorInstrumentationOptions(runtimeLoggerFactory, runtimeActivitySource),
    StyleOptions = style,
    BehaviorOptions = runtimeBehavior,
    PluginRegistrations = pluginRegistrations,
    PluginTrustPolicy = pluginTrustPolicy,
});

var commandMarkers = new List<string>();
var documentChangeKinds = new List<string>();
var viewportChanges = 0;
var pendingConnectionChanges = 0;
var automationStartedCount = 0;
var automationCompletedCount = 0;
var automationProgressCommandIds = new List<string>();
var automationGenericCommandIds = new List<string>();
GraphEditorRecoverableFailureEventArgs? runtimeFailure = null;

runtimeSession.Events.CommandExecuted += (_, args) =>
{
    commandMarkers.Add($"{args.CommandId}:{args.MutationLabel ?? "<none>"}:{args.IsInMutationScope}");
    if (string.Equals(args.MutationLabel, HostSampleAutomationRunId, StringComparison.Ordinal))
    {
        automationGenericCommandIds.Add(args.CommandId);
    }
};
runtimeSession.Events.DocumentChanged += (_, args) => documentChangeKinds.Add(args.ChangeKind.ToString());
runtimeSession.Events.ViewportChanged += (_, _) => viewportChanges++;
runtimeSession.Events.PendingConnectionChanged += (_, _) => pendingConnectionChanges++;
runtimeSession.Events.RecoverableFailure += (_, args) => runtimeFailure = args;
runtimeSession.Events.AutomationStarted += (_, args) =>
{
    if (string.Equals(args.RunId, HostSampleAutomationRunId, StringComparison.Ordinal))
    {
        automationStartedCount++;
    }
};
runtimeSession.Events.AutomationProgress += (_, args) =>
{
    if (string.Equals(args.RunId, HostSampleAutomationRunId, StringComparison.Ordinal))
    {
        automationProgressCommandIds.Add(args.Step.CommandId);
    }
};
runtimeSession.Events.AutomationCompleted += (_, args) =>
{
    if (string.Equals(args.Result.RunId, HostSampleAutomationRunId, StringComparison.Ordinal))
    {
        automationCompletedCount++;
    }
};

runtimeSession.Commands.UpdateViewportSize(1280, 720);
runtimeSession.Commands.SetSelection(["sample-source-001"], "sample-source-001", updateStatus: false);
runtimeSession.Commands.SetNodePositions(
    [
        new NodePositionSnapshot("sample-source-001", new GraphPoint(144, 180)),
        new NodePositionSnapshot("sample-sink-001", new GraphPoint(456, 180)),
    ],
    updateStatus: false);
var compatibleTargets = runtimeSession.Queries.GetCompatiblePortTargets("sample-source-001", "result");
runtimeSession.Commands.StartConnection("sample-source-001", "result");
runtimeSession.Commands.CancelPendingConnection();
using (runtimeSession.BeginMutation("host-sample-batch"))
{
    runtimeSession.Commands.AddNode(new NodeDefinitionId("host.sample.sink"), new GraphPoint(680, 200));
    runtimeSession.Commands.StartConnection("sample-source-001", "result");
    runtimeSession.Commands.CompleteConnection("sample-sink-001", "input");
    runtimeSession.Commands.CenterViewOnNode("sample-sink-001");
    runtimeSession.Commands.PanBy(12, 18);
}
var runtimeAutomationResult = runtimeSession.Automation.Execute(CreateAutomationRunRequest(HostSampleAutomationRunId, HostSamplePluginDefinitionIdValue));
runtimeSession.Commands.SetSelection(["sample-sink-001"], "sample-sink-001", updateStatus: false);
runtimeSession.Commands.SaveWorkspace();

var runtimeSnapshot = runtimeSession.Queries.CreateDocumentSnapshot();
var runtimeViewport = runtimeSession.Queries.GetViewportSnapshot();
var runtimeCapabilities = runtimeSession.Queries.GetCapabilitySnapshot();
var runtimeCommandDescriptors = runtimeSession.Queries.GetCommandDescriptors();
var runtimeCanvasDescriptors = runtimeSession.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(200, 120)));
var runtimePluginLoadSnapshots = runtimeSession.Queries.GetPluginLoadSnapshots();
var runtimeInspection = runtimeSession.Diagnostics.CaptureInspectionSnapshot();
var runtimeRecentDiagnostics = runtimeSession.Diagnostics.GetRecentDiagnostics(10);
var runtimeReadinessDescriptors = CaptureReadinessDescriptors(runtimeSession);
var runtimeAutomationDescriptorsOk = runtimeInspection.FeatureDescriptors.Any(descriptor => descriptor.Id == "query.feature-descriptors" && descriptor.IsAvailable)
    && runtimeInspection.FeatureDescriptors.Any(descriptor => descriptor.Id == "surface.mutation.batch" && descriptor.IsAvailable);
var phase18ReadinessOk = runtimeReadinessDescriptors.Count == ReadinessFeatureCount
    && runtimeReadinessDescriptors.All(descriptor => descriptor.IsAvailable);
var expectedAutomationCommandIds = new[]
{
    "selection.set",
    "nodes.add",
    "nodes.move",
    "viewport.resize",
    "viewport.pan",
    "connections.start",
    "connections.complete",
};
var runtimeAutomationDiagnosticCodes = runtimeRecentDiagnostics
    .Where(diagnostic => diagnostic.Code.StartsWith("automation.", StringComparison.Ordinal))
    .Select(diagnostic => diagnostic.Code)
    .ToArray();
var runtimePluginMenuOk = runtimeCanvasDescriptors.Any(descriptor => descriptor.Id == HostSamplePluginMenuId);
var runtimePluginLocalizationOk = runtimeCanvasDescriptors.Any(descriptor => descriptor.Id == "canvas-add-node" && descriptor.Header == "Plugin Add Node");
var runtimeTrustedPluginSnapshot = FindPluginLoadSnapshot(runtimePluginLoadSnapshots, HostSampleTrustedManifestId);
var runtimeBlockedPluginSnapshot = FindPluginLoadSnapshot(runtimePluginLoadSnapshots, HostSampleBlockedManifestId);
var phase25AutomationHostOk = runtimeAutomationResult.Succeeded
    && automationStartedCount == 1
    && automationCompletedCount == 1
    && automationProgressCommandIds.SequenceEqual(expectedAutomationCommandIds, StringComparer.Ordinal)
    && automationGenericCommandIds.SequenceEqual(expectedAutomationCommandIds, StringComparer.Ordinal)
    && runtimeAutomationDiagnosticCodes.Contains("automation.run.started", StringComparer.Ordinal)
    && runtimeAutomationDiagnosticCodes.Contains("automation.run.completed", StringComparer.Ordinal)
    && runtimeAutomationResult.Inspection.Document.Nodes.Any(node => node.DefinitionId is { Value: HostSamplePluginDefinitionIdValue });
var phase29TrustHostOk = runtimePluginLoadSnapshots.Count == 2
    && runtimeTrustedPluginSnapshot.SourceKind == GraphEditorPluginLoadSourceKind.Direct
    && runtimeTrustedPluginSnapshot.Status == GraphEditorPluginLoadStatus.Loaded
    && runtimeTrustedPluginSnapshot.Compatibility.Status == GraphEditorPluginCompatibilityStatus.Compatible
    && runtimeTrustedPluginSnapshot.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Allowed
    && runtimeTrustedPluginSnapshot.Descriptor?.Id == "host.sample.plugin"
    && runtimeTrustedPluginSnapshot.Contributions.NodeDefinitionProviderCount == 1
    && runtimeTrustedPluginSnapshot.Contributions.ContextMenuAugmentorCount == 1
    && runtimeTrustedPluginSnapshot.Contributions.NodePresentationProviderCount == 1
    && runtimeTrustedPluginSnapshot.Contributions.LocalizationProviderCount == 1
    && runtimePluginMenuOk
    && runtimePluginLocalizationOk
    && runtimeBlockedPluginSnapshot.SourceKind == GraphEditorPluginLoadSourceKind.Direct
    && runtimeBlockedPluginSnapshot.Status == GraphEditorPluginLoadStatus.Blocked
    && runtimeBlockedPluginSnapshot.Compatibility.Status == GraphEditorPluginCompatibilityStatus.Compatible
    && runtimeBlockedPluginSnapshot.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Blocked
    && runtimeBlockedPluginSnapshot.TrustEvaluation.Source == GraphEditorPluginTrustEvaluationSource.HostPolicy
    && runtimeBlockedPluginSnapshot.TrustEvaluation.ReasonCode == "trust.blocked.host-sample"
    && !runtimeBlockedPluginSnapshot.ActivationAttempted
    && runtimeBlockedPluginSnapshot.Descriptor is null
    && blockedPlugin.RegisterCallCount == 0
    && runtimeDiagnostics.Diagnostics.Any(diagnostic => diagnostic.Code == "plugin.load.blocked");
var runtimeSessionIsKernelFirst = !runtimeSession
    .GetType()
    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
    .Any(field => field.FieldType == typeof(GraphEditorViewModel));
var inspectorProofEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
});
inspectorProofEditor.SelectSingleNode(AssertNode(inspectorProofEditor, "sample-source-001"), updateStatus: false);
inspectorProofEditor.ConnectPorts("sample-source-001", "result", "sample-sink-001", "input");
var historyProofEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
});
var historyProofNode = AssertNode(historyProofEditor, "sample-source-001");
var historyProofOrigin = new GraphPoint(historyProofNode.X, historyProofNode.Y);
historyProofEditor.BeginHistoryInteraction();
historyProofEditor.ApplyDragOffset(
    new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
    {
        ["sample-source-001"] = historyProofOrigin,
    },
    36,
    18);
historyProofEditor.CompleteHistoryInteraction("Host sample state scaling proof.");
var historyDirtyAfterMove = historyProofEditor.IsDirty;
var historyCanUndoAfterMove = historyProofEditor.CanUndo;
historyProofEditor.Undo();
var historyRestoredNode = AssertNode(historyProofEditor, "sample-source-001");

Console.WriteLine($"Session title: {runtimeSnapshot.Title}");
Console.WriteLine($"Session node count after commands: {runtimeSnapshot.Nodes.Count}");
Console.WriteLine($"Session compatible targets: {compatibleTargets.Count}");
Console.WriteLine($"Session DTO compatible target node: {compatibleTargets[0].NodeId}");
Console.WriteLine($"Session command markers: {string.Join(", ", commandMarkers)}");
Console.WriteLine($"Session document changes: {string.Join(", ", documentChangeKinds)}");
Console.WriteLine($"Session viewport events: {viewportChanges}");
Console.WriteLine($"Session pending events: {pendingConnectionChanges}");
Console.WriteLine($"Session viewport snapshot: zoom={runtimeViewport.Zoom:0.00}, pan={runtimeViewport.PanX:0},{runtimeViewport.PanY:0}");
Console.WriteLine($"Session capabilities: save={runtimeCapabilities.CanSaveWorkspace}, load={runtimeCapabilities.CanLoadWorkspace}");
Console.WriteLine($"Session recoverable failure: {runtimeFailure?.Code ?? "<none>"}");
Console.WriteLine($"Diagnostics sink codes: {string.Join(", ", runtimeDiagnostics.Diagnostics.Select(diagnostic => diagnostic.Code))}");
Console.WriteLine($"Runtime compatibility evaluations: {runtimeCompatibility.EvaluateCalls}");
Console.WriteLine($"Runtime workspace override path: {runtimeWorkspace.WorkspacePath}");
Console.WriteLine($"Diagnostics inspection snapshot: nodes={runtimeInspection.Document.Nodes.Count}, selected={runtimeInspection.Selection.SelectedNodeIds.Count}, pending={runtimeInspection.PendingConnection.HasPendingConnection}, status={runtimeInspection.Status.Message}");
Console.WriteLine($"Diagnostics recent history: {string.Join(" | ", runtimeRecentDiagnostics.Select(diagnostic => $"{diagnostic.Code}:{diagnostic.Severity}"))}");
Console.WriteLine($"Diagnostics logger entries: {string.Join(" | ", runtimeLoggerFactory.Entries.Select(entry => $"{entry.Level}:{entry.Message}"))}");
Console.WriteLine($"Diagnostics Activity operations: {string.Join(", ", runtimeActivities)}");
Console.WriteLine($"Session runtime workflow: selection={runtimeSession.Queries.GetSelectionSnapshot().PrimarySelectedNodeId}, connections={runtimeSnapshot.Connections.Count}");
Console.WriteLine($"Session backend: kernel-first={runtimeSessionIsKernelFirst}");
Console.WriteLine($"Descriptor runtime: nodes.add={runtimeCommandDescriptors.Any(descriptor => descriptor.Id == "nodes.add" && descriptor.IsEnabled)}, canvas={string.Join(",", runtimeCanvasDescriptors.Select(descriptor => descriptor.Id))}");
Console.WriteLine($"Readiness seam descriptors: {string.Join(",", runtimeReadinessDescriptors.Where(descriptor => descriptor.IsAvailable).Select(descriptor => descriptor.Id))}");
Console.WriteLine($"Readiness automation boundary: featureDescriptors={runtimeAutomationDescriptorsOk}, diagnostics={runtimeRecentDiagnostics.Count}, inspectionFeatures={runtimeInspection.FeatureDescriptors.Count}");
Console.WriteLine($"Plugin load snapshots: {string.Join(" | ", runtimePluginLoadSnapshots.Select(snapshot => $"{snapshot.Manifest.Id}:{snapshot.Status}:{snapshot.TrustEvaluation.Decision}:{snapshot.Compatibility.Status}:{snapshot.ActivationAttempted}"))}");
Console.WriteLine($"Automation proof commands: {string.Join(",", automationProgressCommandIds)}");
Console.WriteLine($"State scaling proof: inspector={inspectorProofEditor.InspectorConnections}, downstream={inspectorProofEditor.InspectorDownstream}, dirtyAfterMove={historyDirtyAfterMove}, canUndoAfterMove={historyCanUndoAfterMove}, dirtyAfterUndo={historyProofEditor.IsDirty}, restored=({historyRestoredNode.X:0},{historyRestoredNode.Y:0})");

var viewCompatibility = new RecordingCompatibilityService();
var viewDiagnostics = new RecordingDiagnosticsSink();
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = viewCompatibility,
    DiagnosticsSink = viewDiagnostics,
    StyleOptions = style,
    BehaviorOptions = viewBehavior,
    ContextMenuAugmentor = new HostSampleAugmentor(),
    NodePresentationProvider = new HostSamplePresentationProvider(),
    LocalizationProvider = new HostSampleLocalizationProvider(),
    PluginRegistrations = pluginRegistrations,
    PluginTrustPolicy = pluginTrustPolicy,
});
var retainedCanvasDescriptorMenu = editor.Session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(32, 48)));
var retainedCanvasMenu = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(32, 48)));
var retainedDescriptorMenuBacked = retainedCanvasDescriptorMenu.All(
    descriptor => retainedCanvasMenu.Any(item => item.Id == descriptor.Id));

editor.DocumentChanged += (_, args) =>
    Console.WriteLine($"DocumentChanged subscribed: {args.ChangeKind}");
editor.SelectionChanged += (_, args) =>
    Console.WriteLine($"SelectionChanged subscribed: {args.SelectedNodeIds.Count} node(s)");
editor.ViewportChanged += (_, args) =>
    Console.WriteLine($"ViewportChanged subscribed: {args.Zoom:0.00}");
editor.FragmentExported += (_, args) =>
    Console.WriteLine($"FragmentExported subscribed: {args.Path}");
editor.FragmentImported += (_, args) =>
    Console.WriteLine($"FragmentImported subscribed: {args.Path}");

var positions = editor.GetNodePositions();
editor.TryGetNodePosition("sample-source-001", out var snapshot);
var retainedSession = editor.Session;

var hostContext = new HostSampleGraphHostContext(
    new HostSampleOwner("owner-001", "Host Shell"),
    new HostSampleTopLevel("shell-001", "Primary Tool Window"));
var menuContext = new ContextMenuContext(
    ContextMenuTargetKind.Node,
    new GraphPoint(120, 160),
    selectedNodeId: "sample-source-001",
    selectedNodeIds: ["sample-source-001"],
    clickedNodeId: "sample-source-001",
    hostContext: hostContext);
var menu = editor.BuildContextMenu(menuContext);
var hostPreviewItem = menu.SingleOrDefault(item => item.Id == "host-sample-preview");

var node = AssertNode(editor, "sample-source-001");
editor.SelectSingleNode(node, updateStatus: false);
retainedSession.Commands.SaveWorkspace();
editor.ExportSelectionFragment();
var retainedPluginLoadSnapshots = retainedSession.Queries.GetPluginLoadSnapshots();
var retainedPluginMenuOk = retainedCanvasDescriptorMenu.Any(descriptor => descriptor.Id == HostSamplePluginMenuId);
var retainedPluginLocalizationOk = retainedCanvasDescriptorMenu.Any(descriptor => descriptor.Id == "canvas-add-node" && descriptor.Header == "Plugin Add Node");
var retainedPluginBadgeOk = node.Presentation.TopRightBadges.Any(badge => badge.Text == "Plugin");
var retainedTrustedPluginSnapshot = FindPluginLoadSnapshot(retainedPluginLoadSnapshots, HostSampleTrustedManifestId);
var retainedBlockedPluginSnapshot = FindPluginLoadSnapshot(retainedPluginLoadSnapshots, HostSampleBlockedManifestId);
var retainedInspection = retainedSession.Diagnostics.CaptureInspectionSnapshot();
var retainedRecentDiagnostics = retainedSession.Diagnostics.GetRecentDiagnostics(10);
var retainedSessionHost = retainedSession.GetType()
    .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
    .GetValue(retainedSession);
var retainedSessionIsAdapterBacked = retainedSessionHost is not null && retainedSessionHost is not GraphEditorViewModel;
var ownerMatched = menuContext.TryGetOwner<HostSampleOwner>(out var typedOwner);
var topLevelMatched = menuContext.TryGetTopLevel<HostSampleTopLevel>(out var typedTopLevel);
var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.Default,
});
var optOutView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.Default,
    EnableDefaultContextMenu = false,
    EnableDefaultCommandShortcuts = false,
});
var directCompatibilityView = new GraphEditorView
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
};
var defaultHeaderVisible = FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible;
var defaultLibraryVisible = FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible;
var defaultInspectorVisible = FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible;
var defaultStatusVisible = FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible;
view.ChromeMode = GraphEditorViewChromeMode.CanvasOnly;
var canvasOnlyHeaderHidden = !FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible;
var canvasOnlyLibraryHidden = !FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible;
var canvasOnlyInspectorHidden = !FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible;
var canvasOnlyStatusHidden = !FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible;
var canvasStillExists = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas") is not null;
var optOutCanvas = FindRequiredControl<NodeCanvas>(optOutView, "PART_NodeCanvas");
var shellInspectorSurface = FindRequiredControl<GraphInspectorView>(view, "PART_InspectorSurface");
var shellMiniMapSurface = FindRequiredControl<GraphMiniMap>(view, "PART_MiniMapSurface");
var customNodePresenter = new HostSampleNodeVisualPresenter();
var customMenuPresenter = new HostSampleContextMenuPresenter();
var customInspectorPresenter = new HostSampleInspectorPresenter();
var customMiniMapPresenter = new HostSampleMiniMapPresenter();
var customPresentation = new AsterGraphPresentationOptions
{
    NodeVisualPresenter = customNodePresenter,
    ContextMenuPresenter = customMenuPresenter,
    InspectorPresenter = customInspectorPresenter,
    MiniMapPresenter = customMiniMapPresenter,
};
var customView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
    Presentation = customPresentation,
});
var customShellCanvas = FindRequiredControl<NodeCanvas>(customView, "PART_NodeCanvas");
var customShellInspectorSurface = FindRequiredControl<GraphInspectorView>(customView, "PART_InspectorSurface");
var customShellMiniMapSurface = FindRequiredControl<GraphMiniMap>(customView, "PART_MiniMapSurface");
var standaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = editor,
});
var standaloneCanvasOptOut = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = editor,
    EnableDefaultContextMenu = false,
    EnableDefaultCommandShortcuts = false,
});
var standaloneInspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
{
    Editor = editor,
});
var standaloneMiniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
{
    Editor = editor,
});
var customStandaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = editor,
    Presentation = customPresentation,
});
var customStandaloneInspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
{
    Editor = editor,
    Presentation = customPresentation,
});
var customStandaloneMiniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
{
    Editor = editor,
    Presentation = customPresentation,
});
var phase16ShellPresenter = new Phase16RecordingContextMenuPresenter();
var phase16ShellEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = CreateDocument(),
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
    StyleOptions = style,
    BehaviorOptions = viewBehavior,
});
var phase16ShellView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = phase16ShellEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
    Presentation = new AsterGraphPresentationOptions
    {
        ContextMenuPresenter = phase16ShellPresenter,
    },
});
var phase16ShellCanvas = FindRequiredControl<NodeCanvas>(phase16ShellView, "PART_NodeCanvas");
phase16ShellEditor.SelectSingleNode(AssertNode(phase16ShellEditor, "sample-source-001"), updateStatus: false);
var phase16ShellDeleteArgs = new KeyEventArgs
{
    Key = Key.Delete,
};
InvokeNonPublicHandler(phase16ShellView, "HandleKeyDown", phase16ShellView, phase16ShellDeleteArgs);
var phase16ShellShortcutOk = phase16ShellDeleteArgs.Handled && phase16ShellEditor.Nodes.Count == 1;
var phase16ShellMenuArgs = new ContextRequestedEventArgs();
InvokeNonPublicHandler(phase16ShellCanvas, "HandleCanvasContextRequested", phase16ShellCanvas, phase16ShellMenuArgs);
var phase16ShellMenuOk = phase16ShellMenuArgs.Handled
    && phase16ShellPresenter.CanonicalOpenCalls == 1
    && phase16ShellPresenter.CompatibilityOpenCalls == 0;
var phase16ShellPlatformBoundaryOk = !GetAttachPlatformSeams(phase16ShellCanvas);
var phase16CanvasPresenter = new Phase16RecordingContextMenuPresenter();
var phase16CanvasEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = CreateDocument(),
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
    StyleOptions = style,
    BehaviorOptions = viewBehavior,
});
var phase16Canvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = phase16CanvasEditor,
    Presentation = new AsterGraphPresentationOptions
    {
        ContextMenuPresenter = phase16CanvasPresenter,
    },
});
phase16CanvasEditor.StartConnection("sample-source-001", "result");
var phase16CanvasEscapeArgs = new KeyEventArgs
{
    Key = Key.Escape,
};
InvokeNonPublicHandler(phase16Canvas, "HandleCanvasKeyDown", phase16Canvas, phase16CanvasEscapeArgs);
var phase16CanvasShortcutOk = phase16CanvasEscapeArgs.Handled
    && !phase16CanvasEditor.Session.Queries.GetPendingConnectionSnapshot().HasPendingConnection;
var phase16CanvasMenuArgs = new ContextRequestedEventArgs();
InvokeNonPublicHandler(phase16Canvas, "HandleCanvasContextRequested", phase16Canvas, phase16CanvasMenuArgs);
var phase16CanvasMenuOk = phase16CanvasMenuArgs.Handled
    && phase16CanvasPresenter.CanonicalOpenCalls == 1
    && phase16CanvasPresenter.CompatibilityOpenCalls == 0;
var phase16CanvasPlatformBoundaryOk = GetAttachPlatformSeams(phase16Canvas);
var hostedUiRouteSnapshot = CaptureViewRouteSnapshot(view);
var directCompatibilityRouteSnapshot = CaptureViewRouteSnapshot(directCompatibilityView);
var runtimeSharedCanonicalCommandIds = CaptureSharedCanonicalCommandIds(runtimeSession);
var retainedSharedCanonicalCommandIds = CaptureSharedCanonicalCommandIds(retainedSession);
var retainedCompatibilityOnlyCommands = CaptureCompatibilityOnlyCommandIds(runtimeSession, retainedSession);
var phase17HostedUiParityOk = hostedUiRouteSnapshot == directCompatibilityRouteSnapshot;
var phase17SharedCanonicalSubsetOk = runtimeSharedCanonicalCommandIds.SequenceEqual(retainedSharedCanonicalCommandIds, StringComparer.Ordinal);
var phase17RetainedCompatibilityWindowOk = retainedCompatibilityOnlyCommands.Contains("nodes.duplicate", StringComparer.Ordinal);
var phase29RetainedHostProofOk = retainedPluginLoadSnapshots.Count == 2
    && runtimePluginLoadSnapshots.SequenceEqual(retainedPluginLoadSnapshots)
    && retainedTrustedPluginSnapshot.Status == GraphEditorPluginLoadStatus.Loaded
    && retainedTrustedPluginSnapshot.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Allowed
    && retainedBlockedPluginSnapshot.Status == GraphEditorPluginLoadStatus.Blocked
    && retainedBlockedPluginSnapshot.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Blocked
    && !retainedBlockedPluginSnapshot.ActivationAttempted
    && retainedPluginMenuOk
    && retainedPluginLocalizationOk
    && retainedPluginBadgeOk
    && viewDiagnostics.Diagnostics.Any(diagnostic => diagnostic.Code == "plugin.load.blocked");
var phase29HostBoundaryOk = phase29TrustHostOk
    && phase29RetainedHostProofOk
    && runtimeSessionIsKernelFirst
    && retainedSessionIsAdapterBacked;

Console.WriteLine($"Host sample view type: {typeof(GraphEditorView).FullName}");
Console.WriteLine($"Node count: {editor.Nodes.Count}");
Console.WriteLine($"Position count: {positions.Count}");
Console.WriteLine($"Selected snapshot found: {snapshot is not null}");
Console.WriteLine($"Host preview menu item exists: {hostPreviewItem is not null}");
Console.WriteLine($"Host preview menu item header: {hostPreviewItem?.Header ?? "<missing>"}");
Console.WriteLine($"ReadOnly host extension allowed: {editor.CommandPermissions.Host.AllowContextMenuExtensions}");
Console.WriteLine($"Retained session diagnostics reachable: {ReferenceEquals(retainedSession, editor.Session)}");
Console.WriteLine($"Retained backend: adapter-backed={retainedSessionIsAdapterBacked}");
Console.WriteLine($"Retained path: GraphEditorViewModel.Session compatibility surface over the shared runtime boundary");
Console.WriteLine($"Descriptor retained: commands={editor.Session.Queries.GetCommandDescriptors().Count}, menu-adapter={retainedDescriptorMenuBacked}");
Console.WriteLine("Migration proof routes: canonical runtime=AsterGraphEditorFactory.CreateSession(...); canonical hosted-ui=AsterGraphEditorFactory.Create(...) + AsterGraphAvaloniaViewFactory.Create(...); retained compatibility=new GraphEditorView { Editor = editor } plus GraphEditorViewModel.Session.");
Console.WriteLine($"Migration proof shared canonical commands: {string.Join(",", runtimeSharedCanonicalCommandIds)}");
Console.WriteLine($"Migration proof retained-only commands: {(retainedCompatibilityOnlyCommands.Count == 0 ? "<none>" : string.Join(",", retainedCompatibilityOnlyCommands))}");
Console.WriteLine($"Migration proof hosted-ui parity: {phase17HostedUiParityOk}");
Console.WriteLine($"Retained inspection snapshot: nodes={retainedInspection.Document.Nodes.Count}, selected={retainedInspection.Selection.SelectedNodeIds.Count}, pending={retainedInspection.PendingConnection.HasPendingConnection}");
Console.WriteLine($"Retained recent diagnostics count: {retainedRecentDiagnostics.Count}");
Console.WriteLine($"Retained diagnostics sink count: {viewDiagnostics.Diagnostics.Count}");
Console.WriteLine($"StatusMessage compatibility surface (not canonical diagnostics API): {editor.StatusMessage}");
Console.WriteLine($"Host context flowed into menu request: {ReferenceEquals(hostContext, menuContext.HostContext)}");
Console.WriteLine($"Localized inspector title: {editor.InspectorTitle}");
Console.WriteLine($"Presentation subtitle: {node.DisplaySubtitle}");
Console.WriteLine($"Presentation badge count: {node.Presentation.TopRightBadges.Count}");
Console.WriteLine($"Presentation status bar: {node.Presentation.StatusBar?.Text ?? "<none>"}");
Console.WriteLine($"Typed host owner found: {ownerMatched} ({typedOwner?.DisplayName ?? "<none>"})");
Console.WriteLine($"Typed host top level found: {topLevelMatched} ({typedTopLevel?.WindowTitle ?? "<none>"})");
Console.WriteLine($"Style highlight hex: {editor.StyleOptions.Shell.HighlightHex}");
Console.WriteLine($"Style context menu background: {editor.StyleOptions.ContextMenu.BackgroundHex}");
Console.WriteLine($"View compatibility evaluations: {viewCompatibility.EvaluateCalls}");
Console.WriteLine($"ChromeMode default sections visible: header={defaultHeaderVisible}, library={defaultLibraryVisible}, inspector={defaultInspectorVisible}, status={defaultStatusVisible}");
Console.WriteLine($"ChromeMode switched to: {view.ChromeMode}");
Console.WriteLine($"ChromeMode canvas-only sections hidden: header={canvasOnlyHeaderHidden}, library={canvasOnlyLibraryHidden}, inspector={canvasOnlyInspectorHidden}, status={canvasOnlyStatusHidden}");
Console.WriteLine($"ChromeMode canvas-only keeps canvas: {canvasStillExists}");
Console.WriteLine($"Full shell opt-out: menu={optOutView.EnableDefaultContextMenu}, shortcuts={optOutView.EnableDefaultCommandShortcuts}, canvasMenu={optOutCanvas.EnableDefaultContextMenu}, canvasShortcuts={optOutCanvas.EnableDefaultCommandShortcuts}");
Console.WriteLine($"Full shell embeds standalone surfaces: inspector={ReferenceEquals(editor, shellInspectorSurface.Editor)}, minimap={ReferenceEquals(editor, shellMiniMapSurface.ViewModel)}");
Console.WriteLine($"Standalone surfaces share editor: canvas={ReferenceEquals(editor, standaloneCanvas.ViewModel)}, inspector={ReferenceEquals(editor, standaloneInspector.Editor)}, minimap={ReferenceEquals(editor, standaloneMiniMap.ViewModel)}");
Console.WriteLine($"Standalone canvas defaults: menu={standaloneCanvas.EnableDefaultContextMenu}, shortcuts={standaloneCanvas.EnableDefaultCommandShortcuts}");
Console.WriteLine($"Standalone canvas opt-out: menu={standaloneCanvasOptOut.EnableDefaultContextMenu}, shortcuts={standaloneCanvasOptOut.EnableDefaultCommandShortcuts}");
Console.WriteLine($"Stock presenter fallback stays opt-in: shellPresentationNull={view.Presentation is null}, canvasNodePresenterNull={standaloneCanvas.NodeVisualPresenter is null}, canvasMenuPresenterNull={standaloneCanvas.ContextMenuPresenter is null}, inspectorPresenterNull={standaloneInspector.InspectorPresenter is null}, miniMapPresenterNull={standaloneMiniMap.MiniMapPresenter is null}");
Console.WriteLine($"Custom full shell presenters: node={ReferenceEquals(customNodePresenter, customShellCanvas.NodeVisualPresenter)}, menu={ReferenceEquals(customMenuPresenter, customShellCanvas.ContextMenuPresenter)}, inspector={ReferenceEquals(customInspectorPresenter, customShellInspectorSurface.InspectorPresenter)}, minimap={ReferenceEquals(customMiniMapPresenter, customShellMiniMapSurface.MiniMapPresenter)}");
Console.WriteLine($"Custom full shell content: inspector={DescribeContent(customShellInspectorSurface.Content)}, minimap={DescribeContent(customShellMiniMapSurface.Content)}");
Console.WriteLine($"Custom standalone presenters: node={ReferenceEquals(customNodePresenter, customStandaloneCanvas.NodeVisualPresenter)}, menu={ReferenceEquals(customMenuPresenter, customStandaloneCanvas.ContextMenuPresenter)}, inspector={ReferenceEquals(customInspectorPresenter, customStandaloneInspector.InspectorPresenter)}, minimap={ReferenceEquals(customMiniMapPresenter, customStandaloneMiniMap.MiniMapPresenter)}");
Console.WriteLine($"Custom standalone content: inspector={DescribeContent(customStandaloneInspector.Content)}, minimap={DescribeContent(customStandaloneMiniMap.Content)}");
Console.WriteLine($"Presenter types: node={customNodePresenter.GetType().Name}, menu={customMenuPresenter.GetType().Name}, inspector={customInspectorPresenter.GetType().Name}, minimap={customMiniMapPresenter.GetType().Name}");
Console.WriteLine($"Phase 16 adapter boundary: shell menu=canonical({phase16ShellPresenter.CanonicalOpenCalls}), shell shortcut shared={phase16ShellShortcutOk}, shell platform owner=view({phase16ShellPlatformBoundaryOk}); standalone menu=canonical({phase16CanvasPresenter.CanonicalOpenCalls}), standalone shortcut shared={phase16CanvasShortcutOk}, standalone platform owner=canvas({phase16CanvasPlatformBoundaryOk})");
Console.WriteLine($"PHASE16_ADAPTER_BOUNDARY_OK:{phase16ShellMenuOk}:{phase16CanvasMenuOk}:{phase16ShellShortcutOk}:{phase16CanvasShortcutOk}:{phase16ShellPlatformBoundaryOk}:{phase16CanvasPlatformBoundaryOk}");
Console.WriteLine($"PHASE17_MIGRATION_ROUTE_OK:{runtimeSessionIsKernelFirst}:{retainedSessionIsAdapterBacked}:{phase17SharedCanonicalSubsetOk}:{phase17HostedUiParityOk}:{phase17RetainedCompatibilityWindowOk}");
Console.WriteLine($"PHASE18_READINESS_OK:{runtimeSessionIsKernelFirst}:{phase18ReadinessOk}:{runtimeAutomationDescriptorsOk}:{runtimeReadinessDescriptors.Count}");
Console.WriteLine($"PHASE25_AUTOMATION_HOST_OK:{phase25AutomationHostOk}:{runtimeAutomationResult.ExecutedStepCount}:{automationStartedCount}:{automationCompletedCount}:{runtimeAutomationResult.Inspection.Document.Nodes.Count}:{runtimeAutomationDiagnosticCodes.Length}");
Console.WriteLine($"PHASE29_TRUST_HOST_OK:{phase29TrustHostOk}:{runtimeTrustedPluginSnapshot.Status}:{runtimeBlockedPluginSnapshot.Status}:{runtimePluginLoadSnapshots.Count}:{runtimeDiagnostics.Diagnostics.Count(diagnostic => diagnostic.Code == "plugin.load.blocked")}");
Console.WriteLine($"PHASE29_RETAINED_TRUST_OK:{phase29RetainedHostProofOk}:{retainedTrustedPluginSnapshot.Status}:{retainedBlockedPluginSnapshot.Status}:{retainedPluginLoadSnapshots.Count}:{viewDiagnostics.Diagnostics.Count(diagnostic => diagnostic.Code == "plugin.load.blocked")}");
Console.WriteLine($"PHASE29_HOST_BOUNDARY_OK:{phase29HostBoundaryOk}:{runtimeSharedCanonicalCommandIds.Count}:{retainedSharedCanonicalCommandIds.Count}:{blockedPlugin.RegisterCallCount}:{runtimeInspection.PluginLoadSnapshots.Count}");

static GraphDocument CreateDocument()
    => new(
        "Host Sample Graph",
        "Reference host composition sample for AsterGraph.",
        [
            new GraphNode(
                "sample-source-001",
                "Sample Source",
                "Host Sample",
                "Produces a float output",
                "Used to demonstrate host integration.",
                new GraphPoint(120, 160),
                new GraphSize(240, 160),
                [],
                [
                    new GraphPort(
                        "result",
                        "Result",
                        PortDirection.Output,
                        "float",
                        "#6AD5C4",
                        new PortTypeId("float")),
                ],
                "#6AD5C4",
                new NodeDefinitionId("host.sample.source")),
            new GraphNode(
                "sample-sink-001",
                "Sample Sink",
                "Host Sample",
                "Consumes a float input",
                "Used to demonstrate compatibility queries.",
                new GraphPoint(420, 160),
                new GraphSize(240, 160),
                [
                    new GraphPort(
                        "input",
                        "Input",
                        PortDirection.Input,
                        "float",
                        "#F3B36B",
                        new PortTypeId("float")),
                ],
                [],
                "#F3B36B",
                new NodeDefinitionId("host.sample.sink")),
        ],
        []);

static NodeViewModel AssertNode(GraphEditorViewModel editor, string nodeId)
    => editor.Nodes.Single(node => node.Id == nodeId);

static T FindRequiredControl<T>(Control root, string name)
    where T : Control
    => root.FindControl<T>(name) ?? throw new InvalidOperationException($"Could not find control '{name}'.");

static string DescribeContent(object? content)
    => content switch
    {
        TextBlock text => text.Text ?? "<empty>",
        Control control => control.GetType().Name,
        null => "<null>",
        _ => content.GetType().Name,
    };

static void InvokeNonPublicHandler(object target, string methodName, params object[] args)
{
    var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Could not find non-public handler '{methodName}' on {target.GetType().Name}.");
    method.Invoke(target, args);
}

static bool GetAttachPlatformSeams(NodeCanvas canvas)
{
    var property = typeof(NodeCanvas).GetProperty("AttachPlatformSeams", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Could not find NodeCanvas.AttachPlatformSeams.");
    return property.GetValue(canvas) as bool? ?? throw new InvalidOperationException("NodeCanvas.AttachPlatformSeams did not return a bool.");
}

static ViewRouteSnapshot CaptureViewRouteSnapshot(GraphEditorView view)
{
    var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");
    return new(
        view.ChromeMode,
        view.Editor is not null,
        GetAttachPlatformSeams(canvas),
        canvas.EnableDefaultContextMenu,
        canvas.EnableDefaultCommandShortcuts);
}

static IReadOnlyList<string> CaptureSharedCanonicalCommandIds(IGraphEditorSession session)
    => session.Queries.GetCommandDescriptors()
        .Where(descriptor => IsSharedCanonicalCommandId(descriptor.Id))
        .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
        .Select(descriptor => descriptor.Id)
        .ToArray();

static IReadOnlyList<string> CaptureCompatibilityOnlyCommandIds(IGraphEditorSession canonicalSession, IGraphEditorSession retainedSession)
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

static bool IsSharedCanonicalCommandId(string id)
    => id is
        "nodes.add" or
        "selection.set" or
        "selection.delete" or
        "connections.start" or
        "connections.complete" or
        "connections.connect" or
        "connections.cancel" or
        "connections.delete" or
        "connections.break-port" or
        "nodes.move" or
        "viewport.pan" or
        "viewport.resize" or
        "viewport.center" or
        "viewport.fit" or
        "viewport.reset" or
        "viewport.center-node" or
        "workspace.save" or
        "workspace.load";

static IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> CaptureReadinessDescriptors(IGraphEditorSession session)
    => session.Queries.GetFeatureDescriptors()
        .Where(descriptor => IsReadinessFeatureId(descriptor.Id))
        .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
        .ToArray();

static bool IsReadinessFeatureId(string id)
    => id is
        "query.plugin-load-snapshots" or
        "surface.automation.runner" or
        "event.automation.started" or
        "event.automation.progress" or
        "event.automation.completed" or
        "service.workspace" or
        "service.fragment-workspace" or
        "service.fragment-library" or
        "service.clipboard-payload-serializer" or
        "service.diagnostics" or
        "integration.plugin-loader" or
        "integration.context-menu-augmentor" or
        "integration.node-presentation-provider" or
        "integration.localization-provider" or
        "integration.diagnostics-sink" or
        "integration.instrumentation.logger" or
        "integration.instrumentation.activity-source";

static GraphEditorPluginManifest CreateHostSampleManifest(
    string manifestId,
    string displayName,
    Type pluginType,
    string capabilitySummary)
    => new(
        manifestId,
        displayName,
        new GraphEditorPluginManifestProvenance(
            GraphEditorPluginManifestSourceKind.DirectRegistration,
            pluginType.FullName ?? pluginType.Name),
        version: "1.0.0",
        compatibility: new GraphEditorPluginCompatibilityManifest(
            minimumAsterGraphVersion: "0.0.0",
            targetFramework: "net9.0",
            runtimeSurface: "session-first"),
        capabilitySummary: capabilitySummary);

static GraphEditorPluginLoadSnapshot FindPluginLoadSnapshot(
    IReadOnlyList<GraphEditorPluginLoadSnapshot> snapshots,
    string manifestId)
    => snapshots.Single(snapshot => string.Equals(snapshot.Manifest.Id, manifestId, StringComparison.Ordinal));

static GraphEditorAutomationRunRequest CreateAutomationRunRequest(string runId, string pluginDefinitionId)
    => new(
        runId,
        [
            new GraphEditorAutomationStep("select-source", CreateAutomationCommand("selection.set", ("nodeId", "sample-source-001"), ("primaryNodeId", "sample-source-001"), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("add-plugin-node", CreateAutomationCommand("nodes.add", ("definitionId", pluginDefinitionId), ("worldX", "720"), ("worldY", "260"))),
            new GraphEditorAutomationStep("move-plugin-node", CreateAutomationCommand("nodes.move", ("position", "host-sample-plugin-001|744|284"), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("resize-viewport", CreateAutomationCommand("viewport.resize", ("width", "1280"), ("height", "720"))),
            new GraphEditorAutomationStep("pan-viewport", CreateAutomationCommand("viewport.pan", ("deltaX", "8"), ("deltaY", "12"))),
            new GraphEditorAutomationStep("start-connection", CreateAutomationCommand("connections.start", ("sourceNodeId", "sample-source-001"), ("sourcePortId", "result"))),
            new GraphEditorAutomationStep("complete-connection", CreateAutomationCommand("connections.complete", ("targetNodeId", "host-sample-plugin-001"), ("targetPortId", "input"))),
        ]);

static GraphEditorCommandInvocationSnapshot CreateAutomationCommand(
    string commandId,
    params (string Name, string Value)[] arguments)
    => new(
        commandId,
        arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToArray());

readonly record struct ViewRouteSnapshot(
    GraphEditorViewChromeMode ChromeMode,
    bool EditorAssigned,
    bool CanvasAttachPlatformSeams,
    bool EnableDefaultContextMenu,
    bool EnableDefaultCommandShortcuts);

internal sealed class HostSampleNodeDefinitionProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        =>
        [
            new NodeDefinition(
                new NodeDefinitionId("host.sample.source"),
                "Sample Source",
                "Host Sample",
                "Produces a float output",
                [],
                [
                    new PortDefinition(
                        "result",
                        "Result",
                        new PortTypeId("float"),
                        "#6AD5C4",
                        "Sample output for host integration."),
                ],
                description: "Minimal sample source definition used by the host sample.",
                accentHex: "#6AD5C4",
                defaultWidth: 240,
                defaultHeight: 160),
            new NodeDefinition(
                new NodeDefinitionId("host.sample.sink"),
                "Sample Sink",
                "Host Sample",
                "Consumes a float input",
                [
                    new PortDefinition(
                        "input",
                        "Input",
                        new PortTypeId("float"),
                        "#F3B36B",
                        "Sample input for compatibility queries."),
                ],
                [],
                description: "Minimal sample sink definition used by the host sample.",
                accentHex: "#F3B36B",
                defaultWidth: 240,
                defaultHeight: 160),
        ];
}

internal sealed class HostSampleProofPlugin : IGraphEditorPlugin
{
    public GraphEditorPluginDescriptor Descriptor { get; } = new(
        "host.sample.plugin",
        "Host Sample Plugin",
        "Direct-registration proof plugin for the host sample.");

    public void Register(GraphEditorPluginBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.AddNodeDefinitionProvider(new HostSamplePluginNodeDefinitionProvider());
        builder.AddContextMenuAugmentor(new HostSamplePluginContextMenuAugmentor());
        builder.AddNodePresentationProvider(new HostSamplePluginPresentationProvider());
        builder.AddLocalizationProvider(new HostSamplePluginLocalizationProvider());
    }
}

internal sealed class HostSampleBlockedPlugin : IGraphEditorPlugin
{
    public int RegisterCallCount { get; private set; }

    public GraphEditorPluginDescriptor Descriptor { get; } = new(
        "host.sample.blocked-plugin",
        "Host Sample Blocked Plugin",
        "Blocked direct-registration proof plugin for host trust coverage.");

    public void Register(GraphEditorPluginBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RegisterCallCount++;
        builder.AddContextMenuAugmentor(new HostSamplePluginContextMenuAugmentor());
    }
}

internal sealed class HostSampleManifestTrustPolicy(string blockedManifestId) : IGraphEditorPluginTrustPolicy
{
    public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return StringComparer.Ordinal.Equals(context.Manifest.Id, blockedManifestId)
            ? new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Blocked,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                "trust.blocked.host-sample",
                $"Blocked manifest '{context.Manifest.Id}' for host proof coverage.")
            : new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                "trust.allowed.host-sample",
                $"Allowed manifest '{context.Manifest.Id}' for host proof coverage.");
    }
}

internal sealed class HostSamplePluginNodeDefinitionProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        =>
        [
            new NodeDefinition(
                new NodeDefinitionId("host.sample.plugin"),
                "Host Sample Plugin Node",
                "Host Sample",
                "Plugin proof node",
                [
                    new PortDefinition(
                        "input",
                        "Input",
                        new PortTypeId("float"),
                        "#F3B36B",
                        "Plugin proof input."),
                ],
                [],
                description: "Plugin-contributed node definition used by the host proof ring.",
                accentHex: "#8DDCBF",
                defaultWidth: 240,
                defaultHeight: 160),
        ];
}

internal sealed class HostSamplePluginContextMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
{
    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
        => context.StockItems
            .Concat(
            [
                new GraphEditorMenuItemDescriptorSnapshot(
                    "host-sample-plugin-menu",
                    "Plugin Proof Menu",
                    iconKey: "plugin",
                    isEnabled: false),
            ])
            .ToArray();
}

internal sealed class HostSamplePluginPresentationProvider : IGraphEditorPluginNodePresentationProvider
{
    public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
        => new(
            TopRightBadges:
            [
                new NodeAdornmentDescriptor("Plugin", "#8DDCBF", "State comes from a plugin contribution."),
            ]);
}

internal sealed class HostSamplePluginLocalizationProvider : IGraphEditorPluginLocalizationProvider
{
    public string GetString(string key, string fallback)
        => key == "editor.menu.canvas.addNode"
            ? "Plugin Add Node"
            : fallback;
}

internal sealed class HostSampleAugmentor : IGraphContextMenuAugmentor
{
    public IReadOnlyList<MenuItemDescriptor> Augment(
        GraphEditorViewModel editor,
        ContextMenuContext context,
        IReadOnlyList<MenuItemDescriptor> stockItems)
    {
        if (context.TargetKind != ContextMenuTargetKind.Node
            || string.IsNullOrWhiteSpace(context.ClickedNodeId))
        {
            return stockItems;
        }

        var header = "Host Preview";
        if (context.TryGetTopLevel<HostSampleTopLevel>(out var topLevel)
            && topLevel is not null)
        {
            header = $"Host Preview ({topLevel.WindowTitle})";
        }

        var items = stockItems.ToList();
        items.Add(MenuItemDescriptor.Separator("host-sample-separator"));
        items.Add(
            new MenuItemDescriptor(
                "host-sample-preview",
                header,
                new RelayCommand(() => editor.StatusMessage = $"Host preview for {context.ClickedNodeId}"),
                iconKey: "inspect"));
        return items;
    }
}

internal sealed class HostSamplePresentationProvider : INodePresentationProvider
{
    public NodePresentationState GetNodePresentation(NodeViewModel node)
        => new(
            SubtitleOverride: "Runtime annotated",
            DescriptionOverride: node.Description,
            TopRightBadges:
            [
                new NodeAdornmentDescriptor("Ready", "#6AD5C4", "Host reports the node is ready."),
                new NodeAdornmentDescriptor("Host", "#F3B36B", "State comes from the embedding host."),
            ],
            StatusBar: new NodeStatusBarDescriptor(
                "Preview available",
                "#6AD5C4",
                "Host sample marks this node as preview-ready."));
}

internal sealed class HostSampleLocalizationProvider : IGraphLocalizationProvider
{
    private static readonly IReadOnlyDictionary<string, string> Values =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["editor.inspector.title.none"] = "请选择宿主节点",
        };

    public string GetString(string key, string fallback)
        => Values.TryGetValue(key, out var value) ? value : fallback;
}

internal sealed class HostSampleNodeVisualPresenter : IGraphNodeVisualPresenter
{
    private readonly DefaultGraphNodeVisualPresenter _stockPresenter = new();

    public GraphNodeVisual Create(GraphNodeVisualContext context)
        => _stockPresenter.Create(context);

    public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        => _stockPresenter.Update(visual, context);
}

internal sealed class HostSampleContextMenuPresenter : IGraphContextMenuPresenter
{
    private readonly GraphContextMenuPresenter _stockPresenter = new();

    public int OpenCalls { get; private set; }

    public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
    {
        OpenCalls++;
        _stockPresenter.Open(target, descriptors, style);
    }
}

internal sealed class Phase16RecordingContextMenuPresenter : IGraphContextMenuPresenter
{
    public int CompatibilityOpenCalls { get; private set; }

    public int CanonicalOpenCalls { get; private set; }

    public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
        => CompatibilityOpenCalls++;

    public void Open(
        Control target,
        IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> descriptors,
        IGraphEditorCommands commands,
        ContextMenuStyleOptions style)
        => CanonicalOpenCalls++;
}

internal sealed class HostSampleInspectorPresenter : IGraphInspectorPresenter
{
    public Control Create(GraphEditorViewModel? editor)
        => new TextBlock
        {
            Text = $"CUSTOM INSPECTOR:{editor?.InspectorTitle ?? "<none>"}",
        };
}

internal sealed class HostSampleMiniMapPresenter : IGraphMiniMapPresenter
{
    public Control Create(GraphEditorViewModel? editor)
        => new TextBlock
        {
            Text = $"CUSTOM MINIMAP:{editor?.Title ?? "<none>"}",
        };
}

internal sealed class RecordingCompatibilityService : IPortCompatibilityService
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

internal sealed class ThrowingWorkspaceService(string workspacePath) : IGraphWorkspaceService
{
    public string WorkspacePath { get; } = workspacePath;

    public int SaveCalls { get; private set; }

    public GraphDocument? LastSaved { get; private set; }

    public void Save(GraphDocument document)
    {
        SaveCalls++;
        LastSaved = document;
        throw new InvalidOperationException("host sample workspace save failed on purpose");
    }

    public GraphDocument Load()
        => LastSaved ?? throw new InvalidOperationException("No workspace snapshot.");

    public bool Exists()
        => LastSaved is not null;
}

internal sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
{
    public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

    public void Publish(GraphEditorDiagnostic diagnostic)
        => Diagnostics.Add(diagnostic);
}

internal static class HostSampleSupport
{
    internal static ActivityListener CreateListener(string sourceName, List<string> activities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => activities.Add(activity.OperationName),
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    internal sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public List<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName)
            => new RecordingLogger(categoryName, Entries);

        public void AddProvider(ILoggerProvider provider)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    internal sealed class RecordingLogger(string categoryName, List<LogEntry> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Add(new LogEntry(categoryName, logLevel, formatter(state, exception), exception));
        }
    }

    internal sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

    internal sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}

internal sealed record HostSampleGraphHostContext(HostSampleOwner Owner, HostSampleTopLevel? TopLevel) : IGraphHostContext
{
    object IGraphHostContext.Owner => Owner;

    object? IGraphHostContext.TopLevel => TopLevel;

    public IServiceProvider? Services => null;
}

internal sealed record HostSampleOwner(string Id, string DisplayName);

internal sealed record HostSampleTopLevel(string Id, string WindowTitle);
