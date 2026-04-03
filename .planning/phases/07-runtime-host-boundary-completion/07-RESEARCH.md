# Phase 7: Runtime Host Boundary Completion - Research

## Summary

The current host/runtime story already has the right top-level shape: `AsterGraphEditorFactory.CreateSession(...)` returns `IGraphEditorSession`, and the retained `GraphEditorViewModel.Session` exposes the same runtime root. The problem is not the existence of the session root; it is that the session is still too narrow for a serious custom UI host.

The preferred Phase 7 direction is therefore:

1. Keep `IGraphEditorSession` as the canonical runtime root.
2. Extend `Commands`, `Queries`, and `Events` additively.
3. Use stable IDs and DTOs for new APIs.
4. Keep `GraphEditorViewModel` and its methods as compatibility facades.

## Current Gaps

### Core interactions still stuck on the editor facade

Important custom-host operations still live only on `GraphEditorViewModel`:

- selection mutation
- node move / position mutation
- connection lifecycle
- clipboard / fragment workflows
- some viewport helpers

This means `CreateSession(...)` is not yet sufficient for a host that owns its own UI.

### Runtime query leakage

`CompatiblePortTarget` still exposes `NodeViewModel` and `PortViewModel`, which ties the “runtime-side” compatibility story to MVVM implementation details.

## Recommended API Shape

### Commands

Prefer ID-based commands such as:

- `SetSelection(...)`
- `SetNodePositions(...)`
- `BeginConnection(...)`
- `CompleteConnection(...)`
- `CancelPendingConnection()`
- `DeleteConnection(...)`
- optional targeted helpers like `CenterViewOnNode(...)`

Do not add UI gesture/session contracts. Hosts should own hit-testing and pointer semantics.

### Queries

Add smaller DTO-driven queries for:

- pending connection snapshot
- compatible target discovery using IDs/DTOs
- any additional focused state a custom host needs without touching `GraphEditorViewModel`

### Events

The main event gap is pending connection lifecycle/state. Existing document/selection/viewport/fragment/diagnostics events should stay the base.

## Migration Guidance

- Additive only in Phase 7
- Keep old editor-facade methods
- Shift docs and sample code to prefer session-first usage
- Treat MVVM-typed outputs like `CompatiblePortTarget` as compatibility-only after the new DTO path lands

## Proof Ring

The most important proof files are:

- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs`
- `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInspectionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`
- `tools/AsterGraph.HostSample/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`

## Risks

- Letting `IGraphEditorCommands` turn into a junk drawer of editor-specific helpers
- Accidentally reintroducing MVVM leakage through “convenience” DTOs
- Updating the runtime contract without upgrading HostSample and PackageSmoke, leaving the host story technically improved but still poorly proven
