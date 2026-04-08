# Phase 20: Host Menu Control Consolidation - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.3 roadmap + Phase 19 shipped shell baseline + current demo shell/runtime review

<domain>
## Phase Boundary

Phase 20 is the control-consolidation pass for the new graph-first demo shell.

This phase is about:

- turning the top host menu into a real control plane instead of a section launcher only
- grouping shell/chrome, editing behavior, and runtime-facing showcase controls under the same host-level menu structure
- keeping the right-side compact pane as the dense control/readout surface for the currently active menu group
- preserving one live `Editor` / `Editor.Session` while all host controls update the active graph in place

This phase is not yet about:

- rewriting the demo narrative or replacing every remaining proof sentence with polished in-context cues
- adding presets, scene switching, or another showcase navigation model
- introducing new graph-editing features beyond exposing the existing demo/runtime controls more clearly
- reopening kernel/session ownership work that already shipped in v1.2

</domain>

<decisions>
## Implementation Decisions

### Host menu behavior

- **D-01:** Keep the Phase 19 top-level menu groups `展示`, `视图`, `行为`, `运行时`, and `证明`. Phase 20 makes `视图`, `行为`, and `运行时` do real control work.
- **D-02:** The host menu should support direct quick toggles where that keeps the shell compact. Avalonia `MenuItem` checkable state is a good fit for view/chrome and behavior toggles.
- **D-03:** The right-side compact pane remains the dense control surface. The menu is the first entry point; the pane is the place for grouped controls, short explanations, and live runtime readouts.
- **D-04:** Menu structure should stay shallow and operational. Avoid turning the demo into a ribbon, a second toolbar row, or a deeply nested command tree.

### View and chrome controls

- **D-05:** `IsHeaderChromeVisible`, `IsLibraryChromeVisible`, `IsInspectorChromeVisible`, and `IsStatusChromeVisible` should be controlled from the host menu and reflected in the right-side pane.
- **D-06:** View/chrome changes must apply live to the existing `MainGraphEditorView` bindings. They must not recreate `Editor` or replace the current session.
- **D-07:** Phase 20 should not restore permanent left/right narrative rails or any layout that competes with the graph-first shell.

### Editing behavior controls

- **D-08:** `IsReadOnlyEnabled`, `IsGridSnappingEnabled`, `IsAlignmentGuidesEnabled`, `AreWorkspaceCommandsEnabled`, `AreFragmentCommandsEnabled`, and `AreHostMenuExtensionsEnabled` remain the authoritative demo toggles.
- **D-09:** Behavior changes continue to flow through `ApplyHostOptions()` and `BuildCommandPermissions()` in `MainWindowViewModel`. Phase 20 should not add a second behavior-authoring path.
- **D-10:** Behavior toggles should update immediately against the current live graph experience, not by loading a different demo scene.

### Runtime-facing controls and readouts

- **D-11:** The runtime group should expose current runtime state from the existing `Editor.Session.Diagnostics` and inspection snapshot path, but in a purposeful menu/pane section rather than a generic helper card.
- **D-12:** Runtime-facing content should stay compact: current document, node/connection counts, selection, viewport, and recent diagnostics are enough for this phase.
- **D-13:** Final proof storytelling and seam-ownership copy refinements remain a Phase 21 concern.

### Visual and copy direction

- **D-14:** Keep the current dark technical shell language introduced in Phase 19.
- **D-15:** Use short Chinese-first control labels such as `显示顶栏`, `只读模式`, `网格吸附`, `最近诊断`, and `运行时摘要`. Keep API/type names in English where they prove the real SDK seam.

### the agent's Discretion

- The exact split between direct menu quick toggles and drawer-only controls, as long as the menu remains the first control plane.
- The precise names of pane sections and supporting badges.
- Whether runtime readouts are rendered as chips, rows, or compact cards, as long as they remain subordinate to the graph.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and active requirements
- `.planning/PROJECT.md` - v1.3 milestone framing and active demo showcase requirements
- `.planning/REQUIREMENTS.md` - `CTRL-01`, `CTRL-02`, `CTRL-03`
- `.planning/ROADMAP.md` - Phase 20 goal, dependencies, and success criteria
- `.planning/STATE.md` - current milestone state
- `docs/plans/2026-04-08-demo-showcase-design.md` - approved menu-plus-drawer showcase direction

### Shipped shell baseline from Phase 19
- `.planning/phases/19-graph-first-demo-shell/19-CONTEXT.md` - shell-reset decisions
- `.planning/phases/19-graph-first-demo-shell/19-UI-SPEC.md` - graph-first shell contract
- `.planning/phases/19-graph-first-demo-shell/19-01-SUMMARY.md` - top menu shell scaffold summary
- `.planning/phases/19-graph-first-demo-shell/19-02-SUMMARY.md` - compact right-pane summary
- `.planning/phases/19-graph-first-demo-shell/19-03-SUMMARY.md` - shell regression/proof summary

### Demo shell implementation and test baseline
- `src/AsterGraph.Demo/Views/MainWindow.axaml` - current graph-first shell and host menu scaffold
- `src/AsterGraph.Demo/Views/MainWindow.axaml.cs` - current code-behind baseline
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` - existing demo toggles, runtime projections, and shell state
- `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` - current shell structure tests
- `tests/AsterGraph.Editor.Tests/GraphEditorDemoShellTests.cs` - current single-session host shell tests
- `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` - current runtime diagnostics projection proof

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `MainWindowViewModel` already exposes the exact booleans needed for view/chrome and behavior consolidation; most of Phase 20 is presentation and command wiring, not new runtime capability work.
- `ApplyHostOptions()` and `BuildCommandPermissions()` already centralize behavior updates against the single `Editor` instance.
- The Phase 19 `SplitView` shell is already the right structural home for grouped controls and runtime readouts.
- Runtime-facing values are already projected as `RuntimeDocumentTitle`, `RuntimeNodeCount`, `RuntimeConnectionCount`, `RuntimeSelectedNodeCount`, `RuntimeViewportZoom`, `RecentDiagnostics`, and related properties.

### Current Problems

- `PART_ViewMenu`, `PART_BehaviorMenu`, and `PART_RuntimeMenu` currently only open a generic summary pane; they do not expose the existing toggles as real controls.
- The right-side pane currently shows descriptive rows, but not dense, actionable control groups.
- The menu does not yet reflect live checked state for shell or behavior toggles.

### Integration Points

- `src/AsterGraph.Demo/Views/MainWindow.axaml` is the primary consolidation site for menu item structure, toggle bindings, and compact control panels.
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` is the authoritative location for commands, boolean state, and runtime summaries.
- `tests/AsterGraph.Editor.Tests/` already has the correct headless Avalonia harness for locking the new menu-driven controls without introducing a separate UI test project.

</code_context>

<specifics>
## Specific Ideas

- Make `视图` and `行为` menus contain direct checkable quick toggles plus one entry that focuses the right-side pane for the same group.
- Keep `运行时` as a menu-driven entry into compact readouts rather than inventing a fake second runtime dashboard.
- Preserve the existing top badges that prove pane state and single-session continuity, but let the pane content become more action-oriented than descriptive.

</specifics>

<deferred>
## Deferred Ideas

- Final proof-card cleanup, seam-ownership labels, and narrative compression across the whole demo
- Named showcase presets or guided tours
- Any runtime/plugin/automation feature work unrelated to the demo shell story

</deferred>

---

*Phase: 20-host-menu-control-consolidation*
*Context gathered: 2026-04-08*
