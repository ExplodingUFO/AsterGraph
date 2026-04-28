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
        Assert.Contains("CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_VALIDATION_FEEDBACK_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_FEEDBACK_FOCUS_TARGET_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_READINESS_STATUS_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_SNIPPET_CATALOG_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_SNIPPET_INSERT_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("FIVE_MINUTE_ONBOARDING_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("ONBOARDING_CONFIGURATION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:startup_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:inspector_projection_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:plugin_scan_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:command_latency_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:stencil_search_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:command_surface_refresh_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:node_tool_projection_ms", contents, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:edge_tool_projection_ms", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_OVERLAY_SNAPSHOT_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_OVERLAY_SNAPSHOT_POLISH_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_OVERLAY_SCOPE_FILTER_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_LOG_PANEL_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_LOG_FILTER_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_DEBUG_PANEL_INTERACTION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_LOG_LOCATE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_LOG_EXPORT_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_SEARCH_LOCATE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_SEARCH_SCOPE_FILTER_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("COMMAND_PALETTE_GROUPING_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("COMMAND_PALETTE_DISABLED_REASON_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("COMMAND_PALETTE_RECENT_ACTIONS_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("NAVIGATION_HISTORY_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("SCOPE_BREADCRUMB_NAVIGATION_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("FOCUS_RESTORE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("NAVIGATION_PRODUCTIVITY_PROOF_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("NAVIGATION_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("EXPERIENCE_POLISH_HANDOFF_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("FEATURE_ENHANCEMENT_PROOF_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_FLOW_PROOF_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_FLOW_HANDOFF_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("EXPERIENCE_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", contents, StringComparison.Ordinal);
        Assert.Contains("parameterSnapshots", contents, StringComparison.Ordinal);
        Assert.Contains("readinessStatus", contents, StringComparison.Ordinal);
        Assert.Contains("validationSummary", contents, StringComparison.Ordinal);
        Assert.Contains("validationFeedback", contents, StringComparison.Ordinal);
        Assert.Contains("currentValue", contents, StringComparison.Ordinal);
        Assert.Contains("defaultValue", contents, StringComparison.Ordinal);
        Assert.Contains("allowedOptions", contents, StringComparison.Ordinal);
    }
}
