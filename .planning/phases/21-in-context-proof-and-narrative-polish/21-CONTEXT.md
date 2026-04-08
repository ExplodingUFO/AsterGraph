# Phase 21: In-Context Proof And Narrative Polish - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.3 roadmap + approved demo showcase direction + Phase 19/20 shipped shell review

<domain>
## Phase Boundary

Phase 21 is the proof-and-narrative polish pass for the new graph-first demo shell.

This phase is about:

- replacing the remaining generic summary copy with short in-context proof cues
- making host-owned seams versus shared editor/runtime state obvious inside the existing menu, intro strip, and right-side drawer
- surfacing the current live showcase configuration and runtime signals as compact rows or cards instead of paragraph-heavy explanation blocks
- aligning demo-facing documentation with the graph-first, host-menu-first showcase story that now exists in the UI

This phase is not yet about:

- adding new menu groups, presets, guided tours, or scene switching
- introducing new runtime/editor capabilities beyond the current demo state and diagnostics projections
- rebuilding the shell layout again after Phase 19/20 already established the menu-plus-drawer structure
- reopening v1.2 package-boundary, kernel-ownership, or plugin-readiness work

</domain>

<decisions>
## Implementation Decisions

### Proof placement

- **D-01:** Keep the existing Phase 19/20 shell structure: one top host menu, one right-side `SplitView` drawer, and one live `GraphEditorView`.
- **D-02:** Proof should move closer to the active graph and active menu group, not back into a large explanatory rail. The first read should show concise ownership cues in the graph intro strip and compact sections in the drawer.
- **D-03:** The `证明` host group should become a live proof surface for seam ownership and session continuity, not a generic list of narrative bullets.

### Copy and configuration summary

- **D-04:** Replace panel-oriented wording such as “打开…面板” and generic headings like “宿主展示面板” with compact action labels and proof-oriented section titles.
- **D-05:** Separate host-controlled configuration from runtime-observed state. View/behavior toggles prove what the host owns; diagnostics/session rows prove what the shared runtime owns.
- **D-06:** Prefer short labels plus live values over paragraphs. One line of helper text is acceptable where needed, but the default surface should be machine-checkable rows or compact cards.
- **D-07:** Keep UI copy Chinese-first while preserving English seam names such as `Editor.Session`, `GraphEditorView`, and `GraphEditorViewChromeMode` where they map directly to the SDK surface.

### Documentation

- **D-08:** `README.md` is the minimum required documentation alignment surface for this phase. It should explain the demo as a graph-first SDK showcase rather than as a generic sample shell.
- **D-09:** Documentation alignment should reuse the same core narrative terms as the UI: host menu, one live session, host-owned seams, and shared runtime signals.

### the agent's Discretion

- The exact badge/section labels used to express host ownership, shared runtime state, and active menu group.
- Whether compact proof cues are best expressed as chips, bordered rows, or small cards, as long as they remain subordinate to the graph.
- The exact split between configuration summary rows and runtime signal rows inside the drawer.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and active requirements
- `.planning/PROJECT.md` - v1.3 milestone framing and validated context
- `.planning/REQUIREMENTS.md` - `PROOF-01` and `PROOF-02`
- `.planning/ROADMAP.md` - Phase 21 goal, dependencies, and success criteria
- `.planning/STATE.md` - current milestone state
- `docs/plans/2026-04-08-demo-showcase-design.md` - approved graph-first / proof-in-context direction

### Shipped shell baseline from Phases 19 and 20
- `.planning/phases/19-graph-first-demo-shell/19-CONTEXT.md` - graph-first shell decisions
- `.planning/phases/19-graph-first-demo-shell/19-UI-SPEC.md` - shell copy and proof-strip baseline
- `.planning/phases/20-host-menu-control-consolidation/20-CONTEXT.md` - menu-group and drawer decisions
- `.planning/phases/20-host-menu-control-consolidation/20-UI-SPEC.md` - compact host-menu control contract
- `.planning/phases/20-host-menu-control-consolidation/20-VERIFICATION.md` - verified control consolidation baseline

### Demo implementation and documentation baseline
- `src/AsterGraph.Demo/Views/MainWindow.axaml` - current graph-first shell, drawer sections, and leftover generic proof copy
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` - current summary properties, runtime projections, and proof strings
- `README.md` - current package and demo-facing narrative
- `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` - structure and label coverage for the demo window
- `tests/AsterGraph.Editor.Tests/GraphEditorDemoShellTests.cs` - session continuity and shell-caption coverage
- `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` - runtime diagnostics projection proof
- `tests/AsterGraph.Editor.Tests/DemoHostMenuControlTests.cs` - host-menu control and runtime summary coverage

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `MainWindowViewModel` already exposes the live booleans and runtime/session projections needed for proof polish; Phase 21 does not need a new runtime model.
- `MainWindow.axaml` already has the right structural homes for proof cues: the graph intro strip, the top host menu, the runtime/proof drawer sections, and the top-right badge cluster.
- Phase 20 already proved the current runtime metrics and diagnostics path are canonical and test-covered.

### Current Problems

- The shell still uses generic action copy such as `打开展示面板` and `打开证明面板`, which sounds like legacy panel navigation rather than a graph-first proof surface.
- The drawer heading `宿主展示面板` and the graph intro title `实时节点图` are accurate but undersell the actual SDK proof story.
- `SelectedHostMenuGroupSummary`, `SelectedHostMenuGroupLines`, `HostSessionContinuityCaption`, and `MainEditorSummary` still carry too much narrative weight in broad prose form.
- The `证明` drawer section is still a generic text list rather than a purpose-built ownership and continuity proof surface.
- `README.md` explains the package boundary well, but it does not yet explain the demo as the new graph-first showcase.

### Integration Points

- `src/AsterGraph.Demo/Views/MainWindow.axaml` is the primary site for replacing generic proof copy with compact in-context surfaces.
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` is the correct place to introduce dedicated proof/configuration properties and keep them synchronized with the live session.
- `README.md` is the smallest documentation surface that can align the public demo story with the shipped UI.
- The existing headless Avalonia tests in `tests/AsterGraph.Editor.Tests/` are sufficient to lock the new proof labels and live-summary behavior.

</code_context>

<specifics>
## Specific Ideas

- Add compact badges near the graph that explicitly label `宿主控制`, `共享运行时`, and the current active host group.
- Split drawer proof content into compact sections such as current configuration, shared runtime signals, and seam-ownership proof instead of one generic text list.
- Keep the `证明` group focused on one live session, host-owned shell changes, and runtime-observed signals rather than repeating broader milestone history.
- Update README with a short demo-showcase section that tells readers what the demo proves and how it maps to the public SDK boundary.

</specifics>

<deferred>
## Deferred Ideas

- Named showcase presets or guided proof tours
- Richer walkthrough animation or staged onboarding flows
- New runtime/plugin/automation features unrelated to proof and narrative clarity

</deferred>

---

*Phase: 21-in-context-proof-and-narrative-polish*
*Context gathered: 2026-04-08*
