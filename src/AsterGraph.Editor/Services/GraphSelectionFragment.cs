using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 表示当前选择内容的可序列化片段。
/// </summary>
/// <param name="Nodes">片段中的节点集合。</param>
/// <param name="Connections">片段中的连线集合。</param>
/// <param name="Origin">片段的原点坐标。</param>
/// <param name="PrimaryNodeId">可选的主节点实例标识。</param>
public sealed record GraphSelectionFragment(
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphConnection> Connections,
    GraphPoint Origin,
    string? PrimaryNodeId = null);
