# Phase 19: Graph-First Demo Shell - Research

**Completed:** 2026-04-08
**Status:** Ready for planning

## Research Question

What is the most suitable Avalonia shell pattern for a graph-first demo where the first visible controls are a host-level menu and compact on-demand panels, while the live graph remains dominant?

## Current Baseline

The current demo shell in `src/AsterGraph.Demo/Views/MainWindow.axaml` uses a fixed three-column layout:

- left rail for navigation and host toggles
- center rail for the main editor
- right rail for architecture, diagnostics, and proof

This successfully proves many SDK seams, but it makes the graph visually secondary and fragments the host story across too many always-visible cards.

## Official Avalonia Findings

### Menu is the right top-level host menu primitive

Source: [Menu](https://docs.avaloniaui.net/controls/menus/menu), [How to: Work with Menus](https://docs.avaloniaui.net/docs/how-to/menu-how-to)

Relevant findings:

- Avalonia's `Menu` is intended to sit at the top edge of a `DockPanel` in a window.
- First-level `MenuItem` elements create a horizontal menu bar; nested items provide submenus.
- `MenuItem` supports `Command` for actions and `InputGesture` for shortcut display.
- `InputGesture` only displays the shortcut hint; actual keyboard handling still requires matching key bindings.
- Avalonia menu guidance explicitly supports separators, nested menus, icons, and dynamic menu generation from collections.

Planning implication:

- Phase 19 should use a top-level in-window `Menu` as the host-facing first interaction layer.
- Shortcut hints can be shown in menu items, but the real key handling must stay elsewhere if used.

### SplitView is a good fit for a compact on-demand side panel

Source: [SplitView](https://docs.avaloniaui.net/controls/layout/containers/splitview)

Relevant findings:

- `SplitView` has a main content zone that is always visible plus a collapsible pane.
- The pane can be placed on the `Right`, which matches the desired “details drawer” pattern.
- `Overlay` hides the pane until opened and overlays the content area.
- `CompactOverlay` and `CompactInline` leave a narrow closed strip visible.
- Avalonia docs call out the “tool pane” pattern specifically for compact display modes.

Planning implication:

- A right-side `SplitView` pane is a strong candidate for compact secondary showcase controls and summaries.
- `Overlay` or `CompactOverlay` best preserves graph dominance on first read.

### Flyout is useful for small transient content, not the main showcase shell

Source: [Flyout](https://docs.avaloniaui.net/docs/reference/controls/flyouts), [MenuFlyout](https://docs.avaloniaui.net/docs/reference/controls/menu-flyout)

Relevant findings:

- `Flyout` is attached to a host control and is dismissible/transient by design.
- Only some controls support `Flyout` directly; other controls require `AttachedFlyout` and code to show it.
- `MenuFlyout` works well for button-attached menus and dynamic menu content.

Planning implication:

- Flyouts are appropriate for small transient detail popups or overflow actions.
- They are not the best primary container for the shell-wide secondary control surface in this phase.

### CommandBar is a toolbar, not the preferred primary shell frame here

Source: [CommandBar](https://docs.avaloniaui.net/controls/navigation/commandbar)

Relevant findings:

- `CommandBar` is a toolbar-style control with primary commands plus overflow.
- It is optimized for surfacing the most relevant actions directly in the bar.

Planning implication:

- `CommandBar` would bias the shell toward a ribbon/toolbar-first experience.
- That conflicts with the approved direction of “menu first, graph first”, so it is better treated as a rejected option for Phase 19.

## Recommended Technical Shape

1. Replace the demo window root with a `DockPanel` or equivalent top-first composition.
2. Add a top `Menu` as the host-level control plane with groups like `展示`, `视图`, `行为`, `运行时`, and `证明`.
3. Keep one central `GraphEditorView` as the main content surface.
4. Move secondary narrative and host surfaces behind a right-side compact pane, preferably implemented with `SplitView`.
5. Keep flyouts reserved for narrow transient content, not the main control surface.

## Risks

- If too many controls stay permanently visible, Phase 19 will reproduce the existing information-wall problem under a new shell.
- If the pane implementation steals too much width from the graph, `SHOW-02` will fail even if the shell technically has a menu.
- If menu items directly create more runtime owners or editor instances, the milestone will drift away from the single-session showcase requirement.

## Planning Notes

- Phase 19 should establish the graph-first shell and menu scaffold.
- Phase 20 should consolidate the deeper host controls into those menu groups.
- Phase 21 should clean up the compact proof language and final narrative after the new shell is stable.

---

*Research completed for Phase 19 on 2026-04-08*
