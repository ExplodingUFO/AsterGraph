using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Scene;

public enum GraphEditorSceneSurfaceKind
{
    Node,
    Group,
}

public enum GraphEditorSceneResizeHandleKind
{
    LeftEdge,
    TopEdge,
    RightEdge,
    BottomEdge,
    BottomRightCorner,
}

public readonly record struct GraphEditorSceneResizeHit(
    GraphEditorSceneSurfaceKind SurfaceKind,
    GraphEditorSceneResizeHandleKind Handle);

public sealed record GraphEditorSceneResizeHitTestProfile(
    GraphEditorSceneSurfaceKind SurfaceKind,
    double EdgeThickness,
    double CornerThickness,
    bool IncludeLeftEdge,
    bool IncludeTopEdge,
    bool IncludeRightEdge,
    bool IncludeBottomEdge,
    bool IncludeBottomRightCorner)
{
    public static GraphEditorSceneResizeHitTestProfile Node { get; } = new(
        GraphEditorSceneSurfaceKind.Node,
        EdgeThickness: 10d,
        CornerThickness: 16d,
        IncludeLeftEdge: false,
        IncludeTopEdge: false,
        IncludeRightEdge: true,
        IncludeBottomEdge: true,
        IncludeBottomRightCorner: true);

    public static GraphEditorSceneResizeHitTestProfile Group { get; } = new(
        GraphEditorSceneSurfaceKind.Group,
        EdgeThickness: 12d,
        CornerThickness: 12d,
        IncludeLeftEdge: true,
        IncludeTopEdge: true,
        IncludeRightEdge: true,
        IncludeBottomEdge: true,
        IncludeBottomRightCorner: false);
}

public static class GraphEditorSceneResizeHitTester
{
    public static GraphEditorSceneResizeHit? TryHit(
        GraphSize surfaceSize,
        GraphPoint point,
        GraphEditorSceneResizeHitTestProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (surfaceSize.Width <= 0d || surfaceSize.Height <= 0d)
        {
            return null;
        }

        if (profile.IncludeBottomRightCorner
            && point.X >= surfaceSize.Width - profile.CornerThickness
            && point.Y >= surfaceSize.Height - profile.CornerThickness)
        {
            return new GraphEditorSceneResizeHit(profile.SurfaceKind, GraphEditorSceneResizeHandleKind.BottomRightCorner);
        }

        if (profile.IncludeLeftEdge && point.X <= profile.EdgeThickness)
        {
            return new GraphEditorSceneResizeHit(profile.SurfaceKind, GraphEditorSceneResizeHandleKind.LeftEdge);
        }

        if (profile.IncludeTopEdge && point.Y <= profile.EdgeThickness)
        {
            return new GraphEditorSceneResizeHit(profile.SurfaceKind, GraphEditorSceneResizeHandleKind.TopEdge);
        }

        if (profile.IncludeRightEdge && point.X >= surfaceSize.Width - profile.EdgeThickness)
        {
            return new GraphEditorSceneResizeHit(profile.SurfaceKind, GraphEditorSceneResizeHandleKind.RightEdge);
        }

        if (profile.IncludeBottomEdge && point.Y >= surfaceSize.Height - profile.EdgeThickness)
        {
            return new GraphEditorSceneResizeHit(profile.SurfaceKind, GraphEditorSceneResizeHandleKind.BottomEdge);
        }

        return null;
    }
}
