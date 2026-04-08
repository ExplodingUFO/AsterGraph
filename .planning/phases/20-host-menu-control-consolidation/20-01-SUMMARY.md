---
phase: 20-host-menu-control-consolidation
plan: 01
subsystem: host-view-controls
completed: 2026-04-08
---

# Phase 20 Plan 01 Summary

Turned the `视图` host menu group into a live chrome control surface over the existing demo session.

Key changes:

- Added direct checkable `视图` menu items for `显示顶栏`, `显示节点库`, `显示检查器`, and `显示状态栏` plus the `打开视图控制` entry in `src/AsterGraph.Demo/Views/MainWindow.axaml`.
- Replaced the summary-only view pane path with a dedicated `PART_ViewDrawerControls` section that binds the same four booleans used by `MainGraphEditorView`.
- Kept the demo on one live `GraphEditorViewModel` instance; no second editor/session creation path was introduced in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~GraphEditorDemoShellTests|FullyQualifiedName~DemoDiagnosticsProjectionTests|FullyQualifiedName~DemoHostMenuControlTests" -v minimal`
  - exit 0
  - `16` tests passed
- `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal`
  - exit 0

Plan result:

- `CTRL-01` is satisfied for the view/chrome menu path.
- View menu state and drawer state now share the same bindings, so toggles apply live without recreating the demo session.
