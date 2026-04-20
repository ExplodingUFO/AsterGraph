using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasDragAssistResult(
    GraphPoint AdjustedDelta,
    double? GuideWorldX,
    double? GuideWorldY);

internal enum NodeCanvasPlacementAxisMode
{
    None,
    Translate,
    StartEdge,
    EndEdge,
}

internal readonly record struct NodeCanvasPlacementAssistResult(
    NodeBounds AdjustedBounds,
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
        var result = CalculateBounds(
            movingBounds,
            proposedBounds,
            NodeCanvasPlacementAxisMode.Translate,
            NodeCanvasPlacementAxisMode.Translate,
            candidateBounds,
            enableGridSnapping,
            enableAlignmentGuides,
            primaryGridSpacing,
            tolerance);

        return new NodeCanvasDragAssistResult(
            new GraphPoint(
                result.AdjustedBounds.X - movingBounds.X,
                result.AdjustedBounds.Y - movingBounds.Y),
            result.GuideWorldX,
            result.GuideWorldY);
    }

    public static NodeCanvasPlacementAssistResult CalculateBounds(
        NodeBounds originBounds,
        NodeBounds proposedBounds,
        NodeCanvasPlacementAxisMode horizontalMode,
        NodeCanvasPlacementAxisMode verticalMode,
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
            return new NodeCanvasPlacementAssistResult(proposedBounds, null, null);
        }

        var candidates = enableAlignmentGuides ? candidateBounds.ToList() : [];
        var horizontal = CalculateAxis(
            originBounds.X,
            originBounds.Width,
            proposedBounds.X,
            proposedBounds.Width,
            horizontalMode,
            candidates,
            enableGridSnapping,
            enableAlignmentGuides,
            primaryGridSpacing,
            tolerance,
            static candidate => candidate.X,
            static candidate => candidate.X + (candidate.Width / 2d),
            static candidate => candidate.X + candidate.Width);
        var vertical = CalculateAxis(
            originBounds.Y,
            originBounds.Height,
            proposedBounds.Y,
            proposedBounds.Height,
            verticalMode,
            candidates,
            enableGridSnapping,
            enableAlignmentGuides,
            primaryGridSpacing,
            tolerance,
            static candidate => candidate.Y,
            static candidate => candidate.Y + (candidate.Height / 2d),
            static candidate => candidate.Y + candidate.Height);

        return new NodeCanvasPlacementAssistResult(
            new NodeBounds(horizontal.Start, vertical.Start, horizontal.Length, vertical.Length),
            horizontal.GuideWorld,
            vertical.GuideWorld);
    }

    private static NodeCanvasPlacementAxisResult CalculateAxis(
        double originStart,
        double originLength,
        double proposedStart,
        double proposedLength,
        NodeCanvasPlacementAxisMode mode,
        IReadOnlyList<NodeBounds> candidateBounds,
        bool enableGridSnapping,
        bool enableAlignmentGuides,
        double primaryGridSpacing,
        double tolerance,
        Func<NodeBounds, double> candidateStartSelector,
        Func<NodeBounds, double> candidateCenterSelector,
        Func<NodeBounds, double> candidateEndSelector)
    {
        if (mode is NodeCanvasPlacementAxisMode.None)
        {
            return new NodeCanvasPlacementAxisResult(proposedStart, proposedLength, null);
        }

        var snapDeltaX = 0d;
        var bestXDelta = double.PositiveInfinity;
        double? guideWorldX = null;
        var proposedEnd = proposedStart + proposedLength;
        var originEnd = originStart + originLength;
        var gridAnchor = mode is NodeCanvasPlacementAxisMode.EndEdge
            ? proposedEnd
            : proposedStart;

        if (enableGridSnapping && primaryGridSpacing > 0d)
        {
            var snappedX = Math.Round(gridAnchor / primaryGridSpacing) * primaryGridSpacing;
            var gridDeltaX = snappedX - gridAnchor;

            if (Math.Abs(gridDeltaX) <= tolerance)
            {
                snapDeltaX = gridDeltaX;
                bestXDelta = Math.Abs(gridDeltaX);
            }
        }

        if (enableAlignmentGuides)
        {
            foreach (var candidate in candidateBounds)
            {
                EvaluateGuideAxis(
                    proposedStart,
                    proposedStart + (proposedLength / 2d),
                    proposedEnd,
                    candidateStartSelector(candidate),
                    candidateCenterSelector(candidate),
                    candidateEndSelector(candidate),
                    mode,
                    tolerance,
                    ref snapDeltaX,
                    ref bestXDelta,
                    ref guideWorldX);
            }
        }

        return mode switch
        {
            NodeCanvasPlacementAxisMode.Translate => new NodeCanvasPlacementAxisResult(
                proposedStart + snapDeltaX,
                proposedLength,
                guideWorldX),
            NodeCanvasPlacementAxisMode.StartEdge => new NodeCanvasPlacementAxisResult(
                proposedStart + snapDeltaX,
                Math.Max(0d, originEnd - (proposedStart + snapDeltaX)),
                guideWorldX),
            NodeCanvasPlacementAxisMode.EndEdge => new NodeCanvasPlacementAxisResult(
                proposedStart,
                Math.Max(0d, (proposedEnd + snapDeltaX) - proposedStart),
                guideWorldX),
            _ => new NodeCanvasPlacementAxisResult(proposedStart, proposedLength, null),
        };
    }

    private static void EvaluateGuideAxis(
        double movingStart,
        double movingCenter,
        double movingEnd,
        double candidateStart,
        double candidateCenter,
        double candidateEnd,
        NodeCanvasPlacementAxisMode mode,
        double tolerance,
        ref double snapDelta,
        ref double bestDeltaMagnitude,
        ref double? guideWorld)
    {
        if (mode is NodeCanvasPlacementAxisMode.Translate)
        {
            EvaluatePair(movingStart, candidateStart, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
            EvaluatePair(movingCenter, candidateCenter, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
            EvaluatePair(movingEnd, candidateEnd, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
            return;
        }

        var movingAnchor = mode is NodeCanvasPlacementAxisMode.EndEdge ? movingEnd : movingStart;
        EvaluatePair(movingAnchor, candidateStart, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
        EvaluatePair(movingAnchor, candidateCenter, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
        EvaluatePair(movingAnchor, candidateEnd, tolerance, ref snapDelta, ref bestDeltaMagnitude, ref guideWorld);
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

internal readonly record struct NodeCanvasPlacementAxisResult(
    double Start,
    double Length,
    double? GuideWorld);
