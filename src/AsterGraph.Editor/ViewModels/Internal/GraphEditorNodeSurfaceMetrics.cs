using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

internal static class GraphEditorNodeSurfaceMetrics
{
    private const double MinimumBodyHeight = 158d;
    internal const double MinimumNodeWidth = 180d;
    private const double BaseChromeHeight = 132d;
    private const double DescriptionHeight = 40d;
    private const double PortRowHeight = 24d;
    private const double PortRowSpacing = 8d;
    private const double StatusBarHeight = 28d;
    private const double InlineInputsHeight = 108d;
    private const double ParametersHeight = 144d;

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

    internal static GraphSize NormalizePersistedSize(GraphSize size, int inputCount, int outputCount)
        => new(
            Math.Max(size.Width, MinimumNodeWidth),
            Math.Max(size.Height, CalculateRequiredHeight(inputCount, outputCount)));

    internal static double CalculateRenderedHeight(
        GraphSize persistedSize,
        GraphEditorNodeSurfaceTierSnapshot activeTier,
        bool hasStatusBar)
    {
        ArgumentNullException.ThrowIfNull(activeTier);

        var renderedHeight = persistedSize.Height;
        if (hasStatusBar)
        {
            renderedHeight += StatusBarHeight;
        }

        if (activeTier.ShowsSection(NodeSurfaceSectionKeys.InlineInputs))
        {
            renderedHeight += InlineInputsHeight;
        }

        if (activeTier.ShowsSection(NodeSurfaceSectionKeys.Parameters))
        {
            renderedHeight += ParametersHeight;
        }

        return renderedHeight;
    }

    internal static double CalculateRenderedHeight(GraphNode node, GraphEditorNodeSurfaceTierSnapshot activeTier, bool hasStatusBar = false)
        => CalculateRenderedHeight(node.Size, activeTier, hasStatusBar);
}
