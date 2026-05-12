using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Viewport;

/// <summary>
/// Captures repeatable viewport-budgeted renderer proof metadata.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class RendererVirtualizationProofHarness
{
    /// <summary>
    /// Captures proof metadata for the current viewport-budgeted scene projection boundary.
    /// </summary>
    public static RendererVirtualizationProofArtifact Capture(
        GraphDocument document,
        GraphEditorViewportSnapshot viewport,
        GraphEditorViewportSnapshot invalidationViewport,
        double overscanWorldUnits = ViewportVisibleSceneProjector.DefaultOverscanWorldUnits)
    {
        ArgumentNullException.ThrowIfNull(document);

        var projectionStopwatch = Stopwatch.StartNew();
        var projection = ViewportVisibleSceneProjector.Project(document, viewport, overscanWorldUnits);
        projectionStopwatch.Stop();

        var invalidationStopwatch = Stopwatch.StartNew();
        var invalidationProjection = ViewportVisibleSceneProjector.Project(document, invalidationViewport, overscanWorldUnits);
        var invalidation = projection.Diff(invalidationProjection);
        invalidationStopwatch.Stop();

        return new RendererVirtualizationProofArtifact(
            "514",
            "viewport-budgeted-scene-projection",
            new RendererVirtualizationGraphSize(
                projection.TotalNodes,
                projection.TotalConnections,
                projection.TotalGroups),
            new RendererVirtualizationViewport(
                viewport.ViewportWidth,
                viewport.ViewportHeight,
                viewport.PanX,
                viewport.PanY),
            viewport.Zoom,
            projection.OverscanWorldUnits,
            new RendererVirtualizationVisibleVisualCounts(
                projection.VisibleNodes,
                projection.TotalNodes,
                projection.VisibleConnections,
                projection.TotalConnections,
                projection.VisibleGroups,
                projection.TotalGroups),
            new RendererVirtualizationInvalidationCounts(
                invalidation.NodesLeavingViewport.Count + invalidation.NodesEnteringViewport.Count,
                invalidation.ConnectionsLeavingViewport.Count + invalidation.ConnectionsEnteringViewport.Count,
                invalidation.GroupsLeavingViewport.Count + invalidation.GroupsEnteringViewport.Count,
                invalidation.InvalidatedSceneItemCount),
            new RendererVirtualizationMeasuredTimings(
                projectionStopwatch.ElapsedTicks,
                invalidationStopwatch.ElapsedTicks),
            AvoidsFullCollectionScan: false,
            AvoidsFullSceneRebuild: false);
    }
}

/// <summary>
/// Repeatable renderer proof metadata for the current viewport-budgeted boundary.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed record RendererVirtualizationProofArtifact(
    string Phase,
    string ClaimBoundary,
    RendererVirtualizationGraphSize GraphSize,
    RendererVirtualizationViewport Viewport,
    double Zoom,
    double OverscanWorldUnits,
    RendererVirtualizationVisibleVisualCounts VisibleVisualCounts,
    RendererVirtualizationInvalidationCounts InvalidationCounts,
    RendererVirtualizationMeasuredTimings MeasuredTimings,
    bool AvoidsFullCollectionScan,
    bool AvoidsFullSceneRebuild)
{
    /// <summary>
    /// Creates a stable machine-readable proof artifact marker.
    /// </summary>
    public string ToMetadataMarker()
        => "RENDERER_VIRTUALIZATION_PROOF_ARTIFACT:"
           + $"phase={Phase}:"
           + $"claimBoundary={ClaimBoundary}:"
           + $"graphSize=nodes:{GraphSize.TotalNodes},connections:{GraphSize.TotalConnections},groups:{GraphSize.TotalGroups}:"
           + $"viewport=width:{Format(Viewport.Width)},height:{Format(Viewport.Height)},panX:{Format(Viewport.PanX)},panY:{Format(Viewport.PanY)}:"
           + $"zoom={Format(Zoom)}:"
           + $"overscan={Format(OverscanWorldUnits)}:"
           + "visibleVisualCounts="
           + $"nodes:{VisibleVisualCounts.VisibleNodes}/{VisibleVisualCounts.TotalNodes},"
           + $"connections:{VisibleVisualCounts.VisibleConnections}/{VisibleVisualCounts.TotalConnections},"
           + $"groups:{VisibleVisualCounts.VisibleGroups}/{VisibleVisualCounts.TotalGroups}:"
           + "invalidationCounts="
           + $"nodes:{InvalidationCounts.Nodes},"
           + $"connections:{InvalidationCounts.Connections},"
           + $"groups:{InvalidationCounts.Groups},"
           + $"total:{InvalidationCounts.Total}:"
           + "measuredTimings="
           + $"projectionTicks:{MeasuredTimings.ProjectionElapsedTicks},"
           + $"invalidationTicks:{MeasuredTimings.InvalidationElapsedTicks}:"
           + $"avoidsFullCollectionScan={Format(AvoidsFullCollectionScan)}:"
           + $"avoidsFullSceneRebuild={Format(AvoidsFullSceneRebuild)}";

    private static string Format(double value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Format(bool value)
        => value ? "true" : "false";
}

/// <summary>
/// Graph-size metadata included in renderer proof artifacts.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal readonly record struct RendererVirtualizationGraphSize(
    int TotalNodes,
    int TotalConnections,
    int TotalGroups);

/// <summary>
/// Viewport metadata included in renderer proof artifacts.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal readonly record struct RendererVirtualizationViewport(
    double Width,
    double Height,
    double PanX,
    double PanY);

/// <summary>
/// Visible-scene visual counts included in renderer proof artifacts.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal readonly record struct RendererVirtualizationVisibleVisualCounts(
    int VisibleNodes,
    int TotalNodes,
    int VisibleConnections,
    int TotalConnections,
    int VisibleGroups,
    int TotalGroups)
{
    /// <summary>
    /// Number of visible node visuals in the proof viewport.
    /// </summary>
    public int Nodes => VisibleNodes;

    /// <summary>
    /// Number of visible connection visuals in the proof viewport.
    /// </summary>
    public int Connections => VisibleConnections;

    /// <summary>
    /// Number of visible group visuals in the proof viewport.
    /// </summary>
    public int Groups => VisibleGroups;
}

/// <summary>
/// Scene invalidation counts included in renderer proof artifacts.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal readonly record struct RendererVirtualizationInvalidationCounts(
    int Nodes,
    int Connections,
    int Groups,
    int Total);

/// <summary>
/// Measured timing metadata included in renderer proof artifacts.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal readonly record struct RendererVirtualizationMeasuredTimings(
    long ProjectionElapsedTicks,
    long InvalidationElapsedTicks);
