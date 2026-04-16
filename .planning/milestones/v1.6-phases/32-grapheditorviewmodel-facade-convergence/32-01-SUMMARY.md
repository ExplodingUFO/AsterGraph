---
phase: 32-grapheditorviewmodel-facade-convergence
plan: 01
subsystem: testing
tags: [facade, session-descriptors, maintenance-gate, retained-compatibility]
requires: []
provides:
  - retained session-descriptor guardrails for the view-model overload path
  - maintenance coverage for the retained facade seam bundle
  - explicit regression protection before bootstrap extraction starts
affects: [retained-facade, session-descriptors, maintenance-gate]
tech-stack:
  added: []
  patterns: [focused seam guardrails, narrow maintenance filter]
key-files:
  created: []
  modified:
    - tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs
    - eng/ci.ps1
key-decisions:
  - "Guard the retained view-model overload directly so later extractions cannot silently drop stock descriptor support."
  - "Keep the maintenance lane narrow, but include the dedicated facade seam suite once it becomes part of the moved hotspot surface."
patterns-established:
  - "Retained compatibility seams should be protected by small, behavior-first tests before composition moves."
  - "Maintenance only expands when a moved hotspot seam would otherwise be uncovered."
requirements-completed: [FACADE-01, FACADE-02]
duration: working session
completed: 2026-04-16
---

# Phase 32 Plan 01: Facade Guardrail Expansion Summary

**Added the first Phase 32 guardrails before any extraction work by protecting retained session-descriptor continuity and aligning the maintenance lane with the moved facade seam surface.**

## Accomplishments

- Added a retained view-model overload regression in `GraphEditorSessionTests` that proves stock menu descriptors and descriptor support still survive the compatibility path.
- Kept the assertion target on public retained behavior rather than internal implementation shape.
- Extended the maintenance lane so the dedicated service-seam suite stays in the hotspot proof ring while Phase 32 extraction work moves forward.

## Task Commits

Plan work landed in two atomic commits:

1. `7636616` - `test(32-01): guard retained session descriptor support`
2. `5af00c9` - `test(32-01): extend maintenance facade seam coverage`

## Files Created/Modified

- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` - adds retained descriptor-support coverage for the view-model overload route.
- `eng/ci.ps1` - includes `GraphEditorServiceSeamsTests` in the maintenance hotspot filter.

## Issues Encountered

- The existing facade/session coverage did not explicitly protect the retained overload path that Phase 32 was about to keep refactoring around.

## Resolutions

- Added a focused retained-path regression before moving bootstrap and command collaborators.
- Tightened the refactor gate only where the moved seam would otherwise be unprotected.

## Next Phase Readiness

- The remaining Phase 32 extractions can now move bootstrap and retained command orchestration without relying on only broad end-to-end failures.

## Self-Check: PASSED

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphContextMenuBuilderTests|FullyQualifiedName~EditorClipboardAndFragmentCompatibilityTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal`
- Result: 96 passed, 0 failed
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release`
- Result: maintenance lane completed successfully with 141 passing focused editor tests plus `ScaleSmoke`

---
*Phase: 32-grapheditorviewmodel-facade-convergence*
*Completed: 2026-04-16*
