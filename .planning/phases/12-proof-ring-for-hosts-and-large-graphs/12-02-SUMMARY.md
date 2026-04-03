---
phase: 12-proof-ring-for-hosts-and-large-graphs
plan: 02
subsystem: large-graph-scale-smoke
completed: 2026-04-03
---

# Phase 12 Plan 02 Summary

Added `tools/AsterGraph.ScaleSmoke` as a repeatable large-graph validation harness rooted in `AsterGraph.Editor`.

The tool builds a 180-node / 179-connection document and emits stable `SCALE_*` markers for:

- setup size
- bulk selection
- connection delete/recreate flow
- drag/history/save/undo/redo dirty-state continuity
- viewport fitting
- inspection snapshot continuity
- workspace save continuity

Verification:

- `dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo`

