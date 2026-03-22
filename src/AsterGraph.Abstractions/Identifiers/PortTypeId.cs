namespace AsterGraph.Abstractions.Identifiers;

/// <summary>
/// Stable identifier for a port value type (for example, "aster.number.float").
/// </summary>
public sealed record PortTypeId
{
    public PortTypeId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
