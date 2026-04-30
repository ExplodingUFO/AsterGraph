# Phase 446: Composable Interaction And Connection Model - Plan

status: complete
bead: avalonia-node-map-mqm.3

## Success Criteria

1. Connection selection has a canonical host-facing command route.
2. Direct wire clicks in the stock Avalonia connection renderer select the connection through that route.
3. Selection snapshots remain the inspection surface for host state.
4. Shortcut docs point to the current shortcut opt-out API.
5. Focused editor tests pass.

## Implementation Steps

1. Add `selection.connections.set` to the kernel/session command contract.
2. Route repeated `connectionId` arguments through `SetConnectionSelection`.
3. Attach left-click selection to rendered connection paths.
4. Add focused tests for command descriptors, routed command execution, and direct wire click behavior.
5. Repair English and Chinese shortcut docs.
6. Run focused tests and prohibited external-name scan on touched docs/planning files.

## Dependency And Parallelization Notes

This phase was executed in the `phase446-interaction-contracts` worktree and kept separate from:
- Phase 445 rendering/viewport work.
- Phase 447 customization/docs/proof work.

The only shared conceptual surface is connection rendering; the actual write set did not conflict with Phase 445.
