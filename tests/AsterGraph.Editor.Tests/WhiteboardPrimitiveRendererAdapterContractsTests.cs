using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class WhiteboardPrimitiveRendererAdapterContractsTests
{
    [Fact]
    public void Project_CreatesRendererNeutralSceneDataForRectangleAndFreehandPrimitives()
    {
        var rectangle = CreateRectanglePrimitive("rectangle-001", zIndex: 1);
        var freehand = CreateFreehandPrimitive(
            "freehand-001",
            zIndex: 2,
            [
                new GraphPoint(32d, 42d),
                new GraphPoint(52d, 64d),
                new GraphPoint(74d, 48d),
            ]);

        var scene = GraphWhiteboardPrimitiveRendererAdapter.Project([rectangle, freehand]);

        Assert.Equal(2, scene.Primitives.Count);
        var rectangleScene = Assert.Single(scene.Primitives, item => item.Id == "rectangle-001");
        Assert.Equal(GraphWhiteboardPrimitiveKind.Rectangle, rectangleScene.Kind);
        Assert.Equal(new GraphPoint(10d, 20d), rectangleScene.BoundsOrigin);
        Assert.Equal(new GraphSize(80d, 48d), rectangleScene.BoundsSize);
        Assert.Empty(rectangleScene.Points);
        Assert.Equal("#F2D45C", rectangleScene.Style.FillHex);
        Assert.Equal("#222222", rectangleScene.Style.StrokeHex);
        Assert.Equal(1.5d, rectangleScene.Style.StrokeThickness);
        Assert.Equal(0.8d, rectangleScene.Style.Opacity);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Committed, rectangleScene.EditLifecycle.State);

        var freehandScene = Assert.Single(scene.Primitives, item => item.Id == "freehand-001");
        Assert.Equal(GraphWhiteboardPrimitiveKind.Freehand, freehandScene.Kind);
        Assert.Equal(new GraphPoint(30d, 40d), freehandScene.BoundsOrigin);
        Assert.Equal(new GraphSize(60d, 40d), freehandScene.BoundsSize);
        Assert.Equal(
            [
                new GraphPoint(32d, 42d),
                new GraphPoint(52d, 64d),
                new GraphPoint(74d, 48d),
            ],
            freehandScene.Points);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Editing, freehandScene.EditLifecycle.State);
        Assert.Equal("freehand-drag", freehandScene.EditLifecycle.ActiveHandleKey);
    }

    [Fact]
    public void HitTest_ReturnsTopmostPrimitiveByZIndexAndPreservesEditLifecycleEvidence()
    {
        var lower = CreateRectanglePrimitive("lower-rectangle", zIndex: 1);
        var upper = CreateRectanglePrimitive(
            "upper-rectangle",
            zIndex: 8,
            origin: new GraphPoint(24d, 28d),
            size: new GraphSize(36d, 24d),
            lifecycle: new GraphWhiteboardPrimitiveEditLifecycle(
                GraphWhiteboardPrimitiveEditState.Editing,
                ActiveHandleKey: "resize-east"));

        var scene = GraphWhiteboardPrimitiveRendererAdapter.Project([lower, upper]);

        var hit = GraphWhiteboardPrimitiveRendererAdapter.HitTest(scene, new GraphPoint(30d, 34d));

        Assert.NotNull(hit);
        Assert.Equal("upper-rectangle", hit.PrimitiveId);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Rectangle, hit.Kind);
        Assert.Equal(8, hit.ZIndex);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Editing, hit.EditLifecycle.State);
        Assert.Equal("resize-east", hit.EditLifecycle.ActiveHandleKey);
        Assert.Null(GraphWhiteboardPrimitiveRendererAdapter.HitTest(scene, new GraphPoint(500d, 500d)));
    }

    [Fact]
    public void HitTest_FreehandPrimitiveIgnoresPointInsideBoundsButAwayFromStroke()
    {
        var freehand = CreateFreehandPrimitive(
            "freehand-001",
            zIndex: 2,
            [
                new GraphPoint(32d, 42d),
                new GraphPoint(52d, 64d),
                new GraphPoint(74d, 48d),
            ]);
        var scene = GraphWhiteboardPrimitiveRendererAdapter.Project([freehand]);

        var hit = GraphWhiteboardPrimitiveRendererAdapter.HitTest(scene, new GraphPoint(88d, 78d));

        Assert.Null(hit);
    }

    [Fact]
    public void HitTest_FreehandPrimitiveReturnsHitNearStrokeSegment()
    {
        var freehand = CreateFreehandPrimitive(
            "freehand-001",
            zIndex: 2,
            [
                new GraphPoint(32d, 42d),
                new GraphPoint(52d, 64d),
                new GraphPoint(74d, 48d),
            ]);
        var scene = GraphWhiteboardPrimitiveRendererAdapter.Project([freehand]);

        var hit = GraphWhiteboardPrimitiveRendererAdapter.HitTest(scene, new GraphPoint(53d, 63d));

        Assert.NotNull(hit);
        Assert.Equal("freehand-001", hit.PrimitiveId);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Freehand, hit.Kind);
    }

    [Fact]
    public void RendererAdapter_StaysInternalRendererNeutralAndSeparateFromGraphSceneAndAvaloniaRenderers()
    {
        Assert.False(typeof(GraphWhiteboardPrimitiveRendererAdapter).IsPublic);
        Assert.False(typeof(GraphWhiteboardPrimitiveSceneSnapshot).IsPublic);
        Assert.False(typeof(GraphWhiteboardPrimitiveSceneItem).IsPublic);
        Assert.False(typeof(GraphWhiteboardPrimitiveHitTestResult).IsPublic);

        Assert.Equal(typeof(GraphWhiteboardPrimitive).Assembly, typeof(GraphWhiteboardPrimitiveRendererAdapter).Assembly);
        Assert.DoesNotContain(
            typeof(GraphWhiteboardPrimitiveRendererAdapter).Assembly.GetReferencedAssemblies(),
            assemblyName => assemblyName.Name is "AsterGraph.Editor" or "AsterGraph.Avalonia");

        AssertNoWhiteboardProperty(typeof(GraphDocument));
        AssertNoWhiteboardProperty(typeof(GraphEditorSceneSnapshot));

        var avaloniaConnectionRenderer = Type.GetType(
            "AsterGraph.Avalonia.Controls.Internal.NodeCanvasConnectionSceneRenderer, AsterGraph.Avalonia");
        Assert.NotNull(avaloniaConnectionRenderer);
        Assert.NotEqual(
            avaloniaConnectionRenderer!.Assembly,
            typeof(GraphWhiteboardPrimitiveRendererAdapter).Assembly);
    }

    private static GraphWhiteboardPrimitive CreateRectanglePrimitive(
        string id,
        int zIndex,
        GraphPoint? origin = null,
        GraphSize? size = null,
        GraphWhiteboardPrimitiveEditLifecycle? lifecycle = null)
        => new(
            id,
            GraphWhiteboardPrimitiveKind.Rectangle,
            new GraphWhiteboardPrimitiveGeometry(
                origin ?? new GraphPoint(10d, 20d),
                size ?? new GraphSize(80d, 48d)),
            new GraphWhiteboardPrimitiveStyle(
                FillHex: "#F2D45C",
                StrokeHex: "#222222",
                StrokeThickness: 1.5d,
                Opacity: 0.8d),
            zIndex,
            lifecycle ?? GraphWhiteboardPrimitiveEditLifecycle.Default);

    private static GraphWhiteboardPrimitive CreateFreehandPrimitive(
        string id,
        int zIndex,
        IReadOnlyList<GraphPoint> points)
        => new(
            id,
            GraphWhiteboardPrimitiveKind.Freehand,
            new GraphWhiteboardPrimitiveGeometry(
                new GraphPoint(30d, 40d),
                new GraphSize(60d, 40d),
                points),
            GraphWhiteboardPrimitiveStyle.Default,
            zIndex,
            new GraphWhiteboardPrimitiveEditLifecycle(
                GraphWhiteboardPrimitiveEditState.Editing,
                ActiveHandleKey: "freehand-drag"));

    private static void AssertNoWhiteboardProperty(Type type)
    {
        Assert.DoesNotContain(
            type.GetProperties(),
            property => property.Name.Contains("Whiteboard", StringComparison.OrdinalIgnoreCase)
                || property.PropertyType.Name.Contains("Whiteboard", StringComparison.OrdinalIgnoreCase));
    }
}
