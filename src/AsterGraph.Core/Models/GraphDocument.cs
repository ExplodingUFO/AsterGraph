namespace AsterGraph.Core.Models;

public sealed record GraphDocument(
    string Title,
    string Description,
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections);
