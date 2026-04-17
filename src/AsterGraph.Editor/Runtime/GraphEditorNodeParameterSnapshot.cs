using AsterGraph.Abstractions.Definitions;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Represents a host-facing snapshot of one editable node parameter across the current selection.
/// </summary>
/// <param name="Definition">Stable parameter metadata from the registered node definition.</param>
/// <param name="CurrentValue">Normalized value shared by the current selection, or <see langword="null"/> for mixed values.</param>
/// <param name="HasMixedValues">Whether the selected nodes currently expose different effective values for this parameter.</param>
/// <param name="CanEdit">Whether the current host permissions and parameter constraints allow mutation.</param>
public sealed record GraphEditorNodeParameterSnapshot(
    NodeParameterDefinition Definition,
    object? CurrentValue,
    bool HasMixedValues,
    bool CanEdit);
