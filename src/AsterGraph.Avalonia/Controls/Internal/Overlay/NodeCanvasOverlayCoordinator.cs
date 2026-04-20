using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasOverlayHost
{
    GraphEditorStyleOptions StyleOptions { get; }

    GraphEditorBehaviorOptions BehaviorOptions { get; }

    double Zoom { get; }

    IReadOnlyList<NodeViewModel> Nodes { get; }

    IReadOnlyList<GraphEditorNodeGroupSnapshot> GroupSnapshots { get; }

    IReadOnlyList<NodeViewModel> SelectedNodes { get; }

    NodeViewModel? SelectedNode { get; }

    Size Bounds { get; }

    Border? SelectionAdorner { get; }

    Border? VerticalGuideAdorner { get; }

    Border? HorizontalGuideAdorner { get; }

    GraphPoint WorldToScreen(double x, double y);

    GraphPoint ScreenToWorld(GraphPoint point);

    IReadOnlyList<NodeViewModel> GetNodesInRectangle(GraphPoint firstCorner, GraphPoint secondCorner);

    NodeCanvasInteractionSession InteractionSession { get; }

    void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode, string? status = null);
}

internal sealed class NodeCanvasOverlayCoordinator
{
    private readonly INodeCanvasOverlayHost _host;

    public NodeCanvasOverlayCoordinator(INodeCanvasOverlayHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void ApplySelectionAdornerStyle()
    {
        if (_host.SelectionAdorner is null)
        {
            return;
        }

        var style = _host.StyleOptions.Canvas;
        _host.SelectionAdorner.BorderBrush = BrushFactory.Solid(style.SelectionBorderHex);
        _host.SelectionAdorner.Background = BrushFactory.Solid(style.SelectionFillHex, style.SelectionFillOpacity);
        _host.SelectionAdorner.BorderThickness = new Thickness(style.SelectionBorderThickness);
        _host.SelectionAdorner.CornerRadius = new CornerRadius(style.SelectionCornerRadius);
    }

    public void ApplyGuideAdornerStyle()
    {
        var style = _host.StyleOptions.Canvas;
        if (_host.VerticalGuideAdorner is not null)
        {
            _host.VerticalGuideAdorner.Background = BrushFactory.Solid(style.GuideHex, style.GuideOpacity);
            _host.VerticalGuideAdorner.Width = style.GuideThickness;
        }

        if (_host.HorizontalGuideAdorner is not null)
        {
            _host.HorizontalGuideAdorner.Background = BrushFactory.Solid(style.GuideHex, style.GuideOpacity);
            _host.HorizontalGuideAdorner.Height = style.GuideThickness;
        }
    }

    public void HideSelectionAdorner()
    {
        if (_host.SelectionAdorner is null)
        {
            return;
        }

        _host.SelectionAdorner.IsVisible = false;
        _host.SelectionAdorner.Width = 0;
        _host.SelectionAdorner.Height = 0;
    }

    public void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize)
    {
        if (_host.SelectionAdorner is null
            || _host.InteractionSession.SelectionStartScreenPosition is null)
        {
            return;
        }

        var start = _host.InteractionSession.SelectionStartScreenPosition.Value;
        var left = Math.Min(start.X, currentScreenPosition.X);
        var top = Math.Min(start.Y, currentScreenPosition.Y);
        var width = Math.Abs(currentScreenPosition.X - start.X);
        var height = Math.Abs(currentScreenPosition.Y - start.Y);

        _host.SelectionAdorner.IsVisible = true;
        _host.SelectionAdorner.Width = width;
        _host.SelectionAdorner.Height = height;
        Canvas.SetLeft(_host.SelectionAdorner, left);
        Canvas.SetTop(_host.SelectionAdorner, top);

        var worldStart = _host.ScreenToWorld(new GraphPoint(start.X, start.Y));
        var worldEnd = _host.ScreenToWorld(new GraphPoint(currentScreenPosition.X, currentScreenPosition.Y));
        var hitNodes = _host.GetNodesInRectangle(worldStart, worldEnd).ToList();
        var nodes = ApplySelectionModifiers(hitNodes);
        var primaryNode = nodes.LastOrDefault();

        if (SelectionsMatchCurrentState(nodes, primaryNode))
        {
            return;
        }

        _host.SetSelection(
            nodes,
            primaryNode,
            finalize
                ? nodes.Count switch
                {
                    0 => "No nodes inside marquee selection.",
                    1 => $"Selected {nodes[0].Title}.",
                    _ => $"Selected {nodes.Count} nodes.",
                }
                : null);
    }

    public GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY)
    {
        var style = _host.StyleOptions.Canvas;
        var behavior = _host.BehaviorOptions.DragAssist;
        HideGuideAdorners();

        if (!behavior.EnableGridSnapping && !behavior.EnableAlignmentGuides)
        {
            return new GraphPoint(deltaX, deltaY);
        }

        var tolerance = behavior.SnapTolerance / Math.Max(_host.Zoom, 0.001);
        var result = NodeCanvasDragAssistCalculator.Calculate(
            dragSession.OriginBounds,
            deltaX,
            deltaY,
            ResolveDragCandidateBounds(dragSession),
            behavior.EnableGridSnapping,
            behavior.EnableAlignmentGuides,
            style.PrimaryGridSpacing,
            tolerance);

        ShowGuideAdorners(result.GuideWorldX, result.GuideWorldY);
        return result.AdjustedDelta;
    }

    public NodeCanvasGroupResizePreview ApplyGroupResizeAssist(
        GraphEditorNodeGroupSnapshot group,
        NodeCanvasGroupResizeEdge edge,
        GraphPoint proposedPosition,
        GraphSize proposedSize,
        GraphSize minimumSize)
    {
        var style = _host.StyleOptions.Canvas;
        var behavior = _host.BehaviorOptions.DragAssist;
        HideGuideAdorners();

        if (!behavior.EnableGridSnapping && !behavior.EnableAlignmentGuides)
        {
            return new NodeCanvasGroupResizePreview(group.Id, proposedPosition, proposedSize);
        }

        var tolerance = behavior.SnapTolerance / Math.Max(_host.Zoom, 0.001);
        var originBounds = new NodeBounds(group.Position.X, group.Position.Y, group.Size.Width, group.Size.Height);
        var proposedBounds = new NodeBounds(proposedPosition.X, proposedPosition.Y, proposedSize.Width, proposedSize.Height);
        var result = NodeCanvasDragAssistCalculator.CalculateBounds(
            originBounds,
            proposedBounds,
            ResolveHorizontalMode(edge),
            ResolveVerticalMode(edge),
            ResolveGroupCandidateBounds(group),
            behavior.EnableGridSnapping,
            behavior.EnableAlignmentGuides,
            style.PrimaryGridSpacing,
            tolerance);
        var adjustedBounds = EnforceMinimumBounds(result.AdjustedBounds, edge, minimumSize);

        ShowGuideAdorners(result.GuideWorldX, result.GuideWorldY);
        return new NodeCanvasGroupResizePreview(
            group.Id,
            new GraphPoint(adjustedBounds.X, adjustedBounds.Y),
            new GraphSize(adjustedBounds.Width, adjustedBounds.Height));
    }

    public NodeCanvasDragSession CreateDragSession(IReadOnlyList<NodeViewModel> nodes)
    {
        var originPositions = nodes.ToDictionary(
            node => node.Id,
            node => new GraphPoint(node.X, node.Y),
            StringComparer.Ordinal);

        return new NodeCanvasDragSession(nodes, originPositions, GetSelectionBounds(nodes));
    }

    public void ShowGuideAdorners(double? worldX, double? worldY)
    {
        if (_host.VerticalGuideAdorner is not null)
        {
            if (worldX is double x)
            {
                var screenX = _host.WorldToScreen(x, 0).X;
                _host.VerticalGuideAdorner.IsVisible = true;
                _host.VerticalGuideAdorner.Height = _host.Bounds.Height;
                Canvas.SetLeft(_host.VerticalGuideAdorner, screenX - (_host.VerticalGuideAdorner.Width / 2));
                Canvas.SetTop(_host.VerticalGuideAdorner, 0);
            }
            else
            {
                _host.VerticalGuideAdorner.IsVisible = false;
            }
        }

        if (_host.HorizontalGuideAdorner is not null)
        {
            if (worldY is double y)
            {
                var screenY = _host.WorldToScreen(0, y).Y;
                _host.HorizontalGuideAdorner.IsVisible = true;
                _host.HorizontalGuideAdorner.Width = _host.Bounds.Width;
                Canvas.SetLeft(_host.HorizontalGuideAdorner, 0);
                Canvas.SetTop(_host.HorizontalGuideAdorner, screenY - (_host.HorizontalGuideAdorner.Height / 2));
            }
            else
            {
                _host.HorizontalGuideAdorner.IsVisible = false;
            }
        }
    }

    public void HideGuideAdorners()
    {
        if (_host.VerticalGuideAdorner is not null)
        {
            _host.VerticalGuideAdorner.IsVisible = false;
        }

        if (_host.HorizontalGuideAdorner is not null)
        {
            _host.HorizontalGuideAdorner.IsVisible = false;
        }
    }

    private IReadOnlyList<NodeViewModel> ApplySelectionModifiers(IReadOnlyList<NodeViewModel> hitNodes)
    {
        if (_host.InteractionSession.SelectionModifiers.HasFlag(KeyModifiers.Control))
        {
            var toggled = _host.InteractionSession.SelectionBaselineNodes.ToList();
            foreach (var node in hitNodes)
            {
                if (!toggled.Remove(node))
                {
                    toggled.Add(node);
                }
            }

            return toggled;
        }

        if (_host.InteractionSession.SelectionModifiers.HasFlag(KeyModifiers.Shift))
        {
            return _host.InteractionSession.SelectionBaselineNodes
                .Concat(hitNodes)
                .Distinct()
                .ToList();
        }

        return hitNodes;
    }

    private bool SelectionsMatchCurrentState(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode)
    {
        if (!ReferenceEquals(_host.SelectedNode, primaryNode))
        {
            return false;
        }

        if (_host.SelectedNodes.Count != nodes.Count)
        {
            return false;
        }

        for (var index = 0; index < nodes.Count; index++)
        {
            if (!ReferenceEquals(_host.SelectedNodes[index], nodes[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static NodeBounds GetSelectionBounds(IReadOnlyList<NodeViewModel> nodes)
    {
        var left = nodes.Min(node => node.X);
        var top = nodes.Min(node => node.Y);
        var right = nodes.Max(node => node.X + node.Width);
        var bottom = nodes.Max(node => node.Y + node.Height);
        return new NodeBounds(left, top, right - left, bottom - top);
    }

    private IEnumerable<NodeBounds> ResolveDragCandidateBounds(NodeCanvasDragSession dragSession)
    {
        var movingNodeIds = dragSession.Nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        return _host.Nodes
            .Where(node => !movingNodeIds.Contains(node.Id))
            .Select(node => new NodeBounds(node.X, node.Y, node.Width, node.Height));
    }

    private IEnumerable<NodeBounds> ResolveGroupCandidateBounds(GraphEditorNodeGroupSnapshot group)
    {
        var memberIds = group.NodeIds.ToHashSet(StringComparer.Ordinal);
        foreach (var node in _host.Nodes)
        {
            if (memberIds.Contains(node.Id))
            {
                continue;
            }

            yield return new NodeBounds(node.X, node.Y, node.Width, node.Height);
        }

        foreach (var candidateGroup in _host.GroupSnapshots)
        {
            if (string.Equals(candidateGroup.Id, group.Id, StringComparison.Ordinal))
            {
                continue;
            }

            yield return new NodeBounds(
                candidateGroup.Position.X,
                candidateGroup.Position.Y,
                candidateGroup.Size.Width,
                candidateGroup.Size.Height);
        }
    }

    private static NodeCanvasPlacementAxisMode ResolveHorizontalMode(NodeCanvasGroupResizeEdge edge)
        => edge switch
        {
            NodeCanvasGroupResizeEdge.Left => NodeCanvasPlacementAxisMode.StartEdge,
            NodeCanvasGroupResizeEdge.Right => NodeCanvasPlacementAxisMode.EndEdge,
            _ => NodeCanvasPlacementAxisMode.None,
        };

    private static NodeCanvasPlacementAxisMode ResolveVerticalMode(NodeCanvasGroupResizeEdge edge)
        => edge switch
        {
            NodeCanvasGroupResizeEdge.Top => NodeCanvasPlacementAxisMode.StartEdge,
            NodeCanvasGroupResizeEdge.Bottom => NodeCanvasPlacementAxisMode.EndEdge,
            _ => NodeCanvasPlacementAxisMode.None,
        };

    private static NodeBounds EnforceMinimumBounds(NodeBounds bounds, NodeCanvasGroupResizeEdge edge, GraphSize minimumSize)
    {
        var x = bounds.X;
        var y = bounds.Y;
        var width = bounds.Width;
        var height = bounds.Height;

        switch (edge)
        {
            case NodeCanvasGroupResizeEdge.Left:
            {
                var right = bounds.X + bounds.Width;
                width = Math.Max(minimumSize.Width, bounds.Width);
                x = right - width;
                break;
            }

            case NodeCanvasGroupResizeEdge.Top:
            {
                var bottom = bounds.Y + bounds.Height;
                height = Math.Max(minimumSize.Height, bounds.Height);
                y = bottom - height;
                break;
            }

            case NodeCanvasGroupResizeEdge.Right:
                width = Math.Max(minimumSize.Width, bounds.Width);
                break;

            case NodeCanvasGroupResizeEdge.Bottom:
                height = Math.Max(minimumSize.Height, bounds.Height);
                break;
        }

        return new NodeBounds(x, y, width, height);
    }
}
