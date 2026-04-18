using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Immutable parameter metadata consumed by editor UIs and serializers.
/// </summary>
public sealed record NodeParameterDefinition
{
    /// <summary>
    /// 初始化节点参数定义。
    /// </summary>
    public NodeParameterDefinition(
        string key,
        string displayName,
        PortTypeId valueType,
        ParameterEditorKind editorKind,
        bool isRequired = false,
        string? description = null,
        object? defaultValue = null,
        ParameterConstraints? constraints = null,
        string? groupName = null,
        string? placeholderText = null)
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
        GroupName = string.IsNullOrWhiteSpace(groupName) ? null : groupName.Trim();
        PlaceholderText = string.IsNullOrWhiteSpace(placeholderText) ? null : placeholderText.Trim();
    }

    /// <summary>
    /// 参数键。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 参数显示名称。
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 参数值类型。
    /// </summary>
    public PortTypeId ValueType { get; }

    /// <summary>
    /// 建议的参数编辑器类型。
    /// </summary>
    public ParameterEditorKind EditorKind { get; }

    /// <summary>
    /// 是否必填。
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// 参数描述。
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 默认值。
    /// </summary>
    public object? DefaultValue { get; }

    /// <summary>
    /// 参数约束。
    /// </summary>
    public ParameterConstraints Constraints { get; }

    /// <summary>
    /// Optional inspector group name for shipped or custom property editors.
    /// </summary>
    public string? GroupName { get; }

    /// <summary>
    /// Optional placeholder text or short display hint for text-oriented editors.
    /// </summary>
    public string? PlaceholderText { get; }
}
