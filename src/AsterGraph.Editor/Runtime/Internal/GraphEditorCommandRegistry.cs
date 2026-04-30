namespace AsterGraph.Editor.Runtime.Internal;

internal static class GraphEditorCommandRegistry
{
    private const string CommandRouteSurfaceId = "runtime.session.commands";
    private const string ShortcutSurfaceId = "runtime.keyboard-shortcuts";

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<GraphEditorCommandPlacementSnapshot>> StockPlacementsByCommandId =
        new Dictionary<string, IReadOnlyList<GraphEditorCommandPlacementSnapshot>>(StringComparer.Ordinal)
        {
            ["nodes.add"] =
            [
                ContextMenu("canvas", "canvas-add-node", 0),
            ],
            ["selection.delete"] =
            [
                ContextMenu("selection", "selection-delete", 0),
            ],
            ["fragments.export-selection"] =
            [
                ContextMenu("selection", "selection-export", 10),
            ],
            ["groups.create"] =
            [
                ContextMenu("selection", "selection-create-group", 20),
                Tool("selection", "selection-create-group", 0),
            ],
            ["composites.wrap-selection"] =
            [
                ContextMenu("selection", "selection-wrap-composite", 30),
                Tool("selection", "selection-wrap-composite", 10),
            ],
            ["layout.align-left"] =
            [
                ContextMenu("selection", "selection-align-left", 50),
                Tool("selection", "selection-align-left", 20),
            ],
            ["layout.align-center"] =
            [
                ContextMenu("selection", "selection-align-center", 51),
                Tool("selection", "selection-align-center", 30),
            ],
            ["layout.align-right"] =
            [
                ContextMenu("selection", "selection-align-right", 52),
                Tool("selection", "selection-align-right", 40),
            ],
            ["layout.align-top"] =
            [
                ContextMenu("selection", "selection-align-top", 53),
                Tool("selection", "selection-align-top", 50),
            ],
            ["layout.align-middle"] =
            [
                ContextMenu("selection", "selection-align-middle", 54),
                Tool("selection", "selection-align-middle", 60),
            ],
            ["layout.align-bottom"] =
            [
                ContextMenu("selection", "selection-align-bottom", 55),
                Tool("selection", "selection-align-bottom", 70),
            ],
            ["layout.distribute-horizontal"] =
            [
                ContextMenu("selection", "selection-distribute-horizontal", 70),
                Tool("selection", "selection-distribute-horizontal", 80),
            ],
            ["layout.distribute-vertical"] =
            [
                ContextMenu("selection", "selection-distribute-vertical", 71),
                Tool("selection", "selection-distribute-vertical", 90),
            ],
            ["layout.snap-selection"] =
            [
                ContextMenu("selection", "selection-snap-grid", 80),
                Tool("selection", "selection-snap-grid", 100),
            ],
            ["viewport.fit"] =
            [
                ContextMenu("canvas", "canvas-fit-view", 20),
            ],
            ["viewport.reset"] =
            [
                ContextMenu("canvas", "canvas-reset-view", 30),
            ],
            ["layout.snap-all"] =
            [
                ContextMenu("canvas", "canvas-snap-all-grid", 40),
            ],
            ["scopes.exit"] =
            [
                ContextMenu("canvas", "canvas-return-parent-scope", 50),
            ],
            ["workspace.save"] =
            [
                ContextMenu("canvas", "canvas-save", 60),
            ],
            ["workspace.load"] =
            [
                ContextMenu("canvas", "canvas-load", 70),
            ],
            ["fragments.import"] =
            [
                ContextMenu("canvas", "canvas-import-fragment", 80),
            ],
            ["connections.cancel"] =
            [
                ContextMenu("canvas", "canvas-cancel-pending", 90),
            ],
            ["nodes.inspect"] =
            [
                ContextMenu("node", "node-inspect", 0),
                Tool("node", "node-inspect", 0),
            ],
            ["viewport.center-node"] =
            [
                ContextMenu("node", "node-center", 10),
                Tool("node", "node-center", 10),
            ],
            ["nodes.surface.expand"] =
            [
                ContextMenu("node", "node-toggle-surface-expansion", 20),
                Tool("node", "node-toggle-surface-expansion", 20),
            ],
            ["scopes.enter"] =
            [
                ContextMenu("node", "node-enter-composite-scope", 25),
                Tool("node", "node-enter-composite-scope", 80),
            ],
            ["nodes.delete-by-id"] =
            [
                ContextMenu("node", "node-delete", 30),
                Tool("node", "node-delete", 30),
            ],
            ["nodes.duplicate"] =
            [
                ContextMenu("node", "node-duplicate", 40),
                Tool("node", "node-duplicate", 40),
            ],
            ["connections.disconnect-incoming"] =
            [
                ContextMenu("node", "node-disconnect-in", 50),
                Tool("node", "node-disconnect-incoming", 50),
            ],
            ["connections.disconnect-outgoing"] =
            [
                ContextMenu("node", "node-disconnect-out", 51),
                Tool("node", "node-disconnect-outgoing", 60),
            ],
            ["connections.disconnect-all"] =
            [
                ContextMenu("node", "node-disconnect-all", 52),
                Tool("node", "node-disconnect-all", 70),
            ],
            ["connections.connect"] =
            [
                ContextMenu("node", "node-create-connection", 60),
                ContextMenu("port", "port-compatible-targets", 10),
            ],
            ["connections.start"] =
            [
                ContextMenu("port", "port-start", 0),
            ],
            ["composites.expose-port"] =
            [
                ContextMenu("port", "port-expose-composite-port", 20),
            ],
            ["composites.unexpose-port"] =
            [
                ContextMenu("port", "port-unexpose-composite-port", 20),
            ],
            ["connections.break-port"] =
            [
                ContextMenu("port", "port-break-connections", 0),
            ],
            ["connections.note.set"] =
            [
                ContextMenu("connection", "connection-clear-note", 0),
                Tool("connection", "connection-clear-note", 20),
            ],
            ["connections.reconnect"] =
            [
                ContextMenu("connection", "connection-reconnect", 10),
                Tool("connection", "connection-reconnect", 0),
            ],
            ["connections.disconnect"] =
            [
                ContextMenu("connection", "connection-disconnect", 20),
                Tool("connection", "connection-disconnect", 10),
            ],
        };

    public static IReadOnlyList<GraphEditorCommandRegistryEntrySnapshot> Create(
        IReadOnlyList<GraphEditorCommandDescriptorSnapshot> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        return descriptors
            .GroupBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(descriptor => descriptor.Group, StringComparer.Ordinal)
            .ThenBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .Select(descriptor => new GraphEditorCommandRegistryEntrySnapshot(
                descriptor,
                CreatePlacements(descriptor)))
            .ToList();
    }

    private static IReadOnlyList<GraphEditorCommandPlacementSnapshot> CreatePlacements(
        GraphEditorCommandDescriptorSnapshot descriptor)
    {
        var placements = new List<GraphEditorCommandPlacementSnapshot>
        {
            new(
                GraphEditorCommandSurfaceKind.CommandRoute,
                CommandRouteSurfaceId,
                descriptor.Id),
        };

        if (!string.IsNullOrWhiteSpace(descriptor.DefaultShortcut))
        {
            placements.Add(new GraphEditorCommandPlacementSnapshot(
                GraphEditorCommandSurfaceKind.KeyboardShortcut,
                ShortcutSurfaceId,
                descriptor.DefaultShortcut));
        }

        if (StockPlacementsByCommandId.TryGetValue(descriptor.Id, out var stockPlacements))
        {
            placements.AddRange(stockPlacements);
        }

        return placements
            .OrderBy(placement => placement.SurfaceKind)
            .ThenBy(placement => placement.SurfaceId, StringComparer.Ordinal)
            .ThenBy(placement => placement.Order)
            .ThenBy(placement => placement.PlacementId, StringComparer.Ordinal)
            .ToList();
    }

    private static GraphEditorCommandPlacementSnapshot ContextMenu(string contextKind, string placementId, int order)
        => new(
            GraphEditorCommandSurfaceKind.ContextMenu,
            $"context-menu.{contextKind}",
            placementId,
            order,
            contextKind);

    private static GraphEditorCommandPlacementSnapshot Tool(string contextKind, string placementId, int order)
        => new(
            GraphEditorCommandSurfaceKind.Tool,
            $"tool.{contextKind}",
            placementId,
            order,
            contextKind);
}
