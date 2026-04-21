using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable world-space anchor snapshot for one connection endpoint.
/// </summary>
/// <param name="NodeId">Owning node identifier.</param>
/// <param name="EndpointId">Stable port or parameter endpoint identifier.</param>
/// <param name="EndpointKind">Whether the endpoint resolves to a port or parameter target.</param>
/// <param name="Position">Resolved world-space anchor position.</param>
public sealed record GraphEditorConnectionEndpointGeometrySnapshot(
    string NodeId,
    string EndpointId,
    GraphConnectionTargetKind EndpointKind,
    GraphPoint Position);
