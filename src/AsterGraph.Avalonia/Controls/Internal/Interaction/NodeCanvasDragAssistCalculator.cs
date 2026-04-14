using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasDragAssistResult(
    GraphPoint AdjustedDelta,
    double? GuideWorldX,
    double? GuideWorldY);

internal static class NodeCanvasDragAssistCalculator
{
    public static NodeCanvasDragAssistResult Calculate(
        NodeBounds movingBounds,
        double deltaX,
        double deltaY,
        IEnumerable<NodeBounds> candidateBounds,
        bool enableGridSnapping,
        bool enableAlignmentGuides,
        double primaryGridSpacing,
        double tolerance)
    {
        ArgumentNullException.ThrowIfNull(candidateBounds);
        tolerance = Math.Max(0d, tolerance);

        if (!enableGridSnapping && !enableAlignmentGuides)
        {
            return new NodeCanvasDragAssistResult(new GraphPoint(deltaX, deltaY), null, null);
        }

        var proposedBounds = new NodeBounds(
            movingBounds.X + deltaX,
            movingBounds.Y + deltaY,
            movingBounds.Width,
            movingBounds.Height);

        var snapDeltaX = 0d;
        var snapDeltaY = 0d;
        double? guideWorldX = null;
        double? guideWorldY = null;
        var bestXDelta = double.PositiveInfinity;
        var bestYDelta = double.PositiveInfinity;

        if (enableGridSnapping && primaryGridSpacing > 0d)
        {
            var snappedX = Math.Round(proposedBounds.X / primaryGridSpacing) * primaryGridSpacing;
            var snappedY = Math.Round(proposedBounds.Y / primaryGridSpacing) * primaryGridSpacing;
            var gridDeltaX = snappedX - proposedBounds.X;
            var gridDeltaY = snappedY - proposedBounds.Y;

            if (Math.Abs(gridDeltaX) <= tolerance)
            {
                snapDeltaX = gridDeltaX;
                bestXDelta = Math.Abs(gridDeltaX);
            }

            if (Math.Abs(gridDeltaY) <= tolerance)
            {
                snapDeltaY = gridDeltaY;
                bestYDelta = Math.Abs(gridDeltaY);
            }
        }

        if (enableAlignmentGuides)
        {
            foreach (var candidate in candidateBounds)
            {
                EvaluateGuideAxis(
                    proposedBounds.X,
                    proposedBounds.X + (proposedBounds.Width / 2),
                    proposedBounds.X + proposedBounds.Width,
                    candidate.X,
                    candidate.X + (candidate.Width / 2),
                    candidate.X + candidate.Width,
                    tolerance,
                    ref snapDeltaX,
                    ref bestXDelta,
                    ref guideWorldX);

                EvaluateGuideAxis(
                    proposedBounds.Y,
                    proposedBounds.Y + (proposedBounds.Height / 2),
                    proposedBounds.Y + proposedBounds.Height,
                    candidate.Y,
                    candidate.Y + (candidate.Height / 2),
                    candidate.Y + candidate.Height,
                    tolerance,
                    ref snapDeltaY,
                    ref bestYDelta,
                    ref guideWorldY);
            }
        }

        return new NodeCanvasDragAssistResult(
            new GraphPoint(deltaX + snapDeltaX, deltaY + snapDeltaY),
            guideWorldX,
            guideWorldY);
    }

    private static void EvaluateGuideAxis(
        double movingStart,
        double movingCenter,
        double movingEnd,
        double candidateStart,
        double candidateCenter,
        double candidateEnd,
        double tolerance,
        ref double snapDelta,
        ref double bestDeltaMagnitude,
        ref double? guideWorld)
    {
        EvaluatePair(movingStart, candidateStart, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
        EvaluatePair(movingCenter, candidateCenter, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
        EvaluatePair(movingEnd, candidateEnd, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
    }

    private static void EvaluatePair(
        double moving,
        double candidate,
        double tolerance,
        ref double snapDelta,
        ref double bestDeltaMagnitude,
        ref double? guideWorld)
    {
        var delta = candidate - moving;
        var magnitude = Math.Abs(delta);
        if (magnitude > tolerance || magnitude >= bestDeltaMagnitude)
        {
            return;
        }

        snapDelta = delta;
        bestDeltaMagnitude = magnitude;
        guideWorld = candidate;
    }
}
