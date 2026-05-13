namespace AsterGraph.Core.Models;

/// <summary>
/// Internal hit-test result for renderer-neutral whiteboard primitive scene data.
/// </summary>
/// <param name="PrimitiveId">Hit primitive identifier.</param>
/// <param name="Kind">Hit primitive kind.</param>
/// <param name="ZIndex">Hit primitive stacking order.</param>
/// <param name="EditLifecycle">Hit primitive edit lifecycle metadata.</param>
internal sealed record GraphWhiteboardPrimitiveHitTestResult(
    string PrimitiveId,
    GraphWhiteboardPrimitiveKind Kind,
    int ZIndex,
    GraphWhiteboardPrimitiveEditLifecycle EditLifecycle);
