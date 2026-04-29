using System;
using System.ComponentModel;

namespace AsterGraph.Editor.Scene;

[Flags]
[EditorBrowsable(EditorBrowsableState.Never)]
public enum GraphEditorInputModifiers
{
    None = 0,
    Shift = 1 << 0,
    Control = 1 << 1,
    Alt = 1 << 2,
    Meta = 1 << 3,
}

[EditorBrowsable(EditorBrowsableState.Never)]
public enum GraphEditorPointerPressRouteKind
{
    Ignore,
    BeginPanning,
    BeginCanvasSelection,
}

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct GraphEditorPointerPressRoute(
    GraphEditorPointerPressRouteKind Kind,
    bool CancelPendingConnection)
{
    public static GraphEditorPointerPressRoute Ignore { get; } = new(GraphEditorPointerPressRouteKind.Ignore, CancelPendingConnection: false);
}

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct GraphEditorPointerInputContext(
    bool IsAlreadyHandled,
    bool IsLeftButtonPressed,
    bool IsMiddleButtonPressed,
    GraphEditorInputModifiers Modifiers,
    bool EnableAltLeftDragPanning,
    bool HasPendingConnection);

[EditorBrowsable(EditorBrowsableState.Never)]
public static class GraphEditorPointerInputRouter
{
    public static GraphEditorPointerPressRoute RoutePressed(GraphEditorPointerInputContext context)
    {
        if (context.IsAlreadyHandled)
        {
            return GraphEditorPointerPressRoute.Ignore;
        }

        if (!context.IsLeftButtonPressed && !context.IsMiddleButtonPressed)
        {
            return GraphEditorPointerPressRoute.Ignore;
        }

        if (context.IsMiddleButtonPressed
            || (context.EnableAltLeftDragPanning
                && context.IsLeftButtonPressed
                && context.Modifiers.HasFlag(GraphEditorInputModifiers.Alt)))
        {
            return new GraphEditorPointerPressRoute(GraphEditorPointerPressRouteKind.BeginPanning, CancelPendingConnection: false);
        }

        return new GraphEditorPointerPressRoute(
            GraphEditorPointerPressRouteKind.BeginCanvasSelection,
            CancelPendingConnection: context.HasPendingConnection);
    }
}
