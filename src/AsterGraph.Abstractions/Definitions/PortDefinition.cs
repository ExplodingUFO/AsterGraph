using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Immutable definition for a single input or output port.
/// </summary>
public sealed record PortDefinition
{
    /// <summary>
    /// 初始化端口定义。
    /// </summary>
    public PortDefinition(
        string key,
        string displayName,
        PortTypeId typeId,
        string accentHex = "#FFFFFF",
        string? description = null,
        string? inlineParameterKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Key = key.Trim();
        DisplayName = displayName.Trim();
        TypeId = typeId;
        AccentHex = accentHex;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        InlineParameterKey = string.IsNullOrWhiteSpace(inlineParameterKey) ? null : inlineParameterKey.Trim();
    }

    /// <summary>
    /// 端口键。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 端口显示名称。
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 端口类型标识。
    /// </summary>
    public PortTypeId TypeId { get; }

    /// <summary>
    /// 端口强调色。
    /// </summary>
    public string AccentHex { get; }

    /// <summary>
    /// 端口描述。
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Optional parameter key used by shipped and custom progressive surfaces for inline literal editing.
    /// </summary>
    public string? InlineParameterKey { get; }
}
