using Avalonia.Input;

namespace AsterGraph.Avalonia.Presentation;

internal static class GraphResizeFeedbackDefaults
{
    public static Cursor ResolveCursor(GraphResizeFeedbackContext context)
        => new(
            context.Handle switch
            {
                GraphResizeFeedbackHandle.LeftEdge or GraphResizeFeedbackHandle.RightEdge => StandardCursorType.SizeWestEast,
                GraphResizeFeedbackHandle.TopEdge or GraphResizeFeedbackHandle.BottomEdge => StandardCursorType.SizeNorthSouth,
                GraphResizeFeedbackHandle.BottomRightCorner => StandardCursorType.BottomRightCorner,
                _ => StandardCursorType.Arrow,
            });
}
