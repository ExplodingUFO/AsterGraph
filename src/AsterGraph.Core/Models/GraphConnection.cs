using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted connection snapshot between two node ports.
/// </summary>
/// <param name="Id">Stable connection identifier within the document.</param>
/// <param name="SourceNodeId">Identifier of the source node.</param>
/// <param name="SourcePortId">Identifier of the source port.</param>
/// <param name="TargetNodeId">Identifier of the target node.</param>
/// <param name="TargetPortId">Identifier of the target port.</param>
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
    GraphEdgePresentation? Presentation = null);
