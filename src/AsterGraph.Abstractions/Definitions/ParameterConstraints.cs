namespace AsterGraph.Abstractions.Definitions;

public sealed record ParameterConstraints(
    bool IsReadOnly = false,
    double? Minimum = null,
    double? Maximum = null,
    IReadOnlyList<ParameterOptionDefinition>? AllowedOptions = null)
{
    public IReadOnlyList<ParameterOptionDefinition> AllowedOptions { get; init; } = AllowedOptions ?? [];
}
