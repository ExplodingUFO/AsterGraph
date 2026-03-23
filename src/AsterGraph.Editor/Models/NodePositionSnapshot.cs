using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Models;

/// <summary>
/// 编辑器对外暴露的节点位置快照，可用于宿主持久化当前画布布局。
/// </summary>
public sealed record NodePositionSnapshot
{
    /// <summary>
    /// 初始化节点位置快照。
    /// </summary>
    /// <param name="nodeId">节点实例标识。</param>
    /// <param name="position">节点在画布世界坐标中的位置。</param>
    public NodePositionSnapshot(string nodeId, GraphPoint position)
    {
        NodeId = nodeId;
        Position = position;
    }

    /// <summary>
    /// 节点实例标识。
    /// </summary>
    public string NodeId { get; }

    /// <summary>
    /// 节点在画布世界坐标中的位置。
    /// </summary>
    public GraphPoint Position { get; }
}
