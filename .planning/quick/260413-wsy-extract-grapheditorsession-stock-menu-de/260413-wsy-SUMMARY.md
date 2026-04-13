---
phase: quick-260413-wsy-extract-grapheditorsession-stock-menu-de
plan: 1
subsystem: runtime-menus
tags: [runtime, context-menu, refactor, tests]
provides:
  - Dedicated internal stock menu descriptor builder for `GraphEditorSession`
  - Session-level regression coverage for stock menu descriptor signatures across menu target kinds
  - Structural guard that prevents the stock menu builder methods from drifting back into `GraphEditorSession`
key-files:
  created:
    - .planning/quick/260413-wsy-extract-grapheditorsession-stock-menu-de/260413-wsy-SUMMARY.md
    - src/AsterGraph.Editor/Runtime/Internal/GraphEditorSessionStockMenuDescriptorBuilder.cs
  modified:
    - src/AsterGraph.Editor/Runtime/GraphEditorSession.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs
completed: 2026-04-13
---

# Quick Task Summary

## Outcome

- Extracted the stock context-menu descriptor construction logic out of `GraphEditorSession` into `GraphEditorSessionStockMenuDescriptorBuilder`.
- Kept `GraphEditorSession.BuildContextMenuDescriptors(...)` as the outer orchestration layer:
  - argument null-check
  - command descriptor snapshot dictionary
  - stock builder invocation
  - plugin augmentor chain with recoverable-failure fallback
- Added two focused regression tests in `GraphEditorSessionTests`:
  - a structural guard that fails if the stock builder methods move back into `GraphEditorSession`
  - a session-level descriptor signature regression that covers canvas, selection, node, output-port, input-port, and connection menus

## Verification

Executed:

- `dotnet test "C:\Users\SuperDragon\AppData\Local\Temp\astergraph-session-menu-tests\AsterGraph.SessionMenu.Tests.csproj" --filter "FullyQualifiedName~GraphEditorSession_DelegatesStockContextMenuDescriptorBuildingToDedicatedCollaborator|FullyQualifiedName~RuntimeSession_StockContextMenuDescriptorSignatures_RemainStableAcrossTargets" -v minimal`
- `dotnet test "C:\Users\SuperDragon\AppData\Local\Temp\astergraph-session-menu-tests\AsterGraph.SessionMenu.Tests.csproj" --filter "FullyQualifiedName~GraphEditorSessionTests" -v minimal`
- `dotnet build "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Editor/AsterGraph.Editor.csproj" -v minimal`

Result:

- Focused red/green cycle completed successfully through the temporary harness.
- All 28 tests in `GraphEditorSessionTests` passed in the focused harness.
- `AsterGraph.Editor` built successfully for `net8.0` and `net9.0`.

## Notes

- The shared `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` project currently has unrelated compile failures in other test files already present in the workspace, so this quick task used a temporary focused harness for verification instead of the full test project.
- No public API changes were made.
- No retained `GraphContextMenuBuilder` cleanup, kernel command routing, event batching, automation, or diagnostics refactors were included in this slice.
