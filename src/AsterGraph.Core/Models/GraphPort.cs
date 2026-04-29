using System.ComponentModel;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted port definition attached to a graph node snapshot.
/// </summary>
/// <param name="Id">Stable port identifier within the node.</param>
/// <param name="Label">Display label shown for the port.</param>
/// <param name="Direction">Whether the port is an input or output.</param>
/// <param name="DataType">Human-readable data type caption.</param>
/// <param name="AccentHex">Accent color used for the port dot and related visuals.</param>
/// <param name="TypeId">Optional stable type identifier used by compatibility services.</param>
/// <param name="GroupName">Optional port group name used for visual grouping.</param>
/// <param name="MinConnections">Minimum required connections for validation.</param>
/// <param name="MaxConnections">Maximum allowed connections for validation.</param>
public sealed record GraphPort(
    string Id,
    string Label,
    PortDirection Direction,
    string DataType,
    string AccentHex,
    PortTypeId? TypeId = null,
    string? GroupName = null,
    int MinConnections = 0,
    int MaxConnections = int.MaxValue)
{
    /// <summary>
    /// Stable handle identifier used by hosted presenters and connection geometry.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string HandleId => Id;

    /// <summary>
    /// Short authoring hint for connection search and hover affordances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string ConnectionHint
        => string.IsNullOrWhiteSpace(GroupName)
            ? $"{Label} ({TypeId?.Value ?? DataType})"
            : $"{Label} ({GroupName}, {TypeId?.Value ?? DataType})";
}
