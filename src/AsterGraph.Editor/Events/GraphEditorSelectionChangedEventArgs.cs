namespace AsterGraph.Editor.Events;

/// <summary>
/// 编辑器选择变化事件参数。
/// </summary>
public sealed class GraphEditorSelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// 初始化选择变化事件参数。
    /// </summary>
    public GraphEditorSelectionChangedEventArgs(
        IReadOnlyList<string> selectedNodeIds,
        string? primarySelectedNodeId)
    {
        SelectedNodeIds = selectedNodeIds;
        PrimarySelectedNodeId = primarySelectedNodeId;
    }

    /// <summary>
    /// 当前选中的节点实例标识集合。
    /// </summary>
    public IReadOnlyList<string> SelectedNodeIds { get; }

    /// <summary>
    /// 当前主选中节点实例标识。
    /// </summary>
    public string? PrimarySelectedNodeId { get; }
}
