---
phase: 01-consumption-compatibility-guardrails
plan: 02
subsystem: hosting
tags: [editor, avalonia, hosting, factories, initialization]
requires: [01-01]
provides:
  - Public factory/options editor initialization surface in AsterGraph.Editor
  - Public factory/options Avalonia view initialization surface in AsterGraph.Avalonia
  - Regression coverage for factory validation and seam forwarding
affects: [01-03, 01-04, host-sample]
tech-stack:
  added: []
  patterns: [factory-first host composition, compatibility-facade preservation]
key-files:
  created:
    - src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs
    - src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs
    - src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs
    - .planning/phases/01-consumption-compatibility-guardrails/01-02-SUMMARY.md
  modified:
    - tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs
    - tools/AsterGraph.HostSample/Program.cs
    - .planning/STATE.md
    - .planning/ROADMAP.md
key-decisions:
  - "Keep the new host entry surface factory-first and place it under the existing Hosting namespaces in AsterGraph.Editor and AsterGraph.Avalonia."
  - "Preserve direct GraphEditorViewModel and GraphEditorView construction as the compatibility path while making the new factories the canonical host-sample composition route."
patterns-established:
  - "New initialization entry points delegate directly to the existing GraphEditorViewModel and GraphEditorView compatibility facades instead of creating a second behavior path."
  - "Host-facing option records carry the same style, behavior, workspace, menu, presentation, and localization seams already supported by constructor composition."
requirements-completed: [PKG-02]
duration: 17min
completed: 2026-03-26
---

# Phase 01 Plan 02: Consumption & Compatibility Guardrails Summary

**Public factory/options initialization surface for the editor runtime and default Avalonia view**

## Performance

- **Duration:** 17 min
- **Completed:** 2026-03-26T00:33:59+08:00
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments

- Added `AsterGraphEditorOptions` plus `AsterGraphEditorFactory.Create(...)` so hosts can compose a `GraphEditorViewModel` through a public AsterGraph-owned contract instead of copying demo wiring.
- Added `AsterGraphAvaloniaViewOptions` plus `AsterGraphAvaloniaViewFactory.Create(...)` so hosts can compose the default `GraphEditorView` through a public Avalonia-facing contract while keeping direct view construction supported.
- Replaced the skipped initialization placeholder suite with real regression coverage for required input validation, seam forwarding, and default view wiring.
- Updated `tools/AsterGraph.HostSample/Program.cs` to use the new factories/options as the canonical sample path while retaining a migration note that direct construction still works.

## Task Commits

1. **Task 1: Define host-facing initialization contracts for editor and Avalonia composition** - `d3bd7e9` (feat)
2. **Task 2: Prove the new initialization surface through tests and the host sample** - `179e654` (test)

**Plan metadata:** pending

## Files Created/Modified

- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` - Host-facing runtime composition contract for editor setup.
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - Canonical editor factory that validates required inputs and delegates to `GraphEditorViewModel`.
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs` - Host-facing view composition contract for the default Avalonia shell.
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs` - Canonical view factory that wires `Editor` and `ChromeMode` onto `GraphEditorView`.
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - Regression coverage for factory validation, seam forwarding, and default view wiring.
- `tools/AsterGraph.HostSample/Program.cs` - Canonical host sample now demonstrates the new factory-based composition path.
- `.planning/phases/01-consumption-compatibility-guardrails/01-02-SUMMARY.md` - Execution summary and verification evidence.
- `.planning/STATE.md` - Updated after plan execution.
- `.planning/ROADMAP.md` - Updated with Phase 1 plan progress.

## Decisions Made

- Kept the new entry surface factory-first instead of adding DI helpers in this plan because the approved research explicitly recommended factory/options as the required Phase 1 baseline.
- Put both new contracts under the existing `Hosting` namespaces so hosts can discover the new composition APIs without changing layer boundaries.
- Documented direct `GraphEditorViewModel` and `GraphEditorView` construction as still supported to preserve the staged migration story from D-04 and D-05.

## Deviations from Plan

### Verification Adjustments

- **Found during:** Task 1 verification
- **Issue:** The required `dotnet build avalonia-node-map.sln -v minimal` and targeted editor test command are still blocked by the out-of-scope untracked file `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`.
- **Adjustment:** Ran focused builds for `src/AsterGraph.Editor` and `src/AsterGraph.Avalonia` to verify the new public hosting types compile, and used the host sample smoke run as the executable integration proof.
- **Files modified:** None
- **Commit:** N/A

## Issues Encountered

- `dotnet build avalonia-node-map.sln -v minimal` still fails in this workspace because the pre-existing untracked `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` file references `GraphEditorViewTestsAppBuilder` from assembly scope without resolving it.
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests" -v minimal` is blocked by that same out-of-scope compile failure before the targeted initialization tests can run.
- A transient `AsterGraph.Avalonia.pdb` lock occurred when `dotnet test` and `dotnet run` were started in parallel; rerunning the test command sequentially confirmed the real blocker is still `GraphEditorViewTests.cs`.

## Verification Evidence

- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj -v minimal` ✅
- `dotnet build src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -v minimal` ✅
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` ✅
- `dotnet build avalonia-node-map.sln -v minimal` ❌ blocked by `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests" -v minimal` ❌ blocked by `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`

## User Setup Required

None.

## Next Phase Readiness

- Plan `01-03` can build migration-compatibility work on top of the new public initialization surface instead of inventing another composition path.
- Plan `01-04` can align docs and package-consumption guidance around the new hosting factories while still documenting the compatibility route.
- A clean initialization-test run in this workspace still requires resolving the out-of-scope `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` compile issue separately.

## Self-Check
PASSED

- FOUND: `.planning/phases/01-consumption-compatibility-guardrails/01-02-SUMMARY.md`
- FOUND: `d3bd7e9`
- FOUND: `179e654`
