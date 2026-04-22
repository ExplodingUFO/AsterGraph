using System.Windows;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Wpf.Hosting;

internal static class WpfGraphPlatformSeamBinder
{
    public static void Apply(GraphEditorViewModel? editor, FrameworkElement owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (editor is null)
        {
            return;
        }

        editor.ApplyPlatformServices(WpfGraphPlatformServicesFactory.Create(owner));
    }

    public static void Clear(GraphEditorViewModel? editor)
    {
        if (editor is null)
        {
            return;
        }

        editor.ApplyPlatformServices(null);
    }

    public static void Replace(GraphEditorViewModel? previous, GraphEditorViewModel? current, FrameworkElement owner)
    {
        if (ReferenceEquals(previous, current))
        {
            Apply(current, owner);
            return;
        }

        Clear(previous);
        Apply(current, owner);
    }
}
