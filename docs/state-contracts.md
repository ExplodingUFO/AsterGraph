# History, Save, and Dirty Contract

This document publishes the host-facing state contract behind AsterGraph history, save, and dirty behavior.

For current alpha scope, known limitations, and stability notes around the public prerelease baseline, see [`alpha-status.md`](./alpha-status.md).

It is not a second implementation guide. It is the behavior contract that the focused proof lane must keep true.

## Scope

This contract applies to:

- the canonical runtime/session path rooted in `IGraphEditorSession`
- the retained compatibility path rooted in `GraphEditorViewModel`
- mixed flows where retained interactions and session commands both touch the same editor state

## Contract

### 1) Save makes the current state clean

After a successful save, the current document snapshot becomes the saved baseline and `IsDirty` is `false`.

### 2) Undo after save makes the editor dirty when it leaves the saved baseline

If the user undoes back to a state that differs from the last saved snapshot, `IsDirty` becomes `true`.

### 3) Redo back to the saved baseline clears dirty again

If redo returns the editor to the last saved snapshot, `IsDirty` becomes `false` again.

### 4) No-op interactions do not create fake dirty or undo state

Interactions that end with no net document change must not leave `IsDirty` latched and must not create a meaningful undo step.

### 5) Retained and runtime mutations share one history/save authority

Mixed retained-host operations and canonical session commands are still expected to resolve through one kernel-owned history/save authority. The product does not support a second retained-only undo/redo owner.

## Proof

The official proof for this contract is:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release`

That lane includes the focused history/save suites:

- `tests/AsterGraph.Editor.Tests/GraphEditorHistoryInteractionTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorSaveBoundaryTests.cs`
- `tests/AsterGraph.Editor.Tests/GraphEditorHistorySemanticTests.cs`

The larger readiness proof also emits:

- `SCALE_HISTORY_CONTRACT_OK`

through:

- `tools/AsterGraph.ScaleSmoke/Program.cs`

## Consumer Guidance

- Treat `SaveWorkspace()` as the operation that establishes the clean baseline.
- Do not assume every interaction creates a durable history step; no-op interactions should collapse away.
- For new host integrations, prefer the canonical session boundary and use the retained path only as a staged migration bridge.
