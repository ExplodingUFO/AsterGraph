namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Detects duplicate effective shortcuts in host-facing action projections.
/// </summary>
public static class AsterGraphHostedActionShortcutConflictDetector
{
    public static IReadOnlyList<AsterGraphHostedActionShortcutConflict> FindConflicts(
        IEnumerable<AsterGraphHostedActionDescriptor> actions,
        IEnumerable<string>? excludedActionIds = null)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var excludedIds = excludedActionIds?
            .Where(actionId => !string.IsNullOrWhiteSpace(actionId))
            .ToHashSet(StringComparer.Ordinal);
        var groups = new Dictionary<string, ShortcutConflictBuilder>(StringComparer.OrdinalIgnoreCase);
        foreach (var action in actions)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (excludedIds?.Contains(action.Id) == true)
            {
                continue;
            }

            var shortcut = NormalizeShortcut(action.DefaultShortcut);
            if (shortcut is null)
            {
                continue;
            }

            if (!groups.TryGetValue(shortcut, out var builder))
            {
                builder = new ShortcutConflictBuilder(shortcut);
                groups.Add(shortcut, builder);
            }

            builder.Add(action);
        }

        return groups.Values
            .Where(group => group.ActionIds.Count > 1)
            .Select(group => group.Build())
            .ToList();
    }

    private static string? NormalizeShortcut(string? shortcut)
        => string.IsNullOrWhiteSpace(shortcut) ? null : shortcut.Trim();

    private sealed class ShortcutConflictBuilder
    {
        public ShortcutConflictBuilder(string shortcut)
        {
            Shortcut = shortcut;
        }

        public string Shortcut { get; }

        public List<string> ActionIds { get; } = [];

        public List<string> CommandIds { get; } = [];

        public void Add(AsterGraphHostedActionDescriptor action)
        {
            ActionIds.Add(action.Id);
            if (!string.IsNullOrWhiteSpace(action.CommandId))
            {
                CommandIds.Add(action.CommandId);
            }
        }

        public AsterGraphHostedActionShortcutConflict Build()
            => new(Shortcut, ActionIds, CommandIds.Distinct(StringComparer.Ordinal).ToList());
    }
}

public sealed record AsterGraphHostedActionShortcutConflict(
    string Shortcut,
    IReadOnlyList<string> ActionIds,
    IReadOnlyList<string> CommandIds);
