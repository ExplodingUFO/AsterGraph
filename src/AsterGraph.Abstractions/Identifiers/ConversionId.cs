namespace AsterGraph.Abstractions.Identifiers;

/// <summary>
/// Stable identifier for a compatible implicit conversion rule.
/// </summary>
public sealed record ConversionId
{
    public ConversionId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
