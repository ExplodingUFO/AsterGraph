using Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class GraphEditorPlatformSeamBinder
{
    public static void Apply(
        GraphEditorViewModel? editor,
        Control owner,
        bool includeClipboard = true,
        bool includeHostContext = true)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (editor is null)
        {
            return;
        }

        if (includeClipboard)
        {
            // The bridge resolves the current top level lazily, so it remains valid
            // even if the control is reparented after the seam is attached.
            editor.SetTextClipboardBridge(new AvaloniaTextClipboardBridge(() => TopLevel.GetTopLevel(owner)?.Clipboard));
        }

        if (includeHostContext)
        {
            editor.SetHostContext(new AvaloniaGraphHostContext(owner, TopLevel.GetTopLevel(owner)));
        }
    }

    public static void Clear(
        GraphEditorViewModel? editor,
        bool includeClipboard = true,
        bool includeHostContext = true)
    {
        if (editor is null)
        {
            return;
        }

        if (includeClipboard)
        {
            editor.SetTextClipboardBridge(null);
        }

        if (includeHostContext)
        {
            editor.SetHostContext(null);
        }
    }

    public static void Replace(
        GraphEditorViewModel? previous,
        GraphEditorViewModel? current,
        Control owner,
        bool includeClipboard = true,
        bool includeHostContext = true)
    {
        if (ReferenceEquals(previous, current))
        {
            Apply(current, owner, includeClipboard, includeHostContext);
            return;
        }

        Clear(previous, includeClipboard, includeHostContext);
        Apply(current, owner, includeClipboard, includeHostContext);
    }
}
