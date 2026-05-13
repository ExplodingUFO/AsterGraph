namespace AsterGraph.Core.Models;

/// <summary>
/// Internal whiteboard primitive edit lifecycle state.
/// </summary>
internal enum GraphWhiteboardPrimitiveEditState
{
    /// <summary>
    /// Primitive is being created by a future authoring gesture.
    /// </summary>
    Creating,

    /// <summary>
    /// Primitive is being edited by a future authoring gesture.
    /// </summary>
    Editing,

    /// <summary>
    /// Primitive is committed and no gesture is active.
    /// </summary>
    Committed,
}
