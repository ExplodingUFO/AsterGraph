namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Represents the nodes and connections that intersect a canvas selection rectangle.
/// </summary>
/// <param name="NodeIds">Node identifiers whose bounds intersect the rectangle.</param>
/// <param name="ConnectionIds">Connection identifiers whose source and target nodes are both inside the rectangle.</param>
public sealed record GraphEditorSelectionRectangleSnapshot(
    IReadOnlyList<string> NodeIds,
    IReadOnlyList<string> ConnectionIds);
