using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Automation;

/// <summary>
/// 表示一次自动化运行的结果快照。
/// </summary>
public sealed record GraphEditorAutomationExecutionSnapshot
{
    /// <summary>
    /// 初始化自动化执行结果快照。
    /// </summary>
    public GraphEditorAutomationExecutionSnapshot(
        string runId,
        bool succeeded,
        bool usedMutationScope,
        string? mutationLabel,
        int executedStepCount,
        int totalStepCount,
        IReadOnlyList<GraphEditorAutomationStepExecutionSnapshot> steps,
        GraphEditorInspectionSnapshot inspection,
        string? failureCode = null,
        string? failureMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentOutOfRangeException.ThrowIfNegative(executedStepCount);
        ArgumentOutOfRangeException.ThrowIfNegative(totalStepCount);

        RunId = runId.Trim();
        Succeeded = succeeded;
        UsedMutationScope = usedMutationScope;
        MutationLabel = string.IsNullOrWhiteSpace(mutationLabel) ? null : mutationLabel.Trim();
        ExecutedStepCount = executedStepCount;
        TotalStepCount = totalStepCount;
        Steps = steps.ToList();
        Inspection = inspection;
        FailureCode = string.IsNullOrWhiteSpace(failureCode) ? null : failureCode.Trim();
        FailureMessage = string.IsNullOrWhiteSpace(failureMessage) ? null : failureMessage.Trim();
    }

    public string RunId { get; }

    public bool Succeeded { get; }

    public bool UsedMutationScope { get; }

    public string? MutationLabel { get; }

    public int ExecutedStepCount { get; }

    public int TotalStepCount { get; }

    public IReadOnlyList<GraphEditorAutomationStepExecutionSnapshot> Steps { get; }

    public GraphEditorInspectionSnapshot Inspection { get; }

    public string? FailureCode { get; }

    public string? FailureMessage { get; }
}

/// <summary>
/// 表示自动化运行中单个步骤的结果。
/// </summary>
public sealed record GraphEditorAutomationStepExecutionSnapshot
{
    /// <summary>
    /// 初始化步骤结果。
    /// </summary>
    public GraphEditorAutomationStepExecutionSnapshot(
        int stepIndex,
        string stepId,
        string commandId,
        bool succeeded,
        string? failureCode = null,
        string? failureMessage = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(stepIndex);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        StepIndex = stepIndex;
        StepId = stepId.Trim();
        CommandId = commandId.Trim();
        Succeeded = succeeded;
        FailureCode = string.IsNullOrWhiteSpace(failureCode) ? null : failureCode.Trim();
        FailureMessage = string.IsNullOrWhiteSpace(failureMessage) ? null : failureMessage.Trim();
    }

    public int StepIndex { get; }

    public string StepId { get; }

    public string CommandId { get; }

    public bool Succeeded { get; }

    public string? FailureCode { get; }

    public string? FailureMessage { get; }
}
