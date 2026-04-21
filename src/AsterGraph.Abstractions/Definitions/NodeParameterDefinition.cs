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
        string? placeholderText = null,
        string? templateKey = null,
        string? helpText = null,
        int sortOrder = 0,
        bool isAdvanced = false,
        string? unitSuffix = null)
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
        TemplateKey = string.IsNullOrWhiteSpace(templateKey) ? null : templateKey.Trim();
        HelpText = string.IsNullOrWhiteSpace(helpText) ? null : helpText.Trim();
        SortOrder = sortOrder;
        IsAdvanced = isAdvanced;
        UnitSuffix = string.IsNullOrWhiteSpace(unitSuffix) ? null : unitSuffix.Trim();
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

    /// <summary>
    /// Optional host-facing editor template key used by stock and custom UI registries.
    /// </summary>
    public string? TemplateKey { get; }

    /// <summary>
    /// Optional high-signal authoring guidance shown by shipped inspector surfaces.
    /// </summary>
    public string? HelpText { get; }

    /// <summary>
    /// Stable authoring sort order shared by runtime snapshots and shipped inspector surfaces.
    /// </summary>
    public int SortOrder { get; }

    /// <summary>
    /// Indicates the parameter belongs to the advanced authoring section.
    /// </summary>
    public bool IsAdvanced { get; }

    /// <summary>
    /// Optional display suffix rendered next to numeric or text-oriented values.
    /// </summary>
    public string? UnitSuffix { get; }
}
