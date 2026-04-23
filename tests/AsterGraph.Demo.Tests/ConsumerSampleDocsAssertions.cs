using Xunit;

namespace AsterGraph.Demo.Tests;

internal static class ConsumerSampleDocsAssertions
{
    internal static void AssertSupportBundleProofMarkers(string contents)
    {
        Assert.Contains("CONSUMER_SAMPLE_HOST_ACTION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PLUGIN_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_SURFACE_METADATA_PROJECTION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_SURFACE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_PARAMETER_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_WINDOW_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_TRUST_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CAPABILITY_BREADTH_STENCIL_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CAPABILITY_BREADTH_EXPORT_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CAPABILITY_BREADTH_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_FOCUS_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("WIDENED_SURFACE_PERFORMANCE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:startup_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:inspector_projection_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:plugin_scan_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:command_latency_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:stencil_search_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:command_surface_refresh_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:node_tool_projection_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:edge_tool_projection_ms", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", contents, StringComparison.Ordinal);
        Assert.Contains("currentValue", contents, StringComparison.Ordinal);
        Assert.Contains("defaultValue", contents, StringComparison.Ordinal);
        Assert.Contains("allowedOptions", contents, StringComparison.Ordinal);
    }
}
