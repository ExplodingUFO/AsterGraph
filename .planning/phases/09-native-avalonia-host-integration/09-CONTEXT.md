# Phase 9: Native Avalonia Host Integration - Context

**Gathered:** 2026-04-03
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase improves native desktop host behavior for the shipped Avalonia surfaces.

Phase 9 covers:

- wheel / pan / gesture cooperation with host containers
- keyboard-invoked context menu anchoring and behavior
- keyboard/focus usability of the stock graph interaction path

Phase 9 does not cover:

- menu/presentation contract redesign (Phase 8)
- graph-wide performance/scaling work (Phase 10+)

</domain>

<decisions>
## Implementation Decisions

- **D-01:** Treat native desktop host cooperation as product behavior, not polish.
- **D-02:** Prefer opt-in/opt-out controls and safe defaults over forcing one input policy everywhere.
- **D-03:** Keep behavior changes incremental and host-compatible; do not restyle the UI in this phase.

</decisions>

<canonical_refs>
## Canonical References

- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `.planning/phases/08-stable-host-extension-contracts/08-03-SUMMARY.md`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`
- `src/AsterGraph.Avalonia/Presentation/DefaultGraphNodeVisualPresenter.cs`

</canonical_refs>

---

*Phase: 09-native-avalonia-host-integration*
