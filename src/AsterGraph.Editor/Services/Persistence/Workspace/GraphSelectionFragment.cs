using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 表示当前选择内容的可序列化片段。
/// </summary>
public sealed record GraphSelectionFragment
{
    public GraphSelectionFragment(
        IReadOnlyList<GraphNode> nodes,
        IReadOnlyList<GraphConnection> connections,
        GraphPoint origin,
        string? primaryNodeId = null,
        IReadOnlyList<GraphNodeGroup>? groups = null)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(connections);

        Nodes = nodes;
        Connections = connections;
        Origin = origin;
        PrimaryNodeId = primaryNodeId;
        Groups = groups ?? [];
    }

    public IReadOnlyList<GraphNode> Nodes { get; init; }

    public IReadOnlyList<GraphConnection> Connections { get; init; }

    public GraphPoint Origin { get; init; }

    public string? PrimaryNodeId { get; init; }

    public IReadOnlyList<GraphNodeGroup> Groups { get; init; }
}
