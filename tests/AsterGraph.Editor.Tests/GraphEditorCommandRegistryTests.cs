using System.Reflection;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorCommandRegistryTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.command-registry.node");
    private const string SourceNodeId = "tests.command-registry.source";
    private const string TargetNodeId = "tests.command-registry.target";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void IGraphEditorQueries_DefinesRuntimeCommandRegistryQuery()
    {
        var method = typeof(IGraphEditorQueries).GetMethod(
            nameof(IGraphEditorQueries.GetCommandRegistry),
            BindingFlags.Public | BindingFlags.Instance,
            Type.EmptyTypes);

        Assert.NotNull(method);
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorCommandRegistryEntrySnapshot>),
            method!.ReturnType);
    }

    [Fact]
    public void CommandRegistry_CoversExistingStockCommandDescriptors()
    {
        var session = CreateSession();

        var descriptors = session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var registry = session.Queries.GetCommandRegistry()
            .ToDictionary(entry => entry.CommandId, StringComparer.Ordinal);

        Assert.Equal(descriptors.Keys.Order(StringComparer.Ordinal), registry.Keys.Order(StringComparer.Ordinal));

        foreach (var (commandId, descriptor) in descriptors)
        {
            var entry = registry[commandId];
            Assert.Equal(descriptor.Id, entry.Descriptor.Id);
            Assert.Equal(descriptor.IsEnabled, entry.Descriptor.IsEnabled);
            Assert.Equal(descriptor.DisabledReason, entry.Descriptor.DisabledReason);
            Assert.Equal(descriptor.Title, entry.Title);
            Assert.Equal(descriptor.Group, entry.Group);
            Assert.Equal(descriptor.IconKey, entry.IconKey);
            Assert.Equal(descriptor.DefaultShortcut, entry.DefaultShortcut);
            Assert.Equal(descriptor.Source, entry.Source);
            Assert.Contains(
                entry.Placements,
                placement =>
                    placement.SurfaceKind == GraphEditorCommandSurfaceKind.CommandRoute
                    && placement.SurfaceId == "runtime.session.commands"
                    && placement.PlacementId == commandId);
        }
    }

    [Fact]
    public void CommandRegistry_ExposesStableMenuToolAndShortcutPlacements()
    {
        var session = CreateSession();
        session.Commands.SetSelection([SourceNodeId, TargetNodeId], SourceNodeId, updateStatus: false);

        var registry = session.Queries.GetCommandRegistry()
            .ToDictionary(entry => entry.CommandId, StringComparer.Ordinal);

        AssertPlacement(
            registry["layout.align-left"],
            GraphEditorCommandSurfaceKind.ContextMenu,
            "context-menu.selection",
            "selection-align-left",
            "selection");
        AssertPlacement(
            registry["layout.align-left"],
            GraphEditorCommandSurfaceKind.Tool,
            "tool.selection",
            "selection-align-left",
            "selection");

        AssertPlacement(
            registry["nodes.inspect"],
            GraphEditorCommandSurfaceKind.ContextMenu,
            "context-menu.node",
            "node-inspect",
            "node");
        AssertPlacement(
            registry["connections.disconnect"],
            GraphEditorCommandSurfaceKind.ContextMenu,
            "context-menu.connection",
            "connection-disconnect",
            "connection");

        AssertPlacement(
            registry["workspace.save"],
            GraphEditorCommandSurfaceKind.KeyboardShortcut,
            "runtime.keyboard-shortcuts",
            "Ctrl+S",
            null);
        AssertPlacement(
            registry["workspace.save"],
            GraphEditorCommandSurfaceKind.Workbench,
            "workbench.header",
            "workspace.save",
            null);
        AssertPlacement(
            registry["composites.wrap-selection"],
            GraphEditorCommandSurfaceKind.Workbench,
            "workbench.composite-workflow",
            "composites.wrap-selection",
            null);
        AssertPlacement(
            registry["workspace.save"],
            GraphEditorCommandSurfaceKind.Workbench,
            "workbench.command-palette",
            "workspace.save",
            null);
        AssertPlacement(
            registry["workspace.save"],
            GraphEditorCommandSurfaceKind.Workbench,
            "workbench.shortcut-help",
            "workspace.save",
            null);
        AssertPlacement(
            registry["selection.delete"],
            GraphEditorCommandSurfaceKind.KeyboardShortcut,
            "runtime.keyboard-shortcuts",
            "Delete",
            null);
    }

    [Fact]
    public void CommandRegistry_PlacementsUseExistingSessionCommandIds()
    {
        var session = CreateSession();

        var descriptorIds = session.Queries.GetCommandDescriptors()
            .Select(descriptor => descriptor.Id)
            .ToHashSet(StringComparer.Ordinal);
        var registry = session.Queries.GetCommandRegistry()
            .ToDictionary(entry => entry.CommandId, StringComparer.Ordinal);
        var menuCommandIds = Flatten(session.Queries.BuildContextMenuDescriptors(
                new ContextMenuContext(
                    ContextMenuTargetKind.Selection,
                    new GraphPoint(160, 90),
                    selectedNodeIds: [SourceNodeId, TargetNodeId])))
            .Where(item => item.Command is not null)
            .Select(item => item.Command!.CommandId)
            .ToHashSet(StringComparer.Ordinal);
        var selectionToolCommandIds = session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForSelection([SourceNodeId, TargetNodeId], SourceNodeId))
            .Select(tool => tool.Invocation.CommandId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var entry in registry.Values)
        {
            Assert.Contains(entry.CommandId, descriptorIds);
        }

        Assert.Contains("layout.align-left", menuCommandIds);
        Assert.Contains("layout.align-left", selectionToolCommandIds);
        Assert.Contains(
            registry["layout.align-left"].Placements,
            placement => placement.SurfaceKind == GraphEditorCommandSurfaceKind.ContextMenu);
        Assert.Contains(
            registry["layout.align-left"].Placements,
            placement => placement.SurfaceKind == GraphEditorCommandSurfaceKind.Tool);
    }

    [Fact]
    public void CommandRegistry_SpatialAuthoringCommandsExposeCanonicalRouteMenuAndToolPlacements()
    {
        var session = CreateSession();
        session.Commands.SetSelection([SourceNodeId, TargetNodeId], SourceNodeId, updateStatus: false);

        var descriptors = session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var registry = session.Queries.GetCommandRegistry()
            .ToDictionary(entry => entry.CommandId, StringComparer.Ordinal);
        var selectionMenuCommands = Flatten(session.Queries.BuildContextMenuDescriptors(
                new ContextMenuContext(
                    ContextMenuTargetKind.Selection,
                    new GraphPoint(160, 90),
                    selectedNodeId: SourceNodeId,
                    selectedNodeIds: [SourceNodeId, TargetNodeId])))
            .Where(item => item.Command is not null)
            .ToDictionary(item => item.Command!.CommandId, item => item, StringComparer.Ordinal);
        var selectionTools = session.Queries.GetToolDescriptors(
                GraphEditorToolContextSnapshot.ForSelection([SourceNodeId, TargetNodeId], SourceNodeId))
            .ToDictionary(tool => tool.Invocation.CommandId, tool => tool, StringComparer.Ordinal);
        var spatialCommands = new[]
        {
            ("groups.create", "selection-create-group"),
            ("layout.align-left", "selection-align-left"),
            ("layout.align-center", "selection-align-center"),
            ("layout.align-right", "selection-align-right"),
            ("layout.align-top", "selection-align-top"),
            ("layout.align-middle", "selection-align-middle"),
            ("layout.align-bottom", "selection-align-bottom"),
            ("layout.distribute-horizontal", "selection-distribute-horizontal"),
            ("layout.distribute-vertical", "selection-distribute-vertical"),
            ("layout.snap-selection", "selection-snap-grid"),
        };

        foreach (var (commandId, placementId) in spatialCommands)
        {
            Assert.Equal(commandId.StartsWith("layout.", StringComparison.Ordinal) ? "layout" : "groups", descriptors[commandId].Group);
            Assert.Equal(GraphEditorCommandSourceKind.Kernel, descriptors[commandId].Source);
            Assert.Equal(descriptors[commandId].Id, registry[commandId].Descriptor.Id);
            AssertPlacement(
                registry[commandId],
                GraphEditorCommandSurfaceKind.CommandRoute,
                "runtime.session.commands",
                commandId,
                null);
            AssertPlacement(
                registry[commandId],
                GraphEditorCommandSurfaceKind.ContextMenu,
                "context-menu.selection",
                placementId,
                "selection");
            AssertPlacement(
                registry[commandId],
                GraphEditorCommandSurfaceKind.Tool,
                "tool.selection",
                placementId,
                "selection");
            Assert.Equal(commandId, selectionMenuCommands[commandId].Command!.CommandId);
            Assert.Equal(commandId, selectionTools[commandId].Invocation.CommandId);
        }

        Assert.Contains(
            selectionMenuCommands["groups.create"].Command!.Arguments,
            argument => argument.Name == "title" && argument.Value == "Group");
        Assert.Contains(
            selectionTools["layout.snap-selection"].Invocation.Arguments,
            argument => argument.Name == "gridSize" && argument.Value == "20");
    }

    private static void AssertPlacement(
        GraphEditorCommandRegistryEntrySnapshot entry,
        GraphEditorCommandSurfaceKind surfaceKind,
        string surfaceId,
        string placementId,
        string? contextKind)
        => Assert.Contains(
            entry.Placements,
            placement =>
                placement.SurfaceKind == surfaceKind
                && placement.SurfaceId == surfaceId
                && placement.PlacementId == placementId
                && placement.ContextKind == contextKind);

    private static IEnumerable<GraphEditorMenuItemDescriptorSnapshot> Flatten(
        IEnumerable<GraphEditorMenuItemDescriptorSnapshot> items)
    {
        foreach (var item in items)
        {
            yield return item;
            foreach (var child in Flatten(item.Children))
            {
                yield return child;
            }
        }
    }

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static GraphDocument CreateDocument()
        => new(
            "Command Registry Graph",
            "Runtime command registry contract coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Registry Source",
                    "Tests",
                    "Registry",
                    "Source node for command registry tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    DefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Registry Target",
                    "Tests",
                    "Registry",
                    "Target node for command registry tests.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    DefinitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Registry Node",
            "Tests",
            "Registry",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }
}
