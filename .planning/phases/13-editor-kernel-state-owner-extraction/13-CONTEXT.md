# Phase 13: Editor Kernel State Owner Extraction - Context

**Gathered:** 2026-04-04  
**Status:** Ready for planning  
**Source:** v1.2 milestone kickoff + architecture review

<domain>
## Phase Boundary

Phase 13 extracts the editor's canonical mutable state owner out of `GraphEditorViewModel` and makes kernel-first composition real rather than nominal.

This phase is about:

- introducing a non-Avalonia, non-VM kernel state owner for the editor runtime
- moving session-owned command/query/event/diagnostic behavior onto kernel-owned state
- enabling canonical runtime composition without first constructing `GraphEditorViewModel`

This phase is not yet about:

- removing the `GraphEditorViewModel` compatibility façade
- fully normalizing capability/menu/descriptor contracts
- rewriting Avalonia controls against the new kernel path

</domain>

<decisions>
## Implementation Decisions

### Locked

- Phase 13 must satisfy `KERN-01` and `KERN-02`; `KERN-03` remains Phase 14 work.
- The extracted kernel must stay inside the existing .NET/C# package stack and must not take an Avalonia dependency.
- `GraphEditorViewModel` remains supported during the migration window; this phase may wrap or delegate, but not remove or obsolete it.
- Kernel extraction should be incremental: extract state ownership and runtime composition first, then normalize public descriptors/contracts later.
- The canonical runtime composition path after this phase should no longer require `AsterGraphEditorFactory.CreateSession(...)` to construct a `GraphEditorViewModel` under the hood.
- The proof ring added in v1.1 must be preserved; Phase 13 changes need focused regressions instead of relying on local reasoning.

### the agent's Discretion

- Exact naming of the extracted kernel types and namespaces.
- Whether the kernel is a single orchestrator class or a small cluster of state/operation services, as long as state ownership becomes explicit.
- How much of the current history/selection/viewport logic moves in this phase versus being delegated temporarily through adapters.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope
- `.planning/PROJECT.md` — current milestone goal, constraints, and architectural decisions
- `.planning/REQUIREMENTS.md` — `KERN-01`, `KERN-02`, and later-phase dependency boundaries
- `.planning/ROADMAP.md` — phase ordering and what must stay deferred to phases 14-18
- `.planning/STATE.md` — current milestone state and carry-forward constraints

### Current runtime center of gravity
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` — current composition root proving `CreateSession(...)` still routes through `GraphEditorViewModel`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` — current state owner, façade, and integration seam concentration point
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` — current session implementation and VM coupling
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` — current host composition contract

### Current public runtime contracts
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorEvents.cs`
- `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnostics.cs`

### Adapter and proof surfaces that must survive the extraction
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs`
- `tools/AsterGraph.HostSample/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`

</canonical_refs>

<specifics>
## Specific Ideas

- `GraphEditorViewModel` is roughly 2950 lines and currently owns selection, history, persistence, diagnostics relay, context menu construction, runtime session creation, and mutable graph state.
- `GraphEditorSession` is already a public control-plane surface, but it still directly depends on `GraphEditorViewModel` and `NodeViewModel`.
- A likely good Phase 13 split is:
  1. extract kernel state + mutation/query ownership
  2. rewire `GraphEditorSession` and `AsterGraphEditorFactory.CreateSession(...)` onto the kernel
  3. keep `GraphEditorViewModel` delegating to the kernel while preserving current tests and host proof surfaces

</specifics>

<deferred>
## Deferred Ideas

- Full capability descriptor rollout (`CAP-01`)
- Menu/command descriptor normalization (`CAP-02`)
- Public mutable collection removal from façade (`CAP-03`)
- Avalonia adapter thinning (`ADAPT-01`, `ADAPT-02`)
- Final migration lock/proof (`MIG-01`, `MIG-02`)

</deferred>

---

*Phase: 13-editor-kernel-state-owner-extraction*  
*Context gathered: 2026-04-04*

