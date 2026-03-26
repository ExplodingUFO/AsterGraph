namespace AsterGraph.Editor.Events;

/// <summary>
/// 图编辑器命令执行事件参数。
/// </summary>
public sealed class GraphEditorCommandExecutedEventArgs : EventArgs
{
    /// <summary>
    /// 初始化命令执行事件参数。
    /// </summary>
    /// <param name="commandId">稳定命令标识。</param>
    /// <param name="mutationLabel">可选的批量变更标签。</param>
    /// <param name="isInMutationScope">当前命令是否处于批量变更作用域中。</param>
    /// <param name="statusMessage">可选的状态文本。</param>
    public GraphEditorCommandExecutedEventArgs(
        string commandId,
        string? mutationLabel,
        bool isInMutationScope,
        string? statusMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        CommandId = commandId;
        MutationLabel = mutationLabel;
        IsInMutationScope = isInMutationScope;
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// 稳定命令标识。
    /// </summary>
    public string CommandId { get; }

    /// <summary>
    /// 可选的批量变更标签。
    /// </summary>
    public string? MutationLabel { get; }

    /// <summary>
    /// 当前命令是否处于批量变更作用域中。
    /// </summary>
    public bool IsInMutationScope { get; }

    /// <summary>
    /// 与当前命令相关的状态文本。
    /// </summary>
    public string? StatusMessage { get; }
}
