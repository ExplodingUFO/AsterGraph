using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one node definition endpoint that can complete the current pending connection.
/// </summary>
public sealed record GraphEditorCompatibleNodeDefinitionSnapshot(
    NodeDefinitionId DefinitionId,
    string DisplayName,
    string Category,
    string TargetId,
    string TargetLabel,
    GraphConnectionTargetKind TargetKind,
    PortTypeId TargetTypeId,
    PortCompatibilityResult Compatibility);
