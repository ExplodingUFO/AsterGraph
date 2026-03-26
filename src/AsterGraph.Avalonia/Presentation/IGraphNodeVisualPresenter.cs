namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 定义节点画布内部节点可视树的展示层替换契约。
/// </summary>
public interface IGraphNodeVisualPresenter
{
    /// <summary>
    /// 为指定节点创建一个新的可视树与端口锚点映射。
    /// </summary>
    GraphNodeVisual Create(GraphNodeVisualContext context);

    /// <summary>
    /// 将最新节点状态应用到现有可视树。
    /// </summary>
    void Update(GraphNodeVisual visual, GraphNodeVisualContext context);
}
