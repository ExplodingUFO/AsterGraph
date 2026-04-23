using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using System.Windows;
using System.Windows.Automation;
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
}
