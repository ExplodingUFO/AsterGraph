# Phase 25: Plugin And Automation Proof Ring - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.4 roadmap + Phase 23/24 closeout + current proof surfaces

<domain>
## Phase Boundary

Phase 25 starts after Phase 23 proved live plugin integration and inspection on the canonical runtime boundary, and after Phase 24 proved a shipped automation runner rooted in `IGraphEditorSession`.

This phase is about:

- locking the new plugin and automation claims into the runnable proof ring instead of leaving them as editor-only regressions
- extending `HostSample`, `PackageSmoke`, and `ScaleSmoke` so package consumers can verify the same canonical plugin/automation path outside the test suite
- aligning focused regressions and README guidance with the shipped public entry points `AsterGraphEditorFactory.CreateSession(...)`, `AsterGraphEditorFactory.Create(...)`, and `IGraphEditorSession.Automation`
- keeping proof machine-checkable through stable console markers, focused tests, and explicit run commands rather than prose-only claims

This phase is not about:

- adding new plugin-loading architecture, new contribution seams, or new automation capabilities beyond the shipped Phase 22-24 baseline
- introducing a scripting language, workflow-designer UI, persisted macro authoring, or performance-benchmark product surface
- reopening the retained/runtime ownership split or rewriting sample tools around private runtime internals
- fixing pre-existing transaction/history baseline failures unless proof updates directly require touching the same behavior

</domain>

<decisions>
## Implementation Decisions

### Proof posture

- **D-01:** Keep the proof ring rooted in runnable host/package surfaces plus focused regressions. Do not let Phase 25 collapse into a docs-only refresh.
- **D-02:** Prove plugin composition and automation execution from the canonical host boundary: `AsterGraphEditorFactory`, `IGraphEditorSession`, descriptors, queries, diagnostics, and `IGraphEditorSession.Automation`.
- **D-03:** Continue the existing smoke-tool pattern of stable grep-friendly markers. Do not replace it with ad hoc narrative output that is harder to verify in CI or release workflows.
- **D-04:** Keep proof aligned across `HostSample`, `PackageSmoke`, `ScaleSmoke`, focused tests, and README commands. The same public claims should be backed by the same surfaces everywhere.

### Scope control

- **D-05:** Treat the current plugin/automation API surface as shipped input, not as a design topic to reopen during proof work.
- **D-06:** Prefer updating existing proof helpers, descriptor lists, and readiness markers over inventing a separate proof abstraction layer just for this phase.
- **D-07:** Keep `ScaleSmoke` focused on credibility proof for larger sessions, not on turning the tool into a benchmark harness or perf-lab framework.
- **D-08:** Keep the known detached-baseline `GraphEditorTransactionTests` failures documented as carry-forward noise unless Phase 25 work proves they are now directly blocking proof correctness.

### the agent's Discretion

- The exact Phase 25 console marker names as long as they stay stable, grep-friendly, and clearly tied to canonical plugin/automation proof.
- Whether focused regressions land mainly in `GraphEditorProofRingTests.cs` or are split across additional focused test files.
- The exact doc structure used in `README.md` as long as it corrects drift and routes hosts to the canonical entry points and proof commands.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and active requirements
- `.planning/PROJECT.md` - v1.4 milestone framing, shipped baseline, and remaining proof risk
- `.planning/REQUIREMENTS.md` - `PROOF-01`, `PROOF-02`
- `.planning/ROADMAP.md` - Phase 25 goal, dependency, and success criteria
- `.planning/STATE.md` - current carry-forward concerns and proof-ring posture

### Carry-forward extension baseline
- `.planning/phases/22-plugin-composition-contracts/22-VERIFICATION.md` - shipped plugin loading contract baseline
- `.planning/phases/23-runtime-plugin-integration-and-inspection/23-VERIFICATION.md` - shipped live plugin integration and inspection baseline
- `.planning/phases/24-automation-execution-runner/24-VERIFICATION.md` - shipped automation runner and parity baseline

### Canonical proof surfaces
- `tools/AsterGraph.HostSample/Program.cs` - canonical host-boundary proof application
- `tools/AsterGraph.PackageSmoke/Program.cs` - package-consumption proof application
- `tools/AsterGraph.ScaleSmoke/Program.cs` - repeatable large-graph proof application
- `README.md` - public repo narrative and runnable commands
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs` - focused proof-ring regression precedent
- `tests/AsterGraph.Editor.Tests/GraphEditorPluginLoadingTests.cs` - plugin-loading proof precedent
- `tests/AsterGraph.Editor.Tests/GraphEditorAutomationExecutionTests.cs` - automation runner proof precedent

### Canonical runtime entry points
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - shared `Create(...)` / `CreateSession(...)` composition path
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` - canonical runtime root
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` - feature/inspection/query proof surface
- `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnostics.cs` - diagnostics/inspection proof surface
- `src/AsterGraph.Editor/Automation/IGraphEditorAutomationRunner.cs` - canonical automation entry surface

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable assets

- `HostSample` and `PackageSmoke` already use stable console markers for host-boundary proof, so Phase 25 can extend an existing verification pattern instead of inventing a new one.
- `GraphEditorProofRingTests` already carries shared canonical command IDs and readiness-feature IDs that include the Phase 24 automation additions.
- The shipped runtime now exposes plugin-load inspection snapshots, feature descriptors, recent diagnostics, and automation execution snapshots through one canonical session boundary.
- `ScaleSmoke` already owns the large-session proof role and can be extended with automation execution without changing its fundamental purpose.

### Current gaps

- `HostSample` and `PackageSmoke` still lag the shipped plugin/automation baseline in their command/readiness marker sets and visible proof markers.
- `ScaleSmoke` proves large-graph editing/readiness, but not the newly shipped automation runner on a larger session.
- `README.md` still contains pre-v1.4 drift, including outdated non-goal language around runtime plugin loading and missing canonical plugin/automation proof guidance.
- The broader `GraphEditorTransactionTests` baseline issue remains real, but it is not yet evidence of a Phase 25 regression.

### Integration points

- `HostSample` is the right place to prove canonical host composition, plugin inspection, and automation execution from the most explicit consumer boundary.
- `PackageSmoke` is the right place to prove the same story as a package-consumption smoke check with grep-stable output.
- `ScaleSmoke` is the right place to prove that automation remains credible once the graph is larger and session state is denser.
- `README.md` must reference these same tools and commands so the public narrative points to actual proof instead of stale milestone assumptions.

</code_context>

<specifics>
## Specific Ideas

- Add new stable Phase 25 proof markers in `HostSample` and `PackageSmoke` that summarize plugin load snapshots, feature-descriptor readiness, automation execution success, and relevant diagnostics.
- Refresh the shared canonical command and readiness-feature inventories used by runnable proof tools so they match the shipped Phase 22-24 descriptor surface.
- Extend `ScaleSmoke` with a canonical automation run over the existing larger-session setup and record a stable `SCALE_*` marker for that path.
- Update focused proof regressions so they assert the same plugin/automation claims echoed by the runnable proof tools.
- Rewrite the README plugin/automation guidance around the shipped canonical entry points and the exact proof commands hosts can run locally.

</specifics>

<deferred>
## Deferred Ideas

- New plugin contribution types or plugin-owned command registration
- Async automation orchestration, script hosts, or persisted macro authoring
- Benchmark-grade performance instrumentation beyond existing smoke credibility checks
- Repairing unrelated history/transaction baseline failures unless Phase 25 directly proves them relevant

</deferred>

---

*Phase: 25-plugin-and-automation-proof-ring*
*Context gathered: 2026-04-08*
