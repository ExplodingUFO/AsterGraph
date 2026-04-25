namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeAuthoringMetricSummary(
    string TierId,
    int Samples,
    ScaleSmokePercentilePair Stencil,
    ScaleSmokePercentilePair CommandSurface,
    ScaleSmokePercentilePair QuickToolProjection,
    ScaleSmokePercentilePair QuickToolExecution,
    ScaleSmokePercentilePair InspectorOpen,
    ScaleSmokePercentilePair NodeResize,
    ScaleSmokePercentilePair EdgeCreate)
{
    public static ScaleSmokeAuthoringMetricSummary FromSamples(
        string tierId,
        IReadOnlyList<ScaleSmokeAuthoringMetrics> samples)
    {
        if (samples.Count == 0)
        {
            throw new ArgumentException("At least one authoring performance sample is required.", nameof(samples));
        }

        return new ScaleSmokeAuthoringMetricSummary(
            tierId,
            samples.Count,
            Summarize(samples.Select(sample => sample.StencilFilterMs)),
            Summarize(samples.Select(sample => sample.CommandSurfaceRefreshMs)),
            Summarize(samples.Select(sample => sample.QuickToolProjectionMs)),
            Summarize(samples.Select(sample => sample.QuickToolExecutionMs)),
            Summarize(samples.Select(sample => sample.InspectorOpenMs)),
            Summarize(samples.Select(sample => sample.NodeResizeMs)),
            Summarize(samples.Select(sample => sample.EdgeCreateMs)));
    }

    public string ToMarker()
    {
        return string.Join(
            ':',
            [
                $"SCALE_AUTHORING_SUMMARY:{TierId}",
                $"samples={Samples}",
                $"stencil-p50={Stencil.P50}",
                $"stencil-p95={Stencil.P95}",
                $"command-surface-p50={CommandSurface.P50}",
                $"command-surface-p95={CommandSurface.P95}",
                $"quick-tool-projection-p50={QuickToolProjection.P50}",
                $"quick-tool-projection-p95={QuickToolProjection.P95}",
                $"quick-tool-execution-p50={QuickToolExecution.P50}",
                $"quick-tool-execution-p95={QuickToolExecution.P95}",
                $"inspector-open-p50={InspectorOpen.P50}",
                $"inspector-open-p95={InspectorOpen.P95}",
                $"node-resize-p50={NodeResize.P50}",
                $"node-resize-p95={NodeResize.P95}",
                $"edge-create-p50={EdgeCreate.P50}",
                $"edge-create-p95={EdgeCreate.P95}",
            ]);
    }

    private static ScaleSmokePercentilePair Summarize(IEnumerable<long> samples)
    {
        var ordered = samples.OrderBy(value => value).ToArray();

        return new ScaleSmokePercentilePair(
            Percentile(ordered, 0.50),
            Percentile(ordered, 0.95));
    }

    private static long Percentile(
        IReadOnlyList<long> ordered,
        double percentile)
    {
        var rank = (int)Math.Ceiling(percentile * ordered.Count);
        var index = Math.Clamp(rank - 1, 0, ordered.Count - 1);
        return ordered[index];
    }
}
