using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class WhiteboardPrimitiveModelContractsTests
{
    [Fact]
    public void WhiteboardPrimitiveModel_CapturesRendererNeutralIdentityGeometryStyleAndLifecycle()
    {
        var points = new List<GraphPoint>
        {
            new(12d, 18d),
            new(24d, 32d),
        };

        var primitive = new GraphWhiteboardPrimitive(
            "whiteboard-primitive-001",
            GraphWhiteboardPrimitiveKind.Freehand,
            new GraphWhiteboardPrimitiveGeometry(
                new GraphPoint(12d, 18d),
                new GraphSize(120d, 64d),
                points),
            new GraphWhiteboardPrimitiveStyle(
                FillHex: "#6AD5C4",
                StrokeHex: "#1A1F2E",
                StrokeThickness: 2.5d,
                Opacity: 0.72d),
            ZIndex: 12,
            new GraphWhiteboardPrimitiveEditLifecycle(
                GraphWhiteboardPrimitiveEditState.Committed,
                ActiveHandleKey: "freehand-finalized"));

        points.Add(new GraphPoint(999d, 999d));

        Assert.Equal("whiteboard-primitive-001", primitive.Id);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Freehand, primitive.Kind);
        Assert.Equal(new GraphPoint(12d, 18d), primitive.Geometry.Origin);
        Assert.Equal(new GraphSize(120d, 64d), primitive.Geometry.Size);
        Assert.Equal([new GraphPoint(12d, 18d), new GraphPoint(24d, 32d)], primitive.Geometry.Points);
        Assert.Equal("#6AD5C4", primitive.Style.FillHex);
        Assert.Equal("#1A1F2E", primitive.Style.StrokeHex);
        Assert.Equal(2.5d, primitive.Style.StrokeThickness);
        Assert.Equal(0.72d, primitive.Style.Opacity);
        Assert.Equal(12, primitive.ZIndex);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Committed, primitive.EditLifecycle.State);
        Assert.Equal("freehand-finalized", primitive.EditLifecycle.ActiveHandleKey);
    }

    [Fact]
    public void WhiteboardPrimitiveModel_StaysInternalAndSeparateFromGraphAndSelectionState()
    {
        Assert.False(typeof(GraphWhiteboardPrimitive).IsPublic);
        Assert.False(typeof(GraphWhiteboardPrimitiveGeometry).IsPublic);
        Assert.False(typeof(GraphWhiteboardPrimitiveStyle).IsPublic);
        Assert.False(typeof(GraphWhiteboardPrimitiveEditLifecycle).IsPublic);

        AssertNoWhiteboardProperty(typeof(GraphDocument));
        AssertNoWhiteboardProperty(typeof(GraphScope));
        AssertNoWhiteboardProperty(typeof(GraphNode));
        AssertNoWhiteboardProperty(typeof(GraphConnection));
        AssertNoWhiteboardProperty(typeof(GraphNodeGroup));
        AssertNoWhiteboardProperty(typeof(GraphEditorSceneSnapshot));
        AssertNoWhiteboardProperty(typeof(GraphEditorSelectionSnapshot));
        AssertNoWhiteboardProperty(typeof(GraphEditorSelectionRectangleSnapshot));
        AssertNoWhiteboardProperty(typeof(GraphEditorSelectionLassoSnapshot));
    }

    [Fact]
    public void WhiteboardPrimitiveModel_RejectsInvalidIdentityAndStyleValues()
    {
        var geometry = new GraphWhiteboardPrimitiveGeometry(new GraphPoint(0d, 0d), new GraphSize(10d, 10d));
        var style = GraphWhiteboardPrimitiveStyle.Default;
        var lifecycle = GraphWhiteboardPrimitiveEditLifecycle.Default;

        Assert.Throws<ArgumentException>(() => new GraphWhiteboardPrimitive(
            string.Empty,
            GraphWhiteboardPrimitiveKind.Rectangle,
            geometry,
            style,
            ZIndex: 0,
            lifecycle));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = style with { StrokeThickness = -0.01d };
        });
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = style with { Opacity = 1.01d };
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = geometry with { Points = null! };
        });
    }

    private static void AssertNoWhiteboardProperty(Type type)
    {
        Assert.DoesNotContain(
            type.GetProperties(),
            property => property.Name.Contains("Whiteboard", StringComparison.OrdinalIgnoreCase)
                || property.PropertyType.Name.Contains("Whiteboard", StringComparison.OrdinalIgnoreCase));
    }
}
