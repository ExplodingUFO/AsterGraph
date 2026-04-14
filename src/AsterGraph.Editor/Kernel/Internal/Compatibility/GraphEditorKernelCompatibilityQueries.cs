using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Kernel.Internal;

internal sealed class GraphEditorKernelCompatibilityQueries
{
    private readonly IPortCompatibilityService _compatibilityService;

    public GraphEditorKernelCompatibilityQueries(IPortCompatibilityService compatibilityService)
    {
        _compatibilityService = compatibilityService ?? throw new ArgumentNullException(nameof(compatibilityService));
    }

    public IReadOnlyList<GraphEditorCompatibleTargetState> GetCompatibleTargetStates(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId)
    {
        ArgumentNullException.ThrowIfNull(document);

        var sourceNode = FindNode(document, sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null)
        {
            return [];
        }

        return document.Nodes
            .SelectMany(node => node.Inputs.Select(port => (node, port)))
            .Where(target => !(target.node.Id == sourceNodeId && target.port.Id == sourcePortId))
            .Where(target => sourcePort.TypeId is not null && target.port.TypeId is not null)
            .Select(target => new GraphEditorCompatibleTargetState(
                target.node,
                target.port,
                _compatibilityService.Evaluate(sourcePort.TypeId!, target.port.TypeId!)))
            .Where(target => target.Compatibility.IsCompatible)
            .ToList();
    }

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId)
        => GetCompatibleTargetStates(document, sourceNodeId, sourcePortId)
            .Select(target => new GraphEditorCompatiblePortTargetSnapshot(
                target.Node.Id,
                target.Node.Title,
                target.Port.Id,
                target.Port.Label,
                target.Port.TypeId!,
                target.Port.AccentHex,
                target.Compatibility))
            .ToList();

#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(
        GraphDocument document,
        string sourceNodeId,
        string sourcePortId)
        => GetCompatibleTargetStates(document, sourceNodeId, sourcePortId)
            .Select(target =>
            {
                var node = new NodeViewModel(target.Node);
                var port = node.GetPort(target.Port.Id)
                    ?? throw new InvalidOperationException($"Port '{target.Port.Id}' was not found on compatibility bridge node '{target.Node.Id}'.");
                return new CompatiblePortTarget(node, port, target.Compatibility);
            })
            .ToList();
#pragma warning restore CS0618

    private static GraphNode? FindNode(GraphDocument document, string nodeId)
        => document.Nodes.FirstOrDefault(node => string.Equals(node.Id, nodeId, StringComparison.Ordinal));
}

internal sealed record GraphEditorCompatibleTargetState(
    GraphNode Node,
    GraphPort Port,
    PortCompatibilityResult Compatibility);
