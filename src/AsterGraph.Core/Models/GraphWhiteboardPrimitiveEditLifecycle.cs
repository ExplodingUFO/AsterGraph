namespace AsterGraph.Core.Models;

/// <summary>
/// Internal edit lifecycle metadata for a future whiteboard primitive.
/// </summary>
/// <param name="State">Current edit lifecycle state.</param>
/// <param name="ActiveHandleKey">Optional active edit handle or gesture key.</param>
internal sealed record GraphWhiteboardPrimitiveEditLifecycle(
    GraphWhiteboardPrimitiveEditState State = GraphWhiteboardPrimitiveEditState.Committed,
    string? ActiveHandleKey = null)
{
    /// <summary>
    /// Default committed lifecycle state.
    /// </summary>
    public static GraphWhiteboardPrimitiveEditLifecycle Default { get; } = new();
}
