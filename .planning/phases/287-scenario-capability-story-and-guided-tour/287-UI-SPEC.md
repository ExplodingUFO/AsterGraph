# Phase 287 UI Design Contract

## Surface

Add a `Tour` host menu group to the existing Demo shell. The tour appears in the existing right-side drawer and must not replace or obscure the central graph editor.

## Layout

- Reuse the drawer card rhythm already used by Showcase, Runtime, Extensions, and Automation.
- Top of drawer: current tour step, progress text, short summary.
- Controls: Previous, Next, Run Step, and Open Related Panel.
- Below controls: compact list of all tour steps and a signal list showing current proof state.
- Keep card radius at 8px to match existing drawer cards.
- No nested cards.

## Copy

- English labels: `Tour`, `Open guided tour`, `Scenario Tour`, `Run step`, `Open related panel`, `Previous`, `Next`.
- Chinese labels: `导览`, `打开场景导览`, `场景导览`, `运行步骤`, `打开相关面板`, `上一步`, `下一步`.
- Step titles:
  - Create Node
  - Connect Ports
  - Edit Parameters
  - Trust Plugin
  - Run Automation
  - Save And Export

## Interaction

- Selecting Tour from the host menu opens the drawer.
- Selecting a step updates the step detail without changing editor focus.
- `Run step` executes the smallest command path for the selected step.
- `Open related panel` opens the existing host group for the step.
- Previous/Next must clamp to first/last step.
- The center editor remains visible and interactive.

## Visual Style

- Use existing colors: drawer background `#101923`, cards `#14202B`, border `#365063`, primary text `#F4FFFC`, accent `#7FE7D7`.
- Do not add gradients, decorative blobs, hero artwork, or a second palette.
- Text should wrap inside drawer width and avoid horizontal scrolling.

## Accessibility And Testability

- Name XAML elements used by tests with `PART_ScenarioTour...`.
- Buttons must be reachable as real `Button` controls.
- Tour step list must be backed by view-model data so tests can assert required steps.
- Signal lines must include evidence for custom nodes, parameter editing, connection validation, plugin trust, automation, save/load, and export.
