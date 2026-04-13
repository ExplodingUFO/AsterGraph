using System.Reflection;
using AsterGraph.Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Menus;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphContextMenuPresenterTests
{
    [Fact]
    public void PresenterType_IsPublicForHostReuse()
        => Assert.True(typeof(GraphContextMenuPresenter).IsPublic);

    [Fact]
    public void PresenterType_ImplementsReplacementContract()
        => Assert.True(typeof(IGraphContextMenuPresenter).IsAssignableFrom(typeof(GraphContextMenuPresenter)));

    [Fact]
    public void PresenterType_ExposesCanonicalDescriptorOverload()
    {
        var method = typeof(GraphContextMenuPresenter).GetMethod(
            nameof(GraphContextMenuPresenter.Open),
            [
                typeof(Control),
                typeof(IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot>),
                typeof(IGraphEditorCommands),
                typeof(ContextMenuStyleOptions),
            ]);

        Assert.NotNull(method);
    }

    [AvaloniaFact]
    public void FocusedTarget_UsesAnchoredPlacementForKeyboardStyleMenuInvocation()
    {
        var target = new Button
        {
            Focusable = true,
        };
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = target,
        };
        window.Show();

        try
        {
            target.Focus();
            Assert.Equal(PlacementMode.Bottom, GraphContextMenuPresenter.ResolvePlacementForTest(target));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void FocusedNodeCanvas_UsesPointerPlacementForCanvasContextMenus()
    {
        var target = new NodeCanvas
        {
            Focusable = true,
        };
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = target,
        };
        window.Show();

        try
        {
            target.Focus();
            Assert.Equal(PlacementMode.Pointer, GraphContextMenuPresenter.ResolvePlacementForTest(target));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BuildMenuControl_DisabledItem_UsesDisabledReasonAsToolTip()
    {
        var descriptor = new MenuItemDescriptor(
            "save",
            "Save Snapshot",
            disabledReason: "Snapshot saving is disabled by host permissions.",
            isEnabled: false);

        var control = Assert.IsType<MenuItem>(
            GraphContextMenuPresenter.BuildMenuControlForTest(
                descriptor,
                GraphEditorStyleOptions.Default.ContextMenu));

        Assert.False(control.IsEnabled);
        Assert.Equal("Snapshot saving is disabled by host permissions.", ToolTip.GetTip(control));
    }

    [AvaloniaFact]
    public void BuildCanonicalMenuControl_DisabledItem_UsesDisabledReasonAsToolTip()
    {
        var method = typeof(GraphContextMenuPresenter).GetMethod(
            "BuildMenuControlForTest",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            [
                typeof(GraphEditorMenuItemDescriptorSnapshot),
                typeof(IGraphEditorCommands),
                typeof(ContextMenuStyleOptions),
            ],
            modifiers: null);
        Assert.NotNull(method);

        var descriptor = new GraphEditorMenuItemDescriptorSnapshot(
            "save",
            "Save Snapshot",
            disabledReason: "Snapshot saving is disabled by host permissions.",
            isEnabled: false);

        var control = Assert.IsType<MenuItem>(
            method!.Invoke(
                null,
                [
                    descriptor,
                    new NoOpGraphEditorCommands(),
                    GraphEditorStyleOptions.Default.ContextMenu,
                ]));

        Assert.False(control.IsEnabled);
        Assert.Equal("Snapshot saving is disabled by host permissions.", ToolTip.GetTip(control));
    }

    [AvaloniaFact]
    public void CreateContextMenuForTest_NestedMenus_OverrideFluentSubmenuPopupResources()
    {
        var method = typeof(GraphContextMenuPresenter).GetMethod(
            "CreateContextMenuForTest",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            [
                typeof(IReadOnlyList<MenuItemDescriptor>),
                typeof(ContextMenuStyleOptions),
            ],
            modifiers: null);
        Assert.NotNull(method);

        var descriptors = new[]
        {
            new MenuItemDescriptor(
                "parent",
                "Parent",
                children:
                [
                    new MenuItemDescriptor(
                        "child",
                        "Child",
                        children:
                        [
                            new MenuItemDescriptor("grandchild", "Grandchild"),
                        ]),
                ]),
        };

        var menu = Assert.IsType<ContextMenu>(
            method!.Invoke(
                null,
                [
                    descriptors,
                    GraphEditorStyleOptions.Default.ContextMenu,
                ]));

        var background = Assert.IsAssignableFrom<ISolidColorBrush>(menu.Resources["MenuFlyoutPresenterBackground"]);
        var borderBrush = Assert.IsAssignableFrom<ISolidColorBrush>(menu.Resources["MenuFlyoutPresenterBorderBrush"]);
        var borderThickness = Assert.IsType<Thickness>(menu.Resources["MenuFlyoutPresenterBorderThemeThickness"]);
        var padding = Assert.IsType<Thickness>(menu.Resources["MenuFlyoutPresenterThemePadding"]);

        Assert.Equal(Colors.Transparent, background.Color);
        Assert.Equal(Colors.Transparent, borderBrush.Color);
        Assert.Equal(new Thickness(0), borderThickness);
        Assert.Equal(new Thickness(0), padding);
    }

    [AvaloniaFact]
    public void CreateContextMenuForTest_NestedMenus_OverrideFluentHoverItemResources()
    {
        var method = typeof(GraphContextMenuPresenter).GetMethod(
            "CreateContextMenuForTest",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            [
                typeof(IReadOnlyList<MenuItemDescriptor>),
                typeof(ContextMenuStyleOptions),
            ],
            modifiers: null);
        Assert.NotNull(method);

        var style = GraphEditorStyleOptions.Default.ContextMenu with
        {
            BackgroundHex = "#102030",
            HoverHex = "#203040",
            ForegroundHex = "#E8F6FF",
            DisabledForegroundHex = "#667788",
        };

        var descriptors = new[]
        {
            new MenuItemDescriptor(
                "parent",
                "Parent",
                children:
                [
                    new MenuItemDescriptor("child", "Child"),
                ]),
        };

        var menu = Assert.IsType<ContextMenu>(
            method!.Invoke(
                null,
                [
                    descriptors,
                    style,
                ]));

        var itemBackground = Assert.IsAssignableFrom<ISolidColorBrush>(menu.Resources["MenuFlyoutItemBackground"]);
        var pointerOverBackground = Assert.IsAssignableFrom<ISolidColorBrush>(menu.Resources["MenuFlyoutItemBackgroundPointerOver"]);
        var itemForeground = Assert.IsAssignableFrom<ISolidColorBrush>(menu.Resources["MenuFlyoutItemForeground"]);
        var pointerOverForeground = Assert.IsAssignableFrom<ISolidColorBrush>(menu.Resources["MenuFlyoutItemForegroundPointerOver"]);

        Assert.Equal(Color.Parse(style.BackgroundHex), itemBackground.Color);
        Assert.Equal(Color.Parse(style.HoverHex), pointerOverBackground.Color);
        Assert.Equal(Color.Parse(style.ForegroundHex), itemForeground.Color);
        Assert.Equal(Color.Parse(style.ForegroundHex), pointerOverForeground.Color);
    }

    private sealed class NoOpGraphEditorCommands : IGraphEditorCommands
    {
        public void Undo()
        {
        }

        public void Redo()
        {
        }

        public void ClearSelection(bool updateStatus = false)
        {
        }

        public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null)
        {
        }

        public void DeleteSelection()
        {
        }

        public void PanBy(double deltaX, double deltaY)
        {
        }

        public void ZoomAt(double factor, GraphPoint screenAnchor)
        {
        }

        public void ResetView(bool updateStatus = true)
        {
        }

        public void SaveWorkspace()
        {
        }

        public bool LoadWorkspace()
            => false;
    }
}
