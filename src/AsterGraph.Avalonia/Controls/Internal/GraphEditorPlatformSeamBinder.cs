using Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class GraphEditorPlatformSeamBinder
{
    public static void Apply(GraphEditorViewModel? editor, Control owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (editor is null)
        {
            return;
        }

        editor.ApplyPlatformServices(AvaloniaGraphPlatformServicesFactory.Create(owner));
    }

    public static void Clear(GraphEditorViewModel? editor)
    {
        if (editor is null)
        {
            return;
        }

        editor.ApplyPlatformServices(null);
    }

    public static void Replace(GraphEditorViewModel? previous, GraphEditorViewModel? current, Control owner)
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
