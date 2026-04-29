using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Parameters;

namespace AsterGraph.Editor.Documentation;

public sealed class NodeDocumentationProvider : INodeDocumentationProvider
{
    private readonly INodeCatalog _nodeCatalog;

    public NodeDocumentationProvider(INodeCatalog nodeCatalog)
        => _nodeCatalog = nodeCatalog ?? throw new ArgumentNullException(nameof(nodeCatalog));

    public NodeDocumentationSnapshot? GetNodeDocumentation(NodeDefinitionId? definitionId)
    {
        if (!TryGetDefinition(definitionId, out var definition))
        {
            return null;
        }

        return new NodeDocumentationSnapshot(
            definition.Id,
            definition.DisplayName,
            definition.Category,
            definition.Subtitle,
            definition.Description,
            definition.InputPorts.Select(port => CreatePortDocumentation(definition.Id, port, PortDirection.Input)).ToList(),
            definition.OutputPorts.Select(port => CreatePortDocumentation(definition.Id, port, PortDirection.Output)).ToList(),
            definition.Parameters.Select(parameter => CreateParameterDocumentation(definition.Id, parameter)).ToList());
    }

    public PortDocumentationSnapshot? GetPortDocumentation(NodeDefinitionId? definitionId, string portId, PortDirection direction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portId);

        if (!TryGetDefinition(definitionId, out var definition))
        {
            return null;
        }

        var ports = direction == PortDirection.Input ? definition.InputPorts : definition.OutputPorts;
        var port = ports.FirstOrDefault(candidate => string.Equals(candidate.Key, portId, StringComparison.Ordinal));
        return port is null
            ? null
            : CreatePortDocumentation(definition.Id, port, direction);
    }

    public ParameterDocumentationSnapshot? GetParameterDocumentation(NodeDefinitionId? definitionId, string parameterKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

        if (!TryGetDefinition(definitionId, out var definition))
        {
            return null;
        }

        var parameter = definition.Parameters.FirstOrDefault(candidate => string.Equals(candidate.Key, parameterKey, StringComparison.Ordinal));
        return parameter is null
            ? null
            : CreateParameterDocumentation(definition.Id, parameter);
    }

    public EdgeDocumentationSnapshot? GetEdgeDocumentation(GraphConnection connection, GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(document);

        var sourceNode = document.Nodes.FirstOrDefault(node => string.Equals(node.Id, connection.SourceNodeId, StringComparison.Ordinal));
        var targetNode = document.Nodes.FirstOrDefault(node => string.Equals(node.Id, connection.TargetNodeId, StringComparison.Ordinal));
        if (sourceNode?.DefinitionId is null || targetNode?.DefinitionId is null)
        {
            return null;
        }

        var sourcePort = GetPortDocumentation(sourceNode.DefinitionId, connection.SourcePortId, PortDirection.Output);
        if (sourcePort is null)
        {
            return null;
        }

        string? targetHelp;
        string targetLabel;
        if (connection.TargetKind == GraphConnectionTargetKind.Parameter)
        {
            var parameter = GetParameterDocumentation(targetNode.DefinitionId, connection.TargetPortId);
            if (parameter is null)
            {
                return null;
            }

            targetLabel = parameter.DisplayName;
            targetHelp = parameter.InspectorHelpText;
        }
        else
        {
            var targetPort = GetPortDocumentation(targetNode.DefinitionId, connection.TargetPortId, PortDirection.Input);
            if (targetPort is null)
            {
                return null;
            }

            targetLabel = targetPort.DisplayName;
            targetHelp = targetPort.HelpText;
        }

        return new EdgeDocumentationSnapshot(
            connection.Id,
            sourceNode.Title,
            sourcePort.DisplayName,
            targetNode.Title,
            targetLabel,
            connection.TargetKind,
            sourcePort.HelpText,
            targetHelp);
    }

    private bool TryGetDefinition(NodeDefinitionId? definitionId, out INodeDefinition definition)
    {
        definition = null!;
        return definitionId is not null
            && _nodeCatalog.TryGetDefinition(definitionId, out definition!)
            && definition is not null;
    }

    private static PortDocumentationSnapshot CreatePortDocumentation(
        NodeDefinitionId definitionId,
        PortDefinition port,
        PortDirection direction)
        => new(
            definitionId,
            port.Key,
            port.DisplayName,
            direction,
            port.TypeId,
            port.Description,
            port.GroupName,
            port.MinConnections,
            port.MaxConnections,
            BuildConnectionRuleText(port));

    private static ParameterDocumentationSnapshot CreateParameterDocumentation(
        NodeDefinitionId definitionId,
        NodeParameterDefinition parameter)
        => new(
            definitionId,
            parameter.Key,
            parameter.DisplayName,
            parameter.ValueType,
            parameter.Description,
            NodeParameterInspectorMetadata.BuildHelpText(parameter),
            BuildResetDefaultGuidance(parameter),
            BuildExampleText(parameter),
            BuildValidationRuleText(parameter));

    private static string? BuildConnectionRuleText(PortDefinition port)
    {
        if (port.MinConnections == 0 && port.MaxConnections == int.MaxValue)
        {
            return null;
        }

        if (port.MaxConnections == int.MaxValue)
        {
            return $"Connections: at least {port.MinConnections}.";
        }

        return $"Connections: {port.MinConnections} - {port.MaxConnections}.";
    }

    private static string? BuildResetDefaultGuidance(NodeParameterDefinition parameter)
    {
        var defaultText = NodeParameterValueAdapter.FormatValueForEditor(parameter.DefaultValue);
        return string.IsNullOrWhiteSpace(defaultText)
            ? null
            : $"Reset restores default value {defaultText}.";
    }

    private static string? BuildExampleText(NodeParameterDefinition parameter)
    {
        if (!string.IsNullOrWhiteSpace(parameter.PlaceholderText))
        {
            return $"Example: {parameter.PlaceholderText}.";
        }

        var describedOption = parameter.Constraints.AllowedOptions.FirstOrDefault(option => !string.IsNullOrWhiteSpace(option.Description));
        return describedOption is null
            ? null
            : $"Example option: {describedOption.Label} - {describedOption.Description}";
    }

    private static string? BuildValidationRuleText(NodeParameterDefinition parameter)
    {
        var constraints = parameter.Constraints;
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(constraints.ValidationPatternDescription))
        {
            segments.Add($"Must match {constraints.ValidationPatternDescription}.");
        }

        if (constraints.Minimum is double minimum && constraints.Maximum is double maximum)
        {
            segments.Add($"Range {minimum} - {maximum}.");
        }
        else if (constraints.Minimum is double min)
        {
            segments.Add($"Minimum {min}.");
        }
        else if (constraints.Maximum is double max)
        {
            segments.Add($"Maximum {max}.");
        }

        if (constraints.MinimumLength is int minimumLength && constraints.MaximumLength is int maximumLength)
        {
            segments.Add($"Length {minimumLength} - {maximumLength}.");
        }
        else if (constraints.MinimumLength is int minLength)
        {
            segments.Add($"Minimum length {minLength}.");
        }
        else if (constraints.MaximumLength is int maxLength)
        {
            segments.Add($"Maximum length {maxLength}.");
        }

        if (constraints.MinimumItemCount is int minimumItems && constraints.MaximumItemCount is int maximumItems)
        {
            segments.Add($"Item count {minimumItems} - {maximumItems}.");
        }
        else if (constraints.MinimumItemCount is int minItems)
        {
            segments.Add($"Minimum item count {minItems}.");
        }
        else if (constraints.MaximumItemCount is int maxItems)
        {
            segments.Add($"Maximum item count {maxItems}.");
        }

        return NodeDocumentationText.JoinDistinct(segments.ToArray());
    }
}

