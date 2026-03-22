namespace AsterGraph.Abstractions.Identifiers;

/// <summary>
/// Stable identifier for a node definition (for example, "aster.math.add").
/// </summary>
public sealed record NodeDefinitionId
{
    public NodeDefinitionId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
