using Xunit;

namespace AsterGraph.Demo.Tests;

internal static class ConsumerSampleDocsAssertions
{
    internal static void AssertSupportBundleProofMarkers(string contents)
    {
        Assert.Contains("CONSUMER_SAMPLE_HOST_ACTION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PLUGIN_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_WINDOW_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_TRUST_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:startup_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:inspector_projection_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:plugin_scan_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:command_latency_ms", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", contents, StringComparison.Ordinal);
    }
}
