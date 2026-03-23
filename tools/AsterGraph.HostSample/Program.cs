using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.ViewModels;

var catalog = new NodeCatalog();
catalog.RegisterProvider(new HostSampleNodeDefinitionProvider());

var document = new GraphDocument(
    "Host Sample Graph",
    "Minimal host composition sample for AsterGraph.",
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
    ],
    []);

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

var behavior = GraphEditorBehaviorOptions.Default with
{
    Commands = permissions,
};

var editor = new GraphEditorViewModel(
    document,
    catalog,
    new DefaultPortCompatibilityService(),
    styleOptions: style,
    behaviorOptions: behavior,
    contextMenuAugmentor: new HostSampleAugmentor(),
    nodePresentationProvider: new HostSamplePresentationProvider(),
    localizationProvider: new HostSampleLocalizationProvider());

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

var node = editor.Nodes[0];
var ownerMatched = menuContext.TryGetOwner<HostSampleOwner>(out var typedOwner);
var topLevelMatched = menuContext.TryGetTopLevel<HostSampleTopLevel>(out var typedTopLevel);

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
                description: "Minimal sample node definition used by the host sample.",
                accentHex: "#6AD5C4",
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

internal sealed record HostSampleGraphHostContext(HostSampleOwner Owner, HostSampleTopLevel? TopLevel) : IGraphHostContext
{
    object IGraphHostContext.Owner => Owner;

    object? IGraphHostContext.TopLevel => TopLevel;

    public IServiceProvider? Services => null;
}

internal sealed record HostSampleOwner(string Id, string DisplayName);

internal sealed record HostSampleTopLevel(string Id, string WindowTitle);
