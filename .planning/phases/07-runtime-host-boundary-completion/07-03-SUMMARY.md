---
phase: 07-runtime-host-boundary-completion
plan: 03
subsystem: host-proof-ring
tags: [migration-parity, service-seams, host-sample, package-smoke, docs]
requires:
  - phase: 07-runtime-host-boundary-completion
    plan: 01
    provides: runtime host boundary contract surface
  - phase: 07-runtime-host-boundary-completion
    plan: 02
    provides: typed event backbone and runtime/inspection parity
provides:
  - migration and service-seam proof over the DTO/runtime-first path
  - HostSample output that visibly demonstrates the runtime-first workflow
  - PackageSmoke markers for session selection, connection, pending event, DTO query, and viewport ownership
  - aligned README / host guide / editor README language for the Phase 7 runtime story
affects: [phase-08, external-host-guidance, package-proof-story]
completed: 2026-04-03
---

# Phase 07 Plan 03 Summary

**Closed the external proof ring for Phase 7 by proving the runtime-first boundary in migration tests, service-seam tests, HostSample, PackageSmoke, and host-facing documentation.**

## Accomplishments

- Updated migration and service-seam tests to use the canonical DTO query path and a fuller session-first workflow.
- Added a focused `%TEMP%` proof harness at:
  - `%TEMP%\astergraph-phase7-proof-validation\AsterGraph.Phase7.Proof.Validation.csproj`
- Expanded `HostSample` so the runtime-first section now demonstrates:
  - DTO-based compatible target lookup
  - selection ownership
  - node positioning
  - pending connection start/cancel visibility
  - connection completion
  - viewport centering
- Expanded `PackageSmoke` with explicit machine-checkable markers:
  - `RUNTIME_SELECTION_OK`
  - `RUNTIME_CONNECTION_OK`
  - `RUNTIME_PENDING_EVENT_OK`
  - `RUNTIME_DTO_QUERY_OK`
  - `RUNTIME_VIEWPORT_OK`
- Aligned `README.md`, `docs/host-integration.md`, and `src/AsterGraph.Editor/README.md` so they all describe the same session-first host story and explicitly mark the MVVM compatibility query path as legacy-only.

## Verification

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal`
- `dotnet test %TEMP%\astergraph-phase7-proof-validation\AsterGraph.Phase7.Proof.Validation.csproj -v minimal`
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
- `rg -n "CreateSession|GraphEditorViewModel.Session|SetSelection|SetNodePositions|GetPendingConnectionSnapshot|GetCompatiblePortTargets|compatibility-only" README.md docs/host-integration.md src/AsterGraph.Editor/README.md`

All passed at completion.

## Outcome

Phase 7 is no longer only an internal contract refactor. The canonical runtime-first path is now visible and reproducible in tests, smoke output, host sample output, and public documentation.
