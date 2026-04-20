using Avalonia.Input;

namespace AsterGraph.Avalonia.Presentation;

internal static class GraphResizeFeedbackDefaults
{
    private static readonly Cursor ArrowCursor = new(StandardCursorType.Arrow);
    private static readonly Cursor HorizontalResizeCursor = new(StandardCursorType.SizeWestEast);
    private static readonly Cursor VerticalResizeCursor = new(StandardCursorType.SizeNorthSouth);
    private static readonly Cursor BottomRightResizeCursor = new(StandardCursorType.BottomRightCorner);

    public static Cursor ResolveCursor(GraphResizeFeedbackContext context)
        => context.Handle switch
        {
            GraphResizeFeedbackHandle.LeftEdge or GraphResizeFeedbackHandle.RightEdge => HorizontalResizeCursor,
            GraphResizeFeedbackHandle.TopEdge or GraphResizeFeedbackHandle.BottomEdge => VerticalResizeCursor,
            GraphResizeFeedbackHandle.BottomRightCorner => BottomRightResizeCursor,
            _ => ArrowCursor,
        };
}
