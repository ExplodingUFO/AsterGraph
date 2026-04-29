using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Documentation;
using AsterGraph.Editor.Parameters;

namespace AsterGraph.Editor.Runtime.Internal;

internal static class GraphEditorValidationSnapshotProjector
{
    public static GraphEditorValidationSnapshot Project(
        GraphDocument document,
        GraphEditorSessionDescriptorSupport? descriptorSupport)
    {
        ArgumentNullException.ThrowIfNull(document);

        var issues = new List<GraphEditorValidationIssueSnapshot>();
        foreach (var scope in document.GraphScopes)
        {
            ValidateScope(scope, descriptorSupport, issues);
        }

        var errorCount = issues.Count(issue => issue.Severity == GraphEditorValidationIssueSeverity.Error);
        var warningCount = issues.Count(issue => issue.Severity == GraphEditorValidationIssueSeverity.Warning);
        return new GraphEditorValidationSnapshot(errorCount == 0, errorCount, warningCount, issues);
    }

    private static void ValidateScope(
        GraphScope scope,
        GraphEditorSessionDescriptorSupport? descriptorSupport,
        List<GraphEditorValidationIssueSnapshot> issues)
    {
        var nodesById = scope.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);

        foreach (var node in scope.Nodes)
        {
            ValidateNodeParameters(scope.Id, node, descriptorSupport, issues);
        }

        foreach (var connection in scope.Connections)
        {
            ValidateConnection(scope.Id, connection, nodesById, descriptorSupport, issues);
        }
    }

    private static void ValidateNodeParameters(
        string scopeId,
        GraphNode node,
        GraphEditorSessionDescriptorSupport? descriptorSupport,
        List<GraphEditorValidationIssueSnapshot> issues)
    {
        if (node.DefinitionId is null)
        {
            return;
        }

        if (!TryResolveDefinition(node, descriptorSupport, out var definition))
        {
            issues.Add(Warning(
                "node.definition-unresolved",
                $"Node '{node.Title}' references unresolved definition '{node.DefinitionId.Value}'.",
                scopeId,
                nodeId: node.Id));
            return;
        }

        foreach (var parameter in definition.Parameters)
        {
            var value = node.ParameterValues?
                .FirstOrDefault(candidate => string.Equals(candidate.Key, parameter.Key, StringComparison.Ordinal))
                ?.Value
                ?? parameter.DefaultValue;
            var validation = NodeParameterValueAdapter.NormalizeValue(parameter, value);
            if (validation.IsValid)
            {
                continue;
            }

            issues.Add(Error(
                "node.parameter-invalid",
                $"{node.Title}.{parameter.DisplayName}: {validation.ValidationError}",
                scopeId,
                nodeId: node.Id,
                endpointId: parameter.Key,
                parameterKey: parameter.Key,
                helpTarget: CreateParameterHelpTarget(node, definition, parameter)));
        }
    }

    private static void ValidateConnection(
        string scopeId,
        GraphConnection connection,
        IReadOnlyDictionary<string, GraphNode> nodesById,
        GraphEditorSessionDescriptorSupport? descriptorSupport,
        List<GraphEditorValidationIssueSnapshot> issues)
    {
        nodesById.TryGetValue(connection.SourceNodeId, out var sourceNode);
        nodesById.TryGetValue(connection.TargetNodeId, out var targetNode);

        if (sourceNode is null)
        {
            issues.Add(Error(
                "connection.source-node-missing",
                $"Connection '{connection.Id}' references missing source node '{connection.SourceNodeId}'.",
                scopeId,
                connectionId: connection.Id,
                nodeId: connection.SourceNodeId,
                endpointId: connection.SourcePortId));
        }

        if (targetNode is null)
        {
            issues.Add(Error(
                "connection.target-node-missing",
                $"Connection '{connection.Id}' references missing target node '{connection.TargetNodeId}'.",
                scopeId,
                connectionId: connection.Id,
                nodeId: connection.TargetNodeId,
                endpointId: connection.TargetPortId,
                targetKind: connection.TargetKind));
        }

        var sourceTypeId = ResolveSourceType(scopeId, connection, sourceNode, issues);
        var targetTypeId = ResolveTargetType(scopeId, connection, targetNode, descriptorSupport, issues);
        if (sourceTypeId is null || targetTypeId is null || descriptorSupport is null)
        {
            return;
        }

        var compatibility = descriptorSupport.CompatibilityService.Evaluate(sourceTypeId, targetTypeId);
        if (!compatibility.IsCompatible)
        {
            issues.Add(Error(
                "connection.incompatible-endpoint-types",
                $"Connection '{connection.Id}' is incompatible: {sourceTypeId.Value} -> {targetTypeId.Value}.",
                scopeId,
                connectionId: connection.Id,
                nodeId: connection.TargetNodeId,
                endpointId: connection.TargetPortId,
                targetKind: connection.TargetKind,
                helpTarget: CreateConnectionHelpTarget(connection, sourceNode, targetNode, descriptorSupport)));
        }
    }

    private static PortTypeId? ResolveSourceType(
        string scopeId,
        GraphConnection connection,
        GraphNode? sourceNode,
        List<GraphEditorValidationIssueSnapshot> issues)
    {
        if (sourceNode is null)
        {
            return null;
        }

        var sourcePort = sourceNode.Outputs.FirstOrDefault(port => string.Equals(port.Id, connection.SourcePortId, StringComparison.Ordinal));
        if (sourcePort is null)
        {
            issues.Add(Error(
                "connection.source-output-missing",
                $"Connection '{connection.Id}' references missing source output '{connection.SourcePortId}'.",
                scopeId,
                connectionId: connection.Id,
                nodeId: sourceNode.Id,
                endpointId: connection.SourcePortId));
            return null;
        }

        if (sourcePort.TypeId is not null)
        {
            return sourcePort.TypeId;
        }

        issues.Add(Error(
            "connection.source-type-missing",
            $"Connection '{connection.Id}' source output '{sourcePort.Label}' is missing a type identifier.",
            scopeId,
            connectionId: connection.Id,
            nodeId: sourceNode.Id,
            endpointId: sourcePort.Id,
            helpTarget: CreatePortHelpTarget(sourceNode, sourcePort, PortDirection.Output)));
        return null;
    }

    private static PortTypeId? ResolveTargetType(
        string scopeId,
        GraphConnection connection,
        GraphNode? targetNode,
        GraphEditorSessionDescriptorSupport? descriptorSupport,
        List<GraphEditorValidationIssueSnapshot> issues)
    {
        if (targetNode is null)
        {
            return null;
        }

        if (connection.TargetKind == GraphConnectionTargetKind.Port)
        {
            return ResolveTargetPortType(scopeId, connection, targetNode, issues);
        }

        return ResolveTargetParameterType(scopeId, connection, targetNode, descriptorSupport, issues);
    }

    private static PortTypeId? ResolveTargetPortType(
        string scopeId,
        GraphConnection connection,
        GraphNode targetNode,
        List<GraphEditorValidationIssueSnapshot> issues)
    {
        var targetPort = targetNode.Inputs.FirstOrDefault(port => string.Equals(port.Id, connection.TargetPortId, StringComparison.Ordinal));
        if (targetPort is null)
        {
            issues.Add(Error(
                "connection.target-input-missing",
                $"Connection '{connection.Id}' references missing target input '{connection.TargetPortId}'.",
                scopeId,
                connectionId: connection.Id,
                nodeId: targetNode.Id,
                endpointId: connection.TargetPortId,
                targetKind: GraphConnectionTargetKind.Port));
            return null;
        }

        if (targetPort.TypeId is not null)
        {
            return targetPort.TypeId;
        }

        issues.Add(Error(
            "connection.target-type-missing",
            $"Connection '{connection.Id}' target input '{targetPort.Label}' is missing a type identifier.",
            scopeId,
            connectionId: connection.Id,
            nodeId: targetNode.Id,
            endpointId: targetPort.Id,
            targetKind: GraphConnectionTargetKind.Port,
            helpTarget: CreatePortHelpTarget(targetNode, targetPort, PortDirection.Input)));
        return null;
    }

    private static PortTypeId? ResolveTargetParameterType(
        string scopeId,
        GraphConnection connection,
        GraphNode targetNode,
        GraphEditorSessionDescriptorSupport? descriptorSupport,
        List<GraphEditorValidationIssueSnapshot> issues)
    {
        if (!TryResolveDefinition(targetNode, descriptorSupport, out var definition))
        {
            issues.Add(Warning(
                "connection.target-parameter-unresolved",
                $"Connection '{connection.Id}' targets parameter '{connection.TargetPortId}', but node definition metadata is unresolved.",
                scopeId,
                connectionId: connection.Id,
                nodeId: targetNode.Id,
                endpointId: connection.TargetPortId,
                targetKind: GraphConnectionTargetKind.Parameter,
                parameterKey: connection.TargetPortId));
            return null;
        }

        var parameter = definition.Parameters.FirstOrDefault(candidate => string.Equals(candidate.Key, connection.TargetPortId, StringComparison.Ordinal));
        if (parameter is not null)
        {
            return parameter.ValueType;
        }

        issues.Add(Error(
            "connection.target-parameter-missing",
            $"Connection '{connection.Id}' references missing target parameter '{connection.TargetPortId}'.",
            scopeId,
            connectionId: connection.Id,
            nodeId: targetNode.Id,
            endpointId: connection.TargetPortId,
            targetKind: GraphConnectionTargetKind.Parameter,
            parameterKey: connection.TargetPortId));
        return null;
    }

    private static bool TryResolveDefinition(
        GraphNode node,
        GraphEditorSessionDescriptorSupport? descriptorSupport,
        out INodeDefinition definition)
    {
        definition = null!;
        return node.DefinitionId is not null
            && descriptorSupport is not null
            && descriptorSupport.NodeCatalog.TryGetDefinition(node.DefinitionId, out definition!)
            && definition is not null;
    }

    private static GraphEditorValidationIssueSnapshot Error(
        string code,
        string message,
        string scopeId,
        string? nodeId = null,
        string? connectionId = null,
        string? endpointId = null,
        GraphConnectionTargetKind? targetKind = null,
        string? parameterKey = null,
        GraphEditorValidationHelpTargetSnapshot? helpTarget = null)
        => new(
            code,
            GraphEditorValidationIssueSeverity.Error,
            message,
            scopeId,
            nodeId,
            connectionId,
            endpointId,
            targetKind,
            parameterKey,
            helpTarget);

    private static GraphEditorValidationIssueSnapshot Warning(
        string code,
        string message,
        string scopeId,
        string? nodeId = null,
        string? connectionId = null,
        string? endpointId = null,
        GraphConnectionTargetKind? targetKind = null,
        string? parameterKey = null,
        GraphEditorValidationHelpTargetSnapshot? helpTarget = null)
        => new(
            code,
            GraphEditorValidationIssueSeverity.Warning,
            message,
            scopeId,
            nodeId,
            connectionId,
            endpointId,
            targetKind,
            parameterKey,
            helpTarget);

    private static GraphEditorValidationHelpTargetSnapshot? CreateParameterHelpTarget(
        GraphNode node,
        INodeDefinition definition,
        NodeParameterDefinition parameter)
    {
        var helpText = NodeParameterInspectorMetadata.BuildHelpText(parameter);
        return string.IsNullOrWhiteSpace(helpText)
            ? null
            : new GraphEditorValidationHelpTargetSnapshot(
                "Parameter",
                $"{node.Title}.{parameter.DisplayName}",
                helpText,
                node.Id,
                ParameterKey: parameter.Key);
    }

    private static GraphEditorValidationHelpTargetSnapshot? CreatePortHelpTarget(
        GraphNode node,
        GraphPort port,
        PortDirection direction)
    {
        var helpText = NodeDocumentationText.JoinDistinct(
            port.GroupName,
            port.DataType,
            port.TypeId?.Value);
        return string.IsNullOrWhiteSpace(helpText)
            ? null
            : new GraphEditorValidationHelpTargetSnapshot(
                "Port",
                $"{node.Title}.{port.Label}",
                helpText,
                node.Id,
                EndpointId: port.Id);
    }

    private static GraphEditorValidationHelpTargetSnapshot? CreateConnectionHelpTarget(
        GraphConnection connection,
        GraphNode? sourceNode,
        GraphNode? targetNode,
        GraphEditorSessionDescriptorSupport descriptorSupport)
    {
        if (sourceNode?.DefinitionId is null || targetNode?.DefinitionId is null)
        {
            return null;
        }

        var provider = new NodeDocumentationProvider(descriptorSupport.NodeCatalog);
        var sourceDocumentation = provider.GetPortDocumentation(sourceNode.DefinitionId, connection.SourcePortId, PortDirection.Output);
        if (sourceDocumentation is null)
        {
            return null;
        }

        string? targetHelpText;
        string targetLabel;
        string? parameterKey = null;
        if (connection.TargetKind == GraphConnectionTargetKind.Parameter)
        {
            var parameterDocumentation = provider.GetParameterDocumentation(targetNode.DefinitionId, connection.TargetPortId);
            if (parameterDocumentation is null)
            {
                return null;
            }

            targetHelpText = parameterDocumentation.InspectorHelpText;
            targetLabel = parameterDocumentation.DisplayName;
            parameterKey = parameterDocumentation.ParameterKey;
        }
        else
        {
            var targetDocumentation = provider.GetPortDocumentation(targetNode.DefinitionId, connection.TargetPortId, PortDirection.Input);
            if (targetDocumentation is null)
            {
                return null;
            }

            targetHelpText = targetDocumentation.HelpText;
            targetLabel = targetDocumentation.DisplayName;
        }

        var helpText = NodeDocumentationText.JoinDistinct(sourceDocumentation.HelpText, targetHelpText);
        return string.IsNullOrWhiteSpace(helpText)
            ? null
            : new GraphEditorValidationHelpTargetSnapshot(
                "Connection",
                $"{sourceNode.Title}.{sourceDocumentation.DisplayName} -> {targetNode.Title}.{targetLabel}",
                helpText,
                targetNode.Id,
                connection.Id,
                connection.TargetPortId,
                parameterKey);
    }

}
