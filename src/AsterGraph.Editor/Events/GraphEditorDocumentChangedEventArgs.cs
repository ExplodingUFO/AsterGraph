namespace AsterGraph.Editor.Events;

/// <summary>
/// 图文档变化事件参数。
/// </summary>
public sealed class GraphEditorDocumentChangedEventArgs : EventArgs
{
    /// <summary>
    /// 初始化图文档变化事件参数。
    /// </summary>
    public GraphEditorDocumentChangedEventArgs(
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds = null,
        IReadOnlyList<string>? connectionIds = null,
        string? statusMessage = null)
    {
        ChangeKind = changeKind;
        NodeIds = nodeIds ?? [];
        ConnectionIds = connectionIds ?? [];
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// 变化类型。
    /// </summary>
    public GraphEditorDocumentChangeKind ChangeKind { get; }

    /// <summary>
    /// 相关节点实例标识集合。
    /// </summary>
    public IReadOnlyList<string> NodeIds { get; }

    /// <summary>
    /// 相关连线实例标识集合。
    /// </summary>
    public IReadOnlyList<string> ConnectionIds { get; }

    /// <summary>
    /// 当前变化对应的状态文本。
    /// </summary>
    public string? StatusMessage { get; }
}
