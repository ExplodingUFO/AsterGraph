using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Immutable parameter metadata consumed by editor UIs and serializers.
/// </summary>
public sealed record NodeParameterDefinition
{
    public NodeParameterDefinition(
        string key,
        string displayName,
        PortTypeId valueType,
        ParameterEditorKind editorKind,
        bool isRequired = false,
        string? description = null,
        object? defaultValue = null,
        ParameterConstraints? constraints = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Key = key.Trim();
        DisplayName = displayName.Trim();
        ValueType = valueType;
        EditorKind = editorKind;
        IsRequired = isRequired;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DefaultValue = defaultValue;
        Constraints = constraints ?? new ParameterConstraints();
    }

    public string Key { get; }

    public string DisplayName { get; }

    public PortTypeId ValueType { get; }

    public ParameterEditorKind EditorKind { get; }

    public bool IsRequired { get; }

    public string? Description { get; }

    public object? DefaultValue { get; }

    public ParameterConstraints Constraints { get; }
}
