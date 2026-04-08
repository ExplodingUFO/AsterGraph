---
phase: 21-in-context-proof-and-narrative-polish
plan: 01
subsystem: proof-focused-shell-copy
completed: 2026-04-08
---

# Phase 21 Plan 01 Summary

Replaced the remaining generic shell copy with compact in-context proof cues so the demo now reads as a host-controlled live-session showcase on first launch.

Key changes:

- Renamed the `展示` and `证明` actions to `打开展示摘要` and `查看证明要点` in `src/AsterGraph.Demo/Views/MainWindow.axaml`.
- Replaced the generic drawer overline with `宿主控制抽屉` and the graph intro title with `实时 SDK 会话`.
- Added dedicated proof cue properties and intro-strip badges for `宿主控制`, `共享运行时`, and the active host group in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.
- Expanded shell regressions so the tests now lock the new proof-focused menu copy, drawer caption, intro title, and badge text.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~GraphEditorDemoShellTests" -v minimal`
  - exit 0
  - `11` tests passed

Plan result:

- `PROOF-01` is partially satisfied by the first-read shell itself.
- The top-level host shell now proves seam ownership before the user opens a large explanatory surface.
