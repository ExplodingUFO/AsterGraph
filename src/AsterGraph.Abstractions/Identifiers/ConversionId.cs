namespace AsterGraph.Abstractions.Identifiers;

/// <summary>
/// Stable identifier for a compatible implicit conversion rule.
/// </summary>
public sealed record ConversionId
{
    /// <summary>
    /// 初始化隐式转换标识。
    /// </summary>
    /// <param name="value">稳定的转换标识字符串。</param>
    public ConversionId(string value)
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
