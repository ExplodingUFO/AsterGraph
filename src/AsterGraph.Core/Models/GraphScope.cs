namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted graph scope, used for root and composite child graphs.
/// </summary>
/// <param name="Id">Stable scope identifier within the document.</param>
/// <param name="Nodes">Nodes contained in this graph scope.</param>
/// <param name="Connections">Connections contained in this graph scope.</param>
/// <param name="Groups">Optional node groups owned by this graph scope.</param>
public sealed record GraphScope(
    string Id,
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections,
    IReadOnlyList<GraphNodeGroup>? Groups = null);
