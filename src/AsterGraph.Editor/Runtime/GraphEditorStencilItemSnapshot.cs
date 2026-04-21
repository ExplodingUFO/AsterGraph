using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime.Internal;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing snapshot of one insertable stencil item projected from the active node catalog.
/// </summary>
public sealed record GraphEditorStencilItemSnapshot(
    NodeDefinitionId DefinitionId,
    string Key,
    string Title,
    string Category,
    string Subtitle,
    string Description,
    GraphSize Size,
    string AccentHex,
    int InputCount,
    int OutputCount)
{
    /// <summary>
    /// Human-readable summary of input/output counts.
    /// </summary>
    public string PortSummary => $"{InputCount} in  ·  {OutputCount} out";

    /// <summary>
    /// Projects one node definition into a canonical stencil item snapshot.
    /// </summary>
    public static GraphEditorStencilItemSnapshot Create(INodeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var contentPlan = GraphEditorNodeSurfacePlanner.Create(definition);
        var size = GraphEditorNodeSurfaceMetrics.NormalizePersistedSize(
            new GraphSize(definition.DefaultWidth, definition.DefaultHeight),
            GraphEditorNodeSurfaceMeasurer.Measure(contentPlan));

        return new GraphEditorStencilItemSnapshot(
            definition.Id,
            definition.Id.Value.Replace(".", "-", StringComparison.Ordinal),
            definition.DisplayName,
            definition.Category,
            definition.Subtitle,
            definition.Description ?? string.Empty,
            size,
            definition.AccentHex,
            definition.InputPorts.Count,
            definition.OutputPorts.Count);
    }
}
