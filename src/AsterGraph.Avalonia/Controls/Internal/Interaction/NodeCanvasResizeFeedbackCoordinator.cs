using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasResizeFeedbackHost
{
    GraphEditorViewModel? ViewModel { get; }

    Control Root { get; }

    Canvas? NodeLayer { get; }

    Canvas? GroupLayer { get; }

    IReadOnlyDictionary<Control, NodeViewModel> ResizeFeedbackNodeSurfaces { get; }

    IReadOnlyDictionary<Border, string> ResizeFeedbackGroupSurfaces { get; }

    IReadOnlyDictionary<string, GraphEditorNodeGroupSnapshot> ResizeFeedbackGroupSnapshots { get; }

    IGraphResizeFeedbackPolicy? ResizeFeedbackPolicy { get; }
}

internal sealed class NodeCanvasResizeFeedbackCoordinator
{
    private readonly INodeCanvasResizeFeedbackHost _host;
    private Control? _activeHoverSurface;
    private Cursor? _activeHoverSurfaceOriginalCursor;
    private GraphResizeFeedbackContext? _activeHoverContext;
    private Cursor? _activeSessionOriginalCursor;
    private GraphResizeFeedbackContext? _activeSessionContext;

    public NodeCanvasResizeFeedbackCoordinator(INodeCanvasResizeFeedbackHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void Update(Point currentScreenPosition)
    {
        if (_activeSessionContext is not null)
        {
            return;
        }

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

    public void BeginResizeSession(GraphResizeFeedbackContext context)
    {
        ClearHover();
        if (_activeSessionContext == context)
        {
            return;
        }

        ClearSession();
        _activeSessionOriginalCursor = _host.Root.Cursor;
        _activeSessionContext = context;
        _host.Root.Cursor = ResolveCursor(context);
    }

    public void Clear()
    {
        ClearHover();
        ClearSession();
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
            if (!_host.ResizeFeedbackNodeSurfaces.TryGetValue(surface, out var node))
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

        foreach (var surface in _host.GroupLayer.Children.OfType<Border>().Reverse())
        {
            if (!_host.ResizeFeedbackGroupSurfaces.TryGetValue(surface, out var groupId)
                || !_host.ResizeFeedbackGroupSnapshots.TryGetValue(groupId, out var snapshot))
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
        if (ReferenceEquals(_activeHoverSurface, hit.Surface) && _activeHoverContext == hit.Context)
        {
            return;
        }

        ClearHover();
        _activeHoverSurface = hit.Surface;
        _activeHoverSurfaceOriginalCursor = hit.Surface.Cursor;
        _activeHoverContext = hit.Context;
        hit.Surface.Cursor = ResolveCursor(hit.Context);
    }

    private void ClearHover()
    {
        if (_activeHoverSurface is not null)
        {
            _activeHoverSurface.Cursor = _activeHoverSurfaceOriginalCursor;
        }

        _activeHoverSurface = null;
        _activeHoverSurfaceOriginalCursor = null;
        _activeHoverContext = null;
    }

    private void ClearSession()
    {
        if (_activeSessionContext is not null)
        {
            _host.Root.Cursor = _activeSessionOriginalCursor;
        }

        _activeSessionOriginalCursor = null;
        _activeSessionContext = null;
    }

    private Cursor ResolveCursor(GraphResizeFeedbackContext context)
        => _host.ResizeFeedbackPolicy?.ResolveCursor(context)
            ?? GraphResizeFeedbackDefaults.ResolveCursor(context);
}
