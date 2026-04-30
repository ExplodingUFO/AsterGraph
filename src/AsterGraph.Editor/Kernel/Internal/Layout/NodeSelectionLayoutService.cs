using AsterGraph.Core.Models;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Kernel.Internal.Layout;

internal static class NodeSelectionLayoutService
{
    public static IReadOnlyList<NodePositionSnapshot> Apply(
        GraphSelectionLayoutOperation operation,
        IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        return operation switch
        {
            GraphSelectionLayoutOperation.AlignLeft => AlignLeft(nodes),
            GraphSelectionLayoutOperation.AlignCenter => AlignCenter(nodes),
            GraphSelectionLayoutOperation.AlignRight => AlignRight(nodes),
            GraphSelectionLayoutOperation.AlignTop => AlignTop(nodes),
            GraphSelectionLayoutOperation.AlignMiddle => AlignMiddle(nodes),
            GraphSelectionLayoutOperation.AlignBottom => AlignBottom(nodes),
            GraphSelectionLayoutOperation.DistributeHorizontally => DistributeHorizontally(nodes),
            GraphSelectionLayoutOperation.DistributeVertically => DistributeVertically(nodes),
            _ => SnapshotPositions(nodes),
        };
    }

    private static IReadOnlyList<NodePositionSnapshot> AlignLeft(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 2)
        {
            return SnapshotPositions(nodes);
        }

        var left = nodes.Min(node => node.Position.X);
        return nodes
            .Select(node => new NodePositionSnapshot(node.NodeId, new GraphPoint(left, node.Position.Y)))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> AlignCenter(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 2)
        {
            return SnapshotPositions(nodes);
        }

        var left = nodes.Min(node => node.Position.X);
        var right = nodes.Max(node => node.Position.X + node.Size.Width);
        var center = (left + right) / 2d;
        return nodes
            .Select(node => new NodePositionSnapshot(
                node.NodeId,
                new GraphPoint(center - (node.Size.Width / 2d), node.Position.Y)))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> AlignRight(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 2)
        {
            return SnapshotPositions(nodes);
        }

        var right = nodes.Max(node => node.Position.X + node.Size.Width);
        return nodes
            .Select(node => new NodePositionSnapshot(
                node.NodeId,
                new GraphPoint(right - node.Size.Width, node.Position.Y)))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> AlignTop(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 2)
        {
            return SnapshotPositions(nodes);
        }

        var top = nodes.Min(node => node.Position.Y);
        return nodes
            .Select(node => new NodePositionSnapshot(node.NodeId, new GraphPoint(node.Position.X, top)))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> AlignMiddle(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 2)
        {
            return SnapshotPositions(nodes);
        }

        var top = nodes.Min(node => node.Position.Y);
        var bottom = nodes.Max(node => node.Position.Y + node.Size.Height);
        var middle = (top + bottom) / 2d;
        return nodes
            .Select(node => new NodePositionSnapshot(
                node.NodeId,
                new GraphPoint(node.Position.X, middle - (node.Size.Height / 2d))))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> AlignBottom(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 2)
        {
            return SnapshotPositions(nodes);
        }

        var bottom = nodes.Max(node => node.Position.Y + node.Size.Height);
        return nodes
            .Select(node => new NodePositionSnapshot(
                node.NodeId,
                new GraphPoint(node.Position.X, bottom - node.Size.Height)))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> DistributeHorizontally(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 3)
        {
            return SnapshotPositions(nodes);
        }

        var ordered = nodes
            .OrderBy(node => node.Position.X + (node.Size.Width / 2d))
            .ToList();
        var firstCenter = ordered[0].Position.X + (ordered[0].Size.Width / 2d);
        var lastCenter = ordered[^1].Position.X + (ordered[^1].Size.Width / 2d);
        var step = (lastCenter - firstCenter) / (ordered.Count - 1);
        var positions = ordered.ToDictionary(node => node.NodeId, node => node.Position, StringComparer.Ordinal);

        for (var index = 1; index < ordered.Count - 1; index++)
        {
            var node = ordered[index];
            var center = firstCenter + (step * index);
            positions[node.NodeId] = new GraphPoint(center - (node.Size.Width / 2d), node.Position.Y);
        }

        return nodes
            .Select(node => new NodePositionSnapshot(node.NodeId, positions[node.NodeId]))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> DistributeVertically(IReadOnlyList<NodeSelectionLayoutInput> nodes)
    {
        if (nodes.Count < 3)
        {
            return SnapshotPositions(nodes);
        }

        var ordered = nodes
            .OrderBy(node => node.Position.Y + (node.Size.Height / 2d))
            .ToList();
        var firstCenter = ordered[0].Position.Y + (ordered[0].Size.Height / 2d);
        var lastCenter = ordered[^1].Position.Y + (ordered[^1].Size.Height / 2d);
        var step = (lastCenter - firstCenter) / (ordered.Count - 1);
        var positions = ordered.ToDictionary(node => node.NodeId, node => node.Position, StringComparer.Ordinal);

        for (var index = 1; index < ordered.Count - 1; index++)
        {
            var node = ordered[index];
            var center = firstCenter + (step * index);
            positions[node.NodeId] = new GraphPoint(node.Position.X, center - (node.Size.Height / 2d));
        }

        return nodes
            .Select(node => new NodePositionSnapshot(node.NodeId, positions[node.NodeId]))
            .ToList();
    }

    private static IReadOnlyList<NodePositionSnapshot> SnapshotPositions(IReadOnlyList<NodeSelectionLayoutInput> nodes)
        => nodes
            .Select(node => new NodePositionSnapshot(node.NodeId, node.Position))
            .ToList();
}
