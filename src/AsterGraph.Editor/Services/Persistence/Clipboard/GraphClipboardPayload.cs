using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal sealed record GraphClipboardPayload(
    string Format,
    int SchemaVersion,
    GraphPoint Origin,
    string? PrimaryNodeId,
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections,
    IReadOnlyList<GraphNodeGroup>? Groups = null);

internal sealed record GraphClipboardPayloadLegacy(
    string Format,
    GraphPoint Origin,
    string? PrimaryNodeId,
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections);
