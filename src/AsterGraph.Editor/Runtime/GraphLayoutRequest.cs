namespace AsterGraph.Editor.Runtime;

public sealed record GraphLayoutRequest
{
    public GraphLayoutRequestMode Mode { get; init; } = GraphLayoutRequestMode.All;

    public GraphLayoutOrientation Orientation { get; init; } = GraphLayoutOrientation.LeftToRight;

    public double HorizontalSpacing { get; init; } = 240;

    public double VerticalSpacing { get; init; } = 140;

    public IReadOnlyList<string> SelectedNodeIds { get; init; } = [];

    public string? ScopeId { get; init; }

    public IReadOnlyList<string> PinnedNodeIds { get; init; } = [];
}
