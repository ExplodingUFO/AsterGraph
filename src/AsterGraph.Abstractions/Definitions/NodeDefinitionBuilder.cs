using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Thin fluent wrapper for creating <see cref="NodeDefinition" /> instances.
/// </summary>
public sealed class NodeDefinitionBuilder
{
    private readonly NodeDefinitionId _id;
    private readonly string _displayName;
    private string _category = "General";
    private string _subtitle = "Node";
    private string? _description;
    private string _accentHex = "#FFFFFF";
    private double _defaultWidth = 220d;
    private double _defaultHeight = 140d;
    private NodeSurfaceTierProfile? _surfaceTierProfile;
    private readonly List<PortDefinition> _inputs = [];
    private readonly List<PortDefinition> _outputs = [];
    private readonly List<NodeParameterDefinition> _parameters = [];

    private NodeDefinitionBuilder(NodeDefinitionId id, string displayName)
    {
        _id = id;
        _displayName = displayName;
    }

    public static NodeDefinitionBuilder Create(NodeDefinitionId id, string displayName)
        => new(id, displayName);

    public static NodeDefinitionBuilder Create(string id, string displayName)
        => new(new NodeDefinitionId(id), displayName);

    public NodeDefinitionBuilder Category(string category) { _category = category; return this; }
    public NodeDefinitionBuilder Subtitle(string subtitle) { _subtitle = subtitle; return this; }
    public NodeDefinitionBuilder Description(string? description) { _description = description; return this; }
    public NodeDefinitionBuilder Accent(string accentHex) { _accentHex = accentHex; return this; }
    public NodeDefinitionBuilder Size(double width, double height) { _defaultWidth = width; _defaultHeight = height; return this; }
    public NodeDefinitionBuilder SurfaceTierProfile(NodeSurfaceTierProfile? surfaceTierProfile) { _surfaceTierProfile = surfaceTierProfile; return this; }

    public NodeDefinitionBuilder Input(PortDefinition port) { _inputs.Add(port); return this; }
    public NodeDefinitionBuilder Input(string key, string displayName, PortTypeId typeId, string accentHex = "#FFFFFF")
        => Input(new PortDefinition(key, displayName, typeId, accentHex));
    public NodeDefinitionBuilder Input(string key, string displayName, string typeId, string accentHex = "#FFFFFF")
        => Input(key, displayName, new PortTypeId(typeId), accentHex);

    public NodeDefinitionBuilder Output(PortDefinition port) { _outputs.Add(port); return this; }
    public NodeDefinitionBuilder Output(string key, string displayName, PortTypeId typeId, string accentHex = "#FFFFFF")
        => Output(new PortDefinition(key, displayName, typeId, accentHex));
    public NodeDefinitionBuilder Output(string key, string displayName, string typeId, string accentHex = "#FFFFFF")
        => Output(key, displayName, new PortTypeId(typeId), accentHex);

    public NodeDefinitionBuilder Parameter(NodeParameterDefinition parameter) { _parameters.Add(parameter); return this; }
    public NodeDefinitionBuilder Parameter(NodeParameterDefinitionBuilder parameterBuilder)
        => Parameter(parameterBuilder.Build());

    public NodeDefinition Build()
        => new(
            _id,
            _displayName,
            _category,
            _subtitle,
            _inputs,
            _outputs,
            _parameters,
            _description,
            _accentHex,
            _defaultWidth,
            _defaultHeight,
            _surfaceTierProfile);
}