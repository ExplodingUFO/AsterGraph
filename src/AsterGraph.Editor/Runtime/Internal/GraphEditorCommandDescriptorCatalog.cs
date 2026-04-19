namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorCommandDescriptorCatalog
{
    private static readonly IReadOnlyDictionary<string, GraphEditorCommandDescriptorMetadata> MetadataById =
        new Dictionary<string, GraphEditorCommandDescriptorMetadata>(StringComparer.Ordinal)
        {
            ["nodes.add"] = new("Add Node", "nodes", "add", null),
            ["selection.set"] = new("Set Selection", "selection", "select", null),
            ["selection.delete"] = new("Delete Selection", "selection", "delete", "Delete"),
            ["nodes.move"] = new("Move Nodes", "nodes", "move", null),
            ["nodes.parameters.set"] = new("Edit Parameters", "nodes", "parameter", null),
            ["connections.start"] = new("Start Connection", "connections", "connect", null),
            ["connections.complete"] = new("Complete Connection", "connections", "connect", null),
            ["connections.connect"] = new("Create Connection", "connections", "connect", null),
            ["connections.cancel"] = new("Cancel Pending Connection", "connections", "cancel", "Escape"),
            ["connections.delete"] = new("Delete Connection", "connections", "delete", null),
            ["connections.break-port"] = new("Break Port Connections", "connections", "disconnect", null),
            ["connections.disconnect-incoming"] = new("Disconnect Incoming", "connections", "disconnect", null),
            ["connections.disconnect-outgoing"] = new("Disconnect Outgoing", "connections", "disconnect", null),
            ["connections.disconnect-all"] = new("Disconnect All", "connections", "disconnect", null),
            ["history.undo"] = new("Undo", "history", "undo", "Ctrl+Z"),
            ["history.redo"] = new("Redo", "history", "redo", "Ctrl+Y"),
            ["viewport.fit"] = new("Fit View", "viewport", "fit", null),
            ["viewport.pan"] = new("Pan View", "viewport", "pan", null),
            ["viewport.resize"] = new("Resize Viewport", "viewport", "resize", null),
            ["viewport.reset"] = new("Reset View", "viewport", "reset", null),
            ["viewport.center-node"] = new("Center On Node", "viewport", "center", null),
            ["viewport.center"] = new("Center View", "viewport", "center", null),
            ["workspace.save"] = new("Save Workspace", "workspace", "save", "Ctrl+S"),
            ["workspace.load"] = new("Load Workspace", "workspace", "load", "Ctrl+O"),
            ["clipboard.copy"] = new("Copy Selection", "clipboard", "copy", "Ctrl+C"),
            ["clipboard.paste"] = new("Paste Selection", "clipboard", "paste", "Ctrl+V"),
            ["fragments.export-selection"] = new("Export Selection Fragment", "fragments", "export", null),
            ["fragments.import"] = new("Import Fragment", "fragments", "import", null),
            ["layout.align-left"] = new("Align Left", "layout", "align-left", null),
            ["layout.align-center"] = new("Align Center", "layout", "align-center", null),
            ["layout.align-right"] = new("Align Right", "layout", "align-right", null),
            ["layout.align-top"] = new("Align Top", "layout", "align-top", null),
            ["layout.align-middle"] = new("Align Middle", "layout", "align-middle", null),
            ["layout.align-bottom"] = new("Align Bottom", "layout", "align-bottom", null),
            ["layout.distribute-horizontal"] = new("Distribute Horizontally", "layout", "distribute-horizontal", null),
            ["layout.distribute-vertical"] = new("Distribute Vertically", "layout", "distribute-vertical", null),
            ["nodes.inspect"] = new("Inspect Node", "nodes", "inspect", null),
            ["nodes.delete-by-id"] = new("Delete Node", "nodes", "delete", null),
            ["nodes.duplicate"] = new("Duplicate Node", "nodes", "duplicate", null),
        };

    public static GraphEditorCommandDescriptorSnapshot Create(
        string id,
        GraphEditorCommandSourceKind source,
        bool canExecute,
        string? disabledReason = null)
    {
        var metadata = GetMetadata(id);
        return new GraphEditorCommandDescriptorSnapshot(
            id,
            metadata.Title,
            metadata.Group,
            metadata.IconKey,
            metadata.DefaultShortcut,
            source,
            canExecute,
            disabledReason);
    }

    public static string GetTitle(string id) => GetMetadata(id).Title;

    public static string GetGroup(string id) => GetMetadata(id).Group;

    public static string? GetIconKey(string id) => GetMetadata(id).IconKey;

    public static string? GetDefaultShortcut(string id) => GetMetadata(id).DefaultShortcut;

    private static GraphEditorCommandDescriptorMetadata GetMetadata(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var normalizedId = id.Trim();
        return MetadataById.TryGetValue(normalizedId, out var metadata)
            ? metadata
            : CreateFallback(normalizedId);
    }

    private static GraphEditorCommandDescriptorMetadata CreateFallback(string id)
    {
        var parts = id.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var group = parts.Length > 1 ? parts[0] : "command";
        var titleSegment = parts.Length > 0 ? parts[^1] : id;
        return new GraphEditorCommandDescriptorMetadata(Titleize(titleSegment), group, null, null);
    }

    private static string Titleize(string value)
        => string.Join(
            " ",
            value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(segment => segment.Length == 0
                    ? segment
                    : char.ToUpperInvariant(segment[0]) + segment[1..]));

    private sealed record GraphEditorCommandDescriptorMetadata(
        string Title,
        string Group,
        string? IconKey,
        string? DefaultShortcut);
}
