# Phase 11: State And History Scaling - Context

**Gathered:** 2026-04-03
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase targets non-visual recomputation costs that still scale with graph size:

- inspector/status recomputation
- capability/computed-state churn
- dirty tracking and history snapshot costs

This phase does not focus on pointer/render hot paths anymore; those were handled in Phase 10.

</domain>

<decisions>
## Implementation Decisions

- **D-01:** Prioritize reducing repeated whole-graph scans in inspector/status/computed-state paths before deeper undo architecture changes.
- **D-02:** Preserve current behavior semantics; optimize by narrowing recomputation scope, not by weakening the product contract.
- **D-03:** Keep history/dirty tracking incremental where possible, but do not rewrite persistence format or undo model wholesale in this phase.

</decisions>

<canonical_refs>
## Canonical References

- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `.planning/phases/10-canvas-and-interaction-hot-path-scaling/10-03-SUMMARY.md`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `src/AsterGraph.Editor/Services/GraphEditorInspectorProjection.cs`
- `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs`
- `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`

</canonical_refs>

---

*Phase: 11-state-and-history-scaling*
