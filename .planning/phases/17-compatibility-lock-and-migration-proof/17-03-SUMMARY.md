---
phase: 17-compatibility-lock-and-migration-proof
plan: 03
subsystem: sample-smoke-and-closeout-proof
completed: 2026-04-08
---

# Phase 17 Plan 03 Summary

Locked the migration story into the proof ring, HostSample, PackageSmoke, and host-facing docs so the canonical runtime route, canonical hosted-UI route, and retained compatibility window now tell one coherent story.

Key changes:

- Extended `GraphEditorProofRingTests` with a route-signal proof that compares direct `GraphEditorView` composition, factory-created full-shell views, runtime session descriptors, and retained-only compatibility commands in one place.
- Extended `tools/AsterGraph.HostSample` with human-readable migration-proof output plus the machine-checkable `PHASE17_MIGRATION_ROUTE_OK` marker:
  - `CreateSession(...)` is shown as the canonical runtime route
  - `Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` is shown as the canonical hosted-UI route
  - direct `GraphEditorView` plus `GraphEditorViewModel.Session` is shown as the retained compatibility window
- Extended `tools/AsterGraph.PackageSmoke` with machine-checkable `PHASE17_*` markers:
  - `PHASE17_ROUTE_SIGNAL_OK`
  - `PHASE17_SHARED_CANONICAL_OK`
- Updated `docs/quick-start.md` and `docs/host-integration.md` so they point hosts to the new sample/smoke migration-proof outputs instead of leaving that proof implicit.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~NodeCanvasStandaloneTests" -v minimal`
  - exit 0
  - `37` tests passed
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
  - exit 0
  - emitted `PHASE17_MIGRATION_ROUTE_OK:True:True:True:True:True`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
  - exit 0
  - emitted `PHASE17_ROUTE_SIGNAL_OK:True:True:True:True`
  - emitted `PHASE17_SHARED_CANONICAL_OK:True:True:True:12:16:16`

Phase 17 status after this plan:

- `MIG-01` is complete: the retained `GraphEditorViewModel` / `GraphEditorView` window stays explicitly supported while canonical route signaling is now locked in tests, sample output, smoke output, and docs.
- `MIG-02` is complete: focused regressions, HostSample, and PackageSmoke now all prove the shared canonical subset plus the retained-only compatibility window rather than leaving migration parity to inference.
- Phase 17 is complete. Phase 18 planning is next.
