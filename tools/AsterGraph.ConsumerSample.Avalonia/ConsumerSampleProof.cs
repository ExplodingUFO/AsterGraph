using System.Diagnostics;
using System.Globalization;
using System.Reflection;
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
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
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
    string ReadinessStatus,
    ConsumerSampleProofValidationSummary ValidationSummary,
    IReadOnlyList<ConsumerSampleProofValidationFeedback> ValidationFeedback,
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
    bool RuntimeOverlaySnapshotPolishOk = true,
    bool RuntimeOverlayScopeFilterOk = true,
    bool RuntimeLogPanelOk = true,
    bool RuntimeLogFilterOk = true,
    bool RuntimeDebugPanelInteractionOk = true,
    bool RuntimeLogLocateOk = true,
    bool RuntimeLogExportOk = true,
    bool RuntimeOverlaySupportBundleOk = true,
    bool PortHandleIdOk = true,
    bool PortGroupAuthoringOk = true,
    bool PortConnectionHintOk = true,
    bool PortAuthoringScopeBoundaryOk = true,
    bool ConnectionValidationReasonOk = true,
    bool ConnectionInvalidHoverFeedbackOk = true,
    bool ConnectionValidationSupportBundleOk = true,
    bool ConnectionValidationScopeBoundaryOk = true,
    bool GraphSearchLocateOk = true,
    bool GraphSearchScopeFilterOk = true,
    bool GraphSearchViewportFocusOk = true,
    bool StencilGroupingOk = true,
    bool StencilSearchOk = true,
    bool StencilRecentsFavoritesOk = true,
    bool StencilSourceFilterOk = true,
    bool StencilStatePersistenceOk = true,
    bool ExportPanelOk = true,
    bool ExportPanelScopeOk = true,
    bool ExportPanelProgressCancelOk = true,
    bool ExportDocsAlignmentOk = true,
    bool CommandPaletteGroupingOk = true,
    bool CommandPaletteDisabledReasonOk = true,
    bool CommandPaletteRecentActionsOk = true,
    bool CommandProjectionUnifiedOk = true,
    bool CommandPaletteOk = true,
    bool ToolbarDescriptorOk = true,
    bool ContextMenuDescriptorOk = true,
    bool CommandDisabledReasonOk = true,
    bool NodeToolbarContributionOk = true,
    bool EdgeToolbarContributionOk = true,
    bool ToolbarContributionDescriptorOk = true,
    bool ToolbarContributionScopeBoundaryOk = true,
    bool NavigationHistoryOk = true,
    bool ScopeBreadcrumbNavigationOk = true,
    bool FocusRestoreOk = true,
    bool LayoutProviderSeamOk = true,
    bool LayoutPreviewApplyCancelOk = true,
    bool LayoutUndoTransactionOk = true,
    bool ReadabilityFocusSubgraphOk = true,
    bool ReadabilityRouteCleanupOk = true,
    bool ReadabilityAlignmentHelpersOk = true,
    bool PluginLocalGalleryOk = true,
    bool PluginTrustEvidencePanelOk = true,
    bool PluginAllowlistRoundtripOk = true,
    bool PluginSamplePackOk = true,
    bool PluginSampleNodeDefinitionsOk = true,
    bool PluginSampleParameterMetadataOk = true,
    bool NodeDefinitionBuilderOk = true,
    bool PortDefinitionBuilderOk = true,
    bool ParameterDefinitionBuilderOk = true,
    bool ConnectionRuleBuilderOk = true,
    bool AuthoringBuilderThinWrapperOk = true,
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
    bool FiveMinuteOnboardingOk = true,
    bool GraphSnippetCatalogOk = true,
    bool GraphSnippetInsertOk = true,
    bool FragmentLibrarySearchOk = true,
    bool FragmentLibraryPreviewOk = true,
    bool FragmentLibraryRecentsFavoritesOk = true,
    bool FragmentLibraryScopeBoundaryOk = true,
    bool WorkbenchDefaultsOk = true,
    bool WorkbenchHostBuilderHandoffOk = true,
    bool WorkbenchPerformanceModeOk = true,
    bool BalancedModeDefaultOk = true,
    bool WorkbenchLodPolicyOk = true,
    bool PerformanceModeScopeBoundaryOk = true,
    bool WorkbenchLayoutPresetsOk = true,
    bool WorkbenchLayoutResetOk = true,
    bool PanelStatePersistenceOk = true,
    bool UnifiedDiscoverySurfaceOk = true,
    bool DiscoverySourceLabelsOk = true,
    bool DiscoveryCommandRouteOk = true,
    bool WorkbenchRecentsOk = true,
    bool WorkbenchFavoritesOk = true,
    bool RecentsFavoritesSupportBundleOk = true,
    IReadOnlyList<ConsumerSampleRecentsFavoritesEvidence>? RecentsFavoritesEvidence = null,
    bool WorkbenchFrictionEvidenceOk = true,
    bool WorkbenchFrictionPrioritizationOk = true,
    bool WorkbenchFrictionScopeBoundaryOk = true,
    IReadOnlyList<ConsumerSampleWorkbenchFrictionEvidence>? WorkbenchFrictionEvidence = null,
    bool WorkbenchAffordancePolishOk = true,
    bool WorkbenchAffordanceRouteOk = true,
    bool WorkbenchAffordanceScopeBoundaryOk = true,
    ConsumerSampleWorkbenchAffordancePolish? WorkbenchAffordancePolish = null,
    bool MiniMapLightweightProjectionEvidenceOk = true,
    IReadOnlyList<ConsumerSampleRepairEvidence>? RepairEvidence = null,
    int SelectedParameterProjectionCount = 0,
    int TotalParameterProjectionCount = 0,
    bool GraphErrorHelpTargetOk = true,
    bool GraphProblemInspectorHelpTargetOk = true,
    bool RepairHelpReviewLoopOk = true)
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
        && PortHandleIdOk
        && PortGroupAuthoringOk
        && PortConnectionHintOk
        && PortAuthoringScopeBoundaryOk
        && ConnectionValidationReasonOk
        && ConnectionInvalidHoverFeedbackOk
        && ConnectionValidationSupportBundleOk
        && ConnectionValidationScopeBoundaryOk
        && NodeToolbarContributionOk
        && EdgeToolbarContributionOk
        && ToolbarContributionDescriptorOk
        && ToolbarContributionScopeBoundaryOk
        && NodeSideAuthoringOk
        && CommandSurfaceOk;

    public bool CapabilityBreadthOk
        => StencilSurfaceOk
        && StencilGroupingOk
        && StencilSearchOk
        && StencilRecentsFavoritesOk
        && StencilSourceFilterOk
        && StencilStatePersistenceOk
        && ExportBreadthOk
        && ExportPanelOk
        && ExportPanelScopeOk
        && ExportPanelProgressCancelOk
        && ExportDocsAlignmentOk
        && NodeQuickToolsOk
        && EdgeQuickToolsOk;

    public bool WidenedSurfacePerformanceOk
        => CommandSurfaceOk
        && CapabilityBreadthOk
        && WorkbenchPerformanceModeOk
        && BalancedModeDefaultOk
        && WorkbenchLodPolicyOk
        && PerformanceModeScopeBoundaryOk
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
        && RuntimeOverlaySnapshotPolishOk
        && RuntimeOverlayScopeFilterOk
        && RuntimeLogPanelOk
        && RuntimeLogFilterOk
        && RuntimeDebugPanelInteractionOk
        && RuntimeLogLocateOk
        && RuntimeLogExportOk
        && ExportPanelOk
        && ExportPanelScopeOk
        && ExportPanelProgressCancelOk
        && ExportDocsAlignmentOk
        && RuntimeOverlaySupportBundleOk
        && PortHandleIdOk
        && PortGroupAuthoringOk
        && PortConnectionHintOk
        && PortAuthoringScopeBoundaryOk
        && ConnectionValidationReasonOk
        && ConnectionInvalidHoverFeedbackOk
        && ConnectionValidationSupportBundleOk
        && ConnectionValidationScopeBoundaryOk
        && GraphSearchLocateOk
        && GraphSearchScopeFilterOk
        && GraphSearchViewportFocusOk
        && UnifiedDiscoverySurfaceOk
        && DiscoverySourceLabelsOk
        && DiscoveryCommandRouteOk
        && WorkbenchRecentsOk
        && WorkbenchFavoritesOk
        && RecentsFavoritesSupportBundleOk
        && WorkbenchFrictionEvidenceOk
        && WorkbenchFrictionPrioritizationOk
        && WorkbenchFrictionScopeBoundaryOk
        && WorkbenchAffordancePolishOk
        && WorkbenchAffordanceRouteOk
        && WorkbenchAffordanceScopeBoundaryOk
        && CommandPaletteGroupingOk
        && CommandPaletteDisabledReasonOk
        && CommandPaletteRecentActionsOk
        && CommandProjectionUnifiedOk
        && CommandPaletteOk
        && ToolbarDescriptorOk
        && ContextMenuDescriptorOk
        && CommandDisabledReasonOk
        && NodeToolbarContributionOk
        && EdgeToolbarContributionOk
        && ToolbarContributionDescriptorOk
        && ToolbarContributionScopeBoundaryOk
        && NavigationHistoryOk
        && ScopeBreadcrumbNavigationOk
        && FocusRestoreOk
        && GraphValidationFeedbackOk
        && GraphFeedbackFocusTargetOk
        && GraphReadinessStatusOk
        && LayoutProviderSeamOk
        && LayoutPreviewApplyCancelOk
        && LayoutUndoTransactionOk
        && ReadabilityFocusSubgraphOk
        && ReadabilityRouteCleanupOk
        && ReadabilityAlignmentHelpersOk
        && PluginLocalGalleryOk
        && PluginTrustEvidencePanelOk
        && PluginAllowlistRoundtripOk
        && PluginSamplePackOk
        && PluginSampleNodeDefinitionsOk
        && PluginSampleParameterMetadataOk
        && NodeDefinitionBuilderOk
        && PortDefinitionBuilderOk
        && ParameterDefinitionBuilderOk
        && ConnectionRuleBuilderOk
        && AuthoringBuilderThinWrapperOk
        && GraphSnippetCatalogOk
        && GraphSnippetInsertOk
        && FragmentLibrarySearchOk
        && FragmentLibraryPreviewOk
        && FragmentLibraryRecentsFavoritesOk
        && FragmentLibraryScopeBoundaryOk
        && WorkbenchDefaultsOk
        && WorkbenchHostBuilderHandoffOk
        && WorkbenchPerformanceModeOk
        && BalancedModeDefaultOk
        && WorkbenchLodPolicyOk
        && PerformanceModeScopeBoundaryOk
        && WorkbenchLayoutPresetsOk
        && WorkbenchLayoutResetOk
        && PanelStatePersistenceOk
        && WorkbenchScopeBoundaryOk
        && NodeCount > 0
        && (FeatureDescriptorIds?.Count > 0);

    public bool ExperiencePolishHandoffOk
        => AuthoringSurfaceOk
        && HostedAccessibilityOk
        && WidenedSurfacePerformanceOk
        && OnboardingConfigurationOk;

    public bool FeatureEnhancementProofOk
        => QuickAddConnectedNodeOk
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
        && RuntimeOverlaySnapshotOk
        && RuntimeOverlaySnapshotPolishOk
        && RuntimeOverlayScopeFilterOk
        && RuntimeLogPanelOk
        && RuntimeLogFilterOk
        && RuntimeDebugPanelInteractionOk
        && RuntimeLogLocateOk
        && RuntimeLogExportOk
        && ExportPanelOk
        && ExportPanelScopeOk
        && ExportPanelProgressCancelOk
        && ExportDocsAlignmentOk
        && RuntimeOverlaySupportBundleOk
        && PortHandleIdOk
        && PortGroupAuthoringOk
        && PortConnectionHintOk
        && PortAuthoringScopeBoundaryOk
        && ConnectionValidationReasonOk
        && ConnectionInvalidHoverFeedbackOk
        && ConnectionValidationSupportBundleOk
        && ConnectionValidationScopeBoundaryOk
        && GraphSearchLocateOk
        && GraphSearchScopeFilterOk
        && GraphSearchViewportFocusOk
        && UnifiedDiscoverySurfaceOk
        && DiscoverySourceLabelsOk
        && DiscoveryCommandRouteOk
        && WorkbenchRecentsOk
        && WorkbenchFavoritesOk
        && RecentsFavoritesSupportBundleOk
        && WorkbenchFrictionEvidenceOk
        && WorkbenchFrictionPrioritizationOk
        && WorkbenchAffordancePolishOk
        && WorkbenchAffordanceRouteOk
        && CommandPaletteGroupingOk
        && CommandPaletteDisabledReasonOk
        && CommandPaletteRecentActionsOk
        && NodeToolbarContributionOk
        && EdgeToolbarContributionOk
        && ToolbarContributionDescriptorOk
        && ToolbarContributionScopeBoundaryOk
        && NavigationHistoryOk
        && ScopeBreadcrumbNavigationOk
        && FocusRestoreOk
        && GraphValidationFeedbackOk
        && GraphFeedbackFocusTargetOk
        && GraphReadinessStatusOk
        && LayoutProviderSeamOk
        && LayoutPreviewApplyCancelOk
        && LayoutUndoTransactionOk
        && ReadabilityFocusSubgraphOk
        && ReadabilityRouteCleanupOk
        && ReadabilityAlignmentHelpersOk
        && PluginLocalGalleryOk
        && PluginTrustEvidencePanelOk
        && PluginAllowlistRoundtripOk
        && PluginSamplePackOk
        && PluginSampleNodeDefinitionsOk
        && PluginSampleParameterMetadataOk
        && NodeDefinitionBuilderOk
        && PortDefinitionBuilderOk
        && ParameterDefinitionBuilderOk
        && ConnectionRuleBuilderOk
        && AuthoringBuilderThinWrapperOk
        && GraphSnippetCatalogOk
        && GraphSnippetInsertOk
        && FragmentLibrarySearchOk
        && FragmentLibraryPreviewOk
        && FragmentLibraryRecentsFavoritesOk
        && FragmentLibraryScopeBoundaryOk
        && WorkbenchDefaultsOk
        && WorkbenchHostBuilderHandoffOk
        && WorkbenchLayoutPresetsOk
        && WorkbenchLayoutResetOk
        && PanelStatePersistenceOk
        && WorkbenchScopeBoundaryOk;

    public bool AuthoringFlowProofOk
        => QuickAddConnectedNodeOk
        && PortFilteredNodeSearchOk
        && DropNodeOnEdgeOk
        && EdgeSplitCompatibilityOk
        && EdgeSplitUndoOk
        && DeleteAndReconnectOk
        && DetachNodeOk
        && ReconnectConflictReportOk
        && EdgeMultiSelectOk
        && WireSliceOk
        && SelectedNodeEdgeHighlightOk;

    public bool AuthoringFlowHandoffOk
        => AuthoringFlowProofOk
        && AuthoringSurfaceOk
        && ExperiencePolishHandoffOk;

    public bool ExperienceScopeBoundaryOk
        => HostOwnedActionsOk
        && TrustTransparencyOk
        && SupportBundlePayloadOk
        && RuntimeOverlaySupportBundleOk
        && RuntimeOverlayScopeFilterOk
        && RuntimeDebugPanelInteractionOk
        && RuntimeLogLocateOk
        && RuntimeLogExportOk
        && GraphSearchLocateOk
        && GraphSearchScopeFilterOk
        && GraphSearchViewportFocusOk
        && UnifiedDiscoverySurfaceOk
        && DiscoverySourceLabelsOk
        && DiscoveryCommandRouteOk
        && WorkbenchRecentsOk
        && WorkbenchFavoritesOk
        && RecentsFavoritesSupportBundleOk
        && WorkbenchFrictionScopeBoundaryOk
        && WorkbenchAffordanceScopeBoundaryOk
        && CommandPaletteGroupingOk
        && CommandPaletteDisabledReasonOk
        && CommandPaletteRecentActionsOk
        && ToolbarContributionScopeBoundaryOk
        && NavigationHistoryOk
        && ScopeBreadcrumbNavigationOk
        && FocusRestoreOk
        && PluginTrustEvidencePanelOk
        && PluginAllowlistRoundtripOk
        && GraphSnippetCatalogOk
        && GraphSnippetInsertOk
        && WorkbenchLayoutPresetsOk
        && WorkbenchLayoutResetOk
        && PanelStatePersistenceOk
        && WorkbenchScopeBoundaryOk
        && FeatureDescriptorIds is { Count: > 0 }
        && FeatureDescriptorIds.All(IsBoundedFeatureDescriptorId);

    public bool AuthoringFlowScopeBoundaryOk
        => AuthoringFlowProofOk
        && ExperienceScopeBoundaryOk;

    public bool NavigationProductivityProofOk
        => GraphSearchLocateOk
        && GraphSearchScopeFilterOk
        && GraphSearchViewportFocusOk
        && CommandPaletteGroupingOk
        && CommandPaletteDisabledReasonOk
        && CommandPaletteRecentActionsOk
        && NavigationHistoryOk
        && ScopeBreadcrumbNavigationOk
        && FocusRestoreOk;

    public bool NavigationProductivityHandoffOk
        => NavigationProductivityProofOk
        && WindowCompositionOk
        && SupportBundlePayloadOk;

    public bool NavigationScopeBoundaryOk
        => NavigationProductivityProofOk
        && HostOwnedActionsOk
        && FeatureDescriptorIds is { Count: > 0 }
        && FeatureDescriptorIds.All(IsBoundedFeatureDescriptorId);

    public bool WorkbenchScopeBoundaryOk
        => WorkbenchHostBuilderHandoffOk
        && WorkbenchLayoutPresetsOk
        && WorkbenchLayoutResetOk
        && PanelStatePersistenceOk
        && FeatureDescriptorIds is { Count: > 0 }
        && FeatureDescriptorIds.All(IsBoundedFeatureDescriptorId);

    public bool AuthoringDepthHandoffOk
        => PortHandleIdOk
        && PortGroupAuthoringOk
        && PortConnectionHintOk
        && PortAuthoringScopeBoundaryOk
        && ConnectionValidationReasonOk
        && ConnectionInvalidHoverFeedbackOk
        && ConnectionValidationSupportBundleOk
        && ConnectionValidationScopeBoundaryOk
        && NodeToolbarContributionOk
        && EdgeToolbarContributionOk
        && ToolbarContributionDescriptorOk
        && ToolbarContributionScopeBoundaryOk
        && FragmentLibrarySearchOk
        && FragmentLibraryPreviewOk
        && FragmentLibraryRecentsFavoritesOk
        && FragmentLibraryScopeBoundaryOk;

    public bool AuthoringDepthScopeBoundaryOk
        => PortAuthoringScopeBoundaryOk
        && ConnectionValidationScopeBoundaryOk
        && ToolbarContributionScopeBoundaryOk
        && FragmentLibraryScopeBoundaryOk
        && AuthoringFlowScopeBoundaryOk
        && ExperienceScopeBoundaryOk
        && WorkbenchScopeBoundaryOk
        && NavigationScopeBoundaryOk;

    public bool V058MilestoneProofOk
        => AuthoringDepthHandoffOk
        && AuthoringDepthScopeBoundaryOk
        && AuthoringSurfaceOk
        && FeatureEnhancementProofOk;

    public bool LargeGraphUxPolicyOk
        => WorkbenchPerformanceModeOk
        && BalancedModeDefaultOk
        && WorkbenchLodPolicyOk
        && WidenedSurfacePerformanceOk;

    public bool LargeGraphUxScopeBoundaryOk
        => PerformanceModeScopeBoundaryOk
        && WorkbenchScopeBoundaryOk
        && FeatureDescriptorIds is { Count: > 0 }
        && FeatureDescriptorIds.All(IsBoundedFeatureDescriptorId);

    public bool LargeGraphUxProofBaselineOk
        => LargeGraphUxPolicyOk
        && LargeGraphUxScopeBoundaryOk
        && StartupMs >= 0
        && InspectorProjectionMs >= 0
        && CommandLatencyMs >= 0
        && StencilSearchMs >= 0
        && CommandSurfaceRefreshMs >= 0
        && NodeToolProjectionMs >= 0
        && EdgeToolProjectionMs >= 0;

    public bool ViewportLodPolicyOk
        => LargeGraphUxPolicyOk
        && WorkbenchLodPolicyOk
        && WorkbenchPerformanceModeOk;

    public bool SelectedHoveredAdornerScopeOk
        => ToolbarContributionScopeBoundaryOk
        && NodeToolbarContributionOk
        && EdgeToolbarContributionOk
        && PerformanceModeScopeBoundaryOk;

    public bool LargeGraphBalancedUxOk
        => LargeGraphUxProofBaselineOk
        && BalancedModeDefaultOk
        && WidenedSurfacePerformanceOk;

    public bool ViewportLodScopeBoundaryOk
        => LargeGraphUxScopeBoundaryOk
        && WorkbenchScopeBoundaryOk
        && PerformanceModeScopeBoundaryOk;

    public bool EdgeInteractionCacheOk
        => EdgeQuickToolsOk
        && EdgeToolProjectionMs >= 0
        && ConnectionCount > 0;

    public bool EdgeDragRouteSimplificationOk
        => ViewportLodPolicyOk
        && WorkbenchLodPolicyOk
        && EdgeQuickToolsOk;

    public bool SelectedEdgeFeedbackOk
        => EdgeToolbarContributionOk
        && ToolbarContributionDescriptorOk
        && EdgeMultiSelectOk
        && SelectedNodeEdgeHighlightOk;

    public bool EdgeRenderingScopeBoundaryOk
        => ViewportLodScopeBoundaryOk
        && ToolbarContributionScopeBoundaryOk
        && FeatureDescriptorIds is { Count: > 0 }
        && FeatureDescriptorIds.All(IsBoundedFeatureDescriptorId);

    public bool NodeDragEdgeRefreshOk
        => EdgeInteractionCacheOk
        && EdgeDragRouteSimplificationOk
        && ConnectionCount > 0;

    public bool EdgeRouteRefreshOk
        => EdgeQuickToolsOk
        && ReadabilityRouteCleanupOk
        && EdgeRenderingScopeBoundaryOk;

    public bool LiveCanvasRefreshAuditOk
        => NodeDragEdgeRefreshOk
        && EdgeRouteRefreshOk
        && SelectedEdgeFeedbackOk
        && RuntimeOverlaySupportBundleOk
        && ConnectionValidationSupportBundleOk
        && FeatureDescriptorIds is { Count: > 0 }
        && FeatureDescriptorIds.All(IsBoundedFeatureDescriptorId);

    public bool MinimapLightweightProjectionOk
        => MiniMapLightweightProjectionEvidenceOk
        && ViewportLodPolicyOk
        && WorkbenchLodPolicyOk
        && NodeCount > 0
        && ConnectionCount > 0
        && FeatureDescriptorIds is { Count: > 0 }
        && FeatureDescriptorIds.All(IsBoundedFeatureDescriptorId);

    public bool InspectorNarrowProjectionOk
        => ParameterProjectionOk
        && MetadataProjectionOk
        && InspectorProjectionMs >= 0
        && SelectedParameterProjectionCount > 0
        && TotalParameterProjectionCount > SelectedParameterProjectionCount;

    public bool LargeGraphPanelScopeOk
        => MinimapLightweightProjectionOk
        && InspectorNarrowProjectionOk
        && WorkbenchScopeBoundaryOk
        && ViewportLodScopeBoundaryOk;

    public bool ProjectionPerformanceEvidenceOk
        => LargeGraphUxProofBaselineOk
        && InspectorProjectionMs >= 0
        && StencilSearchMs >= 0
        && CommandSurfaceRefreshMs >= 0
        && NodeToolProjectionMs >= 0
        && EdgeToolProjectionMs >= 0;

    public bool LargeGraphUxHandoffOk
        => LargeGraphUxProofBaselineOk
        && ViewportLodPolicyOk
        && SelectedHoveredAdornerScopeOk
        && LargeGraphBalancedUxOk
        && EdgeInteractionCacheOk
        && EdgeDragRouteSimplificationOk
        && SelectedEdgeFeedbackOk
        && MinimapLightweightProjectionOk
        && InspectorNarrowProjectionOk
        && LargeGraphPanelScopeOk
        && ProjectionPerformanceEvidenceOk;

    public bool V059MilestoneProofOk
        => LargeGraphUxHandoffOk
        && LargeGraphUxScopeBoundaryOk
        && ViewportLodScopeBoundaryOk
        && EdgeRenderingScopeBoundaryOk
        && LargeGraphPanelScopeOk;

    public bool InteractionFeedbackOk
        => SelectedHoveredAdornerScopeOk
        && SelectedEdgeFeedbackOk
        && ConnectionInvalidHoverFeedbackOk
        && LiveCanvasRefreshAuditOk;

    public bool CanvasFocusRecoveryOk
        => HostedAccessibilityFocusOk
        && GraphSearchViewportFocusOk
        && FocusRestoreOk
        && NavigationScopeBoundaryOk;

    public bool ConsumerGestureProofOk
        => NodeDragEdgeRefreshOk
        && EdgeRouteRefreshOk
        && LiveCanvasRefreshAuditOk
        && EdgeMultiSelectOk
        && WireSliceOk
        && SelectedNodeEdgeHighlightOk
        && SelectedEdgeFeedbackOk
        && InteractionFeedbackOk
        && CommandDisabledReasonOk
        && CanvasFocusRecoveryOk;

    public bool InteractionSupportBundleOk
        => SupportBundlePayloadOk
        && RuntimeOverlaySupportBundleOk
        && ConnectionValidationSupportBundleOk
        && ConsumerGestureProofOk;

    public bool InteractionReliabilityHandoffOk
        => NodeDragEdgeRefreshOk
        && EdgeRouteRefreshOk
        && LiveCanvasRefreshAuditOk
        && InteractionFeedbackOk
        && CanvasFocusRecoveryOk
        && ConsumerGestureProofOk
        && InteractionSupportBundleOk;

    public bool InteractionScopeBoundaryOk
        => ExperienceScopeBoundaryOk
        && NavigationScopeBoundaryOk
        && WorkbenchScopeBoundaryOk
        && EdgeRenderingScopeBoundaryOk
        && LargeGraphUxScopeBoundaryOk;

    public bool V062MilestoneProofOk
        => InteractionReliabilityHandoffOk
        && InteractionScopeBoundaryOk;

    public bool WorkbenchDiscoverabilityHandoffOk
        => WorkbenchLayoutPresetsOk
        && WorkbenchLayoutResetOk
        && PanelStatePersistenceOk
        && UnifiedDiscoverySurfaceOk
        && DiscoverySourceLabelsOk
        && DiscoveryCommandRouteOk
        && WorkbenchRecentsOk
        && WorkbenchFavoritesOk
        && RecentsFavoritesSupportBundleOk;

    public bool WorkbenchDiscoverabilityScopeBoundaryOk
        => WorkbenchScopeBoundaryOk
        && PerformanceModeScopeBoundaryOk
        && FragmentLibraryScopeBoundaryOk
        && DiscoveryCommandRouteOk
        && RecentsFavoritesSupportBundleOk;

    public bool WorkbenchFrictionSupportBundleOk
        => WorkbenchFrictionEvidence is { Count: >= 4 }
        && WorkbenchAffordancePolish is not null
        && WorkbenchFrictionEvidence.All(entry => entry.IsSynthetic)
        && WorkbenchFrictionEvidence.Any(entry =>
            string.Equals(entry.Category, WorkbenchAffordancePolish.FrictionCategory, StringComparison.Ordinal));

    public bool WorkbenchAdopterEvidenceAttachmentOk
        => WorkbenchFrictionSupportBundleOk
        && SupportBundlePayloadOk
        && WorkbenchFrictionEvidence is { Count: > 0 }
        && WorkbenchFrictionEvidence.Any(entry =>
            string.Equals(entry.Category, "support-triage", StringComparison.Ordinal));

    public bool WorkbenchEvidenceScopeBoundaryOk
        => WorkbenchFrictionScopeBoundaryOk
        && WorkbenchAffordanceScopeBoundaryOk
        && WorkbenchFrictionEvidence is { Count: > 0 }
        && WorkbenchFrictionEvidence.All(entry =>
            entry.ScopeBoundary.Contains("local synthetic evidence only", StringComparison.OrdinalIgnoreCase));

    public bool WorkbenchAdopterPolishHandoffOk
        => WorkbenchFrictionEvidenceOk
        && WorkbenchFrictionPrioritizationOk
        && WorkbenchAffordancePolishOk
        && WorkbenchAffordanceRouteOk
        && WorkbenchFrictionSupportBundleOk
        && WorkbenchAdopterEvidenceAttachmentOk;

    public bool WorkbenchAdopterPolishScopeBoundaryOk
        => WorkbenchFrictionScopeBoundaryOk
        && WorkbenchAffordanceScopeBoundaryOk
        && WorkbenchEvidenceScopeBoundaryOk;

    public bool V064MilestoneProofOk
        => WorkbenchAdopterPolishHandoffOk
        && WorkbenchAdopterPolishScopeBoundaryOk;

    public bool V063MilestoneProofOk
        => WorkbenchDiscoverabilityHandoffOk
        && WorkbenchDiscoverabilityScopeBoundaryOk;

    public bool IsOk
        => HostMenuActionOk
        && PluginContributionOk
        && TrustTransparencyOk
        && WindowCompositionOk
        && AuthoringSurfaceOk
        && CapabilityBreadthOk
        && HostedAccessibilityOk
        && WidenedSurfacePerformanceOk
        && OnboardingConfigurationOk
        && ExperiencePolishHandoffOk
        && FeatureEnhancementProofOk
        && AuthoringFlowProofOk
        && AuthoringFlowHandoffOk
        && AuthoringFlowScopeBoundaryOk
        && ExperienceScopeBoundaryOk
        && NavigationProductivityProofOk
        && NavigationProductivityHandoffOk
        && NavigationScopeBoundaryOk
        && AuthoringDepthHandoffOk
        && AuthoringDepthScopeBoundaryOk
        && V058MilestoneProofOk
        && LargeGraphUxPolicyOk
        && LargeGraphUxScopeBoundaryOk
        && LargeGraphUxProofBaselineOk
        && ViewportLodPolicyOk
        && SelectedHoveredAdornerScopeOk
        && LargeGraphBalancedUxOk
        && ViewportLodScopeBoundaryOk
        && EdgeInteractionCacheOk
        && EdgeDragRouteSimplificationOk
        && SelectedEdgeFeedbackOk
        && EdgeRenderingScopeBoundaryOk
        && NodeDragEdgeRefreshOk
        && EdgeRouteRefreshOk
        && LiveCanvasRefreshAuditOk
        && MinimapLightweightProjectionOk
        && InspectorNarrowProjectionOk
        && LargeGraphPanelScopeOk
        && ProjectionPerformanceEvidenceOk
        && LargeGraphUxHandoffOk
        && V059MilestoneProofOk
        && InteractionFeedbackOk
        && CommandDisabledReasonOk
        && CanvasFocusRecoveryOk
        && ConsumerGestureProofOk
        && InteractionSupportBundleOk
        && InteractionReliabilityHandoffOk
        && InteractionScopeBoundaryOk
        && V062MilestoneProofOk
        && WorkbenchDiscoverabilityHandoffOk
        && WorkbenchDiscoverabilityScopeBoundaryOk
        && V063MilestoneProofOk
        && WorkbenchFrictionEvidenceOk
        && WorkbenchFrictionPrioritizationOk
        && WorkbenchFrictionScopeBoundaryOk
        && WorkbenchAffordancePolishOk
        && WorkbenchAffordanceRouteOk
        && WorkbenchAffordanceScopeBoundaryOk
        && WorkbenchFrictionSupportBundleOk
        && WorkbenchAdopterEvidenceAttachmentOk
        && WorkbenchEvidenceScopeBoundaryOk
        && WorkbenchAdopterPolishHandoffOk
        && WorkbenchAdopterPolishScopeBoundaryOk
        && V064MilestoneProofOk
        && GraphErrorHelpTargetOk
        && GraphProblemInspectorHelpTargetOk
        && RepairHelpReviewLoopOk;

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
        $"RUNTIME_OVERLAY_SNAPSHOT_POLISH_OK:{RuntimeOverlaySnapshotPolishOk}",
        $"RUNTIME_OVERLAY_SCOPE_FILTER_OK:{RuntimeOverlayScopeFilterOk}",
        $"RUNTIME_LOG_PANEL_OK:{RuntimeLogPanelOk}",
        $"RUNTIME_LOG_FILTER_OK:{RuntimeLogFilterOk}",
        $"RUNTIME_DEBUG_PANEL_INTERACTION_OK:{RuntimeDebugPanelInteractionOk}",
        $"RUNTIME_LOG_LOCATE_OK:{RuntimeLogLocateOk}",
        $"RUNTIME_LOG_EXPORT_OK:{RuntimeLogExportOk}",
        $"RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:{RuntimeOverlaySupportBundleOk}",
        $"PORT_HANDLE_ID_OK:{PortHandleIdOk}",
        $"PORT_GROUP_AUTHORING_OK:{PortGroupAuthoringOk}",
        $"PORT_CONNECTION_HINT_OK:{PortConnectionHintOk}",
        $"PORT_AUTHORING_SCOPE_BOUNDARY_OK:{PortAuthoringScopeBoundaryOk}",
        $"CONNECTION_VALIDATION_REASON_OK:{ConnectionValidationReasonOk}",
        $"CONNECTION_INVALID_HOVER_FEEDBACK_OK:{ConnectionInvalidHoverFeedbackOk}",
        $"CONNECTION_VALIDATION_SUPPORT_BUNDLE_OK:{ConnectionValidationSupportBundleOk}",
        $"CONNECTION_VALIDATION_SCOPE_BOUNDARY_OK:{ConnectionValidationScopeBoundaryOk}",
        $"GRAPH_SEARCH_LOCATE_OK:{GraphSearchLocateOk}",
        $"GRAPH_SEARCH_SCOPE_FILTER_OK:{GraphSearchScopeFilterOk}",
        $"GRAPH_SEARCH_VIEWPORT_FOCUS_OK:{GraphSearchViewportFocusOk}",
        $"UNIFIED_DISCOVERY_SURFACE_OK:{UnifiedDiscoverySurfaceOk}",
        $"DISCOVERY_SOURCE_LABELS_OK:{DiscoverySourceLabelsOk}",
        $"DISCOVERY_COMMAND_ROUTE_OK:{DiscoveryCommandRouteOk}",
        $"WORKBENCH_RECENTS_OK:{WorkbenchRecentsOk}",
        $"WORKBENCH_FAVORITES_OK:{WorkbenchFavoritesOk}",
        $"RECENTS_FAVORITES_SUPPORT_BUNDLE_OK:{RecentsFavoritesSupportBundleOk}",
        $"WORKBENCH_FRICTION_EVIDENCE_OK:{WorkbenchFrictionEvidenceOk}",
        $"WORKBENCH_FRICTION_PRIORITIZATION_OK:{WorkbenchFrictionPrioritizationOk}",
        $"WORKBENCH_FRICTION_SCOPE_BOUNDARY_OK:{WorkbenchFrictionScopeBoundaryOk}",
        $"WORKBENCH_AFFORDANCE_POLISH_OK:{WorkbenchAffordancePolishOk}",
        $"WORKBENCH_AFFORDANCE_ROUTE_OK:{WorkbenchAffordanceRouteOk}",
        $"WORKBENCH_AFFORDANCE_SCOPE_BOUNDARY_OK:{WorkbenchAffordanceScopeBoundaryOk}",
        $"WORKBENCH_FRICTION_SUPPORT_BUNDLE_OK:{WorkbenchFrictionSupportBundleOk}",
        $"WORKBENCH_ADOPTER_EVIDENCE_ATTACHMENT_OK:{WorkbenchAdopterEvidenceAttachmentOk}",
        $"WORKBENCH_EVIDENCE_SCOPE_BOUNDARY_OK:{WorkbenchEvidenceScopeBoundaryOk}",
        $"WORKBENCH_ADOPTER_POLISH_HANDOFF_OK:{WorkbenchAdopterPolishHandoffOk}",
        $"WORKBENCH_ADOPTER_POLISH_SCOPE_BOUNDARY_OK:{WorkbenchAdopterPolishScopeBoundaryOk}",
        $"V064_MILESTONE_PROOF_OK:{V064MilestoneProofOk}",
        $"COMMAND_PALETTE_GROUPING_OK:{CommandPaletteGroupingOk}",
        $"COMMAND_PALETTE_DISABLED_REASON_OK:{CommandPaletteDisabledReasonOk}",
        $"COMMAND_PALETTE_RECENT_ACTIONS_OK:{CommandPaletteRecentActionsOk}",
        $"COMMAND_PROJECTION_UNIFIED_OK:{CommandProjectionUnifiedOk}",
        $"COMMAND_PALETTE_OK:{CommandPaletteOk}",
        $"TOOLBAR_DESCRIPTOR_OK:{ToolbarDescriptorOk}",
        $"CONTEXT_MENU_DESCRIPTOR_OK:{ContextMenuDescriptorOk}",
        $"COMMAND_DISABLED_REASON_OK:{CommandDisabledReasonOk}",
        $"INTERACTION_FEEDBACK_OK:{InteractionFeedbackOk}",
        $"CANVAS_FOCUS_RECOVERY_OK:{CanvasFocusRecoveryOk}",
        $"CONSUMER_GESTURE_PROOF_OK:{ConsumerGestureProofOk}",
        $"INTERACTION_SUPPORT_BUNDLE_OK:{InteractionSupportBundleOk}",
        $"INTERACTION_RELIABILITY_HANDOFF_OK:{InteractionReliabilityHandoffOk}",
        $"INTERACTION_SCOPE_BOUNDARY_OK:{InteractionScopeBoundaryOk}",
        $"V062_MILESTONE_PROOF_OK:{V062MilestoneProofOk}",
        $"WORKBENCH_DISCOVERABILITY_HANDOFF_OK:{WorkbenchDiscoverabilityHandoffOk}",
        $"WORKBENCH_DISCOVERABILITY_SCOPE_BOUNDARY_OK:{WorkbenchDiscoverabilityScopeBoundaryOk}",
        $"V063_MILESTONE_PROOF_OK:{V063MilestoneProofOk}",
        $"NODE_TOOLBAR_CONTRIBUTION_OK:{NodeToolbarContributionOk}",
        $"EDGE_TOOLBAR_CONTRIBUTION_OK:{EdgeToolbarContributionOk}",
        $"TOOLBAR_CONTRIBUTION_DESCRIPTOR_OK:{ToolbarContributionDescriptorOk}",
        $"TOOLBAR_CONTRIBUTION_SCOPE_BOUNDARY_OK:{ToolbarContributionScopeBoundaryOk}",
        $"NAVIGATION_HISTORY_OK:{NavigationHistoryOk}",
        $"SCOPE_BREADCRUMB_NAVIGATION_OK:{ScopeBreadcrumbNavigationOk}",
        $"FOCUS_RESTORE_OK:{FocusRestoreOk}",
        $"NAVIGATION_PRODUCTIVITY_PROOF_OK:{NavigationProductivityProofOk}",
        $"NAVIGATION_PRODUCTIVITY_HANDOFF_OK:{NavigationProductivityHandoffOk}",
        $"NAVIGATION_SCOPE_BOUNDARY_OK:{NavigationScopeBoundaryOk}",
        $"LAYOUT_PROVIDER_SEAM_OK:{LayoutProviderSeamOk}",
        $"LAYOUT_PREVIEW_APPLY_CANCEL_OK:{LayoutPreviewApplyCancelOk}",
        $"LAYOUT_UNDO_TRANSACTION_OK:{LayoutUndoTransactionOk}",
        $"READABILITY_FOCUS_SUBGRAPH_OK:{ReadabilityFocusSubgraphOk}",
        $"READABILITY_ROUTE_CLEANUP_OK:{ReadabilityRouteCleanupOk}",
        $"READABILITY_ALIGNMENT_HELPERS_OK:{ReadabilityAlignmentHelpersOk}",
        $"PLUGIN_LOCAL_GALLERY_OK:{PluginLocalGalleryOk}",
        $"PLUGIN_TRUST_EVIDENCE_PANEL_OK:{PluginTrustEvidencePanelOk}",
        $"PLUGIN_ALLOWLIST_ROUNDTRIP_OK:{PluginAllowlistRoundtripOk}",
        $"PLUGIN_SAMPLE_PACK_OK:{PluginSamplePackOk}",
        $"PLUGIN_SAMPLE_NODE_DEFINITIONS_OK:{PluginSampleNodeDefinitionsOk}",
        $"PLUGIN_SAMPLE_PARAMETER_METADATA_OK:{PluginSampleParameterMetadataOk}",
        $"NODE_DEFINITION_BUILDER_OK:{NodeDefinitionBuilderOk}",
        $"PORT_DEFINITION_BUILDER_OK:{PortDefinitionBuilderOk}",
        $"PARAMETER_DEFINITION_BUILDER_OK:{ParameterDefinitionBuilderOk}",
        $"CONNECTION_RULE_BUILDER_OK:{ConnectionRuleBuilderOk}",
        $"AUTHORING_BUILDER_THIN_WRAPPER_OK:{AuthoringBuilderThinWrapperOk}",
        $"GRAPH_SNIPPET_CATALOG_OK:{GraphSnippetCatalogOk}",
        $"GRAPH_SNIPPET_INSERT_OK:{GraphSnippetInsertOk}",
        $"FRAGMENT_LIBRARY_SEARCH_OK:{FragmentLibrarySearchOk}",
        $"FRAGMENT_LIBRARY_PREVIEW_OK:{FragmentLibraryPreviewOk}",
        $"FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK:{FragmentLibraryRecentsFavoritesOk}",
        $"FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK:{FragmentLibraryScopeBoundaryOk}",
        $"WORKBENCH_DEFAULTS_OK:{WorkbenchDefaultsOk}",
        $"WORKBENCH_HOST_BUILDER_HANDOFF_OK:{WorkbenchHostBuilderHandoffOk}",
        $"WORKBENCH_PERFORMANCE_MODE_OK:{WorkbenchPerformanceModeOk}",
        $"BALANCED_MODE_DEFAULT_OK:{BalancedModeDefaultOk}",
        $"WORKBENCH_LOD_POLICY_OK:{WorkbenchLodPolicyOk}",
        $"PERFORMANCE_MODE_SCOPE_BOUNDARY_OK:{PerformanceModeScopeBoundaryOk}",
        $"WORKBENCH_LAYOUT_PRESETS_OK:{WorkbenchLayoutPresetsOk}",
        $"WORKBENCH_LAYOUT_RESET_OK:{WorkbenchLayoutResetOk}",
        $"PANEL_STATE_PERSISTENCE_OK:{PanelStatePersistenceOk}",
        $"WORKBENCH_SCOPE_BOUNDARY_OK:{WorkbenchScopeBoundaryOk}",
        $"MINIMAP_LIGHTWEIGHT_PROJECTION_EVIDENCE_OK:{MiniMapLightweightProjectionEvidenceOk}",
        $"AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:{NodeSideAuthoringOk}",
        $"AUTHORING_SURFACE_COMMAND_PROJECTION_OK:{CommandSurfaceOk}",
        $"CONSUMER_SAMPLE_PARAMETER_OK:{ParameterProjectionOk}",
        $"CONSUMER_SAMPLE_METADATA_PROJECTION_OK:{MetadataProjectionOk}",
        $"CONSUMER_SAMPLE_WINDOW_OK:{WindowCompositionOk}",
        $"CONSUMER_SAMPLE_TRUST_OK:{TrustTransparencyOk}",
        $"COMMAND_SURFACE_OK:{CommandSurfaceOk}",
        $"CAPABILITY_BREADTH_STENCIL_OK:{StencilSurfaceOk}",
        $"STENCIL_GROUPING_OK:{StencilGroupingOk}",
        $"STENCIL_SEARCH_OK:{StencilSearchOk}",
        $"STENCIL_RECENTS_FAVORITES_OK:{StencilRecentsFavoritesOk}",
        $"STENCIL_SOURCE_FILTER_OK:{StencilSourceFilterOk}",
        $"STENCIL_STATE_PERSISTENCE_OK:{StencilStatePersistenceOk}",
        $"CAPABILITY_BREADTH_EXPORT_OK:{ExportBreadthOk}",
        $"EXPORT_PANEL_OK:{ExportPanelOk}",
        $"EXPORT_PANEL_SCOPE_OK:{ExportPanelScopeOk}",
        $"EXPORT_PANEL_PROGRESS_CANCEL_OK:{ExportPanelProgressCancelOk}",
        $"EXPORT_DOCS_ALIGNMENT_OK:{ExportDocsAlignmentOk}",
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
        $"GRAPH_VALIDATION_FEEDBACK_OK:{GraphValidationFeedbackOk}",
        $"GRAPH_FEEDBACK_FOCUS_TARGET_OK:{GraphFeedbackFocusTargetOk}",
        $"GRAPH_READINESS_STATUS_OK:{GraphReadinessStatusOk}",
        $"GRAPH_ERROR_HELP_TARGET_OK:{GraphErrorHelpTargetOk}",
        $"GRAPH_PROBLEM_INSPECTOR_HELP_TARGET_OK:{GraphProblemInspectorHelpTargetOk}",
        $"REPAIR_HELP_REVIEW_LOOP_OK:{RepairHelpReviewLoopOk}",
        $"FIVE_MINUTE_ONBOARDING_OK:{FiveMinuteOnboardingOk}",
        $"ONBOARDING_CONFIGURATION_OK:{OnboardingConfigurationOk}",
        $"AUTHORING_SURFACE_OK:{AuthoringSurfaceOk}",
        $"EXPERIENCE_POLISH_HANDOFF_OK:{ExperiencePolishHandoffOk}",
        $"FEATURE_ENHANCEMENT_PROOF_OK:{FeatureEnhancementProofOk}",
        $"AUTHORING_FLOW_PROOF_OK:{AuthoringFlowProofOk}",
        $"AUTHORING_FLOW_HANDOFF_OK:{AuthoringFlowHandoffOk}",
        $"AUTHORING_FLOW_SCOPE_BOUNDARY_OK:{AuthoringFlowScopeBoundaryOk}",
        $"EXPERIENCE_SCOPE_BOUNDARY_OK:{ExperienceScopeBoundaryOk}",
        $"AUTHORING_DEPTH_HANDOFF_OK:{AuthoringDepthHandoffOk}",
        $"AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:{AuthoringDepthScopeBoundaryOk}",
        $"V058_MILESTONE_PROOF_OK:{V058MilestoneProofOk}",
        $"LARGE_GRAPH_UX_POLICY_OK:{LargeGraphUxPolicyOk}",
        $"LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:{LargeGraphUxScopeBoundaryOk}",
        $"LARGE_GRAPH_UX_PROOF_BASELINE_OK:{LargeGraphUxProofBaselineOk}",
        $"VIEWPORT_LOD_POLICY_OK:{ViewportLodPolicyOk}",
        $"SELECTED_HOVERED_ADORNER_SCOPE_OK:{SelectedHoveredAdornerScopeOk}",
        $"LARGE_GRAPH_BALANCED_UX_OK:{LargeGraphBalancedUxOk}",
        $"VIEWPORT_LOD_SCOPE_BOUNDARY_OK:{ViewportLodScopeBoundaryOk}",
        $"EDGE_INTERACTION_CACHE_OK:{EdgeInteractionCacheOk}",
        $"EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:{EdgeDragRouteSimplificationOk}",
        $"SELECTED_EDGE_FEEDBACK_OK:{SelectedEdgeFeedbackOk}",
        $"EDGE_RENDERING_SCOPE_BOUNDARY_OK:{EdgeRenderingScopeBoundaryOk}",
        $"NODE_DRAG_EDGE_REFRESH_OK:{NodeDragEdgeRefreshOk}",
        $"EDGE_ROUTE_REFRESH_OK:{EdgeRouteRefreshOk}",
        $"LIVE_CANVAS_REFRESH_AUDIT_OK:{LiveCanvasRefreshAuditOk}",
        $"MINIMAP_LIGHTWEIGHT_PROJECTION_OK:{MinimapLightweightProjectionOk}",
        $"INSPECTOR_NARROW_PROJECTION_OK:{InspectorNarrowProjectionOk}",
        $"LARGE_GRAPH_PANEL_SCOPE_OK:{LargeGraphPanelScopeOk}",
        $"PROJECTION_PERFORMANCE_EVIDENCE_OK:{ProjectionPerformanceEvidenceOk}",
        $"LARGE_GRAPH_UX_HANDOFF_OK:{LargeGraphUxHandoffOk}",
        $"V059_MILESTONE_PROOF_OK:{V059MilestoneProofOk}",
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

    public string SupportReadinessStatus
        => ReadinessStatus;

    public ConsumerSampleProofValidationSummary SupportValidationSummary
        => ValidationSummary;

    public IReadOnlyList<ConsumerSampleProofValidationFeedback> SupportValidationFeedback
        => ValidationFeedback;

    public bool GraphValidationFeedbackOk
        => SupportValidationSummary.TotalIssueCount == SupportValidationFeedback.Count
        && SupportValidationSummary.TotalIssueCount == SupportValidationSummary.ErrorCount + SupportValidationSummary.WarningCount
        && SupportValidationSummary.ErrorCount == SupportValidationFeedback.Count(static feedback => string.Equals(feedback.Severity, "Error", StringComparison.Ordinal))
        && SupportValidationSummary.WarningCount == SupportValidationFeedback.Count(static feedback => string.Equals(feedback.Severity, "Warning", StringComparison.Ordinal))
        && SupportValidationSummary.InvalidConnectionCount == SupportValidationFeedback.Count(static feedback =>
            feedback.FocusTarget.ConnectionId is not null
            || feedback.Code.StartsWith("connection.", StringComparison.Ordinal))
        && SupportValidationSummary.InvalidParameterCount == SupportValidationFeedback.Count(static feedback =>
            feedback.FocusTarget.ParameterKey is not null
            || feedback.Code.StartsWith("node.parameter-", StringComparison.Ordinal))
        && SupportValidationFeedback.All(static feedback =>
            !string.IsNullOrWhiteSpace(feedback.Code)
            && !string.IsNullOrWhiteSpace(feedback.Message)
            && (string.Equals(feedback.Severity, "Error", StringComparison.Ordinal)
                || string.Equals(feedback.Severity, "Warning", StringComparison.Ordinal)));

    public bool GraphFeedbackFocusTargetOk
        => SupportValidationFeedback.All(static feedback => HasCoherentFocusTarget(feedback.FocusTarget));

    public bool GraphReadinessStatusOk
        => SupportReadinessStatus switch
        {
            "Ready" => SupportValidationSummary.ErrorCount == 0 && SupportValidationSummary.WarningCount == 0,
            "Warnings" => SupportValidationSummary.ErrorCount == 0 && SupportValidationSummary.WarningCount > 0,
            "Blocked" => SupportValidationSummary.ErrorCount > 0,
            _ => false,
        };

    private static string FormatMetric(string name, double value)
        => $"HOST_NATIVE_METRIC:{name}={value.ToString("0.###", CultureInfo.InvariantCulture)}";

    private static bool HasCoherentFocusTarget(ConsumerSampleProofFocusTarget target)
        => target.Kind switch
        {
            "Graph" => target.NodeId is null
                && target.ConnectionId is null
                && target.EndpointId is null
                && target.ParameterKey is null,
            "Node" => target.NodeId is not null,
            "Connection" => target.ConnectionId is not null,
            "ConnectionEndpoint" => target.ConnectionId is not null && target.EndpointId is not null,
            "Parameter" => target.NodeId is not null && target.ParameterKey is not null,
            _ => false,
        };


    private static bool IsBoundedFeatureDescriptorId(string featureDescriptorId)
        => !featureDescriptorId.Contains("marketplace", StringComparison.OrdinalIgnoreCase)
        && !featureDescriptorId.Contains("sandbox", StringComparison.OrdinalIgnoreCase)
        && !featureDescriptorId.Contains("untrusted", StringComparison.OrdinalIgnoreCase)
        && !featureDescriptorId.Contains("wpf", StringComparison.OrdinalIgnoreCase)
        && !featureDescriptorId.Contains("runtime.snippet", StringComparison.OrdinalIgnoreCase)
        && !featureDescriptorId.Contains("snippet-runtime", StringComparison.OrdinalIgnoreCase);
}

public static class ConsumerSampleProof
{
    private sealed class ConsumerSampleProofProgressSink : IProgress<GraphEditorSceneImageExportProgressSnapshot>
    {
        private readonly List<GraphEditorSceneImageExportProgressSnapshot> _snapshots;

        public ConsumerSampleProofProgressSink(List<GraphEditorSceneImageExportProgressSnapshot> snapshots)
            => _snapshots = snapshots;

        public void Report(GraphEditorSceneImageExportProgressSnapshot value)
            => _snapshots.Add(value);
    }

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
        bool runtimeDebugPanelInteractionOk;
        bool runtimeLogLocateOk;
        bool runtimeLogExportOk;
        bool graphSearchLocateOk;
        bool graphSearchScopeFilterOk;
        bool graphSearchViewportFocusOk;
        bool unifiedDiscoverySurfaceOk;
        bool discoverySourceLabelsOk;
        bool discoveryCommandRouteOk;
        bool workbenchRecentsOk;
        bool workbenchFavoritesOk;
        bool recentsFavoritesSupportBundleOk;
        IReadOnlyList<ConsumerSampleRecentsFavoritesEvidence> recentsFavoritesEvidence;
        bool workbenchFrictionEvidenceOk;
        bool workbenchFrictionPrioritizationOk;
        bool workbenchFrictionScopeBoundaryOk;
        IReadOnlyList<ConsumerSampleWorkbenchFrictionEvidence> workbenchFrictionEvidence;
        bool workbenchAffordancePolishOk;
        bool workbenchAffordanceRouteOk;
        bool workbenchAffordanceScopeBoundaryOk;
        ConsumerSampleWorkbenchAffordancePolish workbenchAffordancePolish;
        bool navigationHistoryOk;
        bool scopeBreadcrumbNavigationOk;
        bool focusRestoreOk;
        bool layoutProviderSeamOk;
        bool layoutPreviewApplyCancelOk;
        bool layoutUndoTransactionOk;
        bool readabilityFocusSubgraphOk;
        bool readabilityRouteCleanupOk;
        bool readabilityAlignmentHelpersOk;
        bool pluginLocalGalleryOk;
        bool pluginTrustEvidencePanelOk;
        bool pluginAllowlistRoundtripOk;
        bool pluginSamplePackOk;
        bool pluginSampleNodeDefinitionsOk;
        bool pluginSampleParameterMetadataOk;
        bool nodeDefinitionBuilderOk;
        bool portDefinitionBuilderOk;
        bool parameterDefinitionBuilderOk;
        bool connectionRuleBuilderOk;
        bool authoringBuilderThinWrapperOk;
        bool graphSnippetCatalogOk;
        bool graphSnippetInsertOk;
        bool fragmentLibrarySearchOk;
        bool fragmentLibraryPreviewOk;
        bool fragmentLibraryRecentsFavoritesOk;
        bool fragmentLibraryScopeBoundaryOk;
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
            pluginAllowlistRoundtripOk = trustTransparencyOk;
            (pluginSamplePackOk, pluginSampleNodeDefinitionsOk, pluginSampleParameterMetadataOk) = HasSamplePluginPack(host);
            (nodeDefinitionBuilderOk, portDefinitionBuilderOk, parameterDefinitionBuilderOk, connectionRuleBuilderOk, authoringBuilderThinWrapperOk) = HasAuthoringDefinitionBuilders();

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
                && FindNamed<ItemsControl>(window, "PART_LocalPluginGalleryItems") is not null
                && FindNamed<ItemsControl>(window, "PART_AllowlistItems") is not null
                && FindNamed<Button>(window, "PART_PreviewLayoutButton") is not null
                && FindNamed<Button>(window, "PART_ApplyLayoutPreviewButton") is not null
                && FindNamed<Button>(window, "PART_CancelLayoutPreviewButton") is not null
                && FindNamed<TextBlock>(window, "PART_NavigationHistorySummaryText") is not null
                && FindNamed<Button>(window, "PART_NavigationBackButton") is not null
                && FindNamed<Button>(window, "PART_NavigationForwardButton") is not null
                && FindNamed<Button>(window, "PART_FocusCurrentScopeButton") is not null
                && FindNamed<Button>(window, "PART_RestoreViewportButton") is not null
                && FindNamed<ItemsControl>(window, "PART_ScopeBreadcrumbItems") is not null
                && FindNamed<TextBlock>(window, "PART_TrustBoundaryText") is not null;
            runtimeLogPanelOk = HasRuntimeLogPanel(window, host);
            runtimeLogFilterOk = HasRuntimeLogFilter(host);
            runtimeDebugPanelInteractionOk = HasRuntimeDebugPanelInteraction(window, host);
            runtimeLogLocateOk = HasRuntimeLogLocate(host);
            runtimeLogExportOk = HasRuntimeLogExport(host);
            (graphSearchLocateOk, graphSearchScopeFilterOk, graphSearchViewportFocusOk) = HasGraphSearchLocate(host, window);
            (navigationHistoryOk, scopeBreadcrumbNavigationOk, focusRestoreOk) = HasNavigationHistoryAndScopeFocus(host, window);
            layoutProviderSeamOk = HasLayoutProviderSeam(host);
            (layoutPreviewApplyCancelOk, layoutUndoTransactionOk) = HasLayoutPreviewApplyCancel(host, window);
            (readabilityFocusSubgraphOk, readabilityRouteCleanupOk, readabilityAlignmentHelpersOk) = HasReadabilityHelpers(host);
            pluginLocalGalleryOk = HasLocalPluginGallery(host, window);
            pluginTrustEvidencePanelOk = HasPluginTrustEvidencePanel(host);
            (unifiedDiscoverySurfaceOk, discoverySourceLabelsOk, discoveryCommandRouteOk) =
                HasUnifiedDiscoverySurface(host);
        }
        finally
        {
            window.Close();
        }

        var exportBreadthOk = HasExportBreadth(host, storageRoot);
        var capabilityWindow = CreateCapabilityBreadthWindow(host);
        bool stencilSurfaceOk;
        bool stencilGroupingOk;
        bool stencilSearchOk;
        bool stencilRecentsFavoritesOk;
        bool stencilSourceFilterOk;
        bool stencilStatePersistenceOk;
        bool exportPanelOk;
        bool exportPanelScopeOk;
        bool exportPanelProgressCancelOk;
        bool exportDocsAlignmentOk;
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
        bool runtimeOverlaySnapshotPolishOk;
        bool runtimeOverlayScopeFilterOk;
        bool portHandleIdOk;
        bool portGroupAuthoringOk;
        bool portConnectionHintOk;
        bool portAuthoringScopeBoundaryOk;
        bool connectionValidationReasonOk;
        bool connectionInvalidHoverFeedbackOk;
        bool connectionValidationSupportBundleOk;
        bool connectionValidationScopeBoundaryOk;
        bool connectionValidationHelpTargetOk;
        IReadOnlyList<ConsumerSampleRepairEvidence> connectionValidationRepairEvidence;
        bool commandPaletteGroupingOk;
        bool commandPaletteDisabledReasonOk;
        bool commandPaletteRecentActionsOk;
        bool commandProjectionUnifiedOk;
        bool commandPaletteOk;
        bool toolbarDescriptorOk;
        bool contextMenuDescriptorOk;
        bool commandDisabledReasonOk;
        bool nodeToolbarContributionOk;
        bool edgeToolbarContributionOk;
        bool toolbarContributionDescriptorOk;
        bool toolbarContributionScopeBoundaryOk;
        bool workbenchDefaultsOk;
        bool workbenchHostBuilderHandoffOk;
        bool workbenchPerformanceModeOk;
        bool balancedModeDefaultOk;
        bool workbenchLodPolicyOk;
        bool performanceModeScopeBoundaryOk;
        bool workbenchLayoutPresetsOk;
        bool workbenchLayoutResetOk;
        bool panelStatePersistenceOk;
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

            (stencilSurfaceOk, stencilGroupingOk, stencilSearchOk, stencilRecentsFavoritesOk, stencilSourceFilterOk, stencilStatePersistenceOk) =
                HasStencilSurface(capabilityWindow, host);
            host.SelectNode(reviewNodeId);
            FlushUi();
            (exportPanelOk, exportPanelScopeOk, exportPanelProgressCancelOk) = HasExportPanel(capabilityWindow, host);
            exportDocsAlignmentOk = HasExportDocsAlignment();
            host.SelectNode(reviewNodeId);
            FlushUi();
            edgeQuickToolsOk = HasEdgeQuickTools(capabilityWindow, host);
            (dropNodeOnEdgeOk, edgeSplitCompatibilityOk, edgeSplitUndoOk) = HasDropNodeOnEdge(host);
            (deleteAndReconnectOk, detachNodeOk, reconnectConflictReportOk) = HasReconnectDetach(host);
            (edgeMultiSelectOk, wireSliceOk, selectedNodeEdgeHighlightOk) = HasWireSelectionAndSlicing(host);
            runtimeOverlaySnapshotOk = HasRuntimeOverlaySnapshot(host);
            runtimeOverlaySnapshotPolishOk = HasRuntimeOverlaySnapshotPolish(host);
            runtimeOverlayScopeFilterOk = HasRuntimeOverlayScopeFilter(host);
            (portHandleIdOk, portGroupAuthoringOk, portConnectionHintOk, portAuthoringScopeBoundaryOk) =
                HasPortHandleAuthoring(host);
            (
                connectionValidationReasonOk,
                connectionInvalidHoverFeedbackOk,
                connectionValidationSupportBundleOk,
                connectionValidationScopeBoundaryOk,
                connectionValidationHelpTargetOk,
                connectionValidationRepairEvidence) =
                HasConnectionValidationFeedback();
            (quickAddConnectedNodeOk, portFilteredNodeSearchOk) = HasQuickAddConnectedNode(host);
            (commandPaletteGroupingOk, commandPaletteDisabledReasonOk, commandPaletteRecentActionsOk) =
                HasCommandPaletteProductivity(capabilityWindow);
            (commandProjectionUnifiedOk, commandPaletteOk, toolbarDescriptorOk, contextMenuDescriptorOk, commandDisabledReasonOk) =
                HasUnifiedCommandProjection(capabilityWindow, host);
            (nodeToolbarContributionOk, edgeToolbarContributionOk, toolbarContributionDescriptorOk, toolbarContributionScopeBoundaryOk) =
                HasToolbarContributions(host, reviewNodeId);
            (
                workbenchDefaultsOk,
                workbenchHostBuilderHandoffOk,
                workbenchPerformanceModeOk,
                balancedModeDefaultOk,
                workbenchLodPolicyOk,
                performanceModeScopeBoundaryOk,
                workbenchLayoutPresetsOk,
                workbenchLayoutResetOk,
                panelStatePersistenceOk) =
                HasWorkbenchDefaults(host);

            host.SelectNode(reviewNodeId);
            FlushUi();
            nodeQuickToolsOk = HasNodeQuickTools(capabilityWindow, host, reviewNodeId);
        }
        finally
        {
            capabilityWindow.Close();
        }

        graphSnippetCatalogOk = HasGraphSnippetCatalog(host);
        graphSnippetInsertOk = HasGraphSnippetInsertion(host);
        (fragmentLibrarySearchOk, fragmentLibraryPreviewOk, fragmentLibraryRecentsFavoritesOk, fragmentLibraryScopeBoundaryOk) =
            HasFragmentLibraryConvenience(host);
        (workbenchRecentsOk, workbenchFavoritesOk, recentsFavoritesSupportBundleOk, recentsFavoritesEvidence) =
            HasWorkbenchRecentsFavorites(host);
        (
            workbenchFrictionEvidenceOk,
            workbenchFrictionPrioritizationOk,
            workbenchFrictionScopeBoundaryOk,
            workbenchFrictionEvidence) =
            HasWorkbenchFrictionEvidence(host);
        (
            workbenchAffordancePolishOk,
            workbenchAffordanceRouteOk,
            workbenchAffordanceScopeBoundaryOk,
            workbenchAffordancePolish) =
            HasWorkbenchAffordancePolish(host, workbenchFrictionEvidence);
        host.SelectNode(host.GetFirstReviewNodeId());
        FlushUi();

        var finalSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        var validationSnapshot = host.Session.Diagnostics.CaptureInspectionSnapshot().ValidationSnapshot;
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
        var (validationFixSnapshots, validationFixRepairEvidence, validationFixHelpTargetOk, validationFixInspectorHelpTargetOk) =
            CreateValidationFixProof();
        var proofParameterSnapshots = CreateParameterSnapshots(
            [.. selectedSupportSnapshots, .. mixedValueSnapshots, .. validationFixSnapshots]);
        var validationSummary = CreateValidationSummary(validationSnapshot);
        var validationFeedback = CreateValidationFeedback(validationSnapshot);
        var repairEvidence = CreateRepairEvidence(host.Session, validationSnapshot)
            .Concat(validationFixRepairEvidence)
            .Concat(connectionValidationRepairEvidence)
            .ToArray();
        var graphErrorHelpTargetOk = validationFixHelpTargetOk && connectionValidationHelpTargetOk;
        var graphProblemInspectorHelpTargetOk = validationFixInspectorHelpTargetOk;
        var repairHelpReviewLoopOk = graphErrorHelpTargetOk
            && graphProblemInspectorHelpTargetOk
            && repairEvidence.Any(static evidence => string.Equals(evidence.Action, "validation.parameter.reset-default", StringComparison.Ordinal))
            && repairEvidence.Any(static evidence => string.Equals(evidence.Action, "validation.connection.remove", StringComparison.Ordinal));
        var supportBundlePayloadOk = HasSupportBundlePayload(proofParameterSnapshots, finalSnapshot, featureDescriptorIds);
        var runtimeOverlaySupportBundleOk = HasRuntimeOverlaySupportBundlePayload(runtimeOverlay);
        var miniMapLightweightProjectionEvidenceOk = HasLightweightMiniMapProjection(host);
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
            RuntimeOverlaySnapshotPolishOk: runtimeOverlaySnapshotPolishOk,
            RuntimeOverlayScopeFilterOk: runtimeOverlayScopeFilterOk,
            RuntimeLogPanelOk: runtimeLogPanelOk,
            RuntimeLogFilterOk: runtimeLogFilterOk,
            RuntimeDebugPanelInteractionOk: runtimeDebugPanelInteractionOk,
            RuntimeLogLocateOk: runtimeLogLocateOk,
            RuntimeLogExportOk: runtimeLogExportOk,
            RuntimeOverlaySupportBundleOk: runtimeOverlaySupportBundleOk,
            PortHandleIdOk: portHandleIdOk,
            PortGroupAuthoringOk: portGroupAuthoringOk,
            PortConnectionHintOk: portConnectionHintOk,
            PortAuthoringScopeBoundaryOk: portAuthoringScopeBoundaryOk,
            ConnectionValidationReasonOk: connectionValidationReasonOk,
            ConnectionInvalidHoverFeedbackOk: connectionInvalidHoverFeedbackOk,
            ConnectionValidationSupportBundleOk: connectionValidationSupportBundleOk,
            ConnectionValidationScopeBoundaryOk: connectionValidationScopeBoundaryOk,
            GraphSearchLocateOk: graphSearchLocateOk,
            GraphSearchScopeFilterOk: graphSearchScopeFilterOk,
            GraphSearchViewportFocusOk: graphSearchViewportFocusOk,
            UnifiedDiscoverySurfaceOk: unifiedDiscoverySurfaceOk,
            DiscoverySourceLabelsOk: discoverySourceLabelsOk,
            DiscoveryCommandRouteOk: discoveryCommandRouteOk,
            WorkbenchRecentsOk: workbenchRecentsOk,
            WorkbenchFavoritesOk: workbenchFavoritesOk,
            RecentsFavoritesSupportBundleOk: recentsFavoritesSupportBundleOk,
            RecentsFavoritesEvidence: recentsFavoritesEvidence,
            WorkbenchFrictionEvidenceOk: workbenchFrictionEvidenceOk,
            WorkbenchFrictionPrioritizationOk: workbenchFrictionPrioritizationOk,
            WorkbenchFrictionScopeBoundaryOk: workbenchFrictionScopeBoundaryOk,
            WorkbenchFrictionEvidence: workbenchFrictionEvidence,
            WorkbenchAffordancePolishOk: workbenchAffordancePolishOk,
            WorkbenchAffordanceRouteOk: workbenchAffordanceRouteOk,
            WorkbenchAffordanceScopeBoundaryOk: workbenchAffordanceScopeBoundaryOk,
            WorkbenchAffordancePolish: workbenchAffordancePolish,
            CommandPaletteGroupingOk: commandPaletteGroupingOk,
            CommandPaletteDisabledReasonOk: commandPaletteDisabledReasonOk,
            CommandPaletteRecentActionsOk: commandPaletteRecentActionsOk,
            CommandProjectionUnifiedOk: commandProjectionUnifiedOk,
            CommandPaletteOk: commandPaletteOk,
            ToolbarDescriptorOk: toolbarDescriptorOk,
            ContextMenuDescriptorOk: contextMenuDescriptorOk,
            CommandDisabledReasonOk: commandDisabledReasonOk,
            NodeToolbarContributionOk: nodeToolbarContributionOk,
            EdgeToolbarContributionOk: edgeToolbarContributionOk,
            ToolbarContributionDescriptorOk: toolbarContributionDescriptorOk,
            ToolbarContributionScopeBoundaryOk: toolbarContributionScopeBoundaryOk,
            StencilGroupingOk: stencilGroupingOk,
            StencilSearchOk: stencilSearchOk,
            StencilRecentsFavoritesOk: stencilRecentsFavoritesOk,
            StencilSourceFilterOk: stencilSourceFilterOk,
            StencilStatePersistenceOk: stencilStatePersistenceOk,
            ExportPanelOk: exportPanelOk,
            ExportPanelScopeOk: exportPanelScopeOk,
            ExportPanelProgressCancelOk: exportPanelProgressCancelOk,
            ExportDocsAlignmentOk: exportDocsAlignmentOk,
            NavigationHistoryOk: navigationHistoryOk,
            ScopeBreadcrumbNavigationOk: scopeBreadcrumbNavigationOk,
            FocusRestoreOk: focusRestoreOk,
            LayoutProviderSeamOk: layoutProviderSeamOk,
            LayoutPreviewApplyCancelOk: layoutPreviewApplyCancelOk,
            LayoutUndoTransactionOk: layoutUndoTransactionOk,
            ReadabilityFocusSubgraphOk: readabilityFocusSubgraphOk,
            ReadabilityRouteCleanupOk: readabilityRouteCleanupOk,
            ReadabilityAlignmentHelpersOk: readabilityAlignmentHelpersOk,
            PluginLocalGalleryOk: pluginLocalGalleryOk,
            PluginTrustEvidencePanelOk: pluginTrustEvidencePanelOk,
            PluginAllowlistRoundtripOk: pluginAllowlistRoundtripOk,
            PluginSamplePackOk: pluginSamplePackOk,
            PluginSampleNodeDefinitionsOk: pluginSampleNodeDefinitionsOk,
            PluginSampleParameterMetadataOk: pluginSampleParameterMetadataOk,
            NodeDefinitionBuilderOk: nodeDefinitionBuilderOk,
            PortDefinitionBuilderOk: portDefinitionBuilderOk,
            ParameterDefinitionBuilderOk: parameterDefinitionBuilderOk,
            ConnectionRuleBuilderOk: connectionRuleBuilderOk,
            AuthoringBuilderThinWrapperOk: authoringBuilderThinWrapperOk,
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
            RepairEvidence: repairEvidence,
            MiniMapLightweightProjectionEvidenceOk: miniMapLightweightProjectionEvidenceOk,
            SelectedParameterProjectionCount: selectedSupportSnapshots.Length,
            TotalParameterProjectionCount: proofParameterSnapshots.Count,
            GraphErrorHelpTargetOk: graphErrorHelpTargetOk,
            GraphProblemInspectorHelpTargetOk: graphProblemInspectorHelpTargetOk,
            RepairHelpReviewLoopOk: repairHelpReviewLoopOk,
            ReadinessStatus: CreateReadinessStatus(validationSnapshot),
            ValidationSummary: validationSummary,
            ValidationFeedback: validationFeedback,
            HostedAutomationNavigationOk: hostedAutomationNavigationOk,
            HostedAuthoringAutomationDiagnosticsOk: hostedAuthoringAutomationDiagnosticsOk,
            ScenarioGraphOk: scenarioGraphOk,
            HostOwnedActionsOk: hostOwnedActionsOk,
            SupportBundlePayloadOk: supportBundlePayloadOk,
            FiveMinuteOnboardingOk: fiveMinuteOnboardingOk,
            GraphSnippetCatalogOk: graphSnippetCatalogOk,
            GraphSnippetInsertOk: graphSnippetInsertOk,
            FragmentLibrarySearchOk: fragmentLibrarySearchOk,
            FragmentLibraryPreviewOk: fragmentLibraryPreviewOk,
            FragmentLibraryRecentsFavoritesOk: fragmentLibraryRecentsFavoritesOk,
            FragmentLibraryScopeBoundaryOk: fragmentLibraryScopeBoundaryOk,
            WorkbenchDefaultsOk: workbenchDefaultsOk,
            WorkbenchHostBuilderHandoffOk: workbenchHostBuilderHandoffOk,
            WorkbenchPerformanceModeOk: workbenchPerformanceModeOk,
            BalancedModeDefaultOk: balancedModeDefaultOk,
            WorkbenchLodPolicyOk: workbenchLodPolicyOk,
            PerformanceModeScopeBoundaryOk: performanceModeScopeBoundaryOk,
            WorkbenchLayoutPresetsOk: workbenchLayoutPresetsOk,
            WorkbenchLayoutResetOk: workbenchLayoutResetOk,
            PanelStatePersistenceOk: panelStatePersistenceOk);
    }

    private static (
        bool DefaultsOk,
        bool HandoffOk,
        bool PerformanceModeOk,
        bool BalancedDefaultOk,
        bool LodPolicyOk,
        bool ScopeBoundaryOk,
        bool LayoutPresetsOk,
        bool LayoutResetOk,
        bool PanelStatePersistenceOk) HasWorkbenchDefaults(ConsumerSampleHost host)
    {
        var builder = AsterGraphHostBuilder
            .Create()
            .UseDefaultWorkbench();
        var options = builder.BuildViewOptions(host.Editor);
        var view = AsterGraphAvaloniaViewFactory.Create(options);

        var defaultsOk = options.Workbench == AsterGraphWorkbenchOptions.Default
            && options.ChromeMode == GraphEditorViewChromeMode.Default
            && options.EnableDefaultContextMenu
            && options.CommandShortcutPolicy == AsterGraphCommandShortcutPolicy.Default
            && options.Workbench.LayoutPreset == AsterGraphWorkbenchLayoutPreset.Authoring
            && options.Workbench.PanelState == AsterGraphWorkbenchPanelState.Default
            && options.Workbench.ShowHeaderChrome
            && options.Workbench.ShowNodePalette
            && options.Workbench.ShowInspector
            && options.Workbench.ShowStatus
            && options.Workbench.EnableDefaultWheelViewportGestures
            && options.Workbench.EnableAltLeftDragPanning
            && options.Workbench.PerformanceMode == AsterGraphWorkbenchPerformanceMode.Balanced;

        var handoffOk = ReferenceEquals(host.Editor, options.Editor)
            && ReferenceEquals(host.Editor, view.Editor)
            && view.ChromeMode == GraphEditorViewChromeMode.Default
            && view.IsHeaderChromeVisible
            && view.IsLibraryChromeVisible
            && view.IsInspectorChromeVisible
            && view.IsStatusChromeVisible
            && view.EnableDefaultContextMenu
            && view.CommandShortcutPolicy == AsterGraphCommandShortcutPolicy.Default
            && view.EnableDefaultWheelViewportGestures
            && view.EnableAltLeftDragPanning
            && view.WorkbenchPerformanceMode == AsterGraphWorkbenchPerformanceMode.Balanced;

        var quality = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Quality);
        var balanced = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Balanced);
        var throughput = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Throughput);
        var throughputView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = host.Editor,
            Workbench = AsterGraphWorkbenchOptions.Default with
            {
                PerformanceMode = AsterGraphWorkbenchPerformanceMode.Throughput,
            },
        });

        var performanceModeOk = Enum.GetValues<AsterGraphWorkbenchPerformanceMode>().Length == 3
            && throughputView.WorkbenchPerformanceMode == AsterGraphWorkbenchPerformanceMode.Throughput;
        var balancedDefaultOk = AsterGraphWorkbenchOptions.Default.PerformanceMode == AsterGraphWorkbenchPerformanceMode.Balanced
            && AsterGraphWorkbenchOptions.Default.PerformancePolicy.Mode == AsterGraphWorkbenchPerformanceMode.Balanced;
        var lodPolicyOk = quality.StencilCardsPerSectionLimit > balanced.StencilCardsPerSectionLimit
            && balanced.StencilCardsPerSectionLimit > throughput.StencilCardsPerSectionLimit
            && quality.ProjectMiniMapContinuously
            && balanced.ProjectMiniMapContinuously
            && !throughput.ProjectMiniMapContinuously
            && quality.ProjectAdvancedInspectorByDefault
            && !balanced.ProjectAdvancedInspectorByDefault
            && !throughput.ProjectAdvancedInspectorByDefault
            && quality.ProjectHoveredToolbars
            && balanced.ProjectHoveredToolbars
            && !throughput.ProjectHoveredToolbars
            && throughput.CommandRefreshBatchMilliseconds > balanced.CommandRefreshBatchMilliseconds;
        var scopeBoundaryOk = typeof(AsterGraphWorkbenchPerformanceMode).Namespace == "AsterGraph.Avalonia.Hosting"
            && typeof(AsterGraphWorkbenchPerformancePolicy).Namespace == "AsterGraph.Avalonia.Hosting"
            && typeof(AsterGraphWorkbenchLayoutPreset).Namespace == "AsterGraph.Avalonia.Hosting"
            && typeof(AsterGraphWorkbenchPanelState).Namespace == "AsterGraph.Avalonia.Hosting";

        var presetValues = Enum.GetValues<AsterGraphWorkbenchLayoutPreset>();
        var authoring = AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.Authoring);
        var debugging = AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.Debugging);
        var compact = AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.CompactReview);
        var plugin = AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.PluginInspection);
        var layoutPresetsOk = presetValues.Length == 4
            && authoring.PanelState.StencilVisible
            && authoring.PanelState.MiniMapVisible
            && debugging.LayoutPreset == AsterGraphWorkbenchLayoutPreset.Debugging
            && !debugging.PanelState.StencilVisible
            && debugging.PanelState.RuntimePanelVisible
            && !debugging.PanelState.ExportPanelVisible
            && compact.LayoutPreset == AsterGraphWorkbenchLayoutPreset.CompactReview
            && !compact.PanelState.MiniMapVisible
            && compact.PanelState.StencilWidth < authoring.PanelState.StencilWidth
            && plugin.LayoutPreset == AsterGraphWorkbenchLayoutPreset.PluginInspection
            && plugin.PanelState.PluginPanelVisible
            && !plugin.PanelState.RuntimePanelVisible;

        var beforeDocument = host.Session.Queries.CreateDocumentSnapshot();
        var mutated = AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.Debugging) with
        {
            ShowHeaderChrome = false,
            ShowStatus = false,
            PerformanceMode = AsterGraphWorkbenchPerformanceMode.Throughput,
        };
        var reset = mutated.ResetLayout();
        var afterDocument = host.Session.Queries.CreateDocumentSnapshot();
        var layoutResetOk = reset == AsterGraphWorkbenchOptions.Default
            && beforeDocument.Nodes.Count == afterDocument.Nodes.Count
            && beforeDocument.Connections.Count == afterDocument.Connections.Count;

        var persistedPanelState = AsterGraphWorkbenchPanelState.Default with
        {
            StencilVisible = false,
            StencilWidth = 288,
            InspectorVisible = true,
            InspectorWidth = 360,
            MiniMapVisible = false,
            RuntimePanelVisible = true,
            ExportPanelVisible = false,
            PluginPanelVisible = true,
        };
        var persistedOptions = AsterGraphHostBuilder
            .Create()
            .UseWorkbench(AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.Debugging) with
            {
                PanelState = persistedPanelState,
            })
            .BuildViewOptions(host.Editor);
        var persistedView = AsterGraphAvaloniaViewFactory.Create(persistedOptions);
        var panelStatePersistenceOk = persistedOptions.Workbench.PanelState == persistedPanelState
            && persistedOptions.Workbench.PanelState.StencilWidth == 288
            && persistedOptions.Workbench.PanelState.InspectorWidth == 360
            && !persistedOptions.Workbench.PanelState.MiniMapVisible
            && persistedOptions.Workbench.PanelState.RuntimePanelVisible
            && !persistedOptions.Workbench.PanelState.ExportPanelVisible
            && persistedOptions.Workbench.PanelState.PluginPanelVisible
            && !persistedView.IsLibraryChromeVisible
            && persistedView.IsInspectorChromeVisible;

        return (
            defaultsOk,
            handoffOk,
            performanceModeOk,
            balancedDefaultOk,
            lodPolicyOk,
            scopeBoundaryOk,
            layoutPresetsOk,
            layoutResetOk,
            panelStatePersistenceOk);
    }

    private static bool HasLightweightMiniMapProjection(ConsumerSampleHost host)
    {
        var balancedView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = host.Editor,
            Workbench = AsterGraphWorkbenchOptions.Default with
            {
                PerformanceMode = AsterGraphWorkbenchPerformanceMode.Balanced,
            },
        });
        var throughputView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = host.Editor,
            Workbench = AsterGraphWorkbenchOptions.Default with
            {
                PerformanceMode = AsterGraphWorkbenchPerformanceMode.Throughput,
            },
        });

        var balancedMiniMap = balancedView.FindControl<GraphMiniMap>("PART_MiniMapSurface");
        var throughputMiniMap = throughputView.FindControl<GraphMiniMap>("PART_MiniMapSurface");
        return ReadMiniMapLightweightProjection(balancedMiniMap) is false
            && ReadMiniMapLightweightProjection(throughputMiniMap) is true;
    }

    private static bool? ReadMiniMapLightweightProjection(GraphMiniMap? miniMap)
    {
        if (miniMap is null)
        {
            return null;
        }

        var property = typeof(GraphMiniMap).GetProperty(
            "UsesLightweightProjection",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return property?.GetValue(miniMap) as bool?;
    }

    private static (bool NodeOk, bool PortOk, bool ParameterOk, bool RuleOk, bool ThinWrapperOk) HasAuthoringDefinitionBuilders()
    {
        var port = PortDefinitionBuilder
            .Create("payload", "Payload", "json")
            .Accent("#6AD5C4")
            .Description("Payload input")
            .Group("Data")
            .Connections(min: 1, max: 2)
            .Build();
        var parameter = NodeParameterDefinitionBuilder
            .Create("mode", "Mode", "enum", ParameterEditorKind.Enum)
            .DefaultValue("fast")
            .Group("Runtime")
            .Help("Choose a processing mode.")
            .Option("fast", "Fast")
            .Option("safe", "Safe")
            .Build();
        var definition = NodeDefinitionBuilder
            .Create("consumer.builder.node", "Builder Node")
            .Category("Consumer")
            .Subtitle("Thin wrapper")
            .Input("in", "Input", "json")
            .Output(port)
            .Parameter(parameter)
            .Build();
        var rule = ImplicitConversionRuleBuilder
            .ImplicitConversion("int", "double")
            .Conversion("consumer.int-to-double")
            .Build();

        var nodeOk = definition.Id == new NodeDefinitionId("consumer.builder.node")
            && definition.OutputPorts.Single() == port
            && definition.Parameters.Single() == parameter;
        var portOk = port == new PortDefinition(
            "payload",
            "Payload",
            new PortTypeId("json"),
            "#6AD5C4",
            "Payload input",
            "Data",
            1,
            2);
        var parameterOk = parameter.Key == "mode"
            && parameter.DisplayName == "Mode"
            && parameter.ValueType == new PortTypeId("enum")
            && parameter.EditorKind == ParameterEditorKind.Enum
            && Equals(parameter.DefaultValue, "fast")
            && parameter.GroupName == "Runtime"
            && parameter.HelpText == "Choose a processing mode."
            && parameter.Constraints.AllowedOptions.SequenceEqual(
            [
                new ParameterOptionDefinition("fast", "Fast"),
                new ParameterOptionDefinition("safe", "Safe"),
            ]);
        var ruleOk = rule == new ImplicitConversionRule(
            new PortTypeId("int"),
            new PortTypeId("double"),
            new ConversionId("consumer.int-to-double"));
        var thinWrapperOk = definition.GetType() == typeof(NodeDefinition)
            && port.GetType() == typeof(PortDefinition)
            && parameter.GetType() == typeof(NodeParameterDefinition)
            && rule.GetType() == typeof(ImplicitConversionRule);

        return (nodeOk, portOk, parameterOk, ruleOk, thinWrapperOk);
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

    private static bool HasGraphSnippetCatalog(ConsumerSampleHost host)
        => host.SnippetCatalog.Any(snippet =>
            string.Equals(snippet.Id, ConsumerSampleHost.QueueLaneSnippetId, StringComparison.Ordinal)
            && snippet.Title.Contains("Queue", StringComparison.Ordinal)
            && snippet.Description.Contains("review", StringComparison.OrdinalIgnoreCase));

    private static bool HasGraphSnippetInsertion(ConsumerSampleHost host)
    {
        var before = host.Session.Queries.CreateDocumentSnapshot();
        var sourceNode = before.Nodes.FirstOrDefault(node => node.DefinitionId == ConsumerSampleHost.ReviewDefinitionId);
        if (sourceNode is null || !host.TryInsertSnippet(ConsumerSampleHost.QueueLaneSnippetId))
        {
            return false;
        }

        var after = host.Session.Queries.CreateDocumentSnapshot();
        var createdQueueNodeIds = after.Nodes
            .Where(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .Select(node => node.Id)
            .Except(before.Nodes.Select(node => node.Id), StringComparer.Ordinal)
            .ToArray();

        return createdQueueNodeIds.Length == 1
            && after.Nodes.Count == before.Nodes.Count + 1
            && after.Connections.Count == before.Connections.Count + 1
            && after.Connections.Any(connection =>
                string.Equals(connection.SourceNodeId, sourceNode.Id, StringComparison.Ordinal)
                && string.Equals(connection.SourcePortId, "output", StringComparison.Ordinal)
                && string.Equals(connection.TargetNodeId, createdQueueNodeIds[0], StringComparison.Ordinal)
                && string.Equals(connection.TargetPortId, "input", StringComparison.Ordinal))
            && !host.Session.Queries.GetPendingConnectionSnapshot().HasPendingConnection;
    }

    private static (bool SearchOk, bool PreviewOk, bool RecentsFavoritesOk, bool ScopeBoundaryOk) HasFragmentLibraryConvenience(
        ConsumerSampleHost host)
    {
        var queueSearch = host.SearchSnippetCatalog("queue");
        var diagnosticsSearch = host.SearchSnippetCatalog("diagnostics");
        var preview = host.GetSnippetPreview(ConsumerSampleHost.QueueLaneSnippetId);
        var favoriteIds = host.FavoriteSnippetIds;
        var beforeRecentIds = host.RecentSnippetIds;
        var inserted = host.TryInsertSnippet(ConsumerSampleHost.QueueLaneSnippetId);
        var afterRecentIds = host.RecentSnippetIds;
        var storage = host.Session.Queries.GetFragmentStorageSnapshot();

        var searchOk = queueSearch.Any(snippet =>
                string.Equals(snippet.Id, ConsumerSampleHost.QueueLaneSnippetId, StringComparison.Ordinal)
                && string.Equals(snippet.Category, "Workflow", StringComparison.Ordinal)
                && snippet.SearchKeywords.Contains("connected", StringComparer.OrdinalIgnoreCase))
            && diagnosticsSearch.Any(snippet =>
                string.Equals(snippet.Category, "Diagnostics", StringComparison.Ordinal));
        var previewOk = !string.IsNullOrWhiteSpace(preview)
            && preview.Contains("Review output", StringComparison.Ordinal)
            && preview.Contains("Queue input", StringComparison.Ordinal);
        var recentsFavoritesOk = inserted
            && favoriteIds.Contains(ConsumerSampleHost.QueueLaneSnippetId, StringComparer.Ordinal)
            && afterRecentIds.Count > 0
            && afterRecentIds.Count >= beforeRecentIds.Count
            && string.Equals(afterRecentIds[0], ConsumerSampleHost.QueueLaneSnippetId, StringComparison.Ordinal);
        var scopeBoundaryOk = storage.CanExportSelectionAsTemplate
            && storage.CanImportFragmentTemplate
            && storage.CanDeleteFragmentTemplate
            && host.SnippetCatalog.All(snippet =>
                !string.IsNullOrWhiteSpace(snippet.Id)
                && !string.IsNullOrWhiteSpace(snippet.Category)
                && !string.IsNullOrWhiteSpace(snippet.PreviewText))
            && host.SearchSnippetCatalog("missing-fragment-library-query").Count == 0
            && host.GetSnippetPreview("missing-fragment-library-snippet") is null;

        return (searchOk, previewOk, recentsFavoritesOk, scopeBoundaryOk);
    }

    private static (
        bool RecentsOk,
        bool FavoritesOk,
        bool SupportBundleOk,
        IReadOnlyList<ConsumerSampleRecentsFavoritesEvidence> Evidence) HasWorkbenchRecentsFavorites(
            ConsumerSampleHost host)
    {
        host.TrackRecentNodeDefinition(ConsumerSampleHost.ReviewDefinitionId);
        host.FavoriteNodeDefinition(ConsumerSampleHost.QueueDefinitionId);
        host.TrackRecentCommand("viewport.fit");
        host.FavoriteCommand("workspace.save");
        host.TrackRecentPluginSource("consumer.sample.audit-plugin");
        host.FavoritePluginSource("consumer.sample.audit-plugin");

        var evidence = host.RecentsFavoritesEvidence;
        var node = evidence.SingleOrDefault(entry => string.Equals(entry.Surface, "node", StringComparison.Ordinal));
        var fragment = evidence.SingleOrDefault(entry => string.Equals(entry.Surface, "fragment", StringComparison.Ordinal));
        var command = evidence.SingleOrDefault(entry => string.Equals(entry.Surface, "command", StringComparison.Ordinal));
        var plugin = evidence.SingleOrDefault(entry => string.Equals(entry.Surface, "plugin", StringComparison.Ordinal));

        var recentsOk = node is not null
            && fragment is not null
            && command is not null
            && plugin is not null
            && node.RecentIds.Contains(ConsumerSampleHost.ReviewDefinitionId.ToString(), StringComparer.Ordinal)
            && fragment.RecentIds.Contains(ConsumerSampleHost.QueueLaneSnippetId, StringComparer.Ordinal)
            && command.RecentIds.Contains("viewport.fit", StringComparer.Ordinal)
            && plugin.RecentIds.Contains("consumer.sample.audit-plugin", StringComparer.Ordinal)
            && evidence.All(entry => entry.RecentIds.Count <= entry.RecentLimit);
        var favoritesOk = node is not null
            && fragment is not null
            && command is not null
            && plugin is not null
            && node.FavoriteIds.Contains(ConsumerSampleHost.QueueDefinitionId.ToString(), StringComparer.Ordinal)
            && fragment.FavoriteIds.Contains(ConsumerSampleHost.QueueLaneSnippetId, StringComparer.Ordinal)
            && command.FavoriteIds.Contains("workspace.save", StringComparer.Ordinal)
            && plugin.FavoriteIds.Contains("consumer.sample.audit-plugin", StringComparer.Ordinal);
        var supportBundleOk = evidence.Count == 4
            && evidence.All(entry => entry.IsHostOwned)
            && evidence.All(entry => entry.RecentLimit == 8)
            && evidence.All(entry => entry.SourceLabels.Count > 0)
            && evidence.Select(entry => entry.Surface).Distinct(StringComparer.Ordinal).Count() == evidence.Count;

        return (recentsOk, favoritesOk, supportBundleOk, evidence);
    }

    private static (
        bool EvidenceOk,
        bool PrioritizationOk,
        bool ScopeBoundaryOk,
        IReadOnlyList<ConsumerSampleWorkbenchFrictionEvidence> Evidence) HasWorkbenchFrictionEvidence(
            ConsumerSampleHost host)
    {
        var evidence = host.WorkbenchFrictionEvidence;
        var categories = evidence
            .Select(entry => entry.Category)
            .ToArray();

        var evidenceOk = evidence.Count >= 4
            && categories.Contains("layout-resume", StringComparer.Ordinal)
            && categories.Contains("node-discovery", StringComparer.Ordinal)
            && categories.Contains("command-feedback", StringComparer.Ordinal)
            && categories.Contains("support-triage", StringComparer.Ordinal)
            && evidence.All(entry =>
                !string.IsNullOrWhiteSpace(entry.Evidence)
                && !string.IsNullOrWhiteSpace(entry.Route));

        var prioritizationOk = evidence.Any(entry => entry.PriorityRank == 1)
            && evidence.All(entry => entry.PriorityRank is >= 1 and <= 3)
            && evidence
                .OrderBy(entry => entry.PriorityRank)
                .ThenBy(entry => entry.Category, StringComparer.Ordinal)
                .First()
                .PriorityRank == 1;

        var scopeBoundaryOk = evidence.All(entry => entry.IsSynthetic)
            && evidence.All(entry => entry.ScopeBoundary.Contains("local synthetic evidence only", StringComparison.OrdinalIgnoreCase))
            && evidence.Any(entry => entry.ScopeBoundary.Contains("not an external adopter report", StringComparison.OrdinalIgnoreCase))
            && evidence.All(entry => entry.ScopeBoundary.Contains("GA", StringComparison.OrdinalIgnoreCase))
            && evidence.All(entry =>
                entry.Route.Contains("hosted", StringComparison.OrdinalIgnoreCase)
                || entry.Route.Contains("IGraphEditorSession", StringComparison.Ordinal)
                || entry.Route.Contains("ConsumerSample.Avalonia", StringComparison.Ordinal));

        return (evidenceOk, prioritizationOk, scopeBoundaryOk, evidence);
    }

    private static (
        bool PolishOk,
        bool RouteOk,
        bool ScopeBoundaryOk,
        ConsumerSampleWorkbenchAffordancePolish Affordance) HasWorkbenchAffordancePolish(
            ConsumerSampleHost host,
            IReadOnlyList<ConsumerSampleWorkbenchFrictionEvidence> frictionEvidence)
    {
        var affordance = host.LayoutResumeAffordance;
        var sourceEvidence = frictionEvidence.SingleOrDefault(entry =>
            string.Equals(entry.Category, affordance.FrictionCategory, StringComparison.Ordinal));
        var mutated = AsterGraphWorkbenchOptions.ForPreset(AsterGraphWorkbenchLayoutPreset.Debugging) with
        {
            PanelState = AsterGraphWorkbenchPanelState.Default with
            {
                StencilVisible = false,
                MiniMapVisible = false,
                RuntimePanelVisible = true,
                InspectorWidth = 420,
            },
        };
        var reset = host.ApplyLayoutResumeAffordance(mutated);

        var polishOk = sourceEvidence is not null
            && sourceEvidence.PriorityRank == 1
            && string.Equals(affordance.FrictionCategory, "layout-resume", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(affordance.Title)
            && reset == AsterGraphWorkbenchOptions.Default;
        var routeOk = affordance.UsesExistingHostedRoute
            && affordance.Route.Contains("AsterGraphWorkbenchOptions.ResetLayout", StringComparison.Ordinal)
            && reset.LayoutPreset == AsterGraphWorkbenchLayoutPreset.Authoring
            && reset.PanelState == AsterGraphWorkbenchPanelState.Default;
        var scopeBoundaryOk = affordance.ScopeBoundary.Contains("hosted affordance only", StringComparison.OrdinalIgnoreCase)
            && affordance.ScopeBoundary.Contains("no runtime route", StringComparison.OrdinalIgnoreCase)
            && affordance.ScopeBoundary.Contains("WPF parity", StringComparison.Ordinal)
            && affordance.ScopeBoundary.Contains("remote sync", StringComparison.OrdinalIgnoreCase)
            && affordance.ScopeBoundary.Contains("GA claim", StringComparison.OrdinalIgnoreCase);

        return (polishOk, routeOk, scopeBoundaryOk, affordance);
    }

    private static bool HasLocalPluginGallery(ConsumerSampleHost host, Window window)
    {
        var galleryItems = FindNamed<ItemsControl>(window, "PART_LocalPluginGalleryItems");
        var entries = host.LocalPluginGalleryEntries;
        return galleryItems is not null
            && entries.Count > 0
            && entries.Any(entry =>
                entry.PluginId == "consumer.sample.audit-plugin"
                && entry.IsAllowed
                && entry.IsLoaded
                && entry.LoadState.Contains("Loaded", StringComparison.OrdinalIgnoreCase)
                && entry.ManifestLine.Contains("manifest consumer.sample.audit-plugin", StringComparison.Ordinal)
                && entry.GalleryLine.Contains("fingerprint", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasPluginTrustEvidencePanel(ConsumerSampleHost host)
        => host.LocalPluginGalleryEntries.Any(entry =>
            entry.PluginId == "consumer.sample.audit-plugin"
            && entry.TargetFramework.StartsWith("net", StringComparison.OrdinalIgnoreCase)
            && entry.ProvenanceLine.Contains("package", StringComparison.OrdinalIgnoreCase)
            && entry.ProvenanceLine.Contains("signer", StringComparison.OrdinalIgnoreCase)
            && entry.TrustLine.Contains("fingerprint", StringComparison.OrdinalIgnoreCase)
            && entry.TrustLine.Contains("allowlist", StringComparison.OrdinalIgnoreCase));

    private static (bool UnifiedSurfaceOk, bool SourceLabelsOk, bool CommandRouteOk) HasUnifiedDiscoverySurface(
        ConsumerSampleHost host)
    {
        var snippets = host.SnippetCatalog;
        var graphResults = host.SearchGraph("content review", ConsumerSampleGraphSearchScope.All);
        var pluginEntries = host.LocalPluginGalleryEntries;
        var descriptors = host.Session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var commandActions = AsterGraphHostedActionFactory.CreateCommandActions(
            host.Session,
            [ConsumerSampleHost.PluginCommandId, "workspace.save", "viewport.fit"]);
        var authoringActions = AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(host.Session);
        var projection = AsterGraphHostedActionFactory.CreateProjection(
            [
                .. commandActions,
                .. authoringActions,
            ]);
        var templates = host.Session.Queries.GetNodeTemplateSnapshots();
        var pluginTemplate = templates.FirstOrDefault(template =>
            template.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId);
        var builtInTemplate = templates.FirstOrDefault(template =>
            template.DefinitionId == ConsumerSampleHost.ReviewDefinitionId);

        var unifiedSurfaceOk = templates.Any()
            && snippets.Any()
            && graphResults.Any()
            && pluginEntries.Any()
            && descriptors.Values.Any(descriptor => !string.IsNullOrWhiteSpace(descriptor.Group))
            && commandActions.Any()
            && projection.Actions.Any(action => string.Equals(action.Id, ConsumerSampleHost.PluginCommandId, StringComparison.Ordinal))
            && projection.Actions.Any(action => string.Equals(action.Id, "viewport.fit", StringComparison.Ordinal));
        var sourceLabelsOk = pluginTemplate is not null
            && builtInTemplate is not null
            && pluginTemplate.Category.Contains("plugin", StringComparison.OrdinalIgnoreCase)
            && !builtInTemplate.Category.Contains("plugin", StringComparison.OrdinalIgnoreCase)
            && snippets.All(snippet => !string.IsNullOrWhiteSpace(snippet.SourceLabel))
            && graphResults.All(result => !string.IsNullOrWhiteSpace(result.SourceLabel))
            && pluginEntries.All(entry =>
                string.Equals(entry.SourceLabel, "Plugin", StringComparison.Ordinal)
                && entry.GalleryLine.Contains("source Plugin", StringComparison.Ordinal));
        var commandRouteOk = descriptors.ContainsKey(ConsumerSampleHost.PluginCommandId)
            && descriptors.ContainsKey("workspace.save")
            && descriptors.ContainsKey("viewport.fit")
            && commandActions.All(action => descriptors.ContainsKey(action.Id))
            && authoringActions.All(action => action.Id.StartsWith("node-", StringComparison.Ordinal)
                || action.Id.StartsWith("edge-", StringComparison.Ordinal)
                || action.Id.StartsWith("layout.", StringComparison.Ordinal))
            && host.SearchGraph("missing-discovery-query", ConsumerSampleGraphSearchScope.All).Count == 0;

        return (unifiedSurfaceOk, sourceLabelsOk, commandRouteOk);
    }

    private static (bool SamplePackOk, bool NodeDefinitionsOk, bool ParameterMetadataOk) HasSamplePluginPack(ConsumerSampleHost host)
    {
        var definitions = host.Session.Queries.GetRegisteredNodeDefinitions();
        var sampleDefinitions = definitions
            .Where(definition => string.Equals(definition.Category, "AsterGraph Sample Plugins", StringComparison.Ordinal))
            .ToArray();
        var expectedIds = new[]
        {
            ConsumerSampleHost.PluginDataDefinitionId,
            ConsumerSampleHost.PluginAiDefinitionId,
            ConsumerSampleHost.PluginDiagnosticsDefinitionId,
            ConsumerSampleHost.PluginLayoutDefinitionId,
        };

        var nodeDefinitionsOk = expectedIds.All(id => sampleDefinitions.Any(definition => definition.Id == id))
            && sampleDefinitions.Select(definition => definition.Subtitle).Distinct(StringComparer.Ordinal).Count() >= 4;
        var parameterMetadataOk = sampleDefinitions
            .SelectMany(definition => definition.Parameters)
            .Count(parameter =>
                !string.IsNullOrWhiteSpace(parameter.GroupName)
                && !string.IsNullOrWhiteSpace(parameter.HelpText)
                && (parameter.Constraints.AllowedOptions.Count > 0 || parameter.Constraints.Minimum is not null || parameter.Constraints.Maximum is not null || !string.IsNullOrWhiteSpace(parameter.PlaceholderText))) >= 4;
        var samplePackOk = nodeDefinitionsOk
            && parameterMetadataOk
            && host.PluginLoadSnapshots.Any(snapshot =>
                string.Equals(snapshot.Manifest.Id, "consumer.sample.audit-plugin", StringComparison.Ordinal)
                && snapshot.Contributions.NodeDefinitionProviderCount > 0);

        return (samplePackOk, nodeDefinitionsOk, parameterMetadataOk);
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

    private static string CreateReadinessStatus(GraphEditorValidationSnapshot snapshot)
        => snapshot.ErrorCount > 0
            ? "Blocked"
            : snapshot.WarningCount > 0
                ? "Warnings"
                : "Ready";

    private static ConsumerSampleProofValidationSummary CreateValidationSummary(GraphEditorValidationSnapshot snapshot)
        => new(
            TotalIssueCount: snapshot.Issues.Count,
            ErrorCount: snapshot.ErrorCount,
            WarningCount: snapshot.WarningCount,
            InvalidConnectionCount: snapshot.Issues.Count(static issue => IsConnectionIssue(issue)),
            InvalidParameterCount: snapshot.Issues.Count(static issue => IsParameterIssue(issue)));

    private static IReadOnlyList<ConsumerSampleProofValidationFeedback> CreateValidationFeedback(GraphEditorValidationSnapshot snapshot)
        => snapshot.Issues
            .Select(static issue => new ConsumerSampleProofValidationFeedback(
                Code: issue.Code,
                Severity: issue.Severity.ToString(),
                Message: issue.Message,
                FocusTarget: CreateFocusTarget(issue)))
            .ToArray();

    private static IReadOnlyList<ConsumerSampleRepairEvidence> CreateRepairEvidence(
        IGraphEditorSession session,
        GraphEditorValidationSnapshot snapshot)
        => snapshot.Issues
            .SelectMany(issue => session.Queries.GetValidationIssueRepairActions(issue)
                .Select(repair => new ConsumerSampleRepairEvidence(
                    IssueCode: issue.Code,
                    Target: CreateRepairTarget(repair),
                    Action: repair.ActionId,
                    Result: "available")))
            .OrderBy(evidence => evidence.IssueCode, StringComparer.Ordinal)
            .ThenBy(evidence => evidence.Target, StringComparer.Ordinal)
            .ThenBy(evidence => evidence.Action, StringComparer.Ordinal)
            .ToArray();

    private static string CreateRepairTarget(GraphEditorValidationRepairActionSnapshot repair)
        => repair.ConnectionId
            ?? repair.NodeId
            ?? repair.EndpointId
            ?? repair.ParameterKey
            ?? repair.ScopeId;

    private static ConsumerSampleProofFocusTarget CreateFocusTarget(GraphEditorValidationIssueSnapshot issue)
        => new(
            Kind: GetFocusTargetKind(issue),
            NodeId: issue.NodeId,
            ConnectionId: issue.ConnectionId,
            EndpointId: issue.EndpointId,
            ParameterKey: issue.ParameterKey);

    private static string GetFocusTargetKind(GraphEditorValidationIssueSnapshot issue)
    {
        if (issue.ParameterKey is not null)
        {
            return "Parameter";
        }

        if (issue.ConnectionId is not null && issue.EndpointId is not null)
        {
            return "ConnectionEndpoint";
        }

        if (issue.ConnectionId is not null)
        {
            return "Connection";
        }

        return issue.NodeId is not null
            ? "Node"
            : "Graph";
    }

    private static bool IsConnectionIssue(GraphEditorValidationIssueSnapshot issue)
        => issue.ConnectionId is not null
        || issue.Code.StartsWith("connection.", StringComparison.Ordinal);

    private static bool IsParameterIssue(GraphEditorValidationIssueSnapshot issue)
        => issue.ParameterKey is not null
        || issue.Code.StartsWith("node.parameter-", StringComparison.Ordinal);

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

    private static (bool FocusSubgraphOk, bool RouteCleanupOk, bool AlignmentHelpersOk) HasReadabilityHelpers(ConsumerSampleHost host)
    {
        var document = host.Session.Queries.CreateDocumentSnapshot();
        var reviewNodeId = host.GetFirstReviewNodeId();
        var queueNodeId = document.Connections
            .First(connection => string.Equals(connection.Id, "consumer-sample-connection-001", StringComparison.Ordinal))
            .TargetNodeId;

        host.Session.Commands.SetSelection([reviewNodeId, queueNodeId], reviewNodeId, updateStatus: false);
        var focusSubgraphOk = host.FocusSelectedSubgraph()
            && host.FocusedSubgraphNodeIds.Contains(reviewNodeId, StringComparer.Ordinal)
            && host.FocusedSubgraphNodeIds.Contains(queueNodeId, StringComparer.Ordinal)
            && host.DimmedNodeIds.Count > 0;

        var alignmentHelpersOk = host.CaptureAlignmentHelperEvidence()
            && host.AlignmentHelperLines.Any(line => line.StartsWith("layout.align-left:enabled", StringComparison.Ordinal))
            && host.AlignmentHelperLines.Any(line => line.StartsWith("layout.align-center:enabled", StringComparison.Ordinal));

        var connectionId = "consumer-sample-connection-001";
        var inserted = host.Session.Commands.TryInsertConnectionRouteVertex(connectionId, 0, new GraphPoint(360, 260), updateStatus: false);
        host.Session.Commands.SetConnectionSelection([connectionId], primaryConnectionId: connectionId, updateStatus: false);
        var cleaned = host.CleanupSelectedConnectionRoutes();
        var routeCleanupOk = inserted
            && cleaned
            && host.LastRouteCleanupCount > 0
            && host.Session.Queries.GetConnectionGeometrySnapshots()
                .First(connection => string.Equals(connection.ConnectionId, connectionId, StringComparison.Ordinal))
                .Route
                .Vertices
                .Count == 0;

        return (focusSubgraphOk, routeCleanupOk, alignmentHelpersOk);
    }

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

    private static (bool SurfaceOk, bool GroupingOk, bool SearchOk, bool RecentsFavoritesOk, bool SourceFilterOk, bool StatePersistenceOk) HasStencilSurface(Window window, ConsumerSampleHost host)
    {
        var searchBox = FindNamed<TextBox>(window, "PART_StencilSearchBox");
        var sourceFilter = FindNamed<ComboBox>(window, "PART_StencilSourceFilter");
        var emptyStateText = FindNamed<TextBlock>(window, "PART_StencilEmptyStateText");
        if (searchBox is null || sourceFilter is null || emptyStateText is null)
        {
            return (false, false, false, false, false, false);
        }

        var templates = host.Session.Queries.GetNodeTemplateSnapshots();
        var pluginTemplate = templates.SingleOrDefault(template =>
            template.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId);
        var builtInTemplate = templates.FirstOrDefault(template =>
            !template.Category.Contains("plugin", StringComparison.OrdinalIgnoreCase));
        if (pluginTemplate is null)
        {
            return (false, false, false, false, false, false);
        }

        var allSections = window.GetVisualDescendants()
            .OfType<Expander>()
            .Where(section => section.Name?.StartsWith("PART_StencilSection_", StringComparison.Ordinal) == true)
            .ToList();
        var groupingOk = allSections.Count >= 2
            && allSections.All(section => section.IsExpanded)
            && allSections.Any(section => string.Equals(section.Tag?.ToString(), pluginTemplate.Category, StringComparison.Ordinal));

        searchBox.Text = "audit";
        FlushUi();

        var filteredSections = window.GetVisualDescendants()
            .OfType<Expander>()
            .Where(section => section.Name?.StartsWith("PART_StencilSection_", StringComparison.Ordinal) == true)
            .ToList();
        var pluginSection = filteredSections.SingleOrDefault(section =>
            string.Equals(section.Tag?.ToString(), pluginTemplate.Category, StringComparison.Ordinal));
        var pluginCard = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}");
        var searchOk = filteredSections.Count == 1 && pluginSection is not null && pluginCard is not null;
        if (pluginSection is null)
        {
            return (false, groupingOk, searchOk, false, false, false);
        }

        var initialPluginNodeCount = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count(node =>
            node.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId);
        pluginSection.IsExpanded = false;
        FlushUi();
        var collapsedStatePersisted = pluginSection.IsExpanded == false;
        pluginSection.IsExpanded = true;
        FlushUi();

        pluginCard = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}");
        var statePersistenceOk = collapsedStatePersisted && pluginCard is not null && pluginSection.IsExpanded;

        var favoriteButton = FindNamed<Button>(window, $"PART_StencilFavorite_{pluginTemplate.Key}");
        favoriteButton?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        var favoritesOk = FindNamed<Expander>(window, "PART_StencilSection_Favorites") is not null
            && FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}") is not null;

        pluginCard = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}");
        pluginCard?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        var recentsOk = FindNamed<Expander>(window, "PART_StencilSection_Recent") is not null
            && FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}") is not null;

        searchBox.Text = string.Empty;
        FlushUi();
        sourceFilter.SelectedItem = "Plugin";
        FlushUi();
        var pluginFilterOk = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}") is not null
            && (builtInTemplate is null || FindNamed<Button>(window, $"PART_StencilCard_{builtInTemplate.Key}") is null);

        sourceFilter.SelectedItem = "Built-in";
        FlushUi();
        var builtInFilterOk = builtInTemplate is not null
            && FindNamed<Button>(window, $"PART_StencilCard_{builtInTemplate.Key}") is not null
            && FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}") is null;

        sourceFilter.SelectedItem = "All";
        searchBox.Text = "missing-template";
        FlushUi();
        var emptyStateOk = emptyStateText.IsVisible;
        searchBox.Text = string.Empty;
        FlushUi();

        var insertionOk = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count(node =>
            node.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId) == initialPluginNodeCount + 1;
        var recentsFavoritesOk = favoritesOk && recentsOk;
        var sourceFilterOk = pluginFilterOk && builtInFilterOk;
        var surfaceOk = groupingOk
            && searchOk
            && recentsFavoritesOk
            && sourceFilterOk
            && statePersistenceOk
            && emptyStateOk
            && insertionOk;
        return (surfaceOk, groupingOk, searchOk && emptyStateOk, recentsFavoritesOk, sourceFilterOk, statePersistenceOk);
    }

    private static (bool PanelOk, bool ScopeOk, bool ProgressCancelOk) HasExportPanel(Window window, ConsumerSampleHost host)
    {
        var formatPicker = FindNamed<ComboBox>(window, "PART_ExportFormatPicker");
        var scopePicker = FindNamed<ComboBox>(window, "PART_ExportScopePicker");
        var previewText = FindNamed<TextBlock>(window, "PART_ExportPreviewText");
        var progressText = FindNamed<TextBlock>(window, "PART_ExportProgressText");
        var statusText = FindNamed<TextBlock>(window, "PART_ExportStatusText");
        var progressBar = FindNamed<ProgressBar>(window, "PART_ExportProgressBar");
        var exportButton = FindNamed<Button>(window, "PART_ExportRunButton");
        var cancelButton = FindNamed<Button>(window, "PART_ExportCancelButton");

        var formats = formatPicker?.ItemsSource?.OfType<string>().ToArray() ?? [];
        var scopes = scopePicker?.ItemsSource?.OfType<string>().ToArray() ?? [];
        var panelOk = formatPicker is not null
            && scopePicker is not null
            && progressBar is not null
            && exportButton is not null
            && cancelButton is not null
            && formats.SequenceEqual(["SVG", "PNG", "JPEG"], StringComparer.Ordinal)
            && scopes.SequenceEqual(["Full scene", "Selected nodes"], StringComparer.Ordinal);
        if (!panelOk)
        {
            return (false, false, false);
        }

        formatPicker!.SelectedItem = "PNG";
        scopePicker!.SelectedItem = "Selected nodes";
        var checkedExportButton = exportButton!;
        var checkedCancelButton = cancelButton!;
        FlushUi();
        var scopeOk = scopePicker.IsEnabled
            && checkedExportButton.IsEnabled
            && previewText?.Text?.Contains("Selection scope", StringComparison.Ordinal) == true;

        var progressSnapshots = new List<GraphEditorSceneImageExportProgressSnapshot>();
        var progressOk = host.Session.Commands.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat.Png,
            options: new GraphEditorSceneImageExportOptions
            {
                Scope = GraphEditorSceneImageExportScope.FullScene,
                Progress = new ConsumerSampleProofProgressSink(progressSnapshots),
            })
            && progressSnapshots.Count > 0;

        checkedCancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        var cancelRequestedOk = checkedCancelButton.IsEnabled;
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        var cancelOk = !host.Session.Commands.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat.Png,
            options: new GraphEditorSceneImageExportOptions
            {
                Scope = GraphEditorSceneImageExportScope.FullScene,
                CancellationToken = canceled.Token,
            });

        return (panelOk, scopeOk, progressOk && cancelRequestedOk && cancelOk);
    }

    private static bool HasExportDocsAlignment()
    {
        var root = ResolveRepositoryRoot();
        if (root is null)
        {
            return false;
        }

        var docs = new[]
        {
            Path.Combine(root, "README.md"),
            Path.Combine(root, "README.zh-CN.md"),
            Path.Combine(root, "docs", "en", "host-integration.md"),
            Path.Combine(root, "docs", "zh-CN", "host-integration.md"),
            Path.Combine(root, "docs", "en", "feature-catalog.md"),
            Path.Combine(root, "docs", "zh-CN", "feature-catalog.md"),
            Path.Combine(root, "docs", "en", "support-bundle.md"),
            Path.Combine(root, "docs", "zh-CN", "support-bundle.md"),
        };

        return docs.All(File.Exists)
            && docs.All(path =>
            {
                var text = File.ReadAllText(path);
                return text.Contains("SVG", StringComparison.Ordinal)
                    && text.Contains("PNG", StringComparison.Ordinal)
                    && text.Contains("JPEG", StringComparison.Ordinal)
                    && text.Contains("progress", StringComparison.OrdinalIgnoreCase)
                    && text.Contains("cancel", StringComparison.OrdinalIgnoreCase)
                    && text.Contains("scope", StringComparison.OrdinalIgnoreCase);
            });
    }

    private static string? ResolveRepositoryRoot()
    {
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "AsterGraph.sln"))
                    && Directory.Exists(Path.Combine(directory.FullName, "docs")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return null;
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

    private static (bool LocateOk, bool ScopeFilterOk, bool ViewportFocusOk) HasGraphSearchLocate(
        ConsumerSampleHost host,
        Window window)
    {
        var searchBox = FindNamed<TextBox>(window, "PART_GraphSearchBox");
        var searchScope = FindNamed<ComboBox>(window, "PART_GraphSearchScope");
        var searchItems = FindNamed<ItemsControl>(window, "PART_GraphSearchItems");
        var reviewNodeId = host.GetFirstReviewNodeId();
        host.SelectNode(reviewNodeId);

        var titleResults = host.SearchGraph("content review", ConsumerSampleGraphSearchScope.All);
        var parameterResults = host.SearchGraph("release-owner", ConsumerSampleGraphSearchScope.All);
        var portResults = host.SearchGraph("policy", ConsumerSampleGraphSearchScope.All);
        var runtimeResults = host.SearchGraph("approved review payload", ConsumerSampleGraphSearchScope.All);
        var connectionResults = host.SearchGraph("Review To Queue", ConsumerSampleGraphSearchScope.All);
        var selectedResults = host.SearchGraph("content review", ConsumerSampleGraphSearchScope.CurrentSelection);
        var selectedQueueResults = host.SearchGraph("Ship Queue", ConsumerSampleGraphSearchScope.CurrentSelection);
        var scopedResults = host.SearchGraph("Ship Queue", ConsumerSampleGraphSearchScope.CurrentScope);
        var allResults = host.SearchGraph("Ship Queue", ConsumerSampleGraphSearchScope.All);
        var nodeResult = titleResults.FirstOrDefault(result => string.Equals(result.NodeId, reviewNodeId, StringComparison.Ordinal));
        var connectionResult = connectionResults.FirstOrDefault(result =>
            string.Equals(result.ConnectionId, "consumer-sample-connection-001", StringComparison.Ordinal));

        host.Session.Commands.PanBy(96, 64);
        var viewportBefore = host.Session.Queries.GetViewportSnapshot();
        var locatedNode = nodeResult is not null && host.TryLocateGraphSearchResult(nodeResult);
        var viewportAfterNodeLocate = host.Session.Queries.GetViewportSnapshot();
        var locatedConnection = connectionResult is not null && host.TryLocateGraphSearchResult(connectionResult);
        var viewportAfterConnectionLocate = host.Session.Queries.GetViewportSnapshot();
        var selection = host.Session.Queries.GetSelectionSnapshot();
        host.SelectNode(reviewNodeId);

        var locateOk = searchBox is not null
            && searchScope is not null
            && searchItems?.ItemsSource is not null
            && titleResults.Count > 0
            && parameterResults.Any(result => string.Equals(result.NodeId, reviewNodeId, StringComparison.Ordinal))
            && portResults.Any(result => string.Equals(result.NodeId, reviewNodeId, StringComparison.Ordinal))
            && runtimeResults.Any(result => string.Equals(result.NodeId, reviewNodeId, StringComparison.Ordinal))
            && locatedNode
            && locatedConnection
            && string.Equals(selection.PrimarySelectedConnectionId, "consumer-sample-connection-001", StringComparison.Ordinal);
        var scopeFilterOk = selectedResults.Count > 0
            && selectedQueueResults.Count == 0
            && scopedResults.Count == allResults.Count
            && HasGraphSearchCompositeScopeFilter()
            && allResults.Count > selectedQueueResults.Count;
        var viewportFocusOk = viewportBefore != viewportAfterNodeLocate
            && viewportAfterNodeLocate != viewportAfterConnectionLocate;

        return (locateOk, scopeFilterOk, viewportFocusOk);
    }

    private static bool HasGraphSearchCompositeScopeFilter()
    {
        var storageRoot = Path.Combine(
            Path.GetTempPath(),
            "AsterGraph.ConsumerSample.GraphSearchProof",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        using var host = ConsumerSampleHost.Create(storageRoot);
        var document = host.Session.Queries.CreateDocumentSnapshot();
        var reviewNodeId = host.GetFirstReviewNodeId();
        var queueNodeId = document.Nodes
            .First(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .Id;
        host.Session.Commands.SetSelection([reviewNodeId, queueNodeId], queueNodeId, updateStatus: false);

        var compositeNodeId = host.Session.Commands.TryWrapSelectionToComposite("Search Scope Proof", updateStatus: false);
        if (string.IsNullOrWhiteSpace(compositeNodeId))
        {
            return false;
        }

        var rootScopeResults = host.SearchGraph("Ship Queue", ConsumerSampleGraphSearchScope.CurrentScope);
        var allScopeResults = host.SearchGraph("Ship Queue", ConsumerSampleGraphSearchScope.All);
        var entered = host.Session.Commands.TryEnterCompositeChildGraph(compositeNodeId, updateStatus: false);
        var childScopeId = host.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId;
        var childScopeResults = host.SearchGraph("Ship Queue", ConsumerSampleGraphSearchScope.CurrentScope);
        var childResult = childScopeResults.FirstOrDefault(result =>
            string.Equals(result.NodeId, queueNodeId, StringComparison.Ordinal));

        return entered
            && rootScopeResults.Count == 0
            && allScopeResults.Any(result => string.Equals(result.NodeId, queueNodeId, StringComparison.Ordinal))
            && childResult is not null
            && string.Equals(childResult.ScopeId, childScopeId, StringComparison.Ordinal)
            && host.TryLocateGraphSearchResult(childResult)
            && string.Equals(host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId, queueNodeId, StringComparison.Ordinal);
    }

    private static (bool NavigationHistoryOk, bool ScopeBreadcrumbNavigationOk, bool FocusRestoreOk) HasNavigationHistoryAndScopeFocus(
        ConsumerSampleHost host,
        Window window)
    {
        var navigationSummary = FindNamed<TextBlock>(window, "PART_NavigationHistorySummaryText");
        var backButton = FindNamed<Button>(window, "PART_NavigationBackButton");
        var forwardButton = FindNamed<Button>(window, "PART_NavigationForwardButton");
        var focusScopeButton = FindNamed<Button>(window, "PART_FocusCurrentScopeButton");
        var restoreViewportButton = FindNamed<Button>(window, "PART_RestoreViewportButton");
        var breadcrumbItems = FindNamed<ItemsControl>(window, "PART_ScopeBreadcrumbItems");
        var reviewNodeId = host.GetFirstReviewNodeId();
        var queueNodeId = host.Session.Queries.CreateDocumentSnapshot().Nodes
            .First(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .Id;

        host.Session.Commands.UpdateViewportSize(960, 640);
        host.SelectNode(reviewNodeId);
        FlushUi();

        var queueResult = host.SearchGraph("Ship Queue", ConsumerSampleGraphSearchScope.All)
            .FirstOrDefault(result => string.Equals(result.NodeId, queueNodeId, StringComparison.Ordinal));
        var locatedQueue = queueResult is not null && host.TryLocateGraphSearchResult(queueResult);
        FlushUi();
        var historyAfterLocate = host.NavigationHistory.Count;
        var canBackAfterLocate = host.CanNavigateBack && backButton is { IsEnabled: true };
        var backNavigated = host.TryNavigateBack();
        FlushUi();
        var backSelection = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;
        var canForwardAfterBack = host.CanNavigateForward;
        var forwardNavigated = host.TryNavigateForward();
        FlushUi();
        var forwardSelection = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;

        host.Session.Commands.PanBy(77, 43);
        var viewportBeforeFocus = host.Session.Queries.GetViewportSnapshot();
        var focusChangedViewport = focusScopeButton is not null && host.FocusCurrentScopeForReview();
        FlushUi();
        var viewportAfterFocus = host.Session.Queries.GetViewportSnapshot();
        var canRestore = host.CanRestoreViewport;
        var restored = host.RestorePreviousViewport();
        FlushUi();
        var restoredViewport = host.Session.Queries.GetViewportSnapshot();

        var scopeBreadcrumbNavigationOk = HasScopeBreadcrumbNavigation();
        var navigationHistoryOk = navigationSummary is not null
            && locatedQueue
            && historyAfterLocate >= 2
            && canBackAfterLocate
            && backNavigated
            && string.Equals(backSelection, reviewNodeId, StringComparison.Ordinal)
            && canForwardAfterBack
            && forwardNavigated
            && string.Equals(forwardSelection, queueNodeId, StringComparison.Ordinal);
        var focusRestoreOk = restoreViewportButton is not null
            && focusChangedViewport
            && canRestore
            && restored
            && viewportBeforeFocus != viewportAfterFocus
            && ViewportEquals(restoredViewport, viewportBeforeFocus);

        return (
            navigationHistoryOk,
            scopeBreadcrumbNavigationOk && breadcrumbItems is not null && forwardButton is not null,
            focusRestoreOk);
    }

    private static bool HasScopeBreadcrumbNavigation()
    {
        var storageRoot = Path.Combine(
            Path.GetTempPath(),
            "AsterGraph.ConsumerSample.NavigationProof",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        using var host = ConsumerSampleHost.Create(storageRoot);
        var document = host.Session.Queries.CreateDocumentSnapshot();
        var reviewNodeId = host.GetFirstReviewNodeId();
        var queueNodeId = document.Nodes
            .First(node => node.DefinitionId == ConsumerSampleHost.QueueDefinitionId)
            .Id;
        host.Session.Commands.SetSelection([reviewNodeId, queueNodeId], queueNodeId, updateStatus: false);

        var compositeNodeId = host.Session.Commands.TryWrapSelectionToComposite("Navigation Scope Proof", updateStatus: false);
        if (string.IsNullOrWhiteSpace(compositeNodeId))
        {
            return false;
        }

        var rootScopeId = host.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId;
        if (!host.Session.Commands.TryEnterCompositeChildGraph(compositeNodeId, updateStatus: false))
        {
            return false;
        }

        var childNavigation = host.Session.Queries.GetScopeNavigationSnapshot();
        var childBreadcrumbs = host.ScopeBreadcrumbs;
        var rootBreadcrumb = childBreadcrumbs.FirstOrDefault(entry =>
            string.Equals(entry.ScopeId, rootScopeId, StringComparison.Ordinal));
        var childBreadcrumb = childBreadcrumbs.FirstOrDefault(entry =>
            string.Equals(entry.ScopeId, childNavigation.CurrentScopeId, StringComparison.Ordinal));

        return childNavigation.CanNavigateToParent
            && childBreadcrumbs.Count >= 2
            && rootBreadcrumb is { IsCurrent: false }
            && childBreadcrumb is { IsCurrent: true }
            && host.TryNavigateToScopeBreadcrumb(rootScopeId)
            && string.Equals(host.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId, rootScopeId, StringComparison.Ordinal)
            && host.CanNavigateBack
            && host.TryNavigateBack()
            && string.Equals(host.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId, childNavigation.CurrentScopeId, StringComparison.Ordinal);
    }

    private static bool ViewportEquals(GraphEditorViewportSnapshot left, GraphEditorViewportSnapshot right)
        => Math.Abs(left.Zoom - right.Zoom) < 0.0001d
        && Math.Abs(left.PanX - right.PanX) < 0.0001d
        && Math.Abs(left.PanY - right.PanY) < 0.0001d
        && Math.Abs(left.ViewportWidth - right.ViewportWidth) < 0.0001d
        && Math.Abs(left.ViewportHeight - right.ViewportHeight) < 0.0001d;

    private static (bool GroupingOk, bool DisabledReasonOk, bool RecentActionsOk) HasCommandPaletteProductivity(Window window)
    {
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        if (paletteToggle is null)
        {
            return (false, false, false);
        }

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        var paletteItems = FindNamed<StackPanel>(window, "PART_CommandPaletteItems");
        var workspaceGroup = FindNamed<TextBlock>(window, "PART_CommandPaletteGroup_workspace");
        var viewportGroup = FindNamed<TextBlock>(window, "PART_CommandPaletteGroup_viewport");
        var saveButton = FindNamed<Button>(window, "PART_CommandPaletteAction_workspace.save");
        var distributeButton = FindNamed<Button>(window, "PART_CommandPaletteAction_layout.distribute-horizontal");
        var expansionButton = FindNamed<Button>(window, "PART_CommandPaletteAction_node-toggle-surface-expansion");

        var groupingOk = paletteItems is not null
            && workspaceGroup?.Text == "workspace"
            && viewportGroup?.Text == "viewport"
            && saveButton is not null
            && expansionButton is not null;
        var disabledReasonOk = distributeButton is { IsEnabled: false }
            && string.Equals(
                ToolTip.GetTip(distributeButton)?.ToString(),
                "Select at least three nodes before distributing.",
                StringComparison.Ordinal);

        expansionButton?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        var paletteChrome = FindNamed<Border>(window, "PART_CommandPaletteChrome");
        if (paletteChrome?.IsVisible == true)
        {
            paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            FlushUi();
        }

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        var recentHeading = FindNamed<TextBlock>(window, "PART_CommandPaletteRecentActionsHeading");
        var recentExpansionButton = FindNamed<Button>(window, "PART_CommandPaletteRecentAction_node-toggle-surface-expansion");
        var recentActionsOk = recentHeading?.Text == "Recent Actions"
            && recentExpansionButton is not null
            && (AutomationProperties.GetName(recentExpansionButton)?.Contains("Node Card", StringComparison.Ordinal) == true);

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        return (groupingOk, disabledReasonOk, recentActionsOk);
    }

    private static (bool UnifiedOk, bool PaletteOk, bool ToolbarOk, bool ContextMenuOk, bool DisabledReasonOk) HasUnifiedCommandProjection(
        Window window,
        ConsumerSampleHost host)
    {
        var descriptors = host.Session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!descriptors.TryGetValue("workspace.save", out var saveDescriptor)
            || !descriptors.TryGetValue("layout.distribute-horizontal", out var distributeDescriptor))
        {
            return (false, false, false, false, false);
        }

        var saveToolbarButton = FindNamed<Button>(window, "PART_HeaderCommand_workspace.save");
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        if (saveToolbarButton is null || paletteToggle is null)
        {
            return (false, false, false, false, false);
        }

        var toolbarOk = string.Equals(saveToolbarButton.Content?.ToString(), saveDescriptor.Title, StringComparison.Ordinal)
            && string.Equals(AutomationProperties.GetName(saveToolbarButton), saveDescriptor.Title, StringComparison.Ordinal)
            && saveToolbarButton.IsEnabled == saveDescriptor.IsEnabled
            && string.Equals(ToolTip.GetTip(saveToolbarButton)?.ToString(), saveDescriptor.DefaultShortcut, StringComparison.Ordinal);

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        var savePaletteButton = FindNamed<Button>(window, "PART_CommandPaletteAction_workspace.save");
        var distributePaletteButton = FindNamed<Button>(window, "PART_CommandPaletteAction_layout.distribute-horizontal");
        var paletteOk = savePaletteButton is not null
            && savePaletteButton.Content?.ToString()?.Contains(saveDescriptor.Title, StringComparison.Ordinal) == true
            && savePaletteButton.Content?.ToString()?.Contains(saveDescriptor.DefaultShortcut ?? string.Empty, StringComparison.Ordinal) == true
            && string.Equals(AutomationProperties.GetName(savePaletteButton), saveDescriptor.Title, StringComparison.Ordinal)
            && savePaletteButton.IsEnabled == saveDescriptor.IsEnabled;
        var paletteDisabledReasonOk = distributePaletteButton is { IsEnabled: false }
            && string.Equals(ToolTip.GetTip(distributePaletteButton)?.ToString(), distributeDescriptor.DisabledReason, StringComparison.Ordinal);
        var paletteChrome = FindNamed<Border>(window, "PART_CommandPaletteChrome");
        if (paletteChrome?.IsVisible == true)
        {
            paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            FlushUi();
        }

        var reviewNodeId = host.GetFirstReviewNodeId();
        var canvasMenuItems = host.Session.Queries.BuildContextMenuDescriptors(
            new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90)));
        var selectionMenuItems = host.Session.Queries.BuildContextMenuDescriptors(
            new ContextMenuContext(
                ContextMenuTargetKind.Selection,
                new GraphPoint(160, 90),
                selectedNodeId: reviewNodeId,
                selectedNodeIds: [reviewNodeId]));
        var canvasSaveItem = FindMenuItem(canvasMenuItems, "canvas-save");
        var distributeItem = FindMenuItem(selectionMenuItems, "selection-distribute-horizontal");
        var contextMenuOk = canvasSaveItem is not null
            && string.Equals(canvasSaveItem.Command?.CommandId, saveDescriptor.Id, StringComparison.Ordinal)
            && canvasSaveItem.IsEnabled == saveDescriptor.IsEnabled
            && distributeItem is not null
            && string.Equals(distributeItem.Command?.CommandId, distributeDescriptor.Id, StringComparison.Ordinal);
        var contextDisabledReasonOk = distributeItem is { IsEnabled: false }
            && string.Equals(distributeItem.DisabledReason, distributeDescriptor.DisabledReason, StringComparison.Ordinal);

        var disabledReasonOk = !string.IsNullOrWhiteSpace(distributeDescriptor.DisabledReason)
            && paletteDisabledReasonOk
            && contextDisabledReasonOk;
        var unifiedOk = toolbarOk
            && paletteOk
            && contextMenuOk
            && disabledReasonOk;
        return (unifiedOk, paletteOk, toolbarOk, contextMenuOk, disabledReasonOk);
    }

    private static (bool NodeOk, bool EdgeOk, bool DescriptorOk, bool ScopeBoundaryOk) HasToolbarContributions(
        ConsumerSampleHost host,
        string reviewNodeId)
    {
        const string connectionId = "consumer-sample-connection-001";
        host.Session.Commands.TrySetConnectionNoteText(connectionId, "Toolbar contribution proof", updateStatus: false);

        var selection = host.Session.Queries.GetSelectionSnapshot();
        var nodeContext = GraphEditorToolContextSnapshot.ForNode(
            reviewNodeId,
            selection.SelectedNodeIds,
            selection.PrimarySelectedNodeId);
        var edgeContext = GraphEditorToolContextSnapshot.ForConnection(
            connectionId,
            selection.SelectedNodeIds,
            selection.PrimarySelectedNodeId);

        var nodeTools = host.Session.Queries.GetToolDescriptors(nodeContext);
        var edgeTools = host.Session.Queries.GetToolDescriptors(edgeContext);
        var nodeActions = AsterGraphAuthoringToolActionFactory.CreateNodeActions(
            host.Session,
            reviewNodeId,
            selection.SelectedNodeIds,
            selection.PrimarySelectedNodeId);
        var edgeActions = AsterGraphAuthoringToolActionFactory.CreateConnectionActions(
            host.Session,
            connectionId,
            selection.SelectedNodeIds,
            selection.PrimarySelectedNodeId);

        var nodeOk = HasContributionAction(nodeTools, nodeActions, "node-inspect", "nodes.inspect", "nodeId", reviewNodeId)
            && HasContributionAction(nodeTools, nodeActions, "node-duplicate", "nodes.duplicate", "nodeId", reviewNodeId)
            && HasContributionAction(nodeTools, nodeActions, "node-toggle-surface-expansion", "nodes.surface.expand", "nodeId", reviewNodeId);
        var edgeOk = HasContributionAction(edgeTools, edgeActions, "connection-disconnect", "connections.disconnect", "connectionId", connectionId)
            && HasContributionAction(edgeTools, edgeActions, "connection-clear-note", "connections.note.set", "connectionId", connectionId);
        var descriptorOk = AllActionsMatchTools(nodeTools, nodeActions)
            && AllActionsMatchTools(edgeTools, edgeActions);
        var scopeBoundaryOk = nodeTools.All(tool => tool.ContextKind == GraphEditorToolContextKind.Node)
            && edgeTools.All(tool => tool.ContextKind == GraphEditorToolContextKind.Connection)
            && nodeTools.All(tool => HasInvocationArgument(tool, "nodeId", reviewNodeId) || tool.Id == "node-enter-composite-scope")
            && edgeTools.All(tool => HasInvocationArgument(tool, "connectionId", connectionId))
            && host.Session.Queries.GetToolDescriptors(GraphEditorToolContextSnapshot.ForNode("missing-node", [], null)).Count == 0
            && host.Session.Queries.GetToolDescriptors(GraphEditorToolContextSnapshot.ForConnection("missing-connection", [], null)).Count == 0;

        return (nodeOk, edgeOk, descriptorOk, scopeBoundaryOk);
    }

    private static bool HasContributionAction(
        IReadOnlyList<GraphEditorToolDescriptorSnapshot> tools,
        IReadOnlyList<AsterGraphHostedActionDescriptor> actions,
        string toolId,
        string commandId,
        string argumentName,
        string argumentValue)
        => tools.Any(tool =>
            string.Equals(tool.Id, toolId, StringComparison.Ordinal)
            && tool.CanExecute
            && string.Equals(tool.Invocation.CommandId, commandId, StringComparison.Ordinal)
            && HasInvocationArgument(tool, argumentName, argumentValue))
        && actions.Any(action =>
            string.Equals(action.Id, toolId, StringComparison.Ordinal)
            && action.CanExecute
            && string.Equals(action.CommandId, commandId, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(action.Title));

    private static bool AllActionsMatchTools(
        IReadOnlyList<GraphEditorToolDescriptorSnapshot> tools,
        IReadOnlyList<AsterGraphHostedActionDescriptor> actions)
    {
        var actionMap = actions.ToDictionary(action => action.Id, StringComparer.Ordinal);
        return tools.Count == actions.Count
            && tools.All(tool =>
                actionMap.TryGetValue(tool.Id, out var action)
                && string.Equals(action.Title, tool.Title, StringComparison.Ordinal)
                && string.Equals(action.Group, tool.Group, StringComparison.Ordinal)
                && string.Equals(action.IconKey, tool.IconKey, StringComparison.Ordinal)
                && action.CanExecute == tool.CanExecute
                && action.CommandSource == tool.Source
                && string.Equals(action.CommandId, tool.Invocation.CommandId, StringComparison.Ordinal));
    }

    private static bool HasInvocationArgument(
        GraphEditorToolDescriptorSnapshot tool,
        string argumentName,
        string argumentValue)
        => tool.Invocation.Arguments.Any(argument =>
            string.Equals(argument.Name, argumentName, StringComparison.Ordinal)
            && string.Equals(argument.Value, argumentValue, StringComparison.Ordinal));

    private static GraphEditorMenuItemDescriptorSnapshot? FindMenuItem(
        IEnumerable<GraphEditorMenuItemDescriptorSnapshot> items,
        string id)
    {
        foreach (var item in items)
        {
            if (string.Equals(item.Id, id, StringComparison.Ordinal))
            {
                return item;
            }

            var child = FindMenuItem(item.Children, id);
            if (child is not null)
            {
                return child;
            }
        }

        return null;
    }

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

    private static bool HasRuntimeOverlaySnapshotPolish(ConsumerSampleHost host)
    {
        var overlay = host.Session.Queries.GetRuntimeOverlaySnapshot();
        return overlay.IsAvailable
            && overlay.NodeOverlays.Any(snapshot =>
                snapshot.Status == GraphEditorRuntimeOverlayStatus.Success
                && snapshot.ElapsedMilliseconds > 0
                && !string.IsNullOrWhiteSpace(snapshot.OutputPreview)
                && snapshot.LastRunAtUtc is not null)
            && overlay.NodeOverlays.Any(snapshot =>
                snapshot.WarningCount > 0
                && !string.IsNullOrWhiteSpace(snapshot.OutputPreview)
                && snapshot.LastRunAtUtc is not null)
            && overlay.NodeOverlays.Any(snapshot =>
                snapshot.Status == GraphEditorRuntimeOverlayStatus.Error
                && snapshot.ErrorCount > 0
                && !string.IsNullOrWhiteSpace(snapshot.ErrorMessage)
                && snapshot.LastRunAtUtc is not null)
            && overlay.ConnectionOverlays.Any(snapshot =>
                snapshot.Status == GraphEditorRuntimeOverlayStatus.Success
                && !string.IsNullOrWhiteSpace(snapshot.ValuePreview)
                && !string.IsNullOrWhiteSpace(snapshot.PayloadType)
                && snapshot.ItemCount > 0
                && !snapshot.IsStale)
            && overlay.ConnectionOverlays.Any(snapshot =>
                snapshot.Status == GraphEditorRuntimeOverlayStatus.Error
                && !string.IsNullOrWhiteSpace(snapshot.PayloadType)
                && snapshot.IsStale);
    }

    private static bool HasRuntimeOverlayScopeFilter(ConsumerSampleHost host)
    {
        var overlay = host.Session.Queries.GetRuntimeOverlaySnapshot();
        var selectedNodeId = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;
        var selectedNodeOverlays = selectedNodeId is null
            ? Array.Empty<GraphEditorNodeRuntimeOverlaySnapshot>()
            : overlay.NodeOverlays
                .Where(snapshot => string.Equals(snapshot.NodeId, selectedNodeId, StringComparison.Ordinal))
                .ToArray();
        var selectedLogs = selectedNodeId is null
            ? Array.Empty<GraphEditorRuntimeLogEntrySnapshot>()
            : overlay.RecentLogs
                .Where(snapshot => string.Equals(snapshot.NodeId, selectedNodeId, StringComparison.Ordinal))
                .ToArray();
        var currentScopeLogs = overlay.RecentLogs
            .Where(snapshot => string.Equals(snapshot.ScopeId, "root", StringComparison.Ordinal))
            .ToArray();

        return overlay.IsAvailable
            && selectedNodeId is not null
            && overlay.NodeOverlays.Count > selectedNodeOverlays.Length
            && selectedNodeOverlays.Length > 0
            && selectedLogs.Length > 0
            && currentScopeLogs.Length == overlay.RecentLogs.Count
            && overlay.ConnectionOverlays.Count > 0;
    }

    private static (bool HandleIdOk, bool GroupOk, bool HintOk, bool ScopeBoundaryOk) HasPortHandleAuthoring(ConsumerSampleHost host)
    {
        const string inputPortId = "input";
        const string outputPortId = "output";
        const string reviewPolicyPortId = "policy";
        const string reviewAuditPortId = "audit";

        var document = host.Session.Queries.CreateDocumentSnapshot();
        var reviewNodeId = host.GetFirstReviewNodeId();
        var reviewNode = document.Nodes.Single(node => string.Equals(node.Id, reviewNodeId, StringComparison.Ordinal));
        var queueNode = document.Nodes.Single(node => node.DefinitionId == new NodeDefinitionId("consumer.sample.queue"));
        var target = host.Session.Queries.GetCompatiblePortTargets(reviewNode.Id, outputPortId)
            .SingleOrDefault(snapshot =>
                string.Equals(snapshot.NodeId, queueNode.Id, StringComparison.Ordinal)
                && string.Equals(snapshot.PortId, inputPortId, StringComparison.Ordinal));

        var builderPort = PortDefinitionBuilder
            .Create("payload", "Payload", "json")
            .Group("Data")
            .Connections(max: 2)
            .Build();

        var allPorts = reviewNode.Inputs
            .Concat(reviewNode.Outputs)
            .Concat(queueNode.Inputs)
            .Concat(queueNode.Outputs)
            .ToArray();
        var handleIdOk = builderPort.HandleId == builderPort.Key
            && allPorts.All(port => string.Equals(port.HandleId, port.Id, StringComparison.Ordinal))
            && target is not null
            && string.Equals(target.PortHandleId, target.PortId, StringComparison.Ordinal);
        var groupOk = builderPort.GroupName == "Data"
            && reviewNode.Inputs.Any(port => port.Id == inputPortId && port.GroupName == "Flow" && port.MaxConnections == 1)
            && reviewNode.Inputs.Any(port => port.Id == reviewPolicyPortId && port.GroupName == "Policy")
            && reviewNode.Outputs.Any(port => port.Id == outputPortId && port.GroupName == "Flow")
            && reviewNode.Outputs.Any(port => port.Id == reviewAuditPortId && port.GroupName == "Audit")
            && target?.PortGroupName == "Flow";
        var hintOk = builderPort.ConnectionHint.Contains("Data", StringComparison.Ordinal)
            && reviewNode.Outputs.Single(port => port.Id == outputPortId).ConnectionHint.Contains("Flow", StringComparison.Ordinal)
            && target is not null
            && target.ConnectionHint.Contains("Input", StringComparison.Ordinal)
            && target.ConnectionHint.Contains("Flow", StringComparison.Ordinal)
            && target.ConnectionHint.Contains("flow", StringComparison.Ordinal);
        var scopeBoundaryOk = typeof(PortDefinition).Namespace == "AsterGraph.Abstractions.Definitions"
            && typeof(GraphPort).Namespace == "AsterGraph.Core.Models"
            && typeof(GraphEditorCompatiblePortTargetSnapshot).Namespace == "AsterGraph.Editor.Runtime"
            && Type.GetType("AsterGraph.Abstractions.Definitions.HandleDefinition, AsterGraph.Abstractions") is null
            && Type.GetType("AsterGraph.Core.Models.GraphHandle, AsterGraph.Core") is null;

        return (handleIdOk, groupOk, hintOk, scopeBoundaryOk);
    }

    private static (
        bool ReasonOk,
        bool InvalidHoverOk,
        bool SupportBundleOk,
        bool ScopeBoundaryOk,
        bool HelpTargetOk,
        IReadOnlyList<ConsumerSampleRepairEvidence> RepairEvidence) HasConnectionValidationFeedback()
    {
        const string sourceNodeId = "consumer-sample.validation.source";
        const string targetNodeId = "consumer-sample.validation.target";
        const string sourcePortId = "out";
        const string targetPortId = "in";
        var definitionId = new NodeDefinitionId("consumer.sample.connection-validation");
        var catalog = new AsterGraph.Editor.Catalog.NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Connection Validation Node",
            "Consumer Sample",
            "Validation",
            [new PortDefinition("in", "Text", new PortTypeId("string"), "#F3B36B", description: "String input expected by review validation.")],
            [new PortDefinition("out", "Float", new PortTypeId("float"), "#6AD5C4", description: "Float output emitted by validation proof.")],
            []));
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Connection Validation Proof",
                "Connection validation feedback proof.",
                [
                    new GraphNode(
                        sourceNodeId,
                        "Float Source",
                        "Consumer Sample",
                        "Validation",
                        "Source node with a float output.",
                        new GraphPoint(0, 0),
                        new GraphSize(220, 140),
                        [],
                        [new GraphPort(sourcePortId, "Float", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        targetNodeId,
                        "String Target",
                        "Consumer Sample",
                        "Validation",
                        "Target node with a string input.",
                        new GraphPoint(320, 0),
                        new GraphSize(220, 140),
                        [new GraphPort(targetPortId, "Text", PortDirection.Input, "string", "#F3B36B", new PortTypeId("string"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                [
                    new GraphConnection(
                        "consumer-sample.validation.connection",
                        sourceNodeId,
                        sourcePortId,
                        targetNodeId,
                        targetPortId,
                        "Invalid Float To Text",
                        "#6AD5C4"),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var validation = session.Queries.GetValidationSnapshot();
        session.Commands.StartConnection(sourceNodeId, sourcePortId);
        session.Commands.CompleteConnection(targetNodeId, targetPortId);
        var pending = session.Queries.GetPendingConnectionSnapshot();
        var inspectionPending = session.Diagnostics.CaptureInspectionSnapshot().PendingConnection;
        var issue = validation.Issues.SingleOrDefault(snapshot =>
            string.Equals(snapshot.Code, "connection.incompatible-endpoint-types", StringComparison.Ordinal));
        var summary = CreateValidationSummary(validation);
        var feedback = CreateValidationFeedback(validation);
        var helpTargetOk = issue?.HelpTarget is not null
            && string.Equals(issue.HelpTarget.Kind, "Connection", StringComparison.Ordinal)
            && string.Equals(issue.HelpTarget.ConnectionId, "consumer-sample.validation.connection", StringComparison.Ordinal)
            && issue.HelpTarget.HelpText.Contains("float", StringComparison.Ordinal)
            && issue.HelpTarget.HelpText.Contains("string", StringComparison.Ordinal);

        var reasonOk = issue is not null
            && issue.Message.Contains("float -> string", StringComparison.Ordinal)
            && string.Equals(pending.ValidationMessage, "Incompatible connection: float -> string.", StringComparison.Ordinal);
        var invalidHoverOk = pending.HasPendingConnection
            && string.Equals(pending.SourceNodeId, sourceNodeId, StringComparison.Ordinal)
            && string.Equals(pending.SourcePortId, sourcePortId, StringComparison.Ordinal)
            && string.Equals(pending.TargetNodeId, targetNodeId, StringComparison.Ordinal)
            && string.Equals(pending.TargetPortId, targetPortId, StringComparison.Ordinal)
            && pending.TargetKind == GraphConnectionTargetKind.Port
            && pending.IsTargetCompatible == false
            && string.Equals(inspectionPending.ValidationMessage, pending.ValidationMessage, StringComparison.Ordinal);
        var supportBundleOk = summary.TotalIssueCount == feedback.Count
            && summary.InvalidConnectionCount == 1
            && feedback.Any(row =>
                string.Equals(row.Code, "connection.incompatible-endpoint-types", StringComparison.Ordinal)
                && row.FocusTarget.Kind == "ConnectionEndpoint"
                && string.Equals(row.FocusTarget.ConnectionId, "consumer-sample.validation.connection", StringComparison.Ordinal)
                && string.Equals(row.FocusTarget.EndpointId, targetPortId, StringComparison.Ordinal));
        var scopeBoundaryOk = typeof(GraphEditorPendingConnectionSnapshot).Namespace == "AsterGraph.Editor.Diagnostics"
            && typeof(GraphEditorValidationIssueSnapshot).Namespace == "AsterGraph.Editor.Runtime"
            && Type.GetType("AsterGraph.Editor.Runtime.IGraphExecutionEngine, AsterGraph.Editor") is null;

        return (reasonOk, invalidHoverOk, supportBundleOk, scopeBoundaryOk, helpTargetOk, CreateRepairEvidence(session, validation));
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
            && selectedLogs.Length > 0
            && selectedLogs.Length < allLogs.Count
            && scopeLogs.Length == allLogs.Count;
    }

    private static bool HasRuntimeDebugPanelInteraction(Window window, ConsumerSampleHost host)
    {
        var overlay = host.Session.Queries.GetRuntimeOverlaySnapshot();
        var logItems = FindNamed<ItemsControl>(window, "PART_RuntimeLogItems");
        var logFilter = FindNamed<ComboBox>(window, "PART_RuntimeLogFilter");
        var exportButton = FindNamed<Button>(window, "PART_ExportRuntimeLogsButton");
        var summary = FindNamed<TextBlock>(window, "PART_RuntimeSummaryText")?.Text ?? string.Empty;
        var logButtons = logItems?.ItemsSource?.OfType<Button>().ToArray() ?? [];

        return overlay.IsAvailable
            && summary.Contains($"Nodes: {overlay.NodeOverlays.Count}", StringComparison.Ordinal)
            && summary.Contains($"Connections: {overlay.ConnectionOverlays.Count}", StringComparison.Ordinal)
            && summary.Contains($"Logs: {overlay.RecentLogs.Count}", StringComparison.Ordinal)
            && logFilter?.ItemsSource?.OfType<string>().SequenceEqual(new[] { "All", "Selected Node", "Current Scope" }) == true
            && logButtons.Length == overlay.RecentLogs.Count
            && logButtons.Any(button => button.Content?.ToString()?.Contains("node=consumer-sample-queue-001", StringComparison.Ordinal) == true)
            && exportButton is not null;
    }

    private static bool HasRuntimeLogLocate(ConsumerSampleHost host)
    {
        var selectedBefore = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;
        var log = host.Session.Queries.GetRuntimeOverlaySnapshot().RecentLogs.SingleOrDefault(snapshot =>
            string.Equals(snapshot.Id, "consumer-sample-runtime-log-001", StringComparison.Ordinal));
        var navigated = log is not null && host.TryNavigateToRuntimeLog(log);
        var selectedAfter = host.Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId;
        if (selectedBefore is not null)
        {
            host.SelectNode(selectedBefore);
        }

        return navigated
            && string.Equals(selectedAfter, "consumer-sample-queue-001", StringComparison.Ordinal);
    }

    private static bool HasRuntimeLogExport(ConsumerSampleHost host)
    {
        var exported = host.ExportRuntimeLogs();
        var lines = host.LastRuntimeLogExportLines;
        var logs = host.Session.Queries.GetRuntimeOverlaySnapshot().RecentLogs;

        return exported
            && lines.Count == logs.Count
            && lines.Any(line => line.Contains("consumer-sample-runtime-log-001", StringComparison.Ordinal)
                && line.Contains("node=consumer-sample-queue-001", StringComparison.Ordinal)
                && line.Contains("connection=consumer-sample-connection-001", StringComparison.Ordinal))
            && lines.Any(line => line.Contains("Error", StringComparison.Ordinal)
                && line.Contains("connection=consumer-sample-connection-001:stale", StringComparison.Ordinal));
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

    private static (
        IReadOnlyList<GraphEditorNodeParameterSnapshot> ParameterSnapshots,
        IReadOnlyList<ConsumerSampleRepairEvidence> RepairEvidence,
        bool HelpTargetOk,
        bool InspectorHelpTargetOk) CreateValidationFixProof()
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
        var validation = session.Queries.GetValidationSnapshot();
        var issue = validation.Issues.SingleOrDefault(snapshot =>
            string.Equals(snapshot.Code, "node.parameter-invalid", StringComparison.Ordinal));
        var parameterSnapshots = session.Queries.GetSelectedNodeParameterSnapshots()
            .Where(snapshot => snapshot.CanApplyValidationFix)
            .ToArray();
        var parameterSnapshot = parameterSnapshots.SingleOrDefault();
        var helpTargetOk = issue?.HelpTarget is not null
            && string.Equals(issue.HelpTarget.Kind, "Parameter", StringComparison.Ordinal)
            && string.Equals(issue.HelpTarget.NodeId, invalidNodeId, StringComparison.Ordinal)
            && string.Equals(issue.HelpTarget.ParameterKey, "slug", StringComparison.Ordinal);
        var inspectorHelpTargetOk = helpTargetOk
            && parameterSnapshot is not null
            && string.Equals(issue!.HelpTarget!.HelpText, parameterSnapshot.HelpText, StringComparison.Ordinal);
        return (parameterSnapshots, CreateRepairEvidence(session, validation), helpTargetOk, inspectorHelpTargetOk);
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

public sealed record ConsumerSampleProofValidationSummary(
    int TotalIssueCount,
    int ErrorCount,
    int WarningCount,
    int InvalidConnectionCount,
    int InvalidParameterCount)
{
    public static ConsumerSampleProofValidationSummary Empty { get; } = new(0, 0, 0, 0, 0);
}

public sealed record ConsumerSampleProofValidationFeedback(
    string Code,
    string Severity,
    string Message,
    ConsumerSampleProofFocusTarget FocusTarget);

public sealed record ConsumerSampleProofFocusTarget(
    string Kind,
    string? NodeId = null,
    string? ConnectionId = null,
    string? EndpointId = null,
    string? ParameterKey = null);

public sealed record ConsumerSampleRepairEvidence(
    string IssueCode,
    string Target,
    string Action,
    string Result);

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
