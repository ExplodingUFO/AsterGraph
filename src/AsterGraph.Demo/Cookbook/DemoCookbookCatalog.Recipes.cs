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
            "v077-authoring-platform-route",
            DemoCookbookRecipeCategory.Authoring,
            "v0.77 authoring platform route",
            "Walk command discovery, semantic edits, reusable presets, selection transforms, and navigation focus as one supported authoring flow.",
            [
                new DemoCookbookAnchor(
                    "Runtime command registry",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetCommandRegistry"),
                new DemoCookbookAnchor(
                    "Semantic delete and reconnect",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TryDeleteSelectionAndReconnect"),
                new DemoCookbookAnchor(
                    "Reusable preset application",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TryApplyFragmentTemplatePreset"),
                new DemoCookbookAnchor(
                    "Selection transform snapshot",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetSelectionTransformSnapshot"),
                new DemoCookbookAnchor(
                    "Graph item search and bookmarks",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "SearchGraphItems"),
                new DemoCookbookAnchor(
                    "Search result focus command",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TryFocusGraphItemSearchResult"),
            ],
            [
                new DemoCookbookAnchor(
                    "Command registry proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorCommandRegistryTests.cs",
                    "CommandRegistry_ExposesStableMenuToolAndShortcutPlacements"),
                new DemoCookbookAnchor(
                    "Semantic editing proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorDeleteReconnectDetachTests.cs",
                    "TryDeleteSelectionAndReconnect"),
                new DemoCookbookAnchor(
                    "Template preset proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSessionFragmentContractsTests.cs",
                    "SessionCommands_ApplyFragmentTemplatePreset_IsOneUndoableRemappedFragmentPaste"),
                new DemoCookbookAnchor(
                    "Selection transform proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSelectionTransformContractsTests.cs",
                    "Commands_RouteSelectionTransformMoveThroughCanonicalCommandInvocation"),
                new DemoCookbookAnchor(
                    "Navigation search proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorGraphItemSearchProjectionTests.cs",
                    "Queries_SearchGraphItemsReturnsStableNodeGroupConnectionScopeAndIssueResults"),
                new DemoCookbookAnchor(
                    "Bookmark focus proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorNavigationFocusWorkflowTests.cs",
                    "Commands_ViewportBookmarksCaptureActivateAndRemoveCurrentScopeViewport"),
            ],
            [
                new DemoCookbookAnchor(
                    "Public API inventory",
                    "docs/en/public-api-inventory.md",
                    "canonical session route"),
                new DemoCookbookAnchor(
                    "Command surface support note",
                    "docs/en/support-bundle.md",
                    "Command palette and toolbar contribution proof remains on the shared command/session route"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "The command registry is the code anchor for menus, tools, shortcuts, and disabled recovery states.",
                    "GetCommandRegistry"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Semantic edits and reusable presets run through undoable session commands.",
                    "TryDeleteSelectionAndReconnect"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Selection transform and snap evidence stay query-backed instead of UI-local.",
                    "GetSelectionTransformSnapshot"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Search and focus workflows return stable graph item targets.",
                    "SearchGraphItems"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Focused tests prove each v0.77 workflow without turning Demo into a generator.",
                    "CommandRegistry_ExposesStableMenuToolAndShortcutPlacements"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Selection movement is represented by source-backed transform snapshots and canonical move commands.",
                    "Commands_RouteSelectionTransformMoveThroughCanonicalCommandInvocation"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Delete-and-reconnect keeps semantic connection repair on the shared command route.",
                    "TryDeleteSelectionAndReconnect"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Template presets and snap/transform evidence keep repeated authoring actions readable.",
                    "TryApplyFragmentTemplatePreset"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Command registry entries expose where hosts should present actions.",
                    "CommandRegistry_ExposesStableMenuToolAndShortcutPlacements"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Search-result and issue focus reuse stable runtime targets.",
                    "TryFocusGraphItemSearchResult"),
            ],
            [
                "CommandRegistry_ExposesStableMenuToolAndShortcutPlacements",
                "SessionCommands_ApplyFragmentTemplatePreset_IsOneUndoableRemappedFragmentPaste",
                "Commands_RouteSelectionTransformMoveThroughCanonicalCommandInvocation",
                "Queries_SearchGraphItemsReturnsStableNodeGroupConnectionScopeAndIssueResults",
                "Commands_ViewportBookmarksCaptureActivateAndRemoveCurrentScopeViewport",
            ],
            new DemoCookbookRouteClarity(
                "v0.77 route: IGraphEditorSession.Queries.GetCommandRegistry(), SearchGraphItems(), GetSelectionTransformSnapshot(), and IGraphEditorSession.Commands semantic/template/focus commands.",
                "Supported seams live in `AsterGraph.Editor` command/query snapshots and `AsterGraph.Avalonia` hosted projection surfaces.",
                "Demo cookbook presents code anchors and proof markers only; it does not generate workflows or execute macro scripts."),
            "v0.77 cookbook steps are code-plus-proof guidance over supported session contracts; Demo remains a sample/proof surface.",
            [
                new DemoCookbookWorkflowStep(
                    DemoCookbookWorkflowKind.CommandRegistry,
                    "Discover command surfaces",
                    "query.command-registry",
                    "GetCommandRegistry",
                    "CommandRegistry_ExposesStableMenuToolAndShortcutPlacements",
                    "CommandRegistry_ExposesStableMenuToolAndShortcutPlacements"),
                new DemoCookbookWorkflowStep(
                    DemoCookbookWorkflowKind.SemanticEditing,
                    "Run semantic delete and reconnect",
                    "selection.delete-reconnect",
                    "TryDeleteSelectionAndReconnect",
                    "TryDeleteSelectionAndReconnect",
                    "CommandRegistry_ExposesStableMenuToolAndShortcutPlacements"),
                new DemoCookbookWorkflowStep(
                    DemoCookbookWorkflowKind.TemplatePreset,
                    "Apply a reusable fragment preset",
                    "fragments.apply-template-preset",
                    "TryApplyFragmentTemplatePreset",
                    "SessionCommands_ApplyFragmentTemplatePreset_IsOneUndoableRemappedFragmentPaste",
                    "SessionCommands_ApplyFragmentTemplatePreset_IsOneUndoableRemappedFragmentPaste"),
                new DemoCookbookWorkflowStep(
                    DemoCookbookWorkflowKind.SelectionTransform,
                    "Inspect and move selected graph items",
                    "selection.transform.move",
                    "GetSelectionTransformSnapshot",
                    "Commands_RouteSelectionTransformMoveThroughCanonicalCommandInvocation",
                    "Commands_RouteSelectionTransformMoveThroughCanonicalCommandInvocation"),
                new DemoCookbookWorkflowStep(
                    DemoCookbookWorkflowKind.NavigationFocus,
                    "Search, bookmark, and focus graph items",
                    "viewport.focus-search-result",
                    "SearchGraphItems",
                    "Commands_ViewportBookmarksCaptureActivateAndRemoveCurrentScopeViewport",
                    "Queries_SearchGraphItemsReturnsStableNodeGroupConnectionScopeAndIssueResults"),
            ]),
        new DemoCookbookRecipe(
            "v078-rendering-viewport-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "v0.78 rendering and viewport route",
            "Trace scene snapshots, viewport projection, visible-connection budgeting, and ConsumerSample LOD proof for rendering hosts.",
            [
                new DemoCookbookAnchor(
                    "Scene snapshot query",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetSceneSnapshot"),
                new DemoCookbookAnchor(
                    "Viewport snapshot query",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetViewportSnapshot"),
                new DemoCookbookAnchor(
                    "Visible scene projection marker",
                    "src/AsterGraph.Editor/Viewport/ViewportVisibleSceneProjection.cs",
                    "ToBudgetMarker"),
                new DemoCookbookAnchor(
                    "Connection renderer budget gate",
                    "src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasConnectionSceneRenderer.cs",
                    "ApplyVisibleSceneBudget"),
            ],
            [
                new DemoCookbookAnchor(
                    "Visible connection rendering proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasConnectionSceneRendererTests.cs",
                    "RenderConnections_VisibleSceneBudgetScopesCommittedConnectionsButPreservesPendingPreview"),
                new DemoCookbookAnchor(
                    "Scene host viewport invalidation proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasSceneHostViewportProjectionTests.cs",
                    "UpdateViewportTransform_ReportsVisibleSceneInvalidationDiffMarker"),
                new DemoCookbookAnchor(
                    "ConsumerSample viewport LOD proof",
                    "tests/AsterGraph.ConsumerSample.Tests/ConsumerSampleProofTests.cs",
                    "VIEWPORT_LOD_POLICY_OK:True"),
            ],
            [
                new DemoCookbookAnchor(
                    "ConsumerSample viewport LOD docs",
                    "docs/en/consumer-sample.md",
                    "VIEWPORT_LOD_POLICY_OK:True"),
                new DemoCookbookAnchor(
                    "ConsumerSample edge rendering docs",
                    "docs/en/consumer-sample.md",
                    "EDGE_RENDERING_SCOPE_BOUNDARY_OK:True"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Hosts read one scene snapshot that combines document, viewport, selection, and pending connection state.",
                    "GetSceneSnapshot"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Committed connection rendering can be scoped by the visible scene budget while pending previews remain visible.",
                    "ApplyVisibleSceneBudget"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Viewport projection emits budget markers for the currently visible graph region.",
                    "ToBudgetMarker"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "ConsumerSample LOD proof bounds selected and hovered affordances to hosted workbench policy.",
                    "VIEWPORT_LOD_POLICY_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Selected and hovered adorners remain scoped to the viewport LOD policy.",
                    "SELECTED_HOVERED_ADORNER_SCOPE_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Edge rendering stays bounded to connection geometry and viewport projection evidence.",
                    "EDGE_RENDERING_SCOPE_BOUNDARY_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Visible scene projection keeps dense graphs readable without a second rendering route.",
                    "ToBudgetMarker"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Viewport state remains inspectable through the session query surface.",
                    "GetViewportSnapshot"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "LOD scope proof keeps rendering behavior bounded to host-visible feedback.",
                    "VIEWPORT_LOD_SCOPE_BOUNDARY_OK"),
            ],
            [
                "VIEWPORT_LOD_POLICY_OK",
                "SELECTED_HOVERED_ADORNER_SCOPE_OK",
                "LARGE_GRAPH_BALANCED_UX_OK",
                "VIEWPORT_LOD_SCOPE_BOUNDARY_OK",
                "EDGE_RENDERING_SCOPE_BOUNDARY_OK",
            ],
            new DemoCookbookRouteClarity(
                "v0.78 rendering route: IGraphEditorSession.Queries.GetSceneSnapshot(), GetViewportSnapshot(), and ViewportVisibleSceneProjector feed the Avalonia scene renderer.",
                "Supported seams live in `AsterGraph.Editor` scene/viewport snapshots and `AsterGraph.Avalonia` scene rendering controls.",
                "Demo cookbook references rendering evidence only; Demo does not claim another renderer or alternate runtime path."),
            "Rendering and viewport coverage is code/demo/docs proof over existing scene contracts, not a new renderer, virtualizer, or executable sample promise."),
        new DemoCookbookRecipe(
            "v078-customization-route",
            DemoCookbookRecipeCategory.Authoring,
            "v0.78 customization route",
            "Trace custom node presenters, parameter editor registries, edge overlays, and runtime inspector proof through ConsumerSample.",
            [
                new DemoCookbookAnchor(
                    "Presentation options wiring",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs",
                    "CreatePresentationOptions"),
                new DemoCookbookAnchor(
                    "Edge overlay factory",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleAuthoringSurfaceRecipe.cs",
                    "CreateEdgeOverlay"),
                new DemoCookbookAnchor(
                    "Custom node presenter",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleNodeVisualPresenter.cs",
                    "Create"),
                new DemoCookbookAnchor(
                    "Custom connection overlay",
                    "tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleConnectionOverlay.cs",
                    "ConnectionGeometries"),
                new DemoCookbookAnchor(
                    "Connection geometry query",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetConnectionGeometrySnapshots"),
                new DemoCookbookAnchor(
                    "Presentation option surface",
                    "src/AsterGraph.Avalonia/Presentation/AsterGraphPresentationOptions.cs",
                    "NodeVisualPresenter"),
            ],
            [
                new DemoCookbookAnchor(
                    "Customization presentation proof",
                    "tests/AsterGraph.ConsumerSample.Tests/ConsumerSampleAuthoringPresentationTests.cs",
                    "AuthoringSurfaceRecipe_CreatePresentationOptions_ProvidesCustomNodeAndEditorSeams"),
                new DemoCookbookAnchor(
                    "ConsumerSample custom extension proof",
                    "tests/AsterGraph.ConsumerSample.Tests/ConsumerSampleProofTests.cs",
                    "CUSTOM_EXTENSION_SURFACE_OK:True"),
            ],
            [
                new DemoCookbookAnchor(
                    "Authoring surface customization docs",
                    "docs/en/authoring-surface-recipe.md",
                    "ConsumerSampleNodeVisualPresenter"),
                new DemoCookbookAnchor(
                    "Parameter editor registry docs",
                    "docs/en/authoring-surface-recipe.md",
                    "INodeParameterEditorRegistry"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "ConsumerSample wires custom presentation through options instead of Demo internals.",
                    "CreatePresentationOptions"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Node-side editors keep using the parameter editor registry and definition metadata.",
                    "INodeParameterEditorRegistry"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Custom edge overlays read connection geometry snapshots without replacing the stock edge renderer.",
                    "GetConnectionGeometrySnapshots"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "ConsumerSample proof markers bind presenter, anchor, overlay, inspector, and scope boundaries together.",
                    "CUSTOM_EXTENSION_SURFACE_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Custom presenter lifecycle proof stays tied to selected node visuals and anchor affordances.",
                    "CUSTOM_EXTENSION_NODE_PRESENTER_LIFECYCLE_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Host-owned edge overlays remain separate from stock connection rendering.",
                    "CUSTOM_EXTENSION_EDGE_OVERLAY_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Anchor and scope-boundary proof keeps custom chrome aligned with existing graph layout.",
                    "CUSTOM_EXTENSION_ANCHOR_SURFACE_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Runtime inspector customization remains proof-backed and local to ConsumerSample.",
                    "CUSTOM_EXTENSION_RUNTIME_INSPECTOR_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Customization scope proof prevents the sample from widening runtime contracts.",
                    "CUSTOM_EXTENSION_SCOPE_BOUNDARY_OK"),
            ],
            [
                "CUSTOM_EXTENSION_NODE_PRESENTER_LIFECYCLE_OK",
                "CUSTOM_EXTENSION_ANCHOR_SURFACE_OK",
                "CUSTOM_EXTENSION_EDGE_OVERLAY_OK",
                "CUSTOM_EXTENSION_RUNTIME_INSPECTOR_OK",
                "CUSTOM_EXTENSION_SCOPE_BOUNDARY_OK",
                "CUSTOM_EXTENSION_SURFACE_OK",
            ],
            new DemoCookbookRouteClarity(
                "v0.78 customization route: AsterGraphPresentationOptions with custom node presenters, parameter editor registries, and host-owned edge overlays.",
                "Supported seams live in `AsterGraph.Avalonia` presentation options and `AsterGraph.Editor` query snapshots consumed by ConsumerSample.",
                "Demo cookbook treats ConsumerSample as the copyable customization recipe; Demo remains visual proof only."),
            "Customization coverage stays on ConsumerSample presentation seams and proof markers; it does not widen the runtime model or sample boundary."),
        new DemoCookbookRecipe(
            "v078-spatial-authoring-route",
            DemoCookbookRecipeCategory.Authoring,
            "v0.78 spatial authoring route",
            "Trace node surface sizing, composite scopes, scope navigation, and route-vertex geometry through shared session contracts.",
            [
                new DemoCookbookAnchor(
                    "Node surface snapshots",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetNodeSurfaceSnapshots"),
                new DemoCookbookAnchor(
                    "Node surface resize command",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TrySetNodeSize"),
                new DemoCookbookAnchor(
                    "Composite scope command",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TryWrapSelectionToComposite"),
                new DemoCookbookAnchor(
                    "Composite port exposure command",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TryExposeCompositePort"),
                new DemoCookbookAnchor(
                    "Composite node snapshots",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetCompositeNodeSnapshots"),
                new DemoCookbookAnchor(
                    "Scope navigation snapshots",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetScopeNavigationSnapshot"),
                new DemoCookbookAnchor(
                    "Connection route vertex command",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TryInsertConnectionRouteVertex"),
                new DemoCookbookAnchor(
                    "Connection geometry snapshots",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetConnectionGeometrySnapshots"),
            ],
            [
                new DemoCookbookAnchor(
                    "Demo spatial surface cue",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs",
                    "TIERED_NODE_SURFACE_OK / FIXED_GROUP_FRAME_OK / HIERARCHY_SEMANTICS_OK / NON_OBSCURING_EDITING_OK / VISUAL_SEMANTICS_OK"),
                new DemoCookbookAnchor(
                    "Demo composite and edge cue",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs",
                    "COMPOSITE_SCOPE_OK / EDGE_NOTE_OK / EDGE_GEOMETRY_OK / DISCONNECT_FLOW_OK"),
                new DemoCookbookAnchor(
                    "Node surface mutation proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs",
                    "SessionCommands_TrySetNodeSize_PersistsUndoableTierDrivenMutation"),
                new DemoCookbookAnchor(
                    "Composite scope proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorNodeSurfaceContractsTests.cs",
                    "SessionCommands_NodeGroups_PromoteToComposite_AndExposeBoundaryPorts"),
                new DemoCookbookAnchor(
                    "Connection geometry proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorConnectionGeometryContractsTests.cs",
                    "SessionQueries_GetConnectionGeometrySnapshots_ProjectPersistedRouteVertices_IntoSceneContracts"),
            ],
            [
                new DemoCookbookAnchor(
                    "Advanced editing node surface docs",
                    "docs/en/advanced-editing.md",
                    "TIERED_NODE_SURFACE_OK"),
                new DemoCookbookAnchor(
                    "Advanced editing composite scope docs",
                    "docs/en/advanced-editing.md",
                    "COMPOSITE_SCOPE_OK"),
                new DemoCookbookAnchor(
                    "Advanced editing edge geometry docs",
                    "docs/en/advanced-editing.md",
                    "EDGE_GEOMETRY_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Node surface size and disclosure changes move through undoable session commands.",
                    "TrySetNodeSize"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Composite nodes expose child-scope and boundary-port metadata through session snapshots.",
                    "GetCompositeNodeSnapshots"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Scope navigation snapshots keep spatial authoring anchored to the active graph scope.",
                    "GetScopeNavigationSnapshot"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Route-vertex editing and geometry snapshots keep edge authoring spatially inspectable.",
                    "TryInsertConnectionRouteVertex"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Advanced editing markers bind node surfaces, composites, and edge geometry to proof evidence.",
                    "TIERED_NODE_SURFACE_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Node-side editors stay non-obscuring while selected surfaces are resized or expanded.",
                    "NON_OBSCURING_EDITING_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Route-vertex tools keep edge geometry explicit instead of UI-local.",
                    "EDGE_GEOMETRY_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Visual semantics proof keeps tiered nodes and spatial chrome readable.",
                    "VISUAL_SEMANTICS_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Node surface snapshots expose size and tier state for host inspection.",
                    "GetNodeSurfaceSnapshots"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Composite scope proof keeps navigation and boundary-port feedback source-backed.",
                    "COMPOSITE_SCOPE_OK"),
            ],
            [
                "TIERED_NODE_SURFACE_OK",
                "NON_OBSCURING_EDITING_OK",
                "VISUAL_SEMANTICS_OK",
                "COMPOSITE_SCOPE_OK",
                "EDGE_GEOMETRY_OK",
            ],
            new DemoCookbookRouteClarity(
                "v0.78 spatial authoring route: IGraphEditorSession.Commands and Queries drive node surfaces, composite scopes, scope navigation, and connection route vertices.",
                "Supported seams live in `AsterGraph.Editor` spatial authoring commands/query snapshots and `AsterGraph.Avalonia` hosted projection controls.",
                "Demo cookbook points at proof cues only; Demo does not become the copyable spatial authoring implementation."),
            "Spatial authoring coverage is limited to existing session contracts, Demo proof cues, and editor tests; it does not add executable sample promises."),
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
            DemoCookbookRecipeCategory.ReviewHelp,
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
        new DemoCookbookRecipe(
            "v079-selection-rectangle-route",
            DemoCookbookRecipeCategory.Authoring,
            "v0.79 selection rectangle route",
            "Trace selection rectangle queries, marquee drag interaction, and bulk selection commands through the canonical session route.",
            [
                new DemoCookbookAnchor(
                    "Selection rectangle snapshot",
                    "src/AsterGraph.Editor/Runtime/GraphEditorSelectionRectangleSnapshot.cs",
                    "GraphEditorSelectionRectangleSnapshot"),
                new DemoCookbookAnchor(
                    "Selection rectangle query",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetSelectionRectangleSnapshot"),
                new DemoCookbookAnchor(
                    "Marquee overlay coordinator",
                    "src/AsterGraph.Avalonia/Controls/Internal/Overlay/NodeCanvasOverlayCoordinator.cs",
                    "UpdateMarqueeSelection"),
                new DemoCookbookAnchor(
                    "Bulk selection commands",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "SelectAll"),
            ],
            [
                new DemoCookbookAnchor(
                    "Selection rectangle query proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSelectionTransformContractsTests.cs",
                    "Queries_GetSelectionRectangleSnapshot_ReturnsNodesAndConnectionsInRectangle"),
                new DemoCookbookAnchor(
                    "Selection rectangle overlap proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSelectionTransformContractsTests.cs",
                    "Queries_GetSelectionRectangleSnapshot_WithPartialOverlap_ReturnsIntersectingNodesOnly"),
                new DemoCookbookAnchor(
                    "Marquee selection finalize proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasOverlayCoordinatorTests.cs",
                    "UpdateMarqueeSelection_WithFinalizeTrue_UsesBackendSelectionRectangleQuery"),
                new DemoCookbookAnchor(
                    "Bulk selection command proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorKernelCommandRouterTests.cs",
                    "GraphEditorKernel_SelectAllNoneInvert_ExecuteViaCommandInvocation"),
            ],
            [
                new DemoCookbookAnchor(
                    "v0.79 cookbook proof docs",
                    "docs/en/demo-cookbook.md",
                    "DEMO_COOKBOOK_V079_PROOF_DOCS_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Selection rectangle query returns intersecting nodes and connections.",
                    "GetSelectionRectangleSnapshot"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Marquee drag uses backend query on finalize and frontend hit-test during drag.",
                    "UpdateMarqueeSelection"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Select-all, select-none, and invert run through canonical command route.",
                    "SelectAll"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Selection rectangle proof markers back query and marquee behavior.",
                    "SELECTION_RECTANGLE_QUERY_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Marquee selection supports union and toggle modes through modifier keys.",
                    "UpdateMarqueeSelection"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Rectangle query keeps selection scoped to explicit bounds.",
                    "GetSelectionRectangleSnapshot"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "GetSelectionRectangleSnapshot exposes intersecting node and connection ids.",
                    "Queries_GetSelectionRectangleSnapshot_ReturnsNodesAndConnectionsInRectangle"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Bulk selection commands carry disabled-state recovery metadata.",
                    "GraphEditorKernel_SelectAllNoneInvert_ExecuteViaCommandInvocation"),
            ],
            [
                "SELECTION_RECTANGLE_QUERY_OK",
                "SELECTION_RECTANGLE_MARQUEE_OK",
                "SELECTION_INVERT_ALL_NONE_OK",
            ],
            new DemoCookbookRouteClarity(
                "v0.79 selection rectangle route: IGraphEditorSession.Queries.GetSelectionRectangleSnapshot(...) and INodeCanvasOverlayHost drag gestures feed the marquee selection surface.",
                "Supported seams live in `AsterGraph.Editor` query snapshots and `AsterGraph.Avalonia` overlay coordinator controls.",
                "Demo cookbook references selection evidence only; Demo does not claim another selection model or alternate hit-test path."),
            "Selection rectangle coverage is limited to existing session query contracts and Avalonia overlay controls; it does not add a spatial index, alternate selection model, or executable sample promise."),
        new DemoCookbookRecipe(
            "v079-keyboard-navigation-route",
            DemoCookbookRecipeCategory.Authoring,
            "v0.79 keyboard navigation route",
            "Trace arrow-key node nudge and nearest-node selection, viewport zoom/pan shortcuts, and automation peers for canvas accessibility.",
            [
                new DemoCookbookAnchor(
                    "Canvas key down handler",
                    "src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs",
                    "HandleCanvasKeyDown"),
                new DemoCookbookAnchor(
                    "Canvas arrow key handler",
                    "src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs",
                    "TryHandleCanvasArrowKey"),
                new DemoCookbookAnchor(
                    "Canvas automation peer",
                    "src/AsterGraph.Avalonia/Controls/Internal/Automation/NodeCanvasAutomationPeer.cs",
                    "NodeCanvasAutomationPeer"),
                new DemoCookbookAnchor(
                    "Node automation peer",
                    "src/AsterGraph.Avalonia/Controls/Internal/Automation/GraphNodeAutomationPeer.cs",
                    "GraphNodeAutomationPeer"),
                new DemoCookbookAnchor(
                    "Viewport zoom shortcuts",
                    "src/AsterGraph.Editor/Runtime/Internal/GraphEditorCommandDescriptorCatalog.cs",
                    "viewport.zoom-in"),
            ],
            [
                new DemoCookbookAnchor(
                    "Arrow key nudge proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs",
                    "ArrowKey_Nudge_MovesSelectedNodesWhenNodesAreSelected"),
                new DemoCookbookAnchor(
                    "Arrow key navigate proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs",
                    "ArrowKey_Navigate_SelectsNearestNodeWhenNoSelection"),
                new DemoCookbookAnchor(
                    "Automation peer surface proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs",
                    "DefaultChromeMode_ExposesCanvasAndNodeAutomationPeers"),
                new DemoCookbookAnchor(
                    "Canvas focus restore proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs",
                    "CanvasContextRequest_RestoresCanvasFocusForKeyboardRecovery"),
                new DemoCookbookAnchor(
                    "Viewport zoom pan command proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorKernelCommandRouterTests.cs",
                    "GraphEditorKernel_ViewportZoomPanCommands_ExecuteViaCommandInvocation"),
            ],
            [
                new DemoCookbookAnchor(
                    "v0.79 cookbook proof docs",
                    "docs/en/demo-cookbook.md",
                    "DEMO_COOKBOOK_V079_PROOF_DOCS_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Arrow keys nudge selected nodes by 10px (50px with Shift).",
                    "ArrowKey_Nudge_MovesSelectedNodesWhenNodesAreSelected"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Arrow keys select the nearest node when no selection exists.",
                    "ArrowKey_Navigate_SelectsNearestNodeWhenNoSelection"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Automation peers expose canvas and node titles to accessibility tools.",
                    "DefaultChromeMode_ExposesCanvasAndNodeAutomationPeers"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Keyboard navigation proof markers back nudge, navigation, and peer behavior.",
                    "CANVAS_KEYBOARD_NAVIGATION_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Arrow-key nudge moves selected nodes with shift for larger steps.",
                    "ArrowKey_Nudge_MovesSelectedNodesWhenNodesAreSelected"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Zoom and pan shortcuts keep viewport navigable without pointer.",
                    "viewport.zoom-in"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Automation peers expose node titles and canvas group structure.",
                    "DefaultChromeMode_ExposesCanvasAndNodeAutomationPeers"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Canvas focus restoration keeps keyboard routing intact after menus.",
                    "CanvasContextRequest_RestoresCanvasFocusForKeyboardRecovery"),
            ],
            [
                "CANVAS_KEYBOARD_NAVIGATION_OK",
                "ARROW_KEY_NUDGE_OK",
                "ARROW_KEY_NEAREST_NODE_OK",
                "AUTOMATION_PEER_SURFACE_OK",
            ],
            new DemoCookbookRouteClarity(
                "v0.79 keyboard navigation route: NodeCanvas arrow-key handling, command router viewport shortcuts, and Avalonia automation peers.",
                "Supported seams live in `AsterGraph.Avalonia` canvas controls and `AsterGraph.Editor` command descriptor/shortcut contracts.",
                "Demo cookbook references keyboard navigation evidence only; Demo does not add a separate input model or accessibility framework."),
            "Keyboard navigation coverage is limited to existing Avalonia canvas controls and command shortcut contracts; it does not add a custom input framework, full a11y provider suite, or executable sample promise."),
        new DemoCookbookRecipe(
            "v079-host-event-route",
            DemoCookbookRecipeCategory.DiagnosticsSupport,
            "v0.79 host event route",
            "Trace IGraphEditorEvents subscription surface, mutation batching for bounded event cadence, and memory-leak-free lifecycle.",
            [
                new DemoCookbookAnchor(
                    "Host event subscription surface",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorEvents.cs",
                    "IGraphEditorEvents"),
                new DemoCookbookAnchor(
                    "Mutation batching entry",
                    "src/AsterGraph.Editor/Runtime/Mutation/GraphEditorSessionMutation.cs",
                    "BeginMutation"),
                new DemoCookbookAnchor(
                    "Pending event flush",
                    "src/AsterGraph.Editor/Runtime/Mutation/GraphEditorSessionMutation.cs",
                    "FlushPendingEvents"),
                new DemoCookbookAnchor(
                    "Viewport change notification",
                    "src/AsterGraph.Editor/ViewModels/Facade/GraphEditorViewModel.Infrastructure.cs",
                    "NotifyViewportChanged"),
            ],
            [
                new DemoCookbookAnchor(
                    "Event memory leak proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs",
                    "SessionEvents_SubscribeAndUnsubscribe_DoNotLeakMemory"),
                new DemoCookbookAnchor(
                    "Viewport event cadence proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs",
                    "SessionEvents_ViewportChanges_AreThrottledToBoundedCadence"),
                new DemoCookbookAnchor(
                    "Selection event cadence proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs",
                    "SessionEvents_SelectionChanges_AreThrottledToBoundedCadence"),
                new DemoCookbookAnchor(
                    "Typed event args contract proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs",
                    "IGraphEditorEvents_ReusesExistingTypedEventArgs"),
            ],
            [
                new DemoCookbookAnchor(
                    "v0.79 cookbook proof docs",
                    "docs/en/demo-cookbook.md",
                    "DEMO_COOKBOOK_V079_PROOF_DOCS_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "IGraphEditorEvents exposes typed DocumentChanged, SelectionChanged, ViewportChanged, and CommandExecuted events.",
                    "IGraphEditorEvents"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Mutation batching provides bounded event cadence without time-based throttling.",
                    "EVENT_BATCHING_CADENCE_OK"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "Event subscription and unsubscription do not leak memory.",
                    "SessionEvents_SubscribeAndUnsubscribe_DoNotLeakMemory"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "IGraphEditorEvents exposes document, selection, viewport, and command event surfaces.",
                    "IGraphEditorEvents"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "FlushPendingEvents deduplicates and raises pending events when mutation scope ends.",
                    "FlushPendingEvents"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "CommandExecuted events keep command effects observable for support review.",
                    "IGraphEditorEvents_ReusesExistingTypedEventArgs"),
            ],
            [
                "HOST_EVENT_SUBSCRIPTION_OK",
                "EVENT_BATCHING_CADENCE_OK",
                "EVENT_MEMORY_LEAK_OK",
            ],
            new DemoCookbookRouteClarity(
                "v0.79 host event route: IGraphEditorEvents subscription surface with BeginMutation/FlushPendingEvents batching for bounded cadence.",
                "Supported seams live in `AsterGraph.Editor` session event contracts and mutation batching internals.",
                "Demo cookbook references event lifecycle evidence only; Demo does not add telemetry, remote sync, or a new event broker."),
            "Host event coverage is bounded to existing session event contracts and mutation batching; it does not add telemetry, remote sync, time-based throttling, or a separate event broker."),
    ];
}
