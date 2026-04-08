namespace AsterGraph.Editor.Events;

/// <summary>
/// 图编辑器自动化开始事件参数。
/// </summary>
public sealed class GraphEditorAutomationStartedEventArgs : EventArgs
{
    /// <summary>
    /// 初始化自动化开始事件参数。
    /// </summary>
    public GraphEditorAutomationStartedEventArgs(
        string runId,
        int totalStepCount,
        bool usedMutationScope,
        string? mutationLabel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentOutOfRangeException.ThrowIfNegative(totalStepCount);

        RunId = runId.Trim();
        TotalStepCount = totalStepCount;
        UsedMutationScope = usedMutationScope;
        MutationLabel = string.IsNullOrWhiteSpace(mutationLabel) ? null : mutationLabel.Trim();
    }

    public string RunId { get; }

    public int TotalStepCount { get; }

    public bool UsedMutationScope { get; }

    public string? MutationLabel { get; }
}
