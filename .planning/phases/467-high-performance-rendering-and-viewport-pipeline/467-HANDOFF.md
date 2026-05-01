# Phase 467 Handoff

## Completed

Phase 467 closed `RENDER-01` with implementation and proof for:

- visible-scene node/group visual budgeting;
- committed connection render scoping by visible route bounds;
- pending connection preview preservation;
- viewport projection diff marker recording;
- minimap balanced/throughput cadence semantics;
- grid density and resize hover hit-test proof.

## Next Consumer

Phase 470 cookbook should consume the Phase 467 proof as demo recipes. The cookbook should show supported behavior through code anchors and live demo state, not introduce broader rendering claims.

## Do Not Reopen

- Do not introduce a second viewport state model in Avalonia.
- Do not describe the retained scene host as a new renderer rewrite.
- Do not claim an unsupported graph-size tier.
- Do not filter pending preview rendering through committed-route visibility.

## Key Files

- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasSceneHost.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasConnectionSceneRenderer.cs`
- `src/AsterGraph.Editor/Viewport/ViewportVisibleSceneProjection.cs`
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`
- `src/AsterGraph.Avalonia/Controls/GridBackground.cs`
- `tests/AsterGraph.Editor.Tests/NodeCanvasSceneHostViewportProjectionTests.cs`
- `tests/AsterGraph.Editor.Tests/NodeCanvasConnectionSceneRendererTests.cs`
- `tests/AsterGraph.Editor.Tests/ViewportVisibleSceneProjectionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs`
- `tests/AsterGraph.Editor.Tests/GridBackgroundTests.cs`

## Remaining Phase 470 Work

Create cookbook coverage that lets a user inspect:

1. visible projection counts vs rendered canvas elements;
2. connection committed vs pending preview rendering;
3. minimap cadence mode behavior;
4. grid/hit-test bounded behavior.
