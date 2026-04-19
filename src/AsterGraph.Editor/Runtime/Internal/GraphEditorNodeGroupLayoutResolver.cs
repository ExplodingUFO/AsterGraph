using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorNodeGroupLayoutResolver
{
    internal static readonly GraphPadding DefaultContentPadding = new(24d, 44d, 24d, 28d);

    internal static GraphEditorNodeGroupSnapshot CreateSnapshot(
        GraphNodeGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        var padding = group.ExtraPadding.ClampNonNegative();
        var contentPosition = new GraphPoint(
            group.Position.X + padding.Left,
            group.Position.Y + padding.Top);
        var contentSize = group.IsCollapsed
            ? new GraphSize(0d, 0d)
            : new GraphSize(
                Math.Max(0d, group.Size.Width - padding.Left - padding.Right),
                Math.Max(0d, group.Size.Height - padding.Top - padding.Bottom));

        return new GraphEditorNodeGroupSnapshot(
            group.Id,
            group.Title,
            group.Position,
            group.Size,
            contentPosition,
            contentSize,
            padding,
            group.NodeIds.ToList(),
            group.IsCollapsed);
    }

    internal static GraphNodeGroup ResolvePersistedBounds(
        GraphNodeGroup group)
        => group with
        {
            ExtraPadding = group.ExtraPadding.ClampNonNegative(),
            NodeIds = group.NodeIds.ToList(),
        };

    internal static GraphNodeGroup CreateInitialGroup(
        string groupId,
        string title,
        IReadOnlyList<string> nodeIds,
        IReadOnlyDictionary<string, GraphEditorNodeGroupMemberBounds> boundsByNodeId,
        GraphPadding? paddingOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(nodeIds);
        ArgumentNullException.ThrowIfNull(boundsByNodeId);

        var memberBounds = nodeIds
            .Where(boundsByNodeId.ContainsKey)
            .Select(nodeId => boundsByNodeId[nodeId])
            .ToList();
        if (memberBounds.Count == 0)
        {
            throw new InvalidOperationException("Cannot create an initial node group frame without member bounds.");
        }

        var padding = (paddingOverride ?? DefaultContentPadding).ClampNonNegative();
        var contentLeft = memberBounds.Min(bounds => bounds.Position.X);
        var contentTop = memberBounds.Min(bounds => bounds.Position.Y);
        var contentRight = memberBounds.Max(bounds => bounds.Position.X + bounds.Size.Width);
        var contentBottom = memberBounds.Max(bounds => bounds.Position.Y + bounds.Size.Height);

        return new GraphNodeGroup(
            groupId,
            title.Trim(),
            new GraphPoint(contentLeft - padding.Left, contentTop - padding.Top),
            new GraphSize(
                (contentRight - contentLeft) + padding.Left + padding.Right,
                (contentBottom - contentTop) + padding.Top + padding.Bottom),
            nodeIds.ToList(),
            ExtraPadding: padding);
    }
}

internal readonly record struct GraphEditorNodeGroupMemberBounds(
    GraphPoint Position,
    GraphSize Size);
