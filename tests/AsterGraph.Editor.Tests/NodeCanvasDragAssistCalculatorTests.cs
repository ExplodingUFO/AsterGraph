using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasDragAssistCalculatorTests
{
    [Fact]
    public void Calculate_GridSnapWithinTolerance_AdjustsDeltaFromDragOrigin()
    {
        var result = NodeCanvasDragAssistCalculator.Calculate(
            new NodeBounds(12, 12, 100, 80),
            deltaX: 8.2,
            deltaY: 8.2,
            candidateBounds: [],
            enableGridSnapping: true,
            enableAlignmentGuides: false,
            primaryGridSpacing: 10,
            tolerance: 1);

        Assert.Equal(8d, result.AdjustedDelta.X, 6);
        Assert.Equal(8d, result.AdjustedDelta.Y, 6);
        Assert.Null(result.GuideWorldX);
        Assert.Null(result.GuideWorldY);
    }

    [Fact]
    public void Calculate_AlignmentGuideWithinTolerance_AdjustsDeltaAndReportsGuideAxis()
    {
        var result = NodeCanvasDragAssistCalculator.Calculate(
            new NodeBounds(10, 10, 100, 80),
            deltaX: 9,
            deltaY: 0,
            candidateBounds:
            [
                new NodeBounds(20, 200, 100, 80),
            ],
            enableGridSnapping: false,
            enableAlignmentGuides: true,
            primaryGridSpacing: 10,
            tolerance: 2);

        Assert.Equal(10d, result.AdjustedDelta.X, 6);
        Assert.Equal(0d, result.AdjustedDelta.Y, 6);
        Assert.Equal(20, result.GuideWorldX);
        Assert.Null(result.GuideWorldY);
    }

    [Fact]
    public void Calculate_GridAndGuidesEnabled_PicksSmallerDeltaPerAxis()
    {
        var result = NodeCanvasDragAssistCalculator.Calculate(
            new NodeBounds(12, 12, 100, 80),
            deltaX: 8.6,
            deltaY: 8.6,
            candidateBounds:
            [
                new NodeBounds(20.1, 19.7, 100, 80),
            ],
            enableGridSnapping: true,
            enableAlignmentGuides: true,
            primaryGridSpacing: 10,
            tolerance: 1);

        Assert.Equal(8.1d, result.AdjustedDelta.X, 6);
        Assert.Equal(8d, result.AdjustedDelta.Y, 6);
        Assert.Equal(20.1, result.GuideWorldX);
        Assert.Null(result.GuideWorldY);
    }
}
