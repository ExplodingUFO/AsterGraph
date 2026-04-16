---
phase: 33-kernel-canvas-and-guardrail-follow-through
plan: 03
subsystem: build-guardrails
tags: [cs1591, xml-docs, package-boundary, maintenance-gate]
requires: [33-01-SUMMARY.md, 33-02-SUMMARY.md]
provides:
  - package-boundary XML-doc debt scoping
  - explicit project-level CS1591 debt ownership for AsterGraph.Editor
  - proof that Abstractions, Core, and Avalonia no longer rely on the repo-wide blanket suppression
affects: [build-config, package-boundary, documentation-guardrails]
tech-stack:
  added: []
  patterns: [scoped suppression, package-boundary validation]
key-files:
  created: []
  modified:
    - Directory.Build.props
    - src/AsterGraph.Editor/AsterGraph.Editor.csproj
key-decisions:
  - "Stop suppressing CS1591 for all packable projects at repo scope; only keep the debt where it still exists."
  - "Leave the remaining public XML-doc debt explicit inside AsterGraph.Editor until a dedicated documentation retirement pass lands."
patterns-established:
  - "Publishable packages should prove clean XML-doc boundaries directly or carry an explicit project-level debt marker."
  - "Maintenance guardrails should verify the real debt boundary with builds, not only with comments."
requirements-completed: [FACADE-03, GUARD-02]
duration: working session
completed: 2026-04-16
---

# Phase 33 Plan 03: XML-Doc Guardrail Summary

**Scoped `CS1591` debt down to the project that still owns it, instead of letting every packable package inherit the same blanket suppression.**

## Accomplishments

- Changed `Directory.Build.props` so only non-packable projects inherit the central `CS1591` suppression.
- Added explicit project-level `CS1591` suppression and documentation debt note to `AsterGraph.Editor.csproj`.
- Proved `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Avalonia` build under `warnaserror:CS1591`, which confirms the blanket repo-wide debt boundary is gone for those publishable packages.
- Re-ran the maintenance lane so the hotspot proof ring stayed green after the guardrail change.

## Files Created/Modified

- `Directory.Build.props` - limits central `CS1591` suppression to non-packable projects.
- `src/AsterGraph.Editor/AsterGraph.Editor.csproj` - records the remaining public XML-doc debt explicitly at project scope.

## Issues Encountered

- `AsterGraph.Editor` still carries public runtime and event snapshot documentation debt, so removing the suppression completely would have broken the current build.

## Resolutions

- Moved the debt to the only project that still needs it and validated that the other publishable packages no longer depend on the repo-wide blanket.

## Next Phase Readiness

- Future documentation cleanup can now retire `CS1591` debt project-by-project instead of reopening a repo-global switch.

## Self-Check: PASSED

- `dotnet build src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release /warnaserror:CS1591 -v minimal`
- Result: build succeeded, 0 warnings, 0 errors
- `dotnet build src/AsterGraph.Core/AsterGraph.Core.csproj -c Release /warnaserror:CS1591 -v minimal`
- Result: build succeeded, 0 warnings, 0 errors
- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -v minimal`
- Result: build succeeded; only existing `NU1901` warnings for `NuGet.Packaging` remain
- `dotnet build src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release /warnaserror:CS1591 -v minimal`
- Result: build succeeded; no `CS1591` errors surfaced in `AsterGraph.Avalonia`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release`
- Result: maintenance lane completed successfully with 166 passing focused editor tests plus `ScaleSmoke`

---
*Phase: 33-kernel-canvas-and-guardrail-follow-through*
*Completed: 2026-04-16*
