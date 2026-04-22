using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using System.Windows;
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
}
