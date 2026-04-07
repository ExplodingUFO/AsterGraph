using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class GraphEditorDefaultCommandShortcutRouter
{
    public static bool TryHandle(
        GraphEditorViewModel? editor,
        object? source,
        KeyEventArgs args,
        bool includePendingConnectionCancel)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (editor is null || ShortcutBelongsToInputControl(source))
        {
            return false;
        }

        var session = editor.Session;
        var capabilities = session.Queries.GetCapabilitySnapshot();
        var commands = session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.S)
        {
            TryExecute(commands, session.Commands, "workspace.save");
            return true;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.O)
        {
            TryExecute(commands, session.Commands, "workspace.load");
            return true;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control)
            && (args.Key == Key.Y || (args.Key == Key.Z && args.KeyModifiers.HasFlag(KeyModifiers.Shift))))
        {
            if (capabilities.CanRedo)
            {
                session.Commands.Redo();
            }

            return true;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.Z)
        {
            if (capabilities.CanUndo)
            {
                session.Commands.Undo();
            }

            return true;
        }

        // Copy/paste remain compatibility-executed until the runtime command contract
        // grows canonical invocation IDs for those async clipboard operations.
        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.C)
        {
            if (capabilities.CanCopySelection && editor.CopySelectionCommand.CanExecute(null))
            {
                editor.CopySelectionCommand.Execute(null);
            }

            return true;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.V)
        {
            if (capabilities.CanPaste && editor.PasteCommand.CanExecute(null))
            {
                editor.PasteCommand.Execute(null);
            }

            return true;
        }

        if (args.Key == Key.Delete)
        {
            TryExecute(commands, session.Commands, "selection.delete");
            return true;
        }

        if (includePendingConnectionCancel && args.Key == Key.Escape)
        {
            if (IsEnabled(commands, "connections.cancel"))
            {
                session.Commands.CancelPendingConnection();
                return true;
            }

            return false;
        }

        return false;
    }

    private static void TryExecute(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands,
        IGraphEditorCommands commandSurface,
        string commandId)
    {
        if (IsEnabled(commands, commandId))
        {
            commandSurface.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(commandId));
        }
    }

    private static bool IsEnabled(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands,
        string commandId)
        => commands.TryGetValue(commandId, out var descriptor) && descriptor.IsEnabled;

    private static bool ShortcutBelongsToInputControl(object? source)
    {
        for (var current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is TextBox or ComboBox)
            {
                return true;
            }
        }

        return false;
    }
}
