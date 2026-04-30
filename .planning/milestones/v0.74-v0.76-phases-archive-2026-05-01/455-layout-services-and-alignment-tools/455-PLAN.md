# Phase 455 Plan

## Execution Order

1. `455.1` implements supported layout contracts and apply semantics in the editor/runtime layer.
2. `455.2` projects the supported commands into the hosted Avalonia workbench and canvas affordances.
3. `455.3` closes cookbook/docs/API inventory/budget proof and updates milestone state.

## Verification

- Focused `AsterGraph.Editor.Tests` for layout contract, command routing, undo/redo, and snap behavior.
- Focused Avalonia tests for hosted workbench command projection in `455.2`.
- Demo cookbook/docs/proof tests in `455.3`.
- Public API surface validation whenever a public package contract changes.

## Handoff Rule

Each child bead writes its own `455.x-HANDOFF.md` with changed files, validation, and remaining risks. The parent phase closes only after beads, Dolt, Git branch/worktree state, and planning artifacts are clean.
