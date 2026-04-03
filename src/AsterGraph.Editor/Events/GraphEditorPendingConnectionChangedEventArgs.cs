using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Events;

/// <summary>
/// 待完成连线状态变化事件参数。
/// </summary>
public sealed class GraphEditorPendingConnectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// 初始化待完成连线状态变化事件参数。
    /// </summary>
    /// <param name="pendingConnection">变化后的待完成连线快照。</param>
    public GraphEditorPendingConnectionChangedEventArgs(GraphEditorPendingConnectionSnapshot pendingConnection)
    {
        ArgumentNullException.ThrowIfNull(pendingConnection);
        PendingConnection = pendingConnection;
    }

    /// <summary>
    /// 变化后的待完成连线快照。
    /// </summary>
    public GraphEditorPendingConnectionSnapshot PendingConnection { get; }
}
