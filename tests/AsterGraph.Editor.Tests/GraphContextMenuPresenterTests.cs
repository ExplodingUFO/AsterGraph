using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Avalonia.Presentation;
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
}
