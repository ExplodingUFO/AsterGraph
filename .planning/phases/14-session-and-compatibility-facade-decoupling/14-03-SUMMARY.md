---
phase: 14-session-and-compatibility-facade-decoupling
plan: 03
subsystem: parity-proof
completed: 2026-04-08
---

# Phase 14 Plan 03 Summary

Closed Phase 14 with explicit parity proof that the retained `GraphEditorViewModel` session path is now adapter-backed while the canonical runtime path remains kernel-first.

Key changes:

- Added `GraphEditorProofRingTests.RetainedCompatibilityProof_UsesAdapterBackedSessionHost` to prove the retained legacy/factory editor session host is no longer `GraphEditorViewModel`.
- Updated `tools/AsterGraph.HostSample/Program.cs` to print a Phase 14 retained-path proof marker:
  - `Retained backend: adapter-backed=True`
  - `Retained path: GraphEditorViewModel.Session compatibility surface over the shared runtime boundary`
- Updated `tools/AsterGraph.PackageSmoke/Program.cs` to print Phase 14 proof markers:
  - `RETAINED_ADAPTER_OK:...`
  - `RUNTIME_READONLY_OK:...`
- Kept the existing `KERNEL_SESSION_OK:True` proof so the session-first canonical path still explicitly proves kernel-first ownership.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal`
  - exit 0
  - `33` tests passed
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
  - exit 0
  - retained proof markers printed, including `adapter-backed=True`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
  - exit 0
  - printed `KERNEL_SESSION_OK:True`
  - printed `RETAINED_ADAPTER_OK:True:True`
  - printed `RUNTIME_READONLY_OK:True`

Phase 14 result after this plan:

- Canonical runtime composition remains `CreateSession(...)` with kernel-first ownership.
- Retained `GraphEditorViewModel.Session` is now adapter-backed over the same shared runtime boundary.
- Snapshot/DTO queries are the canonical host model; MVVM-shaped queries remain compatibility-only shims.

Deferred to later phases:

- Phase 17 still owns the broader migration lock and longer-horizon compatibility proof.
- Phase 18 still owns plugin/automation readiness proof beyond the Phase 14 boundary work.
