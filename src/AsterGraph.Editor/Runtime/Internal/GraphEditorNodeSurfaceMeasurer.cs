using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorNodeSurfaceMeasurer
{
    private const double MinimumSummaryDisclosureWidth = 320d;
    private const double MinimumInlineEditorDisclosureWidth = 420d;
    private const double WidthPadding = 132d;
    private const double OutputColumnReserve = 40d;
    private const double SummaryWidthDelta = 64d;
    private const double InlineEditorWidthDelta = 96d;
    private const double CharacterWidth = 7.2d;

    internal static GraphEditorNodeSurfaceMeasurement Measure(GraphEditorNodeSurfaceContentPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var requiredInputCount = plan.InputPortCount + plan.RequiredParameters.Count;
        var expandedInputCount = requiredInputCount + plan.OptionalParameters.Count;
        var baselineHeight = GraphEditorNodeSurfaceMetrics.CalculateRequiredHeight(requiredInputCount, plan.OutputPortCount);
        var expandedHeight = GraphEditorNodeSurfaceMetrics.CalculateRequiredHeight(expandedInputCount, plan.OutputPortCount);
        var baselineWidth = Math.Max(
            GraphEditorNodeSurfaceMetrics.MinimumNodeWidth,
            Math.Max(
                plan.PreferredWidth,
                ResolveBaselineWidth(plan)));
        var summaryWidth = plan.SupportsParameterSummaries
            ? Math.Max(MinimumSummaryDisclosureWidth, baselineWidth + SummaryWidthDelta)
            : double.PositiveInfinity;
        var editorWidth = plan.SupportsInlineEditors
            ? Math.Max(MinimumInlineEditorDisclosureWidth, Math.Max(summaryWidth + InlineEditorWidthDelta, baselineWidth))
            : double.PositiveInfinity;

        return new GraphEditorNodeSurfaceMeasurement(
            new GraphSize(baselineWidth, Math.Max(plan.PreferredHeight, baselineHeight)),
            Math.Max(plan.PreferredHeight, expandedHeight),
            summaryWidth,
            editorWidth,
            plan.RequiredParameters.Count,
            plan.OptionalParameters.Count,
            plan.SupportsParameterSummaries,
            plan.SupportsInlineEditors);
    }

    private static double ResolveBaselineWidth(GraphEditorNodeSurfaceContentPlan plan)
    {
        var longestLabelLength = Math.Max(
            Math.Max(plan.Title.Length, plan.Subtitle.Length),
            Math.Max(
                ResolveLongestParameterLabelLength(plan.RequiredParameters),
                0));
        var baselineWidth = WidthPadding + (longestLabelLength * CharacterWidth);
        if (plan.OutputPortCount > 0)
        {
            baselineWidth += OutputColumnReserve;
        }

        return baselineWidth;
    }

    private static int ResolveLongestParameterLabelLength(IReadOnlyList<AsterGraph.Abstractions.Definitions.NodeParameterDefinition> parameters)
        => parameters.Count == 0
            ? 0
            : parameters.Max(parameter => parameter.DisplayName.Length);
}
