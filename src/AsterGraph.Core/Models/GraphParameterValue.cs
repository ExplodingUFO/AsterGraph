using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted parameter value bound to a node parameter key.
/// </summary>
/// <param name="Key">Stable parameter key.</param>
/// <param name="TypeId">Stable parameter value type identifier.</param>
/// <param name="Value">Serialized parameter value payload.</param>
public sealed record GraphParameterValue(
    string Key,
    PortTypeId TypeId,
    object? Value);
