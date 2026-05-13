namespace AsterGraph.Core.Models;

/// <summary>
/// Internal renderer-neutral geometry for a future whiteboard primitive.
/// </summary>
internal sealed record GraphWhiteboardPrimitiveGeometry
{
    private IReadOnlyList<GraphPoint> _points = [];

    /// <summary>
    /// Creates primitive geometry.
    /// </summary>
    public GraphWhiteboardPrimitiveGeometry(
        GraphPoint Origin,
        GraphSize Size,
        IReadOnlyList<GraphPoint>? Points = null)
    {
        this.Origin = Origin;
        this.Size = Size;
        this.Points = Points ?? [];
    }

    /// <summary>
    /// Primitive origin in graph world space.
    /// </summary>
    public GraphPoint Origin { get; init; }

    /// <summary>
    /// Primitive bounds size in graph world space.
    /// </summary>
    public GraphSize Size { get; init; }

    /// <summary>
    /// Optional renderer-neutral points for freehand primitives.
    /// </summary>
    public IReadOnlyList<GraphPoint> Points
    {
        get => _points;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _points = value.ToList();
        }
    }
}
