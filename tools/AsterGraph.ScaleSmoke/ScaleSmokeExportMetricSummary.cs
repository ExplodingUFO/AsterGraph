namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeExportMetricSummary(
    string TierId,
    int Samples,
    ScaleSmokePercentilePair Svg,
    ScaleSmokePercentilePair Png,
    ScaleSmokePercentilePair Jpeg,
    ScaleSmokePercentilePair Reload)
{
    public static ScaleSmokeExportMetricSummary FromSamples(
        string tierId,
        IReadOnlyList<ScaleSmokeExportMetrics> samples)
    {
        if (samples.Count == 0)
        {
            throw new ArgumentException("At least one export performance sample is required.", nameof(samples));
        }

        return new ScaleSmokeExportMetricSummary(
            tierId,
            samples.Count,
            Summarize(samples.Select(sample => sample.SvgExportMs)),
            Summarize(samples.Select(sample => sample.PngExportMs)),
            Summarize(samples.Select(sample => sample.JpegExportMs)),
            Summarize(samples.Select(sample => sample.ReloadMs)));
    }

    public string ToMarker()
    {
        return string.Join(
            ':',
            [
                $"SCALE_EXPORT_SUMMARY:{TierId}",
                $"samples={Samples}",
                $"svg-p50={Svg.P50}",
                $"svg-p95={Svg.P95}",
                $"png-p50={Png.P50}",
                $"png-p95={Png.P95}",
                $"jpeg-p50={Jpeg.P50}",
                $"jpeg-p95={Jpeg.P95}",
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
