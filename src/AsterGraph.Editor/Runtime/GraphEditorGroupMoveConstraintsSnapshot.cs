namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Explicit group-movement constraints for the active scope.
/// </summary>
/// <param name="CanMoveFrameIndependently">Whether a group frame can move without translating its member nodes.</param>
/// <param name="CanMoveFrameWithMembers">Whether a group frame can move together with its member nodes.</param>
public sealed record GraphEditorGroupMoveConstraintsSnapshot(
    bool CanMoveFrameIndependently,
    bool CanMoveFrameWithMembers);
