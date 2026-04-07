using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
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
    /// 获取当前选择快照。
    /// </summary>
    /// <returns>当前选择状态。</returns>
    GraphEditorSelectionSnapshot GetSelectionSnapshot();

    /// <summary>
    /// 获取当前视口快照。
    /// </summary>
    /// <returns>当前视口状态。</returns>
    GraphEditorViewportSnapshot GetViewportSnapshot();

    /// <summary>
    /// 获取当前能力快照。
    /// </summary>
    /// <returns>当前能力状态。</returns>
    GraphEditorCapabilitySnapshot GetCapabilitySnapshot();

    /// <summary>
    /// 获取当前运行时能力、服务和集成特性的显式描述集合。
    /// </summary>
    /// <returns>稳定的特性描述集合。</returns>
    IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors()
        => throw new NotSupportedException();

    /// <summary>
    /// 获取当前稳定命令描述集合。
    /// </summary>
    /// <returns>命令描述集合。</returns>
    IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        => throw new NotSupportedException();

    /// <summary>
    /// 基于命中上下文生成框架无关的菜单描述集合。
    /// </summary>
    /// <param name="context">当前菜单上下文。</param>
    /// <returns>菜单描述集合。</returns>
    IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildContextMenuDescriptors(ContextMenuContext context)
        => throw new NotSupportedException();

    /// <summary>
    /// 获取全部节点位置快照。
    /// </summary>
    /// <returns>节点位置集合。</returns>
    IReadOnlyList<NodePositionSnapshot> GetNodePositions();

    /// <summary>
    /// 获取当前待完成连线快照。
    /// </summary>
    /// <returns>当前待完成连线状态。</returns>
    GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// 获取指定源端口的运行时兼容连接目标。
    /// </summary>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    /// <returns>兼容目标 DTO 集合。</returns>
    IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => throw new NotSupportedException();

    /// <summary>
    /// 获取指定源端口的兼容连接目标。
    /// </summary>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    /// <remarks>
    /// 此成员保留为兼容 shim，用于依赖 MVVM 运行时对象的旧宿主代码。
    /// 新的运行时查询应优先使用 <see cref="GetCompatiblePortTargets(string, string)"/>。
    /// </remarks>
    /// <returns>兼容目标集合。</returns>
    [Obsolete("Compatibility-only shim. Use GetCompatiblePortTargets(string, string) for canonical runtime queries.")]
    IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId);
}
