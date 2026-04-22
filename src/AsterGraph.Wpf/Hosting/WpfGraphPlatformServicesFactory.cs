using System.Windows;
using System.Windows.Threading;
using AsterGraph.Editor.Hosting;

namespace AsterGraph.Wpf.Hosting;

internal static class WpfGraphPlatformServicesFactory
{
    public static GraphEditorPlatformServices Create(FrameworkElement owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new GraphEditorPlatformServices
        {
            TextClipboardBridge = new WpfTextClipboardBridge(() => owner.Dispatcher),
            HostContext = new WpfGraphHostContext(
                owner,
                owner.Dispatcher.Invoke(() => Window.GetWindow(owner)),
                owner.Dispatcher.Invoke(() => owner.DataContext as IServiceProvider)),
        };
    }
}
