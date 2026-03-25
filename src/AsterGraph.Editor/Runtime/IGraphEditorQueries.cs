using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 定义宿主可见的图编辑器查询入口。
/// </summary>
public interface IGraphEditorQueries
{
    /// <summary>
    /// 生成当前图文档快照。
    /// </summary>
    /// <returns>当前不可变图文档。</returns>
    GraphDocument CreateDocumentSnapshot();

    /// <summary>
    /// 获取全部节点位置快照。
    /// </summary>
    /// <returns>节点位置集合。</returns>
    IReadOnlyList<NodePositionSnapshot> GetNodePositions();

    /// <summary>
    /// 获取指定源端口的兼容连接目标。
    /// </summary>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    /// <returns>兼容目标集合。</returns>
    IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId);
}
