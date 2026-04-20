using AsterGraph.Abstractions.Definitions;

namespace AsterGraph.Editor.Runtime;

internal sealed record GraphEditorNodeSurfaceContentPlan(
    string Title,
    string Subtitle,
    string? Description,
    double PreferredWidth,
    double PreferredHeight,
    int InputPortCount,
    int OutputPortCount,
    IReadOnlyList<NodeParameterDefinition> RequiredParameters,
    IReadOnlyList<NodeParameterDefinition> OptionalParameters,
    bool SupportsParameterSummaries,
    bool SupportsInlineEditors);
