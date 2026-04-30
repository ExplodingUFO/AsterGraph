using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphMiniMapStandaloneTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.minimap.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.minimap.target");

    [AvaloniaFact]
    public void StandaloneMiniMapFactory_BindsSessionAndRemainsNonFocusable()
    {
        var editor = CreateEditor();
        var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Session = editor.Session,
        });

        Assert.Same(editor.Session, miniMap.Session);
        Assert.False(miniMap.Focusable);
    }

    [AvaloniaFact]
    public void HostedMiniMap_UsesLightweightProjectionInThroughputMode()
    {
        var editor = CreateEditor();
        var balancedView = new GraphEditorView
        {
            Editor = editor,
            WorkbenchPerformanceMode = AsterGraphWorkbenchPerformanceMode.Balanced,
        };
        var throughputView = new GraphEditorView
        {
            Editor = editor,
            WorkbenchPerformanceMode = AsterGraphWorkbenchPerformanceMode.Throughput,
        };

        var balancedMiniMap = Assert.IsType<GraphMiniMap>(balancedView.FindControl<GraphMiniMap>("PART_MiniMapSurface"));
        var throughputMiniMap = Assert.IsType<GraphMiniMap>(throughputView.FindControl<GraphMiniMap>("PART_MiniMapSurface"));

        Assert.False(ReadMiniMapLightweightProjection(balancedMiniMap));
        Assert.True(ReadMiniMapLightweightProjection(throughputMiniMap));
    }

    [AvaloniaFact]
    public void LightweightMiniMap_ReusesNodeProjectionButRefreshesViewportSnapshot()
    {
        var editor = CreateEditor();
        editor.UpdateViewportSize(480, 320);
        var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Session = editor.Session,
        });
        SetMiniMapLightweightProjection(miniMap, true);

        var firstProjection = InvokeStockSurfaceProjection(miniMap, editor.Session);
        var firstNodes = ReadProjectionProperty(firstProjection, "Nodes");
        var firstViewport = ReadProjectionProperty<GraphEditorViewportSnapshot>(firstProjection, "Viewport");

        editor.PanBy(96, 48);

        var secondProjection = InvokeStockSurfaceProjection(miniMap, editor.Session);
        var secondNodes = ReadProjectionProperty(secondProjection, "Nodes");
        var secondViewport = ReadProjectionProperty<GraphEditorViewportSnapshot>(secondProjection, "Viewport");

        Assert.Same(firstNodes, secondNodes);
        Assert.Equal(firstViewport.PanX + 96d, secondViewport.PanX);
        Assert.Equal(firstViewport.PanY + 48d, secondViewport.PanY);
    }

    [Fact]
    public void WorkbenchPerformancePolicy_ExposesMiniMapCadenceBudgetMarker()
    {
        var balanced = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Balanced);
        var throughput = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Throughput);

        Assert.Equal("viewport-continuous", balanced.MiniMapRefreshCadence);
        Assert.Equal("document-selection-cached", throughput.MiniMapRefreshCadence);
        Assert.Equal(
            "MINIMAP_CADENCE:Balanced:cadence=viewport-continuous:commandRefreshMs=16",
            balanced.ToMiniMapBudgetMarker());
        Assert.Equal(
            "MINIMAP_CADENCE:Throughput:cadence=document-selection-cached:commandRefreshMs=50",
            throughput.ToMiniMapBudgetMarker());
    }

    [AvaloniaFact]
    public void StandaloneMiniMap_RecenterViewport_ForDifferentMiniMapPoints()
    {
        var editor = CreateEditor();
        editor.UpdateViewportSize(480, 320);
        var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Session = editor.Session,
        });
        miniMap.Measure(new Size(240, 160));
        miniMap.Arrange(new Rect(0, 0, 240, 160));

        InvokeMiniMapMethod("CenterViewportFromMiniMap", miniMap, editor.Session, new Point(32, 28), false);

        var panAfterFirstRecenter = (editor.PanX, editor.PanY);

        InvokeMiniMapMethod("CenterViewportFromMiniMap", miniMap, editor.Session, new Point(180, 120), false);

        Assert.NotEqual((0d, 0d), panAfterFirstRecenter);
        Assert.NotEqual(panAfterFirstRecenter, (editor.PanX, editor.PanY));
    }

    [AvaloniaFact]
    public void StandaloneMiniMap_CustomPresenter_RecenterViewportThroughEditorApi()
    {
        var editor = CreateEditor();
        editor.UpdateViewportSize(480, 320);
        var customPresenter = new RecordingMiniMapPresenter();
        var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Session = editor.Session,
            Presentation = new AsterGraphPresentationOptions
            {
                MiniMapPresenter = customPresenter,
            },
        });
        var window = new Window
        {
            Width = 320,
            Height = 220,
            Content = miniMap,
        };
        window.Show();

        try
        {
            var button = miniMap.GetVisualDescendants()
                .OfType<Button>()
                .Single(control => Equals(control.Tag, "custom-minimap-center"));
            var allText = string.Join(
                "\n",
                miniMap.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Same(editor.Session, customPresenter.LastSession);
            Assert.Same(customPresenter, miniMap.MiniMapPresenter);
            Assert.Contains("CUSTOM MINIMAP SURFACE", allText);
            Assert.NotEqual((0d, 0d), (editor.PanX, editor.PanY));
        }
        finally
        {
            window.Close();
        }
    }

    private static void InvokeMiniMapMethod(string methodName, GraphMiniMap miniMap, params object[] args)
    {
        var method = typeof(GraphMiniMap).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Could not find GraphMiniMap handler '{methodName}'.");
        method.Invoke(miniMap, args);
    }

    private static bool ReadMiniMapLightweightProjection(GraphMiniMap miniMap)
    {
        var property = typeof(GraphMiniMap).GetProperty(
            "UsesLightweightProjection",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("Could not find GraphMiniMap lightweight projection property.");
        return Assert.IsType<bool>(property.GetValue(miniMap));
    }

    private static void SetMiniMapLightweightProjection(GraphMiniMap miniMap, bool value)
    {
        var property = typeof(GraphMiniMap).GetProperty(
            "UsesLightweightProjection",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("Could not find GraphMiniMap lightweight projection property.");
        property.SetValue(miniMap, value);
    }

    private static object InvokeStockSurfaceProjection(GraphMiniMap miniMap, IGraphEditorSession session)
    {
        var surface = miniMap.Content
            ?? throw new Xunit.Sdk.XunitException("Expected stock mini map content.");
        var method = surface.GetType().GetMethod("GetProjection", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("Could not find stock mini map projection method.");
        return method.Invoke(surface, [session])
            ?? throw new Xunit.Sdk.XunitException("Expected stock mini map projection.");
    }

    private static object ReadProjectionProperty(object projection, string propertyName)
    {
        var property = projection.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new Xunit.Sdk.XunitException($"Could not find mini map projection property '{propertyName}'.");
        return property.GetValue(projection)
            ?? throw new Xunit.Sdk.XunitException($"Expected mini map projection property '{propertyName}'.");
    }

    private static T ReadProjectionProperty<T>(object projection, string propertyName)
    {
        var value = ReadProjectionProperty(projection, propertyName);
        return Assert.IsType<T>(value);
    }

    [Fact]
    public void MiniMapPresenter_UsesSessionContract()
    {
        var createMethod = typeof(IGraphMiniMapPresenter).GetMethod(nameof(IGraphMiniMapPresenter.Create))
            ?? throw new Xunit.Sdk.XunitException("Could not find minimap presenter create method.");

        Assert.Equal(typeof(IGraphEditorSession), createMethod.GetParameters().Single().ParameterType);
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

    private sealed class RecordingMiniMapPresenter : IGraphMiniMapPresenter
    {
        public IGraphEditorSession? LastSession { get; private set; }

        public Control Create(IGraphEditorSession? session)
        {
            LastSession = session;
            var button = new Button
            {
                Tag = "custom-minimap-center",
                Content = "Center Custom MiniMap",
            };
            button.Click += (_, _) => session?.Commands.CenterViewAt(new GraphPoint(520, 360), updateStatus: false);

            return new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "CUSTOM MINIMAP SURFACE" },
                    button,
                },
            };
        }
    }

}
