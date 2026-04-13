---
phase: quick-260413-x3v-follow-up-fix-keep-grapheditorsession-st
plan: 1
subsystem: runtime-menus
tags: [runtime, context-menu, regression-fix]
completed: 2026-04-13
---

# Quick Task Summary

## Outcome

- Fixed the follow-up regression from the stock menu builder extraction: canvas menu descriptors now read node definitions from a live provider instead of a session-construction snapshot.
- Added a focused regression test proving that registering a new definition on the same `NodeCatalog` after session creation updates `BuildContextMenuDescriptors(ContextMenuTargetKind.Canvas, ...)`.

## Verification

Executed:

- `dotnet test "C:\Users\SuperDragon\AppData\Local\Temp\astergraph-session-menu-tests\AsterGraph.SessionMenu.Tests.csproj" --filter "FullyQualifiedName~RuntimeSession_CanvasMenuDescriptors_UseLiveNodeCatalogDefinitionsAfterSessionCreation" -v minimal`
- `dotnet test "C:\Users\SuperDragon\AppData\Local\Temp\astergraph-session-menu-tests\AsterGraph.SessionMenu.Tests.csproj" --filter "FullyQualifiedName~GraphEditorSessionTests" -v minimal`
- `dotnet build "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Editor/AsterGraph.Editor.csproj" -v minimal`

Result:

- The new regression test failed before the fix and passed after it.
- All 29 focused `GraphEditorSessionTests` passed in the temporary harness.
- `AsterGraph.Editor` built successfully for `net8.0` and `net9.0` with 0 warnings and 0 errors.

## Notes

- Verification still used the temporary focused harness because unrelated compile failures remain in other in-progress files under `tests/AsterGraph.Editor.Tests`.
- This follow-up only changed the definitions dependency from frozen snapshot to live provider; no other runtime orchestration was modified.
