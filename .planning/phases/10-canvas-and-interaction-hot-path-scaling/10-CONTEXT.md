# Phase 10: Canvas And Interaction Hot-Path Scaling - Context

**Gathered:** 2026-04-03
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase targets the first-order interaction hot paths that currently scale with total graph size:

- connection layer rebuilds during drag / preview
- marquee selection graph-wide recomputation
- whole-scene updates on small graph deltas

This phase does not yet cover broader history/dirty/inspector state recomputation. That stays in Phase 11.

</domain>

<decisions>
## Implementation Decisions

- **D-01:** Prioritize removing graph-wide work from the drag/preview and marquee-selection paths before deeper rendering architecture changes.
- **D-02:** Keep changes incremental and compatible with the current `NodeCanvas` / `GraphEditorViewModel` ownership split.
- **D-03:** Measure success through behavior-preserving focused tests and code-path reduction, not speculative micro-optimizations.

</decisions>

<canonical_refs>
## Canonical References

- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `src/AsterGraph.Editor/Services/GraphEditorInspectorProjection.cs`
- `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs`

</canonical_refs>

---

*Phase: 10-canvas-and-interaction-hot-path-scaling*
