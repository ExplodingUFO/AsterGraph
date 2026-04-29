using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Documentation;

public sealed record NodeDocumentationSnapshot(
    NodeDefinitionId DefinitionId,
    string DisplayName,
    string Category,
    string Subtitle,
    string? Description,
    IReadOnlyList<PortDocumentationSnapshot> Inputs,
    IReadOnlyList<PortDocumentationSnapshot> Outputs,
    IReadOnlyList<ParameterDocumentationSnapshot> Parameters)
{
    public string? HelpText => NodeDocumentationText.JoinDistinct(Description, Subtitle);
}
public sealed record PortDocumentationSnapshot(
    NodeDefinitionId DefinitionId,
    string PortId,
    string DisplayName,
    PortDirection Direction,
    PortTypeId TypeId,
    string? Description,
    string? GroupName,
    int MinConnections,
    int MaxConnections,
    string? ConnectionRuleText)
{
    public string? HelpText => NodeDocumentationText.JoinDistinct(
        Description,
        GroupName,
        TypeId.Value,
        ConnectionRuleText);
}

public sealed record ParameterDocumentationSnapshot(
    NodeDefinitionId DefinitionId,
    string ParameterKey,
    string DisplayName,
    PortTypeId ValueType,
    string? Description,
    string? HelpText,
    string? ResetDefaultGuidance,
    string? ExampleText,
    string? ValidationRuleText)
{
    public string? InspectorHelpText => NodeDocumentationText.JoinDistinct(
        HelpText,
        ResetDefaultGuidance,
        ExampleText,
        ValidationRuleText);
}

public sealed record EdgeDocumentationSnapshot(
    string ConnectionId,
    string SourceNodeTitle,
    string SourcePortLabel,
    string TargetNodeTitle,
    string TargetLabel,
    GraphConnectionTargetKind TargetKind,
    string? SourceHelpText,
    string? TargetHelpText)
{
    public string? HelpText => NodeDocumentationText.JoinDistinct(
        $"{SourceNodeTitle}.{SourcePortLabel} -> {TargetNodeTitle}.{TargetLabel}",
        SourceHelpText,
        TargetHelpText);
}

internal static class NodeDocumentationText
{
    public static string? JoinDistinct(params string?[] segments)
    {
        var normalized = segments
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .Select(segment => segment!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return normalized.Count == 0
            ? null
            : string.Join("  ·  ", normalized);
    }
}
