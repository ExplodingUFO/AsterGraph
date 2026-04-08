---
phase: 20-host-menu-control-consolidation
plan: 03
subsystem: host-runtime-controls-and-regressions
completed: 2026-04-08
---

# Phase 20 Plan 03 Summary

Completed the `运行时` host menu group and locked the full Phase 20 control flow with focused headless regressions.

Key changes:

- Added explicit `运行时` menu entries for `打开运行时摘要` and `查看最近诊断` in `src/AsterGraph.Demo/Views/MainWindow.axaml`, both routing into the existing right-side pane.
- Replaced the summary-only runtime pane path with `PART_RuntimeSummarySection` and `PART_RuntimeDiagnosticsSection`, keeping runtime metrics and diagnostics separate and sourced from the current `Editor.Session`.
- Expanded runtime projection coverage in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` so the compact summary now includes `连线数量` alongside the existing document, node, selection, zoom, and diagnostics projections.
- Added `tests/AsterGraph.Editor.Tests/DemoHostMenuControlTests.cs` and updated the Phase 19 demo-shell tests so the suite now proves:
  - view, behavior, and runtime navigation keep the same `MainWindowViewModel.Editor`
  - runtime summary lines include the required connection-count row
  - behavior toggles still drive `Editor.BehaviorOptions` and `Editor.CommandPermissions`
  - chrome toggles reach the live `GraphEditorView`

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~GraphEditorDemoShellTests|FullyQualifiedName~DemoDiagnosticsProjectionTests|FullyQualifiedName~DemoHostMenuControlTests" -v minimal`
  - exit 0
  - `16` tests passed
- `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal`
  - exit 0

Plan result:

- `CTRL-03` is satisfied.
- Phase 20 now has focused regression coverage for menu structure, drawer sections, live session continuity, behavior propagation, and runtime projection fidelity.
