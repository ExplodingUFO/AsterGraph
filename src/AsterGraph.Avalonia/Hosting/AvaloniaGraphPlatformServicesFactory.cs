using Avalonia.Controls;
using AsterGraph.Avalonia.Services;
using AsterGraph.Editor.Hosting;

namespace AsterGraph.Avalonia.Hosting;

internal static class AvaloniaGraphPlatformServicesFactory
{
    public static GraphEditorPlatformServices Create(Control owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new GraphEditorPlatformServices
        {
            TextClipboardBridge = new AvaloniaTextClipboardBridge(() => TopLevel.GetTopLevel(owner)?.Clipboard),
            HostContext = new AvaloniaGraphHostContext(owner, TopLevel.GetTopLevel(owner)),
        };
    }
}
