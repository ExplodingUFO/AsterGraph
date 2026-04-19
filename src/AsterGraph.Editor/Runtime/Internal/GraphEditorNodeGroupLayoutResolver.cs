using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorNodeGroupLayoutResolver
{
    internal static GraphEditorNodeGroupSnapshot CreateSnapshot(
        GraphNodeGroup group,
        IReadOnlyDictionary<string, GraphEditorNodeGroupMemberBounds> boundsByNodeId)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(boundsByNodeId);

        var memberBounds = group.NodeIds
            .Where(boundsByNodeId.ContainsKey)
            .Select(nodeId => boundsByNodeId[nodeId])
            .ToList();
        var padding = group.ExtraPadding.ClampNonNegative();

        if (memberBounds.Count == 0)
        {
            return new GraphEditorNodeGroupSnapshot(
                group.Id,
                group.Title,
                group.Position,
                group.Size,
                group.Position,
                group.Size,
                padding,
                group.NodeIds.ToList(),
                group.IsCollapsed);
        }

        var contentLeft = memberBounds.Min(bounds => bounds.Position.X);
        var contentTop = memberBounds.Min(bounds => bounds.Position.Y);
        var contentRight = memberBounds.Max(bounds => bounds.Position.X + bounds.Size.Width);
        var contentBottom = memberBounds.Max(bounds => bounds.Position.Y + bounds.Size.Height);

        var position = new GraphPoint(contentLeft - padding.Left, contentTop - padding.Top);
        var size = new GraphSize(
            (contentRight - contentLeft) + padding.Left + padding.Right,
            (contentBottom - contentTop) + padding.Top + padding.Bottom);

        return new GraphEditorNodeGroupSnapshot(
            group.Id,
            group.Title,
            position,
            size,
            new GraphPoint(contentLeft, contentTop),
            new GraphSize(contentRight - contentLeft, contentBottom - contentTop),
            padding,
            group.NodeIds.ToList(),
            group.IsCollapsed);
    }

    internal static GraphNodeGroup ResolvePersistedBounds(
        GraphNodeGroup group,
        IReadOnlyDictionary<string, GraphEditorNodeGroupMemberBounds> boundsByNodeId)
    {
        var snapshot = CreateSnapshot(group, boundsByNodeId);
        return group with
        {
            Position = snapshot.Position,
            Size = snapshot.Size,
            ExtraPadding = snapshot.ExtraPadding,
            NodeIds = snapshot.NodeIds.ToList(),
        };
    }
}

internal readonly record struct GraphEditorNodeGroupMemberBounds(
    GraphPoint Position,
    GraphSize Size);
