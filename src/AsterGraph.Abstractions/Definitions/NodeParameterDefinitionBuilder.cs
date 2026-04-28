using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Thin fluent wrapper for creating <see cref="NodeParameterDefinition" /> instances.
/// </summary>
public sealed class NodeParameterDefinitionBuilder
{
    private readonly string _key;
    private readonly string _displayName;
    private readonly PortTypeId _valueType;
    private readonly ParameterEditorKind _editorKind;
    private bool _isRequired;
    private string? _description;
    private object? _defaultValue;
    private ParameterConstraints? _constraints;
    private string? _groupName;
    private string? _placeholderText;
    private string? _templateKey;
    private string? _helpText;
    private int _sortOrder;
    private bool _isAdvanced;
    private string? _unitSuffix;
    private int _contractVersion = 1;

    private NodeParameterDefinitionBuilder(string key, string displayName, PortTypeId valueType, ParameterEditorKind editorKind)
    {
        _key = key;
        _displayName = displayName;
        _valueType = valueType;
        _editorKind = editorKind;
    }

    public static NodeParameterDefinitionBuilder Create(string key, string displayName, PortTypeId valueType, ParameterEditorKind editorKind)
        => new(key, displayName, valueType, editorKind);

    public static NodeParameterDefinitionBuilder Create(string key, string displayName, string valueType, ParameterEditorKind editorKind)
        => new(key, displayName, new PortTypeId(valueType), editorKind);

    public NodeParameterDefinitionBuilder Required(bool isRequired = true) { _isRequired = isRequired; return this; }
    public NodeParameterDefinitionBuilder Description(string? description) { _description = description; return this; }
    public NodeParameterDefinitionBuilder DefaultValue(object? defaultValue) { _defaultValue = defaultValue; return this; }
    public NodeParameterDefinitionBuilder Constraints(ParameterConstraints? constraints) { _constraints = constraints; return this; }
    public NodeParameterDefinitionBuilder Group(string? groupName) { _groupName = groupName; return this; }
    public NodeParameterDefinitionBuilder Placeholder(string? placeholderText) { _placeholderText = placeholderText; return this; }
    public NodeParameterDefinitionBuilder Template(string? templateKey) { _templateKey = templateKey; return this; }
    public NodeParameterDefinitionBuilder Help(string? helpText) { _helpText = helpText; return this; }
    public NodeParameterDefinitionBuilder SortOrder(int sortOrder) { _sortOrder = sortOrder; return this; }
    public NodeParameterDefinitionBuilder Advanced(bool isAdvanced = true) { _isAdvanced = isAdvanced; return this; }
    public NodeParameterDefinitionBuilder Unit(string? unitSuffix) { _unitSuffix = unitSuffix; return this; }
    public NodeParameterDefinitionBuilder ContractVersion(int contractVersion) { _contractVersion = contractVersion; return this; }

    public NodeParameterDefinitionBuilder Range(double? minimum = null, double? maximum = null)
    {
        _constraints = (_constraints ?? new ParameterConstraints()) with { Minimum = minimum, Maximum = maximum };
        return this;
    }

    public NodeParameterDefinitionBuilder Options(params ParameterOptionDefinition[] options)
    {
        _constraints = (_constraints ?? new ParameterConstraints()) with { AllowedOptions = options };
        return this;
    }

    public NodeParameterDefinitionBuilder Option(string value, string label, string? description = null)
    {
        var constraints = _constraints ?? new ParameterConstraints();
        _constraints = constraints with
        {
            AllowedOptions = [.. constraints.AllowedOptions, new ParameterOptionDefinition(value, label, description)],
        };
        return this;
    }

    public NodeParameterDefinition Build()
        => new(
            _key,
            _displayName,
            _valueType,
            _editorKind,
            _isRequired,
            _description,
            _defaultValue,
            _constraints,
            _groupName,
            _placeholderText,
            _templateKey,
            _helpText,
            _sortOrder,
            _isAdvanced,
            _unitSuffix,
            _contractVersion);
}