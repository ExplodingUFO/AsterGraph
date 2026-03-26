# Phase 3: Embeddable Avalonia Surfaces - Context

**Gathered:** 2026-03-26
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase splits the current Avalonia default shell into independently hostable Avalonia surfaces while keeping the full default shell available as a convenience composition. It is about embeddable surface boundaries and host composition, not new graph-editing capabilities, not a non-Avalonia presentation stack, and not the deeper presenter-replacement work reserved for the next phase.

</domain>

<decisions>
## Implementation Decisions

### Surface Granularity
- **D-01:** Phase 3 uses a medium-grain decomposition strategy. Do not split the Avalonia offering all the way down to tiny per-button or per-chrome widgets in this phase.
- **D-02:** The first-class embeddable surfaces for this phase must include: the full default shell, a standalone interactive canvas surface, a standalone inspector surface, and a standalone mini map surface.

### Standalone Canvas
- **D-03:** The standalone canvas surface must be open-box usable. It should ship with core graph interaction behavior intact, including node interaction, connection interaction, selection, marquee selection, and viewport pan/zoom behavior.
- **D-04:** Default context-menu behavior and default keyboard shortcuts stay enabled by default on the standalone canvas surface, but hosts must have explicit options to disable those built-in menu/shortcut behaviors when embedding the canvas into a larger shell.

### Inspector And Mini Map Boundaries
- **D-05:** The standalone inspector surface is a pure inspector. It should cover selection details, node information, connection summaries, and parameter editing only.
- **D-06:** The standalone mini map surface stays narrow and focused. It should remain an overview-and-viewport-navigation control rather than turning into a shell-styled status panel.
- **D-07:** Workspace controls, fragment/template controls, and shortcut-help blocks remain full-shell concerns in Phase 3. They should not be folded into the standalone inspector or standalone mini map surfaces.

### Shell Chrome And Default Menu Surface
- **D-08:** Header, library, and status chrome must become omittable through composition, but they do not need to be promoted into separate first-class public controls in this phase.
- **D-09:** The default Avalonia context-menu presenter should become a public, usable default surface so embeddable canvas hosts can keep the stock menu path without depending on internal implementation details.
- **D-10:** Phase 3 stops at making the default menu surface publicly usable. Full presenter replacement and alternative visual implementations remain Phase 4 work.

### the agent's Discretion
- Exact control names, namespaces, and factory/options shapes for the new standalone surfaces
- Whether the full-shell composition continues to be one `GraphEditorView` over internal shell pieces or becomes a thin composition root over smaller public controls
- Exact option shape for disabling standalone-canvas default menus and shortcuts

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope And Locked Upstream Decisions
- `.planning/ROADMAP.md` — Phase 3 goal, success criteria, and ordering relative to Phase 2 and Phase 4
- `.planning/REQUIREMENTS.md` — `EMBD-01`, `EMBD-02`, `EMBD-03`, `EMBD-04`, `EMBD-05`
- `.planning/PROJECT.md` — active requirement to split the monolithic Avalonia shell while preserving publishable SDK quality and incremental migration
- `.planning/STATE.md` — current session anchor and roadmap progression context
- `.planning/phases/01-consumption-compatibility-guardrails/01-CONTEXT.md` — factory-first public entry story and compatibility-facade preservation
- `.planning/phases/02-runtime-contracts-service-seams/02-CONTEXT.md` — runtime/session state anchor and Phase 2 seam decisions that Phase 3 must build on

### Existing Avalonia Surface Sources
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` — current monolithic full-shell layout, including header, library, canvas frame, inspector column, mini map placement, and status bar
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` — current shell composition logic for style adaptation, clipboard bridge wiring, host-context propagation, chrome visibility, and shell-wide shortcut routing
- `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs` — current shell-level visibility toggle showing the first step toward optional chrome
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` — current standalone-capable canvas control that already owns rendering plus most graph interaction behavior
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs` — current mini map control and its current narrow responsibility boundary
- `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs` — current default Avalonia menu translation layer that must move from internal-only usage toward public usability

### Current Composition Entry Points
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs` — current canonical full-shell creation path
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewOptions.cs` — current full-shell options contract
- `tools/AsterGraph.HostSample/Program.cs` — current reference host that already proves `GraphEditorView`, `ChromeMode`, and Phase 2 runtime/session coexistence

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`: already a concrete reusable surface with graph rendering, node visuals, connection visuals, pointer interaction, selection behavior, viewport transforms, and context-menu invocation
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`: already a concrete reusable control with a narrow and reusable responsibility boundary
- `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`: existing default menu presenter that can be lifted into a public default-surface role
- `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs`: current proof that hosts already need shell omission without rebuilding editor state
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` plus `GraphEditorViewModel.Session`: stable state/control anchor from Phase 2 that standalone surfaces can continue to bind around

### Established Patterns
- Avalonia controls currently bind directly to `GraphEditorViewModel`, not to a second Avalonia-only state layer
- `NodeCanvas` currently owns both rendering and default interaction behavior, so Phase 3 should preserve that open-box value instead of hollowing it out too early
- `GraphEditorView` currently owns shell-wide concerns such as clipboard bridge wiring, host-context propagation, style-resource application, and shell shortcut routing
- The default menu presenter exists as a separate type already, but it is still internal and instantiated inside the canvas path

### Integration Points
- New standalone surfaces should live in `src/AsterGraph.Avalonia/Controls` and continue to compose around Phase 2 editor/runtime state instead of creating parallel state contracts
- The full-shell hosting path in `src/AsterGraph.Avalonia/Hosting/` will need to keep working while exposing additional composition choices for standalone surfaces
- `tools/AsterGraph.HostSample` should become the proof point for composing the full shell and the new standalone surfaces side by side

</code_context>

<specifics>
## Specific Ideas

- The split should favor host composition value over maximum decomposition. The goal is "embed useful surfaces independently" rather than "publish every shell fragment as its own public widget."
- Standalone canvas should still feel like AsterGraph out of the box. Hosts should only have to opt out of default menu/shortcut behavior when they need tighter shell control.
- Standalone inspector should read as a true inspection surface, not as a catch-all side panel.

</specifics>

<deferred>
## Deferred Ideas

- Fine-grained first-class public controls for header, library, and status chrome — reconsider in a later phase if medium-grain composition proves insufficient
- Fully replaceable menu-presentation contracts or alternate menu visual implementations — Phase 4

</deferred>

---

*Phase: 03-embeddable-avalonia-surfaces*
*Context gathered: 2026-03-26*
