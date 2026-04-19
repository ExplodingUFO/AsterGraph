using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal void ApplyNodeViewProjection(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        ApplyNodeSurfaceTier(node);
        _presentationLocalizationCoordinator.ApplyNodePresentation(node);
    }

    internal void RefreshNodeSurfaceTiers()
    {
        foreach (var node in Nodes)
        {
            ApplyNodeSurfaceTier(node);
        }
    }

    internal GraphEditorNodeSurfaceTierSnapshot ResolveNodeSurfaceTier(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var definition = node.DefinitionId is not null && _nodeCatalog.TryGetDefinition(node.DefinitionId, out var resolvedDefinition)
            ? resolvedDefinition
            : null;
        return GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            new Core.Models.GraphSize(node.Width, node.Height),
            BehaviorOptions,
            definition);
    }

    private void ApplyNodeSurfaceTier(NodeViewModel node)
        => node.UpdateActiveSurfaceTier(ResolveNodeSurfaceTier(node));
}
