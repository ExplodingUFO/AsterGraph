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
        "DEMO_GESTURE_PROOF_OK",
        "COMPOSITE_SCOPE_OK",
        "EDGE_NOTE_OK",
        "EDGE_GEOMETRY_OK",
        "DISCONNECT_FLOW_OK",
        "SCENARIO_LAUNCH_OK",
        "SCENARIO_TOUR_OK",
        "AI_PIPELINE_MOCK_RUNNER_OK",
        "AI_PIPELINE_RUNTIME_OVERLAY_OK",
        "AI_PIPELINE_ERROR_STATE_OK",
        "AI_PIPELINE_MOCK_RUNNER_POLISH_OK",
        "AI_PIPELINE_PAYLOAD_PREVIEW_OK",
        "AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK",
        "WORKBENCH_COMMAND_RECOVERY_OK",
        "WORKBENCH_COMMAND_ACTION_GUIDANCE_OK",
        "WORKBENCH_COMMAND_SCOPE_BOUNDARY_OK",
        "DISCOVERY_TO_ACTION_FLOW_OK",
        "DISCOVERY_EMPTY_STATE_GUIDANCE_OK",
        "DISCOVERY_ACTION_SCOPE_BOUNDARY_OK",
        "WORKBENCH_EXPORT_AFFORDANCE_OK",
        "WORKBENCH_SHARE_EVIDENCE_OK",
        "WORKBENCH_EXPORT_SCOPE_BOUNDARY_OK",
        "WORKBENCH_FEATURE_DEPTH_HANDOFF_OK",
        "WORKBENCH_FEATURE_DEPTH_SCOPE_BOUNDARY_OK",
        "V065_MILESTONE_PROOF_OK",
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
            $"DEMO_GESTURE_PROOF_OK:{result.GestureProofOk}",
            $"COMPOSITE_SCOPE_OK:{result.CompositeScopeOk}",
            $"EDGE_NOTE_OK:{result.EdgeNoteOk}",
            $"EDGE_GEOMETRY_OK:{result.EdgeGeometryOk}",
            $"DISCONNECT_FLOW_OK:{result.DisconnectFlowOk}",
            $"DEMO_SCENARIO_PRESETS_OK:{result.DemoScenarioPresetsOk}",
            $"SCENARIO_LAUNCH_OK:{result.ScenarioLaunchOk}",
            $"SCENARIO_TOUR_OK:{result.ScenarioTourOk}",
            $"AI_PIPELINE_MOCK_RUNNER_OK:{result.AiPipelineMockRunnerOk}",
            $"AI_PIPELINE_RUNTIME_OVERLAY_OK:{result.AiPipelineRuntimeOverlayOk}",
            $"AI_PIPELINE_ERROR_STATE_OK:{result.AiPipelineErrorStateOk}",
            $"AI_PIPELINE_MOCK_RUNNER_POLISH_OK:{result.AiPipelineMockRunnerPolishOk}",
            $"AI_PIPELINE_PAYLOAD_PREVIEW_OK:{result.AiPipelinePayloadPreviewOk}",
            $"AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:{result.AiPipelineErrorDebugEvidenceOk}",
            $"WORKBENCH_COMMAND_RECOVERY_OK:{result.WorkbenchCommandRecoveryOk}",
            $"WORKBENCH_COMMAND_ACTION_GUIDANCE_OK:{result.WorkbenchCommandActionGuidanceOk}",
            $"WORKBENCH_COMMAND_SCOPE_BOUNDARY_OK:{result.WorkbenchCommandScopeBoundaryOk}",
            $"DISCOVERY_TO_ACTION_FLOW_OK:{result.DiscoveryToActionFlowOk}",
            $"DISCOVERY_EMPTY_STATE_GUIDANCE_OK:{result.DiscoveryEmptyStateGuidanceOk}",
            $"DISCOVERY_ACTION_SCOPE_BOUNDARY_OK:{result.DiscoveryActionScopeBoundaryOk}",
            $"WORKBENCH_EXPORT_AFFORDANCE_OK:{result.WorkbenchExportAffordanceOk}",
            $"WORKBENCH_SHARE_EVIDENCE_OK:{result.WorkbenchShareEvidenceOk}",
            $"WORKBENCH_EXPORT_SCOPE_BOUNDARY_OK:{result.WorkbenchExportScopeBoundaryOk}",
            $"WORKBENCH_FEATURE_DEPTH_HANDOFF_OK:{result.WorkbenchFeatureDepthHandoffOk}",
            $"WORKBENCH_FEATURE_DEPTH_SCOPE_BOUNDARY_OK:{result.WorkbenchFeatureDepthScopeBoundaryOk}",
            $"V065_MILESTONE_PROOF_OK:{result.V065MilestoneProofOk}",
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
