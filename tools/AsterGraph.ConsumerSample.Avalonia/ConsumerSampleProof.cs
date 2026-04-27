using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;

namespace AsterGraph.ConsumerSample;

public sealed record ConsumerSampleProofResult(
    bool HostMenuActionOk,
    bool PluginContributionOk,
    bool ParameterProjectionOk,
    bool MetadataProjectionOk,
    bool NodeSideAuthoringOk,
    bool WindowCompositionOk,
    bool TrustTransparencyOk,
    bool CommandSurfaceOk,
    bool StencilSurfaceOk,
    bool ExportBreadthOk,
    bool NodeQuickToolsOk,
    bool EdgeQuickToolsOk,
    bool HostedAccessibilityBaselineOk,
    bool HostedAccessibilityFocusOk,
    bool HostedAccessibilityCommandSurfaceOk,
    bool HostedAccessibilityAuthoringSurfaceOk,
    IReadOnlyList<ConsumerSampleProofParameterSnapshot> ParameterSnapshots,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs,
    double StencilSearchMs,
    double CommandSurfaceRefreshMs,
    double NodeToolProjectionMs,
    double EdgeToolProjectionMs,
    double CommandPaletteMs,
    bool QuickAddConnectedNodeOk = true,
    bool PortFilteredNodeSearchOk = true,
    bool DropNodeOnEdgeOk = true,
    bool EdgeSplitCompatibilityOk = true,
    bool EdgeSplitUndoOk = true,
    bool DeleteAndReconnectOk = true,
    bool DetachNodeOk = true,
    bool ReconnectConflictReportOk = true,
    bool EdgeMultiSelectOk = true,
    bool WireSliceOk = true,
    bool SelectedNodeEdgeHighlightOk = true,
    bool RuntimeOverlaySnapshotOk = true,
    bool RuntimeLogPanelOk = true,
    bool RuntimeLogFilterOk = true,
    bool RuntimeOverlaySupportBundleOk = true,
    bool LayoutProviderSeamOk = true,
    bool LayoutPreviewApplyCancelOk = true,
    bool LayoutUndoTransactionOk = true,
    int NodeCount = 0,
    int ConnectionCount = 0,
    IReadOnlyList<string>? FeatureDescriptorIds = null,
    IReadOnlyList<string>? RecentDiagnosticCodes = null,
    IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot>? RuntimeNodeOverlays = null,
    IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot>? RuntimeConnectionOverlays = null,
    IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot>? RuntimeLogs = null,
    bool HostedAutomationNavigationOk = true,
    bool HostedAuthoringAutomationDiagnosticsOk = true,
    bool ScenarioGraphOk = true,
    bool HostOwnedActionsOk = true,
    bool SupportBundlePayloadOk = true,
    bool FiveMinuteOnboardingOk = true)
{
    public bool AuthoringSurfaceOk
        => ParameterProjectionOk
        && MetadataProjectionOk
        && InspectorMetadataPolishOk
        && InspectorMixedValueOk
        && InspectorValidationFixOk
        && SupportBundleParameterEvidenceOk
        && QuickAddConnectedNodeOk
        && PortFilteredNodeSearchOk
        && DropNodeOnEdgeOk
        && EdgeSplitCompatibilityOk
        && EdgeSplitUndoOk
        && DeleteAndReconnectOk
        && DetachNodeOk
        && ReconnectConflictReportOk
        && EdgeMultiSelectOk
        && WireSliceOk
        && SelectedNodeEdgeHighlightOk
        && NodeSideAuthoringOk
        && CommandSurfaceOk;

    public bool CapabilityBreadthOk
        => StencilSurfaceOk
        && ExportBreadthOk
        && NodeQuickToolsOk
        && EdgeQuickToolsOk;

    public bool WidenedSurfacePerformanceOk
        => CommandSurfaceOk
        && CapabilityBreadthOk
        && StencilSearchMs >= 0
        && CommandSurfaceRefreshMs >= 0
        && NodeToolProjectionMs >= 0
        && EdgeToolProjectionMs >= 0;

    public bool HostedAccessibilityAutomationOk
        => HostedAccessibilityFocusOk
        && HostedAutomationNavigationOk
        && HostedAuthoringAutomationDiagnosticsOk
        && HostedAccessibilityCommandSurfaceOk
        && HostedAccessibilityAuthoringSurfaceOk;

    public bool HostedAccessibilityOk
        => HostedAccessibilityBaselineOk
        && HostedAccessibilityAutomationOk;

    public bool OnboardingConfigurationOk
        => ScenarioGraphOk
        && HostOwnedActionsOk
        && SupportBundlePayloadOk
        && FiveMinuteOnboardingOk
        && RuntimeOverlaySnapshotOk
        && RuntimeLogPanelOk
        && RuntimeLogFilterOk
        && RuntimeOverlaySupportBundleOk
        && LayoutProviderSeamOk
        && LayoutPreviewApplyCancelOk
        && LayoutUndoTransactionOk
        && NodeCount > 0
        && (FeatureDescriptorIds?.Count > 0);

    public bool IsOk
        => HostMenuActionOk
        && PluginContributionOk
        && TrustTransparencyOk
        && WindowCompositionOk
        && AuthoringSurfaceOk
        && CapabilityBreadthOk
        && HostedAccessibilityOk
        && WidenedSurfacePerformanceOk
        && OnboardingConfigurationOk;

    public IReadOnlyList<string> MetricLines =>
    [
        FormatMetric("startup_ms", StartupMs),
        FormatMetric("inspector_projection_ms", InspectorProjectionMs),
        FormatMetric("plugin_scan_ms", PluginScanMs),
        FormatMetric("command_latency_ms", CommandLatencyMs),
        FormatMetric("stencil_search_ms", StencilSearchMs),
        FormatMetric("command_surface_refresh_ms", CommandSurfaceRefreshMs),
        FormatMetric("node_tool_projection_ms", NodeToolProjectionMs),
        FormatMetric("edge_tool_projection_ms", EdgeToolProjectionMs),
        FormatMetric("command_palette_ms", CommandPaletteMs),
    ];

    internal IReadOnlyList<string> ProofLines =>
    [
        $"CONSUMER_SAMPLE_HOST_ACTION_OK:{HostMenuActionOk}",
        $"CONSUMER_SAMPLE_PLUGIN_OK:{PluginContributionOk}",
        $"AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:{ParameterProjectionOk}",
        $"AUTHORING_SURFACE_METADATA_PROJECTION_OK:{MetadataProjectionOk}",
        $"INSPECTOR_METADATA_POLISH_OK:{InspectorMetadataPolishOk}",
        $"INSPECTOR_MIXED_VALUE_OK:{InspectorMixedValueOk}",
        $"INSPECTOR_VALIDATION_FIX_OK:{InspectorValidationFixOk}",
        $"SUPPORT_BUNDLE_PARAMETER_EVIDENCE_OK:{SupportBundleParameterEvidenceOk}",
        $"AUTHORING_QUICK_ADD_CONNECTED_NODE_OK:{QuickAddConnectedNodeOk}",
        $"AUTHORING_PORT_FILTERED_NODE_SEARCH_OK:{PortFilteredNodeSearchOk}",
        $"AUTHORING_DROP_NODE_ON_EDGE_OK:{DropNodeOnEdgeOk}",
        $"AUTHORING_EDGE_SPLIT_COMPATIBILITY_OK:{EdgeSplitCompatibilityOk}",
        $"AUTHORING_EDGE_SPLIT_UNDO_OK:{EdgeSplitUndoOk}",
        $"AUTHORING_DELETE_AND_RECONNECT_OK:{DeleteAndReconnectOk}",
        $"AUTHORING_DETACH_NODE_OK:{DetachNodeOk}",
        $"AUTHORING_RECONNECT_CONFLICT_REPORT_OK:{ReconnectConflictReportOk}",
        $"AUTHORING_EDGE_MULTISELECT_OK:{EdgeMultiSelectOk}",
        $"AUTHORING_WIRE_SLICE_OK:{WireSliceOk}",
        $"AUTHORING_SELECTED_NODE_EDGE_HIGHLIGHT_OK:{SelectedNodeEdgeHighlightOk}",
        $"RUNTIME_OVERLAY_SNAPSHOT_OK:{RuntimeOverlaySnapshotOk}",
        $"RUNTIME_LOG_PANEL_OK:{RuntimeLogPanelOk}",
        $"RUNTIME_LOG_FILTER_OK:{RuntimeLogFilterOk}",
        $"RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:{RuntimeOverlaySupportBundleOk}",
        $"LAYOUT_PROVIDER_SEAM_OK:{LayoutProviderSeamOk}",
        $"LAYOUT_PREVIEW_APPLY_CANCEL_OK:{LayoutPreviewApplyCancelOk}",
        $"LAYOUT_UNDO_TRANSACTION_OK:{LayoutUndoTransactionOk}",
        $"AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:{NodeSideAuthoringOk}",
        $"AUTHORING_SURFACE_COMMAND_PROJECTION_OK:{CommandSurfaceOk}",
        $"CONSUMER_SAMPLE_PARAMETER_OK:{ParameterProjectionOk}",
        $"CONSUMER_SAMPLE_METADATA_PROJECTION_OK:{MetadataProjectionOk}",
        $"CONSUMER_SAMPLE_WINDOW_OK:{WindowCompositionOk}",
        $"CONSUMER_SAMPLE_TRUST_OK:{TrustTransparencyOk}",
        $"COMMAND_SURFACE_OK:{CommandSurfaceOk}",
        $"CAPABILITY_BREADTH_STENCIL_OK:{StencilSurfaceOk}",
        $"CAPABILITY_BREADTH_EXPORT_OK:{ExportBreadthOk}",
        $"CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:{NodeQuickToolsOk}",
        $"CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:{EdgeQuickToolsOk}",
        $"CAPABILITY_BREADTH_OK:{CapabilityBreadthOk}",
        $"HOSTED_ACCESSIBILITY_BASELINE_OK:{HostedAccessibilityBaselineOk}",
        $"HOSTED_ACCESSIBILITY_FOCUS_OK:{HostedAccessibilityFocusOk}",
        $"HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:{HostedAutomationNavigationOk}",
        $"HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:{HostedAuthoringAutomationDiagnosticsOk}",
        $"HOSTED_ACCESSIBILITY_AUTOMATION_OK:{HostedAccessibilityAutomationOk}",
        $"HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:{HostedAccessibilityCommandSurfaceOk}",
        $"HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:{HostedAccessibilityAuthoringSurfaceOk}",
        $"HOSTED_ACCESSIBILITY_OK:{HostedAccessibilityOk}",
        $"WIDENED_SURFACE_PERFORMANCE_OK:{WidenedSurfacePerformanceOk}",
        .. MetricLines,
        $"CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:{ScenarioGraphOk}",
        $"CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:{HostOwnedActionsOk}",
        $"CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:{SupportBundlePayloadOk}",
        $"FIVE_MINUTE_ONBOARDING_OK:{FiveMinuteOnboardingOk}",
        $"ONBOARDING_CONFIGURATION_OK:{OnboardingConfigurationOk}",
        $"AUTHORING_SURFACE_OK:{AuthoringSurfaceOk}",
        $"CONSUMER_SAMPLE_OK:{IsOk}",
    ];

    public bool InspectorMetadataPolishOk
        => ParameterSnapshots.Any(snapshot =>
            snapshot.SupportsEnumSearch
            && snapshot.AllowedOptions.Count > 0)
        && ParameterSnapshots.Any(snapshot =>
            !string.IsNullOrWhiteSpace(snapshot.NumberSliderHint))
        && ParameterSnapshots.Any(snapshot =>
            snapshot.UsesMultilineTextInput
            && snapshot.IsCodeLikeText);

    public bool InspectorMixedValueOk
        => ParameterSnapshots.Any(snapshot => snapshot.HasMixedValues);

    public bool InspectorValidationFixOk
        => ParameterSnapshots.Any(snapshot => snapshot.CanApplyValidationFix);

    public bool SupportBundleParameterEvidenceOk
        => ParameterSnapshots.Any(snapshot =>
            !string.IsNullOrWhiteSpace(snapshot.GroupName)
            && !string.IsNullOrWhiteSpace(snapshot.HelpText)
            && !string.IsNullOrWhiteSpace(snapshot.ValueDisplayText))
        && ParameterSnapshots.Any(snapshot =>
            snapshot.ValidationMessage is not null
            || snapshot.CanApplyValidationFix)
        && ParameterSnapshots.Any(snapshot =>
            snapshot.HasMixedValues);

    private static string FormatMetric(string name, double value)
        => $"HOST_NATIVE_METRIC:{name}={value.ToString("0.###", CultureInfo.InvariantCulture)}";
}

public static class ConsumerSampleProof
{
    public static ConsumerSampleProofResult Run()
    {
        ConsumerSampleHeadlessEnvironment.EnsureInitialized();

        var storageRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Proof", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        ConsumerSampleHost? createdHost = null;
        var startupMs = MeasureMilliseconds(() => createdHost = ConsumerSampleHost.Create(storageRoot));
        using var host = createdHost ?? throw new InvalidOperationException("Consumer sample host was not created.");
        var window = ConsumerSampleWindowFactory.Create(host);

        GraphEditorNodeParameterSnapshot[] selectedParameterSnapshots = [];
        bool metadataProjectionOk;
        bool nodeSideAuthoringOk;
        bool commandSurfaceOk;
        bool hostMenuActionOk;
        bool pluginContributionOk;
        bool parameterProjectionOk;
        bool trustTransparencyOk;
        bool windowCompositionOk;
        bool scenarioGraphOk;
        bool hostOwnedActionsOk;
        bool runtimeLogPanelOk;
        bool runtimeLogFilterOk;
        bool layoutProviderSeamOk;
        bool layoutPreviewApplyCancelOk;
        bool layoutUndoTransactionOk;
        double inspectorProjectionMs;
        double pluginScanMs;
        double commandLatencyMs;
        double stencilSearchMs;
        double commandSurfaceRefreshMs;
        double nodeToolProjectionMs;
        double edgeToolProjectionMs;
        double commandPaletteMs;

        try
        {
            window.Show();
            FlushUi();

            scenarioGraphOk = HasScenarioGraph(host);
            hostOwnedActionsOk = HasHostOwnedActions(host);
            host.SelectNode(host.GetFirstReviewNodeId());
            FlushUi();
            inspectorProjectionMs = MeasureMilliseconds(() => selectedParameterSnapshots = host.GetSelectedParameterSnapshots().ToArray());
            pluginScanMs = MeasureMilliseconds(() => host.PluginCandidates.ToArray());
            metadataProjectionOk = HasMetadataProjection(selectedParameterSnapshots);
            nodeSideAuthoringOk = HasNodeSideAuthoring(window, host, host.GetFirstReviewNodeId());

            var commandBaseline = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
            host.Session.Commands.AddNode(ConsumerSampleHost.ReviewDefinitionId, new GraphPoint(880, 220));
            var undoAction = AsterGraphHostedActionFactory.CreateCommandActions(host.Session, ["history.undo"])
                .Single(action => string.Equals(action.Id, "history.undo", StringComparison.Ordinal));
            commandLatencyMs = MeasureMilliseconds(() => undoAction.TryExecute());
            commandSurfaceOk = undoAction.CanExecute
                && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == commandBaseline;

            var initialSnapshot = host.Session.Queries.CreateDocumentSnapshot();
            hostMenuActionOk = host.AddHostReviewNode()
                && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == initialSnapshot.Nodes.Count + 1;

            var afterHostSnapshot = host.Session.Queries.CreateDocumentSnapshot();
            pluginContributionOk = host.HasPluginNodeDefinition()
                && host.HasPluginCommandContribution()
                && host.AddPluginAuditNode()
                && host.PluginLoadSnapshots.Any(snapshot => snapshot.Descriptor?.Id == "consumer.sample.audit-plugin")
                && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == afterHostSnapshot.Nodes.Count + 1;

            host.SelectNode(host.GetFirstReviewNodeId());
            parameterProjectionOk = host.TrySetSelectedOwner("release-owner")
                && host.ApproveSelection()
                && host.GetSelectedParameterSnapshots().Any(snapshot =>
                    snapshot.Definition.Key == "status"
                    && string.Equals(snapshot.CurrentValue?.ToString(), "approved", StringComparison.Ordinal))
                && host.GetSelectedParameterSnapshots().Any(snapshot =>
                    snapshot.Definition.Key == "owner"
                    && string.Equals(snapshot.CurrentValue?.ToString(), "release-owner", StringComparison.Ordinal));

            trustTransparencyOk =
                host.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase))
                && host.ExportPluginAllowlist()
                && File.Exists(host.PluginAllowlistExchangePath)
                && host.BlockPluginCandidate("consumer.sample.audit-plugin")
                && host.PluginCandidateEntries.Any(entry => entry.IsBlocked && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase))
                && host.ImportPluginAllowlist()
                && host.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase));

            stencilSearchMs = MeasureStencilSearchMilliseconds(host.Session, "plugin");
            commandSurfaceRefreshMs = MeasureCommandSurfaceRefreshMilliseconds(host.Session);
            nodeToolProjectionMs = MeasureNodeToolProjectionMilliseconds(host.Session, host.GetFirstReviewNodeId());
            edgeToolProjectionMs = MeasureEdgeToolProjectionMilliseconds(host.Session, host.GetFirstReviewNodeId(), "consumer-sample-connection-001");
            commandPaletteMs = MeasureCommandPaletteMilliseconds(host.Session, window);

            windowCompositionOk =
                FindNamed<Menu>(window, "PART_MainMenu") is not null
                && FindNamed<Button>(window, "PART_AddReviewNodeButton") is not null
                && FindNamed<Button>(window, "PART_AddPluginNodeButton") is not null
                && FindNamed<Button>(window, "PART_ApproveSelectionButton") is not null
                && FindNamed<ItemsControl>(window, "PART_PluginCandidateItems") is not null
                && FindNamed<ItemsControl>(window, "PART_AllowlistItems") is not null
                && FindNamed<Button>(window, "PART_PreviewLayoutButton") is not null
                && FindNamed<Button>(window, "PART_ApplyLayoutPreviewButton") is not null
                && FindNamed<Button>(window, "PART_CancelLayoutPreviewButton") is not null
                && FindNamed<TextBlock>(window, "PART_TrustBoundaryText") is not null;
            runtimeLogPanelOk = HasRuntimeLogPanel(window, host);
            runtimeLogFilterOk = HasRuntimeLogFilter(host);
            layoutProviderSeamOk = HasLayoutProviderSeam(host);
            (layoutPreviewApplyCancelOk, layoutUndoTransactionOk) = HasLayoutPreviewApplyCancel(host, window);
        }
        finally
        {
            window.Close();
        }

        var exportBreadthOk = HasExportBreadth(host, storageRoot);
        var capabilityWindow = CreateCapabilityBreadthWindow(host);
        bool stencilSurfaceOk;
        bool edgeQuickToolsOk;
        bool nodeQuickToolsOk;
        bool hostedAccessibilityBaselineOk;
        bool hostedAccessibilityFocusOk;
        bool hostedAutomationNavigationOk;
        bool hostedAuthoringAutomationDiagnosticsOk;
        bool hostedAccessibilityCommandSurfaceOk;
        bool hostedAccessibilityAuthoringSurfaceOk;
        bool quickAddConnectedNodeOk;
        bool portFilteredNodeSearchOk;
        bool dropNodeOnEdgeOk;
        bool edgeSplitCompatibilityOk;
        bool edgeSplitUndoOk;
        bool deleteAndReconnectOk;
        bool detachNodeOk;
        bool reconnectConflictReportOk;
        bool edgeMultiSelectOk;
        bool wireSliceOk;
        bool selectedNodeEdgeHighlightOk;
        bool runtimeOverlaySnapshotOk;
        try
        {
            capabilityWindow.Show();
            FlushUi();

            var reviewNodeId = host.GetFirstReviewNodeId();
            host.SelectNode(reviewNodeId);
            FlushUi();

            hostedAccessibilityBaselineOk = HasHostedAccessibilityBaselines(capabilityWindow);
            hostedAccessibilityAuthoringSurfaceOk = HasHostedAccessibilityAuthoringSurface(capabilityWindow);
            hostedAccessibilityFocusOk = HasHostedAccessibilityFocus(capabilityWindow);
            hostedAutomationNavigationOk = HasHostedAutomationNavigation(capabilityWindow, host, reviewNodeId);
            hostedAuthoringAutomationDiagnosticsOk = HasHostedAuthoringAutomationDiagnostics(host, reviewNodeId);
            hostedAccessibilityCommandSurfaceOk = HasHostedAccessibilityCommandSurface(capabilityWindow);
            host.SelectNode(reviewNodeId);
            FlushUi();

            stencilSurfaceOk = HasStencilSurface(capabilityWindow, host);
            host.SelectNode(reviewNodeId);
            FlushUi();
            edgeQuickToolsOk = HasEdgeQuickTools(capabilityWindow, host);
            (dropNodeOnEdgeOk, edgeSplitCompatibilityOk, edgeSplitUndoOk) = HasDropNodeOnEdge(host);
            (deleteAndReconnectOk, detachNodeOk, reconnectConflictReportOk) = HasReconnectDetach(host);
            (edgeMultiSelectOk, wireSliceOk, selectedNodeEdgeHighlightOk) = HasWireSelectionAndSlicing(host);
            runtimeOverlaySnapshotOk = HasRuntimeOverlaySnapshot(host);
            (quickAddConnectedNodeOk, portFilteredNodeSearchOk) = HasQuickAddConnectedNode(host);

            host.SelectNode(reviewNodeId);
            FlushUi();
            nodeQuickToolsOk = HasNodeQuickTools(capabilityWindow, host, reviewNodeId);
        }
        finally
        {
            capabilityWindow.Close();
        }

        var finalSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        var runtimeOverlay = host.Session.Queries.GetRuntimeOverlaySnapshot();
        var featureDescriptorIds = host.Session.Queries.GetFeatureDescriptors()
            .Where(d => d.IsAvailable)
            .Select(d => d.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        var recentDiagnosticCodes = host.Session.Diagnostics.GetRecentDiagnostics(16)
            .Select(d => d.Code)
            .Distinct()
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();
        var selectedSupportSnapshots = host.GetSelectedParameterSnapshots().ToArray();
        var mixedValueSnapshots = CaptureMixedValueSnapshots(host);
        var validationFixSnapshots = CreateValidationFixSnapshots();
        var proofParameterSnapshots = CreateParameterSnapshots(
            [.. selectedSupportSnapshots, .. mixedValueSnapshots, .. validationFixSnapshots]);
        var supportBundlePayloadOk = HasSupportBundlePayload(proofParameterSnapshots, finalSnapshot, featureDescriptorIds);
        var runtimeOverlaySupportBundleOk = HasRuntimeOverlaySupportBundlePayload(runtimeOverlay);
        var fiveMinuteOnboardingOk = scenarioGraphOk
            && hostOwnedActionsOk
            && parameterProjectionOk
            && trustTransparencyOk
            && exportBreadthOk
            && supportBundlePayloadOk;

        return new ConsumerSampleProofResult(
            HostMenuActionOk: hostMenuActionOk,
            PluginContributionOk: pluginContributionOk,
            ParameterProjectionOk: parameterProjectionOk,
            MetadataProjectionOk: metadataProjectionOk,
            NodeSideAuthoringOk: nodeSideAuthoringOk,
            WindowCompositionOk: windowCompositionOk,
            TrustTransparencyOk: trustTransparencyOk,
            CommandSurfaceOk: commandSurfaceOk,
            StencilSurfaceOk: stencilSurfaceOk,
            ExportBreadthOk: exportBreadthOk,
            NodeQuickToolsOk: nodeQuickToolsOk,
            EdgeQuickToolsOk: edgeQuickToolsOk,
            HostedAccessibilityBaselineOk: hostedAccessibilityBaselineOk,
            HostedAccessibilityFocusOk: hostedAccessibilityFocusOk,
            HostedAccessibilityCommandSurfaceOk: hostedAccessibilityCommandSurfaceOk,
            HostedAccessibilityAuthoringSurfaceOk: hostedAccessibilityAuthoringSurfaceOk,
            QuickAddConnectedNodeOk: quickAddConnectedNodeOk,
            PortFilteredNodeSearchOk: portFilteredNodeSearchOk,
            DropNodeOnEdgeOk: dropNodeOnEdgeOk,
            EdgeSplitCompatibilityOk: edgeSplitCompatibilityOk,
            EdgeSplitUndoOk: edgeSplitUndoOk,
            DeleteAndReconnectOk: deleteAndReconnectOk,
            DetachNodeOk: detachNodeOk,
            ReconnectConflictReportOk: reconnectConflictReportOk,
            EdgeMultiSelectOk: edgeMultiSelectOk,
            WireSliceOk: wireSliceOk,
            SelectedNodeEdgeHighlightOk: selectedNodeEdgeHighlightOk,
            RuntimeOverlaySnapshotOk: runtimeOverlaySnapshotOk,
            RuntimeLogPanelOk: runtimeLogPanelOk,
            RuntimeLogFilterOk: runtimeLogFilterOk,
            RuntimeOverlaySupportBundleOk: runtimeOverlaySupportBundleOk,
            LayoutProviderSeamOk: layoutProviderSeamOk,
            LayoutPreviewApplyCancelOk: layoutPreviewApplyCancelOk,
            LayoutUndoTransactionOk: layoutUndoTransactionOk,
            ParameterSnapshots: proofParameterSnapshots,
            StartupMs: startupMs,
            InspectorProjectionMs: inspectorProjectionMs,
            PluginScanMs: pluginScanMs,
            CommandLatencyMs: commandLatencyMs,
            StencilSearchMs: stencilSearchMs,
            CommandSurfaceRefreshMs: commandSurfaceRefreshMs,
            NodeToolProjectionMs: nodeToolProjectionMs,
            EdgeToolProjectionMs: edgeToolProjectionMs,
            CommandPaletteMs: commandPaletteMs,
            NodeCount: finalSnapshot.Nodes.Count,
            ConnectionCount: finalSnapshot.Connections.Count,
            FeatureDescriptorIds: featureDescriptorIds,
            RecentDiagnosticCodes: recentDiagnosticCodes,
            RuntimeNodeOverlays: runtimeOverlay.NodeOverlays,
            RuntimeConnectionOverlays: runtimeOverlay.ConnectionOverlays,
            RuntimeLogs: runtimeOverlay.RecentLogs,
            HostedAutomationNavigationOk: hostedAutomationNavigationOk,
            HostedAuthoringAutomationDiagnosticsOk: hostedAuthoringAutomationDiagnosticsOk,
            ScenarioGraphOk: scenarioGraphOk,
            HostOwnedActionsOk: hostOwnedActionsOk,
            SupportBundlePayloadOk: supportBundlePayloadOk,
            FiveMinuteOnboardingOk: fiveMinuteOnboardingOk);
    }

    private static bool HasScenarioGraph(ConsumerSampleHost host)
    {
        var document = host.Session.Queries.CreateDocumentSnapshot();
        var reviewNode = document.Nodes.SingleOrDefault(node => node.DefinitionId == ConsumerSampleHost.ReviewDefinitionId);
        var queueNode = document.Nodes.SingleOrDefault(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId);
        var connection = document.Connections.SingleOrDefault(connection =>
            string.Equals(connection.Id, "consumer-sample-connection-001", StringComparison.Ordinal));
        var definitions = host.Session.Queries.GetRegisteredNodeDefinitions();

        return string.Equals(document.Title, ConsumerSampleHost.ScenarioTitle, StringComparison.Ordinal)
            && document.Description.Contains("content-review scenario", StringComparison.OrdinalIgnoreCase)
            && reviewNode is not null
            && queueNode is not null
            && reviewNode.ParameterValues?.Any(parameter =>
                string.Equals(parameter.Key, "status", StringComparison.Ordinal)
                && string.Equals(parameter.Value?.ToString(), "draft", StringComparison.Ordinal)) == true
            && reviewNode.ParameterValues?.Any(parameter =>
                string.Equals(parameter.Key, "owner", StringComparison.Ordinal)
                && string.Equals(parameter.Value?.ToString(), "design-review", StringComparison.Ordinal)) == true
            && connection is not null
            && string.Equals(connection.SourceNodeId, reviewNode.Id, StringComparison.Ordinal)
            && string.Equals(connection.TargetNodeId, queueNode.Id, StringComparison.Ordinal)
            && definitions.Any(definition => definition.Id == ConsumerSampleHost.ReviewDefinitionId)
            && definitions.Any(definition => definition.Id == ConsumerSampleHost.QueueDefinitionId)
            && definitions.Any(definition => definition.Id == ConsumerSampleHost.PluginAuditDefinitionId);
    }

    private static bool HasHostOwnedActions(ConsumerSampleHost host)
    {
        var descriptors = host.Session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        return host.OnboardingCopyPathLines.Count == 4
            && host.OnboardingCopyPathLines.Any(line => line.Contains("ConsumerSample.Avalonia", StringComparison.Ordinal))
            && host.RouteBoundaryLines.Any(line => line.Contains("Hosted UI route", StringComparison.Ordinal) && line.Contains("AsterGraphAvaloniaViewFactory.Create", StringComparison.Ordinal))
            && host.RouteBoundaryLines.Any(line => line.Contains("Runtime-only route", StringComparison.Ordinal) && line.Contains("CreateSession", StringComparison.Ordinal))
            && host.RouteBoundaryLines.Any(line => line.Contains("Plugin route", StringComparison.Ordinal) && line.Contains("PluginTrustPolicy", StringComparison.Ordinal))
            && host.RouteBoundaryLines.Any(line => line.Contains("Migration route", StringComparison.Ordinal) && line.Contains("retained migration", StringComparison.OrdinalIgnoreCase))
            && host.HasPluginCommandContribution()
            && descriptors.ContainsKey(ConsumerSampleHost.PluginCommandId)
            && descriptors.ContainsKey("workspace.save")
            && descriptors.ContainsKey("workspace.load")
            && descriptors.ContainsKey("history.undo")
            && host.PluginCandidateEntries.Any(entry =>
                string.Equals(entry.PluginId, "consumer.sample.audit-plugin", StringComparison.Ordinal)
                && entry.IsAllowed);
    }

    private static bool HasSupportBundlePayload(
        IReadOnlyList<ConsumerSampleProofParameterSnapshot> parameterSnapshots,
        GraphDocument document,
        IReadOnlyList<string> featureDescriptorIds)
        => string.Equals(document.Title, ConsumerSampleHost.ScenarioTitle, StringComparison.Ordinal)
        && document.Nodes.Count > 0
        && parameterSnapshots.Any(snapshot =>
            string.Equals(snapshot.Key, "status", StringComparison.Ordinal)
            && string.Equals(snapshot.CurrentValue?.ToString(), "approved", StringComparison.Ordinal))
        && parameterSnapshots.Any(snapshot =>
            string.Equals(snapshot.Key, "owner", StringComparison.Ordinal)
            && string.Equals(snapshot.CurrentValue?.ToString(), "release-owner", StringComparison.Ordinal))
        && featureDescriptorIds.Count > 0;

    private static bool HasLayoutProviderSeam(ConsumerSampleHost host)
    {
        var request = new GraphLayoutRequest
        {
            Mode = GraphLayoutRequestMode.All,
            Orientation = GraphLayoutOrientation.LeftToRight,
            HorizontalSpacing = 320,
            VerticalSpacing = 160,
        };
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var plan = host.Session.Queries.CreateLayoutPlan(request);
        var after = host.Session.Queries.CreateDocumentSnapshot();
        var descriptors = host.Session.Queries.GetFeatureDescriptors();

        return plan.IsAvailable
            && ReferenceEquals(request, plan.Request)
            && plan.NodePositions.Count >= 2
            && plan.ResetManualRoutes
            && before.Nodes.Select(node => node.Position).SequenceEqual(after.Nodes.Select(node => node.Position))
            && descriptors.Any(descriptor => descriptor.Id == "query.layout-plan" && descriptor.IsAvailable)
            && descriptors.Any(descriptor => descriptor.Id == "integration.layout-provider" && descriptor.IsAvailable);
    }

    private static (bool PreviewApplyCancelOk, bool UndoTransactionOk) HasLayoutPreviewApplyCancel(ConsumerSampleHost host, Window window)
    {
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var beforePositions = before.Nodes.ToDictionary(node => node.Id, node => node.Position, StringComparer.Ordinal);

        var previewCreated = host.PreviewLayout();
        FlushUi();
        var preview = host.LayoutPreview;
        var previewVisible = FindNamed<TextBlock>(window, "PART_LayoutPreviewSummaryText")?.Text?.Contains("Preview nodes:", StringComparison.Ordinal) == true;
        var unchangedAfterPreview = DocumentHasPositions(host.Session.Queries.CreateDocumentSnapshot(), beforePositions);
        var canceled = host.CancelLayoutPreview();
        FlushUi();
        var canceledWithoutMutation = host.LayoutPreview is null
            && DocumentHasPositions(host.Session.Queries.CreateDocumentSnapshot(), beforePositions);

        var applied = host.PreviewLayout() && host.ApplyLayoutPreview();
        FlushUi();
        var afterApply = host.Session.Queries.CreateDocumentSnapshot();
        var applyMutated = afterApply.Nodes.Any(node =>
            beforePositions.TryGetValue(node.Id, out var previous)
            && previous != node.Position);
        var previewApplyCancelOk = previewCreated
            && preview is not null
            && previewVisible
            && unchangedAfterPreview
            && canceled
            && canceledWithoutMutation
            && applied
            && applyMutated
            && host.LayoutPreview is null;

        var undo = host.Session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot("history.undo"));
        FlushUi();
        var undoTransactionOk = undo
            && DocumentHasPositions(host.Session.Queries.CreateDocumentSnapshot(), beforePositions);

        return (previewApplyCancelOk, undoTransactionOk);
    }

    private static bool DocumentHasPositions(GraphDocument document, IReadOnlyDictionary<string, GraphPoint> expectedPositions)
        => document.Nodes.All(node =>
            expectedPositions.TryGetValue(node.Id, out var expected)
            && expected == node.Position);

    private static (bool QuickAddConnectedNodeOk, bool PortFilteredNodeSearchOk) HasQuickAddConnectedNode(ConsumerSampleHost host)
    {
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var sourceNode = before.Nodes.FirstOrDefault(node => node.DefinitionId == ConsumerSampleHost.ReviewDefinitionId);
        if (sourceNode is null)
        {
            return (false, false);
        }

        host.Session.Commands.StartConnection(sourceNode.Id, "output");
        var search = host.Session.Queries.GetCompatibleNodeDefinitionsForPendingConnection();
        var queueInput = search.Results.FirstOrDefault(result =>
            result.DefinitionId == ConsumerSampleHost.QueueDefinitionId
            && result.TargetKind == GraphConnectionTargetKind.Port
            && string.Equals(result.TargetId, "input", StringComparison.Ordinal));
        var portFilteredNodeSearchOk = search.HasPendingConnection
            && string.IsNullOrWhiteSpace(search.EmptyReason)
            && queueInput is not null
            && search.Results.All(result => result.Compatibility.IsCompatible);
        if (queueInput is null)
        {
            host.Session.Commands.CancelPendingConnection();
            return (false, portFilteredNodeSearchOk);
        }

        var connected = host.Session.Commands.TryCreateConnectedNodeFromPendingConnection(
            queueInput.DefinitionId,
            queueInput.TargetId,
            queueInput.TargetKind,
            new GraphPoint(760, 520));
        var after = host.Session.Queries.CreateDocumentSnapshot();
        var createdQueueNodeIds = after.Nodes
            .Where(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .Select(node => node.Id)
            .Except(before.Nodes.Select(node => node.Id), StringComparer.Ordinal)
            .ToArray();
        var quickAddConnectedNodeOk = connected
            && createdQueueNodeIds.Length == 1
            && after.Connections.Any(connection =>
                string.Equals(connection.SourceNodeId, sourceNode.Id, StringComparison.Ordinal)
                && string.Equals(connection.SourcePortId, "output", StringComparison.Ordinal)
                && string.Equals(connection.TargetNodeId, createdQueueNodeIds[0], StringComparison.Ordinal)
                && string.Equals(connection.TargetPortId, "input", StringComparison.Ordinal))
            && !host.Session.Queries.GetPendingConnectionSnapshot().HasPendingConnection;
        return (quickAddConnectedNodeOk, portFilteredNodeSearchOk);
    }

    private static (bool DropNodeOnEdgeOk, bool EdgeSplitCompatibilityOk, bool EdgeSplitUndoOk) HasDropNodeOnEdge(ConsumerSampleHost host)
    {
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var originalConnection = before.Connections.FirstOrDefault(connection =>
            string.Equals(connection.Id, "consumer-sample-connection-001", StringComparison.Ordinal));
        if (originalConnection is null)
        {
            return (false, false, false);
        }

        var rejected = host.Session.Commands.TryInsertNodeIntoConnection(
            originalConnection.Id,
            ConsumerSampleHost.QueueDefinitionId,
            "missing-input",
            GraphConnectionTargetKind.Port,
            "output",
            new GraphPoint(420, 360));
        var afterRejected = host.Session.Queries.CreateDocumentSnapshot();
        var edgeSplitCompatibilityOk = !rejected
            && afterRejected.Nodes.Count == before.Nodes.Count
            && afterRejected.Connections.Count == before.Connections.Count
            && afterRejected.Connections.Any(connection => string.Equals(connection.Id, originalConnection.Id, StringComparison.Ordinal));

        var inserted = host.Session.Commands.TryInsertNodeIntoConnection(
            originalConnection.Id,
            ConsumerSampleHost.QueueDefinitionId,
            "input",
            GraphConnectionTargetKind.Port,
            "output",
            new GraphPoint(420, 360));
        var afterInsert = host.Session.Queries.CreateDocumentSnapshot();
        var insertedQueueNode = afterInsert.Nodes
            .Where(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .FirstOrDefault(node => before.Nodes.All(beforeNode => !string.Equals(beforeNode.Id, node.Id, StringComparison.Ordinal)));
        var dropNodeOnEdgeOk = inserted
            && insertedQueueNode is not null
            && !afterInsert.Connections.Any(connection => string.Equals(connection.Id, originalConnection.Id, StringComparison.Ordinal))
            && afterInsert.Connections.Any(connection =>
                string.Equals(connection.SourceNodeId, originalConnection.SourceNodeId, StringComparison.Ordinal)
                && string.Equals(connection.SourcePortId, originalConnection.SourcePortId, StringComparison.Ordinal)
                && string.Equals(connection.TargetNodeId, insertedQueueNode.Id, StringComparison.Ordinal)
                && string.Equals(connection.TargetPortId, "input", StringComparison.Ordinal))
            && afterInsert.Connections.Any(connection =>
                string.Equals(connection.SourceNodeId, insertedQueueNode.Id, StringComparison.Ordinal)
                && string.Equals(connection.SourcePortId, "output", StringComparison.Ordinal)
                && string.Equals(connection.TargetNodeId, originalConnection.TargetNodeId, StringComparison.Ordinal)
                && string.Equals(connection.TargetPortId, originalConnection.TargetPortId, StringComparison.Ordinal));

        host.Session.Commands.Undo();
        var afterUndo = host.Session.Queries.CreateDocumentSnapshot();
        var edgeSplitUndoOk = afterUndo.Nodes.Count == before.Nodes.Count
            && afterUndo.Connections.Count == before.Connections.Count
            && afterUndo.Connections.Any(connection =>
                string.Equals(connection.Id, originalConnection.Id, StringComparison.Ordinal)
                && string.Equals(connection.SourceNodeId, originalConnection.SourceNodeId, StringComparison.Ordinal)
                && string.Equals(connection.TargetNodeId, originalConnection.TargetNodeId, StringComparison.Ordinal));

        return (dropNodeOnEdgeOk, edgeSplitCompatibilityOk, edgeSplitUndoOk);
    }

    private static (bool DeleteAndReconnectOk, bool DetachNodeOk, bool ReconnectConflictReportOk) HasReconnectDetach(ConsumerSampleHost host)
    {
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var originalConnection = before.Connections.FirstOrDefault(connection =>
            string.Equals(connection.Id, "consumer-sample-connection-001", StringComparison.Ordinal));
        if (originalConnection is null)
        {
            return (false, false, false);
        }

        var conflictReportOk = !host.Session.Commands.TryDeleteSelectionAndReconnect()
            && host.Session.Diagnostics.CaptureInspectionSnapshot().Status.Message.Contains("Reconnect", StringComparison.Ordinal);

        if (!host.Session.Commands.TryInsertNodeIntoConnection(
                originalConnection.Id,
                ConsumerSampleHost.QueueDefinitionId,
                "input",
                GraphConnectionTargetKind.Port,
                "output",
                new GraphPoint(420, 360)))
        {
            return (false, false, conflictReportOk);
        }

        var withDetachMiddle = host.Session.Queries.CreateDocumentSnapshot();
        var detachMiddle = withDetachMiddle.Nodes
            .Where(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .FirstOrDefault(node => before.Nodes.All(beforeNode => !string.Equals(beforeNode.Id, node.Id, StringComparison.Ordinal)));
        if (detachMiddle is null)
        {
            return (false, false, conflictReportOk);
        }

        host.Session.Commands.SetSelection([detachMiddle.Id], detachMiddle.Id);
        var detached = host.Session.Commands.TryDetachSelectionFromConnections();
        var afterDetach = host.Session.Queries.CreateDocumentSnapshot();
        var detachNodeOk = detached
            && afterDetach.Nodes.Any(node => string.Equals(node.Id, detachMiddle.Id, StringComparison.Ordinal))
            && afterDetach.Connections.Any(connection =>
                string.Equals(connection.SourceNodeId, originalConnection.SourceNodeId, StringComparison.Ordinal)
                && string.Equals(connection.SourcePortId, originalConnection.SourcePortId, StringComparison.Ordinal)
                && string.Equals(connection.TargetNodeId, originalConnection.TargetNodeId, StringComparison.Ordinal)
                && string.Equals(connection.TargetPortId, originalConnection.TargetPortId, StringComparison.Ordinal));
        host.Session.Commands.Undo();
        host.Session.Commands.Undo();

        if (!host.Session.Commands.TryInsertNodeIntoConnection(
                originalConnection.Id,
                ConsumerSampleHost.QueueDefinitionId,
                "input",
                GraphConnectionTargetKind.Port,
                "output",
                new GraphPoint(420, 360)))
        {
            return (false, detachNodeOk, conflictReportOk);
        }

        var withDeleteMiddle = host.Session.Queries.CreateDocumentSnapshot();
        var deleteMiddle = withDeleteMiddle.Nodes
            .Where(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .FirstOrDefault(node => before.Nodes.All(beforeNode => !string.Equals(beforeNode.Id, node.Id, StringComparison.Ordinal)));
        if (deleteMiddle is null)
        {
            return (false, detachNodeOk, conflictReportOk);
        }

        host.Session.Commands.SetSelection([deleteMiddle.Id], deleteMiddle.Id);
        var deleted = host.Session.Commands.TryDeleteSelectionAndReconnect();
        var afterDelete = host.Session.Queries.CreateDocumentSnapshot();
        var deleteAndReconnectOk = deleted
            && afterDelete.Nodes.All(node => !string.Equals(node.Id, deleteMiddle.Id, StringComparison.Ordinal))
            && afterDelete.Connections.Any(connection =>
                string.Equals(connection.SourceNodeId, originalConnection.SourceNodeId, StringComparison.Ordinal)
                && string.Equals(connection.SourcePortId, originalConnection.SourcePortId, StringComparison.Ordinal)
                && string.Equals(connection.TargetNodeId, originalConnection.TargetNodeId, StringComparison.Ordinal)
                && string.Equals(connection.TargetPortId, originalConnection.TargetPortId, StringComparison.Ordinal));
        host.Session.Commands.Undo();
        host.Session.Commands.Undo();

        return (deleteAndReconnectOk, detachNodeOk, conflictReportOk);
    }

    private static (bool EdgeMultiSelectOk, bool WireSliceOk, bool SelectedNodeEdgeHighlightOk) HasWireSelectionAndSlicing(ConsumerSampleHost host)
    {
        const string connectionId = "consumer-sample-connection-001";
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var originalConnection = before.Connections.FirstOrDefault(connection =>
            string.Equals(connection.Id, connectionId, StringComparison.Ordinal));
        if (originalConnection is null)
        {
            return (false, false, false);
        }

        host.Session.Commands.SetConnectionSelection([connectionId, "missing-connection"], connectionId, updateStatus: false);
        var selection = host.Session.Queries.GetSelectionSnapshot();
        var edgeMultiSelectOk = selection.SelectedNodeIds.Count == 0
            && selection.SelectedConnectionIds.SequenceEqual([connectionId], StringComparer.Ordinal)
            && string.Equals(selection.PrimarySelectedConnectionId, connectionId, StringComparison.Ordinal)
            && host.Session.Commands.TryDeleteSelectedConnections()
            && host.Session.Queries.CreateDocumentSnapshot().Connections.All(connection => !string.Equals(connection.Id, connectionId, StringComparison.Ordinal));
        host.Session.Commands.Undo();

        host.Session.Commands.SetSelection([originalConnection.SourceNodeId, originalConnection.TargetNodeId], originalConnection.SourceNodeId, updateStatus: false);
        var selectedNodeEdgeHighlightOk = host.Session.Queries.GetSelectedNodeConnectionIds()
            .SequenceEqual([connectionId], StringComparer.Ordinal);

        var wireSliceOk = host.Session.Commands.TrySliceConnections(new GraphPoint(470, 230), new GraphPoint(470, 330))
            && host.Session.Queries.CreateDocumentSnapshot().Connections.All(connection => !string.Equals(connection.Id, connectionId, StringComparison.Ordinal));
        host.Session.Commands.Undo();

        return (edgeMultiSelectOk, wireSliceOk, selectedNodeEdgeHighlightOk);
    }

    private static Window CreateCapabilityBreadthWindow(ConsumerSampleHost host)
    {
        var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = host.Editor,
            ChromeMode = GraphEditorViewChromeMode.Default,
            EnableDefaultContextMenu = true,
            CommandShortcutPolicy = AsterGraphCommandShortcutPolicy.Default,
            Presentation = ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions(),
        });

        return new Window
        {
            Width = 1440,
            Height = 900,
            Content = view,
        };
    }

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));

    private static bool HasNodeSideAuthoring(Window window, ConsumerSampleHost host, string reviewNodeId)
    {
        var initialWidth = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .Size.Width;
        var widenButton = FindNamed<Button>(window, $"PART_RecipeToolbarWiden_{reviewNodeId}");
        if (widenButton is not null)
        {
            widenButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            FlushUi();
        }

        var widenedWidth = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .Size.Width;
        var nodeSnapshots = host.Session.Queries.GetNodeParameterSnapshots(reviewNodeId);
        var statusTemplateOk = nodeSnapshots.Any(snapshot =>
            snapshot.Definition.Key == "status"
            && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.StatusTemplateKey, StringComparison.Ordinal));
        var ownerTemplateOk = nodeSnapshots.Any(snapshot =>
            snapshot.Definition.Key == "owner"
            && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.OwnerTemplateKey, StringComparison.Ordinal));

        return FindNamed<Canvas>(window, "PART_AuthoringEdgeOverlay") is not null
            && FindNamed<Border>(window, "PART_AuthoringEdgeBadge_consumer-sample-connection-001") is not null
            && widenButton is not null
            && widenedWidth > initialWidth
            && statusTemplateOk
            && ownerTemplateOk;
    }

    private static bool HasStencilSurface(Window window, ConsumerSampleHost host)
    {
        var searchBox = FindNamed<TextBox>(window, "PART_StencilSearchBox");
        if (searchBox is null)
        {
            return false;
        }

        var templates = host.Session.Queries.GetNodeTemplateSnapshots();
        var pluginTemplate = templates.SingleOrDefault(template =>
            template.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId);
        if (pluginTemplate is null)
        {
            return false;
        }

        var allSections = window.GetVisualDescendants()
            .OfType<Expander>()
            .Where(section => section.Name?.StartsWith("PART_StencilSection_", StringComparison.Ordinal) == true)
            .ToList();
        if (allSections.Count < 2)
        {
            return false;
        }

        searchBox.Text = "plugin";
        FlushUi();

        var filteredSections = window.GetVisualDescendants()
            .OfType<Expander>()
            .Where(section => section.Name?.StartsWith("PART_StencilSection_", StringComparison.Ordinal) == true)
            .ToList();
        var pluginSection = filteredSections.SingleOrDefault(section =>
            string.Equals(section.Tag?.ToString(), pluginTemplate.Category, StringComparison.Ordinal));
        var pluginCard = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}");
        if (filteredSections.Count != 1 || pluginSection is null || pluginCard is null)
        {
            return false;
        }

        var initialPluginNodeCount = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count(node =>
            node.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId);
        pluginSection.IsExpanded = false;
        FlushUi();
        pluginSection.IsExpanded = true;
        FlushUi();

        pluginCard = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}");
        if (pluginCard is null || !pluginSection.IsExpanded)
        {
            return false;
        }

        pluginCard.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        return host.Session.Queries.CreateDocumentSnapshot().Nodes.Count(node =>
            node.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId) == initialPluginNodeCount + 1;
    }

    private static bool HasExportBreadth(ConsumerSampleHost host, string storageRoot)
    {
        var featureDescriptors = host.Session.Queries.GetFeatureDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!featureDescriptors.TryGetValue("capability.export.scene-svg", out var svgFeature)
            || !svgFeature.IsAvailable
            || !featureDescriptors.TryGetValue("capability.export.scene-png", out var pngFeature)
            || !pngFeature.IsAvailable
            || !featureDescriptors.TryGetValue("capability.export.scene-jpeg", out var jpegFeature)
            || !jpegFeature.IsAvailable)
        {
            return false;
        }

        var exportRoot = Path.Combine(storageRoot, "capability-breadth-proof");
        Directory.CreateDirectory(exportRoot);
        var svgPath = Path.Combine(exportRoot, "consumer-sample-proof.svg");
        var pngPath = Path.Combine(exportRoot, "consumer-sample-proof.png");
        var jpegPath = Path.Combine(exportRoot, "consumer-sample-proof.jpg");

        return host.Session.Commands.TryExportSceneAsSvg(svgPath)
            && File.Exists(svgPath)
            && File.ReadAllText(svgPath).Contains("<svg", StringComparison.Ordinal)
            && host.Session.Commands.TryExportSceneAsImage(GraphEditorSceneImageExportFormat.Png, pngPath)
            && File.Exists(pngPath)
            && HasImageSignature(File.ReadAllBytes(pngPath), GraphEditorSceneImageExportFormat.Png)
            && host.Session.Commands.TryExportSceneAsImage(GraphEditorSceneImageExportFormat.Jpeg, jpegPath)
            && File.Exists(jpegPath)
            && HasImageSignature(File.ReadAllBytes(jpegPath), GraphEditorSceneImageExportFormat.Jpeg);
    }

    private static double MeasureStencilSearchMilliseconds(
        IGraphEditorSession session,
        string filter)
        => MeasureMilliseconds(() =>
        {
            var addNodeDescriptor = session.Queries.GetCommandDescriptors()
                .FirstOrDefault(descriptor => string.Equals(descriptor.Id, "nodes.add", StringComparison.Ordinal));
            var sections = session.Queries.GetNodeTemplateSnapshots()
                .Where(snapshot => MatchesStencilFilter(snapshot, filter))
                .GroupBy(snapshot => snapshot.Category, StringComparer.Ordinal)
                .OrderBy(section => section.Key, StringComparer.Ordinal)
                .Select(section => (Category: section.Key, Count: section.Count()))
                .ToArray();
            _ = addNodeDescriptor?.Id;
            _ = sections.Length;
        });

    private static double MeasureCommandSurfaceRefreshMilliseconds(IGraphEditorSession session)
        => MeasureMilliseconds(() =>
        {
            var actions = AsterGraphHostedActionFactory.ApplyCommandShortcutPolicy(
                [
                    .. AsterGraphHostedActionFactory.CreateCommandActions(session),
                    .. AsterGraphHostedActionFactory.CreateCommandActions(
                        session,
                        ["composites.wrap-selection", "scopes.enter", "scopes.exit"]),
                    .. AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(session),
                ],
                AsterGraphCommandShortcutPolicy.Default);
            _ = actions.Count;
        });

    private static double MeasureNodeToolProjectionMilliseconds(
        IGraphEditorSession session,
        string nodeId)
        => MeasureMilliseconds(() =>
        {
            var selection = session.Queries.GetSelectionSnapshot();
            var actions = AsterGraphAuthoringToolActionFactory.CreateNodeActions(
                session,
                nodeId,
                selection.SelectedNodeIds,
                selection.PrimarySelectedNodeId);
            _ = actions.Count;
        });

    private static double MeasureEdgeToolProjectionMilliseconds(
        IGraphEditorSession session,
        string nodeId,
        string connectionId)
        => MeasureMilliseconds(() =>
        {
            var selection = session.Queries.GetSelectionSnapshot();
            var nodeActions = AsterGraphAuthoringToolActionFactory.CreateNodeActions(
                session,
                nodeId,
                selection.SelectedNodeIds,
                selection.PrimarySelectedNodeId);
            var connectionActions = AsterGraphAuthoringToolActionFactory.CreateConnectionActions(
                session,
                connectionId,
                selection.SelectedNodeIds,
                selection.PrimarySelectedNodeId);
            _ = nodeActions.Count + connectionActions.Count;
        });

    private static double MeasureCommandPaletteMilliseconds(
        IGraphEditorSession session,
        Window window)
        => MeasureMilliseconds(() =>
        {
            var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
            if (paletteToggle is not null)
            {
                paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                FlushUi();
            }
            var paletteChrome = FindNamed<Border>(window, "PART_CommandPaletteChrome");
            _ = paletteChrome?.IsVisible;
            if (paletteToggle is not null)
            {
                paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                FlushUi();
            }
        });

    private static bool HasNodeQuickTools(Window window, ConsumerSampleHost host, string reviewNodeId)
    {
        var expansionButton = FindNamed<Button>(window, "PART_NodeToolToggleExpansionButton");
        var duplicateButton = FindNamed<Button>(window, "PART_NodeToolDuplicateButton");
        if (expansionButton is null
            || duplicateButton is null
            || !expansionButton.IsEnabled
            || !duplicateButton.IsEnabled)
        {
            return false;
        }

        var initialExpansionState = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .ExpansionState;
        expansionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        var expansionChanged = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .ExpansionState != initialExpansionState;

        host.SelectNode(reviewNodeId);
        FlushUi();
        duplicateButton = FindNamed<Button>(window, "PART_NodeToolDuplicateButton");
        if (duplicateButton is null || !duplicateButton.IsEnabled)
        {
            return false;
        }

        var nodeCount = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
        duplicateButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        return expansionChanged
            && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == nodeCount + 1;
    }

    private static bool HasEdgeQuickTools(Window window, ConsumerSampleHost host)
    {
        const string connectionId = "consumer-sample-connection-001";
        if (!host.Session.Commands.TryExecuteCommand(CreateCommand(
                "connections.note.set",
                ("connectionId", connectionId),
                ("text", "Proof note"),
                ("updateStatus", bool.TrueString))))
        {
            return false;
        }

        FlushUi();

        var clearNoteButton = FindNamed<Button>(window, $"PART_ConnectionToolClearNote_{connectionId}");
        var disconnectButton = FindNamed<Button>(window, $"PART_ConnectionToolDisconnect_{connectionId}");
        if (clearNoteButton is null
            || disconnectButton is null
            || !clearNoteButton.IsEnabled
            || !disconnectButton.IsEnabled)
        {
            return false;
        }

        clearNoteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        var noteCleared = string.IsNullOrWhiteSpace(host.Session.Queries.CreateDocumentSnapshot()
            .Connections
            .Single(connection => connection.Id == connectionId)
            .Presentation?.NoteText);

        host.Session.Commands.TryExecuteCommand(CreateCommand(
            "connections.note.set",
            ("connectionId", connectionId),
            ("text", "Disconnect proof"),
            ("updateStatus", bool.TrueString)));
        FlushUi();

        disconnectButton = FindNamed<Button>(window, $"PART_ConnectionToolDisconnect_{connectionId}");
        if (disconnectButton is null || !disconnectButton.IsEnabled)
        {
            return false;
        }

        disconnectButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        var disconnected = host.Session.Queries.CreateDocumentSnapshot().Connections.Count == 0;
        host.Session.Commands.Undo();
        FlushUi();
        var restored = host.Session.Queries.CreateDocumentSnapshot().Connections.Any(connection =>
            string.Equals(connection.Id, connectionId, StringComparison.Ordinal));

        return noteCleared && disconnected && restored;
    }

    private static bool HasHostedAccessibilityBaselines(Window window)
    {
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var canvas = FindNamed<NodeCanvas>(window, "PART_NodeCanvas");
        var stencilSearchBox = FindNamed<TextBox>(window, "PART_StencilSearchBox");
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        var paletteSearchBox = FindNamed<TextBox>(window, "PART_CommandPaletteSearchBox");
        var inspector = FindNamed<GraphInspectorView>(window, "PART_InspectorSurface");
        var parameterSearchBox = FindNamed<TextBox>(window, "PART_ParameterSearchBox");

        return editorView is not null
            && string.Equals(AutomationProperties.GetName(editorView), "Graph editor host", StringComparison.Ordinal)
            && canvas is not null
            && string.Equals(AutomationProperties.GetName(canvas), "Graph canvas", StringComparison.Ordinal)
            && stencilSearchBox is not null
            && string.Equals(AutomationProperties.GetName(stencilSearchBox), "Stencil search", StringComparison.Ordinal)
            && paletteToggle is not null
            && string.Equals(AutomationProperties.GetName(paletteToggle), "Open command palette", StringComparison.Ordinal)
            && paletteSearchBox is not null
            && string.Equals(AutomationProperties.GetName(paletteSearchBox), "Command palette search", StringComparison.Ordinal)
            && inspector is not null
            && string.Equals(AutomationProperties.GetName(inspector), "Graph inspector", StringComparison.Ordinal)
            && parameterSearchBox is not null
            && string.Equals(AutomationProperties.GetName(parameterSearchBox), "Inspector parameter search", StringComparison.Ordinal);
    }

    private static bool HasHostedAccessibilityFocus(Window window)
    {
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var paletteChrome = FindNamed<Border>(window, "PART_CommandPaletteChrome");
        var paletteSearchBox = FindNamed<TextBox>(window, "PART_CommandPaletteSearchBox");
        var stencilSearchBox = FindNamed<TextBox>(window, "PART_StencilSearchBox");
        var parameterSearchBox = FindNamed<TextBox>(window, "PART_ParameterSearchBox");
        return editorView is not null
            && paletteChrome is not null
            && paletteSearchBox is not null
            && stencilSearchBox is not null
            && parameterSearchBox is not null
            && HasCommandPaletteFocusRoundTrip(editorView, stencilSearchBox, paletteChrome, paletteSearchBox)
            && HasCommandPaletteFocusRoundTrip(editorView, parameterSearchBox, paletteChrome, paletteSearchBox);
    }

    private static bool HasHostedAutomationNavigation(
        Window window,
        ConsumerSampleHost host,
        string reviewNodeId)
    {
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        var paletteSearchBox = FindNamed<TextBox>(window, "PART_CommandPaletteSearchBox");
        if (editorView is null
            || paletteToggle is null
            || paletteSearchBox is null)
        {
            return false;
        }

        var descriptors = host.Session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!HasEnabledCommand(descriptors, "selection.set")
            || !HasEnabledCommand(descriptors, "viewport.center-node")
            || !HasEnabledCommand(descriptors, "viewport.pan"))
        {
            return false;
        }

        var automationRunIds = new List<string>();
        var automationProgressCommands = new List<string>();
        var automationCompletedStates = new List<bool>();
        host.Session.Events.AutomationStarted += (_, args) => automationRunIds.Add(args.RunId);
        host.Session.Events.AutomationProgress += (_, args) => automationProgressCommands.Add(args.Step.CommandId);
        host.Session.Events.AutomationCompleted += (_, args) => automationCompletedStates.Add(args.Result.Succeeded);

        var initialViewport = host.Session.Queries.GetViewportSnapshot();
        var automationResult = host.Session.Automation.Execute(new GraphEditorAutomationRunRequest(
            "consumer-sample-hosted-navigation",
            [
                CreateAutomationStep("select-review", "selection.set", ("nodeId", reviewNodeId), ("primaryNodeId", reviewNodeId), ("updateStatus", "false")),
                CreateAutomationStep("center-review", "viewport.center-node", ("nodeId", reviewNodeId)),
                CreateAutomationStep("pan-view", "viewport.pan", ("deltaX", "24"), ("deltaY", "18")),
            ]));
        FlushUi();

        var selection = host.Session.Queries.GetSelectionSnapshot();
        var viewport = host.Session.Queries.GetViewportSnapshot();
        var diagnostics = host.Session.Diagnostics.GetRecentDiagnostics(20)
            .Select(diagnostic => diagnostic.Code)
            .ToArray();

        return automationResult.Succeeded
            && automationResult.ExecutedStepCount == 3
            && automationResult.TotalStepCount == 3
            && automationRunIds.Count == 1
            && string.Equals(automationRunIds[0], "consumer-sample-hosted-navigation", StringComparison.Ordinal)
            && automationCompletedStates.Count == 1
            && automationCompletedStates[0]
            && automationProgressCommands.SequenceEqual(
                ["selection.set", "viewport.center-node", "viewport.pan"],
                StringComparer.Ordinal)
            && selection.PrimarySelectedNodeId == reviewNodeId
            && automationResult.Inspection.Selection.PrimarySelectedNodeId == reviewNodeId
            && HasViewportChanged(initialViewport, automationResult.Inspection.Viewport)
            && HasViewportChanged(initialViewport, viewport)
            && diagnostics.Contains("automation.run.started", StringComparer.Ordinal)
            && diagnostics.Contains("automation.run.completed", StringComparer.Ordinal);
    }

    private static bool HasHostedAuthoringAutomationDiagnostics(
        ConsumerSampleHost host,
        string reviewNodeId)
    {
        const string connectionId = "consumer-sample-connection-001";
        var automationResult = host.Session.Automation.Execute(new GraphEditorAutomationRunRequest(
            "consumer-sample-authoring-diagnostics",
            [
                CreateAutomationStep("select-review", "selection.set", ("nodeId", reviewNodeId), ("primaryNodeId", reviewNodeId), ("updateStatus", "false")),
            ]));

        var labelEdited = host.Session.Commands.TrySetConnectionLabel(connectionId, "Automation branch", updateStatus: false);
        var noteEdited = host.Session.Commands.TrySetConnectionNoteText(connectionId, "Automation note", updateStatus: false);
        var selectedParameters = host.Session.Queries.GetSelectedNodeParameterSnapshots();
        var nodeParameters = host.Session.Queries.GetNodeParameterSnapshots(reviewNodeId);
        var nodeSurface = host.Session.Queries.GetNodeSurfaceSnapshots()
            .SingleOrDefault(snapshot => string.Equals(snapshot.NodeId, reviewNodeId, StringComparison.Ordinal));
        var selectionTools = host.Session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForSelection([reviewNodeId], reviewNodeId));
        var nodeTools = host.Session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForNode(reviewNodeId, [reviewNodeId], reviewNodeId));
        var connectionTools = host.Session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForConnection(connectionId, [reviewNodeId], reviewNodeId));
        var inspection = host.Session.Diagnostics.CaptureInspectionSnapshot();
        var connection = inspection.Document.Connections.SingleOrDefault(candidate =>
            string.Equals(candidate.Id, connectionId, StringComparison.Ordinal));

        return automationResult.Succeeded
            && automationResult.ExecutedStepCount == 1
            && labelEdited
            && noteEdited
            && inspection.Selection.PrimarySelectedNodeId == reviewNodeId
            && inspection.RecentDiagnostics.Any(diagnostic => diagnostic.Code == "automation.run.started")
            && inspection.RecentDiagnostics.Any(diagnostic => diagnostic.Code == "automation.run.completed")
            && selectedParameters.Any(snapshot =>
                snapshot.Definition.Key == "status"
                && !string.IsNullOrWhiteSpace(snapshot.Definition.Description)
                && !string.IsNullOrWhiteSpace(snapshot.GroupDisplayName)
                && !string.IsNullOrWhiteSpace(snapshot.ValueDisplayText)
                && snapshot.CanEdit
                && snapshot.IsValid)
            && selectedParameters.Any(snapshot =>
                snapshot.Definition.Key == "owner"
                && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.OwnerTemplateKey, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(snapshot.Definition.Description))
            && nodeParameters.Any(snapshot =>
                snapshot.Definition.Key == "status"
                && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.StatusTemplateKey, StringComparison.Ordinal))
            && nodeSurface is not null
            && nodeSurface.ActiveTier.VisibleSectionKeys.Count > 0
            && ContainsExecutableTool(selectionTools, "selection-create-group")
            && ContainsExecutableTool(nodeTools, "node-inspect")
            && ContainsExecutableTool(nodeTools, "node-toggle-surface-expansion")
            && ContainsExecutableTool(connectionTools, "connection-disconnect")
            && ContainsExecutableTool(connectionTools, "connection-clear-note")
            && connection is not null
            && string.Equals(connection.Label, "Automation branch", StringComparison.Ordinal)
            && string.Equals(connection.Presentation?.NoteText, "Automation note", StringComparison.Ordinal);
    }

    private static bool HasHostedAccessibilityCommandSurface(Window window)
    {
        var saveButton = FindNamed<Button>(window, "PART_HeaderCommand_workspace.save");
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        var duplicateButton = FindNamed<Button>(window, "PART_NodeToolDuplicateButton");
        if (saveButton is null || paletteToggle is null || duplicateButton is null)
        {
            return false;
        }

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        var paletteSaveButton = FindNamed<Button>(window, "PART_CommandPaletteAction_workspace.save");
        var paletteDuplicateButton = FindNamed<Button>(window, "PART_CommandPaletteAction_node-duplicate");
        var result = string.Equals(AutomationProperties.GetName(saveButton), "Save Workspace", StringComparison.Ordinal)
            && string.Equals(AutomationProperties.GetName(duplicateButton), duplicateButton.Content?.ToString(), StringComparison.Ordinal)
            && paletteSaveButton is not null
            && string.Equals(AutomationProperties.GetName(paletteSaveButton), "Save Workspace", StringComparison.Ordinal)
            && paletteDuplicateButton is not null
            && string.Equals(AutomationProperties.GetName(paletteDuplicateButton), paletteDuplicateButton.Content?.ToString(), StringComparison.Ordinal);
        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        return result;
    }

    private static bool HasHostedAccessibilityAuthoringSurface(Window window)
    {
        const string connectionId = "consumer-sample-connection-001";
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var hasAccessibleParameterMetadata = editorView?.Editor?.SelectedNodeParameters.Any(parameter =>
            !string.IsNullOrWhiteSpace(parameter.DisplayName)
            && (!string.IsNullOrWhiteSpace(parameter.HelpText)
                || !string.IsNullOrWhiteSpace(parameter.Description)
                || !string.IsNullOrWhiteSpace(parameter.ReadOnlyReason)
                || !string.IsNullOrWhiteSpace(parameter.ValueStateCaption))) == true;
        var labelEditor = FindNamed<TextBox>(window, $"PART_ConnectionToolLabelEditor_{connectionId}");
        var noteEditor = FindNamed<TextBox>(window, $"PART_ConnectionToolNoteEditor_{connectionId}");
        if (!hasAccessibleParameterMetadata
            || labelEditor is null
            || noteEditor is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(AutomationProperties.GetName(labelEditor))
            && AutomationProperties.GetName(labelEditor)!.EndsWith("label editor", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(AutomationProperties.GetName(noteEditor))
            && AutomationProperties.GetName(noteEditor)!.EndsWith("note editor", StringComparison.Ordinal);
    }

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string? Value)[] arguments)
        => new(
            commandId,
            arguments
                .Where(argument => argument.Value is not null)
                .Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value!))
                .ToArray());

    private static GraphEditorAutomationStep CreateAutomationStep(
        string stepId,
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            stepId,
            CreateCommand(
                commandId,
                arguments
                    .Select(static argument => (argument.Name, (string?)argument.Value))
                    .ToArray()));

    private static bool HasImageSignature(byte[] bytes, GraphEditorSceneImageExportFormat format)
        => bytes.Length > 3
        && format switch
        {
            GraphEditorSceneImageExportFormat.Png
                => bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47,
            GraphEditorSceneImageExportFormat.Jpeg
                => bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            _ => false,
        };

    private static bool MatchesStencilFilter(GraphEditorNodeTemplateSnapshot snapshot, string filter)
        => string.IsNullOrWhiteSpace(filter)
        || snapshot.Category.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || snapshot.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || snapshot.Subtitle.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || snapshot.Description.Contains(filter, StringComparison.OrdinalIgnoreCase);

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static bool HasEnabledCommand(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> descriptors,
        string commandId)
        => descriptors.TryGetValue(commandId, out var descriptor)
        && descriptor.IsEnabled;

    private static bool ContainsExecutableTool(
        IReadOnlyList<GraphEditorToolDescriptorSnapshot> tools,
        string toolId)
        => tools.Any(tool => string.Equals(tool.Id, toolId, StringComparison.Ordinal) && tool.CanExecute);

    private static bool HasViewportChanged(
        GraphEditorViewportSnapshot before,
        GraphEditorViewportSnapshot after)
        => before.Zoom != after.Zoom
        || before.PanX != after.PanX
        || before.PanY != after.PanY;

    private static bool HasCommandPaletteFocusRoundTrip(
        GraphEditorView view,
        Control focusOrigin,
        Border paletteChrome,
        TextBox paletteSearchBox)
    {
        focusOrigin.Focus();
        FlushUi();

        InvokeViewKeyDown(view, new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
            Source = focusOrigin,
        });
        FlushUi();

        if (!paletteChrome.IsVisible
            || !ReferenceEquals(TopLevel.GetTopLevel(view)?.FocusManager?.GetFocusedElement(), paletteSearchBox))
        {
            return false;
        }

        InvokeViewKeyDown(view, new KeyEventArgs
        {
            Key = Key.Escape,
        });
        FlushUi();

        return !paletteChrome.IsVisible
            && ReferenceEquals(TopLevel.GetTopLevel(view)?.FocusManager?.GetFocusedElement(), focusOrigin);
    }

    private static void InvokeViewKeyDown(GraphEditorView view, KeyEventArgs args)
    {
        var handler = typeof(GraphEditorView).GetMethod(
            "HandleKeyDown",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        handler?.Invoke(view, [view, args]);
    }

    private static void FlushUi()
        => Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

    private static bool HasMetadataProjection(IReadOnlyList<GraphEditorNodeParameterSnapshot> snapshots)
        => snapshots.Any(snapshot =>
            snapshot.Definition.Key == "status"
            && snapshot.Definition.ValueType == new PortTypeId("enum")
            && snapshot.Definition.EditorKind == ParameterEditorKind.Enum
            && string.Equals(snapshot.Definition.DefaultValue?.ToString(), "draft", StringComparison.Ordinal)
            && HasAllowedOptions(
                snapshot.Definition.Constraints.AllowedOptions,
                ("draft", "Draft"),
                ("review", "In Review"),
                ("approved", "Approved")))
        && snapshots.Any(snapshot =>
            snapshot.Definition.Key == "owner"
            && snapshot.Definition.ValueType == new PortTypeId("string")
            && snapshot.Definition.EditorKind == ParameterEditorKind.Text
            && string.Equals(snapshot.Definition.DefaultValue?.ToString(), "design-review", StringComparison.Ordinal))
        && snapshots.Any(snapshot =>
            snapshot.Definition.Key == "priority"
            && snapshot.Definition.ValueType == new PortTypeId("int")
            && snapshot.Definition.EditorKind == ParameterEditorKind.Number
            && Equals(snapshot.Definition.DefaultValue, 2)
            && snapshot.Definition.Constraints.Minimum == 1
            && snapshot.Definition.Constraints.Maximum == 5
            && !string.IsNullOrWhiteSpace(snapshot.NumberSliderHint))
        && snapshots.Any(snapshot =>
            snapshot.Definition.Key == "review-script"
            && snapshot.UsesMultilineTextInput
            && snapshot.IsCodeLikeText
            && !string.IsNullOrWhiteSpace(snapshot.HelpText));

    private static bool HasAllowedOptions(
        IReadOnlyList<ParameterOptionDefinition> options,
        params (string Value, string Label)[] expected)
        => options.Count == expected.Length
        && options.Zip(expected, static (actual, wanted) =>
            string.Equals(actual.Value, wanted.Value, StringComparison.Ordinal)
            && string.Equals(actual.Label, wanted.Label, StringComparison.Ordinal))
            .All(static matches => matches);

    private static bool HasRuntimeOverlaySnapshot(ConsumerSampleHost host)
    {
        var overlay = host.Session.Queries.GetRuntimeOverlaySnapshot();
        return overlay.IsAvailable
            && overlay.NodeOverlays.Any(snapshot =>
                snapshot.NodeId == "consumer-sample-review-001"
                && snapshot.Status == GraphEditorRuntimeOverlayStatus.Success
                && snapshot.ElapsedMilliseconds > 0
                && !string.IsNullOrWhiteSpace(snapshot.OutputPreview)
                && snapshot.LastRunAtUtc is not null)
            && overlay.ConnectionOverlays.Any(snapshot =>
                snapshot.ConnectionId == "consumer-sample-connection-001"
                && snapshot.Status == GraphEditorRuntimeOverlayStatus.Success
                && string.Equals(snapshot.PayloadType, "review", StringComparison.Ordinal)
                && snapshot.ItemCount == 1
                && !snapshot.IsStale)
            && overlay.RecentLogs.Any(snapshot =>
                snapshot.NodeId == "consumer-sample-queue-001"
                && snapshot.ConnectionId == "consumer-sample-connection-001"
                && snapshot.Status == GraphEditorRuntimeOverlayStatus.Success)
            && host.Session.Queries.GetFeatureDescriptors().Any(descriptor =>
                descriptor.Id == "integration.runtime-overlay-provider"
                && descriptor.IsAvailable);
    }

    private static bool HasRuntimeLogPanel(Window window, ConsumerSampleHost host)
    {
        var log = host.Session.Queries.GetRuntimeOverlaySnapshot().RecentLogs.SingleOrDefault(snapshot =>
            string.Equals(snapshot.Id, "consumer-sample-runtime-log-001", StringComparison.Ordinal));
        var selectedBefore = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;
        var navigated = log is not null && host.TryNavigateToRuntimeLog(log);
        var selectedAfter = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;
        if (selectedBefore is not null)
        {
            host.SelectNode(selectedBefore);
        }

        return FindNamed<TextBlock>(window, "PART_RuntimeSummaryText") is not null
            && FindNamed<ComboBox>(window, "PART_RuntimeLogFilter") is not null
            && FindNamed<ItemsControl>(window, "PART_RuntimeLogItems") is not null
            && FindNamed<Button>(window, "PART_ExportRuntimeLogsButton") is not null
            && navigated
            && string.Equals(selectedAfter, "consumer-sample-queue-001", StringComparison.Ordinal);
    }

    private static bool HasRuntimeLogFilter(ConsumerSampleHost host)
    {
        var overlay = host.Session.Queries.GetRuntimeOverlaySnapshot();
        var selectedNodeId = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;
        var allLogs = overlay.RecentLogs;
        var selectedLogs = selectedNodeId is null
            ? Array.Empty<GraphEditorRuntimeLogEntrySnapshot>()
            : allLogs.Where(log => string.Equals(log.NodeId, selectedNodeId, StringComparison.Ordinal)).ToArray();
        var scopeLogs = allLogs.Where(log => string.Equals(log.ScopeId, "root", StringComparison.Ordinal)).ToArray();

        return allLogs.Count > 0
            && selectedLogs.Length == 0
            && scopeLogs.Length == allLogs.Count;
    }

    private static bool HasRuntimeOverlaySupportBundlePayload(GraphEditorRuntimeOverlaySnapshot overlay)
        => overlay.IsAvailable
        && overlay.NodeOverlays.Count > 0
        && overlay.ConnectionOverlays.Count > 0
        && overlay.RecentLogs.Count > 0;

    private static IReadOnlyList<ConsumerSampleProofParameterSnapshot> CreateParameterSnapshots(
        IReadOnlyList<GraphEditorNodeParameterSnapshot> snapshots)
        => snapshots
            .Select(static snapshot => new ConsumerSampleProofParameterSnapshot(
                Key: snapshot.Definition.Key,
                ValueType: snapshot.Definition.ValueType.Value,
                EditorKind: snapshot.Definition.EditorKind.ToString(),
                CurrentValue: snapshot.CurrentValue,
                DefaultValue: snapshot.Definition.DefaultValue,
                HasMixedValues: snapshot.HasMixedValues,
                CanEdit: snapshot.CanEdit,
                IsValid: snapshot.IsValid,
                ValidationMessage: snapshot.ValidationMessage,
                ReadOnlyReason: snapshot.ReadOnlyReason,
                HelpText: snapshot.HelpText,
                GroupName: snapshot.GroupDisplayName,
                PlaceholderText: snapshot.Definition.PlaceholderText,
                IsAdvanced: snapshot.Definition.IsAdvanced,
                ValueState: snapshot.ValueState.ToString(),
                ValueDisplayText: snapshot.ValueDisplayText,
                UsesMultilineTextInput: snapshot.UsesMultilineTextInput,
                IsCodeLikeText: snapshot.IsCodeLikeText,
                SupportsEnumSearch: snapshot.SupportsEnumSearch,
                NumberSliderHint: snapshot.NumberSliderHint,
                CanApplyValidationFix: snapshot.CanApplyValidationFix,
                ValidationFixActionLabel: snapshot.ValidationFixActionLabel,
                AllowedOptions: snapshot.Definition.Constraints.AllowedOptions
                    .Select(static option => new ConsumerSampleProofParameterOptionSnapshot(option.Value, option.Label))
                    .ToArray(),
                Minimum: snapshot.Definition.Constraints.Minimum,
                Maximum: snapshot.Definition.Constraints.Maximum))
            .ToArray();

    private static IReadOnlyList<GraphEditorNodeParameterSnapshot> CaptureMixedValueSnapshots(ConsumerSampleHost host)
    {
        var reviewNodeIds = host.Session.Queries.CreateDocumentSnapshot().Nodes
            .Where(node => node.DefinitionId == ConsumerSampleHost.ReviewDefinitionId)
            .Select(node => node.Id)
            .Take(2)
            .ToArray();
        if (reviewNodeIds.Length < 2)
        {
            return [];
        }

        host.Session.Commands.SetSelection(reviewNodeIds, reviewNodeIds[0], updateStatus: false);
        return host.Session.Queries.GetSelectedNodeParameterSnapshots()
            .Where(snapshot => snapshot.HasMixedValues)
            .ToArray();
    }

    private static IReadOnlyList<GraphEditorNodeParameterSnapshot> CreateValidationFixSnapshots()
    {
        var invalidNodeId = "consumer-sample.validation.invalid-node";
        var definitionId = new NodeDefinitionId("consumer.sample.validation-fix");
        var catalog = new AsterGraph.Editor.Catalog.NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Validation Fix Node",
            "Consumer Sample",
            "Validation",
            [],
            [],
            [
                new NodeParameterDefinition(
                    "slug",
                    "Slug",
                    new PortTypeId("string"),
                    ParameterEditorKind.Text,
                    defaultValue: "valid-slug",
                    constraints: new ParameterConstraints(
                        MinimumLength: 3,
                        ValidationPattern: "^[a-z-]+$",
                        ValidationPatternDescription: "lowercase letters and dashes"),
                    groupName: "Metadata",
                    helpText: "Stable lowercase identifier."),
            ]));
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Validation Fix",
                "Parameter validation fix proof.",
                [
                    new GraphNode(
                        invalidNodeId,
                        "Validation Fix Node",
                        "Consumer Sample",
                        "Validation",
                        "Contains invalid persisted parameter data.",
                        new GraphPoint(0, 0),
                        new GraphSize(220, 140),
                        [],
                        [],
                        "#6AD5C4",
                        definitionId,
                        [new GraphParameterValue("slug", new PortTypeId("string"), "ab")]),
                ],
                []),
            NodeCatalog = catalog,
            CompatibilityService = new AsterGraph.Core.Compatibility.DefaultPortCompatibilityService(),
        });
        session.Commands.SetSelection([invalidNodeId], invalidNodeId, updateStatus: false);
        return session.Queries.GetSelectedNodeParameterSnapshots()
            .Where(snapshot => snapshot.CanApplyValidationFix)
            .ToArray();
    }
}

public sealed record ConsumerSampleProofParameterSnapshot(
    string Key,
    string ValueType,
    string EditorKind,
    object? CurrentValue,
    object? DefaultValue,
    bool HasMixedValues,
    bool CanEdit,
    bool IsValid,
    string? ValidationMessage,
    string? ReadOnlyReason,
    string? HelpText,
    string GroupName,
    string? PlaceholderText,
    bool IsAdvanced,
    string ValueState,
    string ValueDisplayText,
    bool UsesMultilineTextInput,
    bool IsCodeLikeText,
    bool SupportsEnumSearch,
    string? NumberSliderHint,
    bool CanApplyValidationFix,
    string? ValidationFixActionLabel,
    IReadOnlyList<ConsumerSampleProofParameterOptionSnapshot> AllowedOptions,
    double? Minimum,
    double? Maximum);

public sealed record ConsumerSampleProofParameterOptionSnapshot(
    string Value,
    string Label);

public static class ConsumerSampleHeadlessEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized || Application.Current is not null)
        {
            _initialized = true;
            return;
        }

        AppBuilder.Configure<ConsumerSampleProofApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();

        _initialized = true;
    }
}

internal sealed class ConsumerSampleProofApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
