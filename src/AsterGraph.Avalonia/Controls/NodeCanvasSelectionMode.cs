namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Defines the canvas-level pointer selection gesture used when the host starts a selection drag.
/// </summary>
public enum NodeCanvasSelectionMode
{
    /// <summary>
    /// Dragging on empty canvas space uses the stock rectangular marquee selection.
    /// </summary>
    Marquee,

    /// <summary>
    /// Dragging on empty canvas space records a freeform lasso path and finalizes through lasso selection.
    /// </summary>
    Lasso,
}
