namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Suggests which editor control a host UI should use for a parameter.
/// </summary>
public enum ParameterEditorKind
{
    /// <summary>
    /// Free-form text editor.
    /// </summary>
    Text = 0,

    /// <summary>
    /// Numeric editor.
    /// </summary>
    Number = 1,

    /// <summary>
    /// Boolean toggle editor.
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// Fixed-option enum selector.
    /// </summary>
    Enum = 3,

    /// <summary>
    /// Color picker or color text editor.
    /// </summary>
    Color = 4,
}
