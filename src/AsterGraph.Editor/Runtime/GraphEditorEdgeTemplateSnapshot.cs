using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable runtime snapshot for one edge template projected from a source endpoint and one compatible target.
/// </summary>
public sealed record GraphEditorEdgeTemplateSnapshot
{
    /// <summary>
    /// Initializes one edge template snapshot.
    /// </summary>
    public GraphEditorEdgeTemplateSnapshot(
        string targetNodeId,
        string targetNodeTitle,
        string targetId,
        string targetLabel,
        GraphConnectionTargetKind targetKind,
        PortTypeId sourceTypeId,
        PortTypeId targetTypeId,
        string accentHex,
        string defaultLabel,
        PortCompatibilityResult compatibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        ArgumentNullException.ThrowIfNull(sourceTypeId);
        ArgumentNullException.ThrowIfNull(targetTypeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultLabel);

        TargetNodeId = targetNodeId;
        TargetNodeTitle = string.IsNullOrWhiteSpace(targetNodeTitle) ? targetNodeId : targetNodeTitle;
        TargetId = targetId;
        TargetLabel = string.IsNullOrWhiteSpace(targetLabel) ? targetId : targetLabel;
        TargetKind = targetKind;
        SourceTypeId = sourceTypeId;
        TargetTypeId = targetTypeId;
        AccentHex = accentHex ?? string.Empty;
        DefaultLabel = defaultLabel;
        Compatibility = compatibility;
    }

    public string TargetNodeId { get; }

    public string TargetNodeTitle { get; }

    public string TargetId { get; }

    public string TargetLabel { get; }

    public GraphConnectionTargetKind TargetKind { get; }

    public PortTypeId SourceTypeId { get; }

    public PortTypeId TargetTypeId { get; }

    public string AccentHex { get; }

    public string DefaultLabel { get; }

    public PortCompatibilityResult Compatibility { get; }

    /// <summary>
    /// Convenience typed target reference.
    /// </summary>
    public GraphConnectionTargetRef Target => new(TargetNodeId, TargetId, TargetKind);
}
