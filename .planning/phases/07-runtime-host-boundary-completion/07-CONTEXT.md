# Phase 7: Runtime Host Boundary Completion - Context

**Gathered:** 2026-04-03
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase completes the runtime/session host story for serious custom UI hosts. It does not attempt to stabilize menu augmentation, node presentation, inspector editing, or Avalonia shell behavior yet. Those stay in Phase 8 and Phase 9.

The Phase 7 target is narrower and more maintainable:

- custom hosts should be able to drive core graph interaction through `IGraphEditorSession`
- runtime-side queries should stop leaking `NodeViewModel` / `PortViewModel`
- the retained `GraphEditorViewModel` path stays supported as a compatibility facade

</domain>

<decisions>
## Implementation Decisions

### Boundary Split
- **D-01:** Phase 7 handles only core runtime interaction ownership for custom UI hosts.
- **D-02:** Inspector projection, parameter-editing projection, menu-building seams, and broader MVVM seam cleanup are deferred to Phase 8.
- **D-03:** Avalonia host-native input/focus/menu behavior is deferred to Phase 9 unless a Phase 7 runtime change forces a tiny compatibility adjustment.

### Runtime Contract Direction
- **D-04:** Keep `IGraphEditorSession` as the single canonical runtime root; do not create a second parallel host runtime API.
- **D-05:** Extend `IGraphEditorCommands`, `IGraphEditorQueries`, and `IGraphEditorEvents` additively.
- **D-06:** New runtime commands and queries should be ID/DTO-based, not `NodeViewModel` / `PortViewModel` based.
- **D-07:** `GraphEditorViewModel` remains the compatibility facade; new runtime capabilities should route through the existing session/factory path and be available from `GraphEditorViewModel.Session`.

### Missing Capability Focus
- **D-08:** The missing runtime interaction scope includes selection mutation, node position/movement mutation, connection lifecycle, and viewport-centered helpers.
- **D-09:** Phase 7 should not model UI gesture lifecycles. Hosts own input, hit-testing, and rendering policy; the runtime owns state mutation, validation, pending connection state, and notifications.
- **D-10:** Runtime compatibility queries should return stable references/DTOs rather than concrete MVVM objects. `CompatiblePortTarget` in its current shape should become compatibility-only.

### Validation Strategy
- **D-11:** The main proof ring for this phase is not Avalonia interaction tests first; it is runtime/session contract tests, migration parity tests, PackageSmoke, HostSample, and focused diagnostics/service-seam tests.
- **D-12:** Phase 7 must prove that a host can stay on `CreateSession(...)` for the core workflow without dropping down to direct `GraphEditorViewModel` calls.

### the agent's Discretion
- Exact runtime DTO naming, as long as the names stay stable, descriptive, and detached from MVVM implementation types
- Which runtime helpers belong on `Commands` versus `Queries`, as long as mutation and read boundaries remain clear
- Whether some editor methods are wrapped directly or refactored underneath, as long as the public contract stays additive and maintainable

</decisions>

<canonical_refs>
## Canonical References

### Phase Definition
- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`

### Current Runtime Boundary
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorEvents.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs`

### Existing Host Seams And Samples
- `README.md`
- `docs/host-integration.md`
- `tools/AsterGraph.HostSample/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`

### Primary Validation Targets
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs`
- `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInspectionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`

</canonical_refs>

<code_context>
## Existing Code Insights

### Current Runtime Gap
- `IGraphEditorCommands` currently covers undo/redo, clear selection, add/delete, viewport, and workspace save/load.
- Core interaction primitives still live only on `GraphEditorViewModel`, including:
  - selection mutation
  - node movement / position mutation
  - connection lifecycle
  - clipboard and fragment workflows
  - some viewport helpers

### Current Leakage
- `CompatiblePortTarget` still exposes `NodeViewModel` and `PortViewModel`.
- Host-facing docs describe a runtime-first path, but the examples and test proof still fall back to `GraphEditorViewModel` for important workflows.

### Preferred Runtime Shape
- ID-based commands such as selection setting, node position updates, connection begin/complete/cancel, and connection removal
- DTO-based query results for pending connection state and compatible targets
- additive changes only; the compatibility facade remains intact

</code_context>

<specifics>
## Specific Ideas

- Treat `SetNodePositions(...)` as the movement primitive rather than adding drag-session semantics to the runtime.
- Add pending-connection query/event support so custom hosts can render connection previews without dropping down to the editor facade.
- Keep document snapshots authoritative; add smaller runtime DTOs only where hosts currently have to inspect MVVM objects.
- Update HostSample and PackageSmoke so their runtime-first sections prove the new commands/queries instead of using editor-only fallbacks.

</specifics>

<deferred>
## Deferred Ideas

- Menu augmentation contract redesign
- Node presentation contract redesign
- Inspector/parameter-editing runtime boundary
- Full-shell shortcut/menu opt-out and native Avalonia focus/wheel behavior

</deferred>

---

*Phase: 07-runtime-host-boundary-completion*
*Context gathered: 2026-04-03*
