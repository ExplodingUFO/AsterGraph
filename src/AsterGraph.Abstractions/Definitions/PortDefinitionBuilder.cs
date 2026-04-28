using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Thin fluent wrapper for creating <see cref="PortDefinition" /> instances.
/// </summary>
public sealed class PortDefinitionBuilder
{
    private readonly string _key;
    private readonly string _displayName;
    private readonly PortTypeId _typeId;
    private string _accentHex = "#FFFFFF";
    private string? _description;
    private string? _groupName;
    private int _minConnections;
    private int _maxConnections = int.MaxValue;

    private PortDefinitionBuilder(string key, string displayName, PortTypeId typeId)
    {
        _key = key;
        _displayName = displayName;
        _typeId = typeId;
    }

    public static PortDefinitionBuilder Create(string key, string displayName, PortTypeId typeId)
        => new(key, displayName, typeId);

    public static PortDefinitionBuilder Create(string key, string displayName, string typeId)
        => new(key, displayName, new PortTypeId(typeId));

    public PortDefinitionBuilder Accent(string accentHex)
    {
        _accentHex = accentHex;
        return this;
    }

    public PortDefinitionBuilder Description(string? description)
    {
        _description = description;
        return this;
    }

    public PortDefinitionBuilder Group(string? groupName)
    {
        _groupName = groupName;
        return this;
    }

    public PortDefinitionBuilder Connections(int min = 0, int max = int.MaxValue)
    {
        _minConnections = min;
        _maxConnections = max;
        return this;
    }

    public PortDefinition Build()
        => new(
            _key,
            _displayName,
            _typeId,
            _accentHex,
            _description,
            _groupName,
            _minConnections,
            _maxConnections);
}