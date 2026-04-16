---
phase: 30-milestone-history-and-refactor-gate-closeout
plan: 02
subsystem: testing
tags: [ci, powershell, refactor-gate, scale-smoke, regression]
requires:
  - phase: 30-01
    provides: archived v1.4 milestone history so refactor-gate docs can refer to current planning state cleanly
provides:
  - maintenance lane in eng/ci.ps1
  - fixed hotspot-sensitive editor test filter
  - scale-smoke execution in the maintenance gate
affects: [phase-30-closeout, hotspot-refactors, contributor-validation]
tech-stack:
  added: []
  patterns: [script-first maintenance gate, focused hotspot regression filter]
key-files:
  created: []
  modified:
    - eng/ci.ps1
key-decisions:
  - "Keep maintenance as a dedicated script lane inside eng/ci.ps1 instead of adding a second command path."
  - "Keep release-only pack, PackageSmoke, coverage, and package validation out of maintenance so the new lane stays refactor-focused."
patterns-established:
  - "Maintenance validation is a narrow hotspot-refactor lane, not a second release lane."
  - "Hotspot guardrails combine focused editor tests with ScaleSmoke for one runnable readiness proof."
requirements-completed: [GUARD-01]
duration: 1 min
completed: 2026-04-16
---

# Phase 30 Plan 02: Maintenance Refactor Gate Summary

**Added a script-first `maintenance` lane to `eng/ci.ps1` that runs the hotspot-sensitive editor regression surface plus `ScaleSmoke`, giving refactor work one checked-in proof command that is narrower than `release`.**

## Performance

- **Duration:** 1 min
- **Started:** 2026-04-16T11:59:33+08:00
- **Completed:** 2026-04-16T12:00:14+08:00
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments

- Added `-Lane maintenance` to the existing script entrypoint instead of introducing a second validation script.
- Kept the maintenance lane separate from release-only behavior such as pack, `PackageSmoke`, coverage reporting, and package validation.
- Curated the maintenance proof surface down to the hotspot-sensitive editor suites and `ScaleSmoke`.

## Task Commits

Each task was committed atomically:

1. **Task 1: Add a script-first `maintenance` lane to `eng/ci.ps1`** - `f699cdd` (`chore`)
2. **Task 2: Curate the hotspot-sensitive maintenance proof surface** - `7c31f85` (`test`)

Plan metadata is recorded separately in the final `docs(30-02)` completion commit.

## Files Created/Modified

- `eng/ci.ps1` - Adds the `maintenance` lane, fixed hotspot test filter, and `ScaleSmoke` execution while leaving the release lane intact.

## Decisions Made

- Ignored `-Framework` overrides for `maintenance`, matching the `release` lane posture because the guardrail spans both the `net9.0` editor test project and the `net8.0` `ScaleSmoke` tool.
- Used a checked-in filter string for the hotspot suites so contributors do not have to hand-assemble `dotnet test --filter ...` during refactor work.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- The initial RED run failed exactly as expected because `maintenance` was not in the lane `ValidateSet` yet. That confirmed the lane was genuinely missing before implementation.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- The repo now has one runnable maintenance/refactor gate that later docs can point to directly.
- Phase 30 Plan 03 can now sync `PROJECT.md`, `ROADMAP.md`, `STATE.md`, `README.md`, and `docs/host-integration.md` to the archived `v1.4` history and the new maintenance lane.

## Self-Check: PASSED

- Re-ran `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release`.
- Verified the lane executed a filtered editor test run with 139 passing tests.
- Verified the lane built and ran `tools/AsterGraph.ScaleSmoke`, including `SCALE_*` and `PHASE*_SCALE_*` proof markers.

---
*Phase: 30-milestone-history-and-refactor-gate-closeout*
*Completed: 2026-04-16*
