# Phase 470 Summary: Cookbook Component Showcase

## Outcome

Phase 470 turns the Avalonia Demo cookbook into a code-plus-demo index for the completed v0.78 component-platform surfaces. The cookbook now links rendering/viewport proof, customization proof, and spatial authoring proof to source code anchors, demo cues, documentation anchors, interaction facets, and support-boundary text.

## Beads

- Parent: `avalonia-node-map-v78.5`
- Completed children:
  - `avalonia-node-map-v78.5.1` — cookbook v0.78 recipe catalog coverage
  - `avalonia-node-map-v78.5.2` — cookbook workspace projection and UI detail polish
  - `avalonia-node-map-v78.5.3` — cookbook proof docs and support boundaries
  - `avalonia-node-map-v78.5.4` — synthesis and verification

## Implemented Surface

### Rendering And Viewport Route

- Recipe: `v078-rendering-viewport-route`
- Source anchors: `GetSceneSnapshot`, `GetViewportSnapshot`, `ToBudgetMarker`, `ApplyVisibleSceneBudget`
- Demo/test proof: visible-scene connection budget, viewport invalidation diff, and `VIEWPORT_LOD_POLICY_OK`
- Boundary: proof over existing scene and viewport contracts, not a renderer rewrite or virtualizer promise

### Customization Route

- Recipe: `v078-customization-route`
- Source anchors: `CreatePresentationOptions`, `CreateEdgeOverlay`, `ConnectionGeometries`, `GetConnectionGeometrySnapshots`, `NodeVisualPresenter`
- Demo/docs proof: ConsumerSample custom presenter/editor registry and custom extension proof markers
- Boundary: ConsumerSample remains the copyable customization recipe; Demo remains visual proof

### Spatial Authoring Route

- Recipe: `v078-spatial-authoring-route`
- Source anchors: `GetNodeSurfaceSnapshots`, `TrySetNodeSize`, `TryWrapSelectionToComposite`, `TryExposeCompositePort`, `GetCompositeNodeSnapshots`, `GetScopeNavigationSnapshot`, `TryInsertConnectionRouteVertex`, `GetConnectionGeometrySnapshots`
- Demo/test proof: tiered node surface cues, composite scope cues, route geometry cues, and editor contract tests
- Boundary: existing session commands/query snapshots remain the supported route; Demo does not become the spatial authoring implementation

## Changed Files

- `src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.Recipes.cs`
- `src/AsterGraph.Demo/ViewModels/DemoCookbookWorkspaceProjection.cs`
- `src/AsterGraph.Demo/ViewModels/DemoCookbookWorkspaceProjectionFactory.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Cookbook.cs`
- `docs/en/demo-cookbook.md`
- `docs/zh-CN/demo-cookbook.md`
- `tests/AsterGraph.Demo.Tests/DemoCookbookCatalogTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoCookbookV078CatalogTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoCookbookDocsTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoCookbookV078DocsTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoCookbookProofClosureTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoCookbookWorkspaceProjectionTests.cs`
- `tests/AsterGraph.Demo.Tests/MainWindowViewModelCookbookTests.cs`

## Verification

- `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj --filter Cookbook -m:1 --logger "console;verbosity=minimal"` — passed, 64 tests
- `git diff --check` — passed; only repository line-ending warnings were reported

## Result

`COOK-02` is satisfied for v0.78 planning purposes. The cookbook now presents the component platform as recipes backed by code, graph/demo cues, documentation, and support boundaries, while keeping Demo as a proof/sample surface.
