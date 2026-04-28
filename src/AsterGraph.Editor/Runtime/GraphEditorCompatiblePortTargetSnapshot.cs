using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 表示运行时兼容端口目标的稳定快照。
/// </summary>
public sealed record GraphEditorCompatiblePortTargetSnapshot
{
    /// <summary>
    /// 初始化兼容端口目标快照。
    /// </summary>
    /// <param name="nodeId">目标节点标识。</param>
    /// <param name="nodeTitle">目标节点标题。</param>
    /// <param name="portId">目标端口标识。</param>
    /// <param name="portLabel">目标端口显示名称。</param>
    /// <param name="portTypeId">目标端口类型标识。</param>
    /// <param name="portAccentHex">目标端口强调色。</param>
    /// <param name="compatibility">兼容性结果。</param>
    /// <param name="portGroupName">目标端口分组名称。</param>
    /// <param name="minConnections">目标端口最小连接数。</param>
    /// <param name="maxConnections">目标端口最大连接数。</param>
    public GraphEditorCompatiblePortTargetSnapshot(
        string nodeId,
        string nodeTitle,
        string portId,
        string portLabel,
        PortTypeId portTypeId,
        string portAccentHex,
        PortCompatibilityResult compatibility,
        string? portGroupName,
        int minConnections,
        int maxConnections)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(portId);
        ArgumentNullException.ThrowIfNull(portTypeId);

        if (minConnections < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minConnections), "MinConnections must be non-negative.");
        }

        if (maxConnections < minConnections)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConnections), "MaxConnections must be greater than or equal to MinConnections.");
        }

        NodeId = nodeId;
        NodeTitle = string.IsNullOrWhiteSpace(nodeTitle) ? nodeId : nodeTitle;
        PortId = portId;
        PortLabel = string.IsNullOrWhiteSpace(portLabel) ? portId : portLabel;
        PortTypeId = portTypeId;
        PortAccentHex = portAccentHex ?? string.Empty;
        Compatibility = compatibility;
        PortGroupName = string.IsNullOrWhiteSpace(portGroupName) ? null : portGroupName.Trim();
        MinConnections = minConnections;
        MaxConnections = maxConnections;
    }

    /// <summary>
    /// 初始化兼容端口目标快照。
    /// </summary>
    public GraphEditorCompatiblePortTargetSnapshot(
        string nodeId,
        string nodeTitle,
        string portId,
        string portLabel,
        PortTypeId portTypeId,
        string portAccentHex,
        PortCompatibilityResult compatibility)
        : this(
            nodeId,
            nodeTitle,
            portId,
            portLabel,
            portTypeId,
            portAccentHex,
            compatibility,
            null,
            0,
            int.MaxValue)
    {
    }

    /// <summary>
    /// 目标节点标识。
    /// </summary>
    public string NodeId { get; }

    /// <summary>
    /// 目标节点标题。
    /// </summary>
    public string NodeTitle { get; }

    /// <summary>
    /// 目标端口标识。
    /// </summary>
    public string PortId { get; }

    /// <summary>
    /// 目标端口显示名称。
    /// </summary>
    public string PortLabel { get; }

    /// <summary>
    /// 目标端口类型标识。
    /// </summary>
    public PortTypeId PortTypeId { get; }

    /// <summary>
    /// 目标端口强调色。
    /// </summary>
    public string PortAccentHex { get; }

    /// <summary>
    /// 源端口到目标端口的兼容性结果。
    /// </summary>
    public PortCompatibilityResult Compatibility { get; }

    /// <summary>
    /// Stable handle identifier for the target port.
    /// </summary>
    public string PortHandleId => PortId;

    /// <summary>
    /// 目标端口分组名称。
    /// </summary>
    public string? PortGroupName { get; }

    /// <summary>
    /// 目标端口最小连接数。
    /// </summary>
    public int MinConnections { get; }

    /// <summary>
    /// 目标端口最大连接数。
    /// </summary>
    public int MaxConnections { get; }

    /// <summary>
    /// Short authoring hint for connection search and hover affordances.
    /// </summary>
    public string ConnectionHint
        => string.IsNullOrWhiteSpace(PortGroupName)
            ? $"{PortLabel} ({PortTypeId.Value})"
            : $"{PortLabel} ({PortGroupName}, {PortTypeId.Value})";
}
