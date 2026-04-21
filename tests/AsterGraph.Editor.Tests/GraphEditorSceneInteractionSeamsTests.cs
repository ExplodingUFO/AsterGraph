using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Scene;
using AsterGraph.Core.Models;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSceneInteractionSeamsTests
{
    [Fact]
    public void PointerInputRouter_RoutePressed_WithAltLeftDrag_ReturnsPanningRoute()
    {
        var route = GraphEditorPointerInputRouter.RoutePressed(new GraphEditorPointerInputContext(
            IsAlreadyHandled: false,
            IsLeftButtonPressed: true,
            IsMiddleButtonPressed: false,
            Modifiers: GraphEditorInputModifiers.Alt,
            EnableAltLeftDragPanning: true,
            HasPendingConnection: false));

        Assert.Equal(GraphEditorPointerPressRouteKind.BeginPanning, route.Kind);
        Assert.False(route.CancelPendingConnection);
    }

    [Fact]
    public void PointerInputRouter_RoutePressed_WithPendingConnection_ReturnsSelectionRouteAndCancelsPreview()
    {
        var route = GraphEditorPointerInputRouter.RoutePressed(new GraphEditorPointerInputContext(
            IsAlreadyHandled: false,
            IsLeftButtonPressed: true,
            IsMiddleButtonPressed: false,
            Modifiers: GraphEditorInputModifiers.None,
            EnableAltLeftDragPanning: true,
            HasPendingConnection: true));

        Assert.Equal(GraphEditorPointerPressRouteKind.BeginCanvasSelection, route.Kind);
        Assert.True(route.CancelPendingConnection);
    }

    [Fact]
    public void PointerInputRouter_RoutePressed_WithNonLeftNonMiddleButtons_ReturnsIgnore()
    {
        var route = GraphEditorPointerInputRouter.RoutePressed(new GraphEditorPointerInputContext(
            IsAlreadyHandled: false,
            IsLeftButtonPressed: false,
            IsMiddleButtonPressed: false,
            Modifiers: GraphEditorInputModifiers.None,
            EnableAltLeftDragPanning: true,
            HasPendingConnection: false));

        Assert.Equal(GraphEditorPointerPressRouteKind.Ignore, route.Kind);
        Assert.False(route.CancelPendingConnection);
    }

    [Fact]
    public void SceneResizeHitTester_TryHit_NodeProfile_ResolvesBottomRightCorner()
    {
        var hit = GraphEditorSceneResizeHitTester.TryHit(
            new GraphSize(240d, 160d),
            new GraphPoint(236d, 156d),
            GraphEditorSceneResizeHitTestProfile.Node);

        Assert.NotNull(hit);
        Assert.Equal(GraphEditorSceneSurfaceKind.Node, hit.Value.SurfaceKind);
        Assert.Equal(GraphEditorSceneResizeHandleKind.BottomRightCorner, hit.Value.Handle);
    }

    [Fact]
    public void SceneResizeHitTester_TryHit_GroupProfile_ResolvesLeftEdgeWithoutAvaloniaTypes()
    {
        var hit = GraphEditorSceneResizeHitTester.TryHit(
            new GraphSize(320d, 220d),
            new GraphPoint(3d, 80d),
            GraphEditorSceneResizeHitTestProfile.Group);

        Assert.NotNull(hit);
        Assert.Equal(GraphEditorSceneSurfaceKind.Group, hit.Value.SurfaceKind);
        Assert.Equal(GraphEditorSceneResizeHandleKind.LeftEdge, hit.Value.Handle);
    }

    [AvaloniaFact]
    public void ShortcutRouter_WithTextBoxSource_SkipsHandling()
    {
        var actionHandled = false;
        var actions = new List<AsterGraphHostedActionDescriptor>
        {
            CreateAction("selection.delete", () => actionHandled = true),
        };
        var args = new KeyEventArgs { Key = Key.Delete };
        var source = new TextBox();

        Assert.False(GraphEditorDefaultCommandShortcutRouter.TryHandle(actions, source, args, allowInputControlFocus: false));
        Assert.False(actionHandled);
    }

    [AvaloniaFact]
    public void ShortcutRouter_WithCustomInputScopeSurface_SkipsHandling()
    {
        var actionHandled = false;
        var actions = new List<AsterGraphHostedActionDescriptor>
        {
            CreateAction("selection.delete", () => actionHandled = true),
        };
        var args = new KeyEventArgs { Key = Key.Delete };
        var scope = new CustomInputScopeSurface();
        var source = new TextBlock();
        scope.Child = source;

        Assert.False(GraphEditorDefaultCommandShortcutRouter.TryHandle(actions, source, args, allowInputControlFocus: false));
        Assert.False(actionHandled);
    }

    [AvaloniaFact]
    public void ShortcutRouter_WithNonInputSurface_HandlesShortcut()
    {
        var actionHandled = false;
        var actions = new List<AsterGraphHostedActionDescriptor>
        {
            CreateAction("selection.delete", () => actionHandled = true),
        };
        var args = new KeyEventArgs { Key = Key.Delete };
        var source = new Border();

        Assert.True(GraphEditorDefaultCommandShortcutRouter.TryHandle(actions, source, args, allowInputControlFocus: false));
        Assert.True(actionHandled);
    }

    private static AsterGraphHostedActionDescriptor CreateAction(string id, Action execute)
    {
        return new AsterGraphHostedActionDescriptor(
            new GraphEditorCommandDescriptorSnapshot(id, true),
            () =>
            {
                execute();
                return true;
            });
    }

    private sealed class CustomInputScopeSurface : Border, IGraphEditorInputScope
    {
    }
}
