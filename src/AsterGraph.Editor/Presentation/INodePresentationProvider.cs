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
    /// <param name="context">稳定节点展示上下文。</param>
    /// <returns>节点展示状态。</returns>
#pragma warning disable CS0618
    NodePresentationState GetNodePresentation(NodePresentationContext context)
        => GetNodePresentation(context.CompatibilityNode);
#pragma warning restore CS0618

    /// <summary>
    /// 获取指定节点的展示状态快照。
    /// </summary>
    /// <param name="node">目标节点。</param>
    /// <returns>节点展示状态。</returns>
    [Obsolete("Implement GetNodePresentation(NodePresentationContext) instead.")]
    NodePresentationState GetNodePresentation(NodeViewModel node);
}
