---
phase: 13-editor-kernel-state-owner-extraction
plan: 02
subsystem: session-factory-kernel-first
completed: 2026-04-04
---

# Phase 13 Plan 02 Summary

Made the canonical runtime composition path kernel-first.

Key changes:

- `AsterGraphEditorFactory.CreateSession(...)` now composes `GraphEditorKernel` directly instead of constructing `GraphEditorViewModel`
- `GraphEditorSession` now depends on the internal `IGraphEditorSessionHost` abstraction rather than a concrete `GraphEditorViewModel`
- `GraphEditorViewModel` now implements that internal host abstraction so the retained façade path can continue using the same session wrapper without changing public API shape

The result is that runtime/session hosts now have a composition path that does not require the VM façade as the runtime state owner.

