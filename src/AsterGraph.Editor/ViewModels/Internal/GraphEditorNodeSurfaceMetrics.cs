using AsterGraph.Core.Models;

namespace AsterGraph.Editor.ViewModels;

internal static class GraphEditorNodeSurfaceMetrics
{
    private const double MinimumBodyHeight = 158d;
    private const double BaseChromeHeight = 132d;
    private const double DescriptionHeight = 40d;
    private const double PortRowHeight = 24d;
    private const double PortRowSpacing = 8d;
    private const double StatusBarHeight = 28d;
    private const double ExpandedSurfaceHeight = 152d;

    internal static double CalculateRequiredHeight(int inputCount, int outputCount)
    {
        var visiblePortRows = Math.Max(Math.Max(inputCount, outputCount), 1);
        var portsHeight = (visiblePortRows * PortRowHeight)
            + ((visiblePortRows - 1) * PortRowSpacing);

        return Math.Max(
            MinimumBodyHeight,
            BaseChromeHeight + DescriptionHeight + portsHeight);
    }

    internal static double CalculateBaseHeight(double persistedHeight, int inputCount, int outputCount)
        => Math.Max(persistedHeight, CalculateRequiredHeight(inputCount, outputCount));

    internal static double CalculateRenderedHeight(double baseHeight, GraphNodeExpansionState expansionState, bool hasStatusBar)
        => baseHeight
           + (hasStatusBar ? StatusBarHeight : 0d)
           + (expansionState == GraphNodeExpansionState.Expanded ? ExpandedSurfaceHeight : 0d);

    internal static double CalculateRenderedHeight(GraphNode node, bool hasStatusBar = false)
    {
        ArgumentNullException.ThrowIfNull(node);

        var baseHeight = CalculateBaseHeight(node.Size.Height, node.Inputs.Count, node.Outputs.Count);
        var expansionState = node.Surface?.ExpansionState ?? GraphNodeExpansionState.Collapsed;
        return CalculateRenderedHeight(baseHeight, expansionState, hasStatusBar);
    }
}
