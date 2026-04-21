using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class GraphEditorDefaultCommandShortcutRouter
{
    public static bool TryHandle(
        GraphEditorViewModel? editor,
        object? source,
        KeyEventArgs args,
        AsterGraphCommandShortcutPolicy policy,
        bool includePendingConnectionCancel)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(policy);

        if (editor is null || ShortcutBelongsToInputControl(source))
        {
            return false;
        }

        var session = editor.Session;
        var commands = session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        return TryHandleDescriptorShortcut(commands, session.Commands, args, policy, includePendingConnectionCancel);
    }

    public static bool TryHandle(
        IReadOnlyList<AsterGraphHostedActionDescriptor> actions,
        object? source,
        KeyEventArgs args,
        bool allowInputControlFocus,
        IEnumerable<string>? excludedActionIds = null)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(args);

        if (!allowInputControlFocus && ShortcutBelongsToInputControl(source))
        {
            return false;
        }

        var excludedIds = excludedActionIds?.ToHashSet(StringComparer.Ordinal);
        foreach (var action in actions.Where(action => !string.IsNullOrWhiteSpace(action.DefaultShortcut)))
        {
            if (excludedIds?.Contains(action.Id) == true)
            {
                continue;
            }

            if (!MatchesShortcutText(action.DefaultShortcut, args))
            {
                continue;
            }

            action.TryExecute();
            return true;
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

    private static bool TryHandleDescriptorShortcut(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands,
        IGraphEditorCommands commandSurface,
        KeyEventArgs args,
        AsterGraphCommandShortcutPolicy policy,
        bool includePendingConnectionCancel)
    {
        foreach (var descriptor in commands.Values.Where(descriptor => !string.IsNullOrWhiteSpace(descriptor.DefaultShortcut)))
        {
            if (!includePendingConnectionCancel
                && string.Equals(descriptor.Id, "connections.cancel", StringComparison.Ordinal))
            {
                continue;
            }

            var shortcut = policy.ResolveShortcut(descriptor.Id, descriptor.DefaultShortcut);
            if (!MatchesShortcutText(shortcut, args))
            {
                continue;
            }

            TryExecute(commands, commandSurface, descriptor.Id);
            return true;
        }

        return false;
    }

    private static bool MatchesShortcutText(string? shortcutText, KeyEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(shortcutText))
        {
            return false;
        }

        return shortcutText
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(candidate => MatchesShortcutCandidate(candidate, args));
    }

    private static bool MatchesShortcutCandidate(string candidate, KeyEventArgs args)
    {
        var tokens = candidate.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return false;
        }

        var requiresPrimaryModifier = false;
        var requiresShift = false;
        var requiresAlt = false;
        Key? expectedKey = null;

        foreach (var token in tokens)
        {
            switch (token.ToUpperInvariant())
            {
                case "CTRL":
                case "CMD":
                case "COMMAND":
                    requiresPrimaryModifier = true;
                    break;
                case "SHIFT":
                    requiresShift = true;
                    break;
                case "ALT":
                case "OPTION":
                    requiresAlt = true;
                    break;
                case "DELETE":
                    expectedKey = Key.Delete;
                    break;
                case "ESC":
                case "ESCAPE":
                    expectedKey = Key.Escape;
                    break;
                default:
                    if (Enum.TryParse<Key>(token, ignoreCase: true, out var parsedKey))
                    {
                        expectedKey = parsedKey;
                    }

                    break;
            }
        }

        if (expectedKey is null)
        {
            return false;
        }

        var hasPrimaryModifier = args.KeyModifiers.HasFlag(KeyModifiers.Control)
            || args.KeyModifiers.HasFlag(KeyModifiers.Meta);
        var hasShift = args.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var hasAlt = args.KeyModifiers.HasFlag(KeyModifiers.Alt);

        return args.Key == expectedKey
            && hasPrimaryModifier == requiresPrimaryModifier
            && hasShift == requiresShift
            && hasAlt == requiresAlt;
    }

    private static bool ShortcutBelongsToInputControl(object? source)
    {
        for (var current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is TextBox or ComboBox or IGraphEditorInputScope)
            {
                return true;
            }
        }

        return false;
    }
}
