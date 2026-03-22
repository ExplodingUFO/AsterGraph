using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Immutable definition for a single input or output port.
/// </summary>
public sealed record PortDefinition
{
    public PortDefinition(
        string key,
        string displayName,
        PortTypeId typeId,
        string accentHex = "#FFFFFF",
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Key = key.Trim();
        DisplayName = displayName.Trim();
        TypeId = typeId;
        AccentHex = accentHex;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public string Key { get; }

    public string DisplayName { get; }

    public PortTypeId TypeId { get; }

    public string AccentHex { get; }

    public string? Description { get; }
}
