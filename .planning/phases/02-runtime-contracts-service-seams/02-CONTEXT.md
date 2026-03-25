# Phase 2: Runtime Contracts & Service Seams - Context

**Gathered:** 2026-03-26
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase defines the framework-neutral runtime API layer inside `AsterGraph.Editor` so hosts can drive editor behavior without depending on Avalonia control internals. It focuses on stable commands, queries, events, lightweight batching/transaction support, and the first wave of replaceable service seams. It does not yet split the Avalonia shell into independently hostable surfaces.

</domain>

<decisions>
## Implementation Decisions

### Compatibility Strategy
- **D-01:** `GraphEditorViewModel` remains the compatibility facade during Phase 2. New runtime contracts must be extracted behind or alongside it rather than replacing it outright.
- **D-02:** Phase 2 should reduce the amount of behavior implemented directly inside `GraphEditorViewModel`; it should move toward being a coordinator over smaller runtime contracts and services.

### Runtime Contract Shape
- **D-03:** The main Phase 2 deliverable is a framework-neutral runtime contract layer centered on `session / state / commands / queries / events`.
- **D-04:** Hosts should be able to execute editor behavior through public runtime contracts in `AsterGraph.Editor` without calling Avalonia control internals.
- **D-05:** Queries and events should become first-class public runtime surfaces, not secondary helpers hanging off UI-facing code.

### Replaceable Service Priorities
- **D-06:** The first service seams to extract and formalize are `workspace`, `fragment`, `template`, `clipboard`, and `diagnostics`, plus seams directly tied to persistence and serialization flows.
- **D-07:** Existing `localization`, `presentation`, and `context menu augmentor` seams are already relatively mature and are not the primary battle for this phase. They can be preserved and integrated, but they are not the first extraction targets.
- **D-08:** Default service implementations must keep working without demo-branded storage assumptions.

### Batching And Transactions
- **D-09:** Phase 2 should introduce a lightweight public batching/transaction concept, such as a mutation scope or transaction API, so hosts can group operations and receive coherent runtime notifications.
- **D-10:** Do not introduce a full command bus, event bus, or large messaging architecture in this phase. Keep the transaction surface pragmatic and narrow.

### the agent's Discretion
- Exact naming and packaging of the new runtime contracts, as long as they live in `AsterGraph.Editor` and stay framework-neutral
- How many contracts are needed in Phase 2 versus which behaviors stay on the compatibility facade temporarily
- Whether the lightweight batching API is disposable-scope-based, callback-based, or explicit begin/end style, as long as it remains simple and host-facing

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Definition And Requirements
- `.planning/ROADMAP.md` — Phase 2 goal, success criteria, and phase ordering constraints
- `.planning/REQUIREMENTS.md` — `API-01`, `API-02`, `API-03`, `API-04`, `SERV-01`, `SERV-02`
- `.planning/PROJECT.md` — project-wide core value, Phase 1 completion state, and phased API-reorganization constraints
- `.planning/STATE.md` — current focus, prior phase outcomes, and carry-forward concerns

### Prior Phase Inputs
- `.planning/phases/01-consumption-compatibility-guardrails/01-CONTEXT.md` — locked decisions about factory-first public entry points and compatibility-facade preservation
- `.planning/phases/01-consumption-compatibility-guardrails/01-RESEARCH.md` — guidance that `GraphEditorViewModel` and `GraphEditorView` stay as compatibility facades while deeper seams move behind them
- `.planning/phases/01-consumption-compatibility-guardrails/01-VERIFICATION.md` — confirms the Phase 1 consumption boundary and migration path that Phase 2 must preserve

### Current Runtime And Seam Sources
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` — current monolithic runtime facade and the main extraction target
- `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs` — current workspace persistence implementation
- `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs` — current fragment workspace implementation
- `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs` — current template/fragment library implementation
- `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs` — current history state implementation
- `src/AsterGraph.Editor/Events/` — current editor event surface that Phase 2 should regularize and extend
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` — new canonical host runtime entry point from Phase 1
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` — current Avalonia-side binding path that Phase 2 should decouple from direct runtime usage

### Supporting Design Notes
- `docs/host-integration.md` — current host-facing story that must stay compatible while runtime seams evolve
- `docs/plans/2026-03-23-astergraph-editor-facade-refactor.md` — prior extraction direction for the editor facade
- `docs/plans/2026-03-23-astergraph-host-extensibility.md` — existing host extension seams and expectations

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `src/AsterGraph.Editor/Events/`: existing document, selection, viewport, fragment, and related event args that can inform the new public runtime event contracts
- `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`, `GraphFragmentWorkspaceService.cs`, `GraphFragmentLibraryService.cs`: current concrete service implementations that can be wrapped behind interfaces
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`: current canonical entry path from Phase 1, which should become the top-level constructor for any new runtime session contract
- `src/AsterGraph.Editor/Configuration/GraphEditorBehaviorOptions.cs` and related option types: existing behavior surface that may feed command/session contract shape

### Established Patterns
- `GraphEditorViewModel` is still the dominant runtime facade and owns too many responsibilities today
- Existing host seams already prefer framework-neutral interfaces in `AsterGraph.Editor`
- Avalonia controls currently talk directly to `GraphEditorViewModel`, so Phase 2 must create runtime seams without breaking that path immediately

### Integration Points
- New runtime contracts should be introduced in `src/AsterGraph.Editor` and then consumed by `AsterGraphEditorFactory` and `GraphEditorViewModel`
- Extracted service seams must continue to support current persistence and clipboard flows used by the host sample and smoke project
- The eventual consumers of the new contracts will be later phases that split Avalonia surfaces and replace presentation layers

</code_context>

<specifics>
## Specific Ideas

- Keep `GraphEditorViewModel` alive as the legacy-friendly shell over the new runtime contract layer rather than turning Phase 2 into a rewrite.
- Make `session / state / commands / queries / events` explicit enough that a host can automate and observe editor behavior without touching Avalonia controls.
- Start service extraction where the codebase is most concrete and fragile today: persistence, fragment/template workflows, clipboard, diagnostics, and related serialization boundaries.
- Keep transaction support small and practical; the point is coherent batching and notifications, not a generalized bus architecture.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 02-runtime-contracts-service-seams*
*Context gathered: 2026-03-26*
