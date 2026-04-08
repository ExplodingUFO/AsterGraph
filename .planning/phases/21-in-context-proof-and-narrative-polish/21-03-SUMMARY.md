---
phase: 21-in-context-proof-and-narrative-polish
plan: 03
subsystem: demo-narrative-alignment-and-regressions
completed: 2026-04-08
---

# Phase 21 Plan 03 Summary

Aligned the repository README and the final demo copy with the graph-first proof narrative, then locked the completed phase with focused regressions and a clean build.

Key changes:

- Added a dedicated demo-showcase section to `README.md` that describes `src/AsterGraph.Demo` as the graph-first SDK showcase, calls out the in-window host menu groups, and explains the one-live-session proof.
- Kept the final UI terminology aligned around the same proof story: host-controlled seams, shared runtime state, and one retained `Editor.Session`.
- Preserved all Phase 19/20 single-session guarantees while expanding the focused demo-shell regression suite to `21` passing tests.

Verification run:

- `rg -n "graph-first|host menu|live session" README.md`
  - exit 0
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~DemoMainWindowTests|FullyQualifiedName~GraphEditorDemoShellTests|FullyQualifiedName~DemoHostMenuControlTests|FullyQualifiedName~DemoDiagnosticsProjectionTests" -v minimal`
  - exit 0
  - `21` tests passed
- `dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -v minimal`
  - exit 0
  - `0` warnings, `0` errors

Plan result:

- `PROOF-01` and `PROOF-02` are fully satisfied.
- The demo UI and repo-facing demo narrative now tell the same graph-first host-menu story.
