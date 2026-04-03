# Phase 13: Editor Kernel State Owner Extraction - Research

## Summary

The current architecture already exposes the right public concepts, but the ownership boundary is inverted:

- `AsterGraphEditorFactory.CreateSession(...)` still constructs `GraphEditorViewModel`
- `GraphEditorSession` still depends on `GraphEditorViewModel`
- `GraphEditorViewModel` still owns the canonical mutable graph/editor state

The safest Phase 13 strategy is therefore extraction-before-normalization:

1. introduce an internal kernel state owner that has no Avalonia dependency
2. move session-owned command/query/event/diagnostic behavior onto that kernel
3. adapt `GraphEditorViewModel` onto the kernel rather than trying to redesign public contracts in the same phase

## Why this split

- It satisfies `KERN-01` / `KERN-02` directly without forcing descriptor/capability redesign too early.
- It preserves the v1.1 proof ring while changing only the center of gravity.
- It contains risk: later phases can normalize menu/capability/read-only state exposure after the kernel exists.

## Primary technical risks

- state duplication during the transition if kernel and façade each own mutable copies
- history/dirty/selection drift if restore flows move partially rather than completely
- session regressions if command/event semantics change while ownership is moving
- accidental Avalonia seepage into the kernel through clipboard/host-context/input assumptions

## Recommended planning shape

- `13-01`: introduce kernel state owner and move core mutable graph/editor state ownership
- `13-02`: rewire `GraphEditorSession` and factory/session composition to kernel-first
- `13-03`: lock Phase 13 proof through focused session/transaction/proof-ring updates and host sample/smoke continuity

