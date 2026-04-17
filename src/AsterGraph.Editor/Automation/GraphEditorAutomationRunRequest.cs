namespace AsterGraph.Editor.Automation;

/// <summary>
/// Describes one automation run request.
/// </summary>
public sealed record GraphEditorAutomationRunRequest
{
    /// <summary>
    /// Initializes an automation run request.
    /// </summary>
    /// <param name="runId">A stable run identifier.</param>
    /// <param name="steps">The ordered step list to execute.</param>
    /// <param name="runInMutationScope">Whether to execute the run inside one mutation scope.</param>
    /// <param name="mutationLabel">An optional mutation label associated with the run.</param>
    /// <param name="stopOnFailure">Whether execution should stop as soon as one step fails.</param>
    public GraphEditorAutomationRunRequest(
        string runId,
        IReadOnlyList<GraphEditorAutomationStep> steps,
        bool runInMutationScope = true,
        string? mutationLabel = null,
        bool stopOnFailure = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentNullException.ThrowIfNull(steps);

        RunId = runId.Trim();
        Steps = steps.ToList();
        RunInMutationScope = runInMutationScope;
        MutationLabel = string.IsNullOrWhiteSpace(mutationLabel) ? RunId : mutationLabel.Trim();
        StopOnFailure = stopOnFailure;
    }

    /// <summary>
    /// Gets the stable run identifier.
    /// </summary>
    public string RunId { get; }

    /// <summary>
    /// Gets the ordered automation steps.
    /// </summary>
    public IReadOnlyList<GraphEditorAutomationStep> Steps { get; }

    /// <summary>
    /// Gets whether the run executes inside one mutation scope.
    /// </summary>
    public bool RunInMutationScope { get; }

    /// <summary>
    /// Gets the stable label associated with the mutation scope.
    /// </summary>
    public string? MutationLabel { get; }

    /// <summary>
    /// Gets whether execution stops immediately after a failure.
    /// </summary>
    public bool StopOnFailure { get; }
}
