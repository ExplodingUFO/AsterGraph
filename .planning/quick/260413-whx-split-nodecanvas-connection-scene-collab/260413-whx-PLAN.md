# Quick Task 260413-whx Plan

**Date:** 2026-04-13
**Description:** Split NodeCanvas connection scene collaborator into internal helper without pointer-handler scope creep

## Scope

- Extract the connection-scene rendering logic from `NodeCanvas` into a new internal collaborator.
- Keep `NodeCanvas` limited to orchestration and small delegating wrappers.
- Preserve existing connection rendering, pending preview rendering, connection label/context-menu behavior, and rendered-anchor-first port resolution.
- Remove presenter-era dead code from `NodeCanvas` only if it is unreferenced.

## Tasks

### 1. Add focused regression coverage first
- Add a new `NodeCanvasConnectionSceneRendererTests` file.
- Cover rendered-anchor preference with fallback to `NodeViewModel.GetPortAnchor(...)`.
- Cover committed connection rendering plus pending preview rendering and connection label context-menu routing.
- Verify the new test target fails before implementation.

### 2. Extract the internal collaborator
- Add a new helper under `src/AsterGraph.Avalonia/Controls/Internal/`.
- Move `RenderConnections`, `DrawConnection`, and `GetPortAnchor` logic into the helper.
- Keep `NodeCanvas` pointer handlers and broader interaction code unchanged.
- Delete `BuildPortPanel`, `GetNodeCardStyle`, and `GetPortStyle` from `NodeCanvas` if they are confirmed unused.

### 3. Verify and finish
- Run the focused renderer tests, then the existing `NodeCanvas`-related regression set.
- Review the final diff to ensure scope stayed within the requested files.
- Commit only this task's files as one atomic commit.
