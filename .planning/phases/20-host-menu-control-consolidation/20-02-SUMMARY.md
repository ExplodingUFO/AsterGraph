---
phase: 20-host-menu-control-consolidation
plan: 02
subsystem: host-behavior-controls
completed: 2026-04-08
---

# Phase 20 Plan 02 Summary

Consolidated the demo behavior and command-permission toggles into the `行为` host menu group and compact drawer controls.

Key changes:

- Added direct checkable `行为` menu items for `只读模式`, `网格吸附`, `对齐辅助线`, `工作区命令`, `片段命令`, and `宿主菜单扩展` plus the `打开行为控制` entry in `src/AsterGraph.Demo/Views/MainWindow.axaml`.
- Added the dedicated `PART_BehaviorDrawerControls` section so the right-side pane exposes the same six toggles as live controls instead of summary text.
- Kept behavior propagation on the existing `ApplyHostOptions()` and `BuildCommandPermissions()` path in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~GraphEditorDemoShellTests|FullyQualifiedName~DemoDiagnosticsProjectionTests|FullyQualifiedName~DemoHostMenuControlTests" -v minimal`
  - exit 0
  - `16` tests passed
- `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal`
  - exit 0

Plan result:

- `CTRL-02` is satisfied.
- Behavior controls now remain menu-first while still updating the canonical editor behavior/permission objects on the current live session.
