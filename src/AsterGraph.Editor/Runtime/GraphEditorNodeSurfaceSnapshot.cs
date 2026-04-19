using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing snapshot of one node's progressive surface state.
/// </summary>
/// <param name="NodeId">Stable node identifier.</param>
/// <param name="Size">Persisted node size.</param>
/// <param name="ExpansionState">Persisted node card expansion state.</param>
/// <param name="GroupId">Optional attached editor-only group identifier.</param>
public sealed record GraphEditorNodeSurfaceSnapshot(
    string NodeId,
    GraphSize Size,
    GraphNodeExpansionState ExpansionState,
    string? GroupId);
