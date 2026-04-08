namespace AsterGraph.Editor.Automation;

/// <summary>
/// 表示一次自动化运行请求。
/// </summary>
public sealed record GraphEditorAutomationRunRequest
{
    /// <summary>
    /// 初始化自动化运行请求。
    /// </summary>
    /// <param name="runId">稳定运行标识。</param>
    /// <param name="steps">要顺序执行的步骤集合。</param>
    /// <param name="runInMutationScope">是否在一个批量变更作用域中执行。</param>
    /// <param name="mutationLabel">可选的批量变更标签。</param>
    /// <param name="stopOnFailure">遇到失败后是否立即停止。</param>
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
    /// 稳定运行标识。
    /// </summary>
    public string RunId { get; }

    /// <summary>
    /// 顺序执行的自动化步骤集合。
    /// </summary>
    public IReadOnlyList<GraphEditorAutomationStep> Steps { get; }

    /// <summary>
    /// 是否在一个批量变更作用域中执行。
    /// </summary>
    public bool RunInMutationScope { get; }

    /// <summary>
    /// 关联到批量变更的稳定标签。
    /// </summary>
    public string? MutationLabel { get; }

    /// <summary>
    /// 遇到失败时是否立即停止执行。
    /// </summary>
    public bool StopOnFailure { get; }
}
