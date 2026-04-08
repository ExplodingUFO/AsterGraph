# Phase 25 Research: Plugin And Automation Proof Ring

**Date:** 2026-04-08
**Phase:** 25-plugin-and-automation-proof-ring

## Research Questions

1. Which proof surfaces already exist, and where has drift appeared after Phases 22-24 shipped?
2. What is the most credible way to prove plugin composition and automation execution from the canonical host boundary?
3. How should large-graph proof cover automation without overscoping into performance-benchmark work?
4. Which public docs need correction so the repo narrative matches the shipped runtime baseline?

## Findings

### 1. Focused editor/runtime regressions already prove the core contracts, but the runnable proof ring lags behind

The test suite already proves:

- plugin loading and recoverable failure behavior
- plugin integration and inspection parity
- automation execution, typed telemetry, and retained/runtime parity

Implication:

- Phase 25 should not reopen core plugin/automation design
- the main remaining gap is runnable proof at the sample/package/scale/doc surfaces that package consumers actually inspect

### 2. `HostSample` and `PackageSmoke` already use the right proof style: stable console markers over public APIs

Both tools already:

- compose the editor through public package entry points
- emit grep-friendly `PHASE*` markers
- summarize readiness/descriptor/service state in machine-checkable text

Implication:

- Phase 25 should extend this pattern rather than inventing a new proof harness
- new plugin/automation claims should become first-class stable markers in these tools

### 3. The shipped descriptor baseline has moved ahead of the current runnable marker inventories

`GraphEditorProofRingTests` already treats these as shared canonical command IDs or readiness markers:

- `selection.set`
- `nodes.move`
- `viewport.pan`
- `viewport.resize`
- `viewport.center`
- `surface.automation.runner`
- `event.automation.started`
- `event.automation.progress`
- `event.automation.completed`

Phase 23 also added plugin inspection discoverability such as `query.plugin-load-snapshots`.

Implication:

- Phase 25 needs to realign the runnable proof tools with the shipped descriptor/readiness surface
- otherwise the tools will undersell or misreport the actual supported baseline

### 4. `ScaleSmoke` should prove automation credibility by reusing the current large-session setup, not by inventing benchmark infrastructure

`ScaleSmoke` already proves:

- large-graph setup
- bulk selection
- connection lifecycle
- history continuity
- viewport fitting
- inspection continuity

Implication:

- Phase 25 can add one or more automation runs over that same setup and inspect the resulting session/diagnostic state
- this is enough to satisfy `PROOF-02` without widening into benchmark methodology or perf-metric product claims

### 5. README drift is now a real product risk

The current `README.md` still says runtime plugin loading is a non-goal, which is no longer true after Phases 22-24. It also does not yet position the plugin/automation proof ring as the canonical way to verify the new extension story.

Implication:

- docs work is not optional cleanup; it is required to make the public narrative match the shipped SDK surface
- README should explicitly route hosts to `CreateSession(...)`, `Create(...)`, `IGraphEditorSession.Automation`, `HostSample`, `PackageSmoke`, and `ScaleSmoke`

### 6. The detached-baseline transaction failures remain a guardrail, not an execution target

Phase 24 already proved that the two failing `GraphEditorTransactionTests` cases reproduce on a clean pre-Phase-24 baseline.

Implication:

- Phase 25 should acknowledge these failures in planning/verification context
- proof work should avoid turning into a history-semantics detour unless new evidence shows the proof ring itself is wrong

## Risks And Guardrails

- Do not change proof tools to call private runtime internals or retained-only APIs just to make markers easier to emit.
- Do not let README continue contradicting shipped plugin/automation support.
- Do not make `ScaleSmoke` a benchmark project; keep it a repeatable credibility proof tool.
- Do not duplicate proof logic in too many places if a shared marker inventory or helper update is sufficient.
- Do not treat the pre-existing transaction-test failures as Phase 25 regressions unless fresh evidence says otherwise.

## Recommended Planning Posture

### Wave 1: Canonical host-sample proof

Refresh `HostSample` so one explicit host application proves plugin composition, plugin inspection, readiness descriptors, and automation execution from the canonical runtime boundary.

### Wave 2: Package-consumption and scale proof

Refresh `PackageSmoke` and extend `ScaleSmoke` so the same extension claims remain machine-checkable for package-consumption and larger-session scenarios.

### Wave 3: Docs and focused regression closure

Update README guidance and focused proof tests so the public narrative and the test suite both point to the same canonical plugin/automation story and proof commands.

## Recommendation

Plan Phase 25 around one practical principle: every public plugin/automation claim should have a matching proof surface that a host can run or inspect directly.

That keeps the phase disciplined:

- no new product surface is invented just to satisfy proof
- the runnable tools stay aligned with the canonical session-first architecture
- docs become an index to working proof instead of a second, potentially stale story

---

*Research complete: 2026-04-08*
