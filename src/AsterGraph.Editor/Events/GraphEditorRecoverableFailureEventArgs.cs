namespace AsterGraph.Editor.Events;

/// <summary>
/// 图编辑器可恢复失败事件参数。
/// </summary>
public sealed class GraphEditorRecoverableFailureEventArgs : EventArgs
{
    /// <summary>
    /// 初始化可恢复失败事件参数。
    /// </summary>
    /// <param name="code">稳定失败代码。</param>
    /// <param name="operation">失败操作标识。</param>
    /// <param name="message">宿主可读的失败消息。</param>
    /// <param name="exception">可选异常对象。</param>
    public GraphEditorRecoverableFailureEventArgs(
        string code,
        string operation,
        string message,
        Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Operation = operation;
        Message = message;
        Exception = exception;
    }

    /// <summary>
    /// 稳定失败代码。
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 失败操作标识。
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// 宿主可读的失败消息。
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 可选异常对象。
    /// </summary>
    public Exception? Exception { get; }
}
