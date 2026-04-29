using System.ComponentModel;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// Host-facing interaction focus snapshot used to distinguish inspected and actively edited surfaces.
/// </summary>
public sealed record GraphEditorInteractionFocusState(
    string? InspectedNodeId = null,
    string? EditingNodeId = null,
    string? EditingParameterKey = null)
{
    /// <summary>
    /// Empty focus snapshot.
    /// </summary>
    public static GraphEditorInteractionFocusState Empty { get; } = new();

    /// <summary>
    /// Whether any inspected node is currently projected.
    /// </summary>
    public bool HasInspection => !string.IsNullOrWhiteSpace(InspectedNodeId);

    /// <summary>
    /// Whether any actively edited parameter is currently projected.
    /// </summary>
    public bool HasEditing => !string.IsNullOrWhiteSpace(EditingNodeId) && !string.IsNullOrWhiteSpace(EditingParameterKey);

    public bool IsNodeInspected(string nodeId)
        => !string.IsNullOrWhiteSpace(nodeId)
           && string.Equals(InspectedNodeId, nodeId, StringComparison.Ordinal);

    public bool IsNodeEditing(string nodeId)
        => !string.IsNullOrWhiteSpace(nodeId)
           && string.Equals(EditingNodeId, nodeId, StringComparison.Ordinal);

    public GraphEditorConnectionFocusKind GetConnectionFocusKind(ConnectionViewModel connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (HasEditing
            && (string.Equals(connection.SourceNodeId, EditingNodeId, StringComparison.Ordinal)
                || string.Equals(connection.TargetNodeId, EditingNodeId, StringComparison.Ordinal)))
        {
            return GraphEditorConnectionFocusKind.Editing;
        }

        if (HasInspection
            && (string.Equals(connection.SourceNodeId, InspectedNodeId, StringComparison.Ordinal)
                || string.Equals(connection.TargetNodeId, InspectedNodeId, StringComparison.Ordinal)))
        {
            return GraphEditorConnectionFocusKind.Inspected;
        }

        return GraphEditorConnectionFocusKind.None;
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public enum GraphEditorConnectionFocusKind
{
    None = 0,
    Inspected = 1,
    Editing = 2,
}
