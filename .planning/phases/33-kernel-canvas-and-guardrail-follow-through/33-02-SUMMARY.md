---
phase: 33-kernel-canvas-and-guardrail-follow-through
plan: 02
subsystem: avalonia-shell
tags: [nodecanvas, lifecycle, hotspot-reduction, maintenance-gate]
requires: [33-01-SUMMARY.md]
provides:
  - dedicated lifecycle/property-routing coordination for NodeCanvas
  - focused regression protection for NodeCanvas lifecycle seams
  - maintenance coverage for the moved canvas hotspot surface
affects: [nodecanvas, avalonia-shell, maintenance-gate]
tech-stack:
  added: []
  patterns: [lifecycle coordinator extraction, host-adapter extraction]
key-files:
  created:
    - src/AsterGraph.Avalonia/Controls/Internal/Lifecycle/NodeCanvasLifecycleCoordinator.cs
  modified:
    - src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs
    - src/AsterGraph.Avalonia/Controls/Internal/Hosting/NodeCanvas.HostAdapters.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs
    - eng/ci.ps1
key-decisions:
  - "Move attach/detach and property-routing glue before touching already-extracted pointer and drag math."
  - "Instantiate the lifecycle coordinator before XAML initialization so property routing stays valid during control construction."
patterns-established:
  - "NodeCanvas should keep platform seam, lifecycle, and property-routing glue behind dedicated collaborators instead of expanding the control class."
  - "Constructor-order bugs in extracted coordinators need focused proof, not only broad UI smoke."
requirements-completed: [FACADE-04]
duration: working session
completed: 2026-04-16
---

# Phase 33 Plan 02: NodeCanvas Lifecycle Routing Summary

**Extracted `NodeCanvas` lifecycle and property-routing glue into a dedicated coordinator so the control class keeps shrinking around the remaining platform-facing seams.**

## Accomplishments

- Added `NodeCanvasLifecycleCoordinator` plus a dedicated host adapter for attach/detach and property-change routing.
- Removed inline lifecycle/property-routing branches from `NodeCanvas.axaml.cs` and delegated them to the coordinator.
- Fixed the initialization-order regression by constructing the lifecycle coordinator before `InitializeComponent()`.
- Added a focused reflection guardrail for the lifecycle coordinator and extended the maintenance lane to keep the standalone canvas surface under proof.

## Task Commits

Plan work landed in one atomic commit:

1. `b4572eb` - `refactor(33-02): extract nodecanvas lifecycle routing`

## Files Created/Modified

- `src/AsterGraph.Avalonia/Controls/Internal/Lifecycle/NodeCanvasLifecycleCoordinator.cs` - new lifecycle/property-routing coordinator and host contract.
- `src/AsterGraph.Avalonia/Controls/Internal/Hosting/NodeCanvas.HostAdapters.cs` - adds the lifecycle host adapter.
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` - delegates attach/detach and property-change routing to the coordinator.
- `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs` - guards the extracted lifecycle seam.
- `eng/ci.ps1` - includes `NodeCanvasStandaloneTests` in the maintenance hotspot filter.

## Issues Encountered

- The first extraction attempt exposed a construction-order null reference during XAML initialization because `OnPropertyChanged` could run before the coordinator field existed.

## Resolutions

- Moved coordinator construction ahead of `InitializeComponent()` and reran the focused canvas suites until the extraction proved stable.

## Next Phase Readiness

- `NodeCanvas` lifecycle glue is now separated from pointer/drag/scene coordinators, which keeps later shell hotspot work targeted instead of reopening the whole control class.

## Self-Check: PASSED

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~NodeCanvasStandaloneTests|FullyQualifiedName~NodeCanvasNodeDragCoordinatorTests|FullyQualifiedName~NodeCanvasPointerInteractionCoordinatorTests|FullyQualifiedName~GraphEditorFacadeRefactorTests" -v minimal`
- Result: 81 passed, 0 failed

---
*Phase: 33-kernel-canvas-and-guardrail-follow-through*
*Completed: 2026-04-16*
