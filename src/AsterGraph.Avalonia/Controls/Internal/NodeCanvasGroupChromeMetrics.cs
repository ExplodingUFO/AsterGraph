using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class NodeCanvasGroupChromeMetrics
{
    internal const double HeaderHeight = 40d;
    internal const double HeaderHorizontalPadding = 12d;
    internal const double HeaderVerticalPadding = 8d;
    internal const double BottomInset = 12d;
    internal const double ResizeHandleThickness = 12d;
    internal const double MinimumWidth = 168d;
    internal const double MinimumExpandedHeight = HeaderHeight + 16d;

    internal static double ResolveRenderedWidth(GraphEditorNodeGroupSnapshot group)
        => Math.Max(MinimumWidth, group.Size.Width);

    internal static double ResolveRenderedHeight(GraphEditorNodeGroupSnapshot group)
        => group.IsCollapsed
            ? HeaderHeight
            : Math.Max(MinimumExpandedHeight, group.Size.Height);

    internal static NodeBounds ResolveBodyBounds(GraphEditorNodeGroupSnapshot group)
    {
        if (group.IsCollapsed)
        {
            return default;
        }

        var renderedWidth = ResolveRenderedWidth(group);
        var renderedHeight = ResolveRenderedHeight(group);
        var width = Math.Max(0d, renderedWidth - (HeaderHorizontalPadding * 2d));
        var height = Math.Max(0d, renderedHeight - HeaderHeight - BottomInset);

        return new NodeBounds(
            group.Position.X + HeaderHorizontalPadding,
            group.Position.Y + HeaderHeight,
            width,
            height);
    }
}
