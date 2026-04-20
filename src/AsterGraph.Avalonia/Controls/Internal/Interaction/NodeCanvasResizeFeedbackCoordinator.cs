using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasResizeFeedbackHost
{
    GraphEditorViewModel? ViewModel { get; }

    Canvas? NodeLayer { get; }

    Canvas? GroupLayer { get; }

    IReadOnlyDictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals { get; }

    IReadOnlyDictionary<string, NodeCanvasRenderedGroupVisual> GroupVisuals { get; }

    IGraphResizeFeedbackPolicy? ResizeFeedbackPolicy { get; }
}

internal sealed class NodeCanvasResizeFeedbackCoordinator
{
    private readonly INodeCanvasResizeFeedbackHost _host;
    private Control? _activeSurface;
    private Cursor? _activeSurfaceOriginalCursor;
    private Cursor? _activeCursor;

    public NodeCanvasResizeFeedbackCoordinator(INodeCanvasResizeFeedbackHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void Update(Point currentScreenPosition)
    {
        if (_host.ViewModel is null)
        {
            Clear();
            return;
        }

        var world = _host.ViewModel.ScreenToWorld(new GraphPoint(currentScreenPosition.X, currentScreenPosition.Y));
        if (TryResolveNodeHit(world, out var hit) || TryResolveGroupHit(world, out hit))
        {
            Apply(hit);
            return;
        }

        Clear();
    }

    public void Clear()
    {
        if (_activeSurface is not null)
        {
            _activeSurface.Cursor = _activeSurfaceOriginalCursor;
        }

        _activeSurface = null;
        _activeSurfaceOriginalCursor = null;
        _activeCursor = null;
    }

    private bool TryResolveNodeHit(GraphPoint world, out NodeCanvasResizeFeedbackHit hit)
    {
        hit = default;
        if (_host.NodeLayer is null)
        {
            return false;
        }

        foreach (var surface in _host.NodeLayer.Children.OfType<Control>().Reverse())
        {
            var nodeEntry = _host.NodeVisuals.FirstOrDefault(entry => ReferenceEquals(entry.Value.Root, surface));
            var node = nodeEntry.Key;
            if (node is null)
            {
                continue;
            }

            var local = new Point(world.X - node.X, world.Y - node.Y);
            if (local.X < 0d || local.Y < 0d || local.X > surface.Bounds.Width || local.Y > surface.Bounds.Height)
            {
                continue;
            }

            if (NodeCanvasResizeFeedbackResolver.TryResolveNode(surface, local, out hit))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryResolveGroupHit(GraphPoint world, out NodeCanvasResizeFeedbackHit hit)
    {
        hit = default;
        if (_host.GroupLayer is null || _host.ViewModel is null)
        {
            return false;
        }

        foreach (var surface in _host.GroupLayer.Children.OfType<Control>().Reverse())
        {
            var groupEntry = _host.GroupVisuals.FirstOrDefault(entry => ReferenceEquals(entry.Value.Root, surface));
            if (groupEntry.Value.Root is null)
            {
                continue;
            }

            var snapshot = _host.ViewModel.GetNodeGroupSnapshots()
                .FirstOrDefault(candidate => string.Equals(candidate.Id, groupEntry.Key, StringComparison.Ordinal));
            if (snapshot is null)
            {
                continue;
            }

            var local = new Point(world.X - snapshot.Position.X, world.Y - snapshot.Position.Y);
            if (local.X < 0d || local.Y < 0d || local.X > surface.Bounds.Width || local.Y > surface.Bounds.Height)
            {
                continue;
            }

            if (NodeCanvasResizeFeedbackResolver.TryResolveGroup(surface, local, out hit))
            {
                return true;
            }
        }

        return false;
    }

    private void Apply(NodeCanvasResizeFeedbackHit hit)
    {
        var cursor = _host.ResizeFeedbackPolicy?.ResolveCursor(hit.Context)
            ?? GraphResizeFeedbackDefaults.ResolveCursor(hit.Context);

        if (ReferenceEquals(_activeSurface, hit.Surface) && Equals(_activeCursor, cursor))
        {
            return;
        }

        Clear();
        _activeSurface = hit.Surface;
        _activeSurfaceOriginalCursor = hit.Surface.Cursor;
        _activeCursor = cursor;
        hit.Surface.Cursor = cursor;
    }
}
