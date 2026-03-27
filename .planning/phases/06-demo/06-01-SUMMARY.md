---
phase: 06-demo
plan: 01
subsystem: ui
tags: [avalonia, demo, diagnostics, presentation, embeddable-surfaces]
requires:
  - phase: 03-embeddable-avalonia-surfaces
    provides: standalone canvas, inspector, and mini map host surfaces over one editor session
  - phase: 04-replaceable-presentation-kit
    provides: presenter replacement seams through AsterGraphPresentationOptions
  - phase: 05-diagnostics-integration-inspection
    provides: session diagnostics, inspection snapshots, and recent diagnostic history
provides:
  - three-column Demo showcase shell over one retained GraphEditorViewModel session
  - capability navigation and architecture metadata for full shell, standalone surfaces, presenter replacement, and diagnostics
  - focused DemoMainWindow headless regression coverage for the Phase 6 shell contract
affects: [demo, docs, validation, phase-06]
tech-stack:
  added: []
  patterns: [retained-session demo showcase, capability-driven view-model metadata, focused Avalonia headless shell regression tests]
key-files:
  created: [tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs, tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs]
  modified: [src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs, src/AsterGraph.Demo/Views/MainWindow.axaml, tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj]
key-decisions:
  - "Keep the Demo centered on one live GraphEditorView and project surrounding proof content from the retained editor session instead of creating secondary runtime models."
  - "Use capability metadata in MainWindowViewModel to drive both navigation and right-rail architecture/proof copy so the shell explains real SDK seams without hard-coded duplicate sections."
  - "Retarget the Demo-focused test project to net9.0 and reference AsterGraph.Demo so headless shell coverage can exercise the real MainWindow implementation."
patterns-established:
  - "Demo shell pattern: left rail controls navigation/toggles, center rail keeps one dominant editor, right rail binds architecture and diagnostics evidence from runtime state."
  - "Phase 6 regression pattern: test the Demo shell through a dedicated MainWindow headless test instead of editing the noisier GraphEditorView coverage surface."
requirements-completed: [EMBD-01, EMBD-02, EMBD-03, EMBD-04, EMBD-05, PRES-01, PRES-02, PRES-03, PRES-04]
duration: 20min
completed: 2026-03-27
---

# Phase 6 Plan 01: Demo Showcase Shell Summary

**Three-column Avalonia demo shell with retained-session capability navigation, runtime diagnostics evidence, and focused headless shell regression coverage**

## Performance

- **Duration:** 20 min
- **Started:** 2026-03-27T08:45:00Z
- **Completed:** 2026-03-27T09:05:33Z
- **Tasks:** 3
- **Files modified:** 5

## Accomplishments
- Replaced the old toolbar-over-editor Demo layout with a locked three-column SDK showcase centered on one live `GraphEditorView`.
- Added capability-driven ViewModel metadata so the Demo can switch among 完整壳层, 独立表面, 可替换呈现, and 运行时与诊断 without rebuilding the retained editor session.
- Added focused `DemoMainWindowTests` coverage that guards the Phase 6 shell contract and validates the one-editor, three-column layout.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add focused Demo shell regression coverage** - `d261459` (test)
2. **Task 2: Implement the three-column SDK showcase shell** - `1fa003c` (feat)
3. **Task 3: Add live proof for full-shell boundary, standalone surfaces, and presenter replacement** - `d2ea712` (feat)

**Plan metadata:** pending metadata commit

## Files Created/Modified
- `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` - Focused headless regression coverage for the Phase 6 shell contract.
- `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` - Net9-compatible headless test app attribute/import adjustments needed for the new Demo shell test target.
- `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` - Retargeted tests to `net9.0` and referenced `AsterGraph.Demo` so MainWindow can be tested directly.
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` - Added capability metadata, retained-session diagnostics projection, and shell proof content bindings.
- `src/AsterGraph.Demo/Views/MainWindow.axaml` - Rebuilt the Demo into the fixed three-column showcase shell with named architecture sections and proof rails.

## Decisions Made
- Kept the Demo on one retained `GraphEditorViewModel` and projected all architecture proof from runtime/session APIs instead of creating subordinate runtime objects.
- Bound capability metadata from the ViewModel into both navigation and the right rail so the shell teaches actual API seams and avoids duplicated copy islands.
- Used a dedicated `DemoMainWindowTests` entry point for Phase 6 shell regression coverage while leaving the pre-existing `GraphEditorView` coverage isolated.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Retargeted Demo shell tests to net9.0 and referenced the Demo project**
- **Found during:** Task 1 (Add focused Demo shell regression coverage)
- **Issue:** The existing editor test project targeted `net8.0` and could not compile against `AsterGraph.Demo`, which targets `net9.0`.
- **Fix:** Updated `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` to target `net9.0` and reference `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- **Files modified:** `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`
- **Verification:** `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter DemoMainWindowTests`
- **Committed in:** `d261459`

**2. [Rule 3 - Blocking] Updated headless test scaffolding for the net9 Demo shell path**
- **Found during:** Task 1 (Add focused Demo shell regression coverage)
- **Issue:** The test assembly needed a resolvable headless test application type and Avalonia net9-compatible window/visual-tree usage before the new shell test could compile and run.
- **Fix:** Added the Demo shell test file, restored the shared `GraphEditorViewTests` app builder as a standalone test support file, switched away from `using var` windows, and imported `Avalonia.VisualTree` for visual traversal.
- **Files modified:** `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`
- **Verification:** `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter DemoMainWindowTests`
- **Committed in:** `d261459`

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both auto-fixes were required to make the requested shell regression coverage executable against the real Demo window. No scope creep.

## Issues Encountered
- Existing workspace noise remained in planning files, `src/AsterGraph.Editor/README.md`, and an untracked `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs`; task commits were staged narrowly to avoid touching unrelated changes.
- Running the focused Demo test emits many existing XML-doc warnings from public Demo types, but the requested `DemoMainWindowTests` filter passes successfully.

## Known Stubs
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 6 now has a concrete shell baseline that visibly proves embeddable surfaces, presenter seams, and diagnostics over the retained editor session.
- Follow-on plans can layer deeper Chinese localization, richer standalone previews, or docs/validation against this shell without reworking the page structure.

## Self-Check: PASSED
- Found summary target path and all task commits (`d261459`, `1fa003c`, `d2ea712`).
- Verified focused automation with `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter DemoMainWindowTests`.

---
*Phase: 06-demo*
*Completed: 2026-03-27*
