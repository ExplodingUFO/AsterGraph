---
phase: 06-demo
plan: 03
subsystem: ui
tags: [avalonia, demo, diagnostics, inspection, session]

requires:
  - phase: 05-diagnostics-integration-inspection
    provides: public diagnostics contracts, inspection snapshots, and retained session diagnostics access
  - phase: 06-demo
    provides: three-column demo shell and localized showcase baseline from plans 06-01 and 06-02
provides:
  - Demo right-rail inspection snapshot card backed by retained session diagnostics
  - Machine-readable recent diagnostics projection with code, severity, operation, and message fields
  - Focused regression coverage proving Demo runtime evidence comes from Editor.Session.Diagnostics
affects: [demo, diagnostics, inspection, host-sample]

tech-stack:
  added: []
  patterns: [retained-session projection in view model, machine-readable diagnostics cards in Avalonia XAML]

key-files:
  created: [tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs]
  modified: [src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs, src/AsterGraph.Demo/Views/MainWindow.axaml]

key-decisions:
  - "Project Demo diagnostics directly from the retained IGraphEditorSession instead of introducing a second demo-specific runtime model."
  - "Keep compatibility status text visible, but separate it from the canonical Editor.Session.Diagnostics evidence path."
  - "Render recent diagnostics as machine-readable cards with severity metadata instead of status-only prose lines."

patterns-established:
  - "Demo runtime proof panels should bind to Editor.Session.Diagnostics snapshots rather than duplicating editor state."
  - "Chinese-first demo copy can coexist with English technical identifiers when naming canonical host seams."

requirements-completed: [DIAG-01, DIAG-02, DIAG-03]

duration: 16min
completed: 2026-03-27
---

# Phase 6 Plan 03: Runtime diagnostics rail summary

**Demo right rail now exposes retained inspection snapshots and machine-readable recent diagnostics from Editor.Session.Diagnostics.**

## Performance

- **Duration:** 16 min
- **Started:** 2026-03-27T09:11:28Z
- **Completed:** 2026-03-27T09:27:53Z
- **Tasks:** 3
- **Files modified:** 3

## Accomplishments
- Added focused regression coverage for diagnostics projection and helper-text binding.
- Exposed thin runtime-backed inspection and diagnostics properties in `MainWindowViewModel`.
- Reworked the right rail to show canonical seam names, inspection facts, and recent diagnostics cards with machine-readable fields.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add focused diagnostics projection regression coverage** - `5381ab6` (test)
2. **Task 2: Expose retained-session inspection and recent diagnostics through thin ViewModel projections** - `278e0d1` (feat)
3. **Task 3: Bind runtime and diagnostics evidence cards into the right rail** - `7accb9e` (feat)

## Files Created/Modified
- `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` - Verifies diagnostics projection comes from `Editor.Session.Diagnostics` and stays separate from compatibility status text.
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` - Projects retained inspection snapshot fields and recent diagnostics entries from the retained session.
- `src/AsterGraph.Demo/Views/MainWindow.axaml` - Renders runtime inspection and recent diagnostics evidence cards in the right rail.

## Decisions Made
- Used `IGraphEditorSession` and `Editor.Session.Diagnostics` as named proof seams in the UI so the demo teaches the canonical host path explicitly.
- Kept `StatusMessage` as compatibility UX only and displayed it separately from diagnostics records.
- Added severity label and color metadata to projected diagnostics so the view can remain thin and declarative.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Initial filtered test runs were blocked by concurrent workspace noise, but the focused Demo test suite passed once the worktree stabilized.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Phase 6 demo shell now proves runtime diagnostics and inspection directly from the retained session.
- The summary is ready for higher-level state/roadmap closeout once shared `.planning` metadata ownership is clear.

## Self-Check: PASSED
- Found summary file target: `F:\CodeProjects\DotnetCore\avalonia-node-map\.planning\phases\06-demo\06-03-SUMMARY.md`
- Found task commit `5381ab6`
- Found task commit `278e0d1`
- Found task commit `7accb9e`

---
*Phase: 06-demo*
*Completed: 2026-03-27*
