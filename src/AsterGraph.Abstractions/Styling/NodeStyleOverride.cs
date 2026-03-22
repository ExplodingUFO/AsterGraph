using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Styling;

public sealed record NodeStyleOverride(
    NodeDefinitionId DefinitionId,
    NodeCardStyleOptions Style);
