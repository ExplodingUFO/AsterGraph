using System;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.ConsumerSample;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.ConsumerSample.Tests;

public sealed class ConsumerSampleProofTests
{
    [AvaloniaFact]
    public void ConsumerSampleHost_CombinesHostNodesPluginContributionsAndParameterEditing()
    {
        using var host = ConsumerSampleHost.Create();

        var initialSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        Assert.True(host.PluginLoadSnapshots.Count > 0);
        Assert.Contains(host.PluginLoadSnapshots, snapshot => snapshot.Descriptor?.Id == "consumer.sample.audit-plugin");

        host.AddHostReviewNode();
        host.AddPluginAuditNode();

        var updatedSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        Assert.Equal(initialSnapshot.Nodes.Count + 2, updatedSnapshot.Nodes.Count);

        var reviewNodeId = host.GetFirstReviewNodeId();
        host.Session.Commands.SetSelection([reviewNodeId], reviewNodeId, updateStatus: false);

        var parameterSnapshots = host.Session.Queries.GetSelectedNodeParameterSnapshots();
        Assert.Contains(parameterSnapshots, snapshot => snapshot.Definition.Key == "status");
        Assert.Contains(parameterSnapshots, snapshot => snapshot.Definition.Key == "owner");

        Assert.True(host.ApproveSelection());

        var approvedStatus = host.Session.Queries.GetSelectedNodeParameterSnapshots()
            .Single(snapshot => snapshot.Definition.Key == "status");
        Assert.Equal("approved", approvedStatus.CurrentValue);
    }

    [AvaloniaFact]
    public void ConsumerSampleHost_StartsWithCopyableScenarioGraphAndOnboardingRoute()
    {
        using var host = ConsumerSampleHost.Create();

        var document = host.Session.Queries.CreateDocumentSnapshot();
        var reviewNode = Assert.Single(document.Nodes, node => node.DefinitionId == ConsumerSampleHost.ReviewDefinitionId);
        var queueNode = Assert.Single(document.Nodes, node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId);
        var connection = Assert.Single(document.Connections);
        var commandIds = host.Session.Queries.GetCommandDescriptors()
            .Select(static descriptor => descriptor.Id)
            .ToArray();

        Assert.Equal(ConsumerSampleHost.ScenarioTitle, document.Title);
        Assert.Contains("content-review scenario", document.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("draft", reviewNode.ParameterValues?.Single(parameter => parameter.Key == "status").Value);
        Assert.Equal("design-review", reviewNode.ParameterValues?.Single(parameter => parameter.Key == "owner").Value);
        Assert.Equal(reviewNode.Id, connection.SourceNodeId);
        Assert.Equal(queueNode.Id, connection.TargetNodeId);
        Assert.Contains(host.OnboardingCopyPathLines, line => line.Contains("Starter.Avalonia", StringComparison.Ordinal));
        Assert.Contains(host.OnboardingCopyPathLines, line => line.Contains("HelloWorld.Avalonia", StringComparison.Ordinal));
        Assert.Contains(host.OnboardingCopyPathLines, line => line.Contains("ConsumerSample.Avalonia", StringComparison.Ordinal));
        Assert.Contains(host.OnboardingCopyPathLines, line => line.Contains("Demo", StringComparison.Ordinal));
        Assert.Contains(host.RouteBoundaryLines, line => line.Contains("Hosted UI route", StringComparison.Ordinal) && line.Contains("AsterGraphAvaloniaViewFactory.Create", StringComparison.Ordinal));
        Assert.Contains(host.RouteBoundaryLines, line => line.Contains("Runtime-only route", StringComparison.Ordinal) && line.Contains("CreateSession", StringComparison.Ordinal));
        Assert.Contains(host.RouteBoundaryLines, line => line.Contains("Plugin route", StringComparison.Ordinal) && line.Contains("PluginTrustPolicy", StringComparison.Ordinal));
        Assert.Contains(host.RouteBoundaryLines, line => line.Contains("Migration route", StringComparison.Ordinal) && line.Contains("retained migration", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(ConsumerSampleHost.PluginCommandId, commandIds);
        Assert.Contains("workspace.save", commandIds);
        Assert.Contains("workspace.load", commandIds);
        Assert.Contains(host.PluginCandidateEntries, entry => entry.PluginId == "consumer.sample.audit-plugin" && entry.IsAllowed);
        Assert.Contains(host.LocalPluginGalleryEntries, entry =>
            entry.PluginId == "consumer.sample.audit-plugin"
            && entry.IsAllowed
            && entry.IsLoaded
            && entry.GalleryLine.Contains("fingerprint", StringComparison.OrdinalIgnoreCase));
    }

    [AvaloniaFact]
    public void ConsumerSampleHost_ExposesSnippetCatalogAndInsertsConnectedQueueLane()
    {
        using var host = ConsumerSampleHost.Create();

        var snippet = Assert.Single(host.SnippetCatalog, entry => entry.Id == ConsumerSampleHost.QueueLaneSnippetId);
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var sourceNode = before.Nodes.Single(node => node.DefinitionId == ConsumerSampleHost.ReviewDefinitionId);

        Assert.Contains("Queue", snippet.Title, StringComparison.Ordinal);
        Assert.Contains("review", snippet.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Workflow", snippet.Category);
        Assert.Contains("Review output", snippet.PreviewText, StringComparison.Ordinal);
        Assert.True(snippet.IsFavorite);
        Assert.Contains("connected", snippet.SearchKeywords);
        Assert.Contains(host.SearchSnippetCatalog("queue"), entry => entry.Id == ConsumerSampleHost.QueueLaneSnippetId);
        Assert.Contains(host.SearchSnippetCatalog("diagnostics"), entry => entry.Category == "Diagnostics");
        Assert.Equal(snippet.PreviewText, host.GetSnippetPreview(snippet.Id));
        Assert.Contains(ConsumerSampleHost.QueueLaneSnippetId, host.FavoriteSnippetIds);
        Assert.Empty(host.RecentSnippetIds);

        Assert.True(host.TryInsertSnippet(snippet.Id));
        Assert.Equal(ConsumerSampleHost.QueueLaneSnippetId, Assert.Single(host.RecentSnippetIds));

        var after = host.Session.Queries.CreateDocumentSnapshot();
        var createdQueueNode = Assert.Single(after.Nodes, node =>
            node.DefinitionId == ConsumerSampleHost.QueueDefinitionId
            && before.Nodes.All(existing => !string.Equals(existing.Id, node.Id, StringComparison.Ordinal)));

        Assert.Equal(before.Nodes.Count + 1, after.Nodes.Count);
        Assert.Equal(before.Connections.Count + 1, after.Connections.Count);
        Assert.Contains(after.Connections, connection =>
            string.Equals(connection.SourceNodeId, sourceNode.Id, StringComparison.Ordinal)
            && string.Equals(connection.SourcePortId, "output", StringComparison.Ordinal)
            && string.Equals(connection.TargetNodeId, createdQueueNode.Id, StringComparison.Ordinal)
            && string.Equals(connection.TargetPortId, "input", StringComparison.Ordinal));
        Assert.False(host.Session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);
    }

    [AvaloniaFact]
    public void ConsumerSampleProof_Run_EmitsGreenMarkers()
    {
        var result = ConsumerSampleProof.Run();

        Assert.True(result.IsOk, string.Join(Environment.NewLine, result.ProofLines));
        Assert.True(result.CapabilityBreadthOk);
        Assert.True(result.NodeSideAuthoringOk);
        Assert.True(result.HostMenuActionOk);
        Assert.True(result.PluginContributionOk);
        Assert.True(result.ParameterProjectionOk);
        Assert.True(result.MetadataProjectionOk);
        Assert.True(result.InspectorMetadataPolishOk);
        Assert.True(result.InspectorMixedValueOk);
        Assert.True(result.InspectorValidationFixOk);
        Assert.True(result.SupportBundleParameterEvidenceOk);
        Assert.True(result.QuickAddConnectedNodeOk);
        Assert.True(result.PortFilteredNodeSearchOk);
        Assert.True(result.TrustTransparencyOk);
        Assert.True(result.CommandSurfaceOk);
        Assert.True(result.StencilSurfaceOk);
        Assert.True(result.StencilGroupingOk);
        Assert.True(result.StencilSearchOk);
        Assert.True(result.StencilRecentsFavoritesOk);
        Assert.True(result.StencilSourceFilterOk);
        Assert.True(result.StencilStatePersistenceOk);
        Assert.True(result.ExportBreadthOk);
        Assert.True(result.NodeQuickToolsOk);
        Assert.True(result.EdgeQuickToolsOk);
        Assert.True(result.HostedAccessibilityBaselineOk);
        Assert.True(result.HostedAccessibilityFocusOk);
        Assert.True(result.HostedAutomationNavigationOk);
        Assert.True(result.HostedAuthoringAutomationDiagnosticsOk);
        Assert.True(result.HostedAccessibilityAutomationOk);
        Assert.True(result.HostedAccessibilityCommandSurfaceOk);
        Assert.True(result.HostedAccessibilityAuthoringSurfaceOk);
        Assert.True(result.HostedAccessibilityOk);
        Assert.True(result.ScenarioGraphOk);
        Assert.True(result.HostOwnedActionsOk);
        Assert.True(result.SupportBundlePayloadOk);
        Assert.True(result.FiveMinuteOnboardingOk);
        Assert.True(result.RuntimeOverlaySnapshotOk);
        Assert.True(result.RuntimeOverlaySnapshotPolishOk);
        Assert.True(result.RuntimeOverlayScopeFilterOk);
        Assert.True(result.RuntimeLogPanelOk);
        Assert.True(result.RuntimeLogFilterOk);
        Assert.True(result.RuntimeDebugPanelInteractionOk);
        Assert.True(result.RuntimeLogLocateOk);
        Assert.True(result.RuntimeLogExportOk);
        Assert.True(result.GraphSearchLocateOk);
        Assert.True(result.GraphSearchScopeFilterOk);
        Assert.True(result.GraphSearchViewportFocusOk);
        Assert.True(result.UnifiedDiscoverySurfaceOk);
        Assert.True(result.DiscoverySourceLabelsOk);
        Assert.True(result.DiscoveryCommandRouteOk);
        Assert.True(result.WorkbenchRecentsOk);
        Assert.True(result.WorkbenchFavoritesOk);
        Assert.True(result.RecentsFavoritesSupportBundleOk);
        Assert.NotNull(result.RecentsFavoritesEvidence);
        Assert.Contains(result.RecentsFavoritesEvidence!, entry => entry.Surface == "node");
        Assert.Contains(result.RecentsFavoritesEvidence!, entry => entry.Surface == "fragment");
        Assert.Contains(result.RecentsFavoritesEvidence!, entry => entry.Surface == "command");
        Assert.Contains(result.RecentsFavoritesEvidence!, entry => entry.Surface == "plugin");
        Assert.True(result.WorkbenchFrictionEvidenceOk);
        Assert.True(result.WorkbenchFrictionPrioritizationOk);
        Assert.True(result.WorkbenchFrictionScopeBoundaryOk);
        Assert.NotNull(result.WorkbenchFrictionEvidence);
        Assert.Contains(result.WorkbenchFrictionEvidence!, entry => entry.Category == "layout-resume" && entry.PriorityRank == 1);
        Assert.Contains(result.WorkbenchFrictionEvidence!, entry => entry.Category == "support-triage" && entry.IsSynthetic);
        Assert.True(result.WorkbenchAffordancePolishOk);
        Assert.True(result.WorkbenchAffordanceRouteOk);
        Assert.True(result.WorkbenchAffordanceScopeBoundaryOk);
        Assert.NotNull(result.WorkbenchAffordancePolish);
        Assert.Equal("layout-resume", result.WorkbenchAffordancePolish!.FrictionCategory);
        Assert.True(result.WorkbenchFrictionSupportBundleOk);
        Assert.True(result.WorkbenchAdopterEvidenceAttachmentOk);
        Assert.True(result.WorkbenchEvidenceScopeBoundaryOk);
        Assert.True(result.WorkbenchAdopterPolishHandoffOk);
        Assert.True(result.WorkbenchAdopterPolishScopeBoundaryOk);
        Assert.True(result.V064MilestoneProofOk);
        Assert.True(result.CommandPaletteGroupingOk);
        Assert.True(result.CommandPaletteDisabledReasonOk);
        Assert.True(result.CommandPaletteRecentActionsOk);
        Assert.True(result.CommandProjectionUnifiedOk);
        Assert.True(result.CommandPaletteOk);
        Assert.True(result.ToolbarDescriptorOk);
        Assert.True(result.ContextMenuDescriptorOk);
        Assert.True(result.CommandDisabledReasonOk);
        Assert.True(result.InteractionFeedbackOk);
        Assert.True(result.CanvasFocusRecoveryOk);
        Assert.True(result.ConsumerGestureProofOk);
        Assert.True(result.InteractionSupportBundleOk);
        Assert.True(result.InteractionReliabilityHandoffOk);
        Assert.True(result.InteractionScopeBoundaryOk);
        Assert.True(result.V062MilestoneProofOk);
        Assert.True(result.WorkbenchDiscoverabilityHandoffOk);
        Assert.True(result.WorkbenchDiscoverabilityScopeBoundaryOk);
        Assert.True(result.V063MilestoneProofOk);
        Assert.True(result.NodeToolbarContributionOk);
        Assert.True(result.EdgeToolbarContributionOk);
        Assert.True(result.ToolbarContributionDescriptorOk);
        Assert.True(result.ToolbarContributionScopeBoundaryOk);
        Assert.True(result.NavigationHistoryOk);
        Assert.True(result.ScopeBreadcrumbNavigationOk);
        Assert.True(result.FocusRestoreOk);
        Assert.True(result.NavigationProductivityProofOk);
        Assert.True(result.NavigationProductivityHandoffOk);
        Assert.True(result.NavigationScopeBoundaryOk);
        Assert.True(result.GraphSnippetCatalogOk);
        Assert.True(result.GraphSnippetInsertOk);
        Assert.True(result.FragmentLibrarySearchOk);
        Assert.True(result.FragmentLibraryPreviewOk);
        Assert.True(result.FragmentLibraryRecentsFavoritesOk);
        Assert.True(result.FragmentLibraryScopeBoundaryOk);
        Assert.True(result.WorkbenchDefaultsOk);
        Assert.True(result.WorkbenchHostBuilderHandoffOk);
        Assert.True(result.WorkbenchLayoutPresetsOk);
        Assert.True(result.WorkbenchLayoutResetOk);
        Assert.True(result.PanelStatePersistenceOk);
        Assert.True(result.WorkbenchScopeBoundaryOk);
        Assert.True(result.RuntimeOverlaySupportBundleOk);
        Assert.True(result.OnboardingConfigurationOk);
        Assert.True(result.ExperiencePolishHandoffOk);
        Assert.True(result.FeatureEnhancementProofOk);
        Assert.True(result.AuthoringFlowProofOk);
        Assert.True(result.AuthoringFlowHandoffOk);
        Assert.True(result.AuthoringFlowScopeBoundaryOk);
        Assert.True(result.ExperienceScopeBoundaryOk);
        Assert.True(result.AuthoringDepthHandoffOk);
        Assert.True(result.AuthoringDepthScopeBoundaryOk);
        Assert.True(result.V058MilestoneProofOk);
        Assert.True(result.LargeGraphUxPolicyOk);
        Assert.True(result.LargeGraphUxScopeBoundaryOk);
        Assert.True(result.LargeGraphUxProofBaselineOk);
        Assert.True(result.ViewportLodPolicyOk);
        Assert.True(result.SelectedHoveredAdornerScopeOk);
        Assert.True(result.LargeGraphBalancedUxOk);
        Assert.True(result.ViewportLodScopeBoundaryOk);
        Assert.True(result.EdgeInteractionCacheOk);
        Assert.True(result.EdgeDragRouteSimplificationOk);
        Assert.True(result.SelectedEdgeFeedbackOk);
        Assert.True(result.EdgeRenderingScopeBoundaryOk);
        Assert.True(result.NodeDragEdgeRefreshOk);
        Assert.True(result.EdgeRouteRefreshOk);
        Assert.True(result.LiveCanvasRefreshAuditOk);
        Assert.True(result.MiniMapLightweightProjectionEvidenceOk);
        Assert.True(result.MinimapLightweightProjectionOk);
        Assert.True(result.InspectorNarrowProjectionOk);
        Assert.True(result.LargeGraphPanelScopeOk);
        Assert.True(result.ProjectionPerformanceEvidenceOk);
        Assert.True(result.LargeGraphUxHandoffOk);
        Assert.True(result.V059MilestoneProofOk);
        Assert.True(result.SelectedParameterProjectionCount > 0);
        Assert.True(result.TotalParameterProjectionCount > result.SelectedParameterProjectionCount);
        Assert.True(result.StartupMs >= 0);
        Assert.True(result.InspectorProjectionMs >= 0);
        Assert.True(result.PluginScanMs >= 0);
        Assert.True(result.CommandLatencyMs >= 0);
        Assert.True(result.StencilSearchMs >= 0);
        Assert.True(result.CommandSurfaceRefreshMs >= 0);
        Assert.True(result.NodeToolProjectionMs >= 0);
        Assert.True(result.EdgeToolProjectionMs >= 0);
        Assert.True(result.CommandPaletteMs >= 0);
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_METADATA_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INSPECTOR_METADATA_POLISH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INSPECTOR_MIXED_VALUE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INSPECTOR_VALIDATION_FIX_OK:True");
        Assert.Contains(result.ProofLines, line => line == "SUPPORT_BUNDLE_PARAMETER_EVIDENCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_QUICK_ADD_CONNECTED_NODE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_PORT_FILTERED_NODE_SEARCH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_DROP_NODE_ON_EDGE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_EDGE_SPLIT_COMPATIBILITY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_EDGE_SPLIT_UNDO_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_DELETE_AND_RECONNECT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_DETACH_NODE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_RECONNECT_CONFLICT_REPORT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_EDGE_MULTISELECT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_WIRE_SLICE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SELECTED_NODE_EDGE_HIGHLIGHT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_OVERLAY_SNAPSHOT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_OVERLAY_SNAPSHOT_POLISH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_OVERLAY_SCOPE_FILTER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_LOG_PANEL_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_LOG_FILTER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_DEBUG_PANEL_INTERACTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_LOG_LOCATE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_LOG_EXPORT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PORT_HANDLE_ID_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PORT_GROUP_AUTHORING_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PORT_CONNECTION_HINT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PORT_AUTHORING_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONNECTION_VALIDATION_REASON_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONNECTION_INVALID_HOVER_FEEDBACK_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONNECTION_VALIDATION_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONNECTION_VALIDATION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_SEARCH_LOCATE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_SEARCH_SCOPE_FILTER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "UNIFIED_DISCOVERY_SURFACE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "DISCOVERY_SOURCE_LABELS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "DISCOVERY_COMMAND_ROUTE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_RECENTS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_FAVORITES_OK:True");
        Assert.Contains(result.ProofLines, line => line == "RECENTS_FAVORITES_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_FRICTION_EVIDENCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_FRICTION_PRIORITIZATION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_FRICTION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_AFFORDANCE_POLISH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_AFFORDANCE_ROUTE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_AFFORDANCE_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_FRICTION_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_ADOPTER_EVIDENCE_ATTACHMENT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_EVIDENCE_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_ADOPTER_POLISH_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_ADOPTER_POLISH_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "V064_MILESTONE_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "COMMAND_PALETTE_GROUPING_OK:True");
        Assert.Contains(result.ProofLines, line => line == "COMMAND_PALETTE_DISABLED_REASON_OK:True");
        Assert.Contains(result.ProofLines, line => line == "COMMAND_PALETTE_RECENT_ACTIONS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "COMMAND_PROJECTION_UNIFIED_OK:True");
        Assert.Contains(result.ProofLines, line => line == "COMMAND_PALETTE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "TOOLBAR_DESCRIPTOR_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONTEXT_MENU_DESCRIPTOR_OK:True");
        Assert.Contains(result.ProofLines, line => line == "COMMAND_DISABLED_REASON_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INTERACTION_FEEDBACK_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CANVAS_FOCUS_RECOVERY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_GESTURE_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INTERACTION_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INTERACTION_RELIABILITY_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INTERACTION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "V062_MILESTONE_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_DISCOVERABILITY_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_DISCOVERABILITY_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "V063_MILESTONE_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "NODE_TOOLBAR_CONTRIBUTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EDGE_TOOLBAR_CONTRIBUTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:True");
        Assert.Contains(result.ProofLines, line => line == "TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "NAVIGATION_HISTORY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "SCOPE_BREADCRUMB_NAVIGATION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "FOCUS_RESTORE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "NAVIGATION_PRODUCTIVITY_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "NAVIGATION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LAYOUT_PROVIDER_SEAM_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LAYOUT_PREVIEW_APPLY_CANCEL_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LAYOUT_UNDO_TRANSACTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "READABILITY_FOCUS_SUBGRAPH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "READABILITY_ROUTE_CLEANUP_OK:True");
        Assert.Contains(result.ProofLines, line => line == "READABILITY_ALIGNMENT_HELPERS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PLUGIN_LOCAL_GALLERY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PLUGIN_TRUST_EVIDENCE_PANEL_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PLUGIN_ALLOWLIST_ROUNDTRIP_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PLUGIN_SAMPLE_PACK_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PLUGIN_SAMPLE_NODE_DEFINITIONS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PLUGIN_SAMPLE_PARAMETER_METADATA_OK:True");
        Assert.Contains(result.ProofLines, line => line == "NODE_DEFINITION_BUILDER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PORT_DEFINITION_BUILDER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PARAMETER_DEFINITION_BUILDER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONNECTION_RULE_BUILDER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_BUILDER_THIN_WRAPPER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_SNIPPET_CATALOG_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_SNIPPET_INSERT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "FRAGMENT_LIBRARY_SEARCH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "FRAGMENT_LIBRARY_PREVIEW_OK:True");
        Assert.Contains(result.ProofLines, line => line == "FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK:True");
        Assert.Contains(result.ProofLines, line => line == "FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_DEFAULTS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_HOST_BUILDER_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_PERFORMANCE_MODE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "BALANCED_MODE_DEFAULT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_LOD_POLICY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_LAYOUT_PRESETS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_LAYOUT_RESET_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PANEL_STATE_PERSISTENCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_STENCIL_OK:True");
        Assert.Contains(result.ProofLines, line => line == "STENCIL_GROUPING_OK:True");
        Assert.Contains(result.ProofLines, line => line == "STENCIL_SEARCH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "STENCIL_RECENTS_FAVORITES_OK:True");
        Assert.Contains(result.ProofLines, line => line == "STENCIL_SOURCE_FILTER_OK:True");
        Assert.Contains(result.ProofLines, line => line == "STENCIL_STATE_PERSISTENCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_EXPORT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EXPORT_PANEL_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EXPORT_PANEL_SCOPE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EXPORT_PANEL_PROGRESS_CANCEL_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EXPORT_DOCS_ALIGNMENT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_BASELINE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_FOCUS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WIDENED_SURFACE_PERFORMANCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_VALIDATION_FEEDBACK_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_FEEDBACK_FOCUS_TARGET_OK:True");
        Assert.Contains(result.ProofLines, line => line == "GRAPH_READINESS_STATUS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "FIVE_MINUTE_ONBOARDING_OK:True");
        Assert.Contains(result.ProofLines, line => line == "ONBOARDING_CONFIGURATION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EXPERIENCE_POLISH_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "FEATURE_ENHANCEMENT_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_FLOW_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_FLOW_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EXPERIENCE_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_DEPTH_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "V058_MILESTONE_PROOF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LARGE_GRAPH_UX_POLICY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LARGE_GRAPH_UX_PROOF_BASELINE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "VIEWPORT_LOD_POLICY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "SELECTED_HOVERED_ADORNER_SCOPE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LARGE_GRAPH_BALANCED_UX_OK:True");
        Assert.Contains(result.ProofLines, line => line == "VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EDGE_INTERACTION_CACHE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "SELECTED_EDGE_FEEDBACK_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EDGE_RENDERING_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(result.ProofLines, line => line == "NODE_DRAG_EDGE_REFRESH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "EDGE_ROUTE_REFRESH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LIVE_CANVAS_REFRESH_AUDIT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "MINIMAP_LIGHTWEIGHT_PROJECTION_EVIDENCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "INSPECTOR_NARROW_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LARGE_GRAPH_PANEL_SCOPE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "PROJECTION_PERFORMANCE_EVIDENCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "LARGE_GRAPH_UX_HANDOFF_OK:True");
        Assert.Contains(result.ProofLines, line => line == "V059_MILESTONE_PROOF_OK:True");
        Assert.Contains(result.MetricLines, line => line.Contains("startup_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("command_latency_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("stencil_search_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("command_surface_refresh_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("node_tool_projection_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("edge_tool_projection_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("command_palette_ms", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_ForbiddenFeatureDescriptor_FailsScopeBoundaryMarker()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: true,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Ready",
            ValidationSummary: ConsumerSampleProofValidationSummary.Empty,
            ValidationFeedback: [],
            NodeCount: 2,
            FeatureDescriptorIds: ["capability.export.scene-svg", "marketplace.remote-install"]);

        Assert.False(result.ExperienceScopeBoundaryOk);
        Assert.False(result.AuthoringFlowScopeBoundaryOk);
        Assert.False(result.WorkbenchScopeBoundaryOk);
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_FLOW_SCOPE_BOUNDARY_OK:False");
        Assert.Contains(result.ProofLines, line => line == "EXPERIENCE_SCOPE_BOUNDARY_OK:False");
        Assert.Contains(result.ProofLines, line => line == "WORKBENCH_SCOPE_BOUNDARY_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_MetadataProjectionMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: false,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: true,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Ready",
            ValidationSummary: ConsumerSampleProofValidationSummary.Empty,
            ValidationFeedback: []);

        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_METADATA_PROJECTION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_METADATA_PROJECTION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_OnboardingMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: true,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Ready",
            ValidationSummary: ConsumerSampleProofValidationSummary.Empty,
            ValidationFeedback: [],
            NodeCount: 2,
            FeatureDescriptorIds: ["capability.export.scene-svg"],
            ScenarioGraphOk: false);

        Assert.False(result.OnboardingConfigurationOk);
        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:False");
        Assert.Contains(result.ProofLines, line => line == "ONBOARDING_CONFIGURATION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_CapabilityBreadthMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: false,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: true,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Ready",
            ValidationSummary: ConsumerSampleProofValidationSummary.Empty,
            ValidationFeedback: []);

        Assert.False(result.CapabilityBreadthOk);
        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_EXPORT_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_OK:False");
        Assert.Contains(result.ProofLines, line => line == "WIDENED_SURFACE_PERFORMANCE_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_AccessibilityMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: false,
            HostedAutomationNavigationOk: true,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Ready",
            ValidationSummary: ConsumerSampleProofValidationSummary.Empty,
            ValidationFeedback: [],
            HostedAuthoringAutomationDiagnosticsOk: true);

        Assert.False(result.HostedAccessibilityOk);
        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_FOCUS_OK:False");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_HostedAutomationNavigationMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: true,
            HostedAutomationNavigationOk: false,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Ready",
            ValidationSummary: ConsumerSampleProofValidationSummary.Empty,
            ValidationFeedback: [],
            HostedAuthoringAutomationDiagnosticsOk: true);

        Assert.False(result.HostedAccessibilityOk);
        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_HostedAuthoringDiagnosticsMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: true,
            HostedAutomationNavigationOk: true,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Ready",
            ValidationSummary: ConsumerSampleProofValidationSummary.Empty,
            ValidationFeedback: [],
            HostedAuthoringAutomationDiagnosticsOk: false);

        Assert.False(result.HostedAccessibilityOk);
        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:False");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "HOSTED_ACCESSIBILITY_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProof_CanWriteSupportBundleContract()
    {
        var result = ConsumerSampleProof.Run();
        var tempRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var bundlePath = Path.Combine(tempRoot, "consumer-support-bundle.json");

        ConsumerSampleSupportBundle.WriteProofBundle(
            bundlePath,
            result,
            "dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle consumer-support-bundle.json",
            "repro-note");

        Assert.True(File.Exists(bundlePath));

        using var document = JsonDocument.Parse(File.ReadAllText(bundlePath));
        var root = document.RootElement;
        var packageVersion = root.GetProperty("packageVersion").GetString();
        var publicTag = root.GetProperty("publicTag").GetString();
        var generatedAtUtc = root.GetProperty("generatedAtUtc").GetString();
        var environment = root.GetProperty("environment");
        var reproduction = root.GetProperty("reproduction");
        var persistenceStatus = root.GetProperty("persistenceStatus").GetString();
        var parameterSnapshots = root.GetProperty("parameterSnapshots").EnumerateArray().ToArray();
        var readinessStatus = root.GetProperty("readinessStatus").GetString();
        var validationSummary = root.GetProperty("validationSummary");
        var validationFeedback = root.GetProperty("validationFeedback").EnumerateArray().ToArray();

        Assert.Equal(4, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("ConsumerSample.Avalonia", root.GetProperty("route").GetString());
        Assert.False(string.IsNullOrWhiteSpace(packageVersion));
        Assert.Equal($"v{packageVersion}", publicTag);
        Assert.Equal("written", persistenceStatus);
        Assert.True(parameterSnapshots.Length >= 7);
        Assert.True(
            new[] { "Ready", "Warnings", "Blocked" }.Contains(readinessStatus, StringComparer.Ordinal),
            $"Unexpected readiness status '{readinessStatus}'.");
        Assert.Equal(validationFeedback.Length, validationSummary.GetProperty("totalIssueCount").GetInt32());
        Assert.True(validationSummary.GetProperty("errorCount").GetInt32() >= 0);
        Assert.True(validationSummary.GetProperty("warningCount").GetInt32() >= 0);
        Assert.True(validationSummary.GetProperty("invalidConnectionCount").GetInt32() >= 0);
        Assert.True(validationSummary.GetProperty("invalidParameterCount").GetInt32() >= 0);
        Assert.All(validationFeedback, feedback =>
        {
            Assert.False(string.IsNullOrWhiteSpace(feedback.GetProperty("code").GetString()));
            var severity = feedback.GetProperty("severity").GetString();
            Assert.True(
                new[] { "Error", "Warning" }.Contains(severity, StringComparer.Ordinal),
                $"Unexpected validation severity '{severity}'.");
            Assert.False(string.IsNullOrWhiteSpace(feedback.GetProperty("message").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(feedback.GetProperty("focusTarget").GetProperty("kind").GetString()));
        });
        Assert.True(
            DateTimeOffset.TryParse(generatedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _),
            "Support bundle generatedAtUtc should be a parseable round-trip UTC timestamp.");
        var proofLines = root.GetProperty("proofLines").EnumerateArray().Select(static item => item.GetString()).ToArray();
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_HOST_ACTION_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_PLUGIN_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_METADATA_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "INSPECTOR_METADATA_POLISH_OK:True");
        Assert.Contains(proofLines, line => line == "INSPECTOR_MIXED_VALUE_OK:True");
        Assert.Contains(proofLines, line => line == "INSPECTOR_VALIDATION_FIX_OK:True");
        Assert.Contains(proofLines, line => line == "SUPPORT_BUNDLE_PARAMETER_EVIDENCE_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_QUICK_ADD_CONNECTED_NODE_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_PORT_FILTERED_NODE_SEARCH_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_DROP_NODE_ON_EDGE_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_EDGE_SPLIT_COMPATIBILITY_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_EDGE_SPLIT_UNDO_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_DELETE_AND_RECONNECT_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_DETACH_NODE_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_RECONNECT_CONFLICT_REPORT_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_EDGE_MULTISELECT_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_WIRE_SLICE_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SELECTED_NODE_EDGE_HIGHLIGHT_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_OVERLAY_SNAPSHOT_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_OVERLAY_SNAPSHOT_POLISH_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_OVERLAY_SCOPE_FILTER_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_LOG_PANEL_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_LOG_FILTER_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_DEBUG_PANEL_INTERACTION_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_LOG_LOCATE_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_LOG_EXPORT_OK:True");
        Assert.Contains(proofLines, line => line == "RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_SEARCH_LOCATE_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_SEARCH_SCOPE_FILTER_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_SEARCH_VIEWPORT_FOCUS_OK:True");
        Assert.Contains(proofLines, line => line == "UNIFIED_DISCOVERY_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line == "DISCOVERY_SOURCE_LABELS_OK:True");
        Assert.Contains(proofLines, line => line == "DISCOVERY_COMMAND_ROUTE_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_RECENTS_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_FAVORITES_OK:True");
        Assert.Contains(proofLines, line => line == "RECENTS_FAVORITES_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_PALETTE_GROUPING_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_PALETTE_DISABLED_REASON_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_PALETTE_RECENT_ACTIONS_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_PROJECTION_UNIFIED_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_PALETTE_OK:True");
        Assert.Contains(proofLines, line => line == "TOOLBAR_DESCRIPTOR_OK:True");
        Assert.Contains(proofLines, line => line == "CONTEXT_MENU_DESCRIPTOR_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_DISABLED_REASON_OK:True");
        Assert.Contains(proofLines, line => line == "INTERACTION_FEEDBACK_OK:True");
        Assert.Contains(proofLines, line => line == "CANVAS_FOCUS_RECOVERY_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_GESTURE_PROOF_OK:True");
        Assert.Contains(proofLines, line => line == "INTERACTION_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(proofLines, line => line == "INTERACTION_RELIABILITY_HANDOFF_OK:True");
        Assert.Contains(proofLines, line => line == "INTERACTION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "V062_MILESTONE_PROOF_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_DISCOVERABILITY_HANDOFF_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_DISCOVERABILITY_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "V063_MILESTONE_PROOF_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_FRICTION_EVIDENCE_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_FRICTION_PRIORITIZATION_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_FRICTION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_AFFORDANCE_POLISH_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_AFFORDANCE_ROUTE_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_AFFORDANCE_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_FRICTION_SUPPORT_BUNDLE_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_ADOPTER_EVIDENCE_ATTACHMENT_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_EVIDENCE_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_ADOPTER_POLISH_HANDOFF_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_ADOPTER_POLISH_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "V064_MILESTONE_PROOF_OK:True");
        Assert.Contains(proofLines, line => line == "NAVIGATION_HISTORY_OK:True");
        Assert.Contains(proofLines, line => line == "SCOPE_BREADCRUMB_NAVIGATION_OK:True");
        Assert.Contains(proofLines, line => line == "FOCUS_RESTORE_OK:True");
        Assert.Contains(proofLines, line => line == "NAVIGATION_PRODUCTIVITY_PROOF_OK:True");
        Assert.Contains(proofLines, line => line == "NAVIGATION_PRODUCTIVITY_HANDOFF_OK:True");
        Assert.Contains(proofLines, line => line == "NAVIGATION_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "LAYOUT_PROVIDER_SEAM_OK:True");
        Assert.Contains(proofLines, line => line == "LAYOUT_PREVIEW_APPLY_CANCEL_OK:True");
        Assert.Contains(proofLines, line => line == "LAYOUT_UNDO_TRANSACTION_OK:True");
        Assert.Contains(proofLines, line => line == "READABILITY_FOCUS_SUBGRAPH_OK:True");
        Assert.Contains(proofLines, line => line == "READABILITY_ROUTE_CLEANUP_OK:True");
        Assert.Contains(proofLines, line => line == "READABILITY_ALIGNMENT_HELPERS_OK:True");
        Assert.Contains(proofLines, line => line == "PLUGIN_LOCAL_GALLERY_OK:True");
        Assert.Contains(proofLines, line => line == "PLUGIN_TRUST_EVIDENCE_PANEL_OK:True");
        Assert.Contains(proofLines, line => line == "PLUGIN_ALLOWLIST_ROUNDTRIP_OK:True");
        Assert.Contains(proofLines, line => line == "PLUGIN_SAMPLE_PACK_OK:True");
        Assert.Contains(proofLines, line => line == "PLUGIN_SAMPLE_NODE_DEFINITIONS_OK:True");
        Assert.Contains(proofLines, line => line == "PLUGIN_SAMPLE_PARAMETER_METADATA_OK:True");
        Assert.Contains(proofLines, line => line == "NODE_DEFINITION_BUILDER_OK:True");
        Assert.Contains(proofLines, line => line == "PORT_DEFINITION_BUILDER_OK:True");
        Assert.Contains(proofLines, line => line == "PARAMETER_DEFINITION_BUILDER_OK:True");
        Assert.Contains(proofLines, line => line == "CONNECTION_RULE_BUILDER_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_BUILDER_THIN_WRAPPER_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_STENCIL_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_EXPORT_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_BASELINE_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_FOCUS_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_AUTOMATION_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line == "HOSTED_ACCESSIBILITY_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_PERFORMANCE_MODE_OK:True");
        Assert.Contains(proofLines, line => line == "BALANCED_MODE_DEFAULT_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_LOD_POLICY_OK:True");
        Assert.Contains(proofLines, line => line == "PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_LAYOUT_PRESETS_OK:True");
        Assert.Contains(proofLines, line => line == "WORKBENCH_LAYOUT_RESET_OK:True");
        Assert.Contains(proofLines, line => line == "PANEL_STATE_PERSISTENCE_OK:True");
        Assert.Contains(proofLines, line => line == "WIDENED_SURFACE_PERFORMANCE_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_SCENARIO_GRAPH_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_VALIDATION_FEEDBACK_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_FEEDBACK_FOCUS_TARGET_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_READINESS_STATUS_OK:True");
        Assert.Contains(proofLines, line => line == "FIVE_MINUTE_ONBOARDING_OK:True");
        Assert.Contains(proofLines, line => line == "ONBOARDING_CONFIGURATION_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_FLOW_PROOF_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_FLOW_HANDOFF_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_PARAMETER_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_WINDOW_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_TRUST_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:startup_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:inspector_projection_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:plugin_scan_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:command_latency_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:stencil_search_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:command_surface_refresh_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:node_tool_projection_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:edge_tool_projection_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:command_palette_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_OK:True");
        Assert.Equal(
            ["frameworkDescription", "osArchitecture", "osDescription", "processArchitecture"],
            environment.EnumerateObject().Select(static property => property.Name).OrderBy(static name => name).ToArray());
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("frameworkDescription").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("osDescription").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("osArchitecture").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("processArchitecture").GetString()));
        Assert.Equal("repro-note", reproduction.GetProperty("note").GetString());
        Assert.Contains("--support-bundle", reproduction.GetProperty("command").GetString(), StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(reproduction.GetProperty("workingDirectory").GetString()));

        var graphSummary = root.GetProperty("graphSummary");
        Assert.True(graphSummary.GetProperty("nodeCount").GetInt32() >= 0);
        Assert.True(graphSummary.GetProperty("connectionCount").GetInt32() >= 0);

        var featureDescriptors = root.GetProperty("featureDescriptors").EnumerateArray().ToArray();
        Assert.True(featureDescriptors.Length > 0);

        var recentsFavorites = root.GetProperty("recentsFavorites").EnumerateArray().ToArray();
        Assert.Equal(4, recentsFavorites.Length);
        Assert.Contains(recentsFavorites, entry =>
            entry.GetProperty("surface").GetString() == "node"
            && entry.GetProperty("recentIds").EnumerateArray().Any(id => id.GetString() == ConsumerSampleHost.ReviewDefinitionId.ToString())
            && entry.GetProperty("favoriteIds").EnumerateArray().Any(id => id.GetString() == ConsumerSampleHost.QueueDefinitionId.ToString())
            && entry.GetProperty("isHostOwned").GetBoolean());
        Assert.Contains(recentsFavorites, entry =>
            entry.GetProperty("surface").GetString() == "fragment"
            && entry.GetProperty("recentIds").EnumerateArray().Any(id => id.GetString() == ConsumerSampleHost.QueueLaneSnippetId)
            && entry.GetProperty("favoriteIds").EnumerateArray().Any(id => id.GetString() == ConsumerSampleHost.QueueLaneSnippetId));
        Assert.Contains(recentsFavorites, entry =>
            entry.GetProperty("surface").GetString() == "command"
            && entry.GetProperty("recentIds").EnumerateArray().Any(id => id.GetString() == "viewport.fit")
            && entry.GetProperty("favoriteIds").EnumerateArray().Any(id => id.GetString() == "workspace.save"));
        Assert.Contains(recentsFavorites, entry =>
            entry.GetProperty("surface").GetString() == "plugin"
            && entry.GetProperty("recentIds").EnumerateArray().Any(id => id.GetString() == "consumer.sample.audit-plugin")
            && entry.GetProperty("favoriteIds").EnumerateArray().Any(id => id.GetString() == "consumer.sample.audit-plugin"));

        var workbenchFrictionEvidence = root.GetProperty("workbenchFrictionEvidence").EnumerateArray().ToArray();
        Assert.True(workbenchFrictionEvidence.Length >= 4);
        Assert.Contains(workbenchFrictionEvidence, entry =>
            entry.GetProperty("category").GetString() == "layout-resume"
            && entry.GetProperty("priorityRank").GetInt32() == 1
            && entry.GetProperty("isSynthetic").GetBoolean());
        Assert.Contains(workbenchFrictionEvidence, entry =>
            entry.GetProperty("category").GetString() == "support-triage"
            && entry.GetProperty("scopeBoundary").GetString()!.Contains("not an external adopter report", StringComparison.OrdinalIgnoreCase));

        var workbenchAffordancePolish = root.GetProperty("workbenchAffordancePolish");
        Assert.Equal("layout-resume", workbenchAffordancePolish.GetProperty("frictionCategory").GetString());
        Assert.Contains("ResetLayout", workbenchAffordancePolish.GetProperty("route").GetString(), StringComparison.Ordinal);
        Assert.True(workbenchAffordancePolish.GetProperty("usesExistingHostedRoute").GetBoolean());

        var recentDiagnostics = root.GetProperty("recentDiagnostics").EnumerateArray().ToArray();
        Assert.True(recentDiagnostics.Length >= 0);
        var runtimeNodeOverlays = root.GetProperty("runtimeNodeOverlays").EnumerateArray().ToArray();
        var runtimeConnectionOverlays = root.GetProperty("runtimeConnectionOverlays").EnumerateArray().ToArray();
        var runtimeLogs = root.GetProperty("runtimeLogs").EnumerateArray().ToArray();
        Assert.True(runtimeNodeOverlays.Length > 0);
        Assert.True(runtimeConnectionOverlays.Length > 0);
        Assert.True(runtimeLogs.Length > 0);
        Assert.Contains(runtimeNodeOverlays, overlay =>
            overlay.GetProperty("status").GetInt32() == (int)GraphEditorRuntimeOverlayStatus.Error
            && overlay.GetProperty("errorCount").GetInt32() > 0
            && !string.IsNullOrWhiteSpace(overlay.GetProperty("errorMessage").GetString()));
        Assert.Contains(runtimeConnectionOverlays, overlay =>
            overlay.GetProperty("isStale").GetBoolean()
            && overlay.GetProperty("status").GetInt32() == (int)GraphEditorRuntimeOverlayStatus.Error);
        Assert.Contains(runtimeLogs, log => log.GetProperty("nodeId").GetString() == "consumer-sample-queue-001");

        var statusSnapshot = parameterSnapshots.First(snapshot =>
            snapshot.GetProperty("key").GetString() == "status"
            && snapshot.TryGetProperty("currentValue", out var currentValue)
            && currentValue.GetString() == "approved");
        Assert.Equal("enum", statusSnapshot.GetProperty("valueType").GetString());
        Assert.Equal("Enum", statusSnapshot.GetProperty("editorKind").GetString());
        Assert.Equal("approved", statusSnapshot.GetProperty("currentValue").GetString());
        Assert.Equal("draft", statusSnapshot.GetProperty("defaultValue").GetString());
        Assert.True(statusSnapshot.GetProperty("canEdit").GetBoolean());
        Assert.True(statusSnapshot.GetProperty("isValid").GetBoolean());
        Assert.True(statusSnapshot.GetProperty("supportsEnumSearch").GetBoolean());
        Assert.Equal("Draft", statusSnapshot.GetProperty("allowedOptions").EnumerateArray().First().GetProperty("label").GetString());
        var allowedOptions = statusSnapshot.GetProperty("allowedOptions").EnumerateArray().ToArray();
        Assert.Equal(
            ["draft", "review", "approved"],
            allowedOptions.Select(option => option.GetProperty("value").GetString()!).ToArray());
        Assert.Equal(
            ["Draft", "In Review", "Approved"],
            allowedOptions.Select(option => option.GetProperty("label").GetString()!).ToArray());

        var ownerSnapshot = parameterSnapshots.First(snapshot =>
            snapshot.GetProperty("key").GetString() == "owner"
            && snapshot.TryGetProperty("currentValue", out var currentValue)
            && currentValue.GetString() == "release-owner");
        Assert.Equal("string", ownerSnapshot.GetProperty("valueType").GetString());
        Assert.Equal("Text", ownerSnapshot.GetProperty("editorKind").GetString());
        Assert.Equal("release-owner", ownerSnapshot.GetProperty("currentValue").GetString());
        Assert.Equal("design-review", ownerSnapshot.GetProperty("defaultValue").GetString());
        Assert.True(ownerSnapshot.GetProperty("canEdit").GetBoolean());
        Assert.True(ownerSnapshot.GetProperty("isValid").GetBoolean());
        Assert.Empty(ownerSnapshot.GetProperty("allowedOptions").EnumerateArray());

        var prioritySnapshot = parameterSnapshots.First(snapshot =>
            snapshot.GetProperty("key").GetString() == "priority"
            && snapshot.TryGetProperty("currentValue", out var currentValue)
            && currentValue.ValueKind == JsonValueKind.Number);
        Assert.Equal("int", prioritySnapshot.GetProperty("valueType").GetString());
        Assert.Equal("Number", prioritySnapshot.GetProperty("editorKind").GetString());
        Assert.Equal(2, prioritySnapshot.GetProperty("currentValue").GetInt32());
        Assert.Equal(2, prioritySnapshot.GetProperty("defaultValue").GetInt32());
        Assert.Equal(1, prioritySnapshot.GetProperty("minimum").GetDouble());
        Assert.Equal(5, prioritySnapshot.GetProperty("maximum").GetDouble());
        Assert.Equal("Slider range: 1 - 5", prioritySnapshot.GetProperty("numberSliderHint").GetString());
        Assert.Empty(prioritySnapshot.GetProperty("allowedOptions").EnumerateArray());

        var scriptSnapshot = parameterSnapshots.Single(snapshot => snapshot.GetProperty("key").GetString() == "review-script");
        Assert.True(scriptSnapshot.GetProperty("usesMultilineTextInput").GetBoolean());
        Assert.True(scriptSnapshot.GetProperty("isCodeLikeText").GetBoolean());
        Assert.Contains("review automation", scriptSnapshot.GetProperty("helpText").GetString(), StringComparison.Ordinal);

        var mixedStatusSnapshot = parameterSnapshots.First(snapshot =>
            snapshot.GetProperty("key").GetString() == "status"
            && snapshot.GetProperty("hasMixedValues").GetBoolean());
        Assert.Equal("Mixed", mixedStatusSnapshot.GetProperty("valueState").GetString());

        var validationFixSnapshot = parameterSnapshots.Single(snapshot => snapshot.GetProperty("key").GetString() == "slug");
        Assert.False(validationFixSnapshot.GetProperty("isValid").GetBoolean());
        Assert.True(validationFixSnapshot.GetProperty("canApplyValidationFix").GetBoolean());
        Assert.Equal("Restore default", validationFixSnapshot.GetProperty("validationFixActionLabel").GetString());
    }

    [AvaloniaFact]
    public void ConsumerSampleSupportBundle_WritesValidationFeedbackFocusTargets()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            HostedAccessibilityBaselineOk: true,
            HostedAccessibilityFocusOk: true,
            HostedAccessibilityCommandSurfaceOk: true,
            HostedAccessibilityAuthoringSurfaceOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1,
            CommandPaletteMs: 1,
            ReadinessStatus: "Blocked",
            ValidationSummary: new ConsumerSampleProofValidationSummary(
                TotalIssueCount: 2,
                ErrorCount: 2,
                WarningCount: 0,
                InvalidConnectionCount: 1,
                InvalidParameterCount: 1),
            ValidationFeedback:
            [
                new ConsumerSampleProofValidationFeedback(
                    "connection.target-input-missing",
                    "Error",
                    "Connection target input is missing.",
                    new ConsumerSampleProofFocusTarget(
                        "ConnectionEndpoint",
                        NodeId: "target-001",
                        ConnectionId: "connection-001",
                        EndpointId: "missing-input")),
                new ConsumerSampleProofValidationFeedback(
                    "node.parameter-invalid",
                    "Error",
                    "Prompt is required.",
                    new ConsumerSampleProofFocusTarget(
                        "Parameter",
                        NodeId: "node-001",
                        EndpointId: "prompt",
                        ParameterKey: "prompt")),
            ],
            NodeCount: 2,
            ConnectionCount: 1,
            FeatureDescriptorIds: ["query.validation-snapshot"],
            SupportBundlePayloadOk: true);
        var tempRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var bundlePath = Path.Combine(tempRoot, "validation-support-bundle.json");

        ConsumerSampleSupportBundle.WriteProofBundle(
            bundlePath,
            result,
            "consumer-sample --proof --support-bundle validation-support-bundle.json");

        using var document = JsonDocument.Parse(File.ReadAllText(bundlePath));
        var root = document.RootElement;
        var validationSummary = root.GetProperty("validationSummary");
        var validationFeedback = root.GetProperty("validationFeedback").EnumerateArray().ToArray();
        var connectionTarget = validationFeedback.Single(row => row.GetProperty("code").GetString() == "connection.target-input-missing").GetProperty("focusTarget");
        var parameterTarget = validationFeedback.Single(row => row.GetProperty("code").GetString() == "node.parameter-invalid").GetProperty("focusTarget");
        var proofLines = root.GetProperty("proofLines").EnumerateArray().Select(static item => item.GetString()).ToArray();

        Assert.Equal("Blocked", root.GetProperty("readinessStatus").GetString());
        Assert.Equal(2, validationSummary.GetProperty("totalIssueCount").GetInt32());
        Assert.Equal(2, validationSummary.GetProperty("errorCount").GetInt32());
        Assert.Equal(1, validationSummary.GetProperty("invalidConnectionCount").GetInt32());
        Assert.Equal(1, validationSummary.GetProperty("invalidParameterCount").GetInt32());
        Assert.Equal("ConnectionEndpoint", connectionTarget.GetProperty("kind").GetString());
        Assert.Equal("target-001", connectionTarget.GetProperty("nodeId").GetString());
        Assert.Equal("connection-001", connectionTarget.GetProperty("connectionId").GetString());
        Assert.Equal("missing-input", connectionTarget.GetProperty("endpointId").GetString());
        Assert.Equal("Parameter", parameterTarget.GetProperty("kind").GetString());
        Assert.Equal("node-001", parameterTarget.GetProperty("nodeId").GetString());
        Assert.Equal("prompt", parameterTarget.GetProperty("endpointId").GetString());
        Assert.Equal("prompt", parameterTarget.GetProperty("parameterKey").GetString());
        Assert.Contains(proofLines, line => line == "GRAPH_VALIDATION_FEEDBACK_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_FEEDBACK_FOCUS_TARGET_OK:True");
        Assert.Contains(proofLines, line => line == "GRAPH_READINESS_STATUS_OK:True");
    }

    [AvaloniaFact]
    public void Program_SupportBundleOption_EmitsBundleMarkersAndFile()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var bundlePath = Path.Combine(tempRoot, "program-support-bundle.json");
        var originalWriter = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            Program.Main(["--support-bundle", bundlePath, "--support-note", "cli-note"]);
        }
        finally
        {
            Console.SetOut(originalWriter);
        }

        var lines = output.ToString();
        Assert.Contains("SUPPORT_BUNDLE_OK:True", lines, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_SCHEMA_OK:True", lines, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:True", lines, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:", lines, StringComparison.Ordinal);
        Assert.True(File.Exists(bundlePath));
    }

    [AvaloniaFact]
    public void Program_SupportBundleOption_EmitsFalsePersistenceMarkerWhenWriteFails()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var originalWriter = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            Assert.ThrowsAny<Exception>(() => Program.Main(["--support-bundle", tempRoot, "--support-note", "cli-note"]));
        }
        finally
        {
            Console.SetOut(originalWriter);
        }

        var lines = output.ToString();
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:False", lines, StringComparison.Ordinal);
        Assert.DoesNotContain(lines.Split(Environment.NewLine), line => line == "SUPPORT_BUNDLE_OK:True");
    }

    [AvaloniaFact]
    public void Program_SupportBundleOption_RequiresExplicitPath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Program.Main(["--support-bundle", "--support-note", "cli-note"]));

        Assert.Contains("--support-bundle requires a file path.", exception.Message, StringComparison.Ordinal);
    }

    [AvaloniaFact]
    public void ConsumerSampleHost_PersistsExportsAndImportsPluginAllowlistDecisions()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        using var host = ConsumerSampleHost.Create(storageRoot);

        Assert.Contains(host.PluginCandidateEntries, entry => entry.PluginId == "consumer.sample.audit-plugin" && entry.IsAllowed);

        var exportPath = Path.Combine(storageRoot, "consumer-allowlist-export.json");
        Assert.True(host.ExportPluginAllowlist(exportPath));
        Assert.True(File.Exists(exportPath));

        Assert.True(host.BlockPluginCandidate("consumer.sample.audit-plugin"));
        Assert.Contains(host.PluginCandidateEntries, entry => entry.PluginId == "consumer.sample.audit-plugin" && entry.IsBlocked);

        Assert.True(host.ImportPluginAllowlist(exportPath));
        Assert.Contains(host.PluginCandidateEntries, entry => entry.PluginId == "consumer.sample.audit-plugin" && entry.IsAllowed);
        Assert.Contains(host.PluginAllowlistLines, line => line.Contains("Allowlist", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_RendersHostedEditorAndIntegrationPanels()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();

            Assert.NotNull(FindNamed<Menu>(window, "PART_MainMenu"));
            Assert.NotNull(FindNamed<Button>(window, "PART_AddReviewNodeButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_AddPluginNodeButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_ApproveSelectionButton"));
            Assert.NotNull(FindNamed<GraphEditorView>(window, "PART_EditorView"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_ParameterItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_PluginCandidateItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_LocalPluginGalleryItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_PluginSnapshotItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_AllowlistItems"));
            Assert.NotNull(FindNamed<TextBlock>(window, "PART_RuntimeSummaryText"));
            Assert.NotNull(FindNamed<ComboBox>(window, "PART_RuntimeLogFilter"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_RuntimeLogItems"));
            Assert.NotNull(FindNamed<Button>(window, "PART_ExportRuntimeLogsButton"));
            Assert.NotNull(FindNamed<TextBox>(window, "PART_GraphSearchBox"));
            Assert.NotNull(FindNamed<ComboBox>(window, "PART_GraphSearchScope"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_GraphSearchItems"));
            Assert.NotNull(FindNamed<TextBlock>(window, "PART_NavigationHistorySummaryText"));
            Assert.NotNull(FindNamed<Button>(window, "PART_NavigationBackButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_NavigationForwardButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_FocusCurrentScopeButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_RestoreViewportButton"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_ScopeBreadcrumbItems"));
            Assert.NotNull(FindNamed<TextBlock>(window, "PART_TrustBoundaryText"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_CommandRailUsesSharedSessionActionPath()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();

            var initialNodeCount = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
            var addReview = Assert.IsType<Button>(FindNamed<Button>(window, "PART_AddReviewNodeButton"));

            addReview.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(initialNodeCount + 1, host.Session.Queries.CreateDocumentSnapshot().Nodes.Count);

            var undo = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_history.undo"));
            undo.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(initialNodeCount, host.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_HostRailProjectsSharedAuthoringToolActions()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();

            var createGroup = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_selection-create-group"));
            var toggleExpansion = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_node-toggle-surface-expansion"));
            var disconnect = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_connection-disconnect"));
            var reviewNodeId = host.GetFirstReviewNodeId();
            var initialExpansionState = host.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == reviewNodeId)
                .ExpansionState;

            Assert.True(createGroup.IsEnabled);
            Assert.True(toggleExpansion.IsEnabled);
            Assert.True(disconnect.IsEnabled);

            toggleExpansion.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            disconnect.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            var updatedExpansionState = host.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == reviewNodeId)
                .ExpansionState;

            Assert.NotEqual(initialExpansionState, updatedExpansionState);
            Assert.Empty(host.Session.Queries.CreateDocumentSnapshot().Connections);
        }
        finally
        {
            window.Close();
        }
    }

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));
}
