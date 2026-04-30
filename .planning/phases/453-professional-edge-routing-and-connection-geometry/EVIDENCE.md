# Phase 453 Evidence

## Contract

- `GraphEditorConnectionGeometrySnapshot` now carries `RouteStyle` and `RoutingEvidence` beside the existing source anchor, target anchor, and persisted route.
- Empty routes remain `Bezier` and preserve existing pending/committed edge behavior.
- Persisted bend-point routes project as `Orthogonal`; `ConnectionPathBuilder` expands them into deterministic axis-aligned dog-leg points and renders each leg as a straight cubic segment.
- `GraphEditorConnectionRouteEvidenceSnapshot` reports bounded evidence only: routed polyline points, intersected non-endpoint node ids, and a crossing count against other projected connection polylines. It does not rewrite the runtime graph model or introduce an automatic router.
- Crossing evidence excludes connections that share either endpoint node, so fan-out/fan-in edges are not counted as crossings just because they meet at a node anchor.
- `NodeCanvasConnectionSceneRenderer` renders committed connections with the projected route style and keeps pending previews as Bezier.

## Verification

- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "GraphEditorConnectionGeometryContractsTests|NodeCanvasConnectionSceneRendererTests"`: passed, 18 tests after shared-endpoint crossing guard.
- `dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --no-restore`: passed, 682 tests.
- `.\eng\validate-public-api-surface.ps1 -Configuration Debug`: failed before baseline/docs update due intentional public API drift.
- `.\eng\validate-public-api-surface.ps1 -Configuration Debug -UpdateBaseline`: updated `eng/public-api-baseline.txt`.
