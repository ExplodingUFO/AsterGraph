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

        if (TryResolveHit(currentScreenPosition, out var hit))
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

    private bool TryResolveHit(Point rootPoint, out NodeCanvasResizeFeedbackHit hit)
    {
        hit = default;
        var world = _host.ViewModel?.ScreenToWorld(new GraphPoint(rootPoint.X, rootPoint.Y));
        if (world is null)
        {
            return false;
        }

        if (_host.NodeLayer is not null)
        {
            for (var index = _host.NodeLayer.Children.Count - 1; index >= 0; index--)
            {
                if (_host.NodeLayer.Children[index] is not Control nodeSurface
                    || !_host.ResizeFeedbackNodeSurfaces.TryGetValue(nodeSurface, out var node))
                {
                    continue;
                }

                var local = new Point(world.Value.X - node.X, world.Value.Y - node.Y);
                if (local.X >= 0d
                    && local.Y >= 0d
                    && local.X <= nodeSurface.Bounds.Width
                    && local.Y <= nodeSurface.Bounds.Height
                    && NodeCanvasResizeFeedbackResolver.TryResolveNode(
                        nodeSurface,
                        local,
                        out hit))
                {
                    return true;
                }
            }
        }

        if (_host.GroupLayer is not null)
        {
            for (var index = _host.GroupLayer.Children.Count - 1; index >= 0; index--)
            {
                if (_host.GroupLayer.Children[index] is not Border groupSurface
                    || !_host.ResizeFeedbackGroupSurfaces.TryGetValue(groupSurface, out var groupId)
                    || !_host.ResizeFeedbackGroupSnapshots.TryGetValue(groupId, out var snapshot))
                {
                    continue;
                }

                var local = new Point(world.Value.X - snapshot.Position.X, world.Value.Y - snapshot.Position.Y);
                if (local.X >= 0d
                    && local.Y >= 0d
                    && local.X <= groupSurface.Bounds.Width
                    && local.Y <= groupSurface.Bounds.Height
                    && NodeCanvasResizeFeedbackResolver.TryResolveGroup(
                        groupSurface,
                        local,
                        out hit))
                {
                    return true;
                }
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
