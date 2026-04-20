using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted connection snapshot between two node ports.
/// </summary>
/// <param name="Id">Stable connection identifier within the document.</param>
/// <param name="SourceNodeId">Identifier of the source node.</param>
/// <param name="SourcePortId">Identifier of the source port.</param>
/// <param name="TargetNodeId">Identifier of the target node.</param>
/// <param name="TargetPortId">Stable identifier of the target endpoint. For compatibility this field name stays <c>TargetPortId</c>, but it may reference an input port or a parameter endpoint depending on <see cref="TargetKind"/>.</param>
/// <param name="Label">Host-visible connection label.</param>
/// <param name="AccentHex">Accent color used to render the connection.</param>
/// <param name="ConversionId">Optional implicit-conversion identifier applied to the connection.</param>
/// <param name="Presentation">Optional host-owned presentation metadata for the edge.</param>
public sealed record GraphConnection(
    string Id,
    string SourceNodeId,
    string SourcePortId,
    string TargetNodeId,
    string TargetPortId,
    string Label,
    string AccentHex,
    ConversionId? ConversionId = null,
    GraphEdgePresentation? Presentation = null)
{
    /// <summary>
    /// Identifies whether the connection targets an input port or a parameter endpoint.
    /// </summary>
    public GraphConnectionTargetKind TargetKind { get; init; } = GraphConnectionTargetKind.Port;

    /// <summary>
    /// Strongly typed target endpoint reference used by runtime mutation and projection code.
    /// </summary>
    public GraphConnectionTargetRef Target => new(TargetNodeId, TargetPortId, TargetKind);
}
