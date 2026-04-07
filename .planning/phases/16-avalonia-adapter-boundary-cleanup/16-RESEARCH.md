# Phase 16: Avalonia Adapter Boundary Cleanup - Research

## Summary

Phase 15 made runtime discovery, command execution, and menu generation descriptor-first, but the Avalonia layer still consumes the retained facade too directly in three places:

- `GraphEditorView.axaml.cs` and `NodeCanvas.axaml.cs` duplicate default keyboard shortcut routing over `GraphEditorViewModel` command properties
- `NodeCanvas.axaml.cs` still opens stock context menus through `GraphEditorViewModel.BuildContextMenu(...)` instead of the canonical session descriptor path
- `GraphEditorView.axaml.cs` still wires clipboard and host-context seams by mutating the retained facade directly rather than flowing through a narrower Avalonia adapter boundary

The safest Phase 16 strategy is therefore adapter-consumption cleanup before broader public-surface redesign:

1. centralize Avalonia default shortcut and stock context-menu routing around shared helpers that consume the canonical session contracts
2. keep clipboard, host-context, and related platform seams in `AsterGraph.Avalonia`, but consolidate how they attach to the editor/facade boundary
3. lock the thinner adapter story into tests, sample output, smoke markers, and package docs before Phase 17 takes over migration-proof ownership

## Why this split

- It satisfies `ADAPT-01` directly by removing duplicated command-routing policy from shell and canvas code.
- It satisfies `ADAPT-02` without overreaching into a full replacement of current presenter or compatibility APIs.
- It leverages the Phase 15 runtime seams that now exist specifically so UI adapters can stop treating `GraphEditorViewModel` command/menu shape as canonical.

## Primary technical risks

- accidental behavior drift if shortcut handling changes command availability, `Handled` semantics, or focus rules between shell and standalone canvas
- compatibility breakage if stock context-menu presentation flips to descriptor-first without preserving existing presenter/augmentor expectations
- scope bleed if public presenter APIs are fully redesigned instead of tightened only where needed to consume the thinner adapter boundary
- hidden regressions in host-context or clipboard continuity if platform seam consolidation changes attach/detach timing

## Recommended planning shape

- `16-01`: consolidate Avalonia default command/menu routing around shared adapters and canonical session descriptors
- `16-02`: narrow platform seam attachment for clipboard, host context, and related control wiring without moving ownership out of Avalonia
- `16-03`: lock the thinner Avalonia adapter boundary into proof tests, sample/smoke markers, and package/docs guidance
