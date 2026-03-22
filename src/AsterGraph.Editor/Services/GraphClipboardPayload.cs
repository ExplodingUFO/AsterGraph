using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal sealed record GraphClipboardPayload(
    string Format,
    GraphPoint Origin,
    string? PrimaryNodeId,
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections);
