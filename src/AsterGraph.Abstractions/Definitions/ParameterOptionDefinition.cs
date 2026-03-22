namespace AsterGraph.Abstractions.Definitions;

public sealed record ParameterOptionDefinition(
    string Value,
    string Label,
    string? Description = null);
