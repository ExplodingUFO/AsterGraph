# Phase 4: Replaceable Presentation Kit - Context

**Gathered:** 2026-03-26
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase makes the shipped Avalonia presentation layer replaceable while preserving editor-owned behavior, interaction rules, and data contracts. It is about presenter replacement for node visuals, menus, inspector surfaces, and mini-map rendering inside the existing .NET + Avalonia host story. It is not about adding new graph-editing capabilities, not about redoing the full shell into dozens of tiny public widgets, not about a non-Avalonia UI stack, and not about the diagnostics workbench work reserved for Phase 5.

</domain>

<decisions>
## Implementation Decisions

### Replacement Granularity
- **D-01:** Phase 4 keeps the same medium-grain philosophy established in Phase 3. Replacement seams should align to useful presentation units: node visuals, context-menu presentation, inspector presentation, and mini-map presentation. Do not explode the API surface into per-button or per-shell-fragment presenter knobs in this phase.
- **D-02:** Presenter replacement must stay optional. The stock Avalonia presenters remain the default path, and hosts opt into replacement per surface instead of taking an all-or-nothing replacement kit.

### Node Presentation Replacement
- **D-03:** Node visual replacement must preserve `NodeCanvas`-owned interaction behavior. Hosts should be able to replace how nodes are rendered without reimplementing drag, selection, connection creation, marquee selection, viewport interaction, or anchor resolution.
- **D-04:** The node replacement seam should sit above editor state and below host layout. In other words: keep `GraphEditorViewModel` and `NodeCanvas` as the behavior/state owners, and let hosts swap the node visual presenter rather than replacing the canvas interaction controller itself.

### Menu Presentation Replacement
- **D-05:** Context-menu replacement must preserve editor-owned menu intent and command modeling. `GraphEditorViewModel.BuildContextMenu(...)` and `MenuItemDescriptor` stay the source of truth; Phase 4 only makes the Avalonia presentation layer replaceable on top of that intent model.
- **D-06:** The public stock `GraphContextMenuPresenter` added in Phase 3 remains the default implementation and fallback. Phase 4 adds alternative presenter seams; it does not demote the stock presenter back to internal-only use.

### Inspector And Mini Map Replacement
- **D-07:** Inspector replacement should reuse the current editor-provided inspection and parameter-editing data flow rather than inventing a parallel inspector state model. Hosts replace the inspector presentation while continuing to bind to editor-owned selection, connection, and parameter projections.
- **D-08:** Mini-map replacement stays narrow and focused on overview and viewport navigation. Replacement should not turn the mini map seam into a general shell-status area or toolbar host.
- **D-09:** Inspector and mini-map replacement are separate seams. Do not merge them into one monolithic right-panel presenter contract.

### Shell And Package Boundary
- **D-10:** `GraphEditorView` remains the convenience full shell. Hosts that want the stock shell can keep it, while hosts that want custom presentation can swap specific presenters or directly compose standalone surfaces. Do not replace this with a mandatory new shell API in Phase 4.
- **D-11:** Keep the visual replacement kit in `AsterGraph.Avalonia`. Do not push Avalonia-specific presenter abstractions down into `AsterGraph.Editor`, and do not introduce a second non-Avalonia presentation technology in this phase.

### the agent's Discretion
- Exact presenter interface names and namespace placement inside `src/AsterGraph.Avalonia`
- Whether each replacement seam is modeled as control factories, template adapters, render delegates, or another Avalonia-friendly composition shape
- How stock presenter defaults are threaded through `GraphEditorView`, standalone surface factories, and host sample wiring as long as the optional-replacement and no-rewrite constraints above remain true

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope And Locked Upstream Decisions
- `.planning/ROADMAP.md` — Phase 4 goal, requirements, and dependency on completed Phase 3 surfaces
- `.planning/REQUIREMENTS.md` — `PRES-01`, `PRES-02`, `PRES-03`, `PRES-04`
- `.planning/PROJECT.md` — publishable SDK positioning, phased compatibility strategy, and current active requirements
- `.planning/STATE.md` — current milestone position and next-step anchor
- `.planning/phases/03-embeddable-avalonia-surfaces/03-CONTEXT.md` — locked Phase 3 medium-grain surface decisions and explicit deferral of presenter replacement to Phase 4
- `.planning/phases/03-embeddable-avalonia-surfaces/03-03-SUMMARY.md` — full shell now composes over standalone surfaces
- `.planning/phases/03-embeddable-avalonia-surfaces/03-04-SUMMARY.md` — consumer-facing surface matrix and Phase 3 verification ring

### Existing Presentation And Host-Seam Sources
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` — current node rendering choke point plus interaction ownership that must survive presenter replacement
- `src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml` — current standalone inspector surface and its editor-bound data flow
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs` — current standalone mini-map rendering and viewport-navigation behavior
- `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs` — stock public menu presenter that Phase 4 must preserve as the default implementation
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` — full shell composition over standalone surfaces
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` — shell-only composition glue that must stay compatible with optional presenter replacement
- `src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs` — current host-owned display-state seam that should inform, not be replaced by, visual presenter seams
- `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs` — current editor-level menu augmentation seam that must continue to coexist with menu presenter replacement

### Consumer Proof Points
- `tools/AsterGraph.HostSample/Program.cs` — current reference host that proves full shell and standalone-surface composition against one editor state
- `tools/AsterGraph.PackageSmoke/Program.cs` — current machine-checkable surface markers and stock-presenter marker that Phase 4 must preserve while adding replacement coverage

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `NodeCanvas.CreateNodeVisual(...)` and `UpdateNodeVisual(...)` in `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` are the current node-visual choke points. They are the strongest anchor for Phase 4 node presenter extraction because they already separate rendering from most pointer/session orchestration.
- `GraphContextMenuPresenter` in `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs` is already public and can serve as the stock/default menu presenter once a replaceable menu presenter seam is introduced.
- `GraphInspectorView` and `GraphMiniMap` are already first-class standalone surfaces from Phase 3, so Phase 4 can attach replacement seams to these public controls rather than reaching back into the old monolithic shell layout.
- `INodePresentationProvider` and `IGraphContextMenuAugmentor` in `AsterGraph.Editor` are already stable host seams for behavior/data enrichment. Phase 4 should layer on top of them instead of duplicating their responsibilities.

### Established Patterns
- Editor/runtime state lives in `AsterGraph.Editor`; Avalonia controls bind directly to `GraphEditorViewModel`.
- The stock full shell remains a convenience composition root, and hosts can already compose standalone surfaces directly. Presenter replacement must work with both of those adoption paths.
- Default behavior is preserved by default and host customization is opt-in. Phase 4 should continue this pattern instead of forcing hosts into a replacement-first API.

### Integration Points
- New replaceable-presentation contracts should land in `src/AsterGraph.Avalonia` alongside the current controls, menus, and hosting factories.
- `GraphEditorView` and the standalone surface factories will need a coherent way to accept or resolve stock-vs-custom presenters without breaking existing factory/options entry paths.
- Host sample and package smoke will become the proof points that stock and custom presentation paths can coexist without losing Phase 3’s embeddable surface matrix.

</code_context>

<specifics>
## Specific Ideas

- The user already accepted medium-grain componentization in Phase 3, so Phase 4 should keep the presenter story at that same resolution instead of drifting toward micro-widget APIs.
- Replacing a presenter should mean “swap visuals while keeping AsterGraph behavior and data contracts,” not “fork the full control and reimplement interaction.”
- Menu replacement needs to stay above `MenuItemDescriptor` and below host business augmentation; otherwise Phase 4 would accidentally invalidate the editor-level menu seam that already works.

</specifics>

<deferred>
## Deferred Ideas

- Promoting header/library/status chrome into first-class public controls — still outside the current medium-grain scope
- Non-Avalonia presentation adapters — conflicts with current stack boundary and phase scope
- Diagnostics workbench UI and deep inspection tooling — Phase 5

</deferred>

---

*Phase: 04-replaceable-presentation-kit*
*Context gathered: 2026-03-26*
