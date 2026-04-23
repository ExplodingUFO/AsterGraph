namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeAuthoringMetricSummary(
    string TierId,
    int Samples,
    ScaleSmokePercentilePair Stencil,
    ScaleSmokePercentilePair CommandSurface,
    ScaleSmokePercentilePair QuickToolProjection,
    ScaleSmokePercentilePair QuickToolExecution)
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
            Summarize(samples.Select(sample => sample.QuickToolExecutionMs)));
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
