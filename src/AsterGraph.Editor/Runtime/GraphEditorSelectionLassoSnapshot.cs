namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Represents the nodes and connections selected by a freeform lasso polygon.
/// </summary>
/// <param name="NodeIds">Node identifiers whose center points are inside the lasso polygon.</param>
/// <param name="ConnectionIds">Connection identifiers whose source and target nodes are both inside the lasso polygon.</param>
public sealed record GraphEditorSelectionLassoSnapshot(
    IReadOnlyList<string> NodeIds,
    IReadOnlyList<string> ConnectionIds);
