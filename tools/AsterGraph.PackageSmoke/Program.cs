using System.Diagnostics;
using Avalonia.Controls;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Menus;
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
    ClipboardPayloadSerializer = serializer,
    DiagnosticsSink = diagnostics,
    Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
    ContextMenuAugmentor = menuAugmentor,
    NodePresentationProvider = presentationProvider,
    LocalizationProvider = localizationProvider,
});
var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = factoryEditor,
    ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
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
Console.WriteLine($"SURFACE_FULLSHELL_OK:{ReferenceEquals(factoryEditor, fullShellInspector.Editor)}:{ReferenceEquals(factoryEditor, fullShellMiniMap.ViewModel)}");

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
});

var commandIds = new List<string>();
var documentChanges = 0;
var viewportChanges = 0;
var pendingConnectionChanges = 0;
string? failureCode = null;

session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);
session.Events.DocumentChanged += (_, _) => documentChanges++;
session.Events.ViewportChanged += (_, _) => viewportChanges++;
session.Events.PendingConnectionChanged += (_, _) => pendingConnectionChanges++;
session.Events.RecoverableFailure += (_, args) => failureCode = args.Code;

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
session.Commands.SetSelection([targetNodeId], targetNodeId, updateStatus: false);
session.Commands.SaveWorkspace();

var sessionInspection = session.Diagnostics.CaptureInspectionSnapshot();
var sessionRecentDiagnostics = session.Diagnostics.GetRecentDiagnostics(10);
Console.WriteLine($"SESSION_FACTORY_OK:{session.Queries.CreateDocumentSnapshot().Nodes.Count}:{string.Join(",", commandIds)}");
Console.WriteLine($"SESSION_EVENTS_OK:{documentChanges}:{viewportChanges}:{failureCode ?? "<none>"}");
Console.WriteLine($"RUNTIME_SELECTION_OK:{session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId == targetNodeId}");
Console.WriteLine($"RUNTIME_CONNECTION_OK:{sessionInspection.Document.Connections.Count}");
Console.WriteLine($"RUNTIME_PENDING_EVENT_OK:{pendingConnectionChanges > 0}");
Console.WriteLine($"RUNTIME_DTO_QUERY_OK:{compatibleTargets.Count}:{compatibleTargets[0].NodeId}:{compatibleTargets[0].PortId}");
Console.WriteLine($"RUNTIME_VIEWPORT_OK:{sessionInspection.Viewport.ViewportWidth}:{sessionInspection.Viewport.ViewportHeight}");
Console.WriteLine($"SERVICE_OVERRIDE_OK:{workspaceService.WorkspacePath}:{fragmentWorkspaceService.FragmentPath}:{workspaceService.SaveCalls}:{fragmentWorkspaceService.SaveCalls}:{serializer.SerializeCalls}");
Console.WriteLine($"COMPATIBILITY_SERVICE_OK:{compatibleTargets.Count}:{compatibilityService.EvaluateCalls}");
Console.WriteLine($"DIAG_DIAGNOSTICS_SINK_OK:{diagnostics.Diagnostics.Count}:{diagnostics.Diagnostics.LastOrDefault()?.Code ?? "<none>"}");
Console.WriteLine($"DIAG_LEGACY_INSPECTION_OK:{legacyInspection.Document.Nodes.Count}:{legacyInspection.Selection.SelectedNodeIds.Count}:{legacyInspection.PendingConnection.HasPendingConnection}:{legacyInspection.Status.Message}");
Console.WriteLine($"DIAG_FACTORY_INSPECTION_OK:{factoryInspection.Document.Nodes.Count}:{factoryInspection.Selection.SelectedNodeIds.Count}:{factoryInspection.PendingConnection.HasPendingConnection}:{factoryInspection.Status.Message}");
Console.WriteLine($"DIAG_SESSION_INSPECTION_OK:{sessionInspection.Document.Nodes.Count}:{sessionInspection.Selection.SelectedNodeIds.Count}:{sessionInspection.PendingConnection.HasPendingConnection}:{sessionInspection.Status.Message}");
Console.WriteLine($"DIAG_LEGACY_RECENT_OK:{(!legacyRecentDiagnostics.Any() ? "<none>" : string.Join(",", legacyRecentDiagnostics.Select(diagnostic => diagnostic.Code)))}");
Console.WriteLine($"DIAG_FACTORY_RECENT_OK:{(!factoryRecentDiagnostics.Any() ? "<none>" : string.Join(",", factoryRecentDiagnostics.Select(diagnostic => diagnostic.Code)))}");
Console.WriteLine($"DIAG_SESSION_RECENT_OK:{(!sessionRecentDiagnostics.Any() ? "<none>" : string.Join(",", sessionRecentDiagnostics.Select(diagnostic => diagnostic.Code)))}");
Console.WriteLine($"DIAG_INSTRUMENTATION_OK:{(loggerFactory.Entries.Count > 0 && activityOperations.Count > 0)}:{loggerFactory.Entries.Count}:{string.Join(",", activityOperations)}");

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
