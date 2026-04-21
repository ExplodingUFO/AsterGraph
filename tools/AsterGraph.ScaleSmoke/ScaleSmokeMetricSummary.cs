namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokePercentilePair(
    long P50,
    long P95);

public sealed record ScaleSmokeMetricSummary(
    string TierId,
    int Samples,
    ScaleSmokePercentilePair Setup,
    ScaleSmokePercentilePair Selection,
    ScaleSmokePercentilePair Connection,
    ScaleSmokePercentilePair History,
    ScaleSmokePercentilePair Viewport,
    ScaleSmokePercentilePair Save,
    ScaleSmokePercentilePair Reload)
{
    public static ScaleSmokeMetricSummary FromSamples(
        string tierId,
        IReadOnlyList<ScaleSmokeMetrics> samples)
    {
        if (samples.Count == 0)
        {
            throw new ArgumentException("At least one performance sample is required.", nameof(samples));
        }

        return new ScaleSmokeMetricSummary(
            tierId,
            samples.Count,
            Summarize(samples.Select(sample => sample.SetupMs)),
            Summarize(samples.Select(sample => sample.SelectionMs)),
            Summarize(samples.Select(sample => sample.ConnectionMs)),
            Summarize(samples.Select(sample => sample.HistoryMs)),
            Summarize(samples.Select(sample => sample.ViewportMs)),
            Summarize(samples.Select(sample => sample.SaveMs)),
            Summarize(samples.Select(sample => sample.ReloadMs)));
    }

    public string ToMarker()
    {
        return string.Join(
            ':',
            [
                $"SCALE_PERF_SUMMARY:{TierId}",
                $"samples={Samples}",
                $"setup-p50={Setup.P50}",
                $"setup-p95={Setup.P95}",
                $"selection-p50={Selection.P50}",
                $"selection-p95={Selection.P95}",
                $"connection-p50={Connection.P50}",
                $"connection-p95={Connection.P95}",
                $"history-p50={History.P50}",
                $"history-p95={History.P95}",
                $"viewport-p50={Viewport.P50}",
                $"viewport-p95={Viewport.P95}",
                $"save-p50={Save.P50}",
                $"save-p95={Save.P95}",
                $"reload-p50={Reload.P50}",
                $"reload-p95={Reload.P95}",
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
