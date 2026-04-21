using AsterGraph.Abstractions.Definitions;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Represents a host-facing snapshot of one editable node parameter across the current selection.
/// </summary>
/// <param name="Definition">Stable parameter metadata from the registered node definition.</param>
/// <param name="CurrentValue">Normalized value shared by the current selection, or <see langword="null"/> for mixed values.</param>
/// <param name="HasMixedValues">Whether the selected nodes currently expose different effective values for this parameter.</param>
/// <param name="CanEdit">Whether the current host permissions and parameter constraints allow mutation.</param>
/// <param name="IsValid">Whether the current effective value satisfies the definition metadata.</param>
/// <param name="ValidationMessage">Validation feedback when <paramref name="IsValid"/> is <see langword="false"/>.</param>
/// <param name="CanResetToDefault">Whether the host can restore the selection to the declared default value.</param>
/// <param name="IsUsingDefaultValue">Whether the effective value still matches the declared default.</param>
/// <param name="ReadOnlyReason">Optional read-only rationale surfaced to host shells.</param>
/// <param name="HelpText">Optional high-signal authoring guidance projected from the definition metadata.</param>
public sealed record GraphEditorNodeParameterSnapshot(
    NodeParameterDefinition Definition,
    object? CurrentValue,
    bool HasMixedValues,
    bool CanEdit,
    bool IsValid,
    string? ValidationMessage,
    bool CanResetToDefault,
    bool IsUsingDefaultValue,
    string? ReadOnlyReason,
    string? HelpText);
