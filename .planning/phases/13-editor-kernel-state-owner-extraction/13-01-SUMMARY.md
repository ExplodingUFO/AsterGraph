---
phase: 13-editor-kernel-state-owner-extraction
plan: 01
subsystem: kernel-state-owner
completed: 2026-04-04
---

# Phase 13 Plan 01 Summary

Introduced `GraphEditorKernel` as the first non-Avalonia runtime state owner inside `AsterGraph.Editor`.

What moved into the kernel:

- canonical `GraphDocument` ownership for the runtime-only path
- selection, viewport, pending-connection, workspace save/load, and history state for the runtime-only path
- runtime-side document/selection/viewport/pending/failure/diagnostic event production

This plan did not yet rewrite the `GraphEditorViewModel` compatibility path into a kernel adapter. It instead established the first kernel-backed runtime foundation that later phases can safely expand.

