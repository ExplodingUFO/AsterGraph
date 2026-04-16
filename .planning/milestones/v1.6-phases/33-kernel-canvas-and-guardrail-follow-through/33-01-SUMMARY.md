---
phase: 33-kernel-canvas-and-guardrail-follow-through
plan: 01
subsystem: kernel
tags: [kernel, command-routing, hotspot-reduction, maintenance-gate]
requires: []
provides:
  - dedicated command-router host ownership inside GraphEditorKernel
  - focused regression protection for kernel command-routing seams
  - maintenance coverage for the new kernel hotspot seam
affects: [graph-editor-kernel, command-routing, maintenance-gate]
tech-stack:
  added: []
  patterns: [host-adapter extraction, reflection seam guardrail]
key-files:
  created:
    - src/AsterGraph.Editor/Kernel/Internal/CommandRouting/GraphEditorKernelCommandRouterHost.cs
  modified:
    - src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorKernelCommandRouterTests.cs
    - eng/ci.ps1
key-decisions:
  - "Move command-router host ownership behind a dedicated helper instead of leaving GraphEditorKernel itself as the router host interface implementation."
  - "Protect the new seam through focused reflection and routing tests before further kernel extraction continues."
patterns-established:
  - "Kernel hotspots should move behind narrow host adapters before larger routing or mutation changes land."
  - "Maintenance coverage expands only when a new hotspot seam becomes part of the supported refactor loop."
requirements-completed: [FACADE-04]
duration: working session
completed: 2026-04-16
---

# Phase 33 Plan 01: Kernel Command Router Host Summary

**Pulled command-router host ownership out of `GraphEditorKernel` and into a dedicated internal collaborator so the kernel no longer carries one more cross-cutting host role inline.**

## Accomplishments

- Added `GraphEditorKernelCommandRouterHost` as the dedicated adapter that satisfies the router host contract.
- Simplified `GraphEditorKernel` so it now composes the router through the helper instead of implementing the host interface directly.
- Extended focused kernel routing tests to assert the helper seam exists and remains separated from the kernel type.
- Added the kernel routing suite to the maintenance lane so future hotspot refactors keep hitting the same proof path.

## Task Commits

Plan work landed in one atomic commit:

1. `1e027d5` - `refactor(33-01): extract kernel command router host`

## Files Created/Modified

- `src/AsterGraph.Editor/Kernel/Internal/CommandRouting/GraphEditorKernelCommandRouterHost.cs` - new dedicated router-host adapter.
- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` - composes the router through the adapter instead of implementing the host interface directly.
- `tests/AsterGraph.Editor.Tests/GraphEditorKernelCommandRouterTests.cs` - guards the extracted seam and current routing behavior.
- `eng/ci.ps1` - includes the kernel command-router suite in the maintenance hotspot filter.

## Issues Encountered

- Command routing was the clearest remaining kernel-owned cross-cutting host role that still lived inline on the main hotspot type.

## Resolutions

- Moved the host responsibilities behind a dedicated adapter and proved the seam with focused tests instead of relying on broad kernel regressions alone.

## Next Phase Readiness

- The kernel can keep shrinking collaborator-by-collaborator without reopening the public embedding surface or reintroducing direct host-role sprawl.

## Self-Check: PASSED

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorKernelCommandRouterTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal`
- Result: 31 passed, 0 failed

---
*Phase: 33-kernel-canvas-and-guardrail-follow-through*
*Completed: 2026-04-16*
