namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Identifies the resolved resize edge or corner under the pointer.
/// </summary>
public enum GraphResizeFeedbackHandle
{
    LeftEdge = 0,
    TopEdge = 1,
    RightEdge = 2,
    BottomEdge = 3,
    BottomRightCorner = 4,
}
