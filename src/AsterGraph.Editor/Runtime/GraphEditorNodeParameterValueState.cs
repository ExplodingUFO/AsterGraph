namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing value-state classification for parameter authoring surfaces.
/// </summary>
public enum GraphEditorNodeParameterValueState
{
    Default = 0,
    Overridden = 1,
    Mixed = 2,
}
