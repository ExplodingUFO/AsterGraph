namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Document-level graph validation and readiness summary.
/// </summary>
public sealed record GraphEditorValidationSnapshot
{
    /// <summary>
    /// Initializes a validation snapshot.
    /// </summary>
    public GraphEditorValidationSnapshot(
        bool isReady,
        int errorCount,
        int warningCount,
        IReadOnlyList<GraphEditorValidationIssueSnapshot> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        IsReady = isReady;
        ErrorCount = errorCount;
        WarningCount = warningCount;
        Issues = issues.ToList();
    }

    /// <summary>
    /// Whether the graph has no blocking validation errors.
    /// </summary>
    public bool IsReady { get; }

    /// <summary>
    /// Blocking validation issue count.
    /// </summary>
    public int ErrorCount { get; }

    /// <summary>
    /// Non-blocking validation issue count.
    /// </summary>
    public int WarningCount { get; }

    /// <summary>
    /// Validation feedback rows.
    /// </summary>
    public IReadOnlyList<GraphEditorValidationIssueSnapshot> Issues { get; }

    /// <summary>
    /// Empty ready validation snapshot.
    /// </summary>
    public static GraphEditorValidationSnapshot Empty { get; } = new(true, 0, 0, []);
}
