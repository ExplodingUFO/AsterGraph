# History, Save, and Dirty Contract

This document publishes the host-facing state contract behind AsterGraph history, save, and dirty behavior.

## Scope

The contract applies to:

- the canonical runtime/session path rooted in `IGraphEditorSession`
- the retained compatibility bridge rooted in `GraphEditorViewModel`
- mixed flows where retained interactions and runtime/session commands touch the same editor state

## Contract

1. Successful save makes the current state clean.
2. Undo that leaves the saved baseline makes the editor dirty.
3. Redo that returns to the saved baseline clears dirty again.
4. No-op interactions must not create fake dirty or undo state.
5. Retained and runtime mutations share one kernel-owned history/save authority.

## Proof

Official proof:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
```

That lane includes:

- `GraphEditorHistoryInteractionTests`
- `GraphEditorSaveBoundaryTests`
- `GraphEditorHistorySemanticTests`

The larger readiness proof also emits `SCALE_HISTORY_CONTRACT_OK` through `tools/AsterGraph.ScaleSmoke`.

## Consumer Guidance

- Treat `SaveWorkspace()` as the operation that establishes the clean baseline.
- Do not assume every interaction creates a durable history step.
- Prefer the canonical session boundary for new integrations and treat the retained path as migration support only.
