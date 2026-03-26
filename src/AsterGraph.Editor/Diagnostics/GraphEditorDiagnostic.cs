namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 诊断严重级别。
/// </summary>
public enum GraphEditorDiagnosticSeverity
{
    /// <summary>
    /// 信息。
    /// </summary>
    Info,

    /// <summary>
    /// 警告。
    /// </summary>
    Warning,

    /// <summary>
    /// 错误。
    /// </summary>
    Error,
}

/// <summary>
/// 表示一个宿主可消费的图编辑器诊断项。
/// </summary>
public sealed record GraphEditorDiagnostic
{
    /// <summary>
    /// 初始化诊断项。
    /// </summary>
    /// <param name="code">稳定诊断代码。</param>
    /// <param name="operation">诊断对应的操作标识。</param>
    /// <param name="message">宿主可读的消息。</param>
    /// <param name="severity">严重级别。</param>
    /// <param name="exception">可选异常对象。</param>
    public GraphEditorDiagnostic(
        string code,
        string operation,
        string message,
        GraphEditorDiagnosticSeverity severity,
        Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Operation = operation;
        Message = message;
        Severity = severity;
        Exception = exception;
    }

    /// <summary>
    /// 稳定诊断代码。
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 诊断对应的操作标识。
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// 宿主可读的消息。
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 严重级别。
    /// </summary>
    public GraphEditorDiagnosticSeverity Severity { get; }

    /// <summary>
    /// 可选异常对象。
    /// </summary>
    public Exception? Exception { get; }
}
