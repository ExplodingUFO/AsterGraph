---
phase: 16-avalonia-adapter-boundary-cleanup
plan: 03
subsystem: proof-sample-doc-lock
completed: 2026-04-08
---

# Phase 16 Plan 03 Summary

Locked the thinner Avalonia adapter boundary into proof coverage, sample/smoke output, and package/docs guidance so Phase 16 closes with an explicit canonical UI story instead of inferred behavior.

Key changes:

- Added proof coverage in `GraphEditorProofRingTests`, `GraphEditorMigrationCompatibilityTests`, and `GraphEditorInitializationTests` for the post-Phase-16 boundary: full-shell host-context ownership stays on `GraphEditorView`, standalone canvas ownership stays on `NodeCanvas`, and both surfaces route stock context menus through the canonical descriptor presenter overload.
- Updated `tools/AsterGraph.HostSample` with human-readable and machine-checkable Phase 16 markers proving canonical menu routing, shared shortcut routing, and the split platform-seam ownership between full shell and standalone canvas.
- Updated `tools/AsterGraph.PackageSmoke` with machine-checkable `PHASE16_*` markers so package-consumption smoke can prove the same adapter-boundary story outside the test assembly.
- Refreshed `src/AsterGraph.Avalonia/README.md` and `docs/host-integration.md` so the documented canonical-vs-compatibility UI composition story now matches the real post-Phase-16 behavior.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~NodeCanvasStandaloneTests" -v minimal`
  - exit 0
  - `46` tests passed
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
  - exit 0
  - emitted `PHASE16_ADAPTER_BOUNDARY_OK:True:True:True:True:True:True`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
  - exit 0
  - emitted `PHASE16_MENU_ROUTE_OK:True:True:1:1:0:0`
  - emitted `PHASE16_SHORTCUT_ROUTE_OK:True:True`
  - emitted `PHASE16_PLATFORM_BOUNDARY_OK:True:True`

Phase 16 status after this plan:

- `ADAPT-01` is closed: shell and canvas now consume shared Avalonia command/menu routing over the thinner session descriptor boundary rather than duplicating policy independently.
- `ADAPT-02` is closed: clipboard and host-context adaptation remain Avalonia-owned, but the ownership split between full shell and standalone canvas is now explicit, tested, and documented.
- Phase 16 is complete. The next step is Phase 17 planning for compatibility lock and migration proof.
