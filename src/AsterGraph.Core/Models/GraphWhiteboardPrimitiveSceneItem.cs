namespace AsterGraph.Core.Models;

/// <summary>
/// Internal renderer-neutral scene item for a whiteboard primitive.
/// </summary>
internal sealed record GraphWhiteboardPrimitiveSceneItem
{
    private string _id = string.Empty;
    private IReadOnlyList<GraphPoint> _points = [];

    /// <summary>
    /// Creates a renderer-neutral whiteboard primitive scene item.
    /// </summary>
    public GraphWhiteboardPrimitiveSceneItem(
        string Id,
        GraphWhiteboardPrimitiveKind Kind,
        GraphPoint BoundsOrigin,
        GraphSize BoundsSize,
        IReadOnlyList<GraphPoint>? Points,
        GraphWhiteboardPrimitiveStyle Style,
        int ZIndex,
        GraphWhiteboardPrimitiveEditLifecycle EditLifecycle)
    {
        this.Id = Id;
        this.Kind = Kind;
        this.BoundsOrigin = BoundsOrigin;
        this.BoundsSize = BoundsSize;
        this.Points = Points ?? [];
        this.Style = Style ?? throw new ArgumentNullException(nameof(Style));
        this.ZIndex = ZIndex;
        this.EditLifecycle = EditLifecycle ?? throw new ArgumentNullException(nameof(EditLifecycle));
    }

    /// <summary>
    /// Stable primitive identifier.
    /// </summary>
    public string Id
    {
        get => _id;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _id = value;
        }
    }

    /// <summary>
    /// Primitive drawing shape.
    /// </summary>
    public GraphWhiteboardPrimitiveKind Kind { get; init; }

    /// <summary>
    /// Primitive bounds origin in graph world space.
    /// </summary>
    public GraphPoint BoundsOrigin { get; init; }

    /// <summary>
    /// Primitive bounds size in graph world space.
    /// </summary>
    public GraphSize BoundsSize { get; init; }

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

    /// <summary>
    /// Renderer-neutral primitive style.
    /// </summary>
    public GraphWhiteboardPrimitiveStyle Style { get; init; }

    /// <summary>
    /// Relative stacking order among whiteboard primitives.
    /// </summary>
    public int ZIndex { get; init; }

    /// <summary>
    /// Edit lifecycle metadata for future authoring tools.
    /// </summary>
    public GraphWhiteboardPrimitiveEditLifecycle EditLifecycle { get; init; }
}
