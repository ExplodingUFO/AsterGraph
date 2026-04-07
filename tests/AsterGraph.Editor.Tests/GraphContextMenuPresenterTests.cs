using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
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
