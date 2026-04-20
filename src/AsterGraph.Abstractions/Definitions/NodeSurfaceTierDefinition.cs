namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Describes one size-driven node-surface tier without binding it to any UI toolkit.
/// </summary>
public sealed record NodeSurfaceTierDefinition
{
    public NodeSurfaceTierDefinition(
        string key,
        double minWidth = 0d,
        double minHeight = 0d,
        IReadOnlyList<string>? visibleSectionKeys = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (minWidth < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(minWidth), "Tier minimum width cannot be negative.");
        }

        if (minHeight < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(minHeight), "Tier minimum height cannot be negative.");
        }

        Key = key.Trim();
        MinWidth = minWidth;
        MinHeight = minHeight;
        VisibleSectionKeys = NormalizeVisibleSectionKeys(visibleSectionKeys);
    }

    public string Key { get; }

    public double MinWidth { get; }

    public double MinHeight { get; }

    public IReadOnlyList<string> VisibleSectionKeys { get; }

    private static IReadOnlyList<string> NormalizeVisibleSectionKeys(IReadOnlyList<string>? visibleSectionKeys)
        => visibleSectionKeys is null
            ? []
            : visibleSectionKeys
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => key.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
}
