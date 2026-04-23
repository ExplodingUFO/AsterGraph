using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xunit;

namespace AsterGraph.Wpf.Tests;

public sealed class HelloWorldWpfBootstrapTests
{
    [Fact]
    public void WpfHostedHelloWorldRoute_ComposesSessionOwnedStockSurface()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var editor = WpfRouteTestHelpers.CreateHelloWorldEditor();
        var (hostedView, hostEditor, hostApplyServices) = WpfRouteTestHelpers.RunInSta(() =>
        {
            var hostedView = WpfRouteTestHelpers.CreateHelloWorldHostedView(editor);
            var hostedUi = hostedView as UIElement
                ?? throw new InvalidOperationException("Expected hosted WPF view to be a UIElement.");

            var window = new Window
            {
                Content = hostedUi,
                Width = 200,
                Height = 120,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
            };

            window.Show();
            window.Dispatcher.Invoke(() =>
            {
            }, DispatcherPriority.Background);

            var hostEditor = WpfRouteTestHelpers.GetProperty<GraphEditorViewModel>(hostedView, "Editor");
            var hostApplyServices = WpfRouteTestHelpers.GetProperty<bool>(hostedView, "ApplyHostServices");

            window.Close();
            return (hostedView, hostEditor, hostApplyServices);
        });

        Assert.Same(editor, hostEditor);
        Assert.Same(editor.Session, hostEditor.Session);
        Assert.True(hostApplyServices);
        Assert.NotNull(editor.HostContext);
        Assert.True(
            ReferenceEquals(editor.HostContext!.Owner, hostedView),
            "Expected host context owner to be the hosted WPF view.");
    }

    [Fact]
    public void WpfHostedHelloWorldRoute_ExposesBoundedAccessibilityBaseline()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var editor = WpfRouteTestHelpers.CreateHelloWorldEditor();
        WpfRouteTestHelpers.RunInSta(() =>
        {
            var hostedView = WpfRouteTestHelpers.CreateHelloWorldHostedView(editor);
            var hostedUi = hostedView as FrameworkElement
                ?? throw new InvalidOperationException("Expected hosted WPF view to be a FrameworkElement.");

            var window = new Window
            {
                Content = hostedUi,
                Width = 200,
                Height = 120,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
            };

            window.Show();
            window.Dispatcher.Invoke(() =>
            {
            }, DispatcherPriority.Background);

            Assert.True(hostedUi.Focusable);
            Assert.Equal("Graph editor host", AutomationProperties.GetName(hostedUi));
            Assert.Equal(
                "Graph editor command bar",
                AutomationProperties.GetName(Assert.IsAssignableFrom<FrameworkElement>(hostedUi.FindName("PART_CommandBar"))));
            Assert.Equal(
                "Node templates",
                AutomationProperties.GetName(Assert.IsAssignableFrom<FrameworkElement>(hostedUi.FindName("PART_NodeTemplatesRegion"))));
            Assert.Equal(
                "Node list",
                AutomationProperties.GetName(Assert.IsAssignableFrom<FrameworkElement>(hostedUi.FindName("PART_NodesRegion"))));
            Assert.Equal(
                "Inspector summary",
                AutomationProperties.GetName(Assert.IsAssignableFrom<FrameworkElement>(hostedUi.FindName("PART_InspectorSummaryRegion"))));

            window.Close();
            return 0;
        });
    }

    [Fact]
    public void WpfHostedHelloWorldRoute_ExposesFocusAndKeyboardBindings()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var editor = WpfRouteTestHelpers.CreateHelloWorldEditor();
        WpfRouteTestHelpers.RunInSta(() =>
        {
            var hostedView = WpfRouteTestHelpers.CreateHelloWorldHostedView(editor);
            var hostedUi = hostedView as FrameworkElement
                ?? throw new InvalidOperationException("Expected hosted WPF view to be a FrameworkElement.");

            var window = new Window
            {
                Content = hostedUi,
                Width = 200,
                Height = 120,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
            };

            window.Show();
            window.Activate();
            window.Dispatcher.Invoke(() =>
            {
            }, DispatcherPriority.Background);

            Assert.True(hostedUi.Focus());
            Assert.True(hostedUi.IsKeyboardFocusWithin);

            var undoButton = Assert.IsAssignableFrom<Button>(hostedUi.FindName("PART_UndoCommandButton"));
            Assert.True(undoButton.Focusable);
            Assert.True(undoButton.IsTabStop);

            AssertKeyBinding(hostedUi.InputBindings, Key.Z, ModifierKeys.Control, editor.UndoCommand);
            AssertKeyBinding(hostedUi.InputBindings, Key.Delete, ModifierKeys.None, editor.DeleteSelectionCommand);

            window.Close();
            return 0;
        });
    }

    private static void AssertKeyBinding(InputBindingCollection inputBindings, Key key, ModifierKeys modifiers, ICommand expectedCommand)
    {
        var binding = Assert.Single(inputBindings.OfType<KeyBinding>(), candidate =>
            candidate.Key == key && candidate.Modifiers == modifiers);
        Assert.Same(expectedCommand, binding.Command);
    }
}
