using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Menus;
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
            new NodeDefinitionId("host.sample.source"))
    ],
    []);

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
    behaviorOptions: behavior,
    contextMenuAugmentor: new HostSampleAugmentor());

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

var menu = editor.BuildContextMenu(
    new ContextMenuContext(
        ContextMenuTargetKind.Node,
        new GraphPoint(120, 160),
        selectedNodeId: "sample-source-001",
        selectedNodeIds: ["sample-source-001"],
        clickedNodeId: "sample-source-001"));

Console.WriteLine($"Host sample view type: {typeof(GraphEditorView).FullName}");
Console.WriteLine($"Node count: {editor.Nodes.Count}");
Console.WriteLine($"Position count: {positions.Count}");
Console.WriteLine($"Selected snapshot found: {snapshot is not null}");
Console.WriteLine($"Menu item count: {menu.Count}");
Console.WriteLine($"Last menu item: {menu[^1].Header}");
Console.WriteLine($"ReadOnly host extension allowed: {editor.CommandPermissions.Host.AllowContextMenuExtensions}");

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

        var items = stockItems.ToList();
        items.Add(MenuItemDescriptor.Separator("host-sample-separator"));
        items.Add(
            new MenuItemDescriptor(
                "host-sample-preview",
                "Host Preview",
                new RelayCommand(() => editor.StatusMessage = $"Host preview for {context.ClickedNodeId}"),
                iconKey: "inspect"));
        return items;
    }
}
