using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorNodeSurfacePlanner
{
    internal static GraphEditorNodeSurfaceContentPlan Create(INodeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return Create(
            definition.DisplayName,
            definition.Subtitle,
            definition.Description,
            definition.DefaultWidth,
            definition.DefaultHeight,
            definition.InputPorts.Count,
            definition.OutputPorts.Count,
            definition.Parameters);
    }

    internal static GraphEditorNodeSurfaceContentPlan Create(NodeViewModel node, INodeDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(node);

        return Create(
            node.Title,
            node.Subtitle,
            node.Description,
            definition?.DefaultWidth ?? node.Width,
            definition?.DefaultHeight ?? node.Height,
            node.Inputs.Count,
            node.Outputs.Count,
            definition?.Parameters ?? []);
    }

    internal static GraphEditorNodeSurfaceContentPlan Create(GraphNode node, INodeDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(node);

        return Create(
            node.Title,
            node.Subtitle,
            node.Description,
            definition?.DefaultWidth ?? node.Size.Width,
            definition?.DefaultHeight ?? node.Size.Height,
            node.Inputs.Count,
            node.Outputs.Count,
            definition?.Parameters ?? []);
    }

    private static GraphEditorNodeSurfaceContentPlan Create(
        string title,
        string subtitle,
        string? description,
        double preferredWidth,
        double preferredHeight,
        int inputPortCount,
        int outputPortCount,
        IReadOnlyList<NodeParameterDefinition> parameters)
    {
        var requiredParameters = parameters
            .Where(parameter => parameter.IsRequired)
            .ToList();
        var optionalParameters = parameters
            .Where(parameter => !parameter.IsRequired)
            .ToList();
        var supportsInputSummaries = parameters.Count > 0;
        var supportsInputEditors = parameters.Count > 0;

        return new GraphEditorNodeSurfaceContentPlan(
            title,
            subtitle,
            description,
            preferredWidth,
            preferredHeight,
            inputPortCount,
            outputPortCount,
            requiredParameters,
            optionalParameters,
            supportsInputSummaries,
            supportsInputEditors);
    }
}
