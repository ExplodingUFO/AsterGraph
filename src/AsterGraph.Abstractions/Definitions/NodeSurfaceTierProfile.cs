namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Ordered size-driven surface tiers used to resolve how much of a node card should be shown.
/// </summary>
public sealed record NodeSurfaceTierProfile
{
    public NodeSurfaceTierProfile(IReadOnlyList<NodeSurfaceTierDefinition> tiers)
    {
        ArgumentNullException.ThrowIfNull(tiers);

        if (tiers.Count == 0)
        {
            throw new ArgumentException("At least one node-surface tier is required.", nameof(tiers));
        }

        var duplicateKeys = tiers
            .GroupBy(tier => tier.Key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        if (duplicateKeys.Count > 0)
        {
            throw new ArgumentException(
                $"Duplicate node-surface tier keys are not allowed: {string.Join(", ", duplicateKeys)}.",
                nameof(tiers));
        }

        Tiers = [.. tiers];
    }

    /// <summary>
    /// Canonical stock profile used when the host does not supply an override.
    /// </summary>
    public static NodeSurfaceTierProfile Default { get; } = new(
    [
        new NodeSurfaceTierDefinition("compact"),
        new NodeSurfaceTierDefinition(
            "details",
            minWidth: 220d,
            minHeight: 150d,
            visibleSectionKeys:
            [
                NodeSurfaceSectionKeys.Description,
            ]),
        new NodeSurfaceTierDefinition(
            "parameter-rail",
            minWidth: 320d,
            minHeight: 210d,
            visibleSectionKeys:
            [
                NodeSurfaceSectionKeys.Description,
                NodeSurfaceSectionKeys.ParameterRail,
            ]),
        new NodeSurfaceTierDefinition(
            "parameter-editors",
            minWidth: 420d,
            minHeight: 250d,
            visibleSectionKeys:
            [
                NodeSurfaceSectionKeys.Description,
                NodeSurfaceSectionKeys.ParameterRail,
                NodeSurfaceSectionKeys.ParameterEditors,
            ]),
    ]);

    public IReadOnlyList<NodeSurfaceTierDefinition> Tiers { get; }
}
