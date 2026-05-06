using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorNodeLayoutCoordinatorHost
{
    GraphEditorCommandPermissions CommandPermissions { get; }

    IReadOnlyList<NodeViewModel> Nodes { get; }

    IReadOnlyList<NodeViewModel> SelectedNodes { get; }

    NodeViewModel? FindNode(string nodeId);

    void SetNodeMoveDisabledStatus();

    void SetNodeNotFoundStatus(string nodeId);

    void SetNodePositionNoneProvidedStatus();

    void SetNodePositionNoMatchesStatus();

    void ApplyNodePositionUpdates(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus);
}

internal sealed class GraphEditorNodeLayoutCoordinator
{
    private readonly IGraphEditorNodeLayoutCoordinatorHost _host;

    public GraphEditorNodeLayoutCoordinator(IGraphEditorNodeLayoutCoordinatorHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void ApplyDragOffset(IReadOnlyDictionary<string, GraphPoint> originPositions, double deltaX, double deltaY)
    {
        ArgumentNullException.ThrowIfNull(originPositions);

        if (!_host.CommandPermissions.Nodes.AllowMove)
        {
            return;
        }

        foreach (var entry in originPositions)
        {
            var node = _host.FindNode(entry.Key);
            if (node is null)
            {
                continue;
            }

            node.X = entry.Value.X + deltaX;
            node.Y = entry.Value.Y + deltaY;
        }
    }

    public IReadOnlyList<NodeViewModel> GetNodesInRectangle(GraphPoint firstCorner, GraphPoint secondCorner)
    {
        var left = Math.Min(firstCorner.X, secondCorner.X);
        var top = Math.Min(firstCorner.Y, secondCorner.Y);
        var right = Math.Max(firstCorner.X, secondCorner.X);
        var bottom = Math.Max(firstCorner.Y, secondCorner.Y);

        return _host.Nodes
            .Where(node => Intersects(node.Bounds, left, top, right, bottom))
            .ToList();
    }

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _host.Nodes
            .Select(node => new NodePositionSnapshot(node.Id, new GraphPoint(node.X, node.Y)))
            .ToList();

    public bool TryGetNodePosition(string nodeId, out NodePositionSnapshot? snapshot)
    {
        var node = _host.FindNode(nodeId);
        if (node is null)
        {
            snapshot = null;
            return false;
        }

        snapshot = new NodePositionSnapshot(node.Id, new GraphPoint(node.X, node.Y));
        return true;
    }

    public bool TrySetNodePosition(string nodeId, GraphPoint position, bool updateStatus = true)
    {
        if (!_host.CommandPermissions.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                _host.SetNodeMoveDisabledStatus();
            }

            return false;
        }

        var node = _host.FindNode(nodeId);
        if (node is null)
        {
            if (updateStatus)
            {
                _host.SetNodeNotFoundStatus(nodeId);
            }

            return false;
        }

        if (new GraphPoint(node.X, node.Y) == position)
        {
            return true;
        }

        _host.ApplyNodePositionUpdates([new NodePositionSnapshot(nodeId, position)], updateStatus);
        return true;
    }

    public int SetNodePositions(IEnumerable<NodePositionSnapshot> positions, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(positions);

        if (!_host.CommandPermissions.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                _host.SetNodeMoveDisabledStatus();
            }

            return 0;
        }

        var requestedPositions = positions
            .GroupBy(snapshot => snapshot.NodeId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToList();

        if (requestedPositions.Count == 0)
        {
            if (updateStatus)
            {
                _host.SetNodePositionNoneProvidedStatus();
            }

            return 0;
        }

        var appliedCount = requestedPositions.Count(snapshot =>
        {
            var node = _host.FindNode(snapshot.NodeId);
            return node is not null && new GraphPoint(node.X, node.Y) != snapshot.Position;
        });

        if (appliedCount == 0)
        {
            if (updateStatus)
            {
                _host.SetNodePositionNoMatchesStatus();
            }

            return 0;
        }

        _host.ApplyNodePositionUpdates(requestedPositions, updateStatus);
        return appliedCount;
    }

    public void MoveNode(NodeViewModel node, double deltaX, double deltaY)
    {
        if (!_host.CommandPermissions.Nodes.AllowMove)
        {
            return;
        }

        if (node.IsSelected && _host.SelectedNodes.Count > 1)
        {
            foreach (var selectedNode in _host.SelectedNodes)
            {
                selectedNode.MoveBy(deltaX, deltaY);
            }

            return;
        }

        node.MoveBy(deltaX, deltaY);
    }

    private static bool Intersects(NodeBounds bounds, double left, double top, double right, double bottom)
        => bounds.X <= right
           && (bounds.X + bounds.Width) >= left
           && bounds.Y <= bottom
           && (bounds.Y + bounds.Height) >= top;
}
