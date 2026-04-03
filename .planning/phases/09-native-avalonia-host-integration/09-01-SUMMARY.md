---
phase: 09-native-avalonia-host-integration
plan: 01
subsystem: viewport-gesture-policy
completed: 2026-04-03
---

# Phase 09 Plan 01 Summary

Introduced host-cooperative viewport gesture controls:

- `NodeCanvas.EnableDefaultWheelViewportGestures`
- `NodeCanvas.EnableAltLeftDragPanning`
- matching full-shell forwarding through `GraphEditorView`

These switches preserve current defaults while letting hosts opt out of stock wheel and Alt-drag viewport behavior.
