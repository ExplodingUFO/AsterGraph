# Phase 445 Verification: Rendering And Viewport Performance Foundation

**Bead:** `avalonia-node-map-mqm.2`
**Status:** Passed

## Tests

Focused touched-surface test:

```text
dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~ViewportVisibleSceneProjectionTests|FullyQualifiedName~GraphMiniMapStandaloneTests"
Passed: 8, Failed: 0, Skipped: 0
```

Full touched test project:

```text
dotnet test tests\AsterGraph.Editor.Tests\AsterGraph.Editor.Tests.csproj
Passed: 679, Failed: 0, Skipped: 0
```

## Prohibited External-Name Scan

Scanned the Phase 445 planning artifacts with the existing project-style prohibited-name pattern:

```text
rg -n "<prohibited external project names>" .planning\phases\445-rendering-and-viewport-performance-foundation
No matches.
```

## Acceptance Mapping

- Measurable scene projection: `ViewportVisibleSceneProjector` and `VISIBLE_SCENE_PROJECTION` marker.
- Minimap cadence/budget contract: `MiniMapRefreshCadence` and `MINIMAP_CADENCE` marker.
- Existing viewport/rendering behavior preserved: no replacement renderer, no culling layer, no fallback path.
- Verification passed after implementation.
