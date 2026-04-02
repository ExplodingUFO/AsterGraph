using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Overrides port styling for a specific port type.
/// </summary>
/// <param name="TypeId">Stable port type identifier to match.</param>
/// <param name="Style">Replacement port style tokens.</param>
public sealed record PortStyleOverride(
    PortTypeId TypeId,
    PortStyleOptions Style);
