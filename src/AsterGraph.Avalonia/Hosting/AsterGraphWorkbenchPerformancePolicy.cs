namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Describes hosted workbench projection limits derived from <see cref="AsterGraphWorkbenchPerformanceMode" />.
/// </summary>
public sealed record AsterGraphWorkbenchPerformancePolicy(
    AsterGraphWorkbenchPerformanceMode Mode,
    int StencilCardsPerSectionLimit,
    bool ProjectMiniMapContinuously,
    bool ProjectAdvancedInspectorByDefault,
    bool ProjectHoveredToolbars,
    int CommandRefreshBatchMilliseconds)
{
    /// <summary>
    /// Human-readable minimap viewport refresh cadence implied by this policy.
    /// </summary>
    public string MiniMapRefreshCadence
        => ProjectMiniMapContinuously
            ? "viewport-continuous"
            : "document-selection-cached";

    /// <summary>
    /// Machine-readable minimap budget marker for host proof output.
    /// </summary>
    public string ToMiniMapBudgetMarker()
        => $"MINIMAP_CADENCE:{Mode}:cadence={MiniMapRefreshCadence}:commandRefreshMs={CommandRefreshBatchMilliseconds}";

    /// <summary>
    /// Creates the stock hosted projection policy for a performance mode.
    /// </summary>
    public static AsterGraphWorkbenchPerformancePolicy FromMode(AsterGraphWorkbenchPerformanceMode mode)
        => mode switch
        {
            AsterGraphWorkbenchPerformanceMode.Quality => new(
                mode,
                StencilCardsPerSectionLimit: int.MaxValue,
                ProjectMiniMapContinuously: true,
                ProjectAdvancedInspectorByDefault: true,
                ProjectHoveredToolbars: true,
                CommandRefreshBatchMilliseconds: 0),
            AsterGraphWorkbenchPerformanceMode.Throughput => new(
                mode,
                StencilCardsPerSectionLimit: 48,
                ProjectMiniMapContinuously: false,
                ProjectAdvancedInspectorByDefault: false,
                ProjectHoveredToolbars: false,
                CommandRefreshBatchMilliseconds: 50),
            _ => new(
                AsterGraphWorkbenchPerformanceMode.Balanced,
                StencilCardsPerSectionLimit: 128,
                ProjectMiniMapContinuously: true,
                ProjectAdvancedInspectorByDefault: false,
                ProjectHoveredToolbars: true,
                CommandRefreshBatchMilliseconds: 16),
        };
}
