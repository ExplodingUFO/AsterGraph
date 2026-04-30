using System.Text.Json.Serialization;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted editor-only node group metadata.
/// </summary>
/// <param name="Id">Stable group identifier within the document.</param>
/// <param name="Title">Group title shown on the canvas.</param>
/// <param name="Position">Top-left world position for the group boundary.</param>
/// <param name="Size">World-space group boundary size.</param>
/// <param name="NodeIds">Stable node identifiers attached to the group.</param>
/// <param name="IsCollapsed">Whether the group is currently collapsed in the editor surface.</param>
/// <param name="ExtraPadding">Persisted per-edge padding envelope around the member-node bounds.</param>
public sealed record GraphNodeGroup(
    string Id,
    string Title,
    GraphPoint Position,
    GraphSize Size,
    IReadOnlyList<string> NodeIds,
    bool IsCollapsed = false,
    GraphPadding ExtraPadding = default)
{
    /// <summary>
    /// Node groups are single-scope editor containers; membership is defined by <see cref="NodeIds"/>.
    /// </summary>
    [JsonIgnore]
    public bool IsContainer => true;

    /// <summary>
    /// Whether member nodes are directly projected in the active scope.
    /// </summary>
    [JsonIgnore]
    public bool ProjectsMemberNodes => !IsCollapsed;
}
