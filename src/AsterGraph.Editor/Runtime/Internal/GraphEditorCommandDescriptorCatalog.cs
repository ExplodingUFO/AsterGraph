namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorCommandDescriptorCatalog
{
    private static readonly IReadOnlyDictionary<string, GraphEditorCommandDescriptorMetadata> MetadataById =
        new Dictionary<string, GraphEditorCommandDescriptorMetadata>(StringComparer.Ordinal)
        {
            ["nodes.add"] = new("Add Node", "nodes", "add", null),
            ["nodes.insert-into-connection"] = new("Insert Node Into Connection", "nodes", "insert", null),
            ["selection.set"] = new("Set Selection", "selection", "select", null),
            ["selection.connections.set"] = new("Set Connection Selection", "selection", "select", null),
            ["selection.clear"] = new("Clear Selection", "selection", "select", "Escape"),
            ["selection.delete"] = new("Delete Selection", "selection", "delete", "Delete"),
            ["selection.delete-reconnect"] = new("Delete And Reconnect", "selection", "connect", null),
            ["selection.detach-connections"] = new("Detach And Reconnect", "selection", "disconnect", null),
            ["nodes.move"] = new("Move Nodes", "nodes", "move", null),
            ["nodes.resize"] = new("Resize Node", "nodes", "resize", null),
            ["nodes.resize-width"] = new("Resize Node Width", "nodes", "resize-width", null),
            ["nodes.surface.expand"] = new("Toggle Node Expansion", "nodes", "expand", null),
            ["nodes.parameters.set"] = new("Edit Parameters", "nodes", "parameter", null),
            ["groups.create"] = new("Create Group", "groups", "group-create", null),
            ["groups.collapse"] = new("Toggle Group Collapse", "groups", "group-collapse", null),
            ["groups.move"] = new("Move Group", "groups", "group-move", null),
            ["groups.resize"] = new("Resize Group", "groups", "group-resize", null),
            ["groups.promote"] = new("Promote Group To Composite", "groups", "group-promote", null),
            ["composites.wrap-selection"] = new("Wrap Selection To Composite", "composites", "composite-wrap", null),
            ["composites.expose-port"] = new("Expose Composite Port", "composites", "composite-expose", null),
            ["composites.unexpose-port"] = new("Unexpose Composite Port", "composites", "composite-unexpose", null),
            ["scopes.enter"] = new("Enter Composite Scope", "scopes", "scope-enter", null),
            ["scopes.exit"] = new("Return To Parent Scope", "scopes", "scope-exit", null),
            ["connections.start"] = new("Start Connection", "connections", "connect", null),
            ["connections.complete"] = new("Complete Connection", "connections", "connect", null),
            ["connections.connect"] = new("Create Connection", "connections", "connect", null),
            ["connections.cancel"] = new("Cancel Pending Connection", "connections", "cancel", "Escape"),
            ["connections.delete"] = new("Delete Connection", "connections", "delete", null),
            ["connections.delete-selected"] = new("Delete Selected Connections", "connections", "delete", null),
            ["connections.disconnect"] = new("Disconnect Connection", "connections", "disconnect", null),
            ["connections.slice"] = new("Slice Connections", "connections", "slice", null),
            ["connections.label.set"] = new("Set Connection Label", "connections", "rename", null),
            ["connections.note.set"] = new("Set Connection Note", "connections", "inspect", null),
            ["connections.route-vertex.insert"] = new("Insert Connection Route Vertex", "connections", "add", null),
            ["connections.route-vertex.move"] = new("Move Connection Route Vertex", "connections", "move", null),
            ["connections.route-vertex.remove"] = new("Remove Connection Route Vertex", "connections", "delete", null),
            ["connections.reconnect"] = new("Reconnect Connection", "connections", "connect", null),
            ["connections.break-port"] = new("Break Port Connections", "connections", "disconnect", null),
            ["connections.disconnect-incoming"] = new("Disconnect Incoming", "connections", "disconnect", null),
            ["connections.disconnect-outgoing"] = new("Disconnect Outgoing", "connections", "disconnect", null),
            ["connections.disconnect-all"] = new("Disconnect All", "connections", "disconnect", null),
            ["history.undo"] = new("Undo", "history", "undo", "Ctrl+Z"),
            ["history.redo"] = new("Redo", "history", "redo", "Ctrl+Y"),
            ["viewport.fit"] = new("Fit View", "viewport", "fit", null),
            ["viewport.fit-selection"] = new("Fit Selection", "viewport", "fit-selection", null),
            ["viewport.focus-selection"] = new("Focus Selection", "viewport", "focus", null),
            ["viewport.focus-current-scope"] = new("Focus Current Scope", "viewport", "focus", null),
            ["viewport.pan"] = new("Pan View", "viewport", "pan", null),
            ["viewport.resize"] = new("Resize Viewport", "viewport", "resize", null),
            ["viewport.reset"] = new("Reset View", "viewport", "reset", null),
            ["viewport.center-node"] = new("Center On Node", "viewport", "center", null),
            ["viewport.center"] = new("Center View", "viewport", "center", null),
            ["workspace.save"] = new("Save Workspace", "workspace", "save", "Ctrl+S"),
            ["workspace.load"] = new("Load Workspace", "workspace", "load", "Ctrl+O"),
            ["clipboard.copy"] = new("Copy Selection", "clipboard", "copy", "Ctrl+C"),
            ["clipboard.paste"] = new("Paste Selection", "clipboard", "paste", "Ctrl+V"),
            ["export.scene-svg"] = new("Export Scene As SVG", "export", "export", null),
            ["export.scene-image"] = new("Export Scene As Image", "export", "export", null),
            ["fragments.export-selection"] = new("Export Selection Fragment", "fragments", "export", null),
            ["fragments.import"] = new("Import Fragment", "fragments", "import", null),
            ["fragments.clear-workspace"] = new("Clear Saved Fragment", "fragments", "delete", null),
            ["fragments.export-template"] = new("Export Selection As Template", "fragments", "template", null),
            ["fragments.apply-template-preset"] = new("Apply Template Preset", "fragments", "template", null),
            ["layout.align-left"] = new("Align Left", "layout", "align-left", null),
            ["layout.align-center"] = new("Align Center", "layout", "align-center", null),
            ["layout.align-right"] = new("Align Right", "layout", "align-right", null),
            ["layout.align-top"] = new("Align Top", "layout", "align-top", null),
            ["layout.align-middle"] = new("Align Middle", "layout", "align-middle", null),
            ["layout.align-bottom"] = new("Align Bottom", "layout", "align-bottom", null),
            ["layout.distribute-horizontal"] = new("Distribute Horizontally", "layout", "distribute-horizontal", null),
            ["layout.distribute-vertical"] = new("Distribute Vertically", "layout", "distribute-vertical", null),
            ["layout.apply-plan"] = new("Apply Layout Plan", "layout", "layout", null),
            ["layout.snap-selection"] = new("Snap Selection To Grid", "layout", "snap", null),
            ["layout.snap-all"] = new("Snap All Nodes To Grid", "layout", "snap", null),
            ["nodes.inspect"] = new("Inspect Node", "nodes", "inspect", null),
            ["nodes.delete-by-id"] = new("Delete Node", "nodes", "delete", null),
            ["nodes.duplicate"] = new("Duplicate Node", "nodes", "duplicate", null),
        };

    public static GraphEditorCommandDescriptorSnapshot Create(
        string id,
        GraphEditorCommandSourceKind source,
        bool canExecute,
        string? disabledReason = null,
        string? recoveryHint = null,
        string? recoveryCommandId = null)
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
            disabledReason,
            recoveryHint,
            recoveryCommandId);
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
