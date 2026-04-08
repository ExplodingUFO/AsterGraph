using AsterGraph.Editor.Automation;

namespace AsterGraph.Editor.Events;

/// <summary>
/// 图编辑器自动化进度事件参数。
/// </summary>
public sealed class GraphEditorAutomationProgressEventArgs : EventArgs
{
    /// <summary>
    /// 初始化自动化进度事件参数。
    /// </summary>
    public GraphEditorAutomationProgressEventArgs(
        string runId,
        int executedStepCount,
        int totalStepCount,
        bool usedMutationScope,
        string? mutationLabel,
        GraphEditorAutomationStepExecutionSnapshot step)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentOutOfRangeException.ThrowIfNegative(executedStepCount);
        ArgumentOutOfRangeException.ThrowIfNegative(totalStepCount);
        ArgumentNullException.ThrowIfNull(step);

        RunId = runId.Trim();
        ExecutedStepCount = executedStepCount;
        TotalStepCount = totalStepCount;
        UsedMutationScope = usedMutationScope;
        MutationLabel = string.IsNullOrWhiteSpace(mutationLabel) ? null : mutationLabel.Trim();
        Step = step;
    }

    public string RunId { get; }

    public int ExecutedStepCount { get; }

    public int TotalStepCount { get; }

    public bool UsedMutationScope { get; }

    public string? MutationLabel { get; }

    public GraphEditorAutomationStepExecutionSnapshot Step { get; }
}
