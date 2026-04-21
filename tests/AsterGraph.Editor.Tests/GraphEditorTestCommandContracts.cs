namespace AsterGraph.Editor.Tests;

internal static class GraphEditorTestCommandContracts
{
    internal static readonly string[] SharedCanonicalCommandIds =
    [
        "nodes.add",
        "selection.set",
        "selection.delete",
        "nodes.resize-width",
        "nodes.surface.expand",
        "groups.create",
        "groups.collapse",
        "groups.move",
        "groups.resize",
        "connections.start",
        "connections.complete",
        "connections.connect",
        "connections.cancel",
        "connections.delete",
        "connections.disconnect",
        "connections.note.set",
        "connections.reconnect",
        "connections.break-port",
        "nodes.move",
        "viewport.pan",
        "viewport.resize",
        "viewport.center",
        "viewport.fit",
        "viewport.reset",
        "viewport.center-node",
        "composites.wrap-selection",
        "scopes.enter",
        "scopes.exit",
        "workspace.save",
        "workspace.load",
    ];
}
