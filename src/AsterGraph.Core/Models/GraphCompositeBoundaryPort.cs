using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable composite boundary port that exposes an inner child node port to the host graph.
/// </summary>
/// <param name="Id">Stable boundary port identifier within the composite node.</param>
/// <param name="Label">Display label shown on the composite node shell.</param>
/// <param name="Direction">Whether the boundary port is an input or output.</param>
/// <param name="DataType">Human-readable data type caption.</param>
/// <param name="AccentHex">Accent color used for the boundary port visuals.</param>
/// <param name="ChildNodeId">Child graph node identifier that owns the exposed inner port.</param>
/// <param name="ChildPortId">Child graph port identifier exposed through the boundary port.</param>
/// <param name="TypeId">Optional stable type identifier used by compatibility services.</param>
/// <param name="InlineParameterKey">Optional inline parameter key surfaced when no upstream connection is present.</param>
public sealed record GraphCompositeBoundaryPort(
    string Id,
    string Label,
    PortDirection Direction,
    string DataType,
    string AccentHex,
    string ChildNodeId,
    string ChildPortId,
    PortTypeId? TypeId = null,
    string? InlineParameterKey = null);
