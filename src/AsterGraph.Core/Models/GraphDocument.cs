namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted graph document snapshot.
/// </summary>
/// <param name="Title">Document title shown to hosts and end users.</param>
/// <param name="Description">Optional document description or summary.</param>
/// <param name="Nodes">All nodes contained in the graph.</param>
/// <param name="Connections">All connections contained in the graph.</param>
public sealed record GraphDocument(
    string Title,
    string Description,
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections);
