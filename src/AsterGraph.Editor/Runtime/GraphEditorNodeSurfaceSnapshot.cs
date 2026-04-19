using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing snapshot of one node's progressive surface state.
/// </summary>
/// <param name="NodeId">Stable node identifier.</param>
/// <param name="Size">Persisted node size.</param>
/// <param name="ActiveTier">Resolved size-driven active surface tier.</param>
/// <param name="ExpansionState">Legacy persisted node card expansion state kept as a compatibility shim.</param>
/// <param name="GroupId">Optional attached editor-only group identifier.</param>
public sealed record GraphEditorNodeSurfaceSnapshot(
    string NodeId,
    GraphSize Size,
    GraphEditorNodeSurfaceTierSnapshot ActiveTier,
    GraphNodeExpansionState ExpansionState,
    string? GroupId);
