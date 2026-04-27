namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes host-owned runtime state for a graph element.
/// </summary>
public enum GraphEditorRuntimeOverlayStatus
{
    Idle,
    Running,
    Success,
    Warning,
    Error,
}
