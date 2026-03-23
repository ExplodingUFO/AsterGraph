namespace AsterGraph.Editor.Events;

/// <summary>
/// 编辑器文档变化类型。
/// </summary>
public enum GraphEditorDocumentChangeKind
{
    /// <summary>
    /// 新增了节点。
    /// </summary>
    NodesAdded,
    /// <summary>
    /// 删除了节点。
    /// </summary>
    NodesRemoved,
    /// <summary>
    /// 连线发生了变化。
    /// </summary>
    ConnectionsChanged,
    /// <summary>
    /// 布局发生了变化。
    /// </summary>
    LayoutChanged,
    /// <summary>
    /// 参数发生了变化。
    /// </summary>
    ParametersChanged,
    /// <summary>
    /// 粘贴了片段。
    /// </summary>
    FragmentPasted,
    /// <summary>
    /// 从工作区加载了图。
    /// </summary>
    WorkspaceLoaded,
    /// <summary>
    /// 保存了工作区。
    /// </summary>
    WorkspaceSaved,
    /// <summary>
    /// 执行了撤销。
    /// </summary>
    Undo,
    /// <summary>
    /// 执行了重做。
    /// </summary>
    Redo,
}
