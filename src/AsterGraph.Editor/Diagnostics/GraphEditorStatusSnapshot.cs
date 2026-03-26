namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 表示当前兼容状态文本的不可变快照。
/// </summary>
public sealed record GraphEditorStatusSnapshot
{
    /// <summary>
    /// 初始化状态快照。
    /// </summary>
    /// <param name="message">当前状态文本。</param>
    public GraphEditorStatusSnapshot(string? message)
    {
        Message = message ?? string.Empty;
    }

    /// <summary>
    /// 当前状态文本。
    /// </summary>
    public string Message { get; }
}
