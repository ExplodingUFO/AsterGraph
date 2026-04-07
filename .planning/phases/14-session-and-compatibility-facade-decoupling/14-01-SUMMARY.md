---
phase: 14-session-and-compatibility-facade-decoupling
plan: 01
subsystem: kernel-backed-compatibility-adapter
completed: 2026-04-08
commit: fd65712
---

# Phase 14 Plan 01 Summary

Made the retained `GraphEditorViewModel.Session` path adapter-backed over the extracted kernel instead of binding the runtime session directly to `GraphEditorViewModel` as its `IGraphEditorSessionHost`.

Key changes:

- `GraphEditorViewModel` no longer declares `IGraphEditorSessionHost`.
- Added `src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs` as the retained-path host adapter.
- `GraphEditorViewModel` now composes a private `GraphEditorKernel` and creates `Session` through the adapter-backed host instead of `new GraphEditorSession(this, ...)`.
- `GraphEditorSession(ViewModels.GraphEditorViewModel, ...)` now resolves the editor's internal session host instead of casting the VM itself to `IGraphEditorSessionHost`.
- Direct retained-path editor operations that matter for current runtime parity and inspection coverage now delegate into the kernel path:
  - selection
  - pending-connection lifecycle
  - undo/redo
  - viewport mutation
  - workspace save/load
- The adapter now projects kernel-owned document, selection, viewport, pending-connection, status, and dirty-state snapshots back into the retained VM surface.
- The adapter also forwards retained VM fragment/diagnostic/recoverable-failure events into the session so the compatibility façade keeps diagnostics continuity for direct UI-driven flows.

Tests added:

- `GraphEditorSessionTests.AsterGraphEditorFactory_Create_EditorSession_NoLongerStoresGraphEditorViewModelAsItsRuntimeStateOwner`
- `GraphEditorMigrationCompatibilityTests.LegacyAndFactoryEditorSessions_RemainBehaviorallyAlignedAfterAdapterBackedSessionCommands`

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal`
  - exit 0
  - `28` tests passed
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorDiagnosticsInspectionTests" -v minimal`
  - exit 0
  - `33` tests passed
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
  - exit 0
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
  - exit 0

Important boundary after this plan:

- The retained façade is now session-host adapter-backed, but the public compatibility façade still exposes MVVM-shaped runtime seams such as `CompatiblePortTarget`.
- Phase 14-02 remains responsible for making snapshot/DTO reads the default host-facing model and framing MVVM-shaped queries as compatibility-only.
