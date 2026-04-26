# Phase 287: Scenario Capability Story And Guided Tour - Context

**Gathered:** 2026-04-26
**Status:** Ready for planning
**Mode:** Autonomous context from roadmap, Phase 286 output, and Demo inspection

<domain>
## Phase Boundary

Phase 287 turns the Phase 286 `ai-pipeline` launch path into an in-demo capability story. It owns the guided flow and the Demo proof surface for scenario capabilities. It does not own ConsumerSample onboarding, five-minute quick start docs, host builder/facade APIs, or new SDK architecture.

</domain>

<decisions>
## Implementation Decisions

- Add a `Tour` host menu group rather than a modal or landing page so the first screen remains the real graph editor.
- Keep the tour host-owned in `AsterGraph.Demo.ViewModels`; do not add runtime API.
- Each tour step should either execute an existing session/plugin/workspace command path or expose a deterministic proof signal.
- Use the existing dark, dense Demo shell style and drawer layout; no new visual theme or large marketing panel.
- Keep the scenario action set small: create node, connect ports, edit parameter, trust plugin, run automation, save/load/export.

</decisions>

<code_context>
## Existing Code Insights

- `DemoGraphFactory` now creates the `ai-pipeline` graph with Input, Prompt, Tool, LLM, Parser, and Output nodes.
- `MainWindowViewModel` is already split into partial files for host behavior, showcase, automation, plugin trust, shell persistence, and localization.
- `MainWindow.axaml` exposes host menu groups through a right-side `SplitView` drawer.
- Automation currently includes selection, plugin, and workspace canned runs; hardcoded terrain node IDs need to remain valid for the AI scenario.
- Existing tests cover Demo menu groups, capability showcase, plugin trust, automation, and scenario launch.

</code_context>

<specifics>
## Specific Ideas

- Add `DemoHostMenuGroups.Tour` and localized menu/drawer strings.
- Add `ScenarioTourStep` plus tour selection/progress/action properties.
- Add a `RunScenarioTourStep` command that executes the focused action for each step.
- Add an action result and signal list so tests can verify the scenario still proves required capabilities.
- Update Demo Guide to document the scenario launch and tour surface.

</specifics>

<deferred>
## Deferred Ideas

- Multi-scenario tour routing.
- Rich overlay callouts pinned to canvas coordinates.
- Screenshot/GIF capture automation.
- ConsumerSample scenario copy path.

</deferred>
