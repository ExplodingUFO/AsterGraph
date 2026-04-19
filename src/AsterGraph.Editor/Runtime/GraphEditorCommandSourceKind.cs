namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Identifies which editor layer owns a surfaced command descriptor.
/// </summary>
public enum GraphEditorCommandSourceKind
{
    Kernel = 0,
    Retained = 1,
    Host = 2,
    Plugin = 3,
}
