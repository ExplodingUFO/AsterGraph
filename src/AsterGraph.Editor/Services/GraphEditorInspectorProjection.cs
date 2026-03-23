using AsterGraph.Abstractions.Catalog;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal sealed class GraphEditorInspectorProjection
{
    public string FormatPorts(IEnumerable<PortViewModel> ports)
    {
        var items = ports.ToList();
        if (items.Count == 0)
        {
            return "None";
        }

        return string.Join(Environment.NewLine, items.Select(port => $"{port.Label}  ·  {port.DataType}"));
    }

    public string FormatRelatedNodes(
        IEnumerable<ConnectionViewModel> connections,
        bool useSource,
        Func<string, NodeViewModel?> findNode)
    {
        ArgumentNullException.ThrowIfNull(connections);
        ArgumentNullException.ThrowIfNull(findNode);

        var lines = connections
            .Select(connection =>
            {
                var relatedId = useSource ? connection.SourceNodeId : connection.TargetNodeId;
                var relatedPortId = useSource ? connection.SourcePortId : connection.TargetPortId;
                var relatedNode = findNode(relatedId);
                var relatedPort = relatedNode?.GetPort(relatedPortId);
                if (relatedNode is null)
                {
                    return null;
                }

                return $"{relatedNode.Title}  ·  {relatedPort?.Label ?? relatedPortId}";
            })
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return lines.Count == 0
            ? "None"
            : string.Join(Environment.NewLine, lines);
    }

    public string FormatSelectionCaption(NodeViewModel? selectedNode, bool hasMultipleSelection, int selectedNodeCount)
    {
        if (selectedNode is null)
        {
            return "No selection";
        }

        return hasMultipleSelection
            ? $"{selectedNodeCount} nodes selected  ·  primary {selectedNode.Title}"
            : $"{selectedNode.InputCount} inputs  ·  {selectedNode.OutputCount} outputs";
    }

    public IReadOnlyList<NodeParameterViewModel> BuildSelectedNodeParameters(
        IReadOnlyList<NodeViewModel> selectedNodes,
        INodeCatalog nodeCatalog,
        bool enableBatchParameterEditing,
        bool canEditNodeParameters,
        Action<NodeParameterViewModel, object?> applyParameterValue)
    {
        ArgumentNullException.ThrowIfNull(selectedNodes);
        ArgumentNullException.ThrowIfNull(nodeCatalog);
        ArgumentNullException.ThrowIfNull(applyParameterValue);

        if (selectedNodes.Count == 0)
        {
            return [];
        }

        if (selectedNodes.Count > 1 && !enableBatchParameterEditing)
        {
            return [];
        }

        var sharedDefinitionId = selectedNodes[0].DefinitionId;
        if (sharedDefinitionId is null || selectedNodes.Any(node => node.DefinitionId != sharedDefinitionId))
        {
            return [];
        }

        if (!nodeCatalog.TryGetDefinition(sharedDefinitionId, out var definition) || definition is null)
        {
            return [];
        }

        var parameters = new List<NodeParameterViewModel>(definition.Parameters.Count);
        foreach (var parameter in definition.Parameters)
        {
            var currentValues = selectedNodes
                .Select(node => node.GetParameterValue(parameter.Key) ?? parameter.DefaultValue)
                .ToList();
            parameters.Add(new NodeParameterViewModel(
                parameter,
                currentValues,
                applyParameterValue,
                isHostReadOnly: !canEditNodeParameters));
        }

        return parameters;
    }
}
