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

        if (!TryResolveSourcePort(document, sourceNodeId, sourcePortId, out _, out var sourcePortCandidate)
            || sourcePortCandidate?.TypeId is null)
        {
            return [];
        }

        var sourcePort = sourcePortCandidate;
        var sourceTypeId = sourcePort.TypeId;
        return document.Nodes
            .SelectMany(node => EnumerateTargets(node, sourceNodeId, sourcePortId))
            .Select(target => target with
            {
                Compatibility = _compatibilityService.Evaluate(sourceTypeId, target.TargetTypeId),
            })
            .Where(target => target.Compatibility.IsCompatible)
            .ToList();
    }

    public IReadOnlyList<GraphEditorEdgeTemplateSnapshot> GetEdgeTemplateSnapshots(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId)
    {
        if (!TryResolveSourcePort(document, sourceNodeId, sourcePortId, out _, out var sourcePortCandidate)
            || sourcePortCandidate?.TypeId is null)
        {
            return [];
        }

        var sourcePort = sourcePortCandidate;
        var sourceTypeId = sourcePort.TypeId;
        return GetCompatibleTargetStates(document, sourceNodeId, sourcePortId)
            .Select(target => new GraphEditorEdgeTemplateSnapshot(
                target.Node.Id,
                target.Node.Title,
                target.Target.TargetId,
                target.TargetLabel,
                target.Target.Kind,
                sourceTypeId,
                target.TargetTypeId,
                sourcePort.AccentHex,
                $"{sourcePort.Label} to {target.TargetLabel}",
                target.Compatibility))
            .ToList();
    }

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
                target.Compatibility,
                target.TargetGroupName,
                target.TargetMinConnections,
                target.TargetMaxConnections))
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
                port.GroupName,
                port.MinConnections,
                port.MaxConnections,
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
                null,
                0,
                int.MaxValue,
                PortCompatibilityResult.Rejected());
        }
    }

    private static string ResolveParameterAccent(GraphNode node, PortTypeId typeId)
        => string.IsNullOrWhiteSpace(node.AccentHex)
            ? $"#{Math.Abs(typeId.Value.GetHashCode(StringComparison.Ordinal)) & 0xFFFFFF:X6}"
            : node.AccentHex;

    private static bool TryResolveSourcePort(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId,
        out GraphNode? sourceNode,
        out GraphPort? sourcePort)
    {
        sourceNode = FindNode(document, sourceNodeId);
        sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        return sourceNode is not null && sourcePort is not null;
    }

    private static GraphNode? FindNode(GraphDocument document, string nodeId)
        => document.Nodes.FirstOrDefault(node => string.Equals(node.Id, nodeId, StringComparison.Ordinal));
}

internal sealed record GraphEditorCompatibleTargetState(
    GraphNode Node,
    GraphConnectionTargetRef Target,
    string TargetLabel,
    PortTypeId TargetTypeId,
    string TargetAccentHex,
    string? TargetGroupName,
    int TargetMinConnections,
    int TargetMaxConnections,
    PortCompatibilityResult Compatibility);
