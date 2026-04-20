using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Kernel.Internal;

internal sealed class GraphEditorKernelCompatibilityQueries
{
    private readonly IPortCompatibilityService _compatibilityService;
    private readonly INodeCatalog _nodeCatalog;

    public GraphEditorKernelCompatibilityQueries(
        IPortCompatibilityService compatibilityService,
        INodeCatalog nodeCatalog)
    {
        _compatibilityService = compatibilityService ?? throw new ArgumentNullException(nameof(compatibilityService));
        _nodeCatalog = nodeCatalog ?? throw new ArgumentNullException(nameof(nodeCatalog));
    }

    public IReadOnlyList<GraphEditorCompatibleTargetState> GetCompatibleTargetStates(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId)
    {
        ArgumentNullException.ThrowIfNull(document);

        var sourceNode = FindNode(document, sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null || sourcePort.TypeId is null)
        {
            return [];
        }

        return document.Nodes
            .SelectMany(node => EnumerateTargets(node, sourceNodeId, sourcePortId))
            .Select(target => target with
            {
                Compatibility = _compatibilityService.Evaluate(sourcePort.TypeId!, target.TargetTypeId),
            })
            .Where(target => target.Compatibility.IsCompatible)
            .ToList();
    }

    public IReadOnlyList<GraphEditorCompatibleConnectionTargetSnapshot> GetCompatibleConnectionTargets(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId)
        => GetCompatibleTargetStates(document, sourceNodeId, sourcePortId)
            .Select(target => new GraphEditorCompatibleConnectionTargetSnapshot(
                target.Node.Id,
                target.Node.Title,
                target.Target.TargetId,
                target.TargetLabel,
                target.Target.Kind,
                target.TargetTypeId,
                target.TargetAccentHex,
                target.Compatibility))
            .ToList();

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId)
        => GetCompatibleTargetStates(document, sourceNodeId, sourcePortId)
            .Where(target => target.Target.Kind == GraphConnectionTargetKind.Port)
            .Select(target => new GraphEditorCompatiblePortTargetSnapshot(
                target.Node.Id,
                target.Node.Title,
                target.Target.TargetId,
                target.TargetLabel,
                target.TargetTypeId,
                target.TargetAccentHex,
                target.Compatibility))
            .ToList();

    private IEnumerable<GraphEditorCompatibleTargetState> EnumerateTargets(
        GraphNode node,
        string sourceNodeId,
        string sourcePortId)
    {
        foreach (var port in node.Inputs)
        {
            if (node.Id == sourceNodeId && string.Equals(port.Id, sourcePortId, StringComparison.Ordinal))
            {
                continue;
            }

            if (port.TypeId is null)
            {
                continue;
            }

            yield return new GraphEditorCompatibleTargetState(
                node,
                new GraphConnectionTargetRef(node.Id, port.Id, GraphConnectionTargetKind.Port),
                port.Label,
                port.TypeId,
                port.AccentHex,
                PortCompatibilityResult.Rejected());
        }

        if (node.DefinitionId is null
            || !_nodeCatalog.TryGetDefinition(node.DefinitionId, out var definition)
            || definition is null)
        {
            yield break;
        }

        foreach (var parameter in definition.Parameters)
        {
            yield return new GraphEditorCompatibleTargetState(
                node,
                new GraphConnectionTargetRef(node.Id, parameter.Key, GraphConnectionTargetKind.Parameter),
                parameter.DisplayName,
                parameter.ValueType,
                ResolveParameterAccent(node, parameter.ValueType),
                PortCompatibilityResult.Rejected());
        }
    }

    private static string ResolveParameterAccent(GraphNode node, PortTypeId typeId)
        => string.IsNullOrWhiteSpace(node.AccentHex)
            ? $"#{Math.Abs(typeId.Value.GetHashCode(StringComparison.Ordinal)) & 0xFFFFFF:X6}"
            : node.AccentHex;

    private static GraphNode? FindNode(GraphDocument document, string nodeId)
        => document.Nodes.FirstOrDefault(node => string.Equals(node.Id, nodeId, StringComparison.Ordinal));
}

internal sealed record GraphEditorCompatibleTargetState(
    GraphNode Node,
    GraphConnectionTargetRef Target,
    string TargetLabel,
    PortTypeId TargetTypeId,
    string TargetAccentHex,
    PortCompatibilityResult Compatibility);
