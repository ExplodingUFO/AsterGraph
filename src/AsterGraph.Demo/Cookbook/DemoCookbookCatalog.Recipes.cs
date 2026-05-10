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
                    "Demo Avalonia host entry",
                    "src/AsterGraph.Demo/Program.cs",
                    "BuildAvaloniaApp"),
                new DemoCookbookAnchor(
                    "Hosted builder factory",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphHostBuilder.cs",
                    "AsterGraphHostBuilder"),
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
                    "BuildAvaloniaApp"),
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
                    "BuildAvaloniaApp"),
            ],
            ["FIVE_MINUTE_ONBOARDING_OK"],
            new DemoCookbookRouteClarity(
                "Shipped Avalonia route: AsterGraphHostBuilder.Create(...).BuildAvaloniaView() via StarterAvaloniaAppBuilder.",
                "`AsterGraph.Avalonia` composes the hosted UI on top of `AsterGraph.Editor` session/runtime surfaces.",
                "Demo scenario launch is inspection/proof only; copy the starter host code instead of Demo ViewModel code."),
            "Avalonia is the shipped hosted route; WPF remains validation-only and Demo remains sample/proof surface.",
            CodeSample: """
            // Build a hosted Avalonia graph editor
            var builder = AsterGraphHostBuilder.Create()
                .WithCatalog(catalog)
                .WithStyle(style);

            // Compose the full editor view (shell + canvas + panels)
            var view = builder.BuildAvaloniaView();

            // Add it to your Window.Content
            var window = new Window { Content = view };
            window.Show();
            """
            ),
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
            ],
            """
            // Discover all available commands
            var registry = editor.Session.Queries.GetCommandRegistry();
            var descriptors = registry.GetDescriptors();

            // Run semantic delete and reconnect
            editor.Session.Commands.TryDeleteSelectionAndReconnect();

            // Apply a fragment template preset
            editor.Session.Commands.ApplyFragmentTemplatePreset(presetKey, dropPosition);

            // Move selected items via canonical command
            editor.Session.Commands.TryMoveSelectionBy(offsetX, offsetY);
            """
            ),
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
                new DemoCookbookAnchor(
                    "Edge presentation contract",
                    "src/AsterGraph.Core/Models/GraphEdgePresentation.cs",
                    "GraphEdgePresentation"),
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
                new DemoCookbookAnchor(
                    "Path variant edge presentation proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorConnectionGeometryContractsTests.cs",
                    "SessionQueries_GetConnectionGeometrySnapshots_ProjectPathVariantEdgePresentation"),
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
                    DemoCookbookScenarioKind.GraphOperations,
                    "Bezier, SmoothStep, Step, Straight, animated, floating, and marker variants are modeled through edge presentation metadata.",
                    "GraphEdgePresentation"),
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
                    DemoCookbookInteractionKind.Connection,
                    "Path variant edge metadata stays renderer-backed by shared geometry snapshots.",
                    "SessionQueries_GetConnectionGeometrySnapshots_ProjectPathVariantEdgePresentation"),
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
                "SessionQueries_GetConnectionGeometrySnapshots_ProjectPathVariantEdgePresentation",
            ],
            new DemoCookbookRouteClarity(
                "v0.78 spatial authoring route: IGraphEditorSession.Commands and Queries drive node surfaces, composite scopes, scope navigation, connection route vertices, and path variant edge presentation metadata.",
                "Supported seams live in `AsterGraph.Editor` spatial authoring commands/query snapshots and `AsterGraph.Avalonia` hosted projection controls.",
                "Demo cookbook points at proof cues only; Demo does not become the copyable spatial authoring implementation."),
            "Spatial authoring coverage is limited to existing session contracts, Demo proof cues, editor tests, and stock renderer evidence; it does not add executable sample promises.",
            CodeSample: """
            // Resize a node surface
            editor.Session.Commands.TrySetNodeSize(nodeId, width, height);

            // Query node surface snapshots
            var surfaces = editor.Session.Queries.GetNodeSurfaceSnapshots();

            // Navigate into a composite scope
            editor.Session.Commands.TryEnterScope(nodeId);

            // Navigate back to parent scope
            editor.Session.Commands.TryExitScope();

            // Persist an edge path/marker variant
            var presentation = new GraphEdgePresentation(
                PathKind: GraphEdgePathKind.SmoothStep,
                IsAnimated: true,
                UsesFloatingEndpoints: true,
                TargetMarker: GraphEdgeMarkerKind.ArrowClosed);
            """
            ),
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
                    "Scale Baseline keeps baseline, large, and stress budgets tied to release evidence.",
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
                    "Editor tests keep minimap projection and inspector evidence bounded.",
                    "MINIMAP_LIGHTWEIGHT_PROJECTION_OK"),
            ],
            ["MINIMAP_LIGHTWEIGHT_PROJECTION_OK", "PROJECTION_PERFORMANCE_EVIDENCE_OK", "SCALE_PERFORMANCE_BUDGET_OK", "LAYOUT_PROVIDER_SEAM_OK", "LAYOUT_PREVIEW_APPLY_CANCEL_OK", "LAYOUT_UNDO_TRANSACTION_OK"],
            new DemoCookbookRouteClarity(
                "Performance route: ViewportVisibleSceneProjector.Project(...) plus AsterGraphWorkbenchPerformancePolicy.FromMode(...) and IGraphEditorSession layout preview/apply and snap commands.",
                "Supported seams live in `AsterGraph.Editor` viewport projection, layout command contracts, and `AsterGraph.Avalonia` hosted workbench policy contracts.",
                "Demo cookbook projection is a graph-above-code teaching surface only; Editor tests remain the proof sources."),
            "Performance and layout proof reports projection, layout command, and budget evidence; it does not add a background graph index, second renderer, or runtime execution mode.",
            CodeSample: """
            // Project visible scene with budget limits
            var projection = editor.Session.Queries.GetViewportSnapshot();
            var budgetMarker = AsterGraphWorkbenchPerformancePolicy
                .ToMiniMapBudgetMarker(projection);

            // Configure large-graph performance policy
            var policy = new WorkbenchPerformancePolicy
            {
                MaxVisibleNodes = 500,
                MinimapUpdateCadenceMs = 100,
            };
            editor.Session.ApplyPerformancePolicy(policy);
            """
            ),
        new DemoCookbookRecipe(
            "groups-subgraphs-route",
            DemoCookbookRecipeCategory.GroupsSubgraphs,
            "Groups and subgraphs route",
            "Trace persisted group state, composite child scopes, collapsed projection, and boundary-edge proof.",
            [
                new DemoCookbookAnchor(
                    "Collapsed group serialization snapshot",
                    "tests/AsterGraph.Editor.Tests/SerializationCompatibilityTests.cs",
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
            "Group/subgraph proof is limited to persisted snapshots, hierarchy queries, and stock Avalonia projection; nested group mutation and specialized boundary-edge styling remain deferred.",
            CodeSample: """
            // Create a group from selected nodes
            editor.Session.Commands.TryCreateGroupFromSelection();

            // Toggle collapsed state
            editor.Session.Commands.TryToggleGroupCollapse(groupId);

            // Query hierarchy state with collapsed boundaries
            var hierarchy = editor.Session.Queries.GetHierarchyStateSnapshot();
            var boundaryEdges = hierarchy.BoundaryEdges;
            """
            ),
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
                    "Demo plugin command surface",
                    "src/AsterGraph.Demo/Integration/DemoPluginShowcase.cs",
                    "GetCommandDescriptors"),
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
                    "The Demo proof panel evidence backs the trust route.",
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
                    "GetCommandDescriptors"),
            ],
            ["PLUGIN_TRUST_EVIDENCE_PANEL_OK"],
            new DemoCookbookRouteClarity(
                "Plugin route: AsterGraphEditorFactory.DiscoverPluginCandidates(...) with host-owned PluginTrustPolicy before loading.",
                "Supported APIs live in `AsterGraph.Editor` plugin discovery, trust, and registration contracts.",
                "Demo trust workspace is an evidence surface only; it does not sandbox or isolate untrusted plugin code."),
            "Plugins are trusted in-process extensions; the recipe does not imply sandboxing or untrusted-code isolation.",
            CodeSample: """
            // Discover plugin candidates in a folder
            var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(pluginsPath);

            // Stage a plugin package for loading
            var package = AsterGraphEditorFactory.StagePluginPackage(candidate);

            // Validate manifest against allowlist
            var manifest = package.ReadManifest();
            var isTrusted = allowlist.Contains(manifest.Id);
            """
            ),
        new DemoCookbookRecipe(
            "diagnostics-support-route",
            DemoCookbookRecipeCategory.DiagnosticsSupport,
            "Diagnostics and support route",
            "Capture support-bundle and runtime diagnostics evidence from Demo runtime projections.",
            [
                new DemoCookbookAnchor(
                    "Runtime overlay option seam",
                    "src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs",
                    "RuntimeOverlayProvider"),
                new DemoCookbookAnchor(
                    "Demo runtime query path",
                    "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.AiPipelineMockRunner.cs",
                    "GetRuntimeOverlaySnapshot"),
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
                    "RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK"),
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
                    "RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK"),
            ],
            ["RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK"],
            new DemoCookbookRouteClarity(
                "Runtime diagnostics route: AsterGraphEditorOptions.RuntimeOverlayProvider plus IGraphEditorSession.Queries.GetRuntimeOverlaySnapshot().",
                "Supported APIs live in `AsterGraph.Editor` runtime overlay/query contracts.",
                "Demo runtime timeline is a local projection only; it does not add telemetry or remote sync."),
            "Support bundles are local handoff evidence, not telemetry, remote sync, or support-scope expansion.",
            CodeSample: """
            // Configure runtime overlay provider
            var options = new AsterGraphEditorOptions
            {
                RuntimeOverlayProvider = () => new DemoRuntimeOverlay(),
            };

            // Capture a runtime snapshot
            var snapshot = host.GetRuntimeOverlaySnapshot();

            // Emit a support bundle
            var bundle = SupportBundle.Create(snapshot);
            bundle.WriteToFile("support-bundle.json");
            """
            ),
        new DemoCookbookRecipe(
            "review-help-route",
            DemoCookbookRecipeCategory.ReviewHelp,
            "Review and help route",
            "Trace validation repair, contextual help, and review-loop proof markers from existing support-bundle evidence.",
            [
                new DemoCookbookAnchor(
                    "Validation snapshot query",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs",
                    "GetValidationSnapshot"),
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
                    "REPAIR_HELP_REVIEW_LOOP_OK"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "The repair/help proof marker closes the review-loop scenario.",
                    "REPAIR_HELP_REVIEW_LOOP_OK"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "Demo proof code is the host-facing example for repair/help handoff.",
                    "REPAIR_HELP_REVIEW_LOOP_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Validation feedback points repair actions at concrete graph issues.",
                    "REPAIR_HELP_REVIEW_LOOP_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Proof panels expose review/help evidence without adding a workflow engine.",
                    "DemoHostMenuGroups.Proof"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Repair/help proof covers connection repair handoff evidence.",
                    "REPAIR_HELP_REVIEW_LOOP_OK"),
            ],
            ["REPAIR_HELP_REVIEW_LOOP_OK"],
            new DemoCookbookRouteClarity(
                "Review/help route: IGraphEditorSession validation feedback.",
                "Supported seams stay in `AsterGraph.Editor` session validation, repair, and support evidence contracts.",
                "Demo proof panels are review evidence only; they do not add a workflow engine or macro scheduler."),
            "Review/help evidence stays bounded to existing validation and support-bundle proof; it is not a new workflow engine.",
            CodeSample: """
            // Run validation and collect feedback
            var feedback = editor.Session.Diagnostics.Validate();

            // Trigger contextual help for selected node
            var help = editor.Session.Queries.GetContextualHelp(selectedNodeId);

            // Review-loop: export validation report
            var report = new ValidationReport(feedback);
            report.Export("validation-report.md");
            """
            ),
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
            "Selection rectangle coverage is limited to existing session query contracts and Avalonia overlay controls; it does not add a spatial index, alternate selection model, or executable sample promise.",
            CodeSample: """
            // Get the current selection rectangle snapshot
            var rect = editor.Session.Queries.GetSelectionRectangleSnapshot();

            // Select all nodes
            editor.Session.Commands.SelectAll();

            // Invert current selection
            editor.Session.Commands.InvertSelection();

            // Clear selection
            editor.Session.Commands.SelectNone();
            """
            ),
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
            "Keyboard navigation coverage is limited to existing Avalonia canvas controls and command shortcut contracts; it does not add a custom input framework, full a11y provider suite, or executable sample promise.",
            CodeSample: """
            // Handle arrow-key nudge (10px default, 50px with Shift)
            editor.Session.Commands.TryMoveSelectionBy(10, 0); // right

            // Zoom in/out via command
            editor.Session.Commands.ZoomIn();
            editor.Session.Commands.ZoomOut();

            // Pan viewport
            editor.Session.Commands.PanBy(100, 0); // right

            // Automation peer for accessibility testing
            var peer = new NodeCanvasAutomationPeer(canvas);
            var children = peer.GetChildren();
            """
            ),
        new DemoCookbookRecipe(
            "v079-host-event-route",
            DemoCookbookRecipeCategory.ReviewHelp,
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
            "Host event coverage is bounded to existing session event contracts and mutation batching; it does not add telemetry, remote sync, time-based throttling, or a separate event broker.",
            CodeSample: """
            // Subscribe to editor events
            editor.Events.SelectionChanged += (s, e) =>
                Console.WriteLine($"Selected {e.NodeIds.Count} nodes");

            editor.Events.ViewportChanged += (s, e) =>
                Console.WriteLine($"Zoom: {e.Zoom}, Pan: {e.Pan}");

            // Batch mutations for bounded event cadence
            using (editor.Session.BeginMutation())
            {
                editor.Session.Commands.AddNode(node);
                editor.Session.Commands.AddNode(node2);
                editor.Session.Commands.Connect(output, input);
            }
            // Events flushed when mutation scope exits
            """
            ),
        new DemoCookbookRecipe(
            "interaction-selection-marquee-route",
            DemoCookbookRecipeCategory.Authoring,
            "Interaction selection marquee route",
            "Launch a dedicated fixture for rectangle selection, bulk selection commands, and selected-route inspection.",
            [
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
                    "Selection marquee graph fixture",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "selection-marquee-workbench"),
                new DemoCookbookAnchor(
                    "Selection rectangle query proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSelectionTransformContractsTests.cs",
                    "Queries_GetSelectionRectangleSnapshot_ReturnsNodesAndConnectionsInRectangle"),
                new DemoCookbookAnchor(
                    "Marquee selection finalize proof",
                    "tests/AsterGraph.Editor.Tests/NodeCanvasOverlayCoordinatorTests.cs",
                    "UpdateMarqueeSelection_WithFinalizeTrue_UsesBackendSelectionRectangleQuery"),
            ],
            [
                new DemoCookbookAnchor(
                    "Interaction fixture cookbook docs",
                    "docs/en/demo-cookbook.md",
                    "INTERACTION_SELECTION_MARQUEE_FIXTURE_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The selection-marquee-workbench fixture spreads nodes across a rectangle so multi-select captures a distinct scene.",
                    "selection-marquee-workbench"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Selection rectangle query returns intersecting nodes and connections.",
                    "GetSelectionRectangleSnapshot"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Marquee finalize proof keeps the route tied to the backend query path.",
                    "UpdateMarqueeSelection_WithFinalizeTrue_UsesBackendSelectionRectangleQuery"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "The runnable fixture highlights rectangle-sized selection and selected-route inspection.",
                    "selection-marquee-workbench"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "A marked SmoothStep route keeps selected connection inspection visible in screenshots.",
                    "INTERACTION_SELECTION_MARQUEE_FIXTURE_OK"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "GetSelectionRectangleSnapshot exposes the selected node and connection ids.",
                    "GetSelectionRectangleSnapshot"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Bulk selection commands retain the existing command route and proof markers.",
                    "SelectAll"),
            ],
            [
                "INTERACTION_SELECTION_MARQUEE_FIXTURE_OK",
                "SELECTION_RECTANGLE_QUERY_OK",
                "SELECTION_RECTANGLE_MARQUEE_OK",
            ],
            new DemoCookbookRouteClarity(
                "Interaction selection marquee route: launch `selection-marquee-workbench` and inspect GetSelectionRectangleSnapshot plus marquee-selection proof evidence.",
                "Supported seams live in `AsterGraph.Editor` selection query/command contracts and `AsterGraph.Avalonia` overlay coordinator controls.",
                "Demo cookbook provides a rectangular fixture and screenshot-gate coverage only; Demo does not add another selection model or hit-test path."),
            "Selection marquee fixture coverage is limited to existing selection query, command, and overlay contracts; it does not add a spatial index, alternate selection model, or custom hit-test runtime.",
            CodeSample: """
            // Inspect the current rectangle-selection state.
            var rectangle = editor.Session.Queries.GetSelectionRectangleSnapshot();

            // Keep bulk selection on the canonical command route.
            editor.Session.Commands.SelectAll();
            editor.Session.Commands.InvertSelection();
            editor.Session.Commands.SelectNone();
            """
            ),
        new DemoCookbookRecipe(
            "interaction-keyboard-navigation-route",
            DemoCookbookRecipeCategory.Authoring,
            "Interaction keyboard navigation route",
            "Launch a spatial fixture for arrow-key nudge, nearest-node navigation, viewport shortcuts, and automation peer inspection.",
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
                    "Viewport zoom shortcuts",
                    "src/AsterGraph.Editor/Runtime/Internal/GraphEditorCommandDescriptorCatalog.cs",
                    "viewport.zoom-in"),
            ],
            [
                new DemoCookbookAnchor(
                    "Keyboard navigation graph fixture",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "keyboard-navigation-lab"),
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
            ],
            [
                new DemoCookbookAnchor(
                    "Interaction fixture cookbook docs",
                    "docs/en/demo-cookbook.md",
                    "INTERACTION_KEYBOARD_NAVIGATION_FIXTURE_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The keyboard-navigation-lab fixture arranges nearby nodes for nearest-node arrow navigation.",
                    "keyboard-navigation-lab"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Automation peers expose canvas and node titles to accessibility tools.",
                    "DefaultChromeMode_ExposesCanvasAndNodeAutomationPeers"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Arrow-key proof markers back nudge and nearest-node behavior.",
                    "CANVAS_KEYBOARD_NAVIGATION_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Arrow-key nudge moves selected nodes while nearest-node navigation selects a target when no selection exists.",
                    "ArrowKey_Nudge_MovesSelectedNodesWhenNodesAreSelected"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "The fixture keeps directional targets visible around a center node for focus recovery.",
                    "keyboard-navigation-lab"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Automation peers expose node titles and canvas group structure.",
                    "DefaultChromeMode_ExposesCanvasAndNodeAutomationPeers"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Viewport shortcuts remain command-descriptor backed instead of Demo-local.",
                    "viewport.zoom-in"),
            ],
            [
                "INTERACTION_KEYBOARD_NAVIGATION_FIXTURE_OK",
                "CANVAS_KEYBOARD_NAVIGATION_OK",
                "ARROW_KEY_NEAREST_NODE_OK",
            ],
            new DemoCookbookRouteClarity(
                "Interaction keyboard navigation route: launch `keyboard-navigation-lab` and inspect NodeCanvas arrow-key, viewport shortcut, and automation peer evidence.",
                "Supported seams live in `AsterGraph.Avalonia` canvas controls and `AsterGraph.Editor` command descriptor/shortcut contracts.",
                "Demo cookbook provides a spatial fixture and screenshot-gate coverage only; Demo does not add a separate input model or accessibility framework."),
            "Keyboard navigation fixture coverage is limited to existing Avalonia canvas controls, automation peers, and command shortcut contracts; it does not add a custom input framework or full accessibility provider suite.",
            CodeSample: """
            // Let the stock canvas route arrow keys to selection or focus.
            editor.Session.Commands.TryMoveSelectionBy(10, 0);

            // Keep viewport shortcuts command-backed.
            editor.Session.Commands.ZoomIn();
            editor.Session.Commands.PanBy(100, 0);
            """
            ),
        new DemoCookbookRecipe(
            "interaction-host-event-inspector-route",
            DemoCookbookRecipeCategory.ReviewHelp,
            "Interaction host event inspector route",
            "Launch a host-observable fixture for selection, viewport, and mutation event evidence.",
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
            ],
            [
                new DemoCookbookAnchor(
                    "Host event inspector graph fixture",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "host-event-inspector"),
                new DemoCookbookAnchor(
                    "Event memory leak proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs",
                    "SessionEvents_SubscribeAndUnsubscribe_DoNotLeakMemory"),
                new DemoCookbookAnchor(
                    "Selection event cadence proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs",
                    "SessionEvents_SelectionChanges_AreThrottledToBoundedCadence"),
            ],
            [
                new DemoCookbookAnchor(
                    "Interaction fixture cookbook docs",
                    "docs/en/demo-cookbook.md",
                    "INTERACTION_HOST_EVENT_INSPECTOR_FIXTURE_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The host-event-inspector fixture keeps event-producing selection and mutation routes visible.",
                    "host-event-inspector"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "IGraphEditorEvents exposes typed document, selection, viewport, and command events.",
                    "IGraphEditorEvents"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Mutation batching provides bounded event cadence without time-based throttling.",
                    "EVENT_BATCHING_CADENCE_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Selection event cadence stays bounded for host observers.",
                    "SessionEvents_SelectionChanges_AreThrottledToBoundedCadence"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Connection,
                    "Command and mutation events keep graph changes observable for support review.",
                    "IGraphEditorEvents"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "The fixture makes event source nodes and host payload output inspectable.",
                    "host-event-inspector"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "FlushPendingEvents deduplicates and raises pending events when mutation scope ends.",
                    "FlushPendingEvents"),
            ],
            [
                "INTERACTION_HOST_EVENT_INSPECTOR_FIXTURE_OK",
                "HOST_EVENT_SUBSCRIPTION_OK",
                "EVENT_BATCHING_CADENCE_OK",
            ],
            new DemoCookbookRouteClarity(
                "Interaction host event inspector route: launch `host-event-inspector` and inspect IGraphEditorEvents plus mutation batching evidence.",
                "Supported seams live in `AsterGraph.Editor` session event contracts and mutation batching internals.",
                "Demo cookbook provides a host-observable fixture and screenshot-gate coverage only; Demo does not add telemetry, remote sync, or a new event broker."),
            "Host event fixture coverage is bounded to existing session event contracts and mutation batching; it does not add telemetry, remote sync, time-based throttling, or a separate event broker.",
            CodeSample: """
            // Subscribe to stock editor events from the host.
            editor.Events.SelectionChanged += (_, args) =>
                Console.WriteLine(args.NodeIds.Count);

            // Batch mutations for bounded event cadence.
            using (editor.Session.BeginMutation())
            {
                editor.Session.Commands.SelectAll();
                editor.Session.Commands.TryExecuteCommand("viewport.zoom-in");
            }
            """
            ),
        new DemoCookbookRecipe(
            "builtin-minimap-workbench-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "Built-in MiniMap workbench route",
            "Capture the stock MiniMap, performance policy, and wide graph overview as a runnable Cookbook fixture.",
            [
                new DemoCookbookAnchor(
                    "Standalone MiniMap factory",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphMiniMapViewFactory.cs",
                    "AsterGraphMiniMapViewFactory"),
                new DemoCookbookAnchor(
                    "MiniMap cadence budget marker",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphWorkbenchPerformancePolicy.cs",
                    "ToMiniMapBudgetMarker"),
                new DemoCookbookAnchor(
                    "MiniMap lightweight stock surface",
                    "src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs",
                    "UsesLightweightProjection"),
            ],
            [
                new DemoCookbookAnchor(
                    "MiniMap workbench graph fixture",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "minimap-workbench"),
                new DemoCookbookAnchor(
                    "MiniMap performance policy proof",
                    "tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs",
                    "WorkbenchPerformancePolicy_ExposesMiniMapCadenceBudgetMarker"),
                new DemoCookbookAnchor(
                    "Demo MiniMap host proof",
                    "tests/AsterGraph.Demo.Tests/DemoCapabilityShowcaseTests.cs",
                    "PART_StandaloneMiniMapHost"),
            ],
            [
                new DemoCookbookAnchor(
                    "Built-in MiniMap cookbook docs",
                    "docs/en/demo-cookbook.md",
                    "BUILTIN_MINIMAP_WORKBENCH_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "Standalone hosts compose the stock MiniMap through the supported factory.",
                    "AsterGraphMiniMapViewFactory"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The minimap-workbench fixture spreads connected nodes across a wide canvas for overview capture.",
                    "minimap-workbench"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "MiniMap cadence and lightweight projection stay bounded by performance policy markers.",
                    "ToMiniMapBudgetMarker"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Existing MiniMap tests and Demo host proof remain the support evidence for this route.",
                    "MINIMAP_LIGHTWEIGHT_PROJECTION_OK"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "The wide fixture makes the MiniMap overview and world-bounds projection visible.",
                    "minimap-workbench"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Standalone MiniMap composition remains inspectable through the public factory.",
                    "AsterGraphMiniMapViewFactory"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Cadence markers explain when the hosted workbench uses lightweight projection.",
                    "ToMiniMapBudgetMarker"),
            ],
            [
                "MINIMAP_LIGHTWEIGHT_PROJECTION_OK",
                "BUILTIN_MINIMAP_WORKBENCH_OK",
            ],
            new DemoCookbookRouteClarity(
                "Built-in MiniMap workbench route: launch `minimap-workbench` and inspect AsterGraphMiniMapViewFactory plus hosted performance policy evidence.",
                "Supported seams live in `AsterGraph.Avalonia` MiniMap factories, stock GraphMiniMap projection, and AsterGraphWorkbenchPerformancePolicy budget markers.",
                "Demo cookbook provides a wide fixture and screenshot-gate coverage only; Demo does not add a second viewport model or graph index."),
            "MiniMap coverage is limited to the existing standalone factory, hosted workbench policy, and screenshot fixture; it does not add a background graph index, second renderer, or runtime execution mode.",
            CodeSample: """
            // Compose the stock standalone MiniMap beside a hosted editor.
            var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
            {
                Session = editor.Session,
            });

            // Keep cadence policy host-visible for large graphs.
            var policy = AsterGraphWorkbenchPerformancePolicy.FromMode(AsterGraphWorkbenchPerformanceMode.Balanced);
            var marker = policy.ToMiniMapBudgetMarker();
            """
            ),
        new DemoCookbookRecipe(
            "builtin-background-grid-route",
            DemoCookbookRecipeCategory.PerformanceViewport,
            "Built-in Background/Grid route",
            "Capture the stock grid background, grid-density bounds, and snap-aware fixture as a runnable Cookbook route.",
            [
                new DemoCookbookAnchor(
                    "Grid background renderer",
                    "src/AsterGraph.Avalonia/Controls/GridBackground.cs",
                    "CalculateVisibleLineMetrics"),
                new DemoCookbookAnchor(
                    "Canvas grid style",
                    "src/AsterGraph.Abstractions/Styling/CanvasStyleOptions.cs",
                    "GridBackgroundHex"),
                new DemoCookbookAnchor(
                    "Snap-to-grid command seam",
                    "src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs",
                    "TrySnapSelectedNodesToGrid"),
            ],
            [
                new DemoCookbookAnchor(
                    "Background grid graph fixture",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "background-grid-density"),
                new DemoCookbookAnchor(
                    "Grid density proof",
                    "tests/AsterGraph.Editor.Tests/GridBackgroundTests.cs",
                    "CalculateVisibleLineMetrics_WithExtremeZoomSpacing_KeepsLineDensityBounded"),
                new DemoCookbookAnchor(
                    "Layout action projection proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs",
                    "AuthoringToolsChrome_ProjectsStockSelectionLayoutActions"),
            ],
            [
                new DemoCookbookAnchor(
                    "Built-in grid cookbook docs",
                    "docs/en/demo-cookbook.md",
                    "GRID_BACKGROUND_DENSITY_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The background-grid-density fixture keeps nodes aligned to a readable grid cadence.",
                    "background-grid-density"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Canvas styling keeps the stock grid background color and density configurable through existing style options.",
                    "GridBackgroundHex"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "Grid line metrics stay bounded at extreme zoom levels.",
                    "CalculateVisibleLineMetrics_WithExtremeZoomSpacing_KeepsLineDensityBounded"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.SupportEvidence,
                    "Snap-to-grid and layout action projection stay on shared session command seams.",
                    "TrySnapSelectedNodesToGrid"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Snap commands keep selected nodes aligned to the visible grid.",
                    "TrySnapSelectedNodesToGrid"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.LayoutReadability,
                    "Grid line metrics bound visual density instead of letting zoom create unreadable line counts.",
                    "CalculateVisibleLineMetrics"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Hosted layout actions project stock alignment and snap controls with disabled-state feedback.",
                    "AuthoringToolsChrome_ProjectsStockSelectionLayoutActions"),
            ],
            [
                "GRID_BACKGROUND_DENSITY_OK",
                "LAYOUT_PROVIDER_SEAM_OK",
            ],
            new DemoCookbookRouteClarity(
                "Built-in Background/Grid route: launch `background-grid-density` and inspect GridBackground plus snap-to-grid command evidence.",
                "Supported seams live in `AsterGraph.Avalonia` GridBackground rendering, AsterGraph style options, and `AsterGraph.Editor` layout commands.",
                "Demo cookbook provides a snapped graph fixture and screenshot-gate coverage only; Demo does not add another renderer or layout runtime."),
            "Background/grid coverage is limited to existing grid rendering, style options, and snap/layout commands; it does not add a second renderer, background graph index, or new layout engine.",
            CodeSample: """
            // Read the stock grid style used by the canvas.
            var style = GraphEditorStyleOptions.Default.Canvas;
            var gridBackground = style.GridBackgroundHex;

            // Keep node placement aligned through the session command seam.
            editor.Session.Commands.TrySnapSelectedNodesToGrid(20);
            """
            ),
        new DemoCookbookRecipe(
            "builtin-hosted-controls-route",
            DemoCookbookRecipeCategory.Authoring,
            "Built-in hosted Controls/Panel route",
            "Capture hosted workbench controls, panel-state composition, and recovery hints through a runnable Cookbook fixture.",
            [
                new DemoCookbookAnchor(
                    "Default workbench composition",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphHostBuilder.cs",
                    "UseDefaultWorkbench"),
                new DemoCookbookAnchor(
                    "Hosted Avalonia view composition",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphHostBuilder.cs",
                    "BuildAvaloniaView"),
                new DemoCookbookAnchor(
                    "Workbench panel state",
                    "src/AsterGraph.Avalonia/Hosting/AsterGraphWorkbenchOptions.cs",
                    "AsterGraphWorkbenchPanelState"),
                new DemoCookbookAnchor(
                    "Command recovery hint",
                    "src/AsterGraph.Editor/Runtime/GraphEditorCommandDescriptorSnapshot.cs",
                    "RecoveryHint"),
            ],
            [
                new DemoCookbookAnchor(
                    "Hosted controls graph fixture",
                    "src/AsterGraph.Demo/DemoGraphFactory.cs",
                    "hosted-controls-panel"),
                new DemoCookbookAnchor(
                    "Hosted layout tools proof",
                    "tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs",
                    "AuthoringToolsChrome_ProjectsStockSelectionLayoutActions"),
                new DemoCookbookAnchor(
                    "Demo hosted panel proof",
                    "tests/AsterGraph.Demo.Tests/DemoCookbookNavigationTests.cs",
                    "PART_CookbookWorkspaceNavigationPanel"),
            ],
            [
                new DemoCookbookAnchor(
                    "Built-in hosted controls cookbook docs",
                    "docs/en/demo-cookbook.md",
                    "HOSTED_CONTROLS_PANEL_COMPOSITION_OK"),
            ],
            [
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.HostCodeExample,
                    "UseDefaultWorkbench composes the stock toolbar, command palette, panels, MiniMap, diagnostics, and status chrome.",
                    "UseDefaultWorkbench"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.GraphOperations,
                    "The hosted-controls-panel fixture keeps a panel-sized authoring graph ready for screenshot capture.",
                    "hosted-controls-panel"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.NodeMetadata,
                    "Panel state stays host-owned through AsterGraphWorkbenchPanelState.",
                    "AsterGraphWorkbenchPanelState"),
                new DemoCookbookScenarioPoint(
                    DemoCookbookScenarioKind.ValidationRuntimeOverlay,
                    "RecoveryHint keeps disabled hosted controls explainable without Demo-only fallbacks.",
                    "RecoveryHint"),
            ],
            [
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Selection,
                    "Hosted layout tools project selection actions from stock command descriptors.",
                    "AuthoringToolsChrome_ProjectsStockSelectionLayoutActions"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.Inspection,
                    "Workbench panel visibility is controlled through the hosted panel-state options.",
                    "AsterGraphWorkbenchPanelState"),
                new DemoCookbookInteractionFacet(
                    DemoCookbookInteractionKind.ValidationRuntimeFeedback,
                    "Recovery hints keep disabled commands supportable from the hosted control surface.",
                    "RecoveryHint"),
            ],
            [
                "HOSTED_CONTROLS_PANEL_COMPOSITION_OK",
                "DESIGNER_WORKBENCH_AUTHORING_OK",
            ],
            new DemoCookbookRouteClarity(
                "Built-in hosted Controls/Panel route: launch `hosted-controls-panel` and inspect UseDefaultWorkbench, panel-state, and RecoveryHint evidence.",
                "Supported seams live in `AsterGraph.Avalonia` hosted workbench options and `AsterGraph.Editor` command descriptor recovery metadata.",
                "Demo cookbook provides a composed panel fixture and screenshot-gate coverage only; Demo does not add a new workflow engine or control framework."),
            "Hosted controls coverage is limited to existing workbench options, panel state, and command descriptor recovery metadata; it does not add telemetry, remote sync, a workflow engine, or a new control framework.",
            CodeSample: """
            // Compose stock hosted controls when the default workbench is enough.
            var view = AsterGraphHostBuilder.Create()
                .UseDocument(document)
                .UseCatalog(catalog)
                .UseDefaultWorkbench()
                .BuildAvaloniaView();

            // Hosts can still tune panel visibility through workbench options.
            var panels = AsterGraphWorkbenchPanelState.Default;
            """
            ),
    ];
}
