using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorNodeSurfaceTierResolver
{
    internal static NodeSurfaceTierProfile ResolveProfile(
        GraphEditorBehaviorOptions behaviorOptions,
        INodeDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(behaviorOptions);
        return definition?.SurfaceTierProfile ?? behaviorOptions.View.NodeSurfaceTierProfile;
    }

    internal static INodeDefinition? ResolveDefinition(
        AsterGraph.Abstractions.Catalog.INodeCatalog nodeCatalog,
        NodeDefinitionId? definitionId)
    {
        ArgumentNullException.ThrowIfNull(nodeCatalog);
        if (definitionId is null)
        {
            return null;
        }

        return nodeCatalog.TryGetDefinition(definitionId, out var definition)
            ? definition
            : null;
    }

    internal static GraphEditorNodeSurfaceTierSnapshot ResolveActiveTier(
        GraphSize size,
        NodeSurfaceTierProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var activeTier = profile.Tiers
            .LastOrDefault(tier => size.Width >= tier.MinWidth && size.Height >= tier.MinHeight)
            ?? profile.Tiers[0];

        return new GraphEditorNodeSurfaceTierSnapshot(
            activeTier.Key,
            activeTier.MinWidth,
            activeTier.MinHeight,
            activeTier.VisibleSectionKeys.ToList());
    }

    internal static GraphEditorNodeSurfaceTierSnapshot ResolveActiveTier(
        GraphSize size,
        GraphEditorBehaviorOptions behaviorOptions,
        INodeDefinition? definition)
        => ResolveActiveTier(size, ResolveProfile(behaviorOptions, definition));
}
