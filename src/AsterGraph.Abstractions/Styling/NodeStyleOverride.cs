using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Overrides node styling for a specific node definition.
/// </summary>
/// <param name="DefinitionId">Stable node definition identifier to match.</param>
/// <param name="Style">Replacement node card style tokens.</param>
public sealed record NodeStyleOverride(
    NodeDefinitionId DefinitionId,
    NodeCardStyleOptions Style);
