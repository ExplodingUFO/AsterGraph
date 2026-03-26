using Avalonia.Controls;
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

var runtimeCompatibility = new RecordingCompatibilityService();
var runtimeDiagnostics = new RecordingDiagnosticsSink();
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
var permissions = GraphEditorCommandPermissions.ReadOnly with
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

var runtimeSession = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = runtimeCompatibility,
    WorkspaceService = runtimeWorkspace,
    DiagnosticsSink = runtimeDiagnostics,
    StyleOptions = style,
    BehaviorOptions = runtimeBehavior,
});

var commandMarkers = new List<string>();
var documentChangeKinds = new List<string>();
var viewportChanges = 0;
GraphEditorRecoverableFailureEventArgs? runtimeFailure = null;

runtimeSession.Events.CommandExecuted += (_, args) =>
    commandMarkers.Add($"{args.CommandId}:{args.MutationLabel ?? "<none>"}:{args.IsInMutationScope}");
runtimeSession.Events.DocumentChanged += (_, args) => documentChangeKinds.Add(args.ChangeKind.ToString());
runtimeSession.Events.ViewportChanged += (_, _) => viewportChanges++;
runtimeSession.Events.RecoverableFailure += (_, args) => runtimeFailure = args;

var compatibleTargets = runtimeSession.Queries.GetCompatibleTargets("sample-source-001", "result");
using (runtimeSession.BeginMutation("host-sample-batch"))
{
    runtimeSession.Commands.AddNode(new NodeDefinitionId("host.sample.sink"), new GraphPoint(680, 200));
    runtimeSession.Commands.PanBy(12, 18);
}
runtimeSession.Commands.SaveWorkspace();

var runtimeSnapshot = runtimeSession.Queries.CreateDocumentSnapshot();
var runtimeViewport = runtimeSession.Queries.GetViewportSnapshot();
var runtimeCapabilities = runtimeSession.Queries.GetCapabilitySnapshot();

Console.WriteLine($"Session title: {runtimeSnapshot.Title}");
Console.WriteLine($"Session node count after commands: {runtimeSnapshot.Nodes.Count}");
Console.WriteLine($"Session compatible targets: {compatibleTargets.Count}");
Console.WriteLine($"Session command markers: {string.Join(", ", commandMarkers)}");
Console.WriteLine($"Session document changes: {string.Join(", ", documentChangeKinds)}");
Console.WriteLine($"Session viewport events: {viewportChanges}");
Console.WriteLine($"Session viewport snapshot: zoom={runtimeViewport.Zoom:0.00}, pan={runtimeViewport.PanX:0},{runtimeViewport.PanY:0}");
Console.WriteLine($"Session capabilities: save={runtimeCapabilities.CanSaveWorkspace}, load={runtimeCapabilities.CanLoadWorkspace}");
Console.WriteLine($"Session recoverable failure: {runtimeFailure?.Code ?? "<none>"}");
Console.WriteLine($"Diagnostics sink codes: {string.Join(", ", runtimeDiagnostics.Diagnostics.Select(diagnostic => diagnostic.Code))}");
Console.WriteLine($"Runtime compatibility evaluations: {runtimeCompatibility.EvaluateCalls}");
Console.WriteLine($"Runtime workspace override path: {runtimeWorkspace.WorkspacePath}");

var viewCompatibility = new RecordingCompatibilityService();
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = viewCompatibility,
    StyleOptions = style,
    BehaviorOptions = viewBehavior,
    ContextMenuAugmentor = new HostSampleAugmentor(),
    NodePresentationProvider = new HostSamplePresentationProvider(),
    LocalizationProvider = new HostSampleLocalizationProvider(),
});

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
var ownerMatched = menuContext.TryGetOwner<HostSampleOwner>(out var typedOwner);
var topLevelMatched = menuContext.TryGetTopLevel<HostSampleTopLevel>(out var typedTopLevel);
var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.Default,
});
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

Console.WriteLine($"Host sample view type: {typeof(GraphEditorView).FullName}");
Console.WriteLine($"Node count: {editor.Nodes.Count}");
Console.WriteLine($"Position count: {positions.Count}");
Console.WriteLine($"Selected snapshot found: {snapshot is not null}");
Console.WriteLine($"Host preview menu item exists: {hostPreviewItem is not null}");
Console.WriteLine($"Host preview menu item header: {hostPreviewItem?.Header ?? "<missing>"}");
Console.WriteLine($"ReadOnly host extension allowed: {editor.CommandPermissions.Host.AllowContextMenuExtensions}");
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

internal sealed record HostSampleGraphHostContext(HostSampleOwner Owner, HostSampleTopLevel? TopLevel) : IGraphHostContext
{
    object IGraphHostContext.Owner => Owner;

    object? IGraphHostContext.TopLevel => TopLevel;

    public IServiceProvider? Services => null;
}

internal sealed record HostSampleOwner(string Id, string DisplayName);

internal sealed record HostSampleTopLevel(string Id, string WindowTitle);
