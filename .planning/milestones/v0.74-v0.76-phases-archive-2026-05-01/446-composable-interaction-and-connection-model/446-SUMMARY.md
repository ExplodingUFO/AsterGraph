# Phase 446: Composable Interaction And Connection Model - Summary

status: complete
bead: avalonia-node-map-mqm.3
completed: 2026-04-30

## Result

Phase 446 passed. It adds a compact interaction foundation around direct connection selection:

- `selection.connections.set` is now a canonical kernel/session command id.
- The command accepts repeated `connectionId` arguments and optional `primaryConnectionId`.
- Rendered connection paths select their connection through the canonical command route.
- Interaction docs now describe connection selection, selected connection snapshot inspection, route-vertex editing commands, and the current shortcut opt-out API.

## Changed Areas

- Runtime command descriptor/catalog contract.
- Kernel command router and router host seam.
- Avalonia connection scene renderer.
- Focused editor tests.
- English and Chinese interaction docs.

## Deferred

- Multi-select wire gestures beyond the existing command API.
- Drag handles directly on the wire path beyond the existing route-vertex authoring controls.
- Any macro, scripting, or query language.
