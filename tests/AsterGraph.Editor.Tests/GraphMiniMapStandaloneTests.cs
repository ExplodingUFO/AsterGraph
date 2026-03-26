using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphMiniMapStandaloneTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.minimap.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.minimap.target");

    [AvaloniaFact]
    public void StandaloneMiniMapFactory_BindsEditorAndRemainsNonFocusable()
    {
        var editor = CreateEditor();
        var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Editor = editor,
        });

        Assert.Same(editor, miniMap.ViewModel);
        Assert.False(miniMap.Focusable);
    }

    [AvaloniaFact]
    public void StandaloneMiniMap_RecenterViewport_ForDifferentMiniMapPoints()
    {
        var editor = CreateEditor();
        editor.UpdateViewportSize(480, 320);
        var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Editor = editor,
        });
        miniMap.Measure(new Size(240, 160));
        miniMap.Arrange(new Rect(0, 0, 240, 160));

        InvokeMiniMapMethod("CenterViewportFromMiniMap", miniMap, editor, new Point(32, 28), false);

        var panAfterFirstRecenter = (editor.PanX, editor.PanY);

        InvokeMiniMapMethod("CenterViewportFromMiniMap", miniMap, editor, new Point(180, 120), false);

        Assert.NotEqual((0d, 0d), panAfterFirstRecenter);
        Assert.NotEqual(panAfterFirstRecenter, (editor.PanX, editor.PanY));
    }

    private static void InvokeMiniMapMethod(string methodName, GraphMiniMap miniMap, params object[] args)
    {
        var method = typeof(GraphMiniMap).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Could not find GraphMiniMap handler '{methodName}'.");
        method.Invoke(miniMap, args);
    }

    private static GraphEditorViewModel CreateEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                SourceDefinitionId,
                "MiniMap Source",
                "Tests",
                "Standalone MiniMap",
                [],
                [
                    new PortDefinition(
                        "out",
                        "Out",
                        new PortTypeId("float"),
                        "#6AD5C4"),
                ]));
        catalog.RegisterDefinition(
            new NodeDefinition(
                TargetDefinitionId,
                "MiniMap Target",
                "Tests",
                "Standalone MiniMap",
                [
                    new PortDefinition(
                        "in",
                        "In",
                        new PortTypeId("float"),
                        "#F3B36B"),
                ],
                []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Standalone MiniMap Graph",
                "Regression coverage for standalone mini map composition.",
                [
                    new GraphNode(
                        "tests.minimap.source-001",
                        "MiniMap Source",
                        "Tests",
                        "Standalone MiniMap",
                        "Used to define world bounds for the mini map.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [
                            new GraphPort(
                                "out",
                                "Out",
                                PortDirection.Output,
                                "float",
                                "#6AD5C4",
                                new PortTypeId("float")),
                        ],
                        "#6AD5C4",
                        SourceDefinitionId),
                    new GraphNode(
                        "tests.minimap.target-001",
                        "MiniMap Target",
                        "Tests",
                        "Standalone MiniMap",
                        "Used to widen the world bounds for the mini map.",
                        new GraphPoint(680, 420),
                        new GraphSize(260, 180),
                        [
                            new GraphPort(
                                "in",
                                "In",
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

}
