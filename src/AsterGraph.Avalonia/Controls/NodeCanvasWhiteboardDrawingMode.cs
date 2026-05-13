namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Defines the host-selected whiteboard drawing tool activation state for the canvas.
/// </summary>
public enum NodeCanvasWhiteboardDrawingMode
{
    /// <summary>
    /// No whiteboard drawing tool is active.
    /// </summary>
    None,

    /// <summary>
    /// Rectangle whiteboard drawing is selected for the canvas pointer capture route.
    /// </summary>
    Rectangle,

    /// <summary>
    /// Freehand whiteboard drawing is selected for the canvas pointer capture route.
    /// </summary>
    Freehand,

    /// <summary>
    /// Whiteboard primitive erasing is selected for the canvas pointer capture route.
    /// </summary>
    Eraser,
}
