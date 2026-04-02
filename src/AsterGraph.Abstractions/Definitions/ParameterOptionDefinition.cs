namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// One selectable option presented by an enum-style parameter editor.
/// </summary>
/// <param name="Value">Stable persisted option value.</param>
/// <param name="Label">Host-visible display label.</param>
/// <param name="Description">Optional explanatory description for the option.</param>
public sealed record ParameterOptionDefinition(
    string Value,
    string Label,
    string? Description = null);
