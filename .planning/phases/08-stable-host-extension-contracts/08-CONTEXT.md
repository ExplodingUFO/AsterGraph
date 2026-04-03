# Phase 8: Stable Host Extension Contracts - Context

**Gathered:** 2026-04-03
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase stabilizes the host extension seams that still leak concrete MVVM implementation types and closes the full-shell opt-out gap for stock shortcuts/menu behavior.

Phase 8 covers:

- menu augmentation contract shape
- node presentation contract shape
- full-shell opt-out for stock shortcut/menu routing
- migration-safe adapters between compatibility facades and the new host-facing seam types

Phase 8 does **not** cover:

- wheel/pan/focus/menu native desktop interaction policy (Phase 9)
- large-graph rendering and selection performance (Phase 10+)
- broader inspector/parameter-editing redesign beyond what is required for seam stabilization

</domain>

<decisions>
## Implementation Decisions

### Host Extension Seam Direction
- **D-01:** Replace direct `GraphEditorViewModel` / `NodeViewModel` exposure in host extension seams with narrower host/runtime abstractions or DTO contexts.
- **D-02:** Preserve the old MVVM-shaped seams as compatibility paths where needed rather than breaking hosts outright.
- **D-03:** New seam contracts should be data-oriented and purpose-specific, not a grab-bag replacement facade.

### Full-Shell Host Control
- **D-04:** The full-shell Avalonia entry point must gain the same ability to disable or replace stock shortcut/menu routing that standalone canvas hosts already have.
- **D-05:** This phase should expose shell-level toggles and integration points, but should not yet redesign actual desktop input policy. That comes in Phase 9.

### Migration And Compatibility
- **D-06:** `GraphEditorViewModel` remains the compatibility facade, but host docs and samples should shift toward new seam contracts once they exist.
- **D-07:** Prefer additive option contracts and compatibility adapters over hard renames or seam removals.

### the agent's Discretion
- Exact naming of menu/presentation context DTOs or host interfaces
- Whether a compatibility adapter lives in `AsterGraph.Editor` or `AsterGraph.Avalonia`, as long as framework-neutral seams stay out of Avalonia-only packages

</decisions>

<canonical_refs>
## Canonical References

- `.planning/PROJECT.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `.planning/phases/07-runtime-host-boundary-completion/07-CONTEXT.md`
- `.planning/phases/07-runtime-host-boundary-completion/07-03-SUMMARY.md`
- `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs`
- `src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs`
- `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs`
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs`
- `src/AsterGraph.Avalonia/Hosting/AsterGraphCanvasViewOptions.cs`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `tools/AsterGraph.HostSample/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`

</canonical_refs>

<code_context>
## Existing Code Insights

- `IGraphContextMenuAugmentor` currently receives `GraphEditorViewModel`.
- `INodePresentationProvider` currently receives `NodeViewModel`.
- `AsterGraphCanvasViewOptions` already exposes `EnableDefaultContextMenu` and `EnableDefaultCommandShortcuts`.
- `AsterGraphAvaloniaViewOptions` currently lacks equivalent shell-level switches.
- `GraphEditorView` still handles root-level key routing itself in the full-shell path.

</code_context>

<specifics>
## Specific Ideas

- Introduce a host-facing menu augmentation context that exposes only the data/menu/session surface actually needed.
- Introduce a node presentation context/snapshot that avoids requiring direct `NodeViewModel`.
- Mirror standalone canvas opt-out switches on the full-shell options and route them into `GraphEditorView`.
- Keep stock behavior default-on so existing hosts do not change behavior accidentally.

</specifics>

<deferred>
## Deferred Ideas

- Native wheel/menu/focus behavior tuning
- Inspector presenter/runtime seam redesign
- Large-graph performance and virtualization work

</deferred>

---

*Phase: 08-stable-host-extension-contracts*
*Context gathered: 2026-04-03*
