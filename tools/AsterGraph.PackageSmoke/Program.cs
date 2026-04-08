using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
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

var sourceDefinitionId = new NodeDefinitionId("smoke.source");
var targetDefinitionId = new NodeDefinitionId("smoke.target");
const string sourceNodeId = "smoke-source-001";
const string targetNodeId = "smoke-target-001";
const string sourcePortId = "out";

var catalog = new NodeCatalog();
catalog.RegisterDefinition(new NodeDefinition(
    sourceDefinitionId,
    "Smoke Source",
    "Smoke",
    "Migration",
    [],
    [
        new PortDefinition(sourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
    ]));
catalog.RegisterDefinition(new NodeDefinition(
    targetDefinitionId,
    "Smoke Target",
    "Smoke",
    "Migration",
    [
        new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B"),
    ],
    []));

var document = new GraphDocument(
    "Package Smoke Graph",
    "Migration-stage smoke validation.",
    [
        new GraphNode(
            sourceNodeId,
            "Smoke Source",
            "Smoke",
            "Migration",
            "Verifies both legacy and factory host paths.",
            new GraphPoint(64, 96),
            new GraphSize(220, 160),
            [],
            [
                new GraphPort(
                    sourcePortId,
                    "Output",
                    PortDirection.Output,
                    "float",
                    "#6AD5C4",
                    new PortTypeId("float")),
            ],
            "#6AD5C4",
            sourceDefinitionId),
        new GraphNode(
            targetNodeId,
            "Smoke Target",
            "Smoke",
            "Migration",
            "Verifies runtime compatibility queries.",
            new GraphPoint(360, 96),
            new GraphSize(220, 160),
            [
                new GraphPort(
                    "in",
                    "Input",
                    PortDirection.Input,
                    "float",
                    "#F3B36B",
                    new PortTypeId("float")),
            ],
            [],
            "#F3B36B",
            targetDefinitionId),
    ],
    []);

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
var compatibilityService = new RecordingCompatibilityService();
using var loggerFactory = new SmokeSupport.RecordingLoggerFactory();
using var activitySource = new ActivitySource("AsterGraph.PackageSmoke");
var activityOperations = new List<string>();
using var activityListener = SmokeSupport.CreateListener(activitySource.Name, activityOperations);
var workspaceService = new RecordingWorkspaceService("workspace://package-smoke", throwOnSave: true);
var fragmentWorkspaceService = new RecordingFragmentWorkspaceService("fragment://package-smoke");
var fragmentLibraryService = new RecordingFragmentLibraryService("library://package-smoke");
var serializer = new RecordingClipboardPayloadSerializer();
var diagnostics = new RecordingDiagnosticsSink();
const int ReadinessFeatureCount = 17;
const string SmokePluginDefinitionIdValue = "smoke.plugin";
const string SmokePluginMenuId = "smoke-plugin-menu";
const string SmokeAutomationRunId = "smoke-proof-automation";
var pluginRegistrations =
    new[]
    {
        GraphEditorPluginRegistration.FromPlugin(new SmokeProofPlugin()),
    };

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
    localizationProvider,
    serializer,
    diagnostics);
var legacyView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = legacyEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
});
var legacyDirectView = new GraphEditorView
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
    ClipboardPayloadSerializer = serializer,
    DiagnosticsSink = diagnostics,
    Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
    ContextMenuAugmentor = menuAugmentor,
    NodePresentationProvider = presentationProvider,
    LocalizationProvider = localizationProvider,
    PluginRegistrations = pluginRegistrations,
});
var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = factoryEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
});
var factoryDirectView = new GraphEditorView
{
    Editor = factoryEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
};
var inspectorProofEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
    StyleOptions = styleOptions,
    BehaviorOptions = behaviorOptions,
});
inspectorProofEditor.SelectSingleNode(inspectorProofEditor.Nodes.Single(node => node.Id == sourceNodeId), updateStatus: false);
inspectorProofEditor.ConnectPorts(sourceNodeId, sourcePortId, targetNodeId, "in");
var historyProofEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
    StyleOptions = styleOptions,
    BehaviorOptions = behaviorOptions,
});
var historyProofNode = historyProofEditor.Nodes.Single(node => node.Id == sourceNodeId);
var historyProofOrigin = new GraphPoint(historyProofNode.X, historyProofNode.Y);
historyProofEditor.BeginHistoryInteraction();
historyProofEditor.ApplyDragOffset(
    new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
    {
        [sourceNodeId] = historyProofOrigin,
    },
    32,
    16);
historyProofEditor.CompleteHistoryInteraction("Package smoke state scaling proof.");
var historyDirtyAfterMove = historyProofEditor.IsDirty;
var historyCanUndoAfterMove = historyProofEditor.CanUndo;
historyProofEditor.Undo();
var historyRestoredNode = historyProofEditor.Nodes.Single(node => node.Id == sourceNodeId);
var inspectorSummaryOk = inspectorProofEditor.InspectorConnections == "0 incoming  ·  1 outgoing";
var inspectorDownstreamOk = inspectorProofEditor.InspectorDownstream.Contains("Smoke Target", StringComparison.Ordinal);
var optOutFullShell = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = factoryEditor,
    ChromeMode = GraphEditorViewChromeMode.Default,
    EnableDefaultContextMenu = false,
    EnableDefaultCommandShortcuts = false,
});

PrintEditorMarker("LEGACY_EDITOR_OK", legacyEditor, sourceNodeId);
PrintViewMarker("LEGACY_VIEW_OK", legacyView);
legacyEditor.SelectSingleNode(legacyEditor.Nodes.Single(node => node.Id == sourceNodeId), updateStatus: false);
legacyEditor.ExportSelectionFragment();
PrintEditorMarker("FACTORY_EDITOR_OK", factoryEditor, sourceNodeId);
PrintViewMarker("FACTORY_VIEW_OK", factoryView);
var fullShellInspector = factoryView.FindControl<GraphInspectorView>("PART_InspectorSurface")
    ?? throw new InvalidOperationException("Missing PART_InspectorSurface.");
var fullShellMiniMap = factoryView.FindControl<GraphMiniMap>("PART_MiniMapSurface")
    ?? throw new InvalidOperationException("Missing PART_MiniMapSurface.");
var optOutFullShellCanvas = optOutFullShell.FindControl<NodeCanvas>("PART_NodeCanvas")
    ?? throw new InvalidOperationException("Missing PART_NodeCanvas on opt-out full shell.");
Console.WriteLine($"SURFACE_FULLSHELL_OK:{ReferenceEquals(factoryEditor, fullShellInspector.Editor)}:{ReferenceEquals(factoryEditor, fullShellMiniMap.ViewModel)}");
Console.WriteLine($"SURFACE_FULLSHELL_OPTOUT_OK:{optOutFullShell.EnableDefaultContextMenu}:{optOutFullShell.EnableDefaultCommandShortcuts}:{optOutFullShellCanvas.EnableDefaultContextMenu}:{optOutFullShellCanvas.EnableDefaultCommandShortcuts}");

var standaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = factoryEditor,
});
var standaloneCanvasOptOut = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = factoryEditor,
    EnableDefaultContextMenu = false,
    EnableDefaultCommandShortcuts = false,
});
var standaloneInspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
{
    Editor = factoryEditor,
});
var standaloneMiniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
{
    Editor = factoryEditor,
});
var customNodePresenter = new SmokeNodeVisualPresenter();
var customMenuPresenter = new SmokeContextMenuPresenterAdapter();
var customInspectorPresenter = new SmokeInspectorPresenter();
var customMiniMapPresenter = new SmokeMiniMapPresenter();
var customPresentation = new AsterGraphPresentationOptions
{
    NodeVisualPresenter = customNodePresenter,
    ContextMenuPresenter = customMenuPresenter,
    InspectorPresenter = customInspectorPresenter,
    MiniMapPresenter = customMiniMapPresenter,
};
var customView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = factoryEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
    Presentation = customPresentation,
});
var customFullShellCanvas = customView.FindControl<NodeCanvas>("PART_NodeCanvas")
    ?? throw new InvalidOperationException("Missing PART_NodeCanvas on custom full shell.");
var customFullShellInspector = customView.FindControl<GraphInspectorView>("PART_InspectorSurface")
    ?? throw new InvalidOperationException("Missing PART_InspectorSurface on custom full shell.");
var customFullShellMiniMap = customView.FindControl<GraphMiniMap>("PART_MiniMapSurface")
    ?? throw new InvalidOperationException("Missing PART_MiniMapSurface on custom full shell.");
var customStandaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = factoryEditor,
    Presentation = customPresentation,
});
var customStandaloneInspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
{
    Editor = factoryEditor,
    Presentation = customPresentation,
});
var customStandaloneMiniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
{
    Editor = factoryEditor,
    Presentation = customPresentation,
});
var phase16ShellPresenter = new Phase16RecordingContextMenuPresenter();
var phase16ShellEditor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
    StyleOptions = styleOptions,
    BehaviorOptions = behaviorOptions,
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
var phase16ShellCanvas = phase16ShellView.FindControl<NodeCanvas>("PART_NodeCanvas")
    ?? throw new InvalidOperationException("Missing PART_NodeCanvas on Phase 16 full shell.");
phase16ShellEditor.SelectSingleNode(phase16ShellEditor.Nodes.Single(node => node.Id == sourceNodeId), updateStatus: false);
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
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new RecordingCompatibilityService(),
    StyleOptions = styleOptions,
    BehaviorOptions = behaviorOptions,
});
var phase16CanvasProof = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = phase16CanvasEditor,
    Presentation = new AsterGraphPresentationOptions
    {
        ContextMenuPresenter = phase16CanvasPresenter,
    },
});
phase16CanvasEditor.StartConnection(sourceNodeId, sourcePortId);
var phase16CanvasEscapeArgs = new KeyEventArgs
{
    Key = Key.Escape,
};
InvokeNonPublicHandler(phase16CanvasProof, "HandleCanvasKeyDown", phase16CanvasProof, phase16CanvasEscapeArgs);
var phase16CanvasShortcutOk = phase16CanvasEscapeArgs.Handled
    && !phase16CanvasEditor.Session.Queries.GetPendingConnectionSnapshot().HasPendingConnection;
var phase16CanvasMenuArgs = new ContextRequestedEventArgs();
InvokeNonPublicHandler(phase16CanvasProof, "HandleCanvasContextRequested", phase16CanvasProof, phase16CanvasMenuArgs);
var phase16CanvasMenuOk = phase16CanvasMenuArgs.Handled
    && phase16CanvasPresenter.CanonicalOpenCalls == 1
    && phase16CanvasPresenter.CompatibilityOpenCalls == 0;
var phase16CanvasPlatformBoundaryOk = GetAttachPlatformSeams(phase16CanvasProof);

Console.WriteLine($"SURFACE_CANVAS_OK:{ReferenceEquals(factoryEditor, standaloneCanvas.ViewModel)}:{standaloneCanvas.EnableDefaultContextMenu}:{standaloneCanvas.EnableDefaultCommandShortcuts}:{standaloneCanvasOptOut.EnableDefaultContextMenu}:{standaloneCanvasOptOut.EnableDefaultCommandShortcuts}");
Console.WriteLine($"SURFACE_INSPECTOR_OK:{ReferenceEquals(factoryEditor, standaloneInspector.Editor)}");
Console.WriteLine($"SURFACE_MINIMAP_OK:{ReferenceEquals(factoryEditor, standaloneMiniMap.ViewModel)}:{standaloneMiniMap.Focusable}");
Console.WriteLine($"MENU_STOCK_PRESENTER_OK:{typeof(GraphContextMenuPresenter).IsPublic}");
Console.WriteLine($"PRESENTER_STOCK_DEFAULT_OK:{factoryView.Presentation is null}:{standaloneCanvas.NodeVisualPresenter is null}:{standaloneCanvas.ContextMenuPresenter is null}:{standaloneInspector.InspectorPresenter is null}:{standaloneMiniMap.MiniMapPresenter is null}");
Console.WriteLine($"PRESENTER_FULLSHELL_OK:{ReferenceEquals(customPresentation, customView.Presentation)}:{ReferenceEquals(customNodePresenter, customFullShellCanvas.NodeVisualPresenter)}:{ReferenceEquals(customMenuPresenter, customFullShellCanvas.ContextMenuPresenter)}:{ReferenceEquals(customInspectorPresenter, customFullShellInspector.InspectorPresenter)}:{ReferenceEquals(customMiniMapPresenter, customFullShellMiniMap.MiniMapPresenter)}");
Console.WriteLine($"PRESENTER_NODE_OK:{ReferenceEquals(customNodePresenter, customStandaloneCanvas.NodeVisualPresenter)}");
Console.WriteLine($"PRESENTER_MENU_OK:{ReferenceEquals(customMenuPresenter, customStandaloneCanvas.ContextMenuPresenter)}");
Console.WriteLine($"PRESENTER_INSPECTOR_OK:{ReferenceEquals(customInspectorPresenter, customStandaloneInspector.InspectorPresenter)}");
Console.WriteLine($"PRESENTER_MINIMAP_OK:{ReferenceEquals(customMiniMapPresenter, customStandaloneMiniMap.MiniMapPresenter)}");
Console.WriteLine("PHASE16_ADAPTER_BOUNDARY_NOTE:full shell and standalone surfaces now share canonical menu and shortcut routing while keeping shell-owned and standalone-owned platform seams distinct.");
Console.WriteLine($"PHASE16_MENU_ROUTE_OK:{phase16ShellMenuOk}:{phase16CanvasMenuOk}:{phase16ShellPresenter.CanonicalOpenCalls}:{phase16CanvasPresenter.CanonicalOpenCalls}:{phase16ShellPresenter.CompatibilityOpenCalls}:{phase16CanvasPresenter.CompatibilityOpenCalls}");
Console.WriteLine($"PHASE16_SHORTCUT_ROUTE_OK:{phase16ShellShortcutOk}:{phase16CanvasShortcutOk}");
Console.WriteLine($"PHASE16_PLATFORM_BOUNDARY_OK:{phase16ShellPlatformBoundaryOk}:{phase16CanvasPlatformBoundaryOk}");

factoryEditor.SelectSingleNode(factoryEditor.Nodes.Single(node => node.Id == sourceNodeId), updateStatus: false);
factoryEditor.StartConnection(sourceNodeId, sourcePortId);
await factoryEditor.CopySelectionAsync();
factoryEditor.ExportSelectionFragment();
legacyEditor.Session.Commands.SaveWorkspace();

var legacyInspection = legacyEditor.Session.Diagnostics.CaptureInspectionSnapshot();
var legacyRecentDiagnostics = legacyEditor.Session.Diagnostics.GetRecentDiagnostics(10);
var factoryInspection = factoryEditor.Session.Diagnostics.CaptureInspectionSnapshot();
var factoryRecentDiagnostics = factoryEditor.Session.Diagnostics.GetRecentDiagnostics(10);

var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    WorkspaceService = workspaceService,
    FragmentWorkspaceService = fragmentWorkspaceService,
    StyleOptions = styleOptions,
    BehaviorOptions = behaviorOptions,
    FragmentLibraryService = fragmentLibraryService,
    ClipboardPayloadSerializer = serializer,
    DiagnosticsSink = diagnostics,
    Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
    ContextMenuAugmentor = menuAugmentor,
    NodePresentationProvider = presentationProvider,
    LocalizationProvider = localizationProvider,
    PluginRegistrations = pluginRegistrations,
});

var commandIds = new List<string>();
var documentChanges = 0;
var viewportChanges = 0;
var pendingConnectionChanges = 0;
var automationStartedCount = 0;
var automationCompletedCount = 0;
var automationProgressCommandIds = new List<string>();
var automationGenericCommandIds = new List<string>();
string? failureCode = null;

session.Events.CommandExecuted += (_, args) =>
{
    commandIds.Add(args.CommandId);
    if (string.Equals(args.MutationLabel, SmokeAutomationRunId, StringComparison.Ordinal))
    {
        automationGenericCommandIds.Add(args.CommandId);
    }
};
session.Events.DocumentChanged += (_, _) => documentChanges++;
session.Events.ViewportChanged += (_, _) => viewportChanges++;
session.Events.PendingConnectionChanged += (_, _) => pendingConnectionChanges++;
session.Events.RecoverableFailure += (_, args) => failureCode = args.Code;
session.Events.AutomationStarted += (_, args) =>
{
    if (string.Equals(args.RunId, SmokeAutomationRunId, StringComparison.Ordinal))
    {
        automationStartedCount++;
    }
};
session.Events.AutomationProgress += (_, args) =>
{
    if (string.Equals(args.RunId, SmokeAutomationRunId, StringComparison.Ordinal))
    {
        automationProgressCommandIds.Add(args.Step.CommandId);
    }
};
session.Events.AutomationCompleted += (_, args) =>
{
    if (string.Equals(args.Result.RunId, SmokeAutomationRunId, StringComparison.Ordinal))
    {
        automationCompletedCount++;
    }
};

session.Commands.UpdateViewportSize(1280, 720);
session.Commands.SetSelection([sourceNodeId], sourceNodeId, updateStatus: false);
session.Commands.SetNodePositions(
    [
        new NodePositionSnapshot(sourceNodeId, new GraphPoint(96, 120)),
        new NodePositionSnapshot(targetNodeId, new GraphPoint(392, 120)),
    ],
    updateStatus: false);
var compatibleTargets = session.Queries.GetCompatiblePortTargets(sourceNodeId, sourcePortId);
session.Commands.StartConnection(sourceNodeId, sourcePortId);
session.Commands.CancelPendingConnection();
using (session.BeginMutation("smoke-batch"))
{
    session.Commands.AddNode(targetDefinitionId, new GraphPoint(620, 160));
    session.Commands.StartConnection(sourceNodeId, sourcePortId);
    session.Commands.CompleteConnection(targetNodeId, "in");
    session.Commands.CenterViewOnNode(targetNodeId);
    session.Commands.PanBy(12, 18);
}
var runtimeAutomationResult = session.Automation.Execute(CreateAutomationRunRequest(SmokeAutomationRunId, SmokePluginDefinitionIdValue));
session.Commands.SetSelection([targetNodeId], targetNodeId, updateStatus: false);
session.Commands.SaveWorkspace();

var runtimeCommandDescriptors = session.Queries.GetCommandDescriptors();
var runtimeCanvasMenuDescriptors = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(32, 48)));
var legacyCanvasMenuDescriptors = legacyEditor.Session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(32, 48)));
var factoryCanvasMenuDescriptors = factoryEditor.Session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(32, 48)));
var legacyCanvasMenu = legacyEditor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(32, 48)));
var factoryCanvasMenu = factoryEditor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(32, 48)));
var sessionPluginLoadSnapshots = session.Queries.GetPluginLoadSnapshots();
var factoryPluginLoadSnapshots = factoryEditor.Session.Queries.GetPluginLoadSnapshots();
var sessionInspection = session.Diagnostics.CaptureInspectionSnapshot();
var sessionRecentDiagnostics = session.Diagnostics.GetRecentDiagnostics(10);
var runtimeSessionIsKernelFirst = !session
    .GetType()
    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
    .Any(field => field.FieldType == typeof(GraphEditorViewModel));
var detachedSnapshot = session.Queries.CreateDocumentSnapshot();
if (detachedSnapshot.Nodes is List<GraphNode> detachedNodes)
{
    detachedNodes.Add(
        new GraphNode(
            "smoke-detached-001",
            "Detached",
            "Smoke",
            "Safety",
            "Detached snapshot mutation probe.",
            new GraphPoint(0, 0),
            new GraphSize(100, 80),
            [],
            [],
            "#FFFFFF",
            sourceDefinitionId));
}
var runtimeSnapshotDetached = !session.Queries.CreateDocumentSnapshot().Nodes.Any(node => node.Id == "smoke-detached-001");
var legacySessionHost = legacyEditor.Session.GetType()
    .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
    .GetValue(legacyEditor.Session);
var factorySessionHost = factoryEditor.Session.GetType()
    .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
    .GetValue(factoryEditor.Session);
var legacyRetainedSessionIsAdapterBacked = legacySessionHost is not null && legacySessionHost is not GraphEditorViewModel;
var factoryRetainedSessionIsAdapterBacked = factorySessionHost is not null && factorySessionHost is not GraphEditorViewModel;
var legacyRetainedMenuIsDescriptorBacked = legacyCanvasMenuDescriptors.All(descriptor => legacyCanvasMenu.Any(item => item.Id == descriptor.Id));
var factoryRetainedMenuIsDescriptorBacked = factoryCanvasMenuDescriptors.All(descriptor => factoryCanvasMenu.Any(item => item.Id == descriptor.Id));
var legacyDirectViewSnapshot = CaptureViewRouteSnapshot(legacyDirectView);
var legacyFactoryViewSnapshot = CaptureViewRouteSnapshot(legacyView);
var factoryDirectViewSnapshot = CaptureViewRouteSnapshot(factoryDirectView);
var factoryFactoryViewSnapshot = CaptureViewRouteSnapshot(factoryView);
var runtimeSharedCanonicalCommandIds = CaptureSharedCanonicalCommandIds(session);
var legacySharedCanonicalCommandIds = CaptureSharedCanonicalCommandIds(legacyEditor.Session);
var factorySharedCanonicalCommandIds = CaptureSharedCanonicalCommandIds(factoryEditor.Session);
var legacyCompatibilityOnlyCommands = CaptureCompatibilityOnlyCommandIds(session, legacyEditor.Session);
var factoryCompatibilityOnlyCommands = CaptureCompatibilityOnlyCommandIds(session, factoryEditor.Session);
var phase17RouteSignalOk = legacyDirectViewSnapshot == legacyFactoryViewSnapshot
    && legacyDirectViewSnapshot == factoryDirectViewSnapshot
    && legacyDirectViewSnapshot == factoryFactoryViewSnapshot;
var phase17SharedCanonicalOk = runtimeSharedCanonicalCommandIds.SequenceEqual(legacySharedCanonicalCommandIds, StringComparer.Ordinal)
    && runtimeSharedCanonicalCommandIds.SequenceEqual(factorySharedCanonicalCommandIds, StringComparer.Ordinal);
var phase17LegacyCompatibilityWindowOk = legacyCompatibilityOnlyCommands.Contains("nodes.duplicate", StringComparer.Ordinal);
var phase17FactoryCompatibilityWindowOk = factoryCompatibilityOnlyCommands.Contains("nodes.duplicate", StringComparer.Ordinal);
var runtimeReadinessDescriptors = CaptureReadinessDescriptors(session);
var legacyReadinessDescriptors = CaptureReadinessDescriptors(legacyEditor.Session);
var factoryReadinessDescriptors = CaptureReadinessDescriptors(factoryEditor.Session);
var phase18ReadinessDescriptorOk = runtimeReadinessDescriptors.Count == ReadinessFeatureCount
    && runtimeReadinessDescriptors.All(descriptor => descriptor.IsAvailable);
var phase18CanonicalReadinessParityOk = runtimeReadinessDescriptors.SequenceEqual(factoryReadinessDescriptors);
var legacyReadinessById = legacyReadinessDescriptors.ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
var phase18LegacyReadinessWindowOk =
    legacyReadinessDescriptors.Count == ReadinessFeatureCount
    && legacyReadinessById["service.workspace"].IsAvailable
    && legacyReadinessById["service.fragment-workspace"].IsAvailable
    && legacyReadinessById["service.fragment-library"].IsAvailable
    && legacyReadinessById["service.clipboard-payload-serializer"].IsAvailable
    && legacyReadinessById["service.diagnostics"].IsAvailable
    && legacyReadinessById["integration.context-menu-augmentor"].IsAvailable
    && legacyReadinessById["integration.node-presentation-provider"].IsAvailable
    && legacyReadinessById["integration.localization-provider"].IsAvailable
    && legacyReadinessById["integration.diagnostics-sink"].IsAvailable
    && !legacyReadinessById["integration.instrumentation.logger"].IsAvailable
    && !legacyReadinessById["integration.instrumentation.activity-source"].IsAvailable;
var phase18AutomationRuntimeOk = sessionInspection.FeatureDescriptors.Any(descriptor => descriptor.Id == "query.feature-descriptors" && descriptor.IsAvailable)
    && sessionInspection.FeatureDescriptors.Any(descriptor => descriptor.Id == "surface.mutation.batch" && descriptor.IsAvailable)
    && sessionRecentDiagnostics.Count > 0
    && runtimeSessionIsKernelFirst;
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
var automationDiagnosticCodes = sessionRecentDiagnostics
    .Where(diagnostic => diagnostic.Code.StartsWith("automation.", StringComparison.Ordinal))
    .Select(diagnostic => diagnostic.Code)
    .ToArray();
var runtimePluginMenuOk = runtimeCanvasMenuDescriptors.Any(descriptor => descriptor.Id == SmokePluginMenuId);
var factoryPluginMenuOk = factoryCanvasMenuDescriptors.Any(descriptor => descriptor.Id == SmokePluginMenuId);
var runtimePluginLocalizationOk = runtimeCanvasMenuDescriptors.Any(descriptor => descriptor.Id == "canvas-add-node" && descriptor.Header == "Smoke Plugin Add Node");
var factoryPluginLocalizationOk = factoryCanvasMenuDescriptors.Any(descriptor => descriptor.Id == "canvas-add-node" && descriptor.Header == "Smoke Plugin Add Node");
var factoryPluginBadgeOk = factoryEditor.Nodes.Single(node => node.Id == sourceNodeId).Presentation.TopRightBadges.Any(badge => badge.Text == "Plugin");
var phase25PackagePluginOk = sessionPluginLoadSnapshots.SequenceEqual(factoryPluginLoadSnapshots)
    && sessionPluginLoadSnapshots.Count == 1
    && sessionPluginLoadSnapshots[0].SourceKind == GraphEditorPluginLoadSourceKind.Direct
    && sessionPluginLoadSnapshots[0].Status == GraphEditorPluginLoadStatus.Loaded
    && sessionPluginLoadSnapshots[0].Descriptor?.Id == "smoke.proof.plugin"
    && runtimePluginMenuOk
    && factoryPluginMenuOk
    && runtimePluginLocalizationOk
    && factoryPluginLocalizationOk
    && factoryPluginBadgeOk;
var phase25PackageAutomationOk = runtimeAutomationResult.Succeeded
    && automationStartedCount == 1
    && automationCompletedCount == 1
    && automationProgressCommandIds.SequenceEqual(expectedAutomationCommandIds, StringComparer.Ordinal)
    && automationGenericCommandIds.SequenceEqual(expectedAutomationCommandIds, StringComparer.Ordinal)
    && automationDiagnosticCodes.Contains("automation.run.started", StringComparer.Ordinal)
    && automationDiagnosticCodes.Contains("automation.run.completed", StringComparer.Ordinal)
    && runtimeAutomationResult.Inspection.Document.Nodes.Any(node => node.DefinitionId is { Value: SmokePluginDefinitionIdValue });
Console.WriteLine($"SESSION_FACTORY_OK:{session.Queries.CreateDocumentSnapshot().Nodes.Count}:{string.Join(",", commandIds)}");
Console.WriteLine($"SESSION_EVENTS_OK:{documentChanges}:{viewportChanges}:{failureCode ?? "<none>"}");
Console.WriteLine($"KERNEL_SESSION_OK:{runtimeSessionIsKernelFirst}");
Console.WriteLine($"RETAINED_ADAPTER_OK:{legacyRetainedSessionIsAdapterBacked}:{factoryRetainedSessionIsAdapterBacked}");
Console.WriteLine($"RUNTIME_READONLY_OK:{runtimeSnapshotDetached}");
Console.WriteLine($"COMMAND_DESCRIPTOR_OK:{runtimeCommandDescriptors.Any(descriptor => descriptor.Id == "nodes.add" && descriptor.IsEnabled)}:{legacyEditor.Session.Queries.GetCommandDescriptors().Any(descriptor => descriptor.Id == "nodes.add")}:{factoryEditor.Session.Queries.GetCommandDescriptors().Any(descriptor => descriptor.Id == "nodes.add")}");
Console.WriteLine($"MENU_DESCRIPTOR_OK:{runtimeCanvasMenuDescriptors.Any(descriptor => descriptor.Id == "canvas-add-node")}:{legacyCanvasMenuDescriptors.Any(descriptor => descriptor.Id == "canvas-add-node")}:{factoryCanvasMenuDescriptors.Any(descriptor => descriptor.Id == "canvas-add-node")}");
Console.WriteLine($"RETAINED_MENU_ADAPTER_OK:{legacyRetainedMenuIsDescriptorBacked}:{factoryRetainedMenuIsDescriptorBacked}");
Console.WriteLine($"RUNTIME_SELECTION_OK:{session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId == targetNodeId}");
Console.WriteLine($"RUNTIME_CONNECTION_OK:{sessionInspection.Document.Connections.Count}");
Console.WriteLine($"RUNTIME_PENDING_EVENT_OK:{pendingConnectionChanges > 0}");
Console.WriteLine($"RUNTIME_DTO_QUERY_OK:{compatibleTargets.Count}:{compatibleTargets[0].NodeId}:{compatibleTargets[0].PortId}");
Console.WriteLine($"RUNTIME_VIEWPORT_OK:{sessionInspection.Viewport.ViewportWidth}:{sessionInspection.Viewport.ViewportHeight}");
Console.WriteLine($"SERVICE_OVERRIDE_OK:{workspaceService.WorkspacePath}:{fragmentWorkspaceService.FragmentPath}:{workspaceService.SaveCalls}:{fragmentWorkspaceService.SaveCalls}:{serializer.SerializeCalls}");
Console.WriteLine($"COMPATIBILITY_SERVICE_OK:{compatibleTargets.Count}:{compatibilityService.EvaluateCalls}");
Console.WriteLine($"STATE_INSPECTOR_OK:{inspectorSummaryOk}:{inspectorDownstreamOk}");
Console.WriteLine($"STATE_HISTORY_OK:{historyDirtyAfterMove}:{historyCanUndoAfterMove}:{!historyProofEditor.IsDirty}:{historyRestoredNode.X == historyProofOrigin.X}:{historyRestoredNode.Y == historyProofOrigin.Y}");
Console.WriteLine($"DIAG_DIAGNOSTICS_SINK_OK:{diagnostics.Diagnostics.Count}:{diagnostics.Diagnostics.LastOrDefault()?.Code ?? "<none>"}");
Console.WriteLine($"DIAG_LEGACY_INSPECTION_OK:{legacyInspection.Document.Nodes.Count}:{legacyInspection.Selection.SelectedNodeIds.Count}:{legacyInspection.PendingConnection.HasPendingConnection}:{legacyInspection.Status.Message}");
Console.WriteLine($"DIAG_FACTORY_INSPECTION_OK:{factoryInspection.Document.Nodes.Count}:{factoryInspection.Selection.SelectedNodeIds.Count}:{factoryInspection.PendingConnection.HasPendingConnection}:{factoryInspection.Status.Message}");
Console.WriteLine($"DIAG_SESSION_INSPECTION_OK:{sessionInspection.Document.Nodes.Count}:{sessionInspection.Selection.SelectedNodeIds.Count}:{sessionInspection.PendingConnection.HasPendingConnection}:{sessionInspection.Status.Message}");
Console.WriteLine($"DIAG_LEGACY_RECENT_OK:{(!legacyRecentDiagnostics.Any() ? "<none>" : string.Join(",", legacyRecentDiagnostics.Select(diagnostic => diagnostic.Code)))}");
Console.WriteLine($"DIAG_FACTORY_RECENT_OK:{(!factoryRecentDiagnostics.Any() ? "<none>" : string.Join(",", factoryRecentDiagnostics.Select(diagnostic => diagnostic.Code)))}");
Console.WriteLine($"DIAG_SESSION_RECENT_OK:{(!sessionRecentDiagnostics.Any() ? "<none>" : string.Join(",", sessionRecentDiagnostics.Select(diagnostic => diagnostic.Code)))}");
Console.WriteLine($"DIAG_INSTRUMENTATION_OK:{(loggerFactory.Entries.Count > 0 && activityOperations.Count > 0)}:{loggerFactory.Entries.Count}:{string.Join(",", activityOperations)}");
Console.WriteLine("PHASE17_MIGRATION_NOTE:CreateSession=canonical-runtime;Create+AsterGraphAvaloniaViewFactory=canonical-hosted-ui;GraphEditorViewModel+GraphEditorView=retained-compatibility");
Console.WriteLine($"PHASE17_ROUTE_SIGNAL_OK:{runtimeSessionIsKernelFirst}:{legacyRetainedSessionIsAdapterBacked}:{factoryRetainedSessionIsAdapterBacked}:{phase17RouteSignalOk}");
Console.WriteLine($"PHASE17_SHARED_CANONICAL_OK:{phase17SharedCanonicalOk}:{phase17LegacyCompatibilityWindowOk}:{phase17FactoryCompatibilityWindowOk}:{runtimeSharedCanonicalCommandIds.Count}:{legacyCompatibilityOnlyCommands.Count}:{factoryCompatibilityOnlyCommands.Count}");
Console.WriteLine($"PHASE18_READINESS_DESCRIPTOR_OK:{phase18ReadinessDescriptorOk}:{phase18CanonicalReadinessParityOk}:{factoryReadinessDescriptors.Count}:{runtimeReadinessDescriptors.Count}");
Console.WriteLine($"PHASE18_LEGACY_WINDOW_OK:{phase18LegacyReadinessWindowOk}:{legacyReadinessDescriptors.Count}:{legacyReadinessDescriptors.Count(descriptor => descriptor.IsAvailable)}");
Console.WriteLine($"PHASE18_AUTOMATION_RUNTIME_OK:{phase18AutomationRuntimeOk}:{sessionInspection.FeatureDescriptors.Count}:{sessionRecentDiagnostics.Count}:{commandIds.Count}");
Console.WriteLine($"PHASE25_PACKAGE_PLUGIN_OK:{phase25PackagePluginOk}:{sessionPluginLoadSnapshots.Count}:{runtimePluginMenuOk}:{runtimePluginLocalizationOk}:{factoryPluginBadgeOk}");
Console.WriteLine($"PHASE25_PACKAGE_AUTOMATION_OK:{phase25PackageAutomationOk}:{runtimeAutomationResult.ExecutedStepCount}:{automationStartedCount}:{automationCompletedCount}:{runtimeAutomationResult.Inspection.Document.Nodes.Count}:{automationDiagnosticCodes.Length}");
Console.WriteLine($"PHASE25_PACKAGE_PROOF_OK:{(phase25PackagePluginOk && phase25PackageAutomationOk)}:{factoryPluginLoadSnapshots.Count}:{runtimeSharedCanonicalCommandIds.Count}:{runtimeReadinessDescriptors.Count}:{factoryReadinessDescriptors.Count}");

static void PrintEditorMarker(string marker, GraphEditorViewModel editor, string targetNodeId)
{
    var menu = editor.BuildContextMenu(new ContextMenuContext(
        ContextMenuTargetKind.Canvas,
        new GraphPoint(32, 48)));
    var node = editor.Nodes.Single(candidate => candidate.Id == targetNodeId);

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
    var canvas = view.FindControl<NodeCanvas>("PART_NodeCanvas")
        ?? throw new InvalidOperationException("Missing PART_NodeCanvas on migration proof view.");
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

static GraphEditorAutomationRunRequest CreateAutomationRunRequest(string runId, string pluginDefinitionId)
    => new(
        runId,
        [
            new GraphEditorAutomationStep("select-source", CreateAutomationCommand("selection.set", ("nodeId", "smoke-source-001"), ("primaryNodeId", "smoke-source-001"), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("add-plugin-node", CreateAutomationCommand("nodes.add", ("definitionId", pluginDefinitionId), ("worldX", "700"), ("worldY", "220"))),
            new GraphEditorAutomationStep("move-plugin-node", CreateAutomationCommand("nodes.move", ("position", "smoke-plugin-001|732|244"), ("updateStatus", "false"))),
            new GraphEditorAutomationStep("resize-viewport", CreateAutomationCommand("viewport.resize", ("width", "1280"), ("height", "720"))),
            new GraphEditorAutomationStep("pan-viewport", CreateAutomationCommand("viewport.pan", ("deltaX", "10"), ("deltaY", "14"))),
            new GraphEditorAutomationStep("start-connection", CreateAutomationCommand("connections.start", ("sourceNodeId", "smoke-source-001"), ("sourcePortId", "out"))),
            new GraphEditorAutomationStep("complete-connection", CreateAutomationCommand("connections.complete", ("targetNodeId", "smoke-plugin-001"), ("targetPortId", "input"))),
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

sealed class SmokeContextMenuAugmentor : IGraphContextMenuAugmentor
{
    public IReadOnlyList<MenuItemDescriptor> Augment(
        GraphEditorViewModel editor,
        ContextMenuContext context,
        IReadOnlyList<MenuItemDescriptor> stockItems)
        => [.. stockItems, new MenuItemDescriptor("smoke.host-action", "Smoke Host Action")];
}

sealed class SmokeProofPlugin : IGraphEditorPlugin
{
    public GraphEditorPluginDescriptor Descriptor { get; } = new(
        "smoke.proof.plugin",
        "Smoke Proof Plugin",
        "Direct-registration proof plugin for package smoke.");

    public void Register(GraphEditorPluginBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.AddNodeDefinitionProvider(new SmokePluginNodeDefinitionProvider());
        builder.AddContextMenuAugmentor(new SmokePluginContextMenuAugmentor());
        builder.AddNodePresentationProvider(new SmokePluginPresentationProvider());
        builder.AddLocalizationProvider(new SmokePluginLocalizationProvider());
    }
}

sealed class SmokePluginNodeDefinitionProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        =>
        [
            new NodeDefinition(
                new NodeDefinitionId("smoke.plugin"),
                "Smoke Plugin Node",
                "Smoke",
                "Plugin proof node",
                [
                    new PortDefinition("input", "Input", new PortTypeId("float"), "#F3B36B"),
                ],
                [],
                description: "Plugin-contributed node definition used by package smoke proof.",
                accentHex: "#8DDCBF"),
        ];
}

sealed class SmokePluginContextMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
{
    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
        => context.StockItems
            .Concat(
            [
                new GraphEditorMenuItemDescriptorSnapshot(
                    "smoke-plugin-menu",
                    "Smoke Plugin Proof",
                    iconKey: "plugin",
                    isEnabled: false),
            ])
            .ToArray();
}

sealed class SmokePluginPresentationProvider : IGraphEditorPluginNodePresentationProvider
{
    public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
        => new(
            TopRightBadges:
            [
                new NodeAdornmentDescriptor("Plugin", "#8DDCBF"),
            ]);
}

sealed class SmokePluginLocalizationProvider : IGraphEditorPluginLocalizationProvider
{
    public string GetString(string key, string fallback)
        => key == "editor.menu.canvas.addNode"
            ? "Smoke Plugin Add Node"
            : fallback;
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

sealed class SmokeNodeVisualPresenter : IGraphNodeVisualPresenter
{
    private readonly DefaultGraphNodeVisualPresenter _stockPresenter = new();

    public GraphNodeVisual Create(GraphNodeVisualContext context)
        => _stockPresenter.Create(context);

    public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        => _stockPresenter.Update(visual, context);
}

sealed class SmokeContextMenuPresenterAdapter : IGraphContextMenuPresenter
{
    private readonly GraphContextMenuPresenter _stockPresenter = new();

    public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
        => _stockPresenter.Open(target, descriptors, style);
}

sealed class Phase16RecordingContextMenuPresenter : IGraphContextMenuPresenter
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

sealed class SmokeInspectorPresenter : IGraphInspectorPresenter
{
    public Control Create(GraphEditorViewModel? editor)
        => new TextBlock
        {
            Text = $"SMOKE INSPECTOR:{editor?.InspectorTitle ?? "<none>"}",
        };
}

sealed class SmokeMiniMapPresenter : IGraphMiniMapPresenter
{
    public Control Create(GraphEditorViewModel? editor)
        => new TextBlock
        {
            Text = $"SMOKE MINIMAP:{editor?.Title ?? "<none>"}",
        };
}

sealed class RecordingWorkspaceService(string workspacePath, bool throwOnSave) : IGraphWorkspaceService
{
    public string WorkspacePath { get; } = workspacePath;

    public int SaveCalls { get; private set; }

    public GraphDocument? LastSaved { get; private set; }

    public void Save(GraphDocument document)
    {
        SaveCalls++;
        LastSaved = document;
        if (throwOnSave)
        {
            throw new InvalidOperationException("package smoke forced workspace failure");
        }
    }

    public GraphDocument Load()
        => LastSaved ?? throw new InvalidOperationException("No saved workspace.");

    public bool Exists()
        => LastSaved is not null;
}

sealed class RecordingFragmentWorkspaceService(string fragmentPath) : IGraphFragmentWorkspaceService
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

sealed class RecordingFragmentLibraryService(string libraryPath) : IGraphFragmentLibraryService
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

sealed class RecordingClipboardPayloadSerializer : IGraphClipboardPayloadSerializer
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

sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
{
    public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

    public void Publish(GraphEditorDiagnostic diagnostic)
        => Diagnostics.Add(diagnostic);
}

static class SmokeSupport
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

sealed class RecordingCompatibilityService : IPortCompatibilityService
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
