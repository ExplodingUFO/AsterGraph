namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Severity for one document validation issue.
/// </summary>
public enum GraphEditorValidationIssueSeverity
{
    /// <summary>
    /// The issue is visible feedback but does not block graph readiness.
    /// </summary>
    Warning = 0,

    /// <summary>
    /// The issue blocks graph readiness.
    /// </summary>
    Error = 1,
}
