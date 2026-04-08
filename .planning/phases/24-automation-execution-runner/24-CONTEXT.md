# Phase 24: Automation Execution Runner - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.4 roadmap + Phase 18 readiness proof + Phase 23 closeout

<domain>
## Phase Boundary

Phase 24 starts after Phase 23 proved that live plugin composition and canonical plugin inspection now work through the shared `CreateSession(...)` / `Create(...)` runtime boundary.

This phase is about:

- adding the first public automation runner rooted in `IGraphEditorSession`
- executing stable command invocations through canonical command IDs, query snapshots, and mutation batching instead of `GraphEditorViewModel` methods
- publishing typed automation progress/failure/result signals through runtime events and diagnostics suitable for headless or non-Avalonia hosts
- preserving parity between `AsterGraphEditorFactory.CreateSession(...)` and `AsterGraphEditorFactory.Create(...)` by implementing automation on the shared session path

This phase is not yet about:

- introducing a scripting language, workflow-designer UI, or persisted automation-plan authoring format
- adding plugin-defined automation steps, plugin-owned command handlers, or cross-process orchestration
- broadening proof into `HostSample`, `PackageSmoke`, `ScaleSmoke`, or docs refresh; that full proof ring is still Phase 25 scope
- reopening plugin-loading design, view-model ownership, or pre-existing history baseline issues unless automation work directly requires it

</domain>

<decisions>
## Implementation Decisions

### Runner posture

- **D-01:** Keep automation rooted in `IGraphEditorSession` as a first-class runtime surface. Do not make `GraphEditorViewModel`, Avalonia controls, or plugin contracts the canonical automation entry point.
- **D-02:** Model the first automation baseline in terms of stable `GraphEditorCommandInvocationSnapshot` steps plus automation-owned execution metadata. Do not invent a second command language.
- **D-03:** Keep the first baseline synchronous and in-process. The current command/event/diagnostic surface is synchronous, and this phase should not widen into background jobs, cancellation orchestration, or distributed execution.

### Query and observability posture

- **D-04:** Automation must expose machine-readable execution results that correlate command steps with canonical query/inspection state, not only ad hoc status strings.
- **D-05:** Typed automation lifecycle/progress events should be separate from generic `CommandExecuted` because mutation batching intentionally defers generic command notifications until the batch ends.
- **D-06:** Automation availability must be intentionally discoverable through canonical feature descriptors rather than reflection or host knowledge of concrete runtime types.

### Scope control

- **D-07:** Expand generic command-descriptor / command-invocation coverage only where needed to make the first automation baseline credible for session-first workflows.
- **D-08:** Do not turn Phase 24 into a workflow engine, persisted macro library, or script host. The value target is a descriptor-first execution runner over existing session contracts.
- **D-09:** Phase 25 owns the broader proof ring through `HostSample`, `PackageSmoke`, `ScaleSmoke`, and docs. Phase 24 should stay focused on the editor/runtime layer plus focused regressions.

### the agent's Discretion

- The exact public names and shapes of automation DTOs and runner interfaces.
- Whether execution results carry before/after inspection snapshots, per-step snapshots, or a smaller named snapshot set.
- Whether typed automation events should be modeled as separate started/step/completed/failure events or as one richer progress event family.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and active requirements
- `.planning/PROJECT.md` - v1.4 milestone framing and current automation risk
- `.planning/REQUIREMENTS.md` - `AUTO-01`, `AUTO-02`
- `.planning/ROADMAP.md` - Phase 24 goal, dependency, and success criteria
- `.planning/STATE.md` - active project state and carry-forward concerns

### Carry-forward plugin/automation posture
- `.planning/phases/18-plugin-and-automation-readiness-proof-ring/18-CONTEXT.md` - session-first automation readiness decisions
- `.planning/phases/18-plugin-and-automation-readiness-proof-ring/18-RESEARCH.md` - existing command/query/batch proof posture
- `.planning/phases/22-plugin-composition-contracts/22-CONTEXT.md` - v1.4 scope control and deferred automation boundary
- `.planning/phases/23-runtime-plugin-integration-and-inspection/23-CONTEXT.md` - shared factory/session parity posture
- `.planning/phases/23-runtime-plugin-integration-and-inspection/23-VERIFICATION.md` - current verified plugin/session baseline

### Canonical runtime and dispatch code
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - canonical `Create(...)` / `CreateSession(...)` composition flow
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` - canonical runtime root
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs` - stable command surface and generic command invocation entry
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` - canonical snapshot/query surface
- `src/AsterGraph.Editor/Runtime/IGraphEditorEvents.cs` - host-visible runtime events
- `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnostics.cs` - runtime diagnostics/inspection boundary
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` - shared runtime implementation and batching/diagnostics behavior
- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` - canonical command descriptor inventory and generic command dispatch
- `src/AsterGraph.Editor/Runtime/GraphEditorCommandDescriptorSnapshot.cs` - stable command discoverability DTO
- `src/AsterGraph.Editor/Runtime/GraphEditorCommandInvocationSnapshot.cs` - stable generic command invocation DTO
- `src/AsterGraph.Editor/Events/GraphEditorCommandExecutedEventArgs.cs` - existing command telemetry and mutation-label precedent
- `src/AsterGraph.Editor/Events/GraphEditorRecoverableFailureEventArgs.cs` - existing recoverable failure telemetry precedent

### Existing proof surfaces
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` - command descriptor and generic invocation coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs` - mutation batching/event behavior
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInspectionTests.cs` - inspection/diagnostics proof precedent
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - runtime event continuity
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs` - retained/runtime parity precedent
- `tools/AsterGraph.PackageSmoke/Program.cs` - command, batching, and diagnostics session-driving precedent
- `tools/AsterGraph.ScaleSmoke/Program.cs` - mutation-at-scale readiness precedent

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable assets

- `IGraphEditorCommands.TryExecuteCommand(...)` already gives the project one stable generic command-dispatch DTO path.
- `GraphEditorKernel.GetCommandDescriptors()` already exposes stable command IDs and enablement state, so automation can stay descriptor-first instead of hard-coding private methods.
- `IGraphEditorSession.BeginMutation(...)` already provides the correct batch envelope and correlation label for multi-step scripted work.
- `GraphEditorSession` already publishes command, document, selection, viewport, recoverable-failure, and diagnostics signals through one shared runtime boundary.
- `CreateSession(...)` and `Create(...)` now share one runtime composition result, so automation can preserve parity if it is implemented at the session layer.

### Current gaps

- There is no public automation runner surface on `IGraphEditorSession`.
- There are no automation-specific DTOs for run requests, step results, or typed lifecycle/progress reporting.
- Generic command-descriptor / invocation coverage is still narrower than the full session command surface, especially for selection-setting, node movement, and viewport-oriented batch flows.
- There is no discoverability marker that tells hosts a canonical automation runner is available.

### Integration points

- `IGraphEditorSession` and `GraphEditorSession` are the natural home for the automation runner.
- `GraphEditorKernel.GetCommandDescriptors()` and `TryExecuteCommand(...)` are the right choke points for descriptor-first step execution and command-surface widening.
- `IGraphEditorEvents` and `GraphEditorSession.PublishDiagnostic(...)` provide the existing patterns for typed automation telemetry and machine-readable diagnostics.
- `GraphEditorTransactionTests` already prove the expected batching behavior that the automation runner should reuse rather than replace.

</code_context>

<specifics>
## Specific Ideas

- Introduce an `AsterGraph.Editor.Automation` namespace for automation runner interfaces, run-request DTOs, execution results, and event payloads.
- Add a first-class automation entry on `IGraphEditorSession` rather than hiding automation behind host utility code or retained facades.
- Widen stable command descriptors and generic invocation support for automation-critical existing commands such as selection-setting, node movement, and viewport-driving operations.
- Use mutation scopes internally so multi-step automation can batch ordinary runtime notifications while still publishing explicit automation lifecycle events and diagnostics.
- Add explicit discoverability markers such as `surface.automation.runner` and related automation-telemetry feature descriptors so hosts can intentionally detect the new baseline.

</specifics>

<deferred>
## Deferred Ideas

- Persisted macro/automation plan libraries
- Script-host or DSL execution
- Plugin-defined automation steps or plugin-owned command handlers
- Async job orchestration, cancellation tokens, or out-of-process execution
- Full sample/smoke/doc proof ring refresh

</deferred>

---

*Phase: 24-automation-execution-runner*
*Context gathered: 2026-04-08*
