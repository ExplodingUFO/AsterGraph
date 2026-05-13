namespace AsterGraph.Core.Models;

/// <summary>
/// Internal renderer-neutral scene snapshot for whiteboard primitives.
/// </summary>
internal sealed record GraphWhiteboardPrimitiveSceneSnapshot
{
    private IReadOnlyList<GraphWhiteboardPrimitiveSceneItem> _primitives = [];

    /// <summary>
    /// Creates a whiteboard primitive scene snapshot.
    /// </summary>
    /// <param name="Primitives">Renderer-neutral primitive scene items.</param>
    public GraphWhiteboardPrimitiveSceneSnapshot(IReadOnlyList<GraphWhiteboardPrimitiveSceneItem>? Primitives = null)
    {
        this.Primitives = Primitives ?? [];
    }

    /// <summary>
    /// Renderer-neutral primitive scene items.
    /// </summary>
    public IReadOnlyList<GraphWhiteboardPrimitiveSceneItem> Primitives
    {
        get => _primitives;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _primitives = value.ToList();
        }
    }
}
