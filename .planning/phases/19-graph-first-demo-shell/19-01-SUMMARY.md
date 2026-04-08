---
phase: 19-graph-first-demo-shell
plan: 01
subsystem: graph-first-shell-scaffold
completed: 2026-04-08
commit: b31fd40
---

# Phase 19 Plan 01 Summary

Replaced the old three-column capability-console shell with a graph-first demo shell scaffold led by a host-level menu.

Key changes:

- Rewrote `src/AsterGraph.Demo/Views/MainWindow.axaml` from a fixed `280,*,360` shell into a top-first layout.
- Added a host-level `Menu` named `PART_HostMenu` with top-level groups `展示`, `视图`, `行为`, `运行时`, and `证明`.
- Kept exactly one `GraphEditorView` named `MainGraphEditorView` as the main live artifact.
- Removed the permanent left and right narrative rails as the first-screen shell structure.

Verification run:

- `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal`
  - exit 0

Plan 01 result:

- The first read of the demo shell is now host menu first, graph first.
- The old “SDK explanation console” layout is no longer the baseline shell.
