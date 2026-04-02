namespace AsterGraph.Abstractions.Identifiers;

/// <summary>
/// Stable identifier for a node definition (for example, "aster.math.add").
/// </summary>
public sealed record NodeDefinitionId
{
    /// <summary>
    /// 初始化节点定义标识。
    /// </summary>
    /// <param name="value">稳定的节点定义标识字符串。</param>
    public NodeDefinitionId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// 标识的原始字符串值。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 返回标识的字符串表示。
    /// </summary>
    public override string ToString() => Value;
}
