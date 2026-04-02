namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Optional constraints that shape how a node parameter can be edited or validated.
/// </summary>
/// <param name="IsReadOnly">Whether the parameter should be presented as read-only.</param>
/// <param name="Minimum">Optional numeric minimum bound.</param>
/// <param name="Maximum">Optional numeric maximum bound.</param>
/// <param name="AllowedOptions">Optional fixed option list for enum-style editors.</param>
public sealed record ParameterConstraints(
    bool IsReadOnly = false,
    double? Minimum = null,
    double? Maximum = null,
    IReadOnlyList<ParameterOptionDefinition>? AllowedOptions = null)
{
    /// <summary>
    /// Fixed option list available to the parameter editor.
    /// </summary>
    public IReadOnlyList<ParameterOptionDefinition> AllowedOptions { get; init; } = AllowedOptions ?? [];
}
