---
phase: 19-graph-first-demo-shell
plan: 02
subsystem: host-pane-state-and-compact-shell-surface
completed: 2026-04-08
commit: b31fd40
---

# Phase 19 Plan 02 Summary

Introduced host-menu state and a compact secondary shell surface so the graph remains primary while showcase detail stays on demand.

Key changes:

- Added host shell state to `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`:
  - `IsHostPaneOpen`
  - `SelectedHostMenuGroupTitle`
  - `OpenHostMenuGroupCommand`
  - `CloseHostPaneCommand`
- Added compact host-shell summaries such as:
  - `SelectedHostMenuGroupSummary`
  - `SelectedHostMenuGroupLines`
  - `HostPaneStateCaption`
  - `HostSessionContinuityCaption`
- Implemented `PART_HostShellSplitView` as a right-side overlay compact pane in `MainWindow.axaml`.
- Kept `Editor` single-owner and constructor-local; host shell interactions do not replace or rebuild the demo editor.
- Set the embedded `GraphEditorView` chrome visibility defaults to `false` so the first read stays graph-first.

Verification run:

- `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal`
  - exit 0

Plan 02 result:

- The graph remains the dominant visual surface.
- Secondary showcase detail is now available through a compact host-controlled pane over the same live session.
