using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Immutable contract for a node type that can be instantiated in a graph.
/// </summary>
public interface INodeDefinition
{
    /// <summary>
    /// 节点定义的稳定标识。
    /// </summary>
    NodeDefinitionId Id { get; }

    /// <summary>
    /// 节点显示名称。
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 节点分类。
    /// </summary>
    string Category { get; }

    /// <summary>
    /// 节点副标题。
    /// </summary>
    string Subtitle { get; }

    /// <summary>
    /// 节点描述。
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 节点强调色。
    /// </summary>
    string AccentHex { get; }

    /// <summary>
    /// 默认宽度。
    /// </summary>
    double DefaultWidth { get; }

    /// <summary>
    /// 默认高度。
    /// </summary>
    double DefaultHeight { get; }

    /// <summary>
    /// 输入端口定义集合。
    /// </summary>
    IReadOnlyList<PortDefinition> InputPorts { get; }

    /// <summary>
    /// 输出端口定义集合。
    /// </summary>
    IReadOnlyList<PortDefinition> OutputPorts { get; }

    /// <summary>
    /// 参数定义集合。
    /// </summary>
    IReadOnlyList<NodeParameterDefinition> Parameters { get; }
}

/// <summary>
/// 默认的不可变节点定义实现。
/// </summary>
public sealed record NodeDefinition : INodeDefinition
{
    /// <summary>
    /// 初始化节点定义。
    /// </summary>
    /// <param name="id">节点定义的稳定标识。</param>
    /// <param name="displayName">节点显示名称。</param>
    /// <param name="category">节点分类。</param>
    /// <param name="subtitle">节点副标题。</param>
    /// <param name="inputPorts">输入端口定义集合。</param>
    /// <param name="outputPorts">输出端口定义集合。</param>
    /// <param name="parameters">参数定义集合。</param>
    /// <param name="description">可选的节点描述。</param>
    /// <param name="accentHex">节点强调色。</param>
    /// <param name="defaultWidth">默认节点宽度。</param>
    /// <param name="defaultHeight">默认节点高度。</param>
    public NodeDefinition(
        NodeDefinitionId id,
        string displayName,
        string category,
        string subtitle,
        IReadOnlyList<PortDefinition> inputPorts,
        IReadOnlyList<PortDefinition> outputPorts,
        IReadOnlyList<NodeParameterDefinition>? parameters = null,
        string? description = null,
        string accentHex = "#FFFFFF",
        double defaultWidth = 220d,
        double defaultHeight = 140d)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(subtitle);
        ArgumentNullException.ThrowIfNull(inputPorts);
        ArgumentNullException.ThrowIfNull(outputPorts);

        EnsureUniqueKeys(inputPorts.Select(port => port.Key), "input port");
        EnsureUniqueKeys(outputPorts.Select(port => port.Key), "output port");
        EnsureUniqueKeys(inputPorts.Select(port => port.Key).Concat(outputPorts.Select(port => port.Key)), "port");

        if (defaultWidth <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultWidth), "Default width must be positive.");
        }

        if (defaultHeight <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultHeight), "Default height must be positive.");
        }

        Id = id;
        DisplayName = displayName.Trim();
        Category = category.Trim();
        Subtitle = subtitle.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        AccentHex = accentHex;
        DefaultWidth = defaultWidth;
        DefaultHeight = defaultHeight;
        InputPorts = [.. inputPorts];
        OutputPorts = [.. outputPorts];
        Parameters = parameters is null ? [] : [.. parameters];

        EnsureUniqueKeys(Parameters.Select(parameter => parameter.Key), "parameter");
    }

    /// <inheritdoc />
    public NodeDefinitionId Id { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public string Category { get; }

    /// <inheritdoc />
    public string Subtitle { get; }

    /// <inheritdoc />
    public string? Description { get; }

    /// <inheritdoc />
    public string AccentHex { get; }

    /// <inheritdoc />
    public double DefaultWidth { get; }

    /// <inheritdoc />
    public double DefaultHeight { get; }

    /// <inheritdoc />
    public IReadOnlyList<PortDefinition> InputPorts { get; }

    /// <inheritdoc />
    public IReadOnlyList<PortDefinition> OutputPorts { get; }

    /// <inheritdoc />
    public IReadOnlyList<NodeParameterDefinition> Parameters { get; }

    private static void EnsureUniqueKeys(IEnumerable<string> keys, string scope)
    {
        var duplicates = keys
            .GroupBy(key => key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new ArgumentException(
                $"Duplicate {scope} key(s) are not allowed: {string.Join(", ", duplicates)}.");
        }
    }
}
