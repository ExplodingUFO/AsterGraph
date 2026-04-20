using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable runtime snapshot for one compatible connection target endpoint.
/// </summary>
public sealed record GraphEditorCompatibleConnectionTargetSnapshot
{
    /// <summary>
    /// Initializes a compatible connection target snapshot.
    /// </summary>
    public GraphEditorCompatibleConnectionTargetSnapshot(
        string nodeId,
        string nodeTitle,
        string targetId,
        string targetLabel,
        GraphConnectionTargetKind targetKind,
        PortTypeId targetTypeId,
        string targetAccentHex,
        PortCompatibilityResult compatibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        ArgumentNullException.ThrowIfNull(targetTypeId);

        NodeId = nodeId;
        NodeTitle = string.IsNullOrWhiteSpace(nodeTitle) ? nodeId : nodeTitle;
        TargetId = targetId;
        TargetLabel = string.IsNullOrWhiteSpace(targetLabel) ? targetId : targetLabel;
        TargetKind = targetKind;
        TargetTypeId = targetTypeId;
        TargetAccentHex = targetAccentHex ?? string.Empty;
        Compatibility = compatibility;
    }

    public string NodeId { get; }

    public string NodeTitle { get; }

    public string TargetId { get; }

    public string TargetLabel { get; }

    public GraphConnectionTargetKind TargetKind { get; }

    public PortTypeId TargetTypeId { get; }

    public string TargetAccentHex { get; }

    public PortCompatibilityResult Compatibility { get; }

    /// <summary>
    /// Convenience typed target reference.
    /// </summary>
    public GraphConnectionTargetRef Target => new(NodeId, TargetId, TargetKind);
}
