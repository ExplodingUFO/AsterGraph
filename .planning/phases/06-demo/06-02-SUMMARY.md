---
phase: 06-demo
plan: 02
subsystem: ui
tags: [avalonia, demo, localization, i18n, graph-editor]
requires:
  - phase: 06-demo
    provides: three-column Demo showcase shell, retained-session capability navigation, and focused Demo shell regression coverage
  - phase: 02-runtime-contracts-service-seams
    provides: canonical IGraphLocalizationProvider runtime localization seam
provides:
  - Chinese-first stock GraphEditorView shell copy for toolbar, library, workspace, fragments, mini map, and shortcuts
  - focused localization regression coverage for translated shell literals and preserved English technical seam names
  - verification that Demo runtime-owned captions still resolve through IGraphLocalizationProvider
affects: [demo, localization, avalonia-shell, validation, phase-06]
tech-stack:
  added: []
  patterns: [xaml shell translation with preserved API identifiers, focused source-plus-runtime localization regression tests]
key-files:
  created: []
  modified: [src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml, tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs]
key-decisions:
  - "Translate only user-facing stock shell copy in GraphEditorView while keeping host-facing API and type identifiers in English."
  - "Validate the Demo localization path through the existing nested DemoGraphLocalizationProvider and IGraphLocalizationProvider contract instead of modifying MainWindowViewModel in this plan."
patterns-established:
  - "Phase 6 localization pattern: keep runtime-owned captions on the canonical IGraphLocalizationProvider seam and limit Demo plan work to shell copy plus focused regressions when file ownership is constrained."
  - "Localization regression pattern: combine direct source assertions for stock XAML literals with runtime assertions for provider-backed captions and preserved technical identifiers."
requirements-completed: [DEMO-I18N-01]
duration: 14min
completed: 2026-03-27
---

# Phase 6 Plan 02: Demo Localization Summary

**Chinese-first GraphEditorView shell copy with focused regression coverage that preserves the canonical runtime localization seam and English host API identifiers**

## Performance

- **Duration:** 14 min
- **Started:** 2026-03-27T09:09:00Z
- **Completed:** 2026-03-27T09:22:53Z
- **Tasks:** 3
- **Files modified:** 2

## Accomplishments
- Translated the stock `GraphEditorView` shell so user-facing toolbar, library, workspace, fragment, mini map, and shortcut copy now reads in Simplified Chinese.
- Extended `GraphEditorLocalizationTests` to guard exact Chinese literals, preserved English technical seam names, and the canonical Demo localization-provider path.
- Kept the plan isolated from `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` while still proving runtime-owned captions resolve through `IGraphLocalizationProvider`.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add focused localization regression coverage** - `220e467` (test)
2. **Task 2: Translate stock GraphEditorView user-facing copy to Chinese** - `ddf0eb0` (feat)
3. **Task 3: Validate runtime-owned captions still flow through the canonical provider** - `e0dca80` (feat)

**Plan metadata:** pending metadata commit

## Files Created/Modified
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` - Translated stock shell literals to Simplified Chinese while leaving technical identifiers untouched.
- `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs` - Added assertions for translated shell text, canonical provider usage, and preserved English API seam names.

## Decisions Made
- Translated only user-facing `GraphEditorView` copy and kept technical identifiers such as `AsterGraphCanvasViewFactory` and `AsterGraphPresentationOptions` in English so the Demo still teaches the real host API path.
- Verified canonical runtime localization by reflecting the existing nested `DemoGraphLocalizationProvider` and asserting its `IGraphLocalizationProvider.GetString(key, fallback)` behavior instead of editing the Demo ViewModel in this plan.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- The focused localization test initially used an incorrect repo-relative path when reading `GraphEditorView.axaml`; the test helper was corrected and re-verified within the same task scope.
- Focused test runs emit pre-existing XML documentation warnings from public projects, but `GraphEditorLocalizationTests` passes cleanly and remained the intended gate for this plan.

## Known Stubs
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- The Demo shell now reads as Chinese-first without introducing a second localization system.
- Follow-on Phase 6 work can build on these guarded shell literals and provider checks when expanding localized diagnostics or richer showcase copy.

## Self-Check: PASSED
- Found summary target path `F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/phases/06-demo/06-02-SUMMARY.md`.
- Verified task commits `220e467`, `ddf0eb0`, and `e0dca80` exist in git history.
- Verified focused automation with `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter GraphEditorLocalizationTests --nologo -v minimal`.

---
*Phase: 06-demo*
*Completed: 2026-03-27*
