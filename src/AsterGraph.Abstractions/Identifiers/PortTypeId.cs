namespace AsterGraph.Abstractions.Identifiers;

/// <summary>
/// Stable identifier for a port value type (for example, "aster.number.float").
/// </summary>
public sealed record PortTypeId
{
    /// <summary>
    /// 初始化端口类型标识。
    /// </summary>
    /// <param name="value">稳定的端口类型标识字符串。</param>
    public PortTypeId(string value)
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
