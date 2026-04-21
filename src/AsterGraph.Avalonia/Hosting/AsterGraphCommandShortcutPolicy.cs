namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Controls how the stock Avalonia adapter routes command shortcuts.
/// </summary>
public sealed record AsterGraphCommandShortcutPolicy
{
    private static readonly IReadOnlyDictionary<string, string?> EmptyOverrides =
        new Dictionary<string, string?>(StringComparer.Ordinal);

    public static AsterGraphCommandShortcutPolicy Default { get; } = new();

    public static AsterGraphCommandShortcutPolicy Disabled { get; } = new()
    {
        Enabled = false,
    };

    /// <summary>
    /// Enables or disables stock shortcut routing entirely.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Overrides effective shortcut text by action or command id. Null or whitespace disables that route.
    /// </summary>
    public IReadOnlyDictionary<string, string?> ShortcutOverrides { get; init; } = EmptyOverrides;

    public string? ResolveShortcut(string actionId, string? defaultShortcut)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);

        if (!Enabled)
        {
            return null;
        }

        if (ShortcutOverrides.TryGetValue(actionId, out var overrideShortcut))
        {
            return Normalize(overrideShortcut);
        }

        return Normalize(defaultShortcut);
    }

    private static string? Normalize(string? shortcutText)
        => string.IsNullOrWhiteSpace(shortcutText) ? null : shortcutText.Trim();
}
