---
phase: 02-runtime-contracts-service-seams
plan: 05
subsystem: host-consumers-and-docs
tags: [runtime-session, services, diagnostics, migration, docs, verification]
requires:
  - phase: 02-02
    provides: runtime session contracts and implementation
  - phase: 02-03
    provides: public service seams and diagnostics contracts
  - phase: 02-04
    provides: runtime wiring through factory and compatibility facade
provides:
  - Host sample coverage for runtime session commands, queries, events, diagnostics, compatibility, and retained Avalonia composition
  - Package smoke markers for legacy, factory, session, service override, compatibility service, and diagnostics sink paths
  - Initialization and migration regression coverage for `CreateSession(...)`, `GraphEditorViewModel.Session`, and staged compatibility parity
  - Unified Phase 2 consumer documentation across root README, host integration guide, and `AsterGraph.Editor` README
affects: [phase-02, external-consumption, migration-guidance, verification]
tech-stack:
  added: []
  patterns: [runtime-first consumer story, service override seams, diagnostics publication, temp validation harness]
key-files:
  created:
    - .planning/phases/02-runtime-contracts-service-seams/02-05-SUMMARY.md
  modified:
    - tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
    - tools/AsterGraph.HostSample/Program.cs
    - tools/AsterGraph.PackageSmoke/Program.cs
    - README.md
    - docs/host-integration.md
    - src/AsterGraph.Editor/README.md
requirements-completed: [API-01, API-02, API-03, API-04, SERV-01, SERV-02]
completed: 2026-03-26
---

# Phase 02 Plan 05 Summary

**Closed the Phase 2 external-consumer story around runtime sessions, service seams, diagnostics, and staged migration compatibility.**

## Accomplishments

- Expanded `GraphEditorInitializationTests` so the factory path now verifies `CreateSession(...)`, `GraphEditorViewModel.Session`, package-neutral storage defaults through `StorageRootPath`, runtime command/query/event access, and diagnostics publication through the returned session facade.
- Expanded `GraphEditorMigrationCompatibilityTests` so legacy constructor hosts, factory-created editors, and factory-created runtime sessions now share one verified migration story across compatibility queries, service overrides, clipboard serialization, fragment export, workspace save, and retained `GraphEditorView` composition.
- Reworked `tools/AsterGraph.HostSample` to demonstrate a full Phase 2 runtime-session workflow from consumer code:
  - `AsterGraphEditorFactory.CreateSession(...)`
  - batched `Commands` and `Queries`
  - `CommandExecuted` / `RecoverableFailure`
  - host-owned `CompatibilityService`
  - host-supplied `IGraphWorkspaceService`
  - `IGraphEditorDiagnosticsSink`
  - retained `GraphEditorView` / `ChromeMode` compatibility sample
- Reworked `tools/AsterGraph.PackageSmoke` to preserve the Phase 1 markers while adding explicit Phase 2 markers:
  - `SESSION_FACTORY_OK`
  - `SESSION_EVENTS_OK`
  - `SERVICE_OVERRIDE_OK`
  - `COMPATIBILITY_SERVICE_OK`
  - `DIAGNOSTICS_SINK_OK`
- Updated `README.md`, `docs/host-integration.md`, and `src/AsterGraph.Editor/README.md` so they all tell the same Phase 2 story:
  - runtime-first `CreateSession(...)`
  - factory-plus-Avalonia default UI path
  - retained `GraphEditorViewModel` / `GraphEditorView` compatibility facade
  - replaceable services
  - package-neutral storage defaults
  - diagnostics sink expectations
  - unchanged localization / presentation / context-menu seams

## Verification Evidence

- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj -v minimal` ✅
- `dotnet test %TEMP%\\astergraph-phase2-validation\\AsterGraph.Phase2.Validation.csproj -v minimal` ✅
  - linked test files:
    - `GraphEditorSessionTests.cs`
    - `GraphEditorTransactionTests.cs`
    - `GraphEditorServiceSeamsTests.cs`
    - `GraphEditorInitializationTests.cs`
    - `GraphEditorMigrationCompatibilityTests.cs`
  - result: `24` passed, `0` failed
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` ✅
  - confirmed `workspace.save.failed` reaches both runtime events and diagnostics sink
  - confirmed runtime command markers for `nodes.add`, `viewport.pan`, `workspace.save`
  - confirmed retained `GraphEditorView` + `ChromeMode` path still works
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` ✅
  - preserved `LEGACY_*` and `FACTORY_*` markers
  - emitted `SESSION_*`, `SERVICE_*`, `COMPATIBILITY_SERVICE_OK`, and `DIAGNOSTICS_SINK_OK`
- `rg -n "CreateSession|IGraphEditorSession|StorageRootPath|IGraphWorkspaceService|IGraphEditorDiagnosticsSink|GraphEditorViewModel.Session" README.md docs/host-integration.md src/AsterGraph.Editor/README.md` ✅

## Issues Encountered

- The main `tests/AsterGraph.Editor.Tests` project in this workspace still cannot serve as the plan-level gate because the user-local `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` file remains out of scope and unresolved. Validation therefore continued through a temporary focused test project instead of touching that file.
- An early parallel build attempt hit a transient assembly lock on `AsterGraph.Core.dll`. Final verification was rerun serially and passed cleanly.

## Deviations

- Did not update `.planning/STATE.md` or `.planning/config.json` in this step because both files already contained user-local modifications and the request was to continue integrating on top of existing changes without overwriting them.

## Outcome

Phase 2 is now externally consumable through one coherent host story:

- runtime-only hosts can stop at `IGraphEditorSession`
- Avalonia hosts can keep using `Create(...)` plus `AsterGraphAvaloniaViewFactory`
- migrating hosts can adopt `GraphEditorViewModel.Session` incrementally without rewriting everything at once
