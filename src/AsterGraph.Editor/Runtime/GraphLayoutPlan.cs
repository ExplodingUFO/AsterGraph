namespace AsterGraph.Editor.Runtime;

public sealed record GraphLayoutPlan(
    bool IsAvailable,
    GraphLayoutRequest Request,
    IReadOnlyList<GraphLayoutNodePosition> NodePositions,
    bool ResetManualRoutes = false,
    string? EmptyReason = null)
{
    public static GraphLayoutPlan Empty(GraphLayoutRequest request, string emptyReason)
        => new(false, request, [], ResetManualRoutes: false, emptyReason);
}
