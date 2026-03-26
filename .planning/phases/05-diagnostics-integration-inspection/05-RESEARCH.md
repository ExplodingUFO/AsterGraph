# Phase 5: Diagnostics & Integration Inspection - Research

**Researched:** 2026-03-26
**Domain:** Editor-layer diagnostics contracts, immutable inspection snapshots, and opt-in host-standard instrumentation
**Confidence:** MEDIUM-HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Build on the existing diagnostics baseline instead of replacing it. `GraphEditorDiagnostic`, `IGraphEditorDiagnosticsSink`, and `GraphEditorRecoverableFailureEventArgs` stay valid.
- **D-02:** `StatusMessage` remains a UX and compatibility surface only, not the primary diagnostics contract.
- **D-03:** Diagnostics must stay in `AsterGraph.Editor`; they cannot depend on Avalonia.
- **D-04:** Inspection must expose immutable, medium-grain read models rather than live mutable internals.
- **D-05:** Inspection must at minimum cover document/session identity, selection, viewport, pending connection state, capability/permission state, current status, and recent diagnostics context when useful.
- **D-06:** Existing query snapshots (`CreateDocumentSnapshot`, selection, viewport, capabilities, node positions) should be reused instead of duplicated.
- **D-07:** Logging and tracing must be opt-in, vendor-neutral, and fit host-standard .NET tooling.
- **D-08:** Instrumentation belongs in the editor runtime and host seam boundaries, not in Avalonia controls.
- **D-09:** Diagnostics and inspection must be reachable from both the canonical runtime/session path and the retained compatibility facade.
- **D-10:** This phase is contract and runtime work only. A diagnostics workbench UI is deferred.

### the agent's Discretion
- Exact contract names and namespace placement under `src/AsterGraph.Editor`
- Whether inspection is one aggregate snapshot or a small family of focused records
- Exact wiring shape for `ILogger` and `ActivitySource`

### Deferred Ideas
- A dedicated diagnostics panel or in-app workbench UI
- Pointer-gesture or frame-by-frame tracing
- Any Avalonia-owned diagnostics contract
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| DIAG-01 | Host can receive machine-readable warnings, errors, and operation diagnostics without parsing UI status text | Extend the current sink-backed diagnostic record path beyond recoverable failures and keep publication in `AsterGraph.Editor` |
| DIAG-02 | Host can request inspection snapshots or equivalent debug-state output for troubleshooting integrations | Add a session-root immutable inspection surface that aggregates existing query snapshots plus pending/status context |
| DIAG-03 | Host can attach logging or tracing sinks through public diagnostics contracts | Add opt-in `ILogger` and `ActivitySource` configuration through editor options/factory wiring |
</phase_requirements>

## Summary

Phase 2 already established the most important part of the diagnostics story: machine-readable failure publication exists, is host-replaceable, and already flows through the runtime session. Phase 5 therefore should not invent a parallel diagnostics system. It should deepen the existing one in three controlled steps:

1. add a canonical diagnostics/inspection surface to the runtime session
2. aggregate current editor state into immutable inspection snapshots by reusing existing query records
3. add optional host-standard logging/tracing around meaningful runtime operations only

The safest shape is a **session-root diagnostics surface** in `AsterGraph.Editor` that exposes:

- inspection snapshots
- bounded recent diagnostics history
- optional runtime instrumentation through `ILogger` and `ActivitySource`

That preserves the current migration path because:

- factory-created sessions gain the new surface directly
- retained `GraphEditorViewModel.Session` hosts gain it through the existing compatibility bridge
- `StatusMessage` remains available without being treated as a structured API

## Current State

### What Already Exists

- `GraphEditorDiagnostic` already carries stable `Code`, `Operation`, `Message`, `Severity`, and optional `Exception`
- `IGraphEditorDiagnosticsSink` already lets hosts receive diagnostics without owning UI controls
- `GraphEditorSession.PublishRecoverableFailure(...)` already translates recoverable failures into diagnostics
- `GraphEditorViewModel` already owns almost all state needed for inspection:
  - document snapshot
  - selection
  - viewport
  - capability flags
  - node positions
  - pending connection state
  - `StatusMessage`
- `AsterGraphEditorFactory` and `AsterGraphEditorOptions` are already the canonical composition path for service and diagnostics seams

### What Is Missing

- there is no public session-root diagnostics or inspection interface
- there is no immutable aggregate inspection snapshot
- recent diagnostics are published fire-and-forget only; they are not queryable later
- there is no opt-in `ILogger` or `ActivitySource` wiring in the editor runtime
- important non-failure runtime operations do not yet emit structured operation diagnostics

## Options Considered

### Option A: Session-Root Diagnostics Surface Over Existing Baseline

Add a public diagnostics surface to `IGraphEditorSession`, keep `GraphEditorDiagnostic` and `IGraphEditorDiagnosticsSink`, add immutable inspection snapshots, and optionally wire `ILogger` plus `ActivitySource`.

**Pros**

- aligns with D-01 through D-09
- keeps the canonical host story rooted in the runtime session
- reuses existing query records instead of duplicating editor state models
- lets retained hosts adopt the new surface incrementally through `GraphEditorViewModel.Session`

**Cons**

- requires a deliberate public API expansion on `IGraphEditorSession`
- needs careful boundary design so the inspection snapshot is useful without freezing too much implementation detail

### Option B: Separate Diagnostics Service Outside The Session

Introduce a new host service object returned by the factory and keep `IGraphEditorSession` unchanged.

**Pros**

- minimizes interface changes on the runtime session

**Cons**

- weakens the “session is the canonical runtime root” direction from Phase 2
- creates a second discovery path for state inspection
- makes retained compatibility adoption less obvious

### Option C: Keep The Sink, Skip Inspection, And Add Logs Only

Stop at better diagnostic publication and logger/tracer integration.

**Pros**

- smaller implementation footprint

**Cons**

- fails DIAG-02
- still leaves hosts unable to answer “what state is the editor in right now?”

## Recommended Direction

Choose **Option A**.

Phase 5 should introduce a public diagnostics surface that is reachable from `IGraphEditorSession` and backed by `GraphEditorSession`. That surface should combine:

- `CaptureInspectionSnapshot()` or equivalent aggregate snapshot creation
- `GetRecentDiagnostics(...)` or equivalent bounded history access
- optional logger/tracer publication driven from the same runtime events and operation boundaries that already feed diagnostics

This keeps the current runtime/session architecture intact while making observability a first-class contract instead of a side effect.

## Recommended Public Surface Shape

The exact names are planner discretion, but the safest shape is:

```csharp
public interface IGraphEditorDiagnostics
{
    GraphEditorInspectionSnapshot CaptureInspectionSnapshot();
    IReadOnlyList<GraphEditorDiagnostic> GetRecentDiagnostics(int maxCount = 20);
}

public interface IGraphEditorSession
{
    IGraphEditorCommands Commands { get; }
    IGraphEditorQueries Queries { get; }
    IGraphEditorEvents Events { get; }
    IGraphEditorDiagnostics Diagnostics { get; }
    IGraphEditorMutationScope BeginMutation(string? label = null);
}
```

And one aggregate snapshot record plus a few focused supporting records, for example:

- `GraphEditorInspectionSnapshot`
- `GraphEditorPendingConnectionSnapshot`
- `GraphEditorStatusSnapshot`

The aggregate should compose existing records rather than inline duplicate them:

- `GraphDocument`
- `GraphEditorSelectionSnapshot`
- `GraphEditorViewportSnapshot`
- `GraphEditorCapabilitySnapshot`
- `IReadOnlyList<NodePositionSnapshot>`

## Recommended Instrumentation Shape

Logging and tracing should be expressed as an **opt-in options object** on `AsterGraphEditorOptions`, not as a custom AsterGraph-only logging abstraction.

Recommended direction:

- add `GraphEditorInstrumentationOptions` under `src/AsterGraph.Editor/Diagnostics`
- allow optional `ILoggerFactory`
- allow optional `ActivitySource`
- keep both disabled by default

That gives hosts three adoption levels:

1. no instrumentation, sink only
2. sink plus inspection snapshots
3. sink plus inspection plus logs/traces

## Instrumentation Scope

Instrumentation should be added only around meaningful runtime and integration boundaries, such as:

- workspace save and load
- fragment export and import
- clipboard bridge failures when surfaced through the editor runtime
- host seam failures or fallbacks such as context-menu augmentation exceptions
- diagnostics publication itself when it represents a notable warning or error

Instrumentation should **not** cover:

- every pointer move
- frame rendering
- low-value per-command spam such as every pan tick or marquee drag sample

## Existing Code Signals

### The Runtime Session Is Already The Right Owner

`GraphEditorSession` already batches events, publishes recoverable failures, and exposes the public runtime root shape. That makes it the natural place to:

- maintain bounded diagnostic history
- capture aggregate inspection snapshots
- emit logs and tracing around runtime operations

### ViewModel State Already Covers Inspection Needs

`GraphEditorViewModel` already exposes or computes almost every state input needed by Phase 5:

- `CreateDocumentSnapshot()`
- selection state
- viewport state
- capability flags
- pending connection state
- `StatusMessage`
- node positions

The inspection work should therefore be composition over existing view-model state, not new parallel editor state.

### The Options Path Is Already Established

`AsterGraphEditorOptions` already carries diagnostics and other host seams. Adding instrumentation through that same options object preserves the migration story and keeps host discovery simple.

## Pitfalls

### Pitfall 1: Treating `StatusMessage` As Structured Diagnostics

That would violate D-02 and produce a fragile contract tied to localization and UX wording.

### Pitfall 2: Exposing Live Mutable Objects

Returning live `NodeViewModel`, `ConnectionViewModel`, or service instances from the inspection surface would over-couple hosts to the current implementation.

### Pitfall 3: Making Diagnostics History Unbounded

A forever-growing in-memory diagnostics list would turn a host support feature into a runtime liability. The history should be bounded and queryable.

### Pitfall 4: Instrumenting Avalonia Controls

Doing so would blur the runtime/UI boundary and make the diagnostics story unusable for non-shell or future alternate-surface hosts.

### Pitfall 5: Emitting Too Much Operation Noise

If every small UI action emits diagnostics, hosts will ignore the signal. Phase 5 should focus on support-relevant operations and warnings/errors.

## Validation Architecture

### Focused Harnesses

Because the workspace still contains unresolved local test noise, Phase 5 should keep using temp harnesses instead of the full shared test project as the primary gate.

Recommended harnesses:

- `%TEMP%\\astergraph-phase5-contract-validation\\AsterGraph.Phase5.Contracts.Validation.csproj`
- `%TEMP%\\astergraph-phase5-inspection-validation\\AsterGraph.Phase5.Inspection.Validation.csproj`
- `%TEMP%\\astergraph-phase5-instrumentation-validation\\AsterGraph.Phase5.Instrumentation.Validation.csproj`
- `%TEMP%\\astergraph-phase5-validation\\AsterGraph.Phase5.Validation.csproj`

### Final Proof Ring

- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj -v minimal`
- `dotnet test %TEMP%\\astergraph-phase5-validation\\AsterGraph.Phase5.Validation.csproj -v minimal`
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`

## Planning Guidance

Phase 5 should split cleanly into four plans:

1. contract surface and focused contract harness
2. inspection snapshot plus richer diagnostic publication
3. opt-in logging/tracing integration
4. host proof ring, compatibility regressions, and docs

## Sources

### Primary (HIGH confidence)
- `.planning/ROADMAP.md`
- `.planning/REQUIREMENTS.md`
- `.planning/STATE.md`
- `.planning/phases/05-diagnostics-integration-inspection/05-CONTEXT.md`
- `.planning/phases/02-runtime-contracts-service-seams/02-03-SUMMARY.md`
- `.planning/phases/02-runtime-contracts-service-seams/02-04-SUMMARY.md`
- `.planning/phases/02-runtime-contracts-service-seams/02-05-SUMMARY.md`
- `.planning/phases/04-replaceable-presentation-kit/04-04-SUMMARY.md`
- `src/AsterGraph.Editor/Diagnostics/GraphEditorDiagnostic.cs`
- `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnosticsSink.cs`
- `src/AsterGraph.Editor/Events/GraphEditorRecoverableFailureEventArgs.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs`
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`
- `tools/AsterGraph.HostSample/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`

### Secondary (MEDIUM-HIGH confidence)
- `.planning/research/ARCHITECTURE.md`
- `README.md`
- `docs/host-integration.md`

## Metadata

**Confidence breakdown:**
- session-root diagnostics surface: HIGH
- immutable inspection aggregate: HIGH
- bounded recent-diagnostics history: HIGH
- `ILogger` plus `ActivitySource` integration direction: MEDIUM-HIGH
- validation harness strategy: HIGH

**Research date:** 2026-03-26
**Valid until:** 2026-04-25
