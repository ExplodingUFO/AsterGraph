namespace AsterGraph.Core.Models;

/// <summary>
/// Internal renderer-neutral whiteboard primitive snapshot.
/// </summary>
internal sealed record GraphWhiteboardPrimitive
{
    private string _id = string.Empty;

    /// <summary>
    /// Creates a whiteboard primitive snapshot.
    /// </summary>
    public GraphWhiteboardPrimitive(
        string Id,
        GraphWhiteboardPrimitiveKind Kind,
        GraphWhiteboardPrimitiveGeometry Geometry,
        GraphWhiteboardPrimitiveStyle Style,
        int ZIndex,
        GraphWhiteboardPrimitiveEditLifecycle EditLifecycle)
    {
        this.Id = Id;
        this.Kind = Kind;
        this.Geometry = Geometry ?? throw new ArgumentNullException(nameof(Geometry));
        this.Style = Style ?? throw new ArgumentNullException(nameof(Style));
        this.ZIndex = ZIndex;
        this.EditLifecycle = EditLifecycle ?? throw new ArgumentNullException(nameof(EditLifecycle));
    }

    /// <summary>
    /// Stable primitive identifier scoped to the future whiteboard surface.
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
    /// Renderer-neutral primitive geometry in graph world space.
    /// </summary>
    public GraphWhiteboardPrimitiveGeometry Geometry { get; init; }

    /// <summary>
    /// Renderer-neutral primitive style metadata.
    /// </summary>
    public GraphWhiteboardPrimitiveStyle Style { get; init; }

    /// <summary>
    /// Relative stacking order among future whiteboard primitives.
    /// </summary>
    public int ZIndex { get; init; }

    /// <summary>
    /// Edit lifecycle metadata used by future authoring tools.
    /// </summary>
    public GraphWhiteboardPrimitiveEditLifecycle EditLifecycle { get; init; }
}
