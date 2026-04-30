namespace AsterGraph.Demo.Cookbook;

public static partial class DemoCookbookCatalog
{
    public static IReadOnlyList<DemoCookbookRecipe> Recipes { get; } =
    [
        new DemoCookbookRecipe(
            "starter-host-route",
            DemoCookbookRecipeCategory.StarterHost,
            "Starter host route",
            "Start from the hosted Avalonia route, then launch the Demo scenario for the fuller walkthrough.",
            [
                new DemoCookbookAnchor(
                    "Starter Avalonia host entry",
                    "tools/AsterGraph.Starter.Avalonia/Program.cs",
                    "CreateRuntimeSurface"),
                new DemoCookbookAnchor(
                    "Starter hosted builder",
                    "tools/AsterGraph.Starter.Avalonia/Program.cs",
                    "CreateHostBuilder"),
                new DemoCookbookAnchor(
                    "Hosted Avalonia builder facade",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphHostBuilder.cs",
                    "BuildAvaloniaView"),
                new DemoCookbookAnchor(
                    "Runtime-only route contrast",
                    "src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs",
                    "CreateSession(AsterGraphEditorOptions)"),
            ],
            [
                new DemoCookbookAnchor(
                    "AI pipeline scenario launch",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "AiPipelineScenario"),
            ],
            [
                new DemoCookbookAnchor(
                    "Host recipe ladder",
                    "docs/en/host-recipe-ladder.md",
                    "Host Recipe Ladder"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "Host startup copies the shipped Avalonia boot route.",
                    "CreateRuntimeSurface"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The AI pipeline preset opens a ready graph for onboarding.",
                    "AiPipelineScenario"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The onboarding proof marker bounds this route to supported host setup.",
                    "FIVE_MINUTE_ONBOARDING_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Preset launch selects a ready graph for first-run inspection.",
                    "AiPipelineScenario"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "The host ladder keeps the first copied route separate from deeper proof routes.",
                    "Host Recipe Ladder"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Startup evidence points to the hosted entry point instead of Demo internals.",
                    "CreateRuntimeSurface"),
            ],
            ["FIVE_MINUTE_ONBOARDING_OK"],
            new DemoCookbookRouteClarity(
                "Shipped Avalonia route: AsterGraphHostBuilder.Create(...).BuildAvaloniaView() via StarterAvaloniaAppBuilder.",
                "`AsterGraph.Avalonia` composes the hosted UI on top of `AsterGraph.Editor` session/runtime surfaces.",
                "Demo scenario launch is inspection/proof only; copy the starter host code instead of Demo ViewModel code."),
            "Avalonia is the shipped hosted route; WPF remains validation-only and Demo remains sample/proof surface."),
        new DemoCookbookRecipe(
            "authoring-surface-route",
            DemoCookbookRecipeCategory.Authoring,
            "Authoring surface route",
            "Copy custom presentation, node-side parameter editing, command projection, and authoring seams from ConsumerSample.",
            [
                new DemoCookbookAnchor(
                    "ConsumerSample authoring recipe",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs",
                    "CreateEdgeOverlay"),
                new DemoCookbookAnchor(
                    "ConsumerSample host command projection",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs",
                    "GetCommandDescriptors"),
                new DemoCookbookAnchor(
                    "Navigator outline query",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetNavigatorOutlineSnapshot"),
                new DemoCookbookAnchor(
                    "Authoring recovery metadata projection",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphHostedActionFactory.cs",
                    "RecoveryHint"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo authoring presenter surface",
                    "src/AsterGraph.Demo/Presentation/DemoShowcasePresenters.cs",
                    "CreateReplacementPreviewOptions"),
                new DemoCookbookAnchor(
                    "Demo tour proof lines",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs",
                    "scenario tour"),
                new DemoCookbookAnchor(
                    "Designer workbench projection proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorHierarchyStateContractsTests.cs",
                    "SessionQueries_GetNavigatorOutlineSnapshot_ProjectsActiveScopeGroupSelectionAndBoundaryConnections"),
            ],
            [
                new DemoCookbookAnchor(
                    "Authoring surface recipe",
                    "docs/en/authoring-surface-recipe.md",
                    "AUTHORING_SURFACE_OK:True"),
                new DemoCookbookAnchor(
                    "Designer workbench cookbook proof",
                    "docs/en/demo-cookbook.md",
                    "DEMO_COOKBOOK_DESIGNER_WORKBENCH_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Edge overlays and previews show authoring operations on the graph surface.",
                    "CreateEdgeOverlay"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Hosted command and parameter surfaces stay tied to ConsumerSample metadata.",
                    "GetCommandDescriptors"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The authoring recipe is backed by an explicit proof marker.",
                    "AUTHORING_SURFACE_OK"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Navigator outline projection keeps graph, group, scope, and selection state source-backed.",
                    "GetNavigatorOutlineSnapshot"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Selection-owned commands are projected from the hosted session.",
                    "GetCommandDescriptors"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Connection overlays stay visible as graph-surface authoring evidence.",
                    "CreateEdgeOverlay"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Parameter and command metadata remain tied to ConsumerSample surfaces.",
                    "GetCommandDescriptors"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Disabled authoring actions carry recovery hints through hosted workbench metadata.",
                    "RecoveryHint"),
            ],
            ["AUTHORING_SURFACE_OK", "DESIGNER_WORKBENCH_AUTHORING_OK"],
            new DemoCookbookRouteClarity(
                "Hosted Avalonia authoring route: AsterGraphHostBuilder.UsePresentation(...) with IGraphEditorSession.Queries.GetCommandDescriptors() and GetNavigatorOutlineSnapshot().",
                "Supported seams live in `AsterGraph.Avalonia` hosting and `AsterGraph.Editor` session/query/command/navigation contracts.",
                "ConsumerSample is the copyable recipe; Demo presenters are visual proof only and do not define package contracts."),
            "Authoring samples reuse public seams, source-backed outline projection, and command recovery metadata without creating a second editor/runtime model."),
        new DemoCookbookRecipe(
            "performance-viewport-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "Performance and viewport route",
            "Trace visible-scene projection, minimap cadence, large-graph budgets, and scale proof for desktop graph hosts.",
            [
                new DemoCookbookAnchor(
                    "Visible scene projection",
                    "src/AsterGraph.Editor/Viewport/ViewportVisibleSceneProjection.cs",
                    "ToBudgetMarker"),
                new DemoCookbookAnchor(
                    "Workbench performance policy",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphWorkbenchPerformancePolicy.cs",
                    "ToMiniMapBudgetMarker"),
                new DemoCookbookAnchor(
                    "ConsumerSample viewport proof",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs",
                    "MinimapLightweightProjectionOk"),
                new DemoCookbookAnchor(
                    "Layout apply and snap commands",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TryApplyLayoutRequest"),
                new DemoCookbookAnchor(
                    "Layout snap command contract",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TrySnapSelectedNodesToGrid"),
                new DemoCookbookAnchor(
                    "Layout service regression proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorLayoutProviderSeamTests.cs",
                    "TrySnapAllNodesToGrid"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo cookbook graph workspace",
                    "src/AsterGraph.Demo/Cookbook/DemoCookbookWorkspaceProjection.cs",
                    "GraphOperations"),
                new DemoCookbookAnchor(
                    "Workbench layout tool projection",
                    "tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs",
                    "AuthoringToolsChrome_ProjectsStockSelectionLayoutActions"),
            ],
            [
                new DemoCookbookAnchor(
                    "Scale baseline",
                    "docs/en/scale-baseline.md",
                    "SCALE_PERFORMANCE_BUDGET_OK"),
                new DemoCookbookAnchor(
                    "Feature catalog projection evidence",
                    "docs/en/feature-catalog.md",
                    "workbench.panel-projection-evidence"),
                new DemoCookbookAnchor(
                    "Public layout command inventory",
                    "docs/en/public-api-inventory.md",
                    "TryApplyLayoutRequest"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Visible-scene projection reports graph counts against the current viewport.",
                    "ToBudgetMarker"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Layout preview/apply and snap-to-grid commands move through the canonical session route.",
                    "TryApplyLayoutRequest"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Mini-map cadence and projection markers stay visible in proof output.",
                    "ToMiniMapBudgetMarker"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "ScaleSmoke keeps baseline, large, and stress budgets tied to release evidence.",
                    "SCALE_PERFORMANCE_BUDGET_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Viewport projection keeps graph density readable without adding a second renderer.",
                    "ToBudgetMarker"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Selection layout tools expose align, distribute, and snap commands through the hosted workbench.",
                    "AuthoringToolsChrome_ProjectsStockSelectionLayoutActions"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Workbench policy exposes minimap cadence as host-readable evidence.",
                    "ToMiniMapBudgetMarker"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "ConsumerSample proof keeps minimap projection and inspector evidence bounded.",
                    "MINIMAP_LIGHTWEIGHT_PROJECTION_OK"),
            ],
            ["MINIMAP_LIGHTWEIGHT_PROJECTION_OK", "PROJECTION_PERFORMANCE_EVIDENCE_OK", "SCALE_PERFORMANCE_BUDGET_OK", "LAYOUT_PROVIDER_SEAM_OK", "LAYOUT_PREVIEW_APPLY_CANCEL_OK", "LAYOUT_UNDO_TRANSACTION_OK"],
            new DemoCookbookRouteClarity(
                "Performance route: ViewportVisibleSceneProjector.Project(...) plus AsterGraphWorkbenchPerformancePolicy.FromMode(...) and IGraphEditorSession layout preview/apply and snap commands.",
                "Supported seams live in `AsterGraph.Editor` viewport projection, layout command contracts, and `AsterGraph.Avalonia` hosted workbench policy contracts.",
                "Demo cookbook projection is a graph-above-code teaching surface only; ScaleSmoke, Editor tests, and ConsumerSample remain the proof sources."),
            "Performance and layout proof reports projection, layout command, and budget evidence; it does not add a background graph index, second renderer, or runtime execution mode."),
        new DemoCookbookRecipe(
            "groups-subgraphs-route",
            DemoCookbookRecipeCategory.GroupsSubgraphs,
            "Groups and subgraphs route",
            "Trace persisted group state, composite child scopes, collapsed projection, and boundary-edge proof.",
            [
                new DemoCookbookAnchor(
                    "Collapsed group serialization snapshot",
                    "tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs",
                    "WritesAndReadsCollapsedGroupBoundaryPayload"),
                new DemoCookbookAnchor(
                    "Hierarchy projection contract",
                    "tests/AsterGraph.Editor.Tests/GraphEditorHierarchyStateContractsTests.cs",
                    "SessionQueries_GetHierarchyStateSnapshot_ExposesCollapsedGroupMembershipBoundaryEdgesAndMoveConstraints"),
                new DemoCookbookAnchor(
                    "Collapsed canvas proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs",
                    "CollapsedGroup_ProjectsContainerChromeBoundaryEdgesAndHiddenMembers"),
            ],
            [
                new DemoCookbookAnchor(
                    "Cookbook groups workspace projection",
                    "src/AsterGraph.Demo/Cookbook/DemoCookbookWorkspaceProjection.cs",
                    "GroupsSubgraphs"),
            ],
            [
                new DemoCookbookAnchor(
                    "Advanced editing hierarchy semantics",
                    "docs/en/advanced-editing.md",
                    "Hierarchy Semantics"),
                new DemoCookbookAnchor(
                    "Cookbook groups route docs",
                    "docs/en/demo-cookbook.md",
                    "groups-subgraphs-route"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Collapsed groups keep member nodes hidden while boundary connections remain represented.",
                    "SessionQueries_GetHierarchyStateSnapshot_ExposesCollapsedGroupMembershipBoundaryEdgesAndMoveConstraints"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Group membership, collapse state, and composite child scope metadata roundtrip through the current schema.",
                    "WritesAndReadsCollapsedGroupBoundaryPayload"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The cookbook marker ties group/subgraph claims to serialization and canvas proof.",
                    "GROUP_SERIALIZATION_COOKBOOK_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Collapsed group projection keeps hidden member nodes out of direct canvas selection.",
                    "CollapsedGroup_ProjectsContainerChromeBoundaryEdgesAndHiddenMembers"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Boundary-edge behavior remains explicit when one endpoint is hidden by a collapsed group.",
                    "SessionQueries_GetHierarchyStateSnapshot_ExposesCollapsedGroupMembershipBoundaryEdgesAndMoveConstraints"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Serialized snapshots expose persisted groups and composite scopes without persisting projection-only flags.",
                    "WritesAndReadsCollapsedGroupBoundaryPayload"),
            ],
            ["GROUP_SERIALIZATION_COOKBOOK_OK"],
            new DemoCookbookRouteClarity(
                "Groups route: persisted GraphNodeGroup and GraphScope snapshots plus IGraphEditorSession.Queries.GetHierarchyStateSnapshot().",
                "Supported seams live in `AsterGraph.Core` serialization and `AsterGraph.Editor` hierarchy query contracts.",
                "Demo cookbook projection is proof/navigation only; it does not add generated demo code or a workflow engine."),
            "Group/subgraph proof is limited to persisted snapshots, hierarchy queries, and stock Avalonia projection; nested group mutation and specialized boundary-edge styling remain deferred."),
        new DemoCookbookRecipe(
            "plugin-trust-route",
            DemoCookbookRecipeCategory.PluginTrust,
            "Plugin trust route",
            "Inspect trusted in-process plugin discovery, allowlist decisions, manifest validation, and load snapshots.",
            [
                new DemoCookbookAnchor(
                    "Plugin discovery entry point",
                    "src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs",
                    "DiscoverPluginCandidates"),
                new DemoCookbookAnchor(
                    "Plugin package staging entry point",
                    "src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs",
                    "StagePluginPackage"),
                new DemoCookbookAnchor(
                    "ConsumerSample route boundary",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs",
                    "RouteBoundaryLines"),
                new DemoCookbookAnchor(
                    "ConsumerSample plugin proof",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs",
                    "PluginTrustEvidencePanelOk"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo plugin trust policy",
                    "src/AsterGraph.Demo/Integration/DemoPluginTrustWorkspace.cs",
                    "PluginTrustDecision"),
                new DemoCookbookAnchor(
                    "Demo extensions panel projection",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Extensions.cs",
                    "PluginTrust"),
            ],
            [
                new DemoCookbookAnchor(
                    "Plugin host recipe",
                    "docs/en/plugin-host-recipe.md",
                    "Proof Marker Expectations"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Plugin manifests and trust decisions stay attached to trusted extension metadata.",
                    "PluginTrustDecision"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "The Demo extensions projection exposes trusted in-process plugin state.",
                    "PluginTrust"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The ConsumerSample proof panel evidence backs the trust route.",
                    "PLUGIN_TRUST_EVIDENCE_PANEL_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Trust decisions stay inspectable as explicit plugin metadata.",
                    "PluginTrustDecision"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "The extensions panel projects trusted in-process loading state.",
                    "PluginTrust"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Route boundary lines keep plugin trust evidence out of the canonical host path.",
                    "RouteBoundaryLines"),
            ],
            ["PLUGIN_TRUST_EVIDENCE_PANEL_OK"],
            new DemoCookbookRouteClarity(
                "Plugin route: AsterGraphEditorFactory.DiscoverPluginCandidates(...) with host-owned PluginTrustPolicy before loading.",
                "Supported APIs live in `AsterGraph.Editor` plugin discovery, trust, and registration contracts.",
                "Demo trust workspace is an evidence surface only; it does not sandbox or isolate untrusted plugin code."),
            "Plugins are trusted in-process extensions; the recipe does not imply sandboxing or untrusted-code isolation."),
        new DemoCookbookRecipe(
            "diagnostics-support-route",
            DemoCookbookRecipeCategory.DiagnosticsSupport,
            "Diagnostics and support route",
            "Capture support-bundle and runtime diagnostics evidence from ConsumerSample and Demo runtime projections.",
            [
                new DemoCookbookAnchor(
                    "Runtime overlay option seam",
                    "src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs",
                    "RuntimeOverlayProvider"),
                new DemoCookbookAnchor(
                    "ConsumerSample runtime query path",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleHost.cs",
                    "GetRuntimeOverlaySnapshot"),
                new DemoCookbookAnchor(
                    "ConsumerSample support bundle",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs",
                    "RuntimeLogs"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo runtime diagnostics projection",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeProjection.cs",
                    "RuntimeDiagnosticEntry"),
                new DemoCookbookAnchor(
                    "Demo runtime timeline",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeTimeline.cs",
                    "RuntimeCommandTimelineEntry"),
            ],
            [
                new DemoCookbookAnchor(
                    "Support bundle docs",
                    "docs/en/support-bundle.md",
                    "RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK:True"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Runtime diagnostics and command timeline state are captured as local overlays.",
                    "RuntimeDiagnosticEntry"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Support bundle runtime logs provide handoff evidence without telemetry claims.",
                    "RuntimeLogs"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The runtime timeline links graph commands to captured diagnostics.",
                    "RuntimeCommandTimelineEntry"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Runtime diagnostics are projected as local feedback rows.",
                    "RuntimeDiagnosticEntry"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Command timeline entries keep graph-command effects visible during support review.",
                    "RuntimeCommandTimelineEntry"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Support bundles preserve runtime log evidence for handoff.",
                    "RuntimeLogs"),
            ],
            ["RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK"],
            new DemoCookbookRouteClarity(
                "Runtime diagnostics route: AsterGraphEditorOptions.RuntimeOverlayProvider plus IGraphEditorSession.Queries.GetRuntimeOverlaySnapshot().",
                "Supported APIs live in `AsterGraph.Editor` runtime overlay/query contracts and ConsumerSample local support-bundle code.",
                "Demo runtime timeline is a local projection only; it does not add telemetry or remote sync."),
            "Support bundles are local handoff evidence, not telemetry, remote sync, or support-scope expansion."),
        new DemoCookbookRecipe(
            "review-help-route",
            DemoCookbookRecipeCategory.ReviewHelp,
            "Review and help route",
            "Trace validation repair, contextual help, and review-loop proof markers from existing support-bundle evidence.",
            [
                new DemoCookbookAnchor(
                    "ConsumerSample repair/help proof",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs",
                    "RepairHelpReviewLoopOk"),
                new DemoCookbookAnchor(
                    "ConsumerSample validation feedback bundle",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs",
                    "ValidationFeedback"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo proof surface projection",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs",
                    "DemoHostMenuGroups.Proof"),
            ],
            [
                new DemoCookbookAnchor(
                    "Feature catalog review/help row",
                    "docs/en/feature-catalog.md",
                    "authoring.repair-help-review"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Validation feedback links repair suggestions to concrete graph issues.",
                    "ValidationFeedback"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The repair/help proof marker closes the review-loop scenario.",
                    "REPAIR_HELP_REVIEW_LOOP_OK"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "ConsumerSample proof code is the host-facing example for repair/help handoff.",
                    "RepairHelpReviewLoopOk"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Validation feedback points repair actions at concrete graph issues.",
                    "ValidationFeedback"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Proof panels expose review/help evidence without adding a workflow engine.",
                    "DemoHostMenuGroups.Proof"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Repair/help proof covers connection repair handoff evidence.",
                    "RepairHelpReviewLoopOk"),
            ],
            ["REPAIR_HELP_REVIEW_LOOP_OK"],
            new DemoCookbookRouteClarity(
                "Review/help route: IGraphEditorSession validation feedback and ConsumerSample support-bundle proof.",
                "Supported seams stay in `AsterGraph.Editor` session validation, repair, and support evidence contracts.",
                "Demo proof panels are review evidence only; they do not add a workflow engine or macro scheduler."),
            "Review/help evidence stays bounded to existing validation and support-bundle proof; it is not a new workflow engine."),
    ];
}
