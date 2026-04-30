namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one stable stock placement for a runtime command id.
/// </summary>
public sealed record GraphEditorCommandPlacementSnapshot
{
    public GraphEditorCommandPlacementSnapshot(
        GraphEditorCommandSurfaceKind surfaceKind,
        string surfaceId,
        string placementId,
        int order = 0,
        string? contextKind = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(placementId);

        SurfaceKind = surfaceKind;
        SurfaceId = surfaceId.Trim();
        PlacementId = placementId.Trim();
        Order = order;
        ContextKind = string.IsNullOrWhiteSpace(contextKind) ? null : contextKind.Trim();
    }

    public GraphEditorCommandSurfaceKind SurfaceKind { get; }

    public string SurfaceId { get; }

    public string PlacementId { get; }

    public int Order { get; }

    public string? ContextKind { get; }
}
