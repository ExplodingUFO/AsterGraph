namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Selects the hosted workbench projection policy used by the stock Avalonia shell.
/// </summary>
public enum AsterGraphWorkbenchPerformanceMode
{
    /// <summary>
    /// Keeps the richest hosted projection for small graphs and evaluation screenshots.
    /// </summary>
    Quality,

    /// <summary>
    /// Keeps the stock workbench rich while applying bounded projection limits.
    /// </summary>
    Balanced,

    /// <summary>
    /// Applies tighter hosted projection limits for large authoring surfaces.
    /// </summary>
    Throughput,
}
