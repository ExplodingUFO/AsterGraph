using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

internal static class GraphEditorNodeSurfaceMetrics
{
    private const double MinimumBodyHeight = 158d;
    internal const double MinimumNodeWidth = 180d;
    private const double PortRowHeight = 24d;
    private const double PortRowSpacing = 8d;
    private const double StatusBarHeight = 28d;
    internal static double CalculateRequiredHeight(int inputCount, int outputCount)
    {
        var visiblePortRows = Math.Max(Math.Max(inputCount, outputCount), 1);
        var additionalPortRows = Math.Max(0, visiblePortRows - 1);
        return MinimumBodyHeight + (additionalPortRows * (PortRowHeight + PortRowSpacing));
    }

    internal static double CalculateBaseHeight(double persistedHeight, int inputCount, int outputCount)
        => Math.Max(persistedHeight, CalculateRequiredHeight(inputCount, outputCount));

    internal static GraphSize NormalizePersistedSize(GraphSize size, int inputCount, int outputCount)
        => new(
            Math.Max(size.Width, MinimumNodeWidth),
            Math.Max(size.Height, CalculateRequiredHeight(inputCount, outputCount)));

    internal static GraphSize NormalizePersistedSize(GraphSize size, GraphEditorNodeSurfaceMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);

        return new GraphSize(
            Math.Max(size.Width, measurement.BaselineSize.Width),
            Math.Max(size.Height, measurement.BaselineSize.Height));
    }

    internal static double CalculateRenderedHeight(
        GraphSize persistedSize,
        GraphEditorNodeSurfaceTierSnapshot activeTier,
        bool hasStatusBar)
    {
        var renderedHeight = persistedSize.Height;
        if (hasStatusBar)
        {
            renderedHeight += StatusBarHeight;
        }

        return renderedHeight;
    }

    internal static double CalculateRenderedHeight(GraphNode node, GraphEditorNodeSurfaceTierSnapshot activeTier, bool hasStatusBar = false)
        => CalculateRenderedHeight(node.Size, activeTier, hasStatusBar);
}
