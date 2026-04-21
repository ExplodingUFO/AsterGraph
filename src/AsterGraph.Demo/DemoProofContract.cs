using System.Globalization;

namespace AsterGraph.Demo;

public static class DemoProofContract
{
    public static IReadOnlyList<string> PublicSuccessMarkerIds { get; } =
    [
        "COMMAND_SURFACE_OK",
        "TIERED_NODE_SURFACE_OK",
        "FIXED_GROUP_FRAME_OK",
        "NON_OBSCURING_EDITING_OK",
        "VISUAL_SEMANTICS_OK",
        "HIERARCHY_SEMANTICS_OK",
        "CUSTOM_TEMPLATE_OK",
        "TOOL_PROVIDER_OK",
        "NATIVE_INTERACTION_A11Y_OK",
        "COMPOSITE_SCOPE_OK",
        "EDGE_NOTE_OK",
        "EDGE_GEOMETRY_OK",
        "DISCONNECT_FLOW_OK",
    ];

    public static IReadOnlyList<string> NativeMetricNames { get; } =
    [
        "startup_ms",
        "inspector_projection_ms",
        "plugin_scan_ms",
        "command_latency_ms",
    ];

    public static IReadOnlyList<string> CreatePublicSuccessMarkerLines()
        => PublicSuccessMarkerIds
            .Select(markerId => $"{markerId}:True")
            .ToArray();

    internal static IReadOnlyList<string> CreateProofLines(DemoProofResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return
        [
            $"DEMO_TRUST_OK:{result.TrustTransparencyOk}",
            $"DEMO_SHELL_OK:{result.ShellWorkflowOk}",
            $"COMMAND_SURFACE_OK:{result.CommandSurfaceOk}",
            $"TIERED_NODE_SURFACE_OK:{result.TieredNodeSurfaceOk}",
            $"FIXED_GROUP_FRAME_OK:{result.FixedGroupFrameOk}",
            $"NON_OBSCURING_EDITING_OK:{result.NonObscuringEditingOk}",
            $"VISUAL_SEMANTICS_OK:{result.VisualSemanticsOk}",
            $"HIERARCHY_SEMANTICS_OK:{result.HierarchySemanticsOk}",
            $"CUSTOM_TEMPLATE_OK:{result.CustomTemplateOk}",
            $"TOOL_PROVIDER_OK:{result.ToolProviderOk}",
            $"NATIVE_INTERACTION_A11Y_OK:{result.NativeInteractionAccessibilityOk}",
            $"COMPOSITE_SCOPE_OK:{result.CompositeScopeOk}",
            $"EDGE_NOTE_OK:{result.EdgeNoteOk}",
            $"EDGE_GEOMETRY_OK:{result.EdgeGeometryOk}",
            $"DISCONNECT_FLOW_OK:{result.DisconnectFlowOk}",
        ];
    }

    internal static IReadOnlyList<string> CreateMetricLines(DemoProofResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return
        [
            FormatMetric("startup_ms", result.StartupMs),
            FormatMetric("inspector_projection_ms", result.InspectorProjectionMs),
            FormatMetric("plugin_scan_ms", result.PluginScanMs),
            FormatMetric("command_latency_ms", result.CommandLatencyMs),
        ];
    }

    private static string FormatMetric(string name, double value)
        => $"HOST_NATIVE_METRIC:{name}={value.ToString("0.###", CultureInfo.InvariantCulture)}";
}
