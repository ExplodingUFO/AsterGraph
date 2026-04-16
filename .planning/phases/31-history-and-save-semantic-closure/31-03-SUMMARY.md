---
phase: 31-history-and-save-semantic-closure
plan: 03
subsystem: proof
tags: [scale-smoke, proof-ring, history-contract, smoke]
requires:
  - phase: 31-01
    provides: explicit retained history/save contract
  - phase: 31-02
    provides: focused retained history/save suites and updated maintenance lane
provides:
  - explicit scale-smoke history contract marker
  - proof-ring coverage for retained history/save parity
  - removal of the carried known-mismatch proof note
affects: [proof-ring, scale-smoke, maintenance-gate]
tech-stack:
  added: []
  patterns: [explicit proof marker, aligned smoke-and-test contract]
key-files:
  created: []
  modified:
    - tools/AsterGraph.ScaleSmoke/Program.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs
key-decisions:
  - "Replace the ambiguous history tuple with an explicit pass/fail smoke marker that still preserves raw detail."
  - "Protect the same history/save contract through proof-ring tests so smoke output and tests tell one story."
patterns-established:
  - "Smoke markers should expose contract pass/fail first and raw state detail second."
  - "Proof-ring tests should exercise the same contract as smoke tools, not a manually interpreted variant."
requirements-completed: [STATE-03]
duration: working session
completed: 2026-04-16
---

# Phase 31 Plan 03: Proof Ring History Contract Alignment Summary

**Aligned `ScaleSmoke` and proof-ring coverage to the same explicit retained history/save contract, replacing the old ambiguous history marker with a machine-checkable pass/fail result plus detail.**

## Accomplishments

- Replaced the old raw history tuple in `ScaleSmoke` with `SCALE_HISTORY_CONTRACT_OK:{pass}:...`.
- Added proof-ring coverage that drives runtime connection commands, retained drag, save, undo, and redo through the same contract.
- Removed the need for a carried known-mismatch note because smoke output and tests now agree on the intended semantics.

## Task Commits

Plan work was committed atomically:

1. `c88fe5d` - `test(31-03): align scale smoke history proof`

## Files Created/Modified

- `tools/AsterGraph.ScaleSmoke/Program.cs` - emits an explicit history contract pass/fail marker and preserves supporting detail.
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs` - adds retained/runtime proof-ring coverage for the same history/save contract.

## Next Phase Readiness

- Proof output is now explicit enough to survive future hotspot refactors without another planning-only note about a known mismatch.
- Phase 32 can now focus on facade narrowing rather than carrying history/save ambiguity forward.

## Self-Check: PASSED

- `dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -c Release --framework net8.0 --nologo`
- Result marker: `SCALE_HISTORY_CONTRACT_OK:True:True:False:True:False:21`
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorHistoryInteractionTests|FullyQualifiedName~GraphEditorSaveBoundaryTests" -v minimal`
- Result: 19 passed, 0 failed

---
*Phase: 31-history-and-save-semantic-closure*
*Completed: 2026-04-16*
