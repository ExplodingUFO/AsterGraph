---
phase: 260327-pas-phase-6-demo
plan: 1
subsystem: ui
tags: [avalonia, demo, chrome, layout, headless-tests]
requires:
  - phase: 260327-oqi-phase-6-demo-ui-fluent
    provides: Dense three-column demo shell baseline and compact GraphEditorView top chrome structure
provides:
  - Low-radius squared demo shell styling while retaining the locked three-column architecture
  - Full-height center GraphEditorView composition with independently scrolling side rails
  - Independent per-region visibility seams for header, library, inspector, and status chrome
affects: [phase-06-demo, host-embedding, ui-regression-tests]
tech-stack:
  added: []
  patterns:
    - Visual-only chrome toggles stay in Avalonia shell layer and bind from demo host state
    - Center rail editor frame uses star-row composition instead of fixed height cards
key-files:
  created: []
  modified:
    - src/AsterGraph.Demo/Views/MainWindow.axaml
    - src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs
    - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml
    - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs
    - tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs
key-decisions:
  - "Expose per-region chrome booleans on GraphEditorView and compose them with existing ChromeMode instead of replacing mode semantics."
  - "Move demo center editor framing to star-row stretch layout so the graph surface fills available height without forcing fixed frame values."
patterns-established:
  - "Graph shell visibility toggles: host binds booleans to visual shell properties, runtime/editor session remains unchanged."
  - "Demo layout proof: keep one live GraphEditorView and prove architecture with side-rail cards plus focused headless assertions."
requirements-completed: [PHASE6-DEMO]
duration: 14min
completed: 2026-03-27
---

# Phase 260327-pas Plan 1: Demo shell squared pass summary

**Phase 6 quick pass now ships a lower-radius squared demo shell, a full-height center graph surface, and independent chrome region visibility controls without rebuilding the editor session.**

## Performance

- **Duration:** 14 min
- **Started:** 2026-03-27T10:17:44Z
- **Completed:** 2026-03-27T10:31:44Z
- **Tasks:** 3
- **Files modified:** 6

## Accomplishments
- Restyled the Demo shell cards and framing toward a denser squared professional-tool direction while preserving the locked three-column structure.
- Removed the fixed-height center editor frame and switched to stretch composition so the main GraphEditorView fills the available center height.
- Added independent header/library/inspector/status chrome visibility seams in GraphEditorView and bound demo host toggles to those seams.
- Locked the pass with focused headless tests covering three-column contract, full-height composition, and per-region visibility behavior.

## Task Commits

Each task was committed atomically:

1. **Task 1: Restyle the demo shell into the low-radius squared direction** - `ca8ba1a` (feat)
2. **Task 2: Make the main graph surface fill height and support independent chrome visibility** - `54ea3e7` (feat)
3. **Task 3: Lock the quick fix with focused shell regressions** - `f5ec973` (test)

## Files Created/Modified
- `F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/Views/MainWindow.axaml` - Squared shell styling, full-height center frame composition, and bound chrome toggle controls.
- `F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` - Demo chrome visibility state properties and helper copy update.
- `F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` - Node canvas host now explicitly stretches in min-height-safe center region.
- `F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` - New per-region chrome visibility properties and composed chrome-mode application.
- `F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` - Full-height center editor and squared shell regression assertions.
- `F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` - Independent chrome visibility regression assertions.

## Verification

Executed:
- `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests" -v minimal`
- `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "GraphEditorViewTests|DemoMainWindowTests" -v minimal`

Result: filtered tests passed.

## Decisions Made
- Kept `GraphEditorViewChromeMode` as the coarse mode switch, and layered explicit `IsHeaderChromeVisible` / `IsLibraryChromeVisible` / `IsInspectorChromeVisible` / `IsStatusChromeVisible` booleans on top for independent host control.
- Kept visual toggle logic in Avalonia shell and demo host bindings, in line with Phase 6 guidance to avoid moving visual-only concerns into editor behavior options.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Known Stubs

None.

## Next Phase Readiness
- Demo shell now supports low-radius squared presentation and independent chrome embedding demonstrations.
- Focused headless tests provide regression coverage for center stretch and per-region chrome seams.
- Existing unrelated workspace noise remains outside this quick pass scope.

## Self-Check: PASSED
- FOUND: `F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/quick/260327-pas-phase-6-demo/260327-pas-SUMMARY.md`
- FOUND: commit `ca8ba1a`
- FOUND: commit `54ea3e7`
- FOUND: commit `f5ec973`
