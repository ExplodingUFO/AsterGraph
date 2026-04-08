# Phase 19: Graph-First Demo Shell - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.3 roadmap + approved demo showcase design + current demo shell review

<domain>
## Phase Boundary

Phase 19 is the shell reset that makes the new demo readable on first launch.

This phase is about:

- replacing the current explanation-heavy three-column shell with a graph-first layout
- making a host-level menu the first visible control surface above the live graph
- keeping secondary showcase surfaces compact and subordinate to the main graph
- preserving one live `Editor` / `Session` instead of switching scenes or rebuilding runtime state

This phase is not yet about:

- fully consolidating every shell, behavior, and runtime toggle into the host menu hierarchy
- reworking the proof narrative in detail across all cards, docs, and runtime summaries
- adding plugin loading, automation APIs, or unrelated graph-editing features

</domain>

<decisions>
## Implementation Decisions

### Shell structure

- **D-01:** The first read of the demo must be a host-level menu plus the live node graph. Large explanation panels may still exist, but they cannot dominate the first screen.
- **D-02:** The shell should move away from the current `280,*,360` permanent three-column composition in `MainWindow.axaml` and toward a top-first layout where the graph owns most of the window.
- **D-03:** There must remain exactly one main `GraphEditorView` in the demo shell. Secondary content may summarize or preview state, but Phase 19 must not introduce multiple equal-weight live editors.

### Host menu

- **D-04:** The top-level host menu should be an in-window Avalonia `Menu`, not an OS-native menu, so the menu is visually part of the demo shell and consistently visible across supported hosts.
- **D-05:** Menu groups should already reflect the milestone direction even if not every control is moved in this phase: `展示`, `视图`, `行为`, `运行时`, `证明`.
- **D-06:** Phase 19 only needs the menu scaffold and first-read shell integration. Phase 20 will own the deeper consolidation of all existing controls into compact host menu groups.

### Secondary surfaces

- **D-07:** Secondary showcase content should move into compact on-demand surfaces rather than permanent left/right information walls. A right-side drawer/pane is preferred over a fixed narrative rail.
- **D-08:** The graph must stay visible while secondary surfaces open. Showing more detail cannot require navigating away from the active graph.

### Visual and copy direction

- **D-09:** Keep the existing dark, technical visual language as the baseline; this is a shell restructure, not a full aesthetic reset.
- **D-10:** UI copy remains Chinese-first, while API/type/seam names can stay in English where that helps hosts map UI proof to real SDK entry points.

### the agent's Discretion

- The exact menu wording and hierarchy beneath the top-level groups.
- Whether the compact secondary surface is best expressed through `SplitView`, a conditional side pane, or another bounded shell container, as long as the graph remains primary.
- The precise collapse thresholds and default open/closed behavior for the secondary surface.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and approved direction
- `.planning/PROJECT.md` - v1.3 milestone framing and active requirements
- `.planning/REQUIREMENTS.md` - `SHOW-01` and `SHOW-02`
- `.planning/ROADMAP.md` - Phase 19 goal, dependencies, and success criteria
- `.planning/STATE.md` - current milestone state
- `docs/plans/2026-04-08-demo-showcase-design.md` - approved graph-first / host-menu-first direction

### Existing demo shell and runtime sources
- `src/AsterGraph.Demo/Views/MainWindow.axaml` - current three-column shell that buries the graph
- `src/AsterGraph.Demo/Views/MainWindow.axaml.cs` - current code-behind baseline
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` - current demo state, toggles, and capability copy
- `src/AsterGraph.Demo/App.axaml` - demo theme baseline
- `src/AsterGraph.Demo/App.axaml.cs` - demo app composition root
- `src/AsterGraph.Demo/AsterGraph.Demo.csproj` - demo package/runtime baseline

### Shared editor and shell surfaces the demo embeds
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` - current full-shell structure and toolbar/chrome composition
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` - full-shell behavior ownership
- `src/AsterGraph.Avalonia/README.md` - current canonical hosted-UI composition story

### Historical demo baseline
- `.planning/phases/06-demo/06-CONTEXT.md` - previous demo phase decisions
- `.planning/phases/06-demo/06-UI-SPEC.md` - previous demo design contract

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `MainWindowViewModel` already exposes the host-facing toggles needed for later phases: read-only, snapping, alignment guides, workspace/fragment commands, host menu extensions, and chrome visibility.
- `GraphEditorView` remains the real integrated full-shell surface and should stay the main artifact of the page.
- The existing theme and dark palette already match the desired technical showcase tone; Phase 19 should reuse that baseline.

### Current Problems

- `MainWindow.axaml` spends the left and right thirds of the window on static cards, while the main graph is only one middle column.
- The current first read is “SDK explanation console”, not “host controls a live graph”.
- The current shell makes host controls feel fragmented because they live in several separate cards before the graph gets visual priority.

### Integration Points

- `MainWindow.axaml` is the primary shell restructure site.
- `MainWindowViewModel.cs` will need host-menu state and compact-surface state that the current three-column layout does not require.
- If regression coverage is added, the most direct harness is a new headless Avalonia test against the demo window rather than editor-runtime tests alone.

</code_context>

<specifics>
## Specific Ideas

- The top menu should feel like a host-level control plane, not the node graph's own context menu.
- The first screen should still expose that this is an SDK showcase, but through compact badges or a short proof strip rather than several large explanatory cards.
- Phase 19 can leave some deeper controls in a compact side surface as long as the graph and host menu become the dominant composition.

</specifics>

<deferred>
## Deferred Ideas

- Full migration of all host toggles and runtime readouts into polished menu/drawer groups
- Final proof/narrative cleanup across cards, docs, and runtime summaries
- Plugin loading, automation, or other post-v1.2 runtime features

</deferred>

---

*Phase: 19-graph-first-demo-shell*
*Context gathered: 2026-04-08*
