using System.ComponentModel;
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
        string? groupName = null,
        int minConnections = 0,
        int maxConnections = int.MaxValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        if (minConnections < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minConnections), "MinConnections must be non-negative.");
        }

        if (maxConnections < minConnections)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConnections), "MaxConnections must be greater than or equal to MinConnections.");
        }

        Key = key.Trim();
        DisplayName = displayName.Trim();
        TypeId = typeId;
        AccentHex = accentHex;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        GroupName = string.IsNullOrWhiteSpace(groupName) ? null : groupName.Trim();
        MinConnections = minConnections;
        MaxConnections = maxConnections;
    }

    /// <summary>
    /// 端口键。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Stable handle identifier used by hosted presenters and connection geometry.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string HandleId => Key;

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
    /// 端口分组名称。
    /// </summary>
    public string? GroupName { get; }

    /// <summary>
    /// 端口最小连接数。
    /// </summary>
    public int MinConnections { get; }

    /// <summary>
    /// 端口最大连接数。
    /// </summary>
    public int MaxConnections { get; }

    /// <summary>
    /// Short authoring hint for connection search and hover affordances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string ConnectionHint
        => string.IsNullOrWhiteSpace(GroupName)
            ? $"{DisplayName} ({TypeId.Value})"
            : $"{DisplayName} ({GroupName}, {TypeId.Value})";
}
