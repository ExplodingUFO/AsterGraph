# Phase 24 Research: Automation Execution Runner

**Date:** 2026-04-08
**Phase:** 24-automation-execution-runner

## Research Questions

1. Which existing runtime primitives already support a first automation baseline?
2. What is the most concrete gap between current session contracts and `AUTO-01` / `AUTO-02`?
3. How should typed automation telemetry fit with existing mutation batching, events, and diagnostics?
4. What should stay explicitly out of scope so Phase 24 does not turn into a workflow engine?

## Findings

### 1. Most of the automation substrate already exists

The current runtime already exposes:

- stable command IDs through `GetCommandDescriptors()`
- a generic command-dispatch DTO path through `TryExecuteCommand(...)`
- batch execution scope through `BeginMutation(...)`
- state reads through `IGraphEditorQueries`
- diagnostics and inspection through `IGraphEditorDiagnostics`
- runtime event streams through `IGraphEditorEvents`

Implication:

- Phase 24 does not need to invent a second runtime root or a new transaction model
- the first automation baseline can be built as a session-owned orchestration layer over already-shipped command/query/event/diagnostic surfaces

### 2. The first concrete gap is not batching; it is command coverage plus orchestration shape

`GraphEditorKernel.GetCommandDescriptors()` and `TryExecuteCommand(...)` already cover a stable subset of command IDs such as:

- `nodes.add`
- `selection.delete`
- `connections.start`
- `connections.connect`
- `connections.cancel`
- `connections.delete`
- `connections.break-port`
- `viewport.fit`
- `viewport.reset`
- `viewport.center-node`
- `workspace.save`
- `workspace.load`

That is enough to prove descriptor-first dispatch exists, but it is still narrower than the direct runtime command surface.

Implication:

- Phase 24 should widen generic command coverage for the existing session-first commands that make automation credible, especially selection-setting, node movement, and viewport-driving flows
- the public automation runner can then build on that widened canonical command surface instead of embedding private method knowledge

### 3. Mutation scopes already define the right execution envelope

`BeginMutation(...)` already:

- batches normal document/selection/viewport/pending-connection notifications
- preserves a mutation label for correlation
- keeps generic `CommandExecuted` events aligned with batched runtime semantics

Implication:

- the automation runner should reuse mutation scopes rather than inventing a second batch primitive
- the runner can provide optional “run in mutation scope” behavior with a stable label that hosts can correlate across command and automation telemetry

### 4. Typed automation telemetry should not be squeezed into generic command events

Existing `CommandExecuted` and `RecoverableFailure` events are valuable precedents, but they do not express:

- run start / run completion
- step index and total step count
- aggregate run result
- automation-specific failure classification

Also, generic command events are intentionally deferred while a mutation scope is active.

Implication:

- Phase 24 should add automation-specific typed runtime events
- diagnostics should complement those events with machine-readable codes such as run-started, step-failed, and run-completed states
- automation telemetry should stay additive and should not change the semantics of existing generic command batching

### 5. The most stable query story is to return execution results plus canonical snapshots

Hosts already know how to consume:

- document snapshots
- selection snapshots
- viewport snapshots
- inspection snapshots

Implication:

- the first automation result DTO should reuse those snapshot types rather than inventing a second state model
- Phase 24 does not need a query-language or assertion DSL; a run result plus stable snapshots is enough for the first baseline

### 6. Phase 25 should own the broader proof ring

`HostSample`, `PackageSmoke`, `ScaleSmoke`, and docs are explicitly Phase 25 scope in the roadmap.

Implication:

- Phase 24 should stay focused on editor/runtime contracts, execution, telemetry, and focused regressions
- closeout should prove retained/runtime parity, but should not widen into the full sample/smoke/docs refresh yet

## Risks And Guardrails

- Do not turn the first automation baseline into a workflow engine, persisted macro store, or script runtime.
- Do not bypass canonical command IDs by making the automation runner call `GraphEditorViewModel` or private kernel methods directly.
- Do not overload generic `CommandExecuted` as the only automation progress story; mutation batching would make progress visibility ambiguous.
- Do not widen proof into `HostSample`, `PackageSmoke`, `ScaleSmoke`, or docs inside this phase.
- Do not reopen plugin-loading or plugin-defined automation work while planning the first host-owned automation baseline.

## Recommended Planning Posture

### Wave 1: Canonical automation contracts and command-surface widening

Add the public automation namespace, a session-owned runner contract, discoverability markers, and the generic command coverage needed for credible session-driven automation.

### Wave 2: Runner implementation and typed telemetry

Implement synchronous automation execution on top of mutation scopes and generic command invocations, then publish typed lifecycle/progress/failure events plus machine-readable diagnostics.

### Wave 3: Focused parity proof and phase closeout

Lock retained/runtime parity, automation discoverability, and typed telemetry with focused tests, then update planning state to route the next step to Phase 25.

## Recommendation

Plan Phase 24 around one shared principle: the automation runner should be a thin orchestrator over the already-shipped command/query/event/diagnostic control plane, not a new runtime layer.

That keeps the phase credible:

- it delivers real host value over the current public surface
- it stays aligned with the session-first architecture proved in earlier phases
- it preserves room for later proof-ring expansion and richer automation authoring without overscoping the first runner

---

*Research complete: 2026-04-08*
