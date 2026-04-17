using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Automation;

/// <summary>
/// Captures the result of one automation run.
/// </summary>
public sealed record GraphEditorAutomationExecutionSnapshot
{
    /// <summary>
    /// Initializes an automation execution snapshot.
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

    /// <summary>
    /// Gets the stable run identifier.
    /// </summary>
    public string RunId { get; }

    /// <summary>
    /// Gets whether the overall run succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets whether the run executed inside one mutation scope.
    /// </summary>
    public bool UsedMutationScope { get; }

    /// <summary>
    /// Gets the mutation label associated with the run.
    /// </summary>
    public string? MutationLabel { get; }

    /// <summary>
    /// Gets the number of steps that executed before the run completed or failed.
    /// </summary>
    public int ExecutedStepCount { get; }

    /// <summary>
    /// Gets the total number of requested steps.
    /// </summary>
    public int TotalStepCount { get; }

    /// <summary>
    /// Gets the per-step execution results.
    /// </summary>
    public IReadOnlyList<GraphEditorAutomationStepExecutionSnapshot> Steps { get; }

    /// <summary>
    /// Gets the post-run inspection snapshot.
    /// </summary>
    public GraphEditorInspectionSnapshot Inspection { get; }

    /// <summary>
    /// Gets the machine-readable failure code when the run fails.
    /// </summary>
    public string? FailureCode { get; }

    /// <summary>
    /// Gets the host-readable failure message when the run fails.
    /// </summary>
    public string? FailureMessage { get; }
}

/// <summary>
/// Captures the result of one automation step inside a run.
/// </summary>
public sealed record GraphEditorAutomationStepExecutionSnapshot
{
    /// <summary>
    /// Initializes one step execution snapshot.
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

    /// <summary>
    /// Gets the zero-based step index inside the run.
    /// </summary>
    public int StepIndex { get; }

    /// <summary>
    /// Gets the stable step identifier.
    /// </summary>
    public string StepId { get; }

    /// <summary>
    /// Gets the stable command identifier executed by the step.
    /// </summary>
    public string CommandId { get; }

    /// <summary>
    /// Gets whether the step succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the machine-readable failure code when the step fails.
    /// </summary>
    public string? FailureCode { get; }

    /// <summary>
    /// Gets the host-readable failure message when the step fails.
    /// </summary>
    public string? FailureMessage { get; }
}
