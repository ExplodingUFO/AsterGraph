---
phase: 08-stable-host-extension-contracts
plan: 02
subsystem: avalonia-full-shell-optout
completed: 2026-04-03
---

# Phase 08 Plan 02 Summary

Added full-shell opt-out for stock behavior in the Avalonia host path:

- `AsterGraphAvaloniaViewOptions.EnableDefaultContextMenu`
- `AsterGraphAvaloniaViewOptions.EnableDefaultCommandShortcuts`
- `GraphEditorView.EnableDefaultContextMenu`
- `GraphEditorView.EnableDefaultCommandShortcuts`

The full-shell path now forwards these settings into its embedded `NodeCanvas`, and the proof is visible in both `HostSample` and `PackageSmoke`.
