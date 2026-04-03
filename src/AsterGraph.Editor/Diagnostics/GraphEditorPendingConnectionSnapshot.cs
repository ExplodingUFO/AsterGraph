namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 表示当前待完成连线预览状态的不可变快照。
/// </summary>
public sealed record GraphEditorPendingConnectionSnapshot
{
    /// <summary>
    /// 基于原始待完成连线状态创建一个经过归一化的快照。
    /// </summary>
    public static GraphEditorPendingConnectionSnapshot Create(
        bool hasPendingConnection,
        string? sourceNodeId,
        string? sourcePortId)
        => new(
            hasPendingConnection,
            hasPendingConnection ? sourceNodeId : null,
            hasPendingConnection ? sourcePortId : null);

    /// <summary>
    /// 初始化待完成连线快照。
    /// </summary>
    /// <param name="hasPendingConnection">是否存在待完成连线。</param>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    public GraphEditorPendingConnectionSnapshot(
        bool hasPendingConnection,
        string? sourceNodeId,
        string? sourcePortId)
    {
        HasPendingConnection = hasPendingConnection;
        SourceNodeId = sourceNodeId;
        SourcePortId = sourcePortId;
    }

    /// <summary>
    /// 当前是否存在待完成连线。
    /// </summary>
    public bool HasPendingConnection { get; }

    /// <summary>
    /// 待完成连线的源节点实例标识。
    /// </summary>
    public string? SourceNodeId { get; }

    /// <summary>
    /// 待完成连线的源端口实例标识。
    /// </summary>
    public string? SourcePortId { get; }
}
