using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Presentation;

/// <summary>
/// 提供节点展示层状态的宿主扩展接口。
/// </summary>
public interface INodePresentationProvider
{
    /// <summary>
    /// 获取指定节点的展示状态快照。
    /// </summary>
    /// <param name="node">目标节点。</param>
    /// <returns>节点展示状态。</returns>
    NodePresentationState GetNodePresentation(NodeViewModel node);
}
