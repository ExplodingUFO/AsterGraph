using Avalonia;
using Avalonia.Input;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasWheelInteractionCoordinatorTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.wheel.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.wheel.target");

    [Fact]
    public void HandleWheel_WhenViewportGesturesDisabled_DoesNothing()
    {
        var editor = CreateEditor();
        var initialPanX = editor.PanX;
        var initialPanY = editor.PanY;
        var initialZoom = editor.Zoom;
        var host = new TestWheelHost
        {
            ViewModel = editor,
            EnableDefaultWheelViewportGestures = false,
        };
        var coordinator = new NodeCanvasWheelInteractionCoordinator(host);

        var handled = coordinator.HandleWheel(new Point(24, 24), new Vector(0, 1), KeyModifiers.None);

        Assert.False(handled);
        Assert.Equal(initialPanX, editor.PanX);
        Assert.Equal(initialPanY, editor.PanY);
        Assert.Equal(initialZoom, editor.Zoom);
        Assert.Null(host.InteractionSession.PointerScreenPosition);
    }

    [Fact]
    public void HandleWheel_WithoutControlModifier_PansUsingScrollSpeedMultiplier()
    {
        var editor = CreateEditor();
        var initialPanX = editor.PanX;
        var initialPanY = editor.PanY;
        var initialZoom = editor.Zoom;
        var host = new TestWheelHost
        {
            ViewModel = editor,
        };
        var coordinator = new NodeCanvasWheelInteractionCoordinator(host);

        var handled = coordinator.HandleWheel(new Point(40, 32), new Vector(1, -2), KeyModifiers.None);

        Assert.True(handled);
        Assert.Equal(initialPanX + 40, editor.PanX);
        Assert.Equal(initialPanY - 80, editor.PanY);
        Assert.Equal(new Point(40, 32), host.InteractionSession.PointerScreenPosition);
        Assert.Equal(initialZoom, editor.Zoom);
    }

    [Fact]
    public void HandleWheel_WithControlModifier_ZoomsAroundPointerWithoutPanning()
    {
        var editor = CreateEditor();
        var initialPanX = editor.PanX;
        var initialPanY = editor.PanY;
        var initialZoom = editor.Zoom;
        var host = new TestWheelHost
        {
            ViewModel = editor,
        };
        var coordinator = new NodeCanvasWheelInteractionCoordinator(host);

        var handled = coordinator.HandleWheel(new Point(240, 180), new Vector(0, 1), KeyModifiers.Control);

        Assert.True(handled);
        Assert.True(editor.Zoom > initialZoom);
        Assert.True(editor.PanX < initialPanX);
        Assert.True(editor.PanY < initialPanY);
        Assert.Equal(new Point(240, 180), host.InteractionSession.PointerScreenPosition);
    }

    [Fact]
    public void HandleWheel_WithMetaModifier_ZoomsAroundPointerWithoutPanning()
    {
        var editor = CreateEditor();
        var initialPanX = editor.PanX;
        var initialPanY = editor.PanY;
        var initialZoom = editor.Zoom;
        var host = new TestWheelHost
        {
            ViewModel = editor,
        };
        var coordinator = new NodeCanvasWheelInteractionCoordinator(host);

        var handled = coordinator.HandleWheel(new Point(84, 42), new Vector(1, 2), KeyModifiers.Meta);

        Assert.True(handled);
        Assert.True(editor.Zoom > initialZoom);
        Assert.NotEqual(new Point(initialPanX, initialPanY), new Point(editor.PanX, editor.PanY));
        Assert.Equal(new Point(84, 42), host.InteractionSession.PointerScreenPosition);
    }

    [Fact]
    public void HandleWheel_WithShiftModifier_PansUsingScrollSpeedMultiplier()
    {
        var editor = CreateEditor();
        var initialPanX = editor.PanX;
        var initialPanY = editor.PanY;
        var initialZoom = editor.Zoom;
        var host = new TestWheelHost
        {
            ViewModel = editor,
        };
        var coordinator = new NodeCanvasWheelInteractionCoordinator(host);

        var handled = coordinator.HandleWheel(new Point(18, 36), new Vector(-3, 5), KeyModifiers.Shift);

        Assert.True(handled);
        Assert.Equal(initialPanX - 120, editor.PanX);
        Assert.Equal(initialPanY + 200, editor.PanY);
        Assert.Equal(new Point(18, 36), host.InteractionSession.PointerScreenPosition);
        Assert.Equal(initialZoom, editor.Zoom);
    }

    private static GraphEditorViewModel CreateEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                SourceDefinitionId,
                "Wheel Source",
                "Tests",
                "Wheel interaction test source node.",
                [],
                [
                    new PortDefinition(
                        "out",
                        "Result",
                        new PortTypeId("float"),
                        "#6AD5C4"),
                ]));
        catalog.RegisterDefinition(
            new NodeDefinition(
                TargetDefinitionId,
                "Wheel Target",
                "Tests",
                "Wheel interaction test target node.",
                [
                    new PortDefinition(
                        "in",
                        "Input",
                        new PortTypeId("float"),
                        "#F3B36B"),
                ],
                []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Wheel Interaction Graph",
                "Regression coverage for node canvas wheel interaction extraction.",
                [
                    new GraphNode(
                        "tests.wheel.source-001",
                        "Wheel Source",
                        "Tests",
                        "Wheel",
                        "Source node for wheel coordinator tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [
                            new GraphPort(
                                "out",
                                "Result",
                                PortDirection.Output,
                                "float",
                                "#6AD5C4",
                                new PortTypeId("float")),
                        ],
                        "#6AD5C4",
                        SourceDefinitionId),
                    new GraphNode(
                        "tests.wheel.target-001",
                        "Wheel Target",
                        "Tests",
                        "Wheel",
                        "Target node for wheel coordinator tests.",
                        new GraphPoint(420, 160),
                        new GraphSize(240, 160),
                        [
                            new GraphPort(
                                "in",
                                "Input",
                                PortDirection.Input,
                                "float",
                                "#F3B36B",
                                new PortTypeId("float")),
                        ],
                        [],
                        "#F3B36B",
                        TargetDefinitionId),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private sealed class TestWheelHost : INodeCanvasWheelInteractionHost
    {
        public GraphEditorViewModel? ViewModel { get; init; }

        public bool EnableDefaultWheelViewportGestures { get; init; } = true;

        public NodeCanvasInteractionSession InteractionSession { get; } = new();
    }
}
