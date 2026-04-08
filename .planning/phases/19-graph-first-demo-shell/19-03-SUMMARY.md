---
phase: 19-graph-first-demo-shell
plan: 03
subsystem: demo-shell-proof
completed: 2026-04-08
commit: b31fd40
---

# Phase 19 Plan 03 Summary

Locked the new graph-first demo shell baseline with focused headless regressions.

Key changes:

- Replaced the old `DemoMainWindowTests` shell assertions with graph-first expectations:
  - `PART_HostMenu` exists
  - top-level host menu groups exist
  - the old `MainShellGrid` contract is gone
  - only one `MainGraphEditorView` remains
  - the compact right-side pane starts closed
- Added `tests/AsterGraph.Editor.Tests/GraphEditorDemoShellTests.cs` to verify:
  - `OpenHostMenuGroupCommand` exists
  - `IsHostPaneOpen` starts closed and opens on command
  - the same `MainWindowViewModel.Editor` instance survives host shell interactions
  - host shell proof captions are exposed
- Kept `RuntimeDiagnosticsHelperText` reachable in the new compact pane so diagnostics helper coverage survives the shell redesign.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~GraphEditorDemoShellTests|FullyQualifiedName~DemoDiagnosticsProjectionTests" -v minimal`
  - exit 0
  - `9` tests passed
- `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal`
  - exit 0

Phase 19 result:

- `SHOW-01` is satisfied.
- `SHOW-02` is satisfied.
- The graph-first, host-menu-first shell baseline is now protected against regression.
